# 🎮 REFW.Umbra

> A support library and example plugin for **RE Engine** game modding via [REFramework.NET](https://github.com/praydog/REFramework).
> Runs inside the game process through the REFramework managed plugin environment — **.NET 10 / x64**.
> Depends on three host assemblies distributed with REFramework: `REFramework.NET`, `Hexa.NET.ImGui`, and `HexaGen.Runtime`.

---

## 📦 Solution Structure

| Project | Description |
|---|---|
| `Umbra.SDK` | Reusable support library: logging, configuration, ImGui helpers, keyboard utilities. |
| `Umbra.SamplePlugin` | Example plugin demonstrating `Umbra.SDK` features: settings, config UI, panel sections, custom drawers, and plugin lifecycle. |

---

## 🛠️ Development Setup

### Prerequisites

- **.NET 10 SDK** (x64)
- **REFramework** installed in a supported RE Engine game directory

### Fetching dependencies

Run the setup script once before opening the solution. It downloads the latest
[REFramework-nightly](https://github.com/praydog/REFramework-nightly) `csharp-api.zip`,
extracts the three DLLs the projects reference, and optionally copies them into your game
directory and stages generated game-binding assemblies.

```powershell
.\scripts\setup_reframework_deps.ps1
```

Or use the wrapper batch file:

```cmd
scripts\setup_reframework_deps.bat
```

After setup, `dependencies\reframework\api\` will contain exactly:

| File | Used by |
|---|---|
| `REFramework.NET.dll` | Core plugin API — `[PluginEntryPoint]`, `API.Log*`, hooks |
| `Hexa.NET.ImGui.dll` | ImGui bindings — all UI and input code |
| `HexaGen.Runtime.dll` | Runtime support for ImGui bindings |

All other assemblies distributed with REFramework (`AssemblyGenerator`, `REFCoreDeps`,
`Microsoft.CodeAnalysis.*`) are host-internal and are **not** referenced by any project
in this solution.

### Local Debug deployment

Debug builds deploy automatically after a successful build via `Directory.Build.targets`.
Each project declares its own deployment target through the `$(UmbraDeployScript)` property:

| Project | Deploys to |
|---|---|
| `Umbra.SDK` | `reframework\plugins\managed\dependencies\` |
| `Umbra.SamplePlugin` | `reframework\plugins\managed\` |

Deployment is skipped silently when `game_dir.local.txt` is absent (e.g. on CI) or the
batch file does not exist.

---

## 🔧 Umbra.SDK

### Logging — `Umbra.SDK.Logging`

Two exception-safe wrappers around `REFrameworkNET.API.Log*`. All methods silently suppress
errors to avoid disrupting the game process. Formatted overloads (`...(string format, params object[] args)`)
also swallow exceptions thrown by `string.Format` — invalid format strings or mismatched arguments
cause the message to be silently discarded rather than propagated.

#### `PluginLogger` — per-plugin instance (recommended for all plugins)

All managed plugins load into the **same AppDomain**. Using shared static state for a prefix or log
level causes each plugin that loads to silently overwrite every earlier plugin's configuration.
`PluginLogger` solves this by keeping all configuration in a per-plugin instance.

Declare it as a `private static readonly` field and initialise inline — never in the entry point:

```csharp
// Declare once — no entry-point side-effects on shared state.
private static readonly PluginLogger _log = new("MyPlugin");

_log.Info("Plugin loaded");
_log.Info("Version {0}.{1}", major, minor);
_log.Warning("Unexpected state");
_log.Error("Something failed");

// Always use Exception() — not Error() — when logging exceptions:
_log.Exception(ex, "Failed to initialize camera");
_log.Exception(ex, "Failed to initialize {0}", "camera"); // formatted overload
```

| Member | Forwards to | Notes |
|---|---|---|
| `Info(string)` / `Info(string, params object[])` | `API.LogInfo` | |
| `Warning(string)` / `Warning(string, params object[])` | `API.LogWarning` | |
| `Error(string)` / `Error(string, params object[])` | `API.LogError` | |
| `Exception(Exception, string)` | `API.LogError` | Logs exception type, message, and stack trace |
| `Exception(Exception, string, params object[])` | `API.LogError` | Formatted context message overload |
| `Prefix` | — | Optional string prepended as `[Prefix] message` |
| `PrefixFormat` | — | Composite format; `{0}` → `Prefix`. Defaults to `"[{0}]"` |
| `MinLevel` | — | Messages below this `LogLevel` are silently discarded. Defaults to `LogLevel.Info` |

`LogLevel` values: `Info = 0`, `Warning = 1`, `Error = 2`, `None = 3` (silences all output).

#### `Logger` — static, raw forwarding facade

Carries **no configuration** and forwards messages unconditionally. Intended for SDK-internal use
only. Prefer `PluginLogger` in all plugin code.

---

### ⚙️ Configuration — `Umbra.SDK.Config`

A reflection-based, attribute-driven settings system backed by JSON persistence
(`System.Text.Json`, camelCase property names, enums serialized as strings).
The draw tree for the UI is built once at construction — no per-frame reflection.

#### Quick-Start Flow

1. Annotate a config class with SDK attributes.
2. Create a `SettingsStore<TConfig>` with the path to the JSON file.
3. Call `Load()` to get a fully populated config instance. **This must happen before step 4.**
4. Optionally wrap the store in a `DeferredSaveController<TConfig>` for automatic change-triggered saves.
5. Pass the config instance to `ConfigDrawer<TConfig>` (or `ConfigSection<TConfig>`) to render a settings panel.
6. Call `Draw()` and `Tick()` each frame from a `[Callback(typeof(ImGuiDrawUI), CallbackType.Pre)]` static method.
7. On plugin unload: `Flush()` + `Dispose()` the controller, then `Save()` + `Dispose()` the store.

```csharp
[PluginEntryPoint]
public static void Load()
{
    var configPath  = Path.Combine(API.GetPluginDirectory(typeof(MyPlugin).Assembly), "config.json");

    _store          = new SettingsStore<PluginConfig>(configPath);
    var config      = _store.Load();                                // Load FIRST
    _saveController = new DeferredSaveController<PluginConfig>(_store); // THEN construct

    _panel = new PluginPanel("MyPlugin")
        .Add(new ConfigSection<PluginConfig>(config));
}

[Callback(typeof(ImGuiDrawUI), CallbackType.Pre)]
public static void PreDrawUI()
{
    if (API.IsDrawingUI())
        _panel?.Draw();        // render UI on the game's draw thread each frame

    _saveController?.Tick();   // evaluate pending saves; lightweight no-op when idle
}

[PluginExitPoint]
public static void Unload()
{
    _saveController?.Flush();   // write any pending debounced change
    _saveController?.Dispose(); // remove listeners before the store is disposed
    _saveController = null;

    _store?.Save();             // final save
    _store?.Dispose();
    _store = null;

    _panel?.Dispose();
    _panel = null;
}
```

---

#### `Parameter<T>`

The core value container. Supported types for JSON persistence: `bool`, `int`, `float`, `double`,
`string`, and any `enum`. Use `Parameter<Action>` for button controls — the stored action is not
persisted to JSON.

```csharp
public Parameter<float> FieldOfView { get; set; } = new(55f);

// Read
float fov = config.FieldOfView.Value;
float fovImplicit = config.FieldOfView; // implicit conversion

// Write (raises ValueChanged)
config.FieldOfView.Value = 70f;
config.FieldOfView.Set(70f); // alias; avoids .Value.Value inside nested group drawers

// Write silently (no event)
config.FieldOfView.SetWithoutNotify(70f);

// Reset to default
config.FieldOfView.Reset();
```

> `Parameter<T>` enforces `[Range]` bounds at assignment time — out-of-range values are silently rejected.

---

#### `SettingsStore<TConfig>`

| Member | Description |
|---|---|
| `Load()` | Creates a `TConfig` instance, registers parameters, and loads values from disk. Saves defaults if no file exists. **Call this before constructing `DeferredSaveController`.** |
| `Save()` | Serializes all parameter values to the configured JSON file. |
| `ResetAll()` | Resets every registered parameter to its default value. Delegate-typed parameters (e.g. button actions) are skipped. |
| `CopyValuesTo(target, setWithoutNotifying)` | Mirrors all values into another store instance, optionally suppressing change events. |
| `AddListenerToAll(Action)` | Subscribes an untyped callback to `ValueChanged` on **every** parameter. Auto-removed on `Dispose`. |
| `AddListenerToAll<T>(Action<T?,T?>)` | Subscribes a typed callback to `ValueChanged` on every `Parameter<T>` whose value type matches `T`. Auto-removed on `Dispose`. ⚠️ See caveat below. |
| `AddListenerToAll(Func<IParameter,bool>, Action)` | Subscribes to `ValueChanged` on parameters matching the predicate. Preferred over the typed overload when filtering by `ValueType`. Auto-removed on `Dispose`. |
| `RemoveListenerFromAll(Action)` | Manually unsubscribes an untyped listener. |
| `RemoveListenerFromAll<T>(Action<T?,T?>)` | Manually unsubscribes a typed listener. ⚠️ See caveat below. |
| `RemoveListenerFromAll(Func<IParameter,bool>, Action)` | Manually unsubscribes a predicate-filtered listener. |
| `Dispose()` | Removes all event subscriptions registered via `Add*`. Always call after `Save()`. |

> **⚠️ `AddListenerToAll<T>` type-inference caveat**
>
> For unconstrained `T`, passing an `Action<int?,int?>` causes the compiler to infer `T = int?`,
> so the `is Parameter<T>` filter never matches `Parameter<int>` instances. **Always supply the
> type argument explicitly** (e.g. `AddListenerToAll<int>(myDelegate)`) or use the
> `Func<IParameter,bool>` predicate overload, which checks `IParameter.ValueType` directly.

---

#### `DeferredSaveController<TConfig>`

Drives automatic, change-triggered persistence for a `SettingsStore<TConfig>` with smart
debouncing for numeric parameters. Construct after `Load()` and call `Tick()` every frame.

**Save behaviour:**

| Parameter type | When saved |
|---|---|
| `bool`, `string`, `enum` | On the very next `Tick()` after the change — effectively immediate. |
| `int`, `float`, `double` | After `DebounceWindow` elapses with no further numeric changes (default: 1 second). Rapid slider interaction produces one disk write rather than one per frame. |

| Member | Description |
|---|---|
| `DebounceWindow` | The cooldown after the last numeric change before writing to disk. Read-only; set at construction. |
| `Tick()` | Evaluates pending saves; call once per frame from an ImGui callback. Lightweight no-op when nothing is pending. |
| `Flush()` | Forces an immediate save and clears all pending state. |
| `Dispose()` | Calls `Flush()` internally, then unregisters all parameter-change listeners. Dispose this before the store. |

**Ordering requirements:** `Load()` → construct controller → `Tick()` per frame → `Flush()` → `Dispose()` controller → `Save()` + `Dispose()` store.

```csharp
_store          = new SettingsStore<PluginConfig>(configPath);
var config      = _store.Load();
_saveController = new DeferredSaveController<PluginConfig>(_store);

// Optional: shorten the debounce window to 500 ms:
// _saveController = new DeferredSaveController<PluginConfig>(_store, TimeSpan.FromMilliseconds(500));
```

> **Call `DeferredSaveController.Flush()` explicitly before `Dispose()`** — `Dispose()` calls
> `Flush()` internally, but making the save intent explicit at the call site is recommended
> practice and makes the unload sequence self-documenting.

---

### 🏷️ Settings Attributes — `Umbra.SDK.Config.Attributes`

#### Class-level attributes (on a config class or nested group type)

| Attribute | Description |
|---|---|
| `[AutoRegisterSettings]` | Marks the class for parameter auto-discovery in `SettingsStore.Load()`. **Required** on every config type and nested group. |
| `[SettingsPrefix("prefix")]` | Prepends a dot-separated key namespace to every parameter in the class. |
| `[Category("name")]` | Default category name for all parameters in the class; overridable per property. |
| `[CollapseAsTree]` | Renders the category as a collapsible `ImGui.TreeNode` instead of a flat `SeparatorText` header. |
| `[CollapseAsTree(defaultOpen: true)]` | Same, but starts in the expanded state. |
| `[ConfigRootNode]` | Wraps the **entire** config panel inside a single top-level `ImGui.TreeNode`. |
| `[ConfigRootNode("Label", defaultOpen: true)]` | Same with a custom label and starting expanded. |
| `[NestedGroupDrawer<TDrawer>]` | Bypasses recursive parameter expansion; delegates rendering to `INestedGroupDrawer<T>`. |
| `[Indent(amount)]` | Indents all parameters in the class by `amount` pixels (`0` = ImGui default indent). |
| `[LabelMargin(pixels)]` | Adds extra pixels between the label column and the editing widget for all parameters in the class. |

#### Property-level attributes (on `Parameter<T>` properties)

| Attribute | Description |
|---|---|
| `[SettingsParameter]` | Marks the property for registration. Optionally accepts a `keyOverride` string. |
| `[DisplayName("label")]` | Human-readable UI label. Defaults to a PascalCase → `"Pascal Case"` conversion. |
| `[Description("text")]` | Tooltip or help text rendered via a `(?)` help marker. |
| `[Category("name")]` | Overrides the class-level category for this parameter only. |
| `[Range(min, max)]` | Numeric bounds; renders as a slider instead of a drag control. Also enforced at assignment time. |
| `[Step(step)]` | Drag speed for unconstrained numeric controls. Also infers float format precision (e.g. `0.25` → `"%.2f"`). |
| `[Format("%.0f°")]` | Explicit printf-style format string for numeric controls. Overrides the `[Step]`-derived format. |
| `[MaxLength(uint)]` | Maximum character count for `string` input fields (default `256`). |
| `[Multiline(lines)]` | Switches `string` from `InputText` to `InputTextMultiline` with the given visible line count (default `3`). |
| `[ControlWidth(float)]` | Pixel width of the editing widget. `0f` = ImGui default, `-1f` = fill remaining, positive = fixed. |
| `[ButtonWidth(float)]` | Pixel width of a `ButtonDrawer` button. `0f` = auto-size to label, `-1f` = fill, positive = fixed. |
| `[ButtonStyle(ButtonStyle.Danger)]` | Preset color scheme for `ButtonDrawer`. See `ButtonStyle` enum below. |
| `[CustomButtonColors(r, g, b)]` | Custom RGBA button colors for normal/hovered/active states. Overrides `[ButtonStyle]`. |
| `[SpacingBefore(count = 1)]` | Inserts `count` `ImGui.Spacing()` calls **above** the control. |
| `[SpacingAfter(count = 1)]` | Inserts `count` `ImGui.Spacing()` calls **below** the control. |
| `[Indent(amount = 0f)]` | Indents this control only; overrides any class-level `[Indent]`. |
| `[ParameterOrder(int)]` | Explicit render order within the category. Lower = earlier; unordered parameters sort last. |
| `[HideIf<T>("MemberName")]` | Hides the control while the named `bool` member on the same config class is `true`. The member may be a plain `bool` property or a `Parameter<bool>` — the value is automatically unwrapped. |
| `[HideIf<T>("MemberName", value)]` | Hides the control while the named member equals `value`. When the member is a `Parameter<T>`, its `.Value` is automatically unwrapped before the comparison. |
| `[CustomDrawer<TDrawer>]` | Full custom rendering via `IParameterDrawer`. Bypasses label column and all standard layout. |
| `[TwoColumnCustomDrawer<TDrawer>]` | Custom widget via `ITwoColumnParameterDrawer`. Retains the standard two-column label layout. |

**`ButtonStyle` enum:** `Default` · `Primary` (blue) · `Success` (green) · `Warning` (orange) · `Danger` (red) · `Custom` (requires `[CustomButtonColors]`; `ButtonDrawer` logs a one-time warning and falls back to `Default` when `[CustomButtonColors]` is absent).

---

### 🖥️ Config UI — `Umbra.SDK.Config.UI`

#### `ConfigDrawer<TConfig>`

Pre-builds and renders an ImGui settings panel for a typed config class. The draw tree is
assembled once at construction via a single reflection pass; `Draw()` walks the pre-built nodes
with no per-frame reflection.

```csharp
_drawer = new ConfigDrawer<PluginConfig>(config, "MyPlugin");

// In ImGui callback each frame:
_drawer.Draw();

// On unload:
_drawer.Dispose();
_drawer = null;
```

- `idScope` scopes all ImGui widget IDs to prevent cross-plugin collisions. Must be non-empty.
- Dispose on plugin unload to release stateful custom drawers (e.g. hotkey capture counters).
- `Draw()` on a disposed instance logs a warning and is a safe no-op rather than throwing.

---

#### Built-in parameter drawers

Apply these with the corresponding `[CustomDrawer<>]` or `[TwoColumnCustomDrawer<>]` attribute.

##### `HotkeyDrawer`

Full-row hotkey capture for a `Parameter<int>` (an `ImGuiKey` cast to `int`). Renders the
current key name and **Change** / **Cancel** buttons. At most one capture is active per frame.

```csharp
[SettingsParameter, DisplayName("Hotkey"), CustomDrawer<HotkeyDrawer>]
public Parameter<int> ActivateKey { get; set; } = new((int)ImGuiKey.F2);
```

##### `TwoColumnHotkeyDrawer`

Same capture behavior as `HotkeyDrawer` but participates in the standard two-column label
layout. Use this when the hotkey should align with other parameters in the same category.

```csharp
[SettingsParameter, DisplayName("Activate key"), TwoColumnCustomDrawer<TwoColumnHotkeyDrawer>]
public Parameter<int> ActivateKey { get; set; } = new((int)ImGuiKey.F2);
```

Both drawers share `HotkeyCaptureState.WaitingCount` so at most one is ever capturing input at
once. `ConfigDrawer` disposes them on unload to reset the counter.

##### `ButtonDrawer`

Renders a push-button for a `Parameter<Action>`. The stored action is invoked on click.
The button label comes from `[DisplayName]`; the action is not persisted to JSON.

```csharp
[SettingsParameter, DisplayName("Reset to defaults"), ButtonStyle(ButtonStyle.Danger), CustomDrawer<ButtonDrawer>]
public Parameter<Action> ResetButton { get; set; } = new(() => { /* ... */ });
```

---

### 🎨 Custom Drawers — `Umbra.SDK.Config.UI.ParameterDrawers`

#### `IParameterDrawer`

Complete rendering control. The factory skips the label column and all standard layout.

```csharp
public sealed class MyDrawer : IParameterDrawer
{
    public void Draw(string label, IParameter parameter)
    {
        // full ImGui layout; use $"##{parameter.Key}" as the widget ID
    }
}
```

Apply with `[CustomDrawer<MyDrawer>]` on the property.

#### `ITwoColumnParameterDrawer`

Renders only the editing widget; the factory handles label, help marker, column alignment,
and `SetNextItemWidth`.

```csharp
public sealed class MyWidgetDrawer : ITwoColumnParameterDrawer
{
    public void Draw(IParameter parameter)
    {
        // widget only — SetNextItemWidth is already applied
        // use $"##{parameter.Key}" as the ImGui widget ID
    }
}
```

Apply with `[TwoColumnCustomDrawer<MyWidgetDrawer>]` on the property.

#### `INestedGroupDrawer<T>`

Full layout control for an entire nested configuration group. Applied at the **type** level
with `[NestedGroupDrawer<TDrawer>]`.

```csharp
[AutoRegisterSettings, SettingsPrefix("myGroup"), Category("My Group")]
[NestedGroupDrawer<MyGroupDrawer>]
public record MyGroup
{
    [SettingsParameter]
    public Parameter<int> Value { get; set; } = new(42);
}

internal sealed class MyGroupDrawer : INestedGroupDrawer<MyGroup>
{
    public void Draw(MyGroup groupInstance)
    {
        ImGui.Text($"Value: {groupInstance.Value.Value}");
        int v = groupInstance.Value.Value;
        if (ImGui.SliderInt("##value", ref v, 0, 100))
            groupInstance.Value.Set(v);
    }
}
```

All three interfaces extend `IDisposable`. The default `Dispose` calls `GC.SuppressFinalize`;
override only when the drawer holds resources that must be released on plugin unload.

---

### 🖼️ Plugin Panel — `Umbra.SDK.UI.Panel`

`PluginPanel` composes an ordered list of `IPanelSection` instances under a shared ImGui ID scope
and owns their lifetimes. It is the recommended top-level UI type for plugins that display both
configuration settings and live game state.

```csharp
_panel = new PluginPanel("MyPlugin")
    .Add(new LiveSection<CameraState>(_cameraState))
    .Add(new ConfigSection<PluginConfig>(config));

// Per-frame (in ImGui callback):
_panel.Draw();

// On unload:
_panel.Dispose();
_panel = null;
```

Sections render in ascending `Order`; equal-order sections preserve insertion order (stable sort).

#### `IPanelSection`

| Member | Description |
|---|---|
| `int Order` | Default `int.MaxValue`. Derived from `[SectionOrder]` on the state or config type. |
| `void Draw()` | Called every frame by `PluginPanel.Draw()`. Must be render-thread safe. |
| `void Dispose()` | Called by `PluginPanel.Dispose()`. |

#### `ConfigSection<TConfig>`

Wraps `ConfigDrawer<TConfig>` as a panel section.

```csharp
new ConfigSection<PluginConfig>(config)           // idScope defaults to type name
new ConfigSection<PluginConfig>(config, "MySub")  // explicit sub-scope
```

#### `LiveSection<T>`

Renders a live game state instance each frame via `ILiveSectionDrawer<T>` declared on the state
type with `[LiveSectionDrawer<TDrawer>]`. The drawer is instantiated once at construction; no
per-frame reflection.

```csharp
// State type — annotate with the drawer:
[LiveSectionDrawer<CameraStatusDrawer>]
public sealed class CameraState
{
    public float      Fov  { get; set; }
    public CameraMode Mode { get; set; }
}

// Drawer:
internal sealed class CameraStatusDrawer : ILiveSectionDrawer<CameraState>
{
    public void Draw(CameraState state)
    {
        ImGui.Text($"FOV: {state.Fov:F1}");
        ImGui.Text($"Mode: {state.Mode}");
    }
}

// Wiring — plugin owns the state instance:
_cameraState = new CameraState();
FovHooks.Attach(_cameraState);
new LiveSection<CameraState>(_cameraState)
```

- Use the **swap-instance pattern** in hooks that update multiple fields to guarantee the
  drawer always reads a consistent snapshot (see `[PluginExitPoint]` pattern below).
- Always call `Detach()` (or null-assignment) in `[PluginExitPoint]` so post-unload hook
  calls are safe no-ops.

#### `SectionOrderAttribute`

Apply to a state class or config record to control panel render position:

```csharp
[SectionOrder(0)]   public sealed class CameraState  { … }
[SectionOrder(1)]   public record      PluginConfig   { … }
```

---

### ⌨️ Input — `Umbra.SDK.Input.KeyboardInput`

| Member | Description |
|---|---|
| `TryCaptureKeyboardKey(out int)` | Returns `true` and the pressed `ImGuiKey` (as `int`) this frame. Mouse, gamepad, and modifier-alias keys are excluded. |
| `GetKeyName(int)` | Returns the enum member name, or `Key(n)` for unknown values. |
| `IsValidKey(int)` | `true` when the key value is > `ImGuiKey.None`. |
| `IsCtrlHeld` | `true` while left or right Ctrl is held. |
| `IsShiftHeld` | `true` while left or right Shift is held. |
| `IsAltHeld` | `true` while left or right Alt is held. |

Hotkey values are stored as `int` (an `ImGuiKey` cast to `int`).

---

### 🧰 ImGui Helpers — `Umbra.SDK.UI.ImGuiWidgets`

| Member | Description |
|---|---|
| `DrawHelpMarker(string)` | Renders an inline `(?)` marker that shows a tooltip on hover. Call after `ImGui.SameLine()`. |

---

### 🔌 Plugin Lifecycle — Recommended Pattern

```csharp
private static PluginPanel?                          _panel;
private static SettingsStore<PluginConfig>?          _store;
private static DeferredSaveController<PluginConfig>? _saveController;
private static CameraState?                          _cameraState;

private static readonly PluginLogger _log = new("MyPlugin");

[PluginEntryPoint]
public static void Load()
{
    var configPath = Path.Combine(API.GetPluginDirectory(typeof(MyPlugin).Assembly), "config.json");

    _store          = new SettingsStore<PluginConfig>(configPath);
    var config      = _store.Load();                                // Load FIRST
    _saveController = new DeferredSaveController<PluginConfig>(_store); // THEN construct

    _cameraState = new CameraState();
    FovHooks.Attach(_cameraState);

    _panel = new PluginPanel("MyPlugin")
        .Add(new LiveSection<CameraState>(_cameraState))
        .Add(new ConfigSection<PluginConfig>(config));
}

[Callback(typeof(ImGuiDrawUI), CallbackType.Pre)]
public static void PreDrawUI()
{
    if (API.IsDrawingUI())
        _panel?.Draw();        // render UI on the game's draw thread each frame

    _saveController?.Tick();   // evaluate pending saves; lightweight no-op when idle
}

[PluginExitPoint]
public static void Unload()
{
    FovHooks.Detach();
    _cameraState = null;

    _saveController?.Flush();   // guarantee no debounced write is dropped
    _saveController?.Dispose(); // remove listeners before the store is disposed
    _saveController = null;

    _store?.Save();             // belt-and-suspenders final save
    _store?.Dispose();
    _store = null;

    _panel?.Dispose();
    _panel = null;
}
```

> **Call `DeferredSaveController.Flush()` explicitly before `Dispose()`** — `Dispose()` calls
> `Flush()` internally, but making the save intent explicit at the call site is recommended
> practice and makes the unload sequence self-documenting.
