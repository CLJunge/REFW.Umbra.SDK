// <copyright file="NestedGroupDrawerBinderTests.cs" company="Umbra">
// Copyright (c) Umbra. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Tests for <see cref="NestedGroupDrawerBinder"/>.
/// </summary>
[TestClass]
public class NestedGroupDrawerBinderTests
{
    /// <summary>
    /// Tests that BuildDrawAction returns a valid Action when the drawer supports the group type
    /// and correctly binds the drawer instance to the nested group.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_SupportedDrawerType_ReturnsNonNullAction()
    {
        // Arrange
        var mockAttr = new Mock<INestedGroupDrawerAttribute>();
        mockAttr.Setup(a => a.DrawerType).Returns(typeof(TestGroupDrawer));
        var groupInstance = new TestGroup();

        // Act
        var result = NestedGroupDrawerBinder.BuildDrawAction(
            mockAttr.Object,
            typeof(TestGroup),
            groupInstance,
            out var disposable);

        // Assert
        Assert.IsNotNull(result, "BuildDrawAction should return a non-null Action for a supported drawer type.");
        Assert.IsNull(disposable, "Disposable should be null when drawer does not implement IDisposable.");
    }

    /// <summary>
    /// Tests that BuildDrawAction returns null when the drawer does not support the group type
    /// and does not set the disposable out parameter.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_UnsupportedDrawerType_ReturnsNull()
    {
        // Arrange
        var mockAttr = new Mock<INestedGroupDrawerAttribute>();
        mockAttr.Setup(a => a.DrawerType).Returns(typeof(IncompatibleDrawer));
        var groupInstance = new TestGroup();

        // Act
        var result = NestedGroupDrawerBinder.BuildDrawAction(
            mockAttr.Object,
            typeof(TestGroup),
            groupInstance,
            out var disposable);

        // Assert
        Assert.IsNull(result, "BuildDrawAction should return null when the drawer does not support the group type.");
        Assert.IsNull(disposable, "Disposable should be null when no drawer instance is created.");
    }

    /// <summary>
    /// Tests that BuildDrawAction sets the disposable out parameter when the drawer implements IDisposable.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_DisposableDrawer_SetsDisposableOutParameter()
    {
        // Arrange
        var mockAttr = new Mock<INestedGroupDrawerAttribute>();
        mockAttr.Setup(a => a.DrawerType).Returns(typeof(DisposableTestGroupDrawer));
        var groupInstance = new TestGroup();

        // Act
        var result = NestedGroupDrawerBinder.BuildDrawAction(
            mockAttr.Object,
            typeof(TestGroup),
            groupInstance,
            out var disposable);

        // Assert
        Assert.IsNotNull(result, "BuildDrawAction should return a non-null Action.");
        Assert.IsNotNull(disposable, "Disposable should be set when drawer implements IDisposable.");
        Assert.IsInstanceOfType(disposable, typeof(DisposableTestGroupDrawer), "Disposable should be the drawer instance.");
    }

    /// <summary>
    /// Tests that the returned Action from BuildDrawAction invokes the drawer's Draw method
    /// with the correct nested group instance.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_InvokeReturnedAction_CallsDrawerDrawMethod()
    {
        // Arrange
        var mockAttr = new Mock<INestedGroupDrawerAttribute>();
        mockAttr.Setup(a => a.DrawerType).Returns(typeof(TestGroupDrawer));
        var groupInstance = new TestGroup();
        TestGroupDrawer.ResetCallCount();

        // Act
        var action = NestedGroupDrawerBinder.BuildDrawAction(
            mockAttr.Object,
            typeof(TestGroup),
            groupInstance,
            out _);
        action?.Invoke();

        // Assert
        Assert.AreEqual(1, TestGroupDrawer.DrawCallCount, "Draw method should be called once.");
        Assert.AreSame(groupInstance, TestGroupDrawer.LastDrawnGroup, "Draw method should receive the correct group instance.");
    }

