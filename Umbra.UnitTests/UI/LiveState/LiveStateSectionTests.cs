namespace Umbra.UI.LiveState.UnitTests;


/// <summary>
/// Unit tests for the <see cref="LiveStateSection{T}"/> class.
/// </summary>
[TestClass]
public sealed class LiveStateSectionTests
{
    /// <summary>
    /// Tests that SectionLabel returns the value provided to the primary constructor
    /// when a non-null tree node label is supplied.
    /// </summary>
    /// <param name="sectionLabel">The tree node label to test.</param>
    [TestMethod]
    [DataRow("Test Label")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("Very Long Label With Multiple Words And Special Characters !@#$%^&*()")]
    [DataRow("Label\nWith\nNewlines")]
    [DataRow("Label\tWith\tTabs")]
    public void SectionLabel_WhenProvidedInPrimaryConstructor_ReturnsProvidedValue(string sectionLabel)
    {
        // Arrange
        var context = new TestState();

        // Act
        var section = new LiveStateSection<TestState>(context, sectionLabel: sectionLabel);

        // Assert
        Assert.AreEqual(sectionLabel, section.SectionLabel);
    }

    /// <summary>
    /// Tests that SectionLabel returns null when the tree node label parameter
    /// is omitted in the primary constructor (using default value).
    /// </summary>
    [TestMethod]
    public void SectionLabel_WhenOmittedInPrimaryConstructor_ReturnsNull()
    {
        // Arrange
        var context = new TestState();

        // Act
        var section = new LiveStateSection<TestState>(context);

        // Assert
        Assert.IsNull(section.SectionLabel);
    }

    /// <summary>
    /// Tests that SectionLabel returns null when the tree node label parameter
    /// is omitted in the parameterless constructor (using default value).
    /// </summary>
    [TestMethod]
    public void SectionLabel_WhenOmittedInParameterlessConstructor_ReturnsNull()
    {
        // Arrange & Act
        var section = new LiveStateSection<TestState>();

        // Assert
        Assert.IsNull(section.SectionLabel);
    }

    /// <summary>
    /// Test state class for LiveStateSection testing.
    /// Decorated with LiveStateSectionDrawer attribute to satisfy the generic constraint.
    /// </summary>
    [LiveStateSectionDrawer<TestDrawer>]
    private sealed class TestState
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Test drawer that tracks disposal calls for verification.
    /// </summary>
    private sealed class TestDrawer : ILiveStateSectionDrawer<TestState>, IDisposable
    {
        private static int _disposeCallCount;

        /// <summary>
        /// Gets the number of times Dispose has been called across all instances.
        /// </summary>
        public static int DisposeCallCount => _disposeCallCount;

        /// <summary>
        /// Resets the dispose call counter.
        /// </summary>
        public static void Reset() => _disposeCallCount = 0;

        /// <inheritdoc/>
        public void Draw(TestState state)
        {
            // No-op for testing
        }

        /// <inheritdoc/>
        public void Dispose() => _disposeCallCount++;
    }

    /// <summary>
    /// Test drawer implementation for TestState.
    /// Retained for backward compatibility with existing tests.
    /// </summary>
    private sealed class TestStateDrawer : ILiveStateSectionDrawer<TestState>
    {
        public void Draw(TestState state)
        {
            // Empty implementation for testing purposes
        }

        public void Dispose()
        {
            // Empty implementation for testing purposes
        }
    }

