using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra.UI.LiveState;


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
        Type stateType = typeof(ValidState);
        ValidState context = new ValidState();

        // Act
        Action result = LiveStateSectionDrawerResolver.Resolve(stateType, context, out IDisposable disposable);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(disposable);
        Assert.IsInstanceOfType(disposable, typeof(ValidDrawer));
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
        Type stateType = typeof(ValidState);
        ValidState context = new ValidState { Value = 42 };

        // Act
        Action compiledAction = LiveStateSectionDrawerResolver.Resolve(stateType, context, out IDisposable disposable);
        compiledAction();

        // Assert
        ValidDrawer drawer = (ValidDrawer)disposable;
        Assert.IsTrue(drawer.DrawCalled);
        Assert.AreEqual(42, drawer.LastDrawnValue);
    }

    /// <summary>
    /// Verifies that <see cref="LiveStateSectionDrawerResolver.Resolve"/> correctly
    /// sets the <paramref name="disposable"/> out parameter to the drawer instance.
    /// </summary>
    [TestMethod]
    public void Resolve_SetsDisposableOutParameter_Success()
    {
        // Arrange
        Type stateType = typeof(ValidState);
        ValidState context = new ValidState();

        // Act
        LiveStateSectionDrawerResolver.Resolve(stateType, context, out IDisposable disposable);

        // Assert
        Assert.IsNotNull(disposable);
        Assert.IsInstanceOfType(disposable, typeof(ILiveStateSectionDrawer<ValidState>));
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
        Type stateType = typeof(DerivedState);
        DerivedState context = new DerivedState { Value = 100 };

        // Act
        Action compiledAction = LiveStateSectionDrawerResolver.Resolve(stateType, context, out IDisposable disposable);
        compiledAction();

        // Assert
        BaseStateDrawer drawer = (BaseStateDrawer)disposable;
        Assert.IsTrue(drawer.DrawCalled);
        Assert.AreEqual(100, drawer.LastDrawnValue);
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
    /// State type without any drawer attribute.
    /// </summary>
    private sealed class StateWithoutAttribute
    {
    }

    /// <summary>
    /// State type with a drawer that has no parameterless constructor.
    /// </summary>
    [TestDrawer(typeof(DrawerWithoutConstructor))]
    private sealed class StateWithDrawerWithoutConstructor
    {
    }

    /// <summary>
    /// Drawer that requires constructor parameters.
    /// </summary>
    private sealed class DrawerWithoutConstructor : ILiveStateSectionDrawer<StateWithDrawerWithoutConstructor>
    {
        public DrawerWithoutConstructor(int requiredParam)
        {
        }

        public void Draw(StateWithDrawerWithoutConstructor state)
        {
        }
    }

    /// <summary>
    /// State type with a drawer whose constructor throws an exception.
    /// </summary>
    [TestDrawer(typeof(ThrowingDrawer))]
    private sealed class StateWithThrowingDrawer
    {
    }

    /// <summary>
    /// Drawer whose constructor throws an exception.
    /// </summary>
    private sealed class ThrowingDrawer : ILiveStateSectionDrawer<StateWithThrowingDrawer>
    {
        public ThrowingDrawer()
        {
            throw new InvalidOperationException("Constructor exception");
        }

        public void Draw(StateWithThrowingDrawer state)
        {
        }
    }

    /// <summary>
    /// State type with a drawer that does not implement ILiveStateSectionDrawer.
    /// </summary>
    [TestDrawer(typeof(NonDrawer))]
    private sealed class StateWithNonDrawer
    {
    }

    /// <summary>
    /// Class that does not implement ILiveStateSectionDrawer.
    /// </summary>
    private sealed class NonDrawer
    {
    }

    /// <summary>
    /// State type with a drawer that implements ILiveStateSectionDrawer for a different type.
    /// </summary>
    [TestDrawer(typeof(IncompatibleDrawer))]
    private sealed class StateWithIncompatibleDrawer
    {
    }

    /// <summary>
    /// Another state type used for incompatibility testing.
    /// </summary>
    private sealed class OtherState
    {
    }

    /// <summary>
    /// Drawer that implements ILiveStateSectionDrawer for a different state type.
    /// </summary>
    private sealed class IncompatibleDrawer : ILiveStateSectionDrawer<OtherState>
    {
        public void Draw(OtherState state)
        {
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

    private sealed class StateForMultiInterfaceDrawer
    {
        public string? Data { get; set; }
    }

    /// <summary>
    /// Another state type for multiple interface testing.
    /// </summary>
    private sealed class AnotherState
    {
        public string? Info { get; set; }
    }

    /// <summary>
    /// State type with an abstract drawer.
    /// </summary>
    [TestDrawer(typeof(AbstractDrawer))]
    private sealed class StateWithAbstractDrawer
    {
    }

    /// <summary>
    /// Abstract drawer that cannot be instantiated.
    /// </summary>
    private abstract class AbstractDrawer : ILiveStateSectionDrawer<StateWithAbstractDrawer>
    {
        public abstract void Draw(StateWithAbstractDrawer state);
    }

    #endregion
}