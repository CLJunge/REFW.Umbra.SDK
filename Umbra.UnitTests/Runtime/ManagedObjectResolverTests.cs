namespace Umbra.Runtime.UnitTests;

/// <summary>
/// Unit tests for <see cref="ManagedObjectResolver"/>.
/// </summary>
[TestClass]
public sealed class ManagedObjectResolverTests
{
    private TestManagedObjectBridge _bridge = null!;

    /// <summary>
    /// Verifies that an action throws the expected exception type and returns the captured exception.
    /// </summary>
    private static TException AssertThrows<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        Assert.Fail($"Expected exception of type {typeof(TException).Name}.");
        throw new InvalidOperationException("Unreachable");
    }

    /// <summary>
    /// Installs a deterministic managed-object bridge before each test.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _bridge = new TestManagedObjectBridge();
        ManagedObjectResolver.SetBridge(_bridge);
    }

    /// <summary>
    /// Restores the default REFramework-backed bridge after each test.
    /// </summary>
    [TestCleanup]
    public void TestCleanup() => ManagedObjectResolver.ResetBridge();

    /// <summary>
    /// Verifies that <see cref="ManagedObjectResolver.TryResolve{T}(ulong, out T)"/> returns
    /// <see langword="false"/> and leaves the output value <see langword="null"/> for address
    /// zero without consulting the underlying bridge.
    /// </summary>
    [TestMethod]
    public void TryResolve_AddressIsZero_ReturnsFalseAndDoesNotInvokeBridge()
    {
        // Act
        var result = ManagedObjectResolver.TryResolve<object>(0, out var value);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(value);
        Assert.AreEqual(0, _bridge.InvocationCount);
    }

    /// <summary>
    /// Verifies that <see cref="ManagedObjectResolver.Resolve{T}(ulong)"/> returns
    /// <see langword="null"/> for address zero without consulting the underlying bridge.
    /// </summary>
    [TestMethod]
    public void Resolve_AddressIsZero_ReturnsNullAndDoesNotInvokeBridge()
    {
        // Act
        var value = ManagedObjectResolver.Resolve<object>(0);

        // Assert
        Assert.IsNull(value);
        Assert.AreEqual(0, _bridge.InvocationCount);
    }

    /// <summary>
    /// Verifies that <see cref="ManagedObjectResolver.TryResolve{T}(ulong, out T)"/> returns
    /// <see langword="true"/> and forwards the resolved value when the bridge succeeds.
    /// </summary>
    [TestMethod]
    public void TryResolve_BridgeReturnsValue_ReturnsTrueAndSetsValue()
    {
        // Arrange
        const ulong address = 0x1000;
        var expected = new Exception("resolved");
        _bridge.SetResult(address, expected);

        // Act
        var result = ManagedObjectResolver.TryResolve<Exception>(address, out var value);

        // Assert
        Assert.IsTrue(result);
        Assert.AreSame(expected, value);
        Assert.AreEqual(1, _bridge.InvocationCount);
    }

    /// <summary>
    /// Verifies that <see cref="ManagedObjectResolver.Resolve{T}(ulong)"/> returns the value
    /// provided by the bridge when resolution succeeds.
    /// </summary>
    [TestMethod]
    public void Resolve_BridgeReturnsValue_ReturnsResolvedInstance()
    {
        // Arrange
        const ulong address = 0x2000;
        var expected = new InvalidOperationException("resolved");
        _bridge.SetResult(address, expected);

        // Act
        var value = ManagedObjectResolver.Resolve<InvalidOperationException>(address);

        // Assert
        Assert.AreSame(expected, value);
        Assert.AreEqual(1, _bridge.InvocationCount);
    }

    /// <summary>
    /// Verifies that <see cref="ManagedObjectResolver.TryResolve{T}(ulong, out T)"/> returns
    /// <see langword="false"/> and sets the output value to <see langword="null"/> when the bridge
    /// reports an unresolved or incompatible object.
    /// </summary>
    [TestMethod]
    public void TryResolve_BridgeReturnsNull_ReturnsFalseAndSetsValueToNull()
    {
        // Arrange
        const ulong address = 0x3000;
        _bridge.SetResult<string>(address, null);

        // Act
        var result = ManagedObjectResolver.TryResolve<string>(address, out var value);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(value);
        Assert.AreEqual(1, _bridge.InvocationCount);
    }

    /// <summary>
    /// Verifies that <see cref="ManagedObjectResolver.Resolve{T}(ulong)"/> returns
    /// <see langword="null"/> when the bridge reports an unresolved or incompatible object.
    /// </summary>
    [TestMethod]
    public void Resolve_BridgeReturnsNull_ReturnsNull()
    {
        // Arrange
        const ulong address = 0x4000;
        _bridge.SetResult<object>(address, null);

        // Act
        var value = ManagedObjectResolver.Resolve<object>(address);

        // Assert
        Assert.IsNull(value);
        Assert.AreEqual(1, _bridge.InvocationCount);
    }

    /// <summary>
    /// Verifies that <see cref="ManagedObjectResolver.TryResolve{T}(ulong, out T)"/> swallows
    /// bridge exceptions and returns a simple failure result.
    /// </summary>
    [TestMethod]
    public void TryResolve_BridgeThrows_ReturnsFalseAndSetsValueToNull()
    {
        // Arrange
        const ulong address = 0x5000;
        _bridge.SetException<object>(address, new InvalidOperationException("host unavailable"));

        // Act
        var result = ManagedObjectResolver.TryResolve<object>(address, out var value);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(value);
        Assert.AreEqual(1, _bridge.InvocationCount);
    }

    /// <summary>
    /// Verifies that <see cref="ManagedObjectResolver.Resolve{T}(ulong)"/> returns
    /// <see langword="null"/> when the bridge throws.
    /// </summary>
    [TestMethod]
    public void Resolve_BridgeThrows_ReturnsNull()
    {
        // Arrange
        const ulong address = 0x6000;
        _bridge.SetException<object>(address, new InvalidOperationException("host unavailable"));

        // Act
        var value = ManagedObjectResolver.Resolve<object>(address);

        // Assert
        Assert.IsNull(value);
        Assert.AreEqual(1, _bridge.InvocationCount);
    }

    /// <summary>
    /// Verifies that different reference types can be resolved independently through the bridge.
    /// </summary>
    [TestMethod]
    public void Resolve_WithDifferentReferenceTypes_UsesTypeSpecificBridgeEntries()
    {
        // Arrange
        const ulong objectAddress = 0x7000;
        const ulong stringAddress = 0x7001;
        const ulong exceptionAddress = 0x7002;
        var expectedObject = new object();
        var expectedString = "resolved";
        var expectedException = new ArgumentException("resolved");

        _bridge.SetResult(objectAddress, expectedObject);
        _bridge.SetResult(stringAddress, expectedString);
        _bridge.SetResult(exceptionAddress, expectedException);

        // Act
        var objectValue = ManagedObjectResolver.Resolve<object>(objectAddress);
        var stringValue = ManagedObjectResolver.Resolve<string>(stringAddress);
        var exceptionValue = ManagedObjectResolver.Resolve<ArgumentException>(exceptionAddress);

        // Assert
        Assert.AreSame(expectedObject, objectValue);
        Assert.AreEqual(expectedString, stringValue);
        Assert.AreSame(expectedException, exceptionValue);
        Assert.AreEqual(3, _bridge.InvocationCount);
    }

    /// <summary>
    /// Verifies that <see cref="ManagedObjectResolver.SetBridge(IManagedObjectBridge)"/> rejects a null bridge.
    /// </summary>
    [TestMethod]
    public void SetBridge_Null_ThrowsArgumentNullException()
    {
        var exception = AssertThrows<ArgumentNullException>(() => ManagedObjectResolver.SetBridge(null!));

        Assert.AreEqual("bridge", exception.ParamName);
    }
}