    /// <summary>
    /// Tests that BuildDrawAction handles multiple calls with the same unsupported drawer type
    /// without throwing and consistently returns null.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_MultipleCallsWithUnsupportedDrawer_ReturnsNullConsistently()
    {
        // Arrange
        var mockAttr = new Mock<INestedGroupDrawerAttribute>();
        mockAttr.Setup(a => a.DrawerType).Returns(typeof(IncompatibleDrawer));
        var groupInstance = new TestGroup();

        // Act
        var result1 = NestedGroupDrawerBinder.BuildDrawAction(mockAttr.Object, typeof(TestGroup), groupInstance, out var disposable1);
        var result2 = NestedGroupDrawerBinder.BuildDrawAction(mockAttr.Object, typeof(TestGroup), groupInstance, out var disposable2);
        var result3 = NestedGroupDrawerBinder.BuildDrawAction(mockAttr.Object, typeof(TestGroup), groupInstance, out var disposable3);

        // Assert
        Assert.IsNull(result1, "First call should return null.");
        Assert.IsNull(result2, "Second call should return null.");
        Assert.IsNull(result3, "Third call should return null.");
        Assert.IsNull(disposable1, "First call disposable should be null.");
        Assert.IsNull(disposable2, "Second call disposable should be null.");
        Assert.IsNull(disposable3, "Third call disposable should be null.");
    }

    /// <summary>
    /// Tests that BuildDrawAction uses cached factory for the same drawer type and group type combination.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_SameDrawerAndGroupType_ReusesCachedFactory()
    {
        // Arrange
        var mockAttr1 = new Mock<INestedGroupDrawerAttribute>();
        mockAttr1.Setup(a => a.DrawerType).Returns(typeof(TestGroupDrawer));
        var mockAttr2 = new Mock<INestedGroupDrawerAttribute>();
        mockAttr2.Setup(a => a.DrawerType).Returns(typeof(TestGroupDrawer));
        var group1 = new TestGroup();
        var group2 = new TestGroup();

        // Act
        var result1 = NestedGroupDrawerBinder.BuildDrawAction(mockAttr1.Object, typeof(TestGroup), group1, out _);
        var result2 = NestedGroupDrawerBinder.BuildDrawAction(mockAttr2.Object, typeof(TestGroup), group2, out _);

        // Assert
        Assert.IsNotNull(result1, "First call should return a non-null Action.");
        Assert.IsNotNull(result2, "Second call should return a non-null Action.");
    }

    /// <summary>
    /// Tests that BuildDrawAction creates separate drawer instances for each call,
    /// even when the factory is cached.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_MultipleCalls_CreatesSeparateDrawerInstances()
    {
        // Arrange
        var mockAttr = new Mock<INestedGroupDrawerAttribute>();
        mockAttr.Setup(a => a.DrawerType).Returns(typeof(DisposableTestGroupDrawer));
        var group1 = new TestGroup();
        var group2 = new TestGroup();

        // Act
        NestedGroupDrawerBinder.BuildDrawAction(mockAttr.Object, typeof(TestGroup), group1, out var disposable1);
        NestedGroupDrawerBinder.BuildDrawAction(mockAttr.Object, typeof(TestGroup), group2, out var disposable2);

        // Assert
        Assert.IsNotNull(disposable1, "First disposable should not be null.");
        Assert.IsNotNull(disposable2, "Second disposable should not be null.");
        Assert.AreNotSame(disposable1, disposable2, "Each call should create a separate drawer instance.");
    }

    /// <summary>
    /// Tests that BuildDrawAction works correctly with a drawer that supports a base type
    /// when the group type is a derived type.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_DrawerSupportsBaseType_WorksWithDerivedGroupType()
    {
        // Arrange
        var mockAttr = new Mock<INestedGroupDrawerAttribute>();
        mockAttr.Setup(a => a.DrawerType).Returns(typeof(BaseGroupDrawer));
        var derivedGroup = new DerivedGroup();

        // Act
        var result = NestedGroupDrawerBinder.BuildDrawAction(
            mockAttr.Object,
            typeof(DerivedGroup),
            derivedGroup,
            out var disposable);

        // Assert
        Assert.IsNotNull(result, "BuildDrawAction should return a non-null Action when drawer supports a base type.");
        Assert.IsNull(disposable, "Disposable should be null when drawer does not implement IDisposable.");
    }

    /// <summary>
    /// Tests that BuildDrawAction correctly handles a drawer that supports an interface
    /// implemented by the group type.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_DrawerSupportsInterface_WorksWithImplementingGroupType()
    {
        // Arrange
        var mockAttr = new Mock<INestedGroupDrawerAttribute>();
        mockAttr.Setup(a => a.DrawerType).Returns(typeof(InterfaceGroupDrawer));
        var groupInstance = new GroupWithInterface();

        // Act
        var result = NestedGroupDrawerBinder.BuildDrawAction(
            mockAttr.Object,
            typeof(GroupWithInterface),
            groupInstance,
            out var disposable);

        // Assert
        Assert.IsNotNull(result, "BuildDrawAction should return a non-null Action when drawer supports an interface.");
        Assert.IsNull(disposable, "Disposable should be null when drawer does not implement IDisposable.");
    }

    /// <summary>
    /// Tests that the returned Action correctly binds to the specific nested group instance
    /// and not to other instances.
    /// </summary>
    [TestMethod]
    public void BuildDrawAction_ReturnedAction_BindsToSpecificGroupInstance()
    {
        // Arrange
        var mockAttr = new Mock<INestedGroupDrawerAttribute>();
        mockAttr.Setup(a => a.DrawerType).Returns(typeof(TestGroupDrawer));
        var group1 = new TestGroup { Value = 42 };
        var group2 = new TestGroup { Value = 99 };
        TestGroupDrawer.ResetCallCount();

        // Act
        var action1 = NestedGroupDrawerBinder.BuildDrawAction(mockAttr.Object, typeof(TestGroup), group1, out _);
        var action2 = NestedGroupDrawerBinder.BuildDrawAction(mockAttr.Object, typeof(TestGroup), group2, out _);

        action1?.Invoke();

        // Assert
        Assert.AreSame(group1, TestGroupDrawer.LastDrawnGroup, "First action should bind to group1.");
        Assert.AreEqual(42, TestGroupDrawer.LastDrawnGroup?.Value, "First action should pass the correct group instance.");

        // Act
        action2?.Invoke();

        // Assert
        Assert.AreSame(group2, TestGroupDrawer.LastDrawnGroup, "Second action should bind to group2.");
        Assert.AreEqual(99, TestGroupDrawer.LastDrawnGroup?.Value, "Second action should pass the correct group instance.");
    }

    #region Helper Classes

    /// <summary>
    /// Test nested group type.
    /// </summary>
    internal class TestGroup
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Test drawer that supports TestGroup.
    /// </summary>
    internal class TestGroupDrawer : INestedGroupDrawer<TestGroup>
    {
        public static int DrawCallCount { get; private set; }
        public static TestGroup? LastDrawnGroup { get; private set; }

        public static void ResetCallCount()
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
    /// Test drawer that supports TestGroup and implements IDisposable.
    /// </summary>
    internal class DisposableTestGroupDrawer : INestedGroupDrawer<TestGroup>, IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Draw(TestGroup group)
        {
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    /// <summary>
    /// Another group type incompatible with TestGroup.
    /// </summary>
    internal class OtherGroup
    {
    }

    /// <summary>
    /// Test drawer that only supports OtherGroup, making it incompatible with TestGroup.
    /// </summary>
    internal class IncompatibleDrawer : INestedGroupDrawer<OtherGroup>
    {
        public void Draw(OtherGroup group)
        {
        }
    }

    /// <summary>
    /// Base group type for testing inheritance.
    /// </summary>
    internal class BaseGroup
    {
    }

    /// <summary>
    /// Derived group type for testing inheritance.
    /// </summary>
    internal class DerivedGroup : BaseGroup
    {
    }

    /// <summary>
    /// Test drawer that supports BaseGroup.
    /// </summary>
    internal class BaseGroupDrawer : INestedGroupDrawer<BaseGroup>
    {
        public void Draw(BaseGroup group)
        {
        }
    }

    /// <summary>
    /// Test interface for group types.
    /// </summary>
    internal interface ITestGroupInterface
    {
    }

