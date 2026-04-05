# REFW.Umbra

`Umbra` is a support library for building managed `REFramework.NET` mods and plugins for RE Engine games. It provides reusable building blocks for typed settings, ImGui-based UI, plugin lifecycle hosting, logging, keyboard input, managed-object resolution, and runtime game detection inside the game process.

The repository contains three projects:

- `Umbra` - the reusable runtime/config/UI library
- `Umbra.SamplePlugin` - a reference plugin showing the recommended host, config, panel, deferred-save, benchmark, and robust-shutdown patterns
- `Umbra.UnitTests` - automated coverage for settings, UI composition, lifecycle helpers, logging, runtime helpers, and persistence behavior

## Features

- Attribute-driven settings registration with `SettingsStore<TConfig>` and `Parameter<T>`
- JSON persistence for `bool`, `int`, `float`, `double`, `string`, `enum`, and nullable enum parameters
- Config import/export via `SettingsStore<TConfig>.Import(...)` and `Export(...)`, with versioned exchange documents and legacy flat-file import support
- Deferred auto-save with `DeferredSaveController<TConfig>`
- Pre-built ImGui settings UI with `ConfigDrawer<TConfig>`
- Built-in config search/filter UI with visible-match filtering, highlight styling, and previous/next result navigation in `ConfigDrawer<TConfig>` and `ConfigSection<TConfig>`
- Panel composition with `PluginPanel`, `ConfigSection<TConfig>`, and `LiveStateSection<T>`
- Validation attributes with inline feedback for rejected edits in `ConfigDrawer<TConfig>`
- Custom parameter drawers, two-column drawers, and nested-group drawers
- Per-plugin logging with `PluginLogger`
- Global SDK/runtime logging with `Logger`
- Keyboard capture helpers in `KeyboardInput`
- Managed object resolution with `ManagedObjectResolver.Resolve<T>` and `TryResolve<T>`
- Runtime game detection with `GameContext.CurrentGame`
- Supported-game identifiers and display names via `REGame` and `REGameExtensions`
- Best-effort shutdown sequencing for plugin unload cleanup
- Optional panel benchmarking utilities in `Umbra.UI.Panel.Benchmark` when `BENCHMARK` is defined

## Runtime and plugin model

`Umbra` is intended for plugins that run inside the managed `REFramework.NET` host, not standalone desktop applications.

The recommended pattern is:

1. Keep plugin state on an instance type derived from `UmbraPlugin`.
2. Expose static REFramework entry points and callbacks on a separate host class.
3. Let `PluginHost<TPlugin>` own instance creation, callback dispatch, and single-instance coordination.
4. Use `PluginLogger` on the plugin instance and `Logger` only for shared runtime/bootstrap diagnostics.

### Plugin lifecycle callbacks

`UmbraPlugin` exposes five overridable lifecycle methods:

| Override | Typical use |
|---|---|
| `Initialize()` | Load config, create state, build panels |
| `Shutdown()` | Run best-effort teardown steps: flush/dispose controllers, save/dispose stores, dispose panels, and log any cleanup failures |
| `OnPreUpdateBehavior()` | Per-frame gameplay or polling logic |
| `OnPreImGuiDrawUI()` | Draw settings/status UI and tick deferred saves |
| `OnPreImGuiRenderer()` | Draw overlay UI during the renderer pass |

## Game detection with `GameContext`

`Umbra.Runtime.GameContext` detects the active RE Engine title for the current process by matching the process name against embedded `game-metadata.json` entries.

- Use `GameContext.CurrentGame` to branch plugin behavior by game.
- When no match is found, it returns `REGame.Unknown`.
- `REGameExtensions.GetDisplayName()` provides user-facing names for supported non-`Unknown` values.

Current `REGame` values in the codebase:

- `RE2`
- `RE3`
- `RE4`
- `RE7`
- `RE8`
- `RE9`
- `DMC5`
- `SF6`
- `MHRISE`
- `MHWILDS`
- `MHSTORIES3`
- `DD2`
- `PRAGMATA`
- `STARFORCE`

`Umbra.SamplePlugin` currently demonstrates this by only loading when `GameContext.CurrentGame == REGame.RE9`.

## Default config drawers

- `Parameter<Action>` → button via `ButtonDrawer`
- `Parameter<bool>` → checkbox
- `Parameter<int>` → slider when `[UmbraRange]` is present, otherwise drag input
- `Parameter<float>` → slider when `[UmbraRange]` is present, otherwise drag input
- `Parameter<double>` → slider when `[UmbraRange]` is present, otherwise drag input
- `Parameter<string>` → single-line text input by default, multiline text input when `[UmbraMultiline]` is present
- `Parameter<TEnum>` → enum combo box
- `Parameter<TEnum?>` → enum combo box with a `<None>` option for `null`
- Explicit `[UmbraDrawer<TDrawer>]` and `[UmbraTwoColumnDrawer<TDrawer>]` override the defaults

## Search and filtering

When `ConfigDrawerOptions.ShowSearchBar` is enabled, the built-in config UI adds a search row with:

- a visible `Search` label
- a filter input that narrows the rendered config tree to matching parameters
- match highlighting for visible search hits
- previous/next navigation buttons that move the focused result
- automatic branch expansion and scroll-into-view for the explicitly selected result

Query changes do not auto-focus results. Focus moves only when the user navigates with the previous/next controls.

## Validation attributes

- `[UmbraRequired]` rejects `null` and empty strings
- `[UmbraRequired(AllowWhitespace = true)]` still rejects `null` and empty strings, but allows whitespace-only strings
- `[UmbraMinLength(n)]` enforces an inclusive minimum string length
- `[UmbraMaxLength(n)]` limits built-in string input capacity
- `[UmbraRegex("pattern")]` enforces a regular-expression match for string values
- `[UmbraValidateWith<TValidator>]` runs a custom `IParameterValidator`

Validation failures on the non-throwing UI path do not crash the plugin or overwrite the last valid value. The attempted edit is rejected and the current validation message is rendered inline beneath the affected built-in string or numeric control.

## Config import and export

`SettingsStore<TConfig>.Export(string)` writes a versioned exchange document with this shape:

```json
{
  "formatVersion": 1,
  "schemaId": "Fully.Qualified.Config.Type",
  "schemaVersion": 1,
  "values": {
    "myPlugin.someKey": 123,
    "myPlugin.otherKey": "value"
  }
}
```

- `formatVersion` identifies the exchange-document format.
- `schemaId` defaults to `typeof(TConfig).FullName`.
- `schemaVersion` defaults to `1`, or to `[UmbraConfigVersion(n)]` when the root config type declares it.
- `values` contains only persisted registered parameters; delegate-backed button parameters are skipped.

`SettingsStore<TConfig>.Import(string, SettingsImportOptions?)` accepts two document shapes:

1. the versioned envelope shown above
2. the legacy flat JSON dictionary used by Umbra's normal runtime persistence

Import compatibility rules:

- envelope imports reject unsupported `formatVersion` values
- envelope imports reject mismatched `schemaId` values
- envelope imports reject `schemaVersion` values newer than the current config schema
- only keys that exist in the current loaded store are considered
- unknown keys are ignored, not treated as fatal errors
- imported values are applied through the existing `Parameter<T>` validation pipeline, so rejected values keep the last valid in-memory state
- when `SettingsImportOptions.SaveAfterImport` is `true`, Umbra saves the accepted final state once through the normal store persistence path

Config transfer UI is now an optional built-in Umbra feature rather than a plugin-defined nested config group. Enable it through `ConfigDrawerOptions.Transfer` and create the section through `ConfigSection<TConfig>.CreateWithStore(config, store, options, ...)`. The built-in control renders in its own tree node, with a configurable header and configurable placement before or after the normal config nodes. Inside that tree node it uses one shared config-file path field, exposes an explicit browse menu for import or export file selection, shows a second row of equal-width `Import` and `Export` action buttons, and can optionally draw a trailing separator below those actions. The transfer path is persisted in a separate sidecar file derived from the main settings-store file path, so transfer UI state stays decoupled from the actual configuration payload.

## Custom drawers and sections

- `[UmbraDrawer<TDrawer>]` uses an `IParameterDrawer`
- `[UmbraTwoColumnDrawer<TDrawer>]` uses an `ITwoColumnParameterDrawer`
- `[UmbraNestedDrawer<TDrawer>]` uses an `INestedDrawer<T>` for a whole nested settings group
- `[LiveStateSectionDrawer<TDrawer>]` binds a live-state type to its panel drawer
- `PluginPanel` is the recommended top-level UI surface when a plugin needs settings and live state together

## Architecture summary

