# Copilot instructions for REFW.Umbra

## Project context
- `Umbra.SDK` is a support library for REFramework mod/plugin development.
- The primary runtime target is **REFramework**, specifically **REFramework.NET**.
- Code in this repository is intended to run inside the game process through the REFramework managed plugin environment, not as a standalone desktop or web application.
- The workspace targets `.NET 10` and `x64`.

## REFramework-specific guidance
- Prefer patterns that work inside the REFramework managed plugin host.
- Use `REFramework.NET` APIs when interacting with the host environment.
- For logging in plugin code, use `Umbra.SDK.Logging.PluginLogger` — an **instance class** that forwards to `REFrameworkNET.API.LogInfo`, `API.LogWarning`, and `API.LogError`.
  - Declare the logger as a `private static readonly PluginLogger _log = new("PluginName");` field on the plugin class. Never set it in the entry point — initialise it inline so it is always available and never shared with other plugins.
  - Do **not** use `Logger.Prefix`, `Logger.PrefixFormat`, or `Logger.MinLevel`. These properties no longer exist on the static `Logger` class. All managed plugins load into the same AppDomain; any static prefix written by one plugin would silently overwrite every other plugin's prefix.
  - `PluginLogger` exposes `Prefix`, `PrefixFormat`, and `MinLevel` as instance properties, fully isolated per plugin.
  - All `PluginLogger` methods are exception-safe and silently suppress errors to avoid disrupting the game process.
  - `_log.Exception(Exception ex, string message)` logs a context message followed by the exception type, message, and stack trace via `API.LogError`. Use this — **not** `_log.Error` — when logging exceptions.
  - The static `Logger` class still exists as a raw, unconditional forwarding facade (no prefix, no level filter). It is intended for SDK-internal use only.
- Assume game-facing code may run in a constrained plugin environment where resilience is preferred over hard failures.

## UI and input
- UI should be implemented with **ImGui**, using `Hexa.NET.ImGui`.
- Do not suggest WPF, WinForms, MAUI, Blazor, or ASP.NET for in-game UI.
- For hotkeys and keyboard capture, use the helpers in `Umbra.ImGuiHelper` and `Umbra.KeyboardUtil` (both are in the `Umbra` namespace, not `Umbra.SDK`).
  - `ImGuiHelper` provides: `DrawHotKeySetting`, `DrawSlider`, `DrawIntSlider`, `DrawCheckbox`, `DrawComboBox`, `DrawSectionHeader`, `DrawHelpMarker`.
  - `KeyboardUtil` provides: `TryCaptureKeyboardKey(out int)`, `GetKeyName(int)`, `IsValidKey(int)`, and modifier-state properties `IsCtrlHeld`, `IsShiftHeld`, `IsAltHeld`.
  - Hotkey values are stored as `int` (an `ImGuiKey` cast to `int`).

## Settings/configuration
- Prefer the existing configuration model in `Umbra.SDK.Config`.
- Settings flow:
  1. Create a config class with `[AutoRegisterSettings]` (and optionally `[SettingsPrefix("...")]` and/or `[Category("...")]` at the class level).
  2. Declare each setting as a `Parameter<T>` property decorated with `[SettingsParameter]` (optionally with a `keyOverride` string).
  3. Instantiate `SettingsStore<TConfig>` with the absolute path to the JSON file.
  4. Call `settingsStore.Load()` to obtain a populated config instance; call `settingsStore.Save()` to persist current values.