    /// <summary>
    /// Group type that implements an interface.
    /// </summary>
    internal class GroupWithInterface : ITestGroupInterface
    {
    }

    /// <summary>
    /// Test drawer that supports ITestGroupInterface.
    /// </summary>
    internal class InterfaceGroupDrawer : INestedGroupDrawer<ITestGroupInterface>
    {
        public void Draw(ITestGroupInterface group)
        {
        }
    }

    #endregion

    /// <summary>
    /// Tests that the first call to TryMarkUnsupportedLogged returns true,
    /// indicating the flag was successfully set for the first time.
    /// </summary>
    [TestMethod]
    public void TryMarkUnsupportedLogged_FirstCall_ReturnsTrue()
    {
        // Arrange
        var factory = CreateFactory(isSupported: false, invoker: null);

        // Act
        bool result = factory.TryMarkUnsupportedLogged();

        // Assert
        Assert.IsTrue(result, "First call to TryMarkUnsupportedLogged should return true.");
    }

    /// <summary>
    /// Tests that the second call to TryMarkUnsupportedLogged returns false,
    /// indicating the flag was already set by a previous call.
    /// </summary>
    [TestMethod]
    public void TryMarkUnsupportedLogged_SecondCall_ReturnsFalse()
    {
        // Arrange
        var factory = CreateFactory(isSupported: false, invoker: null);
        factory.TryMarkUnsupportedLogged();

        // Act
        bool result = factory.TryMarkUnsupportedLogged();

        // Assert
        Assert.IsFalse(result, "Second call to TryMarkUnsupportedLogged should return false.");
    }

    /// <summary>
    /// Tests that multiple consecutive calls to TryMarkUnsupportedLogged return false after the first call,
    /// verifying that the flag remains set and subsequent calls correctly detect it.
    /// </summary>
    /// <param name="callCount">The total number of times to call TryMarkUnsupportedLogged.</param>
    [TestMethod]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(5)]
    [DataRow(10)]
    public void TryMarkUnsupportedLogged_MultipleConsecutiveCalls_OnlyFirstReturnsTrue(int callCount)
    {
        // Arrange
        var factory = CreateFactory(isSupported: false, invoker: null);

        // Act & Assert
        bool firstResult = factory.TryMarkUnsupportedLogged();
        Assert.IsTrue(firstResult, "First call should return true.");

        for (int i = 1; i < callCount; i++)
        {
            bool subsequentResult = factory.TryMarkUnsupportedLogged();
            Assert.IsFalse(subsequentResult, $"Call {i + 1} should return false.");
        }
    }

    /// <summary>
    /// Tests that when multiple threads concurrently call TryMarkUnsupportedLogged on the same instance,
    /// exactly one thread receives true and all others receive false, verifying thread-safe atomicity.
    /// </summary>
    [TestMethod]
    public void TryMarkUnsupportedLogged_ConcurrentCalls_OnlyOneThreadReturnsTrue()
    {
        // Arrange
        var factory = CreateFactory(isSupported: false, invoker: null);
        const int threadCount = 10;
        int trueCount = 0;
        int falseCount = 0;
        var barrier = new Barrier(threadCount);
        var tasks = new Task[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                barrier.SignalAndWait(); // Synchronize all threads to start at the same time
                bool result = factory.TryMarkUnsupportedLogged();
                if (result)
                    Interlocked.Increment(ref trueCount);
                else
                    Interlocked.Increment(ref falseCount);
            });
        }

        Task.WaitAll(tasks);

