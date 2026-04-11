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
- Event-driven auto-save via `ConfigSaveController` — instant saves for discrete changes, deferred saves during slider/drag and text input interactions
- Validation attributes: `[UmbraRequired]`, `[UmbraMinLength]`, `[UmbraMaxLength]`, `[UmbraRegex]`, `[UmbraValidateWith<T>]` with inline UI feedback
- Versioned config import/export with schema validation

**UI**
- Panel composition with `PluginPanel`, `ConfigSection<TConfig>`, and `LiveStateSection<T>`
- `PluginPanelFactory` for single-call panel creation with managed lifecycle (`ManagedPluginPanel<TConfig>`)
- Two-tier factory overloads: simple (bool flags) and custom (`ConfigDrawerOptions`) for progressive customization
- Automatic config rendering from metadata via `ConfigDrawer<TConfig>` — one-time reflection, per-frame draw
- Built-in search, filter, and match navigation
- Per-section undo/redo stack with slider-aware and text-input-aware coalescing, batch undo, and Ctrl+Z/Y keyboard shortcuts
- Plugin-scoped toast notifications via `PluginToast`, with optional undo/redo integration
- Built-in transfer UI for import/export
- Conditional visibility and disable with `[UmbraHideIf]` and `[UmbraDisableIf]`
- Custom drawers: `IParameterDrawer`, `ITwoColumnParameterDrawer`, `INestedDrawer<T>`

**Plugin System**
- `PluginHost<TPlugin>` for single-instance lifecycle management and safe callback dispatch
- `PluginHost<TPlugin>.Current` static accessor for `[MethodHook]` and static callback forwarding
- Plugin metadata: `PluginName`, `PluginVersion`, `PluginAuthor` on `IUmbraPlugin` with sensible defaults via `UmbraPlugin`
- Lifecycle state tracking: `PluginState`, `LastError`, `LoadedAt` on each host instance
- `PluginStatus` snapshot combining metadata and runtime state via `GetStatus()`
- `PluginRegistry` for runtime discovery of all registered plugin hosts
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
using Umbra.Logging;
using Umbra.UI.Panel;

public sealed class MyPlugin : UmbraPlugin
{
    private static readonly PluginLogger _log = new("MyPlugin");

    private ManagedPluginPanel<MyConfig>? _managedPanel;

    public MyPlugin() : base(_log) { }

    public override void Initialize()
    {
        var configPath = Path.Combine(
            API.GetPluginDirectory(GetType().Assembly), "data", "MyPlugin", "config.json");

        _managedPanel = PluginPanelFactory.Create<MyConfig>(
            configPath,
            "MyPlugin.Panel");

        Log.Info("Loaded.");
    }

    public override void Shutdown()
    {
        RunShutdownStep("dispose managed panel", () =>
        {
            var p = _managedPanel; _managedPanel = null; p?.Dispose();
        });
        Log.Info("Unloaded.");
    }

    public override void OnPreImGuiDrawUI()
    {
        if (API.IsDrawingUI()) _managedPanel?.Draw();
    }
}
```

> **Note:** `PluginPanelFactory.Create` handles store creation, config loading, section wiring (including `ConfigSaveController`), and panel construction in one call. `ManagedPluginPanel.Dispose()` disposes the panel, saves, and disposes the store — no manual multi-step shutdown needed. For custom option settings, pass a `ConfigDrawerOptions` instead of relying on the boolean-flag defaults.

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

**`Umbra.SamplePlugin`** demonstrates nested config groups, game gating, action binding, custom drawers, import/export, toast notifications, and resilient shutdown.
