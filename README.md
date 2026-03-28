# REFW.Umbra

A support library for building REFramework.NET mods and plugins for RE Engine games. `Umbra` provides a small set of reusable building blocks for typed settings, ImGui-based settings panels, live-state panels, logging, and input handling inside the game process.

## Overview

`Umbra` is intended for managed plugins that run under `REFramework.NET`, not standalone desktop applications. It helps plugin authors avoid repeating the same infrastructure for:

- strongly typed JSON-backed configuration
- ImGui settings rendering
- panel composition
- live state display
- safe plugin logging
- keyboard capture helpers

The repository also includes `Umbra.SamplePlugin`, which demonstrates the current configuration and panel workflow.

## Features

- Attribute-driven settings registration with `SettingsStore<TConfig>` and `Parameter<T>`
- JSON persistence for `bool`, `int`, `float`, `double`, `string`, and `enum` parameters
- Deferred auto-save with `DeferredSaveController<TConfig>`
- Pre-built ImGui settings UI with `ConfigDrawer<TConfig>`
- Panel composition with `PluginPanel`, `ConfigSection<TConfig>`, and `LiveStateSection<T>`
- Custom parameter drawers, two-column drawers, and nested-group drawers
- Per-plugin logging with `PluginLogger`
- Global SDK logging control with `Logger`
- Keyboard capture utilities in `KeyboardInput`
- Small runtime helper `ManagedObjectResolver` for resolving REFramework managed objects

### Default config drawers

- `Parameter<Action>` → button via `ButtonDrawer`
- `Parameter<bool>` → checkbox
- `Parameter<int>` → slider when `[UmbraRange]` is present, otherwise drag input
- `Parameter<float>` → slider when `[UmbraRange]` is present, otherwise drag input
- `Parameter<double>` → slider when `[UmbraRange]` is present, otherwise drag input
- `Parameter<string>` → single-line text input by default, multiline text input when `[UmbraMultiline]` is present
- `Parameter<TEnum>` → enum combo box
- Explicit `[UmbraCustomDrawer<TDrawer>]` and `[UmbraTwoColumnCustomDrawer<TDrawer>]` override the defaults

### Custom drawers

- `[UmbraCustomDrawer<TDrawer>]` uses an `IParameterDrawer` and gives the drawer full control over the entire parameter row
- `[UmbraTwoColumnCustomDrawer<TDrawer>]` uses an `ITwoColumnParameterDrawer` and keeps the standard two-column label layout while the drawer renders only the editing widget
- `[UmbraNestedGroupDrawer<TDrawer>]` uses an `INestedGroupDrawer<T>` and replaces the normal recursive rendering for an entire nested settings group
- Use a custom parameter drawer when you need a completely custom control layout, a two-column drawer when you want a custom widget that still aligns with normal settings rows, and a nested-group drawer when one drawer should own a whole section

## Architecture Summary

```text
REFW.Umbra
├─ Umbra
│  ├─ Config
│  │  ├─ Parameter<T>, IParameter, ParameterMetadata
│  │  ├─ SettingsStore<TConfig>, SettingsPersistence, SettingsRegistrar
│  │  ├─ DeferredSaveController<TConfig>
│  │  └─ Attributes for settings discovery and UI metadata
│  ├─ UI
│  │  ├─ Config
│  │  │  ├─ ConfigDrawer<TConfig>, ConfigSection<TConfig>
│  │  │  ├─ ControlFactory, draw-tree builder, nodes
│  │  │  └─ custom drawers and nested-group drawers
│  │  ├─ LiveState
│  │  │  ├─ LiveStateSection<T>
│  │  │  ├─ ILiveStateSectionDrawer<T>
│  │  │  └─ LiveStateSectionDrawerAttribute<TDrawer>
│  │  ├─ Panel
│  │  │  ├─ PluginPanel
│  │  │  └─ IPanelSection
│  │  └─ ImGuiWidgets
│  ├─ Logging
│  │  ├─ PluginLogger
│  │  ├─ Logger
│  │  └─ LogLevel
│  ├─ Input
│  │  └─ KeyboardInput
│  └─ Runtime
│     └─ ManagedObjectResolver
└─ Umbra.SamplePlugin
   └─ reference plugin showing settings, deferred save, nested groups, and custom drawers
```

### Main flow

1. Define a config type with `[UmbraAutoRegisterSettings]` and `Parameter<T>` properties marked with `[UmbraSettingsParameter]`.
2. Load it through `SettingsStore<TConfig>.Load()`.
3. Optionally attach `DeferredSaveController<TConfig>` after load.
4. Render it with `ConfigDrawer<TConfig>` directly or through `ConfigSection<TConfig>` inside `PluginPanel`.
5. For live read-only or hook-driven state, bind a state object to `LiveStateSection<T>` and declare its drawer with `[LiveStateSectionDrawer<TDrawer>]`.
6. On unload, flush/dispose the save controller, save/dispose the store, then dispose the panel.

## Setup Instructions

### Prerequisites

- `.NET 10` SDK
- Windows x64
- an RE Engine game with `REFramework` installed
- local REFramework API/game-binding dependencies under `dependencies/reframework`

### Install dependencies

From the repository root:

```powershell
.\scripts\setup_reframework_deps.ps1
```

This prepares the REFramework API references used by both projects and also sets up the deployment scripts that copy the output DLLs to the correct location under the game `reframework` directory.

### Build

```bash
dotnet build REFW.Umbra.slnx
```

In Debug builds, the repository uses the local deployment scripts configured in each project:

- `Umbra` uses `scripts\deploy_reframework_deps.bat`
- `Umbra.SamplePlugin` uses `scripts\deploy_reframework_plugin.bat`

## Usage Example

### Minimal config

```csharp
using Umbra.Config;
using Umbra.Config.Attributes;

[UmbraAutoRegisterSettings]
[UmbraSettingsPrefix("myPlugin")]
[UmbraCategory("My Plugin")]
public record MyConfig
{
    [UmbraSettingsParameter]
    [UmbraDisplayName("Enabled")]
    [UmbraDescription("Turns the plugin on or off.")]
    public Parameter<bool> IsEnabled { get; set; } = new(true);

    [UmbraSettingsParameter]
    [UmbraDisplayName("Hotkey")]
    public Parameter<int> Hotkey { get; set; } = new(574);
}
```

### Minimal plugin

```csharp
using REFrameworkNET;
using REFrameworkNET.Attributes;
using REFrameworkNET.Callbacks;
using Umbra.Config;
using Umbra.Logging;
using Umbra.UI.Panel;

public static class MyPlugin
{
    private static readonly PluginLogger _log = new("MyPlugin");

    private static PluginPanel? _panel;
    private static SettingsStore<MyConfig>? _store;
    private static DeferredSaveController<MyConfig>? _saveController;

    [PluginEntryPoint]
    public static void Load()
    {
        var configPath = Path.Combine(
            API.GetPluginDirectory(typeof(MyPlugin).Assembly),
            "data", "MyPlugin", "config.json");

        _store = new SettingsStore<MyConfig>(configPath);
        var config = _store.Load();
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
    public static void PreDrawUI()
    {
        if (API.IsDrawingUI())
            _panel?.Draw();

        _saveController?.Tick();
    }
}
```

For a fuller reference, see `Umbra.SamplePlugin`, which demonstrates nested groups, hotkey drawers, button drawers, custom nested-group drawers, and deferred saving.