```text
REFW.Umbra
├─ Umbra
│  ├─ Config
│  │  ├─ Parameter<T>, IParameter, ParameterMetadata
│  │  ├─ SettingsStore<TConfig>, SettingsStorePersistenceCoordinator<TConfig>, SettingsRegistrar
│  │  ├─ DeferredSaveController<TConfig>
│  │  └─ settings/UI metadata attributes
│  ├─ UI
│  │  ├─ Config
│  │  │  ├─ ConfigDrawer<TConfig>, ConfigSection<TConfig>
│  │  │  ├─ control builders, draw-tree composition, render context, nodes
│  │  │  └─ custom drawers, nested-group drawers, and built-in search state/indexing
│  │  ├─ LiveState
│  │  │  ├─ LiveStateSection<T>
│  │  │  ├─ ILiveStateSectionDrawer<T>
│  │  │  └─ LiveStateSectionDrawerAttribute<TDrawer>
│  │  ├─ Panel
│  │  │  ├─ PluginPanel
│  │  │  ├─ IPanelSection
│  │  │  └─ optional benchmark support in BENCHMARK builds
│  │  └─ ImGuiWidgets
│  ├─ Logging
│  │  ├─ PluginLogger
│  │  ├─ Logger
│  │  └─ LogLevel
│  ├─ Input
│  │  └─ KeyboardInput
│  ├─ Runtime
│  │  │  ├─ GameContext, GameMetadataLoader, REGame, REGameExtensions
│  │  │  ├─ ManagedObjectResolver, REFrameworkManagedObjectBridge
│  │  │  └─ IUmbraPlugin
│  ├─ UmbraPlugin
│  ├─ PluginHost<TPlugin>
│  ├─ PluginBootstrapper
│  ├─ PluginInstanceGuard
│  └─ PluginInstanceLease
├─ Umbra.SamplePlugin
│  └─ reference plugin showing nested config groups, custom drawers, deferred saving, panel usage, benchmarking, robust shutdown, and game gating via GameContext
└─ Umbra.UnitTests
   └─ automated tests for settings, UI composition, lifecycle guards, runtime helpers, and logging
```

## Main flow

1. Define a config type with `[UmbraAutoRegister]` and `Parameter<T>` properties marked with `[UmbraParameter]`.
2. Load it with `SettingsStore<TConfig>.Load()`.
3. Optionally attach `DeferredSaveController<TConfig>` after load.
4. Render config through `ConfigDrawer<TConfig>` directly or through `ConfigSection<TConfig>` inside `PluginPanel`.
5. Enable `ConfigDrawerOptions.ShowSearchBar` when the surface should expose built-in filtering and result navigation.
6. For live state, bind a state object to `LiveStateSection<T>` and declare its drawer with `[LiveStateSectionDrawer<TDrawer>]`.
7. Host the plugin instance through `PluginHost<TPlugin>` from a static REFramework entry-point class.
8. On unload, run cleanup in isolated best-effort steps so one failure does not block later flush, save, or dispose work.

### Notes on persistence and lifecycle

- `DeferredSaveController<TConfig>` must be constructed after `Load()`.
- `SettingsStore<TConfig>` exposes `IsLoaded` and `IsDisposed`.
- `Save()`, listener APIs, `ResetAll()`, and `CopyValuesTo(...)` require a loaded store.
- `Parameter<T>.Value` remains non-throwing for UI-driven edits; invalid values are rejected and preserve the last valid value.
- On unreadable JSON, `Load()` attempts a timestamped `.invalid-*.json` backup and restores defaults.
- If that backup cannot be created safely, the current session falls back to declared defaults and later `Save()` calls are suppressed to preserve the original file.
- Changing `[UmbraPrefix("...")]` changes persisted key names; existing JSON is not migrated automatically.
- Because plugins run inside the game process, shutdown should prefer resilient cleanup over fail-fast teardown.
- `Umbra.SamplePlugin` demonstrates a robust shutdown pattern that wraps each unload step, logs failures with `PluginLogger.Exception(...)`, and continues cleaning up the remaining resources.

## Getting started

### Prerequisites

- `.NET 10` SDK
- Windows x64
- an RE Engine game with `REFramework` installed
- local REFramework dependencies under `dependencies/reframework`

### Setup REFramework dependencies

From the repository root:

```powershell
.\scripts\setup_reframework_deps.ps1
```

or

```bat
.\scripts\setup_reframework_deps.bat
```

The setup script downloads the latest REFramework nightly C# API package, stages the required API DLLs under `dependencies/reframework`, and can optionally prepare local game-directory deployment files and generated bindings.

### Build

```bash
 dotnet build REFW.Umbra.slnx
```

### Test

```bash
 dotnet test Umbra.UnitTests/Umbra.UnitTests.csproj -c Release
```

### Local Visual Studio deployment hooks

`Directory.Build.targets` enables developer-only local hooks when building inside Visual Studio:

- `scripts\kill_re9.bat` can terminate the game before build so locked DLLs can be replaced
- `scripts\deploy_reframework_deps.bat` is used by `Umbra`
- `scripts\deploy_reframework_plugin.bat` is used by `Umbra.SamplePlugin`

These hooks are gated by file existence and are skipped on CI or machines without the local scripts.

## Usage example

### Minimal config

```csharp
using Umbra.Config;
using Umbra.Config.Attributes;

[UmbraAutoRegister]
[UmbraPrefix("myPlugin")]
[UmbraCategory("My Plugin")]
public record MyConfig
{
    [UmbraParameter]
    [UmbraDisplayName("Enabled")]
    [UmbraDescription("Turns the plugin on or off.")]
    public Parameter<bool> IsEnabled { get; set; } = new(true);

    [UmbraParameter]
    [UmbraDisplayName("Profile Name")]
    [UmbraRequired]
    [UmbraMinLength(3)]
    [UmbraMaxLength(24)]
    public Parameter<string> ProfileName { get; set; } = new("UmbraUser");
}
```

### Minimal plugin and static host

```csharp
using REFrameworkNET;
using REFrameworkNET.Attributes;
using REFrameworkNET.Callbacks;
using Umbra.Config;
using Umbra.Logging;
using Umbra.Runtime;
using Umbra.UI.Config;
using Umbra.UI.Panel;

public sealed class MyPlugin : UmbraPlugin
{
    private static readonly PluginLogger _log = new("MyPlugin");

    private PluginPanel? _panel;
    private SettingsStore<MyConfig>? _store;
    private DeferredSaveController<MyConfig>? _saveController;

    public MyPlugin() : base(_log) { }

    public override void Initialize()
    {
        var pluginDir = API.GetPluginDirectory(GetType().Assembly);
        var configPath = Path.Combine(pluginDir, "data", "MyPlugin", "config.json");

        _store = new SettingsStore<MyConfig>(configPath);
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
        RunShutdownStep("dispose runtime panel", DisposeRuntimePanel);
        RunShutdownStep("flush deferred save controller", FlushDeferredSaveController);
        RunShutdownStep("dispose deferred save controller", DisposeDeferredSaveController);
        RunShutdownStep("save settings store", SaveSettingsStore);
        RunShutdownStep("dispose settings store", DisposeSettingsStore);

        Log.Info("Unloaded.");
    }

    public override void OnPreImGuiDrawUI()
    {
        if (API.IsDrawingUI())
            _panel?.Draw();

        _saveController?.Tick();
    }

    private void DisposeRuntimePanel()
    {
        var panel = _panel;
        _panel = null;
        panel?.Dispose();
    }

    private void FlushDeferredSaveController()
        => _saveController?.Flush();

    private void DisposeDeferredSaveController()
    {
        var saveController = _saveController;
        _saveController = null;
        saveController?.Dispose();
    }

    private void SaveSettingsStore()
        => _store?.Save();

    private void DisposeSettingsStore()
    {
        var store = _store;
        _store = null;
        store?.Dispose();
    }

    private void RunShutdownStep(string stepName, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Log.Exception(ex, "Shutdown step failed: {0}.", stepName);
        }
    }
}

public static class MyPluginHost
{
    private static readonly PluginHost<MyPlugin> _host = new(
        static () => new MyPlugin());

    [PluginEntryPoint]
    public static void Load()
    {
        if (GameContext.CurrentGame == REGame.Unknown)
        {
            Logger.Warning("Unsupported or undetected game, skipping load.");
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

For a fuller reference, see `Umbra.SamplePlugin`, which demonstrates nested config groups, hotkey drawers, buttons, enum controls, nested-group drawers, deferred saving, validation attributes with inline feedback, benchmark integration, robust shutdown, and `GameContext`-based compatibility gating.

## Panel benchmarking

`Umbra.UI.Panel.Benchmark` is compiled only when the `BENCHMARK` symbol is defined and provides optional timing helpers for panel rendering.