- `SettingsStore<TConfig>` requires `TConfig : class, new()`. `record` types satisfy this constraint and are the preferred style for config classes.
- Key derivation: dot-separated; the prefix from `[SettingsPrefix]` is prepended to each property name (camelCased), unless a `keyOverride` is provided on `[SettingsParameter]`.
- Metadata attributes available from `Umbra.SDK.Config.Attributes`:
  - `[DisplayName("...")]` — human-readable UI label.
  - `[Description("...")]` — tooltip or help text shown via a `(?)` help marker.
  - `[Category("...")]` — groups related parameters in the UI; can be placed on the class or a specific property.
  - `[Range(min, max)]` — numeric bounds; when present the control renders as a slider instead of a drag input.
  - `[Step(step)]` — drag speed for unconstrained numeric controls; also infers the fallback float format's decimal precision.
  - `[Format("...")]` — printf-style format string override for numeric controls (e.g. `"%.0f°"`, `"%d px"`). Overrides the `[Step]`-derived format.
  - `[MaxLength(uint)]` — maximum character count for `string` input fields (default `256`).
  - `[Spacing(count = 1)]` — inserts one or more `ImGui.Spacing()` calls above the control.
  - `[Indent(amount = 0f)]` — indents the control (or all controls in a group class) using `ImGui.Indent`/`Unindent`; `0` uses ImGui's default spacing. Can be applied at the class or property level.
  - `[HideIf<T>("MemberName")]` — hides the control while the named `bool` member on the same config class is `true`.
  - `[HideIf<T>("MemberName", value)]` — hides the control while the named member equals `value`.
  - `[CustomDrawer<TDrawer>]` — renders the control using a custom `IParameterDrawer` implementation instead of the default; `TDrawer` must implement `IParameterDrawer` and have a public parameterless constructor.
- Supported `Parameter<T>` value types for JSON persistence: `bool`, `int`, `float`, `double`, `string`, and any `enum`.
- `SettingsStore<TConfig>` additional API:
  - `CopyValuesTo(target, setWithoutNotifying)` — mirrors all parameter values into another store instance.
  - `AddListenerToAll(Action)` / `AddListenerToAll<T>(Action<T?,T?>)` — subscribes to `ValueChanged` on all (or type-matched) parameters; listeners are auto-removed on `Dispose`.
  - `RemoveListenerFromAll(Action)` / `RemoveListenerFromAll<T>(Action<T?,T?>)` — manually unsubscribes listeners.
  - `IDisposable` — always dispose `SettingsStore` to clean up event subscriptions.
- Persistence uses `System.Text.Json` with camelCase property naming and enums serialized as strings.
- Do not introduce unrelated configuration frameworks.
- Nested settings groups are supported: declare a nested `record` also decorated with `[AutoRegisterSettings]`, `[SettingsPrefix]`, and `[Category]`, then expose it as a `[SettingsParameter]` property on the parent config. `ConfigDrawer` recurses into nested groups automatically.

Example config class demonstrating all major patterns:

````````
using Umbra.SDK.Config;
using System.Text.Json.Serialization;

[AutoRegisterSettings, SettingsPrefix("Example"), Category("Example Plugin")]
public partial record PluginConfig
{
    [SettingsParameter, DisplayName("Enabled"), Description("Is the plugin enabled?")]
    public Parameter<bool> IsEnabled { get; set; } = new(true);

    [SettingsParameter, DisplayName("Hotkey"), Description("The hotkey to activate the plugin.")]
    public Parameter<int> Hotkey { get; set; } = new(70); // F2 by default

    [SettingsParameter("volume"), DisplayName("Volume"), Description("The volume level."), Range(0, 100), Step(1)]
    public Parameter<int> VolumeLevel { get; set; } = new(50);

    [SettingsParameter, DisplayName("Username"), Description("The user's name."), MaxLength(80)]
    public Parameter<string> UserName { get; set; } = new("Player");

    [SettingsParameter, DisplayName("Nested Settings"), Category("Advanced Settings")]
    public Parameter<NestedConfigGroup> NestingExample { get; set; } = new(new());
}

[AutoRegisterSettings, SettingsPrefix("Example.Nested"), Category("Example Plugin/Nested")]
public partial record NestedConfigGroup
{
    [SettingsParameter("advancedFeature"), DisplayName("UseAdvancedFeature"), Description("Enable advanced feature?")]
    public Parameter<bool> UseAdvancedFeature { get; set; } = new(false);

    [SettingsParameter, DisplayName("MaxItems"), Description("Maximum number of items."), Range(1, 100), Step(1)]
    public Parameter<int> MaxItems { get; set; } = new(10);
}
````````

## Settings UI — ConfigDrawer
- `ConfigDrawer<TConfig>` (in `Umbra.SDK.Config.UI`) renders a full ImGui settings panel from a config instance.
- The draw tree is built once at construction via a single reflection pass; `Draw()` walks the pre-built node list with no per-frame reflection.
- Construct with the `TConfig` instance returned by `SettingsStore<TConfig>.Load()` so that `ParameterMetadata` is already populated.
- Call `Draw()` from inside an active ImGui window or child window each frame.
- `ConfigDrawer<TConfig>` is `IDisposable`; dispose it when the settings window is closed or the plugin unloads.
- Custom controls are implemented via `IParameterDrawer` (in `Umbra.SDK.Config.UI`) and applied with `[CustomDrawer<TDrawer>]` on the parameter property.
- For two-column-aware custom controls that participate in the standard label layout, use `ITwoColumnParameterDrawer` with `[TwoColumnCustomDrawer<TDrawer>]`.

## Plugin lifecycle
- Mark the plugin entry point with `[PluginEntryPoint]` and the exit point with `[PluginExitPoint]` (from `REFrameworkNET.Attributes`).
- Initialize `SettingsStore<TConfig>` and `ConfigDrawer<TConfig>` in the entry point; dispose and null both in the exit point.
- Always pass a unique plugin identifier string as `idScope` to `ConfigDrawer<TConfig>` so that all ImGui widget IDs rendered by the drawer are scoped with `ImGui.PushID` / `ImGui.PopID`. This prevents duplicate-ID warnings when multiple plugins render settings panels in the same ImGui window.
- Always call `Save()` before `Dispose()` on the store so the last in-memory values are flushed to disk on unload.
- Null out all static references in the exit point to avoid stale state if the plugin is reloaded in the same process session.

```
    private static ConfigDrawer<PluginConfig>? _drawer;
    private static SettingsStore<PluginConfig>? _store;

    [PluginEntryPoint]
    public static void Load()
    {
        //System.Diagnostics.Debugger.Launch();

        var configPath = GetConfigPath();
        _store = new SettingsStore<PluginConfig>(configPath);
        var config = _store.Load();

        _drawer = new ConfigDrawer<PluginConfig>(config, idScope: "MyPlugin");
    }

    [PluginExitPoint]
    public static void Unload()
    {
        _store?.Save();
        _store?.Dispose();
        _store = null;

        _drawer?.Dispose();
        _drawer = null;
    }
```

## Dependencies and references
- REFramework-related assemblies are referenced from the local `Libs/reframework` layout.
- The project references:
  - `REFramework.NET`
  - generated game bindings such as `application`, `viacore`, and `_System.Private.CoreLib`
  - ImGui support via `Hexa.NET.ImGui`
- Keep solutions compatible with this local reference-based setup unless explicitly asked to change it.

## Coding expectations
- **Enforce the Single Responsibility Principle (SRP) at all times.** Every class, method, and file should have one clearly defined reason to change. Split unrelated concerns into separate types even for small helpers.
- Preserve the existing project style:
  - file-scoped namespaces where already used
  - nullable reference types enabled
  - concise utility-style helpers
- Prefer small, dependency-light solutions suitable for mod/plugin code.
- Avoid adding unnecessary infrastructure such as dependency injection containers, hosted services, or heavyweight abstractions unless explicitly requested.
- **Never use LINQ** (`System.Linq`) anywhere in the codebase. Use explicit `for`/`foreach` loops, manual predicates, and in-place logic instead.
- Whenever code is changed, analyze and update the XML documentation (`/// <summary>`, `/// <remarks>`, `/// <param>`, `/// <returns>`, etc.) on the changed member. Also scan for any other XML doc blocks elsewhere in the codebase that reference or describe the changed type, method, or behavior, and update those too if the change affects their accuracy (e.g. behavior differences, renamed parameters, new constraints, removed overloads).

## Mod/plugin development assumptions
- Treat this repository as game mod/plugin code for RE Engine titles using REFramework.
- Some code may be game-specific; for example, current constants and scripts indicate active development against `re9`.
- Debug workflows may rely on local game install paths and deployment scripts such as `deploy_reframework_deps.bat`.

## When generating code
- Favor examples that integrate with REFramework.NET and ImGui.
- Prefer safe, practical code that can run in-process with the game.
- Avoid suggestions that assume a normal app entry point, service host, or external UI process unless the request explicitly asks for one.