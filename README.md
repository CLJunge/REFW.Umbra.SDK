# REFW.Umbra

`Umbra` is a support library for building managed `REFramework.NET` mods and plugins for RE Engine games. It provides reusable building blocks for typed settings, ImGui-based UI, plugin lifecycle hosting, logging, keyboard input, managed-object resolution, and runtime game detection inside the game process.

The repository contains three projects:

- `Umbra` - the reusable runtime/config/UI library
- `Umbra.SamplePlugin` - a reference plugin showing the recommended host, config, panel, deferred-save, and benchmark patterns
- `Umbra.UnitTests` - automated coverage for settings, UI composition, lifecycle helpers, logging, runtime helpers, and persistence behavior

## Features

- Attribute-driven settings registration with `SettingsStore<TConfig>` and `Parameter<T>`
- JSON persistence for `bool`, `int`, `float`, `double`, `string`, `enum`, nullable enum, and `Action`-backed button parameters where applicable to UI rendering
- Deferred auto-save with `DeferredSaveController<TConfig>`
- Pre-built ImGui settings UI with `ConfigDrawer<TConfig>`
- Panel composition with `PluginPanel`, `ConfigSection<TConfig>`, and `LiveStateSection<T>`
- Custom parameter drawers, two-column drawers, and nested-group drawers
- Per-plugin logging with `PluginLogger`
- Global SDK/runtime logging with `Logger`
- Keyboard capture helpers in `KeyboardInput`
- Managed object resolution with `ManagedObjectResolver.Resolve<T>` and `TryResolve<T>`
- Runtime game detection with `GameContext.CurrentGame`
- Supported-game identifiers and display names via `REGame` and `REGameExtensions`
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
| `Shutdown()` | Flush/dispose controllers, save/dispose store, dispose panels |
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
│  │  ├─ SettingsStore<TConfig>, SettingsPersistence, SettingsRegistrar
│  │  ├─ DeferredSaveController<TConfig>
│  │  └─ settings/UI metadata attributes
│  ├─ UI
│  │  ├─ Config
│  │  │  ├─ ConfigDrawer<TConfig>, ConfigSection<TConfig>
│  │  │  ├─ control builders, draw-tree composition, render context, nodes
│  │  │  └─ custom drawers and nested-group drawers
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
│  │  ├─ GameContext, GameMetadataLoader, REGame, REGameExtensions
│  │  ├─ ManagedObjectResolver, REFrameworkManagedObjectBridge
│  │  └─ IUmbraPlugin
│  ├─ UmbraPlugin
│  ├─ PluginHost<TPlugin>
│  ├─ PluginBootstrapper
│  ├─ PluginInstanceGuard
│  └─ PluginInstanceLease
├─ Umbra.SamplePlugin
│  └─ reference plugin showing nested config groups, custom drawers, deferred saving, panel usage, benchmarking, and game gating via GameContext
└─ Umbra.UnitTests
   └─ automated tests for settings, UI composition, lifecycle guards, runtime helpers, and logging
```

## Main flow

1. Define a config type with `[UmbraAutoRegister]` and `Parameter<T>` properties marked with `[UmbraParameter]`.
2. Load it with `SettingsStore<TConfig>.Load()`.
3. Optionally attach `DeferredSaveController<TConfig>` after load.
4. Render config through `ConfigDrawer<TConfig>` directly or through `ConfigSection<TConfig>` inside `PluginPanel`.
5. For live state, bind a state object to `LiveStateSection<T>` and declare its drawer with `[LiveStateSectionDrawer<TDrawer>]`.
6. Host the plugin instance through `PluginHost<TPlugin>` from a static REFramework entry-point class.
7. On unload, flush/dispose the save controller, save/dispose the store, and dispose panels/state holders.

### Notes on persistence and lifecycle

- `DeferredSaveController<TConfig>` must be constructed after `Load()`.
- `SettingsStore<TConfig>` exposes `IsLoaded` and `IsDisposed`.
- `Save()`, listener APIs, `ResetAll()`, and `CopyValuesTo(...)` require a loaded store.
- On unreadable JSON, `Load()` attempts a timestamped `.invalid-*.json` backup and restores defaults.
- If that backup cannot be created safely, the current session falls back to declared defaults and later `Save()` calls are suppressed to preserve the original file.
- Changing `[UmbraPrefix("...")]` changes persisted key names; existing JSON is not migrated automatically.

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
 dotnet test Umbra.UnitTests/Umbra.UnitTests.csproj
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
    [UmbraDisplayName("Hotkey")]
    public Parameter<int> Hotkey { get; set; } = new(574);
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
            .Add(new ConfigSection<MyConfig>(config, "MyPlugin.RuntimeConfigSection"));

        Log.Info("Loaded.");
    }

    public override void Shutdown()
    {
        _saveController?.Flush();
        _saveController?.Dispose();
        _saveController = null;

        _store?.Save();
        _store?.Dispose();
        _store = null;

        _panel?.Dispose();
        _panel = null;

        Log.Info("Unloaded.");
    }

    public override void OnPreImGuiDrawUI()
    {
        if (API.IsDrawingUI())
            _panel?.Draw();

        _saveController?.Tick();
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

For a fuller reference, see `Umbra.SamplePlugin`, which demonstrates nested config groups, hotkey drawers, buttons, enum controls, nested-group drawers, deferred saving, benchmark integration, and `GameContext`-based compatibility gating.

## Panel benchmarking

`Umbra.UI.Panel.Benchmark` is compiled only when `BENCHMARK` is defined.

`PluginPanelBenchmark` can measure one duplicate `PluginPanel.Draw()` call per frame and export CSV, JSON, and Markdown artifacts.

When the benchmark target is a config-backed panel, use `PluginPanelBenchmark.CreateForConfig(...)`:

```csharp
using REFrameworkNET;
using Umbra.Config;
using Umbra.Logging;
using Umbra.Runtime;
using Umbra.UI.Config;
using Umbra.UI.Panel;
using Umbra.UI.Panel.Benchmark;

public sealed class MyPlugin : UmbraPlugin
{
    private static readonly PluginLogger _log = new("MyPlugin");

    private PluginPanel? _panel;
    private PluginPanelBenchmark? _panelBenchmark;
    private SettingsStore<MyConfig>? _store;
    private DeferredSaveController<MyConfig>? _saveController;

    public MyPlugin() : base(_log) { }

    public override void Initialize()
    {
        var pluginDir = API.GetPluginDirectory(GetType().Assembly);
        var configPath = Path.Combine(pluginDir, "data", "MyPlugin", "config.json");
        var benchmarkDirectory = Path.Combine(pluginDir, "data", "MyPlugin", "artifacts", "perf", "runtime", "panel-draw");

        _store = new SettingsStore<MyConfig>(configPath);
        var config = _store.Load();
        _saveController = new DeferredSaveController<MyConfig>(_store);

        _panel = new PluginPanel("MyPlugin.RuntimePanel")
            .Add(new ConfigSection<MyConfig>(config, "MyPlugin.RuntimeConfigSection"));

        _panelBenchmark = PluginPanelBenchmark.CreateForConfig(
            "MyPlugin Panel Benchmark",
            config,
            "MyPlugin.BenchmarkPanel",
            benchmarkDirectory,
            sectionIdScope: "MyPlugin.BenchmarkConfigSection");
    }

    public override void Shutdown()
    {
        _panelBenchmark?.CompleteActiveRun("PluginUnload");
        _panelBenchmark?.Dispose();
        _panelBenchmark = null;

        _saveController?.Flush();
        _saveController?.Dispose();
        _saveController = null;

        _store?.Save();
        _store?.Dispose();
        _store = null;

        _panel?.Dispose();
        _panel = null;
    }

    public override void OnPreImGuiDrawUI()
    {
        if (API.IsDrawingUI())
        {
            if (_panelBenchmark is null || !_panelBenchmark.ShouldSuppressRuntimePanel)
                _panel?.Draw();

            _panelBenchmark?.DrawWindow();
        }

        _saveController?.Tick();
    }
}
```

Use the constructor overload instead when the benchmark target panel is assembled manually from custom `IPanelSection` instances and should remain caller-owned.

## Coding expectations

The current repository conventions emphasize:

- file-scoped namespaces where appropriate
- nullable reference types enabled
- small, dependency-light runtime code
- ImGui-based in-game UI only
- explicit loops and predicates instead of LINQ
- XML documentation kept in sync with behavior changes

## License

MIT. See `LICENSE.txt`.