    /// <summary>
    /// Tests that <see cref="LiveStateSection{T}.ExpandedByDefault"/> returns true
    /// when the primary constructor is called with expandedByDefault set to true.
    /// </summary>
    [TestMethod]
    public void ExpandedByDefault_PrimaryConstructorWithTrue_ReturnsTrue()
    {
        // Arrange
        var context = new TestState();

        // Act
        using var section = new LiveStateSection<TestState>(
            context,
            idScope: null,
            sectionLabel: null,
            expandedByDefault: true);

        // Assert
        Assert.IsTrue(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that <see cref="LiveStateSection{T}.ExpandedByDefault"/> returns false
    /// when the primary constructor is called without specifying expandedByDefault
    /// (using default parameter value).
    /// </summary>
    [TestMethod]
    public void ExpandedByDefault_PrimaryConstructorWithDefaultParameter_ReturnsFalse()
    {
        // Arrange
        var context = new TestState();

        // Act
        using var section = new LiveStateSection<TestState>(context);

        // Assert
        Assert.IsFalse(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that <see cref="LiveStateSection{T}.ExpandedByDefault"/> returns false
    /// when the parameterless constructor is used without specifying expandedByDefault
    /// (using default parameter value).
    /// </summary>
    [TestMethod]
    public void ExpandedByDefault_ParameterlessConstructorWithDefaultParameter_ReturnsFalse()
    {
        // Arrange & Act
        using var section = new LiveStateSection<TestState>();

        // Assert
        Assert.IsFalse(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that the parameterless constructor successfully creates an instance with default parameters
    /// when the type T is properly decorated with LiveStateSectionDrawerAttribute.
    /// </summary>
    [TestMethod]
    public void Constructor_DefaultParameters_CreatesInstanceSuccessfully()
    {
        // Arrange & Act
        var section = new LiveStateSection<ValidTestState>();

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(typeof(ValidTestState).FullName ?? typeof(ValidTestState).Name, section.SectionId);
        Assert.IsNull(section.SectionLabel);
        Assert.IsFalse(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that the parameterless constructor correctly sets the SectionId property
    /// when a custom idScope parameter is provided.
    /// </summary>
    [TestMethod]
    public void Constructor_CustomIdScope_SetsSectionIdCorrectly()
    {
        // Arrange
        const string customIdScope = "CustomScope";

        // Act
        var section = new LiveStateSection<ValidTestState>(idScope: customIdScope);

        // Assert
        Assert.AreEqual(customIdScope, section.SectionId);
    }

    /// <summary>
    /// Tests that the parameterless constructor correctly sets all properties
    /// when all parameters are provided with custom values.
    /// </summary>
    [TestMethod]
    public void Constructor_AllParametersProvided_SetsAllPropertiesCorrectly()
    {
        // Arrange
        const string customIdScope = "CustomId";
        const string customLabel = "Custom Tree Label";
        const bool defaultOpen = true;

        // Act
        var section = new LiveStateSection<ValidTestState>(
            idScope: customIdScope,
            sectionLabel: customLabel,
            expandedByDefault: defaultOpen);

        // Assert
        Assert.AreEqual(customIdScope, section.SectionId);
        Assert.AreEqual(customLabel, section.SectionLabel);
        Assert.AreEqual(defaultOpen, section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that when idScope is explicitly set to null, the SectionId falls back to
    /// the type's FullName (or Name if FullName is null).
    /// </summary>
    [TestMethod]
    public void Constructor_NullIdScope_UsesFallbackSectionId()
    {
        // Act
        var section = new LiveStateSection<ValidTestState>(idScope: null);

        // Assert
        var expectedId = typeof(ValidTestState).FullName ?? typeof(ValidTestState).Name;
        Assert.AreEqual(expectedId, section.SectionId);
    }

    /// <summary>
    /// Tests that the constructor rejects a null context instance.
    /// </summary>
    [TestMethod]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new LiveStateSection<TestState>((TestState)null!));

        Assert.AreEqual("context", exception.ParamName);
    }

    /// <summary>
    /// Tests that the constructor rejects whitespace-only id scopes.
    /// </summary>
    [TestMethod]
    public void Constructor_WhitespaceIdScope_ThrowsArgumentException()
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(() => _ = new LiveStateSection<TestState>(new TestState(), idScope: "   "));

        Assert.AreEqual("idScope", exception.ParamName);
    }

    /// <summary>
    /// Tests that the explicit-context constructor works even when the state type does not expose a
    /// public parameterless constructor.
    /// </summary>
    [TestMethod]
    public void Constructor_ExplicitContextWithoutParameterlessConstructor_CreatesInstanceSuccessfully()
    {
        var context = new StateWithoutParameterlessConstructor(42);

        using var section = new LiveStateSection<StateWithoutParameterlessConstructor>(context);

        Assert.IsNotNull(section);
        Assert.AreEqual(typeof(StateWithoutParameterlessConstructor).FullName ?? typeof(StateWithoutParameterlessConstructor).Name, section.SectionId);
    }

    /// <summary>
    /// Tests that the parameterless section constructor throws a clear exception when the state type
    /// lacks a public parameterless constructor.
    /// </summary>
    [TestMethod]
    public void Constructor_ParameterlessSectionWithoutParameterlessStateConstructor_ThrowsInvalidOperationException()
    {
        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => _ = new LiveStateSection<StateWithoutParameterlessConstructor>());

        Assert.Contains(nameof(StateWithoutParameterlessConstructor), exception.Message);
        Assert.IsNotNull(exception.InnerException);
        Assert.IsInstanceOfType<MissingMethodException>(exception.InnerException);
    }

    /// <summary>
    /// Tests that the parameterless constructor wraps TargetInvocationException in an
    /// InvalidOperationException with the original constructor exception as the inner exception.
    /// </summary>
    [TestMethod]
    public void Constructor_ParameterlessSectionWhenStateConstructorThrows_ThrowsInvalidOperationExceptionWithOriginalInner()
    {
        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => _ = new LiveStateSection<ThrowingState>());

        Assert.Contains(nameof(ThrowingState), exception.Message);
        Assert.IsNotNull(exception.InnerException);
        Assert.AreEqual("ctor threw", exception.InnerException.Message);
    }

    /// <summary>
    /// A valid test state type decorated with LiveStateSectionDrawerAttribute
    /// for testing valid constructor scenarios.
    /// </summary>
    [LiveStateSectionDrawer<ValidTestStateDrawer>]
    internal class ValidTestState
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Valid state type without a public parameterless constructor for constructor-path tests.
    /// </summary>
    [LiveStateSectionDrawer<StateWithoutParameterlessConstructorDrawer>]
    private sealed class StateWithoutParameterlessConstructor(int value)
    {
        public int Value { get; } = value;
    }

    /// <summary>
    /// State type whose parameterless constructor always throws, for testing TargetInvocationException unwrapping.
    /// </summary>
    [LiveStateSectionDrawer<ThrowingStateDrawer>]
    private sealed class ThrowingState
    {
        public ThrowingState()
        {
            throw new InvalidOperationException("ctor threw");
        }
    }

    /// <summary>
    /// A drawer implementation for ValidTestState.
    /// </summary>
    internal class ValidTestStateDrawer : ILiveStateSectionDrawer<ValidTestState>
    {
        public void Draw(ValidTestState state)
        {
            // Minimal implementation for testing
        }

        public void Dispose()
        {
            // Minimal implementation for testing
        }
    }

    /// <summary>
    /// Minimal drawer for <see cref="StateWithoutParameterlessConstructor"/>.
    /// </summary>
    private sealed class StateWithoutParameterlessConstructorDrawer : ILiveStateSectionDrawer<StateWithoutParameterlessConstructor>
    {
        public void Draw(StateWithoutParameterlessConstructor state)
        {
            // No-op for testing
        }

        public void Dispose()
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Minimal drawer for <see cref="ThrowingState"/>.
    /// </summary>
    private sealed class ThrowingStateDrawer : ILiveStateSectionDrawer<ThrowingState>
    {
        public void Draw(ThrowingState state)
        {
            // No-op for testing
        }

        public void Dispose()
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.Order"/> returns <see cref="int.MaxValue"/>
    /// when the state type has no <see cref="UmbraSectionOrderAttribute"/>.
    /// </summary>
    [TestMethod]
    public void Order_StateTypeWithoutSectionOrderAttribute_ReturnsIntMaxValue()
    {
        // Arrange
        var context = new StateWithoutOrderAttribute();

        // Act
        var section = new LiveStateSection<StateWithoutOrderAttribute>(context);

        // Assert
        Assert.AreEqual(int.MaxValue, section.Order);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.Order"/> returns zero
    /// when the state type has <see cref="UmbraSectionOrderAttribute"/> with Order = 0.
    /// </summary>
    [TestMethod]
    public void Order_StateTypeWithOrderZero_ReturnsZero()
    {
        // Arrange
        var context = new StateWithOrderZero();

        // Act
        var section = new LiveStateSection<StateWithOrderZero>(context);

        // Assert
        Assert.AreEqual(0, section.Order);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.Order"/> returns the positive order value
    /// when the state type has <see cref="UmbraSectionOrderAttribute"/> with a positive Order value.
    /// </summary>
    [TestMethod]
    public void Order_StateTypeWithPositiveOrder_ReturnsPositiveValue()
    {
        // Arrange
        var context = new StateWithOrderPositive();

        // Act
        var section = new LiveStateSection<StateWithOrderPositive>(context);

        // Assert
        Assert.AreEqual(100, section.Order);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.Order"/> returns the correct value
    /// when using the parameterless constructor that internally creates the state instance.
    /// </summary>
    [TestMethod]
    public void Order_ParameterlessConstructor_ReturnsExpectedOrderValue()
    {
        // Arrange & Act
        var section = new LiveStateSection<StateWithOrderPositive>();

        // Assert
        Assert.AreEqual(100, section.Order);
    }

    /// <summary>
    /// Test state type with no <see cref="UmbraSectionOrderAttribute"/> for verifying the default order.
    /// </summary>
    [LiveStateSectionDrawer<StateWithoutOrderAttributeDrawer>]
    private sealed class StateWithoutOrderAttribute
    {
    }

    /// <summary>
    /// Test state type with an explicit zero <see cref="UmbraSectionOrderAttribute"/> value.
    /// </summary>
    [LiveStateSectionDrawer<StateWithOrderZeroDrawer>]
    [UmbraSectionOrder(0)]
    private sealed class StateWithOrderZero
    {
    }

    /// <summary>
    /// Test state type with a positive <see cref="UmbraSectionOrderAttribute"/> value.
    /// </summary>
    [LiveStateSectionDrawer<StateWithOrderPositiveDrawer>]
    [UmbraSectionOrder(100)]
    private sealed class StateWithOrderPositive
    {
    }

    /// <summary>
    /// Minimal drawer for <see cref="StateWithoutOrderAttribute"/>.
    /// </summary>
    private sealed class StateWithoutOrderAttributeDrawer : ILiveStateSectionDrawer<StateWithoutOrderAttribute>
    {
        public void Draw(StateWithoutOrderAttribute state)
        {
            // No-op for testing
        }

        public void Dispose()
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Minimal drawer for <see cref="StateWithOrderZero"/>.
    /// </summary>
    private sealed class StateWithOrderZeroDrawer : ILiveStateSectionDrawer<StateWithOrderZero>
    {
        public void Draw(StateWithOrderZero state)
        {
            // No-op for testing
        }

        public void Dispose()
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Minimal drawer for <see cref="StateWithOrderPositive"/>.
    /// </summary>
    private sealed class StateWithOrderPositiveDrawer : ILiveStateSectionDrawer<StateWithOrderPositive>
    {
        public void Draw(StateWithOrderPositive state)
        {
            // No-op for testing
        }

        public void Dispose()
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Tests that calling Dispose for the first time disposes the underlying drawer.
    /// </summary>
    [TestMethod]
    public void Dispose_FirstCall_DisposesDrawer()
    {
        // Arrange
        TestDrawer.Reset();
        var section = new LiveStateSection<TestState>();

        // Act
        section.Dispose();

        // Assert
        Assert.AreEqual(1, TestDrawer.DisposeCallCount, "Drawer should be disposed exactly once.");
    }

    /// <summary>
    /// Tests that calling Dispose multiple times is idempotent and does not dispose the drawer more than once.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        TestDrawer.Reset();
        var section = new LiveStateSection<TestState>();

        // Act
        section.Dispose();
        section.Dispose();
        section.Dispose();

        // Assert
        Assert.AreEqual(1, TestDrawer.DisposeCallCount, "Drawer should be disposed exactly once even after multiple Dispose calls.");
    }
}


