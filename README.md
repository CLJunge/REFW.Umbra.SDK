# 🌑 REFW.Umbra

**Umbra** is a support library for building managed [REFramework.NET](https://github.com/praydog/REFramework) plugins for RE Engine games. It runs **inside the game process** and provides reusable building blocks for typed configuration, ImGui-based UI, plugin lifecycle hosting, logging, input capture, and runtime game detection.

| Project | Purpose |
|---|---|
| `Umbra` | Reusable runtime, config, logging, and UI library |
| `Umbra.SamplePlugin` | Reference plugin demonstrating recommended patterns |
| `Umbra.UnitTests` | Automated test coverage for all Umbra subsystems |

## ✨ Features

- ⚙️ **Attribute-driven config** — `ConfigStore<TConfig>` + `Parameter<T>` with JSON persistence for `bool`, `int`, `float`, `double`, `string`, `enum`, and nullable enum types
- 💾 **Deferred auto-save** — `DeferredSaveController<TConfig>` batches writes after edits
- 🖥️ **ImGui config UI** — `ConfigDrawer<TConfig>` with built-in search, filtering, and result navigation
- 🧩 **Panel composition** — `PluginPanel`, `ConfigSection<TConfig>`, `LiveStateSection<T>`
- ✅ **Validation** — `[UmbraRequired]`, `[UmbraMinLength]`, `[UmbraMaxLength]`, `[UmbraRegex]`, `[UmbraValidateWith<T>]` with inline feedback
- 📦 **Config import/export** — versioned exchange documents with schema validation and built-in transfer UI
- 🎨 **Custom drawers** — parameter drawers, two-column drawers, and nested-group drawers
- 👁️ **Conditional visibility** — hide/disable attributes with recursive propagation through nested groups
- 📝 **Logging** — per-plugin `PluginLogger` and global `Logger`
- 🎮 **Game detection** — `GameContext.CurrentGame` identifies the active RE Engine title
- 🛡️ **Resilient lifecycle** — best-effort shutdown sequencing for safe in-process cleanup
- ⏱️ **Benchmarking** — optional panel-draw timing utilities when `BENCHMARK` is defined

## 📚 Documentation

For detailed guides, API walkthroughs, and reference documentation, see the **[Umbra Wiki](https://docs.cljunge.com/refw-umbra/)**.

## 🏗️ Architecture

```text
Umbra
├─ Config        → Parameter<T>, ConfigStore, DeferredSaveController, attributes
├─ UI
│  ├─ Config     → ConfigDrawer, ConfigSection, search, custom drawers
│  ├─ LiveState  → LiveStateSection, drawer bindings
│  └─ Panel      → PluginPanel, section composition, optional benchmarking
├─ Logging       → PluginLogger, Logger, LogLevel
├─ Input         → KeyboardInput
├─ Runtime       → GameContext, REGame, ManagedObjectResolver
└─ Lifecycle     → UmbraPlugin, PluginHost<T>, PluginInstanceGuard
```

## 🚀 Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Windows x64
- An RE Engine game with [REFramework](https://github.com/praydog/REFramework) installed

### Setup

```powershell
# Download and stage REFramework dependencies
.\scripts\setup_reframework_deps.ps1
```

### Build & test

```bash
dotnet build REFW.Umbra.slnx -c Release
dotnet test Umbra.UnitTests/Umbra.UnitTests.csproj -c Release
```

### 🔧 Local deployment hooks

`Directory.Build.targets` provides optional Visual Studio hooks for developer workflows (`kill_re9.bat`, `deploy_reframework_deps.bat`, `deploy_reframework_plugin.bat`). These are gated by file existence and skipped on CI.

## 📝 Quick example

<details>
<summary>Minimal config + plugin + host</summary>

**Config:**

```csharp
using Umbra.Config;
using Umbra.Config.Attributes;

[UmbraAutoRegister]
[UmbraPrefix("myPlugin")]
[UmbraCategory("My Plugin")]
public record MyConfig
{
    [UmbraParameter, UmbraDisplayName("Enabled"), UmbraDescription("Turns the plugin on or off.")]
    public Parameter<bool> IsEnabled { get; set; } = new(true);

    [UmbraParameter, UmbraDisplayName("Profile Name"), UmbraRequired, UmbraMinLength(3), UmbraMaxLength(24)]
    public Parameter<string> ProfileName { get; set; } = new("UmbraUser");
}
```

**Plugin:**

```csharp
using REFrameworkNET;
using Umbra.Config;
using Umbra.Logging;
using Umbra.UI.Config;
using Umbra.UI.Panel;

public sealed class MyPlugin : UmbraPlugin
{
    private static readonly PluginLogger _log = new("MyPlugin");

    private PluginPanel? _panel;
    private ConfigStore<MyConfig>? _store;
    private DeferredSaveController<MyConfig>? _saveController;

    public MyPlugin() : base(_log) { }

    public override void Initialize()
    {
        var pluginDir = API.GetPluginDirectory(GetType().Assembly);
        var configPath = Path.Combine(pluginDir, "data", "MyPlugin", "config.json");

        _store = new ConfigStore<MyConfig>(configPath);
        var config = _store.Load();
        _saveController = new DeferredSaveController<MyConfig>(_store);

        _panel = new PluginPanel("MyPlugin.RuntimePanel")
            .Add(new ConfigSection<MyConfig>(
                config,
                new ConfigDrawerOptions { ShowSearchBar = true },
                "MyPlugin.RuntimeConfigSection"));

        Log.Info("Loaded.");
    }

    public override void Shutdown()
    {
        RunShutdownStep("dispose panel", () => { var p = _panel; _panel = null; p?.Dispose(); });
        RunShutdownStep("flush save controller", () => _saveController?.Flush());
        RunShutdownStep("dispose save controller", () => { var sc = _saveController; _saveController = null; sc?.Dispose(); });
        RunShutdownStep("save store", () => _store?.Save());
        RunShutdownStep("dispose store", () => { var s = _store; _store = null; s?.Dispose(); });
        Log.Info("Unloaded.");
    }

    public override void OnPreImGuiDrawUI()
    {
        if (API.IsDrawingUI()) _panel?.Draw();
        _saveController?.Tick();
    }
}
```

**Static host:**

```csharp
using REFrameworkNET.Attributes;
using REFrameworkNET.Callbacks;
using Umbra.Logging;
using Umbra.Runtime;

public static class MyPluginHost
{
    private static readonly PluginHost<MyPlugin> _host = new(static () => new MyPlugin());

    [PluginEntryPoint]
    public static void Load()
    {
        if (GameContext.CurrentGame == REGame.Unknown)
        {
            Logger.Warning("Unsupported game, skipping load.");
            return;
        }
        _host.Load();
    }

    [PluginExitPoint]
    public static void Unload() => _host.Unload();

    [Callback(typeof(UpdateBehavior), CallbackType.Pre)]
    public static void OnPreUpdateBehavior() => _host.OnPreUpdateBehavior();

    [Callback(typeof(ImGuiDrawUI), CallbackType.Pre)]
    public static void OnPreImGuiDrawUI() => _host.OnPreImGuiDrawUI();

    [Callback(typeof(ImGuiRender), CallbackType.Pre)]
    public static void OnPreImGuiRenderer() => _host.OnPreImGuiRenderer();
}
```

</details>

For a more complete reference, see `Umbra.SamplePlugin` which demonstrates nested config groups, custom drawers, validation, import/export, benchmarking, robust shutdown, and game-gated loading.

## 🎮 Supported games

RE2 · RE3 · RE4 · RE7 · RE8 · RE9 · DMC5 · SF6 · MH Rise · MH Wilds · MH Stories 3 · DD2 · Pragmata · Star Force