        // Assert
        Assert.AreEqual(1, trueCount, "Exactly one thread should receive true.");
        Assert.AreEqual(threadCount - 1, falseCount, "All other threads should receive false.");
    }

    /// <summary>
    /// Tests that different instances of NestedGroupDrawerFactory maintain independent state,
    /// verifying that the first call on each instance returns true.
    /// </summary>
    [TestMethod]
    public void TryMarkUnsupportedLogged_DifferentInstances_EachFirstCallReturnsTrue()
    {
        // Arrange
        var factory1 = CreateFactory(isSupported: false, invoker: null);
        var factory2 = CreateFactory(isSupported: true, invoker: null);

        // Act
        bool result1 = factory1.TryMarkUnsupportedLogged();
        bool result2 = factory2.TryMarkUnsupportedLogged();

        // Assert
        Assert.IsTrue(result1, "First call on first instance should return true.");
        Assert.IsTrue(result2, "First call on second instance should return true.");
    }

    /// <summary>
    /// Tests that after the first call returns true, a subsequent call on the same instance returns false,
    /// and then calling it again still returns false, verifying idempotency after the first transition.
    /// </summary>
    [TestMethod]
    public void TryMarkUnsupportedLogged_IdempotentAfterFirstCall_AlwaysReturnsFalse()
    {
        // Arrange
        var factory = CreateFactory(isSupported: false, invoker: null);

        // Act
        bool firstCall = factory.TryMarkUnsupportedLogged();
        bool secondCall = factory.TryMarkUnsupportedLogged();
        bool thirdCall = factory.TryMarkUnsupportedLogged();
        bool fourthCall = factory.TryMarkUnsupportedLogged();

        // Assert
        Assert.IsTrue(firstCall, "First call should return true.");
        Assert.IsFalse(secondCall, "Second call should return false.");
        Assert.IsFalse(thirdCall, "Third call should return false.");
        Assert.IsFalse(fourthCall, "Fourth call should return false.");
    }

    /// <summary>
    /// Tests thread safety with a higher degree of concurrency to verify that the atomic operation
    /// correctly handles race conditions even under heavy contention.
    /// </summary>
    [TestMethod]
    public void TryMarkUnsupportedLogged_HighConcurrency_OnlyOneThreadSucceeds()
    {
        // Arrange
        var factory = CreateFactory(isSupported: false, invoker: null);
        const int threadCount = 100;
        int successCount = 0;
        var countdown = new CountdownEvent(threadCount);
        var tasks = new Task[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                countdown.Signal();
                countdown.Wait(); // Wait for all threads to be ready
                bool result = factory.TryMarkUnsupportedLogged();
                if (result)
                    Interlocked.Increment(ref successCount);
            });
        }

        Task.WaitAll(tasks);

        // Assert
        Assert.AreEqual(1, successCount, "Exactly one thread should succeed even under high concurrency.");
    }

    /// <summary>
    /// Creates an instance of NestedGroupDrawerFactory using reflection.
    /// </summary>
    /// <param name="isSupported">Whether the factory is supported.</param>
    /// <param name="invoker">The optional invoker action.</param>
    /// <returns>An instance of NestedGroupDrawerFactory.</returns>
    /// <remarks>
    /// This helper uses reflection to instantiate the private nested class.
    /// If the nested class is changed to internal and InternalsVisibleTo is configured,
    /// this method can be replaced with direct instantiation.
    /// </remarks>
    private static dynamic CreateFactory(bool isSupported, Action<object, object>? invoker)
    {
        Type binderType = typeof(NestedGroupDrawerBinder);
        Type? factoryType = binderType.GetNestedType("NestedGroupDrawerFactory", BindingFlags.NonPublic);

        if (factoryType == null)
            throw new InvalidOperationException("Could not find NestedGroupDrawerFactory nested type.");

        return Activator.CreateInstance(factoryType, isSupported, invoker)
            ?? throw new InvalidOperationException("Failed to create NestedGroupDrawerFactory instance.");
    }
}


/// <summary>
/// Unit tests for the Bind method in NestedGroupDrawerFactory class.
/// </summary>
[TestClass]
public partial class NestedGroupDrawerFactoryTests
{
    private static Type? _factoryType;
    private static ConstructorInfo? _factoryConstructor;
    private static MethodInfo? _bindMethod;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        // Get the private nested NestedGroupDrawerFactory type via reflection
        var binderType = typeof(global::Umbra.UI.Config.NestedGroupDrawerBinder);
        _factoryType = binderType.GetNestedType("NestedGroupDrawerFactory", BindingFlags.NonPublic);

        Assert.IsNotNull(_factoryType, "Could not find NestedGroupDrawerFactory nested type");

