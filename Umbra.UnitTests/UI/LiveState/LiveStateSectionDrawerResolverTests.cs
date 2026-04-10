namespace Umbra.UI.LiveState.UnitTests;

/// <summary>
/// Tests for <see cref="LiveStateSectionDrawerResolver"/>.
/// </summary>
[TestClass]
public partial class LiveStateSectionDrawerResolverTests
{
    /// <summary>
    /// Verifies that <see cref="LiveStateSectionDrawerResolver.Resolve"/> successfully
    /// returns a compiled <see cref="Action"/> when provided with a valid state type
    /// and drawer.
    /// </summary>
    [TestMethod]
    public void Resolve_ValidDrawerAndState_ReturnsCompiledAction()
    {
        // Arrange
        var stateType = typeof(ValidState);
        var context = new ValidState();

        // Act
        var result = LiveStateSectionDrawerResolver.Resolve(stateType, context, out var disposable);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(disposable);
        Assert.IsInstanceOfType<ValidDrawer>(disposable);
    }

    /// <summary>
    /// Verifies that the compiled <see cref="Action"/> returned by
    /// <see cref="LiveStateSectionDrawerResolver.Resolve"/> correctly invokes
    /// the drawer's Draw method when called.
    /// </summary>
    [TestMethod]
    public void Resolve_CompiledActionInvokesDrawMethod_Success()
    {
        // Arrange
        var stateType = typeof(ValidState);
        var context = new ValidState { Value = 42 };

        // Act
        var compiledAction = LiveStateSectionDrawerResolver.Resolve(stateType, context, out var disposable);
        compiledAction();

        // Assert
        var drawer = (ValidDrawer)disposable;
        Assert.IsTrue(drawer.DrawCalled);
        Assert.AreEqual(42, drawer.LastDrawnValue);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSectionDrawerResolver.Resolve"/> works correctly
    /// with a drawer that implements the interface for a base type when the state type
    /// is assignable to that base type.
    /// </summary>
    [TestMethod]
    public void Resolve_DrawerImplementsInterfaceForBaseType_Success()
    {
        // Arrange
        var stateType = typeof(DerivedState);
        var context = new DerivedState { Value = 100 };

        // Act
        var compiledAction = LiveStateSectionDrawerResolver.Resolve(stateType, context, out var disposable);
        compiledAction();

        // Assert
        var drawer = (BaseStateDrawer)disposable;
        Assert.IsTrue(drawer.DrawCalled);
        Assert.AreEqual(100, drawer.LastDrawnValue);
    }

    /// <summary>
    /// Verifies that resolution fails when the state type does not declare a drawer attribute.
    /// </summary>
    [TestMethod]
    public void Resolve_StateWithoutDrawerAttribute_ThrowsInvalidOperationException()
    {
        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => LiveStateSectionDrawerResolver.Resolve(typeof(StateWithoutDrawerAttribute), new StateWithoutDrawerAttribute(), out _));

        Assert.Contains("is not decorated", exception.Message);
    }

    /// <summary>
    /// Verifies that resolution fails when the declared drawer type is incompatible with the state type.
    /// </summary>
    [TestMethod]
    public void Resolve_IncompatibleDrawerType_ThrowsInvalidOperationException()
    {
        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => LiveStateSectionDrawerResolver.Resolve(typeof(StateWithIncompatibleDrawer), new StateWithIncompatibleDrawer(), out _));

        Assert.Contains("does not implement ILiveStateSectionDrawer<T>", exception.Message);
    }

    /// <summary>
    /// Verifies that resolution fails when the declared drawer cannot be instantiated.
    /// </summary>
    [TestMethod]
    public void Resolve_DrawerWithoutPublicParameterlessConstructor_ThrowsInvalidOperationException()
    {
        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => LiveStateSectionDrawerResolver.Resolve(typeof(StateWithDrawerWithoutDefaultConstructor), new StateWithDrawerWithoutDefaultConstructor(), out _));

        Assert.Contains("public parameterless constructor", exception.Message);
    }

    #region Test Helper Types

    /// <summary>
    /// Test attribute that implements <see cref="ILiveStateSectionDrawerAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    private sealed class TestDrawerAttribute : Attribute, ILiveStateSectionDrawerAttribute
    {
        public Type DrawerType { get; }

        public TestDrawerAttribute(Type drawerType)
        {
            DrawerType = drawerType;
        }
    }

    /// <summary>
    /// Valid state type for successful resolution testing.
    /// </summary>
    [TestDrawer(typeof(ValidDrawer))]
    private sealed class ValidState
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Valid drawer implementation.
    /// </summary>
    private sealed class ValidDrawer : ILiveStateSectionDrawer<ValidState>
    {
        public bool DrawCalled { get; private set; }
        public int LastDrawnValue { get; private set; }

        public void Draw(ValidState state)
        {
            DrawCalled = true;
            LastDrawnValue = state.Value;
        }
    }

    /// <summary>
    /// Base state type for inheritance testing.
    /// </summary>
    private class BaseState
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Derived state type.
    /// </summary>
    [TestDrawer(typeof(BaseStateDrawer))]
    private sealed class DerivedState : BaseState
    {
    }

    /// <summary>
    /// Drawer that implements ILiveStateSectionDrawer for the base type.
    /// </summary>
    private sealed class BaseStateDrawer : ILiveStateSectionDrawer<BaseState>
    {
        public bool DrawCalled { get; private set; }
        public int LastDrawnValue { get; private set; }

        public void Draw(BaseState state)
        {
            DrawCalled = true;
            LastDrawnValue = state.Value;
        }
    }

    private sealed class StateWithoutDrawerAttribute
    {
    }

    [TestDrawer(typeof(IncompatibleDrawer))]
    private sealed class StateWithIncompatibleDrawer
    {
    }

    [TestDrawer(typeof(DrawerWithoutDefaultConstructor))]
    private sealed class StateWithDrawerWithoutDefaultConstructor
    {
    }

    private sealed class IncompatibleDrawer : ILiveStateSectionDrawer<ValidState>
    {
        public void Draw(ValidState state)
        {
        }
    }

    private sealed class DrawerWithoutDefaultConstructor : ILiveStateSectionDrawer<StateWithDrawerWithoutDefaultConstructor>
    {
        public DrawerWithoutDefaultConstructor(string value)
        {
        }

        public void Draw(StateWithDrawerWithoutDefaultConstructor state)
        {
        }
    }

    #endregion
}
