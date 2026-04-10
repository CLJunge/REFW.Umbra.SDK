using Umbra.UI.Toast;

namespace Umbra.UnitTests.UI.Toast;

[TestClass]
public sealed class PluginToastTests
{
    [TestInitialize]
    public void Setup() => ToastQueue.Clear();

    [TestCleanup]
    public void Cleanup() => ToastQueue.Clear();

    [TestMethod]
    public void Constructor_WithNullPluginName_Throws() => Assert.ThrowsExactly<ArgumentException>(() => new PluginToast(null!));

    [TestMethod]
    public void Constructor_WithEmptyPluginName_Throws() => Assert.ThrowsExactly<ArgumentException>(() => new PluginToast(""));

    [TestMethod]
    public void Constructor_WithWhitespacePluginName_Throws() => Assert.ThrowsExactly<ArgumentException>(() => new PluginToast("   "));

    [TestMethod]
    public void Constructor_WithValidPluginName_SetsPluginName()
    {
        var toast = new PluginToast("MyPlugin");
        Assert.AreEqual("MyPlugin", toast.PluginName);
    }

    [TestMethod]
    public void Constructor_WithDefaultDuration_SetsDefaultDuration()
    {
        var duration = TimeSpan.FromSeconds(5);
        var toast = new PluginToast("MyPlugin", duration);
        Assert.AreEqual(duration, toast.DefaultDuration);
    }

    [TestMethod]
    public void Constructor_WithoutDefaultDuration_DefaultDurationIsNull()
    {
        var toast = new PluginToast("MyPlugin");
        Assert.IsNull(toast.DefaultDuration);
    }

    [TestMethod]
    public void Push_DelegatesToToastQueueWithPrefix()
    {
        var toast = new PluginToast("MyPlugin");
        toast.Push("Hello");

        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual("[MyPlugin] Hello", entries[0].Message);
        Assert.AreEqual(ToastLevel.Info, entries[0].Level);
    }

    [TestMethod]
    public void Push_WithCustomLevel_SetsLevel()
    {
        var toast = new PluginToast("MyPlugin");
        toast.Push("Warning!", ToastLevel.Warning);

        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual(ToastLevel.Warning, entries[0].Level);
    }

    [TestMethod]
    public void Push_WithCustomDuration_UsesDuration()
    {
        var duration = TimeSpan.FromSeconds(10);
        var toast = new PluginToast("MyPlugin");
        toast.Push("Long toast", duration: duration);

        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual(duration, entries[0].Duration);
    }

    [TestMethod]
    public void Push_WithDefaultDuration_UsesDefaultWhenNoneProvided()
    {
        var defaultDuration = TimeSpan.FromSeconds(7);
        var toast = new PluginToast("MyPlugin", defaultDuration);
        toast.Push("Default duration");

        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual(defaultDuration, entries[0].Duration);
    }

    [TestMethod]
    public void Push_WithPerCallDuration_OverridesDefault()
    {
        var defaultDuration = TimeSpan.FromSeconds(7);
        var perCallDuration = TimeSpan.FromSeconds(2);
        var toast = new PluginToast("MyPlugin", defaultDuration);
        toast.Push("Override", duration: perCallDuration);

        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual(perCallDuration, entries[0].Duration);
    }

    [TestMethod]
    public void Info_PushesInfoLevel()
    {
        var toast = new PluginToast("MyPlugin");
        toast.Info("Info message");

        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual("[MyPlugin] Info message", entries[0].Message);
        Assert.AreEqual(ToastLevel.Info, entries[0].Level);
    }

    [TestMethod]
    public void Success_PushesSuccessLevel()
    {
        var toast = new PluginToast("MyPlugin");
        toast.Success("Success message");

        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual("[MyPlugin] Success message", entries[0].Message);
        Assert.AreEqual(ToastLevel.Success, entries[0].Level);
    }

    [TestMethod]
    public void Warning_PushesWarningLevel()
    {
        var toast = new PluginToast("MyPlugin");
        toast.Warning("Warning message");

        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual("[MyPlugin] Warning message", entries[0].Message);
        Assert.AreEqual(ToastLevel.Warning, entries[0].Level);
    }

    [TestMethod]
    public void Error_PushesErrorLevel()
    {
        var toast = new PluginToast("MyPlugin");
        toast.Error("Error message");

        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual("[MyPlugin] Error message", entries[0].Message);
        Assert.AreEqual(ToastLevel.Error, entries[0].Level);
    }

    [TestMethod]
    public void Info_WithDuration_UsesProvidedDuration()
    {
        var duration = TimeSpan.FromSeconds(5);
        var toast = new PluginToast("MyPlugin");
        toast.Info("With duration", duration);

        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual(duration, entries[0].Duration);
    }
}
