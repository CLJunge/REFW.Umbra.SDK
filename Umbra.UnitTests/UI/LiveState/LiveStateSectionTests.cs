namespace Umbra.UI.LiveState.UnitTests;


/// <summary>
/// Unit tests for the <see cref="LiveStateSection{T}"/> class.
/// </summary>
[TestClass]
public sealed class LiveStateSectionTests
{
    /// <summary>
    /// Tests that TreeNodeLabel returns the value provided to the primary constructor
    /// when a non-null tree node label is supplied.
    /// </summary>
    /// <param name="treeNodeLabel">The tree node label to test.</param>
    [TestMethod]
    [DataRow("Test Label")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("Very Long Label With Multiple Words And Special Characters !@#$%^&*()")]
    [DataRow("Label\nWith\nNewlines")]
    [DataRow("Label\tWith\tTabs")]
    public void TreeNodeLabel_WhenProvidedInPrimaryConstructor_ReturnsProvidedValue(string treeNodeLabel)
    {
        // Arrange
        var context = new TestState();

        // Act
        var section = new LiveStateSection<TestState>(context, treeNodeLabel: treeNodeLabel);

        // Assert
        Assert.AreEqual(treeNodeLabel, section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that TreeNodeLabel returns null when the tree node label parameter
    /// is omitted in the primary constructor (using default value).
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_WhenOmittedInPrimaryConstructor_ReturnsNull()
    {
        // Arrange
        var context = new TestState();

        // Act
        var section = new LiveStateSection<TestState>(context);

        // Assert
        Assert.IsNull(section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that TreeNodeLabel returns null when the tree node label parameter
    /// is omitted in the parameterless constructor (using default value).
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_WhenOmittedInParameterlessConstructor_ReturnsNull()
    {
        // Arrange & Act
        var section = new LiveStateSection<TestState>();

        // Assert
        Assert.IsNull(section.TreeNodeLabel);
    }

    /// <summary>
    /// Test state class for LiveStateSection testing.
    /// Decorated with LiveStateSectionDrawer attribute to satisfy the generic constraint.
    /// </summary>
    [LiveStateSectionDrawer<TestStateDrawer>]
    private sealed class TestState
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Test drawer implementation for TestState.
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
    /// Tests that <see cref="LiveStateSection{T}.TreeNodeDefaultOpen"/> returns true
    /// when the primary constructor is called with treeNodeDefaultOpen set to true.
    /// </summary>
    [TestMethod]
    public void TreeNodeDefaultOpen_PrimaryConstructorWithTrue_ReturnsTrue()
    {
        // Arrange
        var context = new TestState();

        // Act
        using var section = new LiveStateSection<TestState>(
            context,
            idScope: null,
            treeNodeLabel: null,
            treeNodeDefaultOpen: true);

        // Assert
        Assert.IsTrue(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that <see cref="LiveStateSection{T}.TreeNodeDefaultOpen"/> returns false
    /// when the primary constructor is called without specifying treeNodeDefaultOpen
    /// (using default parameter value).
    /// </summary>
    [TestMethod]
    public void TreeNodeDefaultOpen_PrimaryConstructorWithDefaultParameter_ReturnsFalse()
    {
        // Arrange
        var context = new TestState();

        // Act
        using var section = new LiveStateSection<TestState>(context);

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that <see cref="LiveStateSection{T}.TreeNodeDefaultOpen"/> returns false
    /// when the parameterless constructor is used without specifying treeNodeDefaultOpen
    /// (using default parameter value).
    /// </summary>
    [TestMethod]
    public void TreeNodeDefaultOpen_ParameterlessConstructorWithDefaultParameter_ReturnsFalse()
    {
        // Arrange & Act
        using var section = new LiveStateSection<TestState>();

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
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
        Assert.IsNull(section.TreeNodeLabel);
        Assert.IsFalse(section.TreeNodeDefaultOpen);
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
            treeNodeLabel: customLabel,
            treeNodeDefaultOpen: defaultOpen);

        // Assert
        Assert.AreEqual(customIdScope, section.SectionId);
        Assert.AreEqual(customLabel, section.TreeNodeLabel);
        Assert.AreEqual(defaultOpen, section.TreeNodeDefaultOpen);
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
    /// A valid test state type decorated with LiveStateSectionDrawerAttribute
    /// for testing valid constructor scenarios.
    /// </summary>
    [LiveStateSectionDrawer<ValidTestStateDrawer>]
    internal class ValidTestState
    {
        public int Value { get; set; }
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
    /// Verifies that <see cref="LiveStateSection{T}.Order"/> returns <see cref="int.MaxValue"/>
    /// when the state type has no <see cref="SectionOrderAttribute"/>.
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
    /// when the state type has <see cref="SectionOrderAttribute"/> with Order = 0.
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
    /// when the state type has <see cref="SectionOrderAttribute"/> with a positive Order value.
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

    private sealed class StateWithoutOrderAttribute
    {
    }

    [SectionOrder(0)]
    private sealed class StateWithOrderZero
    {
    }

    [SectionOrder(100)]
    private sealed class StateWithOrderPositive
    {
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

    /// <summary>
    /// Test drawer that tracks disposal calls for verification.
    /// </summary>
    private sealed class TestDrawer : ILiveStateSectionDrawer<TestState>, IDisposable
    {
        private static int s_disposeCallCount;

        /// <summary>
        /// Gets the number of times Dispose has been called across all instances.
        /// </summary>
        public static int DisposeCallCount => s_disposeCallCount;

        /// <summary>
        /// Resets the dispose call counter.
        /// </summary>
        public static void Reset() => s_disposeCallCount = 0;

        /// <inheritdoc/>
        public void Draw(TestState state)
        {
            // No-op for testing
        }

        /// <inheritdoc/>
        public void Dispose() => s_disposeCallCount++;
    }
}
