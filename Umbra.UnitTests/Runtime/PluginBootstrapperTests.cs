using Umbra.Logging;
using Umbra.Logging.UnitTests;

namespace Umbra.Runtime.UnitTests;

/// <summary>
/// Unit tests for <see cref="PluginBootstrapper"/>.
/// </summary>
[TestClass]
public sealed class PluginBootstrapperTests
{
    private TestLogSink _sink = null!;

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
    /// Verifies that load initialization runs under the mutex and does not require manual lease handling.
    /// </summary>
    [TestMethod]
    public void Load_FirstDecoratedPlugin_RunsInitializationAndKeepsInstanceActive()
    {
        // Arrange
        var initialized = false;

        // Act
        var result = PluginBootstrapper.Load(typeof(BootstrapPlugin), () => initialized = true);

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(initialized);

        // The active mutex should block a second load until unload runs.
        var secondResult = PluginBootstrapper.Load(typeof(BootstrapPlugin), () => initialized = false);
        Assert.IsFalse(secondResult);
        Assert.IsTrue(initialized);
        Assert.HasCount(1, _sink.WarningMessages);
        Assert.Contains("Skipped load for plugin", _sink.WarningMessages[0]);
    }

    /// <summary>
    /// Verifies that unload runs cleanup and releases the mutex even when cleanup succeeds.
    /// </summary>
    [TestMethod]
    public void Unload_AfterLoad_ReleasesMutexAndAllowsReload()
    {
        // Arrange
        var cleanupRan = false;
        Assert.IsTrue(PluginBootstrapper.Load(typeof(BootstrapPlugin), () => { }));

        // Act
        PluginBootstrapper.Unload(typeof(BootstrapPlugin), () => cleanupRan = true);
        var reloaded = PluginBootstrapper.Load(typeof(BootstrapPlugin), () => { });

        // Assert
        Assert.IsTrue(cleanupRan);
        Assert.IsTrue(reloaded);
    }

    /// <summary>
    /// Verifies that unload releases the mutex even if cleanup throws.
    /// </summary>
    [TestMethod]
    public void Unload_CleanupThrows_StillReleasesMutex()
    {
        // Arrange
        Assert.IsTrue(PluginBootstrapper.Load(typeof(BootstrapPlugin), () => { }));

        // Act
        var exception = AssertThrows<InvalidOperationException>(() =>
            PluginBootstrapper.Unload(typeof(BootstrapPlugin), () => throw new InvalidOperationException("boom")));

        var reloaded = PluginBootstrapper.Load(typeof(BootstrapPlugin), () => { });

        // Assert
        Assert.AreEqual("boom", exception.Message);
        Assert.IsTrue(reloaded);
    }

    /// <summary>
    /// Verifies that a load failure releases the mutex so a later load can succeed.
    /// </summary>
    [TestMethod]
    public void Load_InitializationThrows_ReleasesMutex()
    {
        // Act
        var exception = AssertThrows<InvalidOperationException>(() =>
            PluginBootstrapper.Load(typeof(BootstrapPlugin), () => throw new InvalidOperationException("load failed")));

        var reloaded = PluginBootstrapper.Load(typeof(BootstrapPlugin), () => { });

        // Assert
        Assert.AreEqual("load failed", exception.Message);
        Assert.IsTrue(reloaded);
    }

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

    [UmbraPlugin]
    private static class BootstrapPlugin;
}
