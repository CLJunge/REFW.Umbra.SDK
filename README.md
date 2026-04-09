# 🌑 REFW.Umbra

**Umbra** is a support library for building managed [REFramework.NET](https://github.com/praydog/REFramework) plugins for RE Engine games. It runs **inside the game process** and provides reusable building blocks for typed configuration, ImGui-based UI, plugin lifecycle hosting, logging, input capture, and runtime game detection.

| Project | Purpose |
|---|---|
| `Umbra` | Reusable runtime, config, logging, and UI library |
| `Umbra.SamplePlugin` | Reference plugin demonstrating recommended patterns |
| `Umbra.UnitTests` | Automated test coverage for all Umbra subsystems |

---

## ✨ Features

**Configuration**
- Attribute-driven typed config with `Parameter<T>` for `bool`, `int`, `float`, `double`, `string`, `enum`, and nullable enum
- JSON persistence via `ConfigStore<TConfig>` with automatic file recovery
- Event-driven auto-save via `ConfigSaveController` — instant saves for discrete changes, deferred saves during slider/drag interactions
- Validation attributes: `[UmbraRequired]`, `[UmbraMinLength]`, `[UmbraMaxLength]`, `[UmbraRegex]`, `[UmbraValidateWith<T>]` with inline UI feedback
- Versioned config import/export with schema validation
- Named presets — save, load, and delete named configuration snapshots

**UI**
- Panel composition with `PluginPanel`, `ConfigSection<TConfig>`, and `LiveStateSection<T>`
- Automatic config rendering from metadata via `ConfigDrawer<TConfig>` — one-time reflection, per-frame draw
- Built-in search, filter, and match navigation
- Per-section undo/redo stack with slider-aware coalescing, batch undo, and Ctrl+Z/Y keyboard shortcuts
- Toast notifications for undo and preset operations
- Built-in transfer UI for import/export
- Conditional visibility and disable with `[UmbraHideIf]` and `[UmbraDisableIf]`
- Custom drawers: `IParameterDrawer`, `ITwoColumnParameterDrawer`, `INestedDrawer<T>`

**Plugin System**
- `PluginHost<TPlugin>` for single-instance lifecycle management and safe callback dispatch
- `UmbraPlugin` base class with resilient `RunShutdownStep` sequencing
- Callback forwarding: `OnPreUpdateBehavior`, `OnPreImGuiDrawUI`, `OnPreImGuiRenderer`

**Supporting**
- Per-plugin `PluginLogger` and global `Logger` with level filtering
- `GameContext.CurrentGame` for RE Engine title detection
- `KeyboardInput` and `HotkeyBinding` for hardware-backed hotkey capture via `UmbraKey`
- Optional panel draw benchmarking when `BENCHMARK` is defined

---

## 🚀 Quick Start

### 1. Install dependencies

```powershell
.\scripts\setup_reframework_deps.ps1
```

### 2. Define a config type

```csharp
using Umbra.Config;
using Umbra.Config.Attributes;

[UmbraAutoRegister]
[UmbraPrefix("myPlugin")]
[UmbraRootNode("My Plugin")]
public record MyConfig
{
    [UmbraParameter]
    [UmbraDisplayName("Enabled")]
    public Parameter<bool> IsEnabled { get; set; } = new(true);

    [UmbraParameter]
    [UmbraDisplayName("Volume")]
    [UmbraRange(0, 100)]
    public Parameter<int> Volume { get; set; } = new(75);
}
```

### 3. Create the plugin instance

```csharp
using REFrameworkNET;
using Umbra;
using Umbra.Config;
using Umbra.Logging;
using Umbra.UI.Config;
using Umbra.UI.Config.Search;
using Umbra.UI.Panel;

public sealed class MyPlugin : UmbraPlugin
{
    private static readonly PluginLogger _log = new("MyPlugin");

    private PluginPanel? _panel;
    private ConfigStore<MyConfig>? _store;

    public MyPlugin() : base(_log) { }

    public override void Initialize()
    {
        var configPath = Path.Combine(
            API.GetPluginDirectory(GetType().Assembly), "data", "MyPlugin", "config.json");

        _store = new ConfigStore<MyConfig>(configPath);
        var config = _store.Load();

        // CreateWithStore wires auto-save, search, undo, and presets in one call.
        _panel = new PluginPanel("MyPlugin.Panel")
            .Add(ConfigSection<MyConfig>.CreateWithStore(
                config, _store,
                new ConfigDrawerOptions
                {
                    Search  = new ConfigSearchOptions(),
                    Undo    = new ConfigUndoOptions(),
                    Presets = new ConfigPresetOptions(),
                },
                "MyPlugin.Section"));

        Log.Info("Loaded.");
    }

    public override void Shutdown()
    {
        RunShutdownStep("dispose panel", () => { var p = _panel; _panel = null; p?.Dispose(); });
        RunShutdownStep("save store",    () => _store?.Save());
        RunShutdownStep("dispose store", () => { var s = _store; _store = null; s?.Dispose(); });
        Log.Info("Unloaded.");
    }

    public override void OnPreImGuiDrawUI()
    {
        if (API.IsDrawingUI()) _panel?.Draw();
    }
}
```

> **Note:** `ConfigSection.CreateWithStore()` creates and owns the `ConfigSaveController` internally — no manual tick or flush required. Disposing the panel disposes the save controller.

### 4. Create the static host

```csharp
using REFrameworkNET.Attributes;
using REFrameworkNET.Callbacks;
using Umbra;
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

### 5. Build

```bash
dotnet build REFW.Umbra.slnx -c Release
dotnet test Umbra.UnitTests/Umbra.UnitTests.csproj -c Release
```

---

## 📚 Documentation

Full guides, API walkthroughs, and architecture details are in the **[Umbra Wiki](https://docs.cljunge.com/refw-umbra/)**.

For a complete real-world reference, see `Umbra.SamplePlugin` — it demonstrates nested config groups, game gating, action binding, custom drawers, import/export, and resilient shutdown.
