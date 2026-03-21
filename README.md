# 🎮 REFW.Umbra

> A support library and example plugin for **RE Engine** game modding via [REFramework.NET](https://github.com/praydog/REFramework).
> Runs inside the game process through the REFramework managed plugin environment — **.NET 10 / x64**.

---

## 📦 Solution Structure

| Project | Description |
|---|---|
| `Umbra.SDK` | Reusable support library: logging, configuration, ImGui helpers, keyboard utilities. |
| `Umbra.SamplePlugin` | Example plugin that uses `Umbra.SDK` to expose a configurable enhanced camera for RE9. |

---

## 🔧 Umbra.SDK

### Logging — `Umbra.SDK.Logging.PluginLogger` / `Logger`

Two exception-safe wrappers around `REFrameworkNET.API.Log*`. All core (non-formatted) methods silently suppress errors to avoid disrupting the game process. Formatted overloads (`...(string format, params object[] args)`) also silently suppress any exception thrown by `string.Format` — invalid format strings or mismatched arguments cause the message to be discarded rather than propagate.

#### `PluginLogger` — per-plugin instance (recommended for all plugins)

Because all managed plugins load into the **same AppDomain**, using shared static state for a
prefix or log level would cause each plugin that loads to silently overwrite every earlier
plugin's configuration. `PluginLogger` solves this by keeping all configuration in a
**per-plugin instance** — declare it as a `private static readonly` field and initialise it
inline so it is never `null` and never shared.

```csharp
// Declare once — no entry-point side-effects on shared state.
private static readonly PluginLogger _log = new("MyPlugin");

// In Load() / Unload() / anywhere:
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
| `Exception(Exception, string)` | `API.LogError` | Includes type, message, and stack trace |
| `Exception(Exception, string, params object[])` | `API.LogError` | Formatted context message overload |
| `Prefix` | — | Optional string prepended as `[Prefix] message` |
| `PrefixFormat` | — | Composite format string; `{0}` → `Prefix`. Defaults to `"[{0}]"` |
| `MinLevel` | — | Messages below this level are silently discarded. Defaults to `LogLevel.Info` |

#### `Logger` — static, raw forwarding facade

The static `Logger` class carries **no configuration** and forwards messages unconditionally. It
is intended for SDK-internal use. Prefer `PluginLogger` in all plugin code.

---

### RE9 Context IDs — `Umbra.SDK.Games.RE9.Re9ContextIds`

Shared RE9 context identifiers.

| Constant | Description |
|---|---|
| `Re9ContextIds.Leon` | RE9 Leon campaign context GUID |
| `Re9ContextIds.Grace` | RE9 Grace campaign context GUID |

---

### ⚙️ Configuration — `Umbra.SDK.Config`

A reflection-based, attribute-driven settings system backed by JSON persistence. The draw tree for the UI is built once at construction — no per-frame reflection.

#### Quick-Start Flow

1. Define a config class decorated with the SDK attributes.
2. Create a `SettingsStore<TConfig>` with the path to the JSON file.
3. Call `Load()` to get a fully populated config instance.
4. Pass the config instance to `ConfigDrawer<TConfig>` to render a settings panel.
5. Optionally wrap the manager in a `DeferredSaveController<TConfig>` for automatic change-triggered saves.
6. Call `Save()` before `Dispose()` on plugin unload.

```csharp
// Plugin entry point
var configPath = Path.Combine(API.GetPluginDirectory(typeof(MyPlugin).Assembly), "data", "config.json");
_store = new SettingsStore<PluginConfig>(configPath);
var config = _store.Load();
_saveController = new DeferredSaveController<PluginConfig>(_store);
_drawer = new ConfigDrawer<PluginConfig>(config, "MyPlugin");

// Plugin exit point
_saveController?.Flush();
_saveController?.Dispose();
_saveController = null;
_store?.Save();
_store?.Dispose();
_store = null;
_drawer?.Dispose();
_drawer = null;
```

---

#### `Parameter<T>`

The core value container. Supported types for JSON persistence: `bool`, `int`, `float`, `double`, `string`, and any `enum`.

```csharp
public Parameter<float> FieldOfView { get; set; } = new(55f);

// Read
float fov = config.FieldOfView.Value;
float fovImplicit = config.FieldOfView; // implicit conversion

// Write (raises ValueChanged)
config.FieldOfView.Value = 70f;

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
| `Load()` | Creates a `TConfig` instance, registers parameters, and loads values from disk. Saves defaults if no file exists. |
| `Save()` | Serializes all parameter values to the configured JSON file. |
| `ResetAll()` | Resets every registered parameter to its default value. Delegate-typed parameters (e.g. button actions) are skipped. |
| `CopyValuesTo(target, setWithoutNotifying)` | Mirrors all values into another store, optionally suppressing change events. |
| `AddListenerToAll(Action)` | Subscribes to `ValueChanged` on every parameter. Auto-removed on `Dispose`. |
| `AddListenerToAll<T>(Action<T?,T?>)` | Subscribes to `ValueChanged` on every `Parameter<T>` whose type matches. Auto-removed on `Dispose`. |
| `RemoveListenerFromAll(Action)` | Manually unsubscribes a listener. |
| `RemoveListenerFromAll<T>(Action<T?,T?>)` | Manually unsubscribes a typed listener. |
| `Dispose()` | Removes all event subscriptions. Always call after `Save()`. |

---

#### `DeferredSaveController<TConfig>`

Drives automatic change-triggered persistence with debouncing for numeric sliders.

- **Non-numeric changes** (booleans, strings, enums) — saved on the very next `Tick()` call.
- **Numeric changes** (`int`, `float`, `double`) — saved after `DebounceWindow` elapses since the last change (default: 1 second), so rapid slider interaction produces a single disk write.

```csharp
var controller = new DeferredSaveController<PluginConfig>(_manager);

// Call once per frame from the ImGuiDrawUI callback:
controller.Tick();

// Before unload — guarantee any pending changes are written:
controller.Flush();
controller.Dispose();
```

---

#### JSON Persistence

Settings are serialized with `System.Text.Json` using camelCase property naming and enums as strings. The file is written to the path passed to `SettingsStore<TConfig>`.

---

### 🏷️ Settings Attributes

#### Class-Level Attributes

| Attribute | Target | Description |
|---|---|---|
| `[AutoRegisterSettings]` | class / struct | Marks a settings group for automatic parameter discovery by `SettingsStore`. Required on every config class and nested group. |
| `[SettingsPrefix("prefix")]` | class / struct | Prepends a dot-separated key prefix to all parameters in the class (e.g. `"Camera"` → keys like `"Camera.fieldOfView"`). |
| `[Category("name")]` | class / struct / property | Groups parameters under a named section in the UI. Can be placed on the class (inherited by all members) or overridden on individual properties. |
| `[CollapseAsTree]` / `[CollapseAsTree(defaultOpen: true)]` | class | Renders the category as a collapsible `ImGui.TreeNode` instead of a flat `SeparatorText` header. `defaultOpen` controls whether the node starts expanded (defaults to `false`). |
| `[ConfigRootNode]` / `[ConfigRootNode("Label")]` | class | Wraps the entire settings panel in a single top-level `ImGui.TreeNode`. When `label` is omitted the config class name is space-separated (e.g. `PluginConfig` → `"Plugin Config"`). Accepts an optional `defaultOpen` bool as a second argument. |
| `[Indent(amount)]` | class / property | On a **class**: indentation fallback for each parameter control in that group. On the **property** declaring a nested group: wraps the entire category block in an `ImGui.Indent`/`ImGui.Unindent` scope. `amount = 0` uses ImGui's default spacing. |

#### Property-Level Attributes

| Attribute | Description |
|---|---|
| `[SettingsParameter]` | Marks a property as a settings parameter. Optional `keyOverride` replaces the auto-derived key. |
| `[DisplayName("label")]` | Human-readable label shown in the settings UI. Falls back to the property name when absent. |
| `[Description("text")]` | Tooltip / help text rendered as a `(?)` marker next to the control. |
| `[Range(min, max)]` | Enforces a numeric range. Renders as `SliderFloat` / `SliderInt` instead of a drag control. Also enforced at the `Parameter<T>` level — out-of-range values are rejected. |
| `[Step(step)]` | Drag speed for unconstrained drag controls (`DragInt` / `DragFloat`). For `float`/`double`, also infers the fallback display format's decimal precision (e.g. `0.25` → `"%.2f"`); applies to both `DragFloat`/`DragDouble` and `SliderFloat`/`SliderDouble` when `[Format]` is absent. Has no effect on `SliderInt`. |
| `[Format("%.1f°")]` | Printf-style format string override for numeric controls. Overrides the `[Step]`-derived format. |
| `[MaxLength(uint)]` | Maximum character count for `string` input fields. Defaults to `256` when absent. |
| `[Multiline(lines = 3)]` | Switches a `string` parameter from `InputText` to `InputTextMultiline`. `lines` controls the visible height. Use alongside `[MaxLength]` to increase the character buffer. |
| `[ParameterOrder(order)]` | Controls display order within a category. Lower values appear first. Parameters without this attribute sort after all explicitly ordered ones; declaration order is preserved among equals. |
| `[SpacingBefore(count = 1)]` | Inserts one or more `ImGui.Spacing()` calls above the control. Travels with the parameter when `[ParameterOrder]` reordering is active. |
| `[SpacingAfter(count = 1)]` | Inserts one or more `ImGui.Spacing()` calls below the control. Travels with the parameter when `[ParameterOrder]` reordering is active. |
| `[Indent(amount)]` | Indents this individual `Parameter<T>` control. Overrides the class-level `[Indent]` fallback for this parameter. |
| `[ButtonStyle(style)]` | Sets the color style of a `ButtonDrawer` button. Variants: `Default`, `Primary`, `Success`, `Warning`, `Danger`. |
| `[ButtonWidth(width)]` | Sets the pixel width of a `ButtonDrawer` button. `0f` = auto-size to label, `-1f` = fill available width, positive = fixed px. |
| `[HideIf<T>("MemberName")]` | Hides the control while the named `bool` member is `true`. |
| `[HideIf<T>("MemberName", value)]` | Hides the control while the named member equals `value`. Works with `Parameter<T>` properties (unwrapped automatically) and plain fields or properties. |
| `[CustomDrawer<TDrawer>]` | Renders the control using a custom `IParameterDrawer` instead of the default type-inferred control. |

#### Key Derivation

Keys are dot-separated and built as `[SettingsPrefix].[propertyName (camelCased)]`, unless `keyOverride` is provided on `[SettingsParameter]`.

---

### 🖼️ Settings UI — `Umbra.SDK.Config.UI`

#### `ConfigDrawer<TConfig>`

Renders a full ImGui settings panel from a config instance. The draw tree is built once at construction; `Draw()` walks the pre-built node list with no per-frame reflection.

```csharp
_drawer = new ConfigDrawer<PluginConfig>(config, "MyPlugin");

// Inside ImGuiDrawUI callback:
_drawer.Draw();

// Dispose when the window closes or the plugin unloads:
_drawer.Dispose();
```

- Construct with the instance returned by `SettingsStore<TConfig>.Load()` so that `ParameterMetadata` is already populated.
- Apply `[CollapseAsTree]` to any nested settings group class to render its category as a collapsible tree node.
- Apply `[ConfigRootNode]` to the root config class to wrap the entire panel in a single top-level tree node.
- `ConfigDrawer<TConfig>` implements `IDisposable` — always dispose it.

#### Default Control Mapping

| `Parameter<T>` type | Rendered as |
|---|---|
| `bool` | `ImGui.Checkbox` |
| `int` (no `[Range]`) | `ImGui.DragInt` |
| `int` (with `[Range]`) | `ImGui.SliderInt` |
| `float` / `double` (no `[Range]`) | `ImGui.DragFloat` |
| `float` / `double` (with `[Range]`) | `ImGui.SliderFloat` |
| `string` (no `[Multiline]`) | `ImGui.InputText` |
| `string` (with `[Multiline]`) | `ImGui.InputTextMultiline` |
| `enum` | `ImGui.Combo` (enum member names) |
| `Action` | `ImGui.Button` via `[CustomDrawer<ButtonDrawer>]` |
| Other | Read-only `ImGui.TextDisabled` label |

#### `IParameterDrawer` — Custom Controls

Implement this interface and apply `[CustomDrawer<TDrawer>]` to a parameter property to replace the default control.

```csharp
public sealed class MyDrawer : IParameterDrawer
{
    public void Draw(string label, IParameter parameter)
    {
        // render your ImGui control here
    }
}
```

> `TDrawer` must have a public parameterless constructor; both constraints are enforced at compile time.

#### Built-in Drawer: `HotkeyDrawer`

Renders a hotkey-capture control for a `Parameter<int>` where the value is an `ImGuiKey` cast to `int`. Multiple `HotkeyDrawer` instances coordinate automatically so only one capture session is active at a time.

```csharp
[SettingsParameter("toggleHotkey")]
[DisplayName("Toggle Hotkey")]
[Description("The hotkey to toggle the plugin.")]
[CustomDrawer<HotkeyDrawer>]
public Parameter<int> ToggleHotkey { get; set; } = new(574); // ImGuiKey.F3
```

#### Built-in Drawer: `ButtonDrawer`

Renders an ImGui push-button for a `Parameter<Action>`. The stored action is invoked on click. Button parameters are never written to or read from the JSON settings file.

```csharp
[SettingsParameter]
[DisplayName("Reset Settings")]
[Description("Resets all values to their defaults.")]
[CustomDrawer<ButtonDrawer>]
[ButtonStyle(ButtonStyle.Danger)]
[ButtonWidth(-1f)]
public Parameter<Action> ResetButton { get; init; }

// Wire up the action in the config constructor:
public PluginConfig()
{
    ResetButton = new(() => { FieldOfView.Reset(); /* … */ });
}
```

---

### 🖱️ ImGui Helpers — `Umbra.SDK.UI.ImGuiControls`

Static helpers for common settings panel controls. All methods must be called from within an active ImGui window.

| Method | Description |
|---|---|
| `DrawHotKeySetting(label, id, ref state, ref keyCode, otherWaiting)` | Renders a hotkey label + "Change" button, handling capture mode and cancellation. `id` must be a stable unique string within the window to prevent ImGui ID collisions when two hotkey controls share the same display label. |
| `DrawSlider(label, ref value, min, max, format)` | `ImGui.SliderFloat` wrapper. Defaults: `min=10`, `max=120`, `format="%.1f"`; |
| `DrawIntSlider(label, ref value, min, max, format)` | `ImGui.SliderInt` wrapper. |
| `DrawCheckbox(label, ref value)` | `ImGui.Checkbox` wrapper. |
| `DrawComboBox(label, ref selectedIndex, string[] items)` | `ImGui.Combo` wrapper. |
| `DrawSectionHeader(label)` | `ImGui.SeparatorText` wrapper. |
| `DrawHelpMarker(description)` | Renders a `(?)` label that shows a tooltip on hover. Call after `ImGui.SameLine()`. |

The following static string properties can be set once at startup to localise hotkey control labels:

| Property | Default | Description |
|---|---|---|
| `HotkeyChangeLabel` | `"Change"` | Label for the "Change" button when no capture is in progress. |
| `HotkeyCancelLabel` | `"Cancel"` | Label for the "Cancel" button during an active capture. |
| `HotkeyCapturingPrompt` | `"{0}: Press any key..."` | Prompt shown while waiting for a key press; `{0}` is replaced by the parameter label. |

---

### ⌨️ Keyboard Utilities — `Umbra.SDK.Input.KeyboardInput`

Helpers for hotkey capture and modifier-key state, backed by ImGui's input system.

```csharp
// Capture a key pressed this frame (excludes mouse/gamepad/modifier keys)
if (KeyboardInput.TryCaptureKeyboardKey(out int key))
    myHotkey = key; // ImGuiKey cast to int

// Get a display name for a stored key code
string name = KeyboardInput.GetKeyName(myHotkey); // e.g. "F3"

// Validate a stored key code
bool ok = KeyboardInput.IsValidKey(myHotkey); // true if key > ImGuiKey.None

// Modifier state (left OR right key)
bool ctrl  = KeyboardInput.IsCtrlHeld;
bool shift = KeyboardInput.IsShiftHeld;
bool alt   = KeyboardInput.IsAltHeld;
```

---

## 🧩 Example Plugin — `Umbra.SamplePlugin`

Demonstrates the full SDK lifecycle for an RE9 camera plugin.

### `PluginConfig`

```csharp
[AutoRegisterSettings]
[ConfigRootNode("Sample Plugin v1.0")]
[SettingsPrefix("samplePlugin")]
[Category("General")]
[CollapseAsTree]
public record PluginConfig
{
    [SettingsParameter, DisplayName("Enabled"), Description("Whether the plugin is enabled.")]
    public Parameter<bool> IsEnabled { get; set; } = new(true);

    [SettingsParameter, DisplayName("Toggle Hotkey"), CustomDrawer<HotkeyDrawer>]
    public Parameter<int> ToggleHotkey { get; set; } = new(574); // ImGuiKey.F3

    [SettingsParameter]
    public FovSettings Fov { get; set; } = new();

    [SettingsParameter]
    [DisplayName("Reset General"), CustomDrawer<ButtonDrawer>, ButtonStyle(ButtonStyle.Danger), ButtonWidth(-1f)]
    public Parameter<Action> ResetGeneral { get; init; }

    public PluginConfig()
    {
        ResetGeneral = new(() => { IsEnabled.Reset(); ToggleHotkey.Reset(); });
    }
}

[AutoRegisterSettings, Category("FOV"), SettingsPrefix("fov"), Indent]
public record FovSettings
{
    [SettingsParameter("tps"), DisplayName("3rd Person"), Range(10f, 180f), Format("%.1f"), ParameterOrder(0)]
    public Parameter<float> Tps { get; set; } = new(55f);
    // …
}
```

### Plugin Entry / Exit Pattern

```csharp
[PluginEntryPoint]
public static void Load()
{
    var configPath = GetConfigPath();
    _store = new SettingsStore<PluginConfig>(configPath);
    var config = _store.Load();
    _saveController = new DeferredSaveController<PluginConfig>(_store);
    _drawer = new ConfigDrawer<PluginConfig>(config, "SamplePlugin");
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
    _drawer?.Dispose();
    _drawer = null;
}

[Callback(typeof(ImGuiDrawUI), CallbackType.Pre)]
public static void PreDrawUI()
{
    if (API.IsDrawingUI())
        _drawer?.Draw();
    _saveController?.Tick();
}
```

---

## 📚 Dependencies

Assemblies are referenced locally from two folders populated by `scripts/setup_reframework_deps.bat` (see [Scripts](#-scripts) below). Neither folder is committed to source control.

**`dependencies/reframework/api/`** — REFramework C# API and ImGui bindings:

| Assembly | Purpose |
|---|---|
| `REFramework.NET` | REFramework managed plugin host API |
| `Hexa.NET.ImGui` | ImGui bindings for in-game UI |
| `HexaGen.Runtime` | Runtime support for Hexa bindings |
| `AssemblyGenerator` | REFramework assembly generator |
| `REFCoreDeps` | REFramework core dependencies |
| `Microsoft.CodeAnalysis` / `Microsoft.CodeAnalysis.CSharp` | Roslyn support bundled with the C# API |

**`dependencies/reframework/generated/`** — Game-specific binding assemblies generated on first game launch with the C# API installed:

| Assembly | Purpose |
|---|---|
| `REFramework.NET.application` | Generated RE Engine application bindings |
| `REFramework.NET.viacore` | Generated RE Engine viacore bindings |
| `REFramework.NET._System.Private.CoreLib` | Generated CoreLib bindings |

---

## 📜 Scripts

All scripts live in the `scripts/` folder. They are Windows batch files and PowerShell scripts; no additional tooling is required beyond PowerShell (included with Windows 10 and later).

---

### `setup_reframework_deps.bat` — One-Time Dev Environment Setup

A thin launcher that invokes `setup_reframework_deps.ps1` with PowerShell. Run it once before opening the solution for the first time, and again whenever you need to refresh the API DLLs or pick up newly generated game bindings.

The underlying PowerShell script can also be called directly when automation is needed:

```powershell
# Non-interactive: skip all optional prompts
.\scripts\setup_reframework_deps.ps1 -NoPrompt

# Pre-supply the game executable path to bypass the file browser
.\scripts\setup_reframework_deps.ps1 -GamePath "C:\SteamLibrary\...\re9.exe"
```

**What it does, step by step:**

1. **Downloads the C# API** — queries the [REFramework-nightly GitHub Releases API](https://api.github.com/repos/praydog/REFramework-nightly/releases) for the latest release, downloads `csharp-api.zip`, and extracts it to a temporary directory.
2. **Copies API DLLs** — copies `REFramework.NET.dll` and all files from `reframework/plugins/managed/dependencies/` inside the zip into `dependencies/reframework/api/` at the solution root.
3. **Configures the game executable** — opens a file browser to select your RE Engine game executable (`.exe`). The game directory is written to `game_dir.local.txt` and the full executable path to `game_exe.local.txt` at the solution root. Both files are gitignored; no game paths are ever committed. If previously configured, shows the current settings and lets you keep, change, or skip them.
4. **Copies generated game bindings** — if the C# API is already installed and the game has been launched at least once, the generated binding assemblies are copied from `{game}/reframework/plugins/managed/generated/` into `dependencies/reframework/generated/`.
5. **Offers to install the C# API into the game** — if the C# API is not yet installed, the script offers to copy the full contents of the downloaded zip into the game root, preserving the directory structure.
6. **Configures a Visual Studio debug profile** *(optional)* — creates or updates `Umbra.SDK/Properties/launchSettings.json` with an executable launch profile pointing to the configured game, enabling F5 debugging in Visual Studio.

> **⚡ First-Time Setup Order**
> 1. Run `setup_reframework_deps.bat`.
> 2. Select your game executable when prompted.
> 3. If the C# API was not installed, let the script install it, then launch your RE Engine game once and close it.
> 4. Run `setup_reframework_deps.bat` again to copy the generated bindings into `dependencies/reframework/generated/`.
> 5. Open the solution — both projects should resolve all references and build.

---

### `kill_re9.bat` — Pre-Build Event *(Debug only)*

Registered as a **pre-build event** in both `Umbra.SDK.csproj` and `Umbra.SamplePlugin.csproj`.

Checks whether `re9.exe` is currently running and, if so, terminates it with `taskkill /F` before the build starts. This releases file locks on plugin assemblies already loaded by the game, preventing "file in use" errors when MSBuild tries to overwrite the output DLLs. Always exits with code `0` so a missing or already-stopped process never fails the build.

---

### `deploy_reframework_deps.bat` — Post-Build Event for `Umbra.SDK` *(Debug only)*

Reads `game_dir.local.txt` from the solution root and copies all files matching `Umbra.SDK.*` into:

```
{game}\reframework\plugins\managed\dependencies\
```

This deploys the SDK support library into the REFramework managed dependencies folder so plugins that depend on it can find it at runtime.

---

### `deploy_reframework_plugin.bat` — Post-Build Event for `Umbra.SamplePlugin` *(Debug only)*

Reads `game_dir.local.txt` from the solution root and copies all files matching `Umbra.SamplePlugin.*` into:

```
{game}\reframework\plugins\managed\
```

This deploys the plugin directly into the REFramework managed plugins folder so the game loads it automatically on the next launch.

---

### Other Files

| File | Description |
|---|---|
| `setup_reframework_deps.ps1` | PowerShell implementation invoked by `setup_reframework_deps.bat`. Can also be called directly; supports `-NoPrompt` (skip optional prompts) and `-GamePath "path\to\game.exe"` (bypass the file browser). |
| `game_dir.local.txt` | Written by `setup_reframework_deps.bat`; read by both deploy scripts. Contains a single line: the absolute path to your RE Engine game root directory. Gitignored — no local paths are ever committed. |
| `game_exe.local.txt` | Written by `setup_reframework_deps.bat`. Contains the full path to the configured game executable. Used to populate `launchSettings.json` and remembered between runs. Gitignored. |