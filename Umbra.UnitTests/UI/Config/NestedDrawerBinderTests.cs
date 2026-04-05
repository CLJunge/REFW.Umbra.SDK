// <copyright file="NestedGroupDrawerBinderTests.cs" company="Umbra">
// Copyright (c) Umbra. All rights reserved.
// </copyright>

using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Tests focused public behavior of <see cref="NestedDrawerBinder"/>.
/// </summary>
[TestClass]
public sealed class NestedDrawerBinderTests
{
    /// <summary>
    /// Verifies that a compatible drawer produces a draw action bound to the provided group instance.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_WithCompatibleDrawer_ReturnsActionThatDrawsProvidedGroup()
    {
        var attribute = new TestNestedDrawerAttribute(typeof(TestGroupDrawer));
        var group = new TestGroup { Value = 42 };
        TestGroupDrawer.Reset();

        var action = NestedDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), group, out var disposable);
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
        var attribute = new TestNestedDrawerAttribute(typeof(IncompatibleDrawer));
        var group = new TestGroup();

        var action = NestedDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), group, out var disposable);

        Assert.IsNull(action);
        Assert.IsNull(disposable);
    }

    /// <summary>
    /// Verifies that a disposable drawer instance is surfaced to the caller and recreated per bind.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_WithDisposableDrawer_ReturnsTrackedDisposablePerCall()
    {
        var attribute = new TestNestedDrawerAttribute(typeof(DisposableTestGroupDrawer));

        var firstAction = NestedDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), new TestGroup(), out var firstDisposable);
        var secondAction = NestedDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), new TestGroup(), out var secondDisposable);

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
        var attribute = new TestNestedDrawerAttribute(typeof(BaseGroupDrawer));
        var group = new DerivedGroup();
        BaseGroupDrawer.Reset();

        var action = NestedDrawerBinder.BuildDrawAction(attribute, typeof(DerivedGroup), group, out var disposable);
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
        var attribute = new TestNestedDrawerAttribute(typeof(TestGroupDrawer));
        var firstGroup = new TestGroup { Value = 1 };
        var secondGroup = new TestGroup { Value = 2 };
        TestGroupDrawer.Reset();

        var firstAction = NestedDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), firstGroup, out _);
        var secondAction = NestedDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), secondGroup, out _);

        firstAction?.Invoke();
        Assert.AreSame(firstGroup, TestGroupDrawer.LastDrawnGroup);

        secondAction?.Invoke();
        Assert.AreSame(secondGroup, TestGroupDrawer.LastDrawnGroup);
        Assert.AreEqual(2, TestGroupDrawer.DrawCallCount);
    }

    /// <summary>
    /// Verifies that a compatible drawer without a public parameterless constructor fails at activation time.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_CompatibleDrawerWithoutParameterlessConstructor_ThrowsMissingMethodException()
    {
        var attribute = new TestNestedDrawerAttribute(typeof(CompatibleDrawerWithoutParameterlessConstructor));
        var group = new TestGroup();

        Assert.ThrowsExactly<MissingMethodException>(() => NestedDrawerBinder.BuildDrawAction(attribute, typeof(TestGroup), group, out _));
    }

    /// <summary>
    /// Verifies that <see cref="ConfigTransferDrawer"/> no longer binds through the nested-drawer binder.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_WithConfigTransferDrawer_ReturnsNull()
    {
        var attribute = new TestNestedDrawerAttribute(typeof(ConfigTransferDrawer));
        var group = new TestTransferGroup();

        var action = NestedDrawerBinder.BuildDrawAction(attribute, typeof(TestTransferGroup), group, out var disposable);

        Assert.IsNull(action);
        Assert.IsNull(disposable);
    }

    /// <summary>
    /// Minimal attribute implementation used to supply drawer types to the binder.
    /// </summary>
    private sealed class TestNestedDrawerAttribute(Type drawerType) : INestedDrawerAttribute
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
    private sealed class TestGroupDrawer : INestedDrawer<TestGroup>
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
    private sealed class DisposableTestGroupDrawer : INestedDrawer<TestGroup>, IDisposable
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
    private sealed class IncompatibleDrawer : INestedDrawer<OtherGroup>
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
    /// Concrete transfer-group type used to verify assignable nested-drawer binding.
    /// </summary>
    private sealed class TestTransferGroup
    {
        public Parameter<string> ConfigFilePath { get; } = new("config.json");

        public Parameter<Action> ImportConfig { get; } = new(static () => { });

        public Parameter<Action> ExportConfig { get; } = new(static () => { });
    }

    /// <summary>
    /// Drawer that records base-group invocations.
    /// </summary>
    private sealed class BaseGroupDrawer : INestedDrawer<BaseGroup>
    {
        public static BaseGroup? LastDrawnGroup { get; private set; }

        public static void Reset() => LastDrawnGroup = null;

        public void Draw(BaseGroup group) => LastDrawnGroup = group;
    }

    /// <summary>
    /// Drawer that is type-compatible but cannot be created via Activator.CreateInstance().
    /// </summary>
    private sealed class CompatibleDrawerWithoutParameterlessConstructor : INestedDrawer<TestGroup>
    {
        public CompatibleDrawerWithoutParameterlessConstructor(int _) { }

        public void Draw(TestGroup group)
        {
        }
    }
}