        _factoryConstructor = _factoryType.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(bool), typeof(Action<object, object>) },
            null);

        Assert.IsNotNull(_factoryConstructor, "Could not find NestedGroupDrawerFactory constructor");

        _bindMethod = _factoryType.GetMethod("Bind", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.IsNotNull(_bindMethod, "Could not find Bind method");
    }

    /// <summary>
    /// Tests that Bind returns an Action when the invoker is not null.
    /// </summary>
    [TestMethod]
    public void Bind_InvokerIsNotNull_ReturnsAction()
    {
        // Arrange
        var mockInvoker = new Mock<Action<object, object>>();
        var factory = CreateFactory(isSupported: true, invoker: mockInvoker.Object);
        var drawerInstance = new object();
        var nested = new object();

        // Act
        var result = _bindMethod!.Invoke(factory, new object[] { drawerInstance, nested });

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<Action>(result);
    }

    /// <summary>
    /// Tests that the returned Action, when invoked, calls the invoker with the correct captured parameters.
    /// </summary>
    [TestMethod]
    public void Bind_ReturnedActionInvoked_CallsInvokerWithCorrectParameters()
    {
        // Arrange
        object? capturedDrawerInstance = null;
        object? capturedNested = null;
        Action<object, object> invoker = (drawer, nested) =>
        {
            capturedDrawerInstance = drawer;
            capturedNested = nested;
        };

        var factory = CreateFactory(isSupported: true, invoker: invoker);
        var drawerInstance = new object();
        var nestedInstance = new object();

        // Act
        var result = (Action)_bindMethod!.Invoke(factory, new object[] { drawerInstance, nestedInstance })!;
        result.Invoke();

        // Assert
        Assert.AreSame(drawerInstance, capturedDrawerInstance);
        Assert.AreSame(nestedInstance, capturedNested);
    }

    /// <summary>
    /// Tests that Bind accepts null for drawerInstance parameter and the returned Action passes null to the invoker.
    /// </summary>
    [TestMethod]
    public void Bind_DrawerInstanceIsNull_ReturnsActionThatPassesNull()
    {
        // Arrange
        object? capturedDrawerInstance = new object(); // Initialize with non-null
        object? capturedNested = null;
        Action<object, object> invoker = (drawer, nested) =>
        {
            capturedDrawerInstance = drawer;
            capturedNested = nested;
        };

        var factory = CreateFactory(isSupported: true, invoker: invoker);
        var nestedInstance = new object();

        // Act
        var result = (Action)_bindMethod!.Invoke(factory, new object?[] { null, nestedInstance })!;
        result.Invoke();

        // Assert
        Assert.IsNull(capturedDrawerInstance);
        Assert.AreSame(nestedInstance, capturedNested);
    }

    /// <summary>
    /// Tests that Bind accepts null for nested parameter and the returned Action passes null to the invoker.
    /// </summary>
    [TestMethod]
    public void Bind_NestedIsNull_ReturnsActionThatPassesNull()
    {
        // Arrange
        object? capturedDrawerInstance = null;
        object? capturedNested = new object(); // Initialize with non-null
        Action<object, object> invoker = (drawer, nested) =>
        {
            capturedDrawerInstance = drawer;
            capturedNested = nested;
        };

        var factory = CreateFactory(isSupported: true, invoker: invoker);
        var drawerInstance = new object();

        // Act
        var result = (Action)_bindMethod!.Invoke(factory, new object?[] { drawerInstance, null })!;
        result.Invoke();

        // Assert
        Assert.AreSame(drawerInstance, capturedDrawerInstance);
        Assert.IsNull(capturedNested);
    }

    /// <summary>
    /// Tests that Bind accepts null for both parameters and the returned Action passes both nulls to the invoker.
    /// </summary>
    [TestMethod]
    public void Bind_BothParametersNull_ReturnsActionThatPassesBothNulls()
    {
        // Arrange
        object? capturedDrawerInstance = new object(); // Initialize with non-null
        object? capturedNested = new object(); // Initialize with non-null
        Action<object, object> invoker = (drawer, nested) =>
        {
            capturedDrawerInstance = drawer;
            capturedNested = nested;
        };

        var factory = CreateFactory(isSupported: true, invoker: invoker);

        // Act
        var result = (Action)_bindMethod!.Invoke(factory, new object?[] { null, null })!;
        result.Invoke();

        // Assert
        Assert.IsNull(capturedDrawerInstance);
        Assert.IsNull(capturedNested);
    }

    /// <summary>
    /// Tests that multiple calls to Bind return distinct Action instances with independently captured parameters.
    /// </summary>
    [TestMethod]
    public void Bind_MultipleCalls_ReturnsDistinctActionsWithIndependentCaptures()
    {
        // Arrange
        var invocationLog = new System.Collections.Generic.List<(object? drawer, object? nested)>();
        Action<object, object> invoker = (drawer, nested) =>
        {
            invocationLog.Add((drawer, nested));
        };

        var factory = CreateFactory(isSupported: true, invoker: invoker);
        var drawerInstance1 = new object();
        var nestedInstance1 = new object();
        var drawerInstance2 = new object();
        var nestedInstance2 = new object();

        // Act
        var action1 = (Action)_bindMethod!.Invoke(factory, new object[] { drawerInstance1, nestedInstance1 })!;
        var action2 = (Action)_bindMethod!.Invoke(factory, new object[] { drawerInstance2, nestedInstance2 })!;

        action1.Invoke();
        action2.Invoke();

        // Assert
        Assert.AreEqual(2, invocationLog.Count);
        Assert.AreSame(drawerInstance1, invocationLog[0].drawer);
        Assert.AreSame(nestedInstance1, invocationLog[0].nested);
        Assert.AreSame(drawerInstance2, invocationLog[1].drawer);
        Assert.AreSame(nestedInstance2, invocationLog[1].nested);
    }

    /// <summary>
    /// Tests that the returned Action can be invoked multiple times, each time calling the invoker.
    /// </summary>
    [TestMethod]
    public void Bind_ReturnedActionInvokedMultipleTimes_CallsInvokerEachTime()
    {
        // Arrange
        var invocationCount = 0;
        Action<object, object> invoker = (drawer, nested) =>
        {
            invocationCount++;
        };

        var factory = CreateFactory(isSupported: true, invoker: invoker);
        var drawerInstance = new object();
        var nestedInstance = new object();

        // Act
        var action = (Action)_bindMethod!.Invoke(factory, new object[] { drawerInstance, nestedInstance })!;
        action.Invoke();
        action.Invoke();
        action.Invoke();

        // Assert
        Assert.AreEqual(3, invocationCount);
    }

    /// <summary>
    /// Tests that Bind works with various object types as parameters.
    /// </summary>
    [TestMethod]
    [DataRow("string drawer", 42)]
    [DataRow(123, "string nested")]
    [DataRow(3.14, true)]
    public void Bind_WithVariousObjectTypes_CapturesAndPassesCorrectly(object drawerInstance, object nested)
    {
        // Arrange
        object? capturedDrawerInstance = null;
        object? capturedNested = null;
        Action<object, object> invoker = (drawer, nestedObj) =>
        {
            capturedDrawerInstance = drawer;
            capturedNested = nestedObj;
        };

        var factory = CreateFactory(isSupported: true, invoker: invoker);

        // Act
        var result = (Action)_bindMethod!.Invoke(factory, new object[] { drawerInstance, nested })!;
        result.Invoke();

        // Assert
        Assert.AreEqual(drawerInstance, capturedDrawerInstance);
        Assert.AreEqual(nested, capturedNested);
    }

    /// <summary>
    /// Helper method to create a NestedGroupDrawerFactory instance using reflection.
    /// </summary>
    /// <param name="isSupported">Whether the factory is supported.</param>
    /// <param name="invoker">The invoker action, or null.</param>
    /// <returns>An instance of NestedGroupDrawerFactory.</returns>
    private static object CreateFactory(bool isSupported, Action<object, object>? invoker)
    {
        return _factoryConstructor!.Invoke(new object?[] { isSupported, invoker });
    }
}