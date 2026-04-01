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
    private PluginLogger _log = null!;

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
        _log = new PluginLogger("PluginInstanceGuardTests");

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
    /// Verifies that the first load of a decorated plugin acquires a lease successfully.
    /// </summary>
    [TestMethod]
    public void TryAcquire_FirstDecoratedPlugin_ReturnsTrueAndProvidesLease()
    {
        // Act
        var result = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), _log, out var lease);

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
        var result = InferredMutexPlugin.Load(_log, out var lease);

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
        var firstResult = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), _log, out var firstLease);
        Assert.IsTrue(firstResult);
        Assert.IsNotNull(firstLease);

        // Act
        var secondResult = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), _log, out var secondLease);

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
        var firstResult = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), _log, out var firstLease);
        Assert.IsTrue(firstResult);
        Assert.IsNotNull(firstLease);

        // Act
        firstLease.Dispose();
        var secondResult = PluginInstanceGuard.TryAcquire(typeof(DefaultMutexPlugin), _log, out var secondLease);

        // Assert
        Assert.IsTrue(secondResult);
        Assert.IsNotNull(secondLease);
        Assert.AreEqual(firstLease.MutexKey, secondLease.MutexKey);

        secondLease.Dispose();
    }

    /// <summary>
    /// Verifies that different plugin types sharing the same explicit mutex key are mutually exclusive.
    /// </summary>
    [TestMethod]
    public void TryAcquire_SharedExplicitMutexKey_RejectsSecondPluginUntilFirstReleases()
    {
        // Arrange
        var firstResult = PluginInstanceGuard.TryAcquire(typeof(SharedMutexPluginA), _log, out var firstLease);
        Assert.IsTrue(firstResult);
        Assert.IsNotNull(firstLease);

        // Act
        var secondResultWhileHeld = PluginInstanceGuard.TryAcquire(typeof(SharedMutexPluginB), _log, out var secondLeaseWhileHeld);
        firstLease.Dispose();
        var secondResultAfterRelease = PluginInstanceGuard.TryAcquire(typeof(SharedMutexPluginB), _log, out var secondLeaseAfterRelease);

        // Assert
        Assert.IsFalse(secondResultWhileHeld);
        Assert.IsNull(secondLeaseWhileHeld);
        Assert.IsTrue(secondResultAfterRelease);
        Assert.IsNotNull(secondLeaseAfterRelease);
        Assert.AreEqual("shared-group", secondLeaseAfterRelease.MutexKey);

        secondLeaseAfterRelease.Dispose();
    }

    /// <summary>
    /// Verifies that attempting to acquire a lease for a type without <see cref="UmbraPluginAttribute"/>
    /// fails with a clear exception.
    /// </summary>
    [TestMethod]
    public void TryAcquire_PluginIsMissingUmbraPluginAttribute_ThrowsInvalidOperationException()
    {
        // Act
        var exception = AssertThrows<InvalidOperationException>(() =>
            PluginInstanceGuard.TryAcquire(typeof(MissingAttributePlugin), _log, out _));

        // Assert
        Assert.Contains("[UmbraPlugin]", exception.Message);
    }

    /// <summary>
    /// Verifies that the convenience overload fails with a clear exception when the calling plugin
    /// type is not decorated with <see cref="UmbraPluginAttribute"/>.
    /// </summary>
    [TestMethod]
    public void TryAcquire_CallerInferenceOverloadWithoutUmbraPluginAttribute_ThrowsInvalidOperationException()
    {
        // Act
        var exception = AssertThrows<InvalidOperationException>(() =>
            MissingAttributeInferencePlugin.Load(_log, out _));

        // Assert
        Assert.Contains("[UmbraPlugin]", exception.Message);
    }

    /// <summary>
    /// Verifies that an explicitly empty or whitespace mutex key is rejected.
    /// </summary>
    [TestMethod]
    public void TryAcquire_ExplicitWhitespaceMutexKey_ThrowsInvalidOperationException()
    {
        // Act
        var exception = AssertThrows<InvalidOperationException>(() =>
            PluginInstanceGuard.TryAcquire(typeof(WhitespaceMutexPlugin), _log, out _));

        // Assert
        Assert.Contains("empty or whitespace mutex key", exception.Message);
    }

    [UmbraPlugin]
    private static class DefaultMutexPlugin;

    [UmbraPlugin]
    private static class InferredMutexPlugin
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool Load(PluginLogger log, out PluginInstanceLease? lease)
            => PluginInstanceGuard.TryAcquire(log, out lease);
    }

    [UmbraPlugin("shared-group")]
    private static class SharedMutexPluginA;

    [UmbraPlugin("shared-group")]
    private static class SharedMutexPluginB;

    private static class MissingAttributePlugin;

    private static class MissingAttributeInferencePlugin
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool Load(PluginLogger log, out PluginInstanceLease? lease)
            => PluginInstanceGuard.TryAcquire(log, out lease);
    }

    [UmbraPlugin("   ")]
    private static class WhitespaceMutexPlugin;
}
