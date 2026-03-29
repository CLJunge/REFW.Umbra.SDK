// <copyright file="NestedGroupDrawerBinderTests.cs" company="Umbra">
// Copyright (c) Umbra. All rights reserved.
// </copyright>

using Umbra.Config.Attributes;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Tests focused public behavior of <see cref="NestedGroupDrawerBinder"/>.
/// </summary>
[TestClass]
public sealed class NestedGroupDrawerBinderTests
{
    /// <summary>
    /// Verifies that a compatible drawer produces a draw action bound to the provided group instance.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_WithCompatibleDrawer_ReturnsActionThatDrawsProvidedGroup()
    {
        var attribute = new TestNestedGroupDrawerAttribute(typeof(TestGroupDrawer));
        var group = new TestGroup { Value = 42 };
        TestGroupDrawer.Reset();

        var action = NestedGroupDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), group, out var disposable);
        action?.Invoke();

        Assert.IsNotNull(action);
        Assert.IsNotNull(disposable);
        Assert.AreEqual(1, TestGroupDrawer.DrawCallCount);
        Assert.AreSame(group, TestGroupDrawer.LastDrawnGroup);
    }

    /// <summary>
    /// Verifies that an incompatible drawer is rejected without creating a disposable instance.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_WithIncompatibleDrawer_ReturnsNull()
    {
        var attribute = new TestNestedGroupDrawerAttribute(typeof(IncompatibleDrawer));
        var group = new TestGroup();

        var action = NestedGroupDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), group, out var disposable);

        Assert.IsNull(action);
        Assert.IsNull(disposable);
    }

    /// <summary>
    /// Verifies that a disposable drawer instance is surfaced to the caller and recreated per bind.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_WithDisposableDrawer_ReturnsTrackedDisposablePerCall()
    {
        var attribute = new TestNestedGroupDrawerAttribute(typeof(DisposableTestGroupDrawer));

        var firstAction = NestedGroupDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), new TestGroup(), out var firstDisposable);
        var secondAction = NestedGroupDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), new TestGroup(), out var secondDisposable);

        Assert.IsNotNull(firstAction);
        Assert.IsNotNull(secondAction);
        Assert.IsNotNull(firstDisposable);
        Assert.IsNotNull(secondDisposable);
        Assert.IsInstanceOfType<DisposableTestGroupDrawer>(firstDisposable);
        Assert.IsInstanceOfType<DisposableTestGroupDrawer>(secondDisposable);
        Assert.AreNotSame(firstDisposable, secondDisposable);
    }

    /// <summary>
    /// Verifies that drawer compatibility honors assignability so a base-group drawer can draw a derived group.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_WhenDrawerSupportsBaseType_AcceptsDerivedGroup()
    {
        var attribute = new TestNestedGroupDrawerAttribute(typeof(BaseGroupDrawer));
        var group = new DerivedGroup();
        BaseGroupDrawer.Reset();

        var action = NestedGroupDrawerBinder.BuildDrawAction(attribute, typeof(DerivedGroup), group, out var disposable);
        action?.Invoke();

        Assert.IsNotNull(action);
        Assert.IsNotNull(disposable);
        Assert.AreSame(group, BaseGroupDrawer.LastDrawnGroup);
    }

    /// <summary>
    /// Verifies that each returned action stays bound to the specific nested-group instance used to create it.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_MultipleBindings_KeepTheirOwnGroupInstances()
    {
        var attribute = new TestNestedGroupDrawerAttribute(typeof(TestGroupDrawer));
        var firstGroup = new TestGroup { Value = 1 };
        var secondGroup = new TestGroup { Value = 2 };
        TestGroupDrawer.Reset();

        var firstAction = NestedGroupDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), firstGroup, out _);
        var secondAction = NestedGroupDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), secondGroup, out _);

        firstAction?.Invoke();
        Assert.AreSame(firstGroup, TestGroupDrawer.LastDrawnGroup);

        secondAction?.Invoke();
        Assert.AreSame(secondGroup, TestGroupDrawer.LastDrawnGroup);
        Assert.AreEqual(2, TestGroupDrawer.DrawCallCount);
    }

    /// <summary>
    /// Minimal attribute implementation used to supply drawer types to the binder.
    /// </summary>
    private sealed class TestNestedGroupDrawerAttribute(Type drawerType) : INestedGroupDrawerAttribute
    {
        public Type DrawerType { get; } = drawerType;
    }

    /// <summary>
    /// Test nested group type.
    /// </summary>
    private sealed class TestGroup
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Drawer that records the group passed to <see cref="Draw"/>.
    /// </summary>
    private sealed class TestGroupDrawer : INestedGroupDrawer<TestGroup>
    {
        public static int DrawCallCount { get; private set; }

        public static TestGroup? LastDrawnGroup { get; private set; }

        public static void Reset()
        {
            DrawCallCount = 0;
            LastDrawnGroup = null;
        }

        public void Draw(TestGroup group)
        {
            DrawCallCount++;
            LastDrawnGroup = group;
        }
    }

    /// <summary>
    /// Drawer that supports <see cref="TestGroup"/> and is disposable.
    /// </summary>
    private sealed class DisposableTestGroupDrawer : INestedGroupDrawer<TestGroup>, IDisposable
    {
        public void Draw(TestGroup group)
        {
            // No-op for testing
        }

        public void Dispose()
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Group type unsupported by <see cref="IncompatibleDrawer"/>.
    /// </summary>
    private sealed class OtherGroup
    {
    }

    /// <summary>
    /// Drawer intentionally incompatible with <see cref="TestGroup"/>.
    /// </summary>
    private sealed class IncompatibleDrawer : INestedGroupDrawer<OtherGroup>
    {
        public void Draw(OtherGroup group)
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Base group type for assignability tests.
    /// </summary>
    private class BaseGroup
    {
    }

    /// <summary>
    /// Derived group type for assignability tests.
    /// </summary>
    private sealed class DerivedGroup : BaseGroup
    {
    }

    /// <summary>
    /// Drawer that records base-group invocations.
    /// </summary>
    private sealed class BaseGroupDrawer : INestedGroupDrawer<BaseGroup>
    {
        public static BaseGroup? LastDrawnGroup { get; private set; }

        public static void Reset() => LastDrawnGroup = null;

        public void Draw(BaseGroup group) => LastDrawnGroup = group;
    }
}
