using System.Runtime.CompilerServices;
using Umbra.Logging;
using Umbra.Logging.UnitTests;

namespace Umbra.Runtime.UnitTests;

/// <summary>
/// Unit tests for <see cref="PluginInstanceGuard"/>.
/// </summary>
[TestClass]
public sealed class PluginInstanceGuardTests
{
    private TestLogSink _sink = null!;

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
    /// Installs an in-memory log sink and clears any active plugin leases before each test.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _sink = new TestLogSink();

        Logger.EnableAll();
        Logger.SetLogSink(_sink);
        PluginInstanceGuard.Reset();
    }

    /// <summary>
    /// Restores the default logging sink and clears any remaining plugin leases after each test.
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        PluginInstanceGuard.Reset();
        Logger.ResetLogSink();
        Logger.EnableAll();
    }

    /// <summary>
    /// Verifies that the first load of a plugin identity type acquires a lease successfully.
    /// </summary>
    [TestMethod]
    public void TryAcquire_FirstPluginType_ReturnsTrueAndProvidesLease()
    {
        // Act
        var result = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), out var lease);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(lease);
        Assert.AreSame(typeof(DefaultMutexPlugin), lease.PluginType);
        Assert.AreEqual(typeof(DefaultMutexPlugin).Assembly.GetName().Name, lease.MutexKey);

        lease.Dispose();
    }

    /// <summary>
    /// Verifies that the convenience overload infers the plugin type from the calling entry-point
    /// method and acquires the corresponding lease successfully.
    /// </summary>
    [TestMethod]
    public void TryAcquire_CallerInferenceOverload_ReturnsTrueAndProvidesLease()
    {
        // Act
        var result = InferredMutexPlugin.Load(out var lease);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(lease);
        Assert.AreSame(typeof(InferredMutexPlugin), lease.PluginType);
        Assert.AreEqual(typeof(InferredMutexPlugin).Assembly.GetName().Name, lease.MutexKey);

        lease.Dispose();
    }

    /// <summary>
    /// Verifies that a second load attempt for the same plugin is rejected and logged while the
    /// first lease remains active.
    /// </summary>
    [TestMethod]
    public void TryAcquire_DuplicateLoad_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var firstResult = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), out var firstLease);
        Assert.IsTrue(firstResult);
        Assert.IsNotNull(firstLease);

        // Act
        var secondResult = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), out var secondLease);

        // Assert
        Assert.IsFalse(secondResult);
        Assert.IsNull(secondLease);
        Assert.HasCount(1, _sink.WarningMessages);
        Assert.Contains("Skipped load for plugin", _sink.WarningMessages[0]);
        Assert.Contains(typeof(DefaultMutexPlugin).FullName!, _sink.WarningMessages[0]);

        firstLease.Dispose();
    }

    /// <summary>
    /// Verifies that disposing a lease releases the mutex key and allows the plugin to acquire it again.
    /// </summary>
    [TestMethod]
    public void Dispose_AfterAcquire_AllowsLaterReacquire()
    {
        // Arrange
        var firstResult = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), out var firstLease);
        Assert.IsTrue(firstResult);
        Assert.IsNotNull(firstLease);

        // Act
        firstLease.Dispose();
        var secondResult = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), out var secondLease);

        // Assert
        Assert.IsTrue(secondResult);
        Assert.IsNotNull(secondLease);
        Assert.AreEqual(firstLease.MutexKey, secondLease.MutexKey);

        secondLease.Dispose();
    }

    /// <summary>
    /// Verifies that different plugin identity types from the same assembly are mutually exclusive.
    /// </summary>
    [TestMethod]
    public void TryAcquire_DifferentPluginTypesInSameAssembly_RejectsSecondPluginUntilFirstReleases()
    {
        // Arrange
        var firstResult = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), out var firstLease);
        Assert.IsTrue(firstResult);
        Assert.IsNotNull(firstLease);

        // Act
        var secondResultWhileHeld = PluginInstanceGuard.TryAcquire(typeof(SameAssemblyPlugin), out var secondLeaseWhileHeld);
        firstLease.Dispose();
        var secondResultAfterRelease = PluginInstanceGuard.TryAcquire(typeof(SameAssemblyPlugin), out var secondLeaseAfterRelease);

        // Assert
        Assert.IsFalse(secondResultWhileHeld);
        Assert.IsNull(secondLeaseWhileHeld);
        Assert.IsTrue(secondResultAfterRelease);
        Assert.IsNotNull(secondLeaseAfterRelease);
        Assert.AreEqual(typeof(DefaultMutexPlugin).Assembly.GetName().Name, secondLeaseAfterRelease.MutexKey);

        secondLeaseAfterRelease.Dispose();
    }

    /// <summary>
    /// Verifies that <see cref="PluginInstanceGuard.Release(Type)"/> releases the lease when called
    /// with a different type from the same assembly that originally acquired it.
    /// </summary>
    [TestMethod]
    public void Release_WithDifferentTypeFromSameAssembly_ReleasesLeaseByMutexKey()
    {
        // Arrange — acquire with DefaultMutexPlugin
        var acquired = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), out var lease);
        Assert.IsTrue(acquired);
        Assert.IsNotNull(lease);

        // Act — release using a different type from the same assembly (same mutexKey)
        PluginInstanceGuard.Release(typeof(SameAssemblyPlugin));

        // Assert — the mutex key is now free; a fresh acquire succeeds
        var reacquired = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), out var newLease);
        Assert.IsTrue(reacquired, "Lease should have been released by the same-assembly type.");
        newLease?.Dispose();
    }

    /// <summary>
    /// Verifies that attempting to acquire a lease for a non-class type fails with a clear exception.
    /// </summary>
    [TestMethod]
    public void TryAcquire_PluginTypeIsNotClass_ThrowsInvalidOperationException()
    {
        // Act
        var exception = AssertThrows<InvalidOperationException>(() =>
            PluginInstanceGuard.TryAcquire(typeof(INonClassPlugin), out _));

        // Assert
        Assert.Contains("must be a class", exception.Message);
    }

    /// <summary>
    /// Verifies that the convenience overload fails with a clear exception when the calling plugin
    /// identity type is not a class.
    /// </summary>
    [TestMethod]
    public void TryAcquire_CallerInferenceOverloadWithStructDeclaringType_ThrowsInvalidOperationException()
    {
        // Act
        var exception = AssertThrows<InvalidOperationException>(() =>
            NonClassInferencePlugin.Load(out _));

        // Assert
        Assert.Contains("must be a class", exception.Message);
    }

    private static class DefaultMutexPlugin;

    private static class InferredMutexPlugin
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool Load(out PluginInstanceLease? lease)
            => PluginInstanceGuard.TryAcquire(out lease);
    }

    private static class SameAssemblyPlugin;

    private interface INonClassPlugin;

    private readonly struct NonClassInferencePlugin
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool Load(out PluginInstanceLease? lease)
            => PluginInstanceGuard.TryAcquire(out lease);
    }
}
