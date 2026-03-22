# Umbra.SDK

A support library for [REFramework](https://github.com/praydog/REFramework) mod and plugin development on RE Engine titles. It runs inside the game process through the REFramework.NET managed plugin host and provides configuration, settings UI, live-state rendering, logging, and input utilities so plugin authors can focus on game logic rather than infrastructure.

---

## Features

- **Attribute-driven configuration** — declare a `record` with `[AutoRegisterSettings]` and `Parameter<T>` properties; the store handles JSON load/save automatically.
- **Auto-save controller** — `DeferredSaveController<T>` coalesces rapid slider changes and writes to disk only after the user stops interacting, with immediate saves for boolean/string/enum changes.
- **Zero-per-frame-reflection settings UI** — `ConfigDrawer<T>` reflects over the config once at construction and walks a pre-built node list every frame. Supports categories, collapsible tree nodes, sliders, hotkey capture, custom drawers, nested groups, and conditional visibility.
- **Plugin panel system** — `PluginPanel` composes an ordered list of `IPanelSection` instances (`ConfigSection<T>`, `LiveSection<T>`) under a shared ImGui ID scope and owns their lifetimes.
- **Live game-state rendering** — `LiveSection<T>` pairs a hook-written state object with an `ILiveSectionDrawer<T>` declared via `[LiveSectionDrawerAttribute]`. Uses the swap-instance pattern for thread-safe multi-field updates.
- **Isolated per-plugin logging** — `PluginLogger` wraps `REFrameworkNET.API.Log*` with a per-instance prefix, log-level filter, and exception-safe formatted overloads.
- **Keyboard input helpers** — `KeyboardInput` captures ImGui keyboard keys, names them, and exposes modifier-state properties (`IsCtrlHeld`, `IsShiftHeld`, `IsAltHeld`).
- **ImGui widget helpers** — `ImGuiWidgets.DrawHelpMarker` and other stateless utilities for plugin UI.

---

## Architecture Summary

```
REFW.Umbra.SDK (solution)
├── Umbra.SDK                     # SDK library (ship this as a dependency)
│   ├── Config/                   # Parameter<T>, SettingsStore<T>, DeferredSaveController<T>
│   │   ├── Attributes/           # [AutoRegisterSettings], [SettingsParameter], [Range], [HideIf], …
│   │   └── UI/                   # ConfigDrawer<T>, ConfigDrawerBuilder, ControlFactory, parameter drawers
│   ├── UI/
│   │   ├── Panel/                # PluginPanel, ConfigSection<T>, LiveSection<T>, IPanelSection
│   │   └── ImGuiWidgets.cs       # Stateless ImGui helpers
│   ├── Logging/                  # PluginLogger, Logger (SDK-internal), LogLevel
│   └── Input/                    # KeyboardInput
└── Umbra.SamplePlugin            # Reference plugin implementation
    └── Config/                   # PluginConfig, nested settings groups, custom drawers
```

### Key flows

**Configuration lifecycle**
1. Construct `SettingsStore<TConfig>` with a file path.
2. Call `.Load()` — registers all `Parameter<T>` instances, loads values from JSON (or saves defaults).
3. Construct `DeferredSaveController<TConfig>` after `Load()` to enable auto-save.
4. Pass the config instance to `ConfigDrawer<T>` or `ConfigSection<T>` for UI.
5. On unload: `Flush()` → `Dispose()` the controller, then `Save()` → `Dispose()` the store.

**Panel + live state**
- The plugin owns a state object (e.g. `CameraState`).
- A static hook class holds a `volatile` reference to the state and writes it via `[MethodHook]` callbacks using the swap-instance pattern.
- `LiveSection<T>` resolves the `[LiveSectionDrawerAttribute]` on the state type and calls the drawer each frame — no per-frame reflection.
- `PluginPanel` renders all sections under a single `ImGui.PushID` scope to prevent widget ID collisions between plugins.

---

## Setup Instructions

### Prerequisites

- .NET 10 SDK (x64)
- [REFramework](https://github.com/praydog/REFramework) installed in an RE Engine game
- PowerShell 5.1+

### 1. Download REFramework API dependencies

Run the setup script from the solution root:

```powershell
.\scripts\setup_reframework_deps.ps1
```

This downloads the latest `csharp-api.zip` from REFramework-nightly, extracts the API DLLs to `dependencies\reframework\api\`, and copies generated game-binding assemblies to `dependencies\reframework\generated\`. You will be prompted to provide your game directory path; this is saved to `game_dir.local.txt` (gitignored).

### 2. Build

Open `REFW.Umbra.SDK.sln` in Visual Studio 2022 and build, or use the CLI:

```bash
dotnet build
```

In **Debug** configuration, a post-build event automatically deploys:
- `Umbra.SDK` → `<GameDir>\reframework\plugins\managed\dependencies\`
- Plugin assemblies → `<GameDir>\reframework\plugins\managed\`

---

## Usage Example

### Minimal plugin

```csharp
using REFrameworkNET.Attributes;
using REFrameworkNET.Callbacks;
using Umbra.SDK.Config;
using Umbra.SDK.Logging;
using Umbra.SDK.UI.Panel;

public static class MyPlugin
{
    private static readonly PluginLogger _log = new("MyPlugin");

    private static PluginPanel?               _panel;
    private static SettingsStore<MyConfig>?   _store;
    private static DeferredSaveController<MyConfig>? _saveController;

    [PluginEntryPoint]
    public static void Load()
    {
        var configPath = Path.Combine(
            API.GetPluginDirectory(typeof(MyPlugin).Assembly),
            "data", "MyPlugin", "config.json");

        _store          = new SettingsStore<MyConfig>(configPath);
        var config      = _store.Load();
        _saveController = new DeferredSaveController<MyConfig>(_store);

        _panel = new PluginPanel("MyPlugin")
            .Add(new ConfigSection<MyConfig>(config));

        _log.Info("Loaded.");
    }

    [PluginExitPoint]
    public static void Unload()
    {
        _saveController?.Flush();
        _saveController?.Dispose();
        _saveController = null;

        _store?.Save();
        _store?.Dispose();
        _store = null;

        _panel?.Dispose();
        _panel = null;
    }

    [Callback(typeof(ImGuiDrawUI), CallbackType.Pre)]
    public static void PreDraw()
    {
        if (API.IsDrawingUI()) _panel?.Draw();
        _saveController?.Tick();
    }
}
```

### Minimal config class

```csharp
using Umbra.SDK.Config;
using Umbra.SDK.Config.Attributes;

[AutoRegisterSettings, SettingsPrefix("myPlugin"), Category("My Plugin")]
public record MyConfig
{
    [SettingsParameter, DisplayName("Enabled"), Description("Toggle the plugin on or off.")]
    public Parameter<bool> IsEnabled { get; set; } = new(true);

    [SettingsParameter, DisplayName("Speed"), Range(0, 100), Step(1)]
    public Parameter<int> Speed { get; set; } = new(50);
}
```

See `Umbra.SamplePlugin` for a full example including nested config groups, hotkey capture, live-state sections, and custom parameter drawers.

---

## License

Copyright © 2026 Chris-Lennart Junge — MIT License
