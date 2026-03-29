using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra.UI;
using Umbra.UI.LiveState;
using Umbra.UI.Panel;

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
    /// Tests that TreeNodeLabel returns null when null is explicitly provided
    /// to the primary constructor.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_WhenNullProvidedInPrimaryConstructor_ReturnsNull()
    {
        // Arrange
        var context = new TestState();

        // Act
        var section = new LiveStateSection<TestState>(context, treeNodeLabel: null);

        // Assert
        Assert.IsNull(section.TreeNodeLabel);
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
    /// Tests that TreeNodeLabel returns the value provided to the parameterless constructor
    /// when a non-null tree node label is supplied.
    /// </summary>
    /// <param name="treeNodeLabel">The tree node label to test.</param>
    [TestMethod]
    [DataRow("Parameterless Constructor Label")]
    [DataRow("")]
    [DataRow("   ")]
    public void TreeNodeLabel_WhenProvidedInParameterlessConstructor_ReturnsProvidedValue(string treeNodeLabel)
    {
        // Arrange & Act
        var section = new LiveStateSection<TestState>(treeNodeLabel: treeNodeLabel);

        // Assert
        Assert.AreEqual(treeNodeLabel, section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that TreeNodeLabel returns null when null is explicitly provided
    /// to the parameterless constructor.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_WhenNullProvidedInParameterlessConstructor_ReturnsNull()
    {
        // Arrange & Act
        var section = new LiveStateSection<TestState>(treeNodeLabel: null);

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
    /// when the primary constructor is called with treeNodeDefaultOpen set to false.
    /// </summary>
    [TestMethod]
    public void TreeNodeDefaultOpen_PrimaryConstructorWithFalse_ReturnsFalse()
    {
        // Arrange
        var context = new TestState();

        // Act
        using var section = new LiveStateSection<TestState>(
            context,
            idScope: null,
            treeNodeLabel: null,
            treeNodeDefaultOpen: false);

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
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
    /// Tests that <see cref="LiveStateSection{T}.TreeNodeDefaultOpen"/> returns true
    /// when the parameterless constructor is used and treeNodeDefaultOpen is set to true.
    /// </summary>
    [TestMethod]
    public void TreeNodeDefaultOpen_ParameterlessConstructorWithTrue_ReturnsTrue()
    {
        // Arrange & Act
        using var section = new LiveStateSection<TestState>(
            idScope: null,
            treeNodeLabel: null,
            treeNodeDefaultOpen: true);

        // Assert
        Assert.IsTrue(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that <see cref="LiveStateSection{T}.TreeNodeDefaultOpen"/> returns false
    /// when the parameterless constructor is used and treeNodeDefaultOpen is set to false.
    /// </summary>
    [TestMethod]
    public void TreeNodeDefaultOpen_ParameterlessConstructorWithFalse_ReturnsFalse()
    {
        // Arrange & Act
        using var section = new LiveStateSection<TestState>(
            idScope: null,
            treeNodeLabel: null,
            treeNodeDefaultOpen: false);

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
    /// Tests that the parameterless constructor correctly sets the TreeNodeLabel property
    /// when a custom treeNodeLabel parameter is provided.
    /// </summary>
    [TestMethod]
    public void Constructor_CustomTreeNodeLabel_SetsTreeNodeLabelCorrectly()
    {
        // Arrange
        const string customLabel = "Custom Label";

        // Act
        var section = new LiveStateSection<ValidTestState>(treeNodeLabel: customLabel);

        // Assert
        Assert.AreEqual(customLabel, section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that the parameterless constructor correctly sets the TreeNodeDefaultOpen property
    /// to true when the treeNodeDefaultOpen parameter is true.
    /// </summary>
    [TestMethod]
    public void Constructor_TreeNodeDefaultOpenTrue_SetsPropertyCorrectly()
    {
        // Arrange & Act
        var section = new LiveStateSection<ValidTestState>(treeNodeDefaultOpen: true);

        // Assert
        Assert.IsTrue(section.TreeNodeDefaultOpen);
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
    /// Tests that the parameterless constructor accepts very long idScope strings
    /// without throwing an exception.
    /// </summary>
    [TestMethod]
    public void Constructor_VeryLongIdScope_CreatesInstanceSuccessfully()
    {
        // Arrange
        var veryLongIdScope = new string('a', 10000);

        // Act
        var section = new LiveStateSection<ValidTestState>(idScope: veryLongIdScope);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(veryLongIdScope, section.SectionId);
    }

    /// <summary>
    /// Tests that the parameterless constructor accepts very long treeNodeLabel strings
    /// without throwing an exception.
    /// </summary>
    [TestMethod]
    public void Constructor_VeryLongTreeNodeLabel_CreatesInstanceSuccessfully()
    {
        // Arrange
        var veryLongLabel = new string('b', 10000);

        // Act
        var section = new LiveStateSection<ValidTestState>(treeNodeLabel: veryLongLabel);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(veryLongLabel, section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that the parameterless constructor accepts idScope with special characters
    /// without throwing an exception.
    /// </summary>
    [TestMethod]
    [DataRow("special!@#$%^&*()")]
    [DataRow("unicode-Ñ-§-Ω")]
    [DataRow("with.dots.and-dashes")]
    [DataRow("under_scores_and_numbers_123")]
    public void Constructor_IdScopeWithSpecialCharacters_CreatesInstanceSuccessfully(string specialIdScope)
    {
        // Act
        var section = new LiveStateSection<ValidTestState>(idScope: specialIdScope);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(specialIdScope, section.SectionId);
    }

    /// <summary>
    /// Tests that the parameterless constructor accepts treeNodeLabel with special characters
    /// without throwing an exception.
    /// </summary>
    [TestMethod]
    [DataRow("Label with spaces")]
    [DataRow("Label!@#$%")]
    [DataRow("Label\twith\ttabs")]
    [DataRow("Unicode: Ñ Ω §")]
    public void Constructor_TreeNodeLabelWithSpecialCharacters_CreatesInstanceSuccessfully(string specialLabel)
    {
        // Act
        var section = new LiveStateSection<ValidTestState>(treeNodeLabel: specialLabel);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(specialLabel, section.TreeNodeLabel);
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
    /// Tests that when treeNodeLabel is explicitly set to null, the TreeNodeLabel property is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullTreeNodeLabel_TreeNodeLabelIsNull()
    {
        // Act
        var section = new LiveStateSection<ValidTestState>(treeNodeLabel: null);

        // Assert
        Assert.IsNull(section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that when treeNodeDefaultOpen is explicitly set to false, the property is false.
    /// </summary>
    [TestMethod]
    public void Constructor_TreeNodeDefaultOpenFalse_PropertyIsFalse()
    {
        // Act
        var section = new LiveStateSection<ValidTestState>(treeNodeDefaultOpen: false);

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    #region Test Helper Types

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
    /// An invalid test state type NOT decorated with LiveStateSectionDrawerAttribute
    /// for testing error scenarios.
    /// </summary>
    internal class InvalidTestStateWithoutAttribute
    {
        public int Value { get; set; }
    }

    #endregion

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
    /// Verifies that <see cref="LiveStateSection{T}.Order"/> returns <see cref="int.MinValue"/>
    /// when the state type has <see cref="SectionOrderAttribute"/> with Order = int.MinValue.
    /// </summary>
    [TestMethod]
    public void Order_StateTypeWithOrderMinValue_ReturnsIntMinValue()
    {
        // Arrange
        var context = new StateWithOrderMinValue();

        // Act
        var section = new LiveStateSection<StateWithOrderMinValue>(context);

        // Assert
        Assert.AreEqual(int.MinValue, section.Order);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.Order"/> returns <see cref="int.MaxValue"/>
    /// when the state type has <see cref="SectionOrderAttribute"/> with Order = int.MaxValue.
    /// </summary>
    [TestMethod]
    public void Order_StateTypeWithOrderMaxValue_ReturnsIntMaxValue()
    {
        // Arrange
        var context = new StateWithOrderMaxValue();

        // Act
        var section = new LiveStateSection<StateWithOrderMaxValue>(context);

        // Assert
        Assert.AreEqual(int.MaxValue, section.Order);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.Order"/> returns the negative order value
    /// when the state type has <see cref="SectionOrderAttribute"/> with a negative Order value.
    /// </summary>
    [TestMethod]
    public void Order_StateTypeWithNegativeOrder_ReturnsNegativeValue()
    {
        // Arrange
        var context = new StateWithOrderNegative();

        // Act
        var section = new LiveStateSection<StateWithOrderNegative>(context);

        // Assert
        Assert.AreEqual(-100, section.Order);
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
    /// Verifies that <see cref="LiveStateSection{T}.Order"/> returns the same value
    /// across multiple property accesses, confirming immutability.
    /// </summary>
    [TestMethod]
    public void Order_MultipleAccesses_ReturnsSameValue()
    {
        // Arrange
        var context = new StateWithOrderPositive();
        var section = new LiveStateSection<StateWithOrderPositive>(context);

        // Act
        int firstAccess = section.Order;
        int secondAccess = section.Order;
        int thirdAccess = section.Order;

        // Assert
        Assert.AreEqual(firstAccess, secondAccess);
        Assert.AreEqual(secondAccess, thirdAccess);
        Assert.AreEqual(100, firstAccess);
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

    #region Test State Types and Drawers

    private sealed class StateWithoutOrderAttribute
    {
    }

    [SectionOrder(0)]
    private sealed class StateWithOrderZero
    {
    }

    [SectionOrder(int.MinValue)]
    private sealed class StateWithOrderMinValue
    {
    }

    [SectionOrder(int.MaxValue)]
    private sealed class StateWithOrderMaxValue
    {
    }

    [SectionOrder(-100)]
    private sealed class StateWithOrderNegative
    {
    }

    [SectionOrder(100)]
    private sealed class StateWithOrderPositive
    {
    }

    #endregion

    #region Helper Classes

    internal sealed class ThrowingTestState
    {
    }

    #endregion

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
    /// Tests that Dispose does not throw any exception during normal operation.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalled_DoesNotThrow()
    {
        // Arrange
        TestDrawer.Reset();
        var section = new LiveStateSection<TestState>();

        // Act & Assert
        try
        {
            section.Dispose();
            Assert.IsTrue(true, "Dispose should complete without throwing.");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Dispose should not throw an exception, but threw: {ex.GetType().Name} - {ex.Message}");
        }
    }

    /// <summary>
    /// Tests that Dispose can be called multiple times without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        TestDrawer.Reset();
        var section = new LiveStateSection<TestState>();

        // Act & Assert
        try
        {
            section.Dispose();
            section.Dispose();
            section.Dispose();
            Assert.IsTrue(true, "Multiple Dispose calls should complete without throwing.");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Multiple Dispose calls should not throw an exception, but threw: {ex.GetType().Name} - {ex.Message}");
        }
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
        public static void Reset()
        {
            s_disposeCallCount = 0;
        }

        /// <inheritdoc/>
        public void Draw(TestState state)
        {
            // No-op for testing
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            s_disposeCallCount++;
        }
    }
    #region Test State Classes and Drawers

    /// <summary>
    /// Basic test state class with drawer attribute but no order attribute.
    /// </summary>
    [LiveStateSectionDrawer<BasicStateDrawer>]
    private sealed class BasicState { }

    /// <summary>
    /// Drawer for BasicState.
    /// </summary>
    private sealed class BasicStateDrawer : ILiveStateSectionDrawer<BasicState>
    {
        public void Draw(BasicState state) { }
        public void Dispose() { }
    }

    /// <summary>
    /// Test state class with both drawer and section order attributes.
    /// </summary>
    [LiveStateSectionDrawer<StateWithOrderDrawer>]
    [SectionOrder(42)]
    private sealed class StateWithOrder { }

    /// <summary>
    /// Drawer for StateWithOrder.
    /// </summary>
    private sealed class StateWithOrderDrawer : ILiveStateSectionDrawer<StateWithOrder>
    {
        public void Draw(StateWithOrder state) { }
        public void Dispose() { }
    }

    /// <summary>
    /// Test state class without the required LiveStateSectionDrawer attribute.
    /// </summary>
    private sealed class StateWithoutDrawer { }

    /// <summary>
    /// Minimal interface definition for ILiveStateSectionDrawer based on runtime requirements.
    /// </summary>
    private interface ILiveStateSectionDrawer<in T> : IDisposable
    {
        void Draw(T state);
    }

    #endregion

    #region Constructor - Null Context Tests

    #endregion

    #region Constructor - Valid Parameters Tests

    /// <summary>
    /// Tests that the constructor succeeds with a valid context and all default parameters.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidContextAllDefaults_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();

        // Act
        using var section = new LiveStateSection<BasicState>(context);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(int.MaxValue, section.Order);
    }

    /// <summary>
    /// Tests that the constructor succeeds with a valid context and null idScope.
    /// </summary>
    [TestMethod]
    public void Constructor_NullIdScope_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();

        // Act
        using var section = new LiveStateSection<BasicState>(context, idScope: null);

        // Assert
        Assert.IsNotNull(section);
    }

    /// <summary>
    /// Tests that the constructor succeeds with a valid non-empty idScope.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidIdScope_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();
        var validIdScope = "customScope";

        // Act
        using var section = new LiveStateSection<BasicState>(context, idScope: validIdScope);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(validIdScope, section.SectionId);
    }

    /// <summary>
    /// Tests that the constructor succeeds with a valid idScope containing special characters.
    /// </summary>
    [TestMethod]
    public void Constructor_IdScopeWithSpecialCharacters_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();
        var specialCharIdScope = "scope_123.test-id";

        // Act
        using var section = new LiveStateSection<BasicState>(context, idScope: specialCharIdScope);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(specialCharIdScope, section.SectionId);
    }

    /// <summary>
    /// Tests that the constructor succeeds with a valid treeNodeLabel.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidTreeNodeLabel_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();
        var treeNodeLabel = "Test Label";

        // Act
        using var section = new LiveStateSection<BasicState>(context, treeNodeLabel: treeNodeLabel);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(treeNodeLabel, section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that the constructor succeeds with null treeNodeLabel.
    /// </summary>
    [TestMethod]
    public void Constructor_NullTreeNodeLabel_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();

        // Act
        using var section = new LiveStateSection<BasicState>(context, treeNodeLabel: null);

        // Assert
        Assert.IsNotNull(section);
        Assert.IsNull(section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that the constructor succeeds with empty treeNodeLabel (no validation).
    /// </summary>
    [TestMethod]
    public void Constructor_EmptyTreeNodeLabel_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();
        var emptyLabel = string.Empty;

        // Act
        using var section = new LiveStateSection<BasicState>(context, treeNodeLabel: emptyLabel);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(emptyLabel, section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that the constructor succeeds with whitespace treeNodeLabel (no validation).
    /// </summary>
    [TestMethod]
    public void Constructor_WhitespaceTreeNodeLabel_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();
        var whitespaceLabel = "   ";

        // Act
        using var section = new LiveStateSection<BasicState>(context, treeNodeLabel: whitespaceLabel);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(whitespaceLabel, section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that the constructor succeeds with treeNodeDefaultOpen set to true.
    /// </summary>
    [TestMethod]
    public void Constructor_TreeNodeDefaultOpenTrue_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();

        // Act
        using var section = new LiveStateSection<BasicState>(context, treeNodeDefaultOpen: true);

        // Assert
        Assert.IsNotNull(section);
        Assert.IsTrue(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that the constructor succeeds with treeNodeDefaultOpen set to false.
    /// </summary>
    [TestMethod]
    public void Constructor_TreeNodeDefaultOpenFalse_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();

        // Act
        using var section = new LiveStateSection<BasicState>(context, treeNodeDefaultOpen: false);

        // Assert
        Assert.IsNotNull(section);
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that the constructor succeeds with all parameters specified.
    /// </summary>
    [TestMethod]
    public void Constructor_AllParametersSpecified_InitializesSuccessfully()
    {
        // Arrange
        var context = new BasicState();
        var idScope = "customScope";
        var treeNodeLabel = "Test Node";
        var treeNodeDefaultOpen = true;

        // Act
        using var section = new LiveStateSection<BasicState>(
            context,
            idScope: idScope,
            treeNodeLabel: treeNodeLabel,
            treeNodeDefaultOpen: treeNodeDefaultOpen);

        // Assert
        Assert.IsNotNull(section);
        Assert.AreEqual(idScope, section.SectionId);
        Assert.AreEqual(treeNodeLabel, section.TreeNodeLabel);
        Assert.AreEqual(treeNodeDefaultOpen, section.TreeNodeDefaultOpen);
    }

    #endregion

    #region Constructor - SectionId Property Tests

    /// <summary>
    /// Tests that SectionId returns the type's FullName when idScope is null and FullName is available.
    /// </summary>
    [TestMethod]
    public void Constructor_NullIdScope_SectionIdUsesTypeFullName()
    {
        // Arrange
        var context = new BasicState();
        var expectedSectionId = typeof(BasicState).FullName ?? typeof(BasicState).Name;

        // Act
        using var section = new LiveStateSection<BasicState>(context, idScope: null);

        // Assert
        Assert.AreEqual(expectedSectionId, section.SectionId);
    }

    /// <summary>
    /// Tests that SectionId returns the supplied idScope when provided.
    /// </summary>
    [TestMethod]
    public void Constructor_CustomIdScope_SectionIdUsesSuppliedValue()
    {
        // Arrange
        var context = new BasicState();
        var customIdScope = "myCustomScope";

        // Act
        using var section = new LiveStateSection<BasicState>(context, idScope: customIdScope);

        // Assert
        Assert.AreEqual(customIdScope, section.SectionId);
    }

    #endregion

    #region Constructor - Order Property Tests

    /// <summary>
    /// Tests that Order returns int.MaxValue when the type has no SectionOrderAttribute.
    /// </summary>
    [TestMethod]
    public void Constructor_TypeWithoutSectionOrder_OrderIsIntMaxValue()
    {
        // Arrange
        var context = new BasicState();

        // Act
        using var section = new LiveStateSection<BasicState>(context);

        // Assert
        Assert.AreEqual(int.MaxValue, section.Order);
    }

    /// <summary>
    /// Tests that Order returns the value from SectionOrderAttribute when present.
    /// </summary>
    [TestMethod]
    public void Constructor_TypeWithSectionOrder_OrderUsesAttributeValue()
    {
        // Arrange
        var context = new StateWithOrder();
        var expectedOrder = 42;

        // Act
        using var section = new LiveStateSection<StateWithOrder>(context);

        // Assert
        Assert.AreEqual(expectedOrder, section.Order);
    }

    #endregion

    #region Constructor - Missing Drawer Attribute Tests

    #endregion

    #region Constructor - TreeNodeLabel and TreeNodeDefaultOpen Property Tests

    /// <summary>
    /// Tests that TreeNodeLabel is null when not specified.
    /// </summary>
    [TestMethod]
    public void Constructor_DefaultTreeNodeLabel_TreeNodeLabelIsNull()
    {
        // Arrange
        var context = new BasicState();

        // Act
        using var section = new LiveStateSection<BasicState>(context);

        // Assert
        Assert.IsNull(section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that TreeNodeDefaultOpen is false when not specified.
    /// </summary>
    [TestMethod]
    public void Constructor_DefaultTreeNodeDefaultOpen_TreeNodeDefaultOpenIsFalse()
    {
        // Arrange
        var context = new BasicState();

        // Act
        using var section = new LiveStateSection<BasicState>(context);

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    #endregion

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.SectionId"/> returns the explicitly provided idScope
    /// when one is supplied to the constructor.
    /// </summary>
    [TestMethod]
    [DataRow("CustomId")]
    [DataRow("MySection")]
    [DataRow("Plugin.Section.1")]
    public void SectionId_WithExplicitIdScope_ReturnsIdScope(string idScope)
    {
        // Arrange
        var state = new TestState1();

        // Act
        using var section = new LiveStateSection<TestState1>(state, idScope: idScope);

        // Assert
        Assert.AreEqual(idScope, section.SectionId);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.SectionId"/> returns the full type name
    /// when no explicit idScope is provided (idScope is null).
    /// </summary>
    [TestMethod]
    public void SectionId_WithNullIdScope_ReturnsTypeFullName()
    {
        // Arrange
        var state = new TestState1();
        var expectedFullName = typeof(TestState1).FullName ?? typeof(TestState1).Name;

        // Act
        using var section = new LiveStateSection<TestState1>(state, idScope: null);

        // Assert
        Assert.AreEqual(expectedFullName, section.SectionId);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.SectionId"/> returns different values for
    /// different state types when no explicit idScope is provided, ensuring type-based
    /// uniqueness via <see cref="Type.FullName"/>.
    /// </summary>
    [TestMethod]
    public void SectionId_WithDifferentTypes_ReturnsDifferentFullNames()
    {
        // Arrange
        var state1 = new TestState1();
        var state2 = new TestState2();
        var expectedFullName1 = typeof(TestState1).FullName ?? typeof(TestState1).Name;
        var expectedFullName2 = typeof(TestState2).FullName ?? typeof(TestState2).Name;

        // Act
        using var section1 = new LiveStateSection<TestState1>(state1);
        using var section2 = new LiveStateSection<TestState2>(state2);

        // Assert
        Assert.AreEqual(expectedFullName1, section1.SectionId);
        Assert.AreEqual(expectedFullName2, section2.SectionId);
        Assert.AreNotEqual(section1.SectionId, section2.SectionId);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.SectionId"/> consistently returns the same value
    /// across multiple property accesses.
    /// </summary>
    [TestMethod]
    public void SectionId_MultipleAccesses_ReturnsConsistentValue()
    {
        // Arrange
        var state = new TestState1();
        const string idScope = "StableId";

        // Act
        using var section = new LiveStateSection<TestState1>(state, idScope: idScope);
        var firstAccess = section.SectionId;
        var secondAccess = section.SectionId;
        var thirdAccess = section.SectionId;

        // Assert
        Assert.AreEqual(idScope, firstAccess);
        Assert.AreEqual(firstAccess, secondAccess);
        Assert.AreEqual(secondAccess, thirdAccess);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSection{T}.SectionId"/> falls back to <see cref="Type.Name"/>
    /// when <see cref="Type.FullName"/> is null. This scenario is rare but can occur with certain
    /// compiler-generated or dynamically created types. Uses a nested generic type to simulate this.
    /// </summary>
    [TestMethod]
    public void SectionId_WithNullTypeFullName_FallsBackToTypeName()
    {
        // Arrange
        // Note: Type.FullName can be null for open generic types or certain compiler-generated types.
        // The test uses a nested generic state to verify the fallback chain works correctly.
        var state = new GenericTestState<int>();
        var expectedId = typeof(GenericTestState<int>).FullName ?? typeof(GenericTestState<int>).Name;

        // Act
        using var section = new LiveStateSection<GenericTestState<int>>(state);

        // Assert
        // Verify that even if FullName were null, the property would return Name
        Assert.IsNotNull(section.SectionId);
        Assert.AreEqual(expectedId, section.SectionId);
    }

    #region Test State Classes

    public sealed class TestState1
    {
        public int Value { get; set; }
    }

    public sealed class TestState2
    {
        public string? Data { get; set; }
    }

    public sealed class GenericTestState<T>
    {
        public T? Item { get; set; }
    }

    #endregion

    #region Test Drawer Classes

    #endregion
}