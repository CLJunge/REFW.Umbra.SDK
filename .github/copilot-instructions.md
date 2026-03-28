# Copilot instructions for REFW.Umbra

## Project context
- `Umbra` is a support library for REFramework mod/plugin development.
- The primary runtime target is **REFramework**, specifically **REFramework.NET**.
- Code in this repository is intended to run inside the game process through the REFramework managed plugin environment, not as a standalone desktop or web application.
- The workspace targets `.NET 10` and `x64`.

## REFramework-specific guidance
- Prefer patterns that work inside the REFramework managed plugin host.
- Use `REFramework.NET` APIs when interacting with the host environment.
- For logging in plugin code, use `Umbra.Logging.PluginLogger` — an **instance class** that forwards to `REFrameworkNET.API.LogInfo`, `API.LogWarning`, and `API.LogError`.
  - Declare the logger as a `private static readonly PluginLogger _log = new("PluginName");` field on the plugin class. Never set it in the entry point — initialise it inline so it is always available and never shared with other plugins.
  - Do **not** use shared static logging configuration for prefixes or minimum levels. All managed plugins load into the same AppDomain; any shared static prefix written by one plugin would silently overwrite every other plugin's prefix.
  - `PluginLogger` exposes `Prefix`, `PrefixFormat`, and `MinLevel` as instance properties, fully isolated per plugin.
  - All `PluginLogger` methods are exception-safe and silently suppress errors to avoid disrupting the game process. Formatted overloads (`...(string format, params object[] args)`) also swallow any exception thrown by `string.Format`, so an invalid format string or mismatched arguments causes the message to be silently discarded rather than propagated.
  - `_log.Exception(Exception ex, string message)` logs a context message followed by the exception type, message, and stack trace via `API.LogError`. Use this — **not** `_log.Error` — when logging exceptions.
  - SDK internals should use the static `Logger` facade for raw, unconditional logging with no per-plugin prefix or minimum level.
  - `Logger.Enabled = false`, `Logger.DisableAll()`, or `using var _ = Logger.Suppress();` silences all Umbra logging, including `PluginLogger`, which is useful for benchmarks and tests.
- Assume game-facing code may run in a constrained plugin environment where resilience is preferred over hard failures.
- When introducing replacement APIs in this codebase, prefer fully implemented replacements over inheriting from obsolete types so old types can be removed cleanly later.

## Thread safety — hooks and callbacks

- `[MethodHook]` methods and REFramework callbacks (e.g. ImGui pre-draw) **must be static**. This means any dedicated hook class uses a `private static` field to hold a reference to the state it writes.
- A `private static` field is scoped to its declaring type, which is scoped to its assembly. All managed plugins share one `AppDomain`, but each plugin assembly has its own type identity — cross-plugin contamination of a hook class's static field is not possible without explicit reflection.
- Hooks fire on whichever thread the hooked method runs on. ImGui callbacks fire on the game's render/update thread. For most RE Engine camera and gameplay hooks these are the same thread, but this is not guaranteed.
- **Use the swap-instance pattern** when a hook or callback writes multiple fields to a shared state object. Build a fresh instance, populate it fully, then replace the reference in a single volatile write. This ensures readers always see a consistent snapshot. The static field holding the reference should be volatile. Always null it in `[PluginExitPoint]` so post-unload hook calls are no-ops. Example:
```csharp
internal static class FovHooks
{
    // volatile ensures the reference write is visible across threads without a lock.
    private static volatile CameraState? _target;

    internal static void Attach(CameraState target) => _target = target;
    internal static void Detach()                   => _target = null;

    [MethodHook(typeof(app.PlayerCameraFOVCalc), nameof(app.PlayerCameraFOVCalc.getFOV), MethodHookType.Post)]
    public static void OnGetFOVPost(ref ulong retval)
    {
        if (_target is null) return;

        float fov = BitConverter.UInt32BitsToSingle((uint)(retval & 0xFFFFFFFF));

        // Swap-instance: build the new snapshot, then replace the reference atomically.
        _target = new CameraState { Fov = fov, Mode = _target.Mode };
    }
}
```
- Always call `Detach()` (or equivalent null-assignment) in `[PluginExitPoint]` so any hook that fires after unload becomes a safe no-op.
- The same pattern applies to ImGui pre-draw or any other static callback that writes shared state consumed by another part of the plugin.

## UI and input
- UI should be implemented with **ImGui**, using `Hexa.NET.ImGui`.
- Do not suggest WPF, WinForms, MAUI, Blazor, or ASP.NET for in-game UI.
- For hotkeys and keyboard capture, use `Umbra.Input.KeyboardInput`.
  - `KeyboardInput` provides: `TryCaptureKeyboardKey(out int)`, `GetKeyName(int)`, `IsValidKey(int)`, and modifier-state properties `IsCtrlHeld`, `IsShiftHeld`, `IsAltHeld`.
  - Hotkey values are stored as `int` (an `ImGuiKey` cast to `int`).
- For reusable ImGui helpers, use `Umbra.UI.ImGuiWidgets`.
  - `ImGuiWidgets` currently provides `DrawHelpMarker(string description)`.
- For config-backed hotkey controls, prefer the built-in drawers in `Umbra.UI.Config.Drawers` such as `HotkeyDrawer` and `TwoColumnHotkeyDrawer`.

## Settings/configuration
- Prefer the existing configuration model in `Umbra.Config`.
- Settings flow:
  1. Create a config class with `[UmbraAutoRegisterSettings]` (and optionally `[UmbraSettingsPrefix("...")]` and/or `[UmbraCategory("...")]` at the class level).
  2. Declare leaf settings as `Parameter<T>` properties decorated with `[UmbraSettingsParameter]` (optionally with a `keyOverride` string).
  3. Declare nested settings groups as regular properties decorated with `[UmbraSettingsParameter]`, where the nested type is also decorated with `[UmbraAutoRegisterSettings]`.
  4. Instantiate `SettingsStore<TConfig>` with the absolute path to the JSON file.
  5. Call `settingsStore.Load()` to obtain a populated config instance; call `settingsStore.Save()` to persist current values.
- `SettingsStore<TConfig>` requires `TConfig : class, new()`. `record` types satisfy this constraint and are the preferred style for config classes.
- Key derivation: dot-separated; the prefix from `[SettingsPrefix]` is prepended to each property name (camelCased), unless a `keyOverride` is provided on `[SettingsParameter]`.
- For nested settings groups, place `[SettingsPrefix("...")]` on the **parent property** that exposes the nested group — this is the preferred approach. Placing `[SettingsPrefix]` on the nested type itself is supported for backwards compatibility but is no longer recommended for new code. `SettingsRegistrar` resolves the prefix property-first: the property attribute wins; the type attribute is the fallback. Do not repeat the full parent prefix on the nested type.
- Metadata attributes available from `Umbra.Config.Attributes`:
  - `[DisplayName("...")]` — human-readable UI label.
  - `[Description("...")]` — tooltip or help text shown via a `(?)` help marker.
  - `[Category("...")]` — groups related parameters in the UI; can be placed on the class or a specific property.
  - `[Range(min, max)]` — numeric bounds; when present the control renders as a slider instead of a drag input.
  - `[Step(step)]` — drag speed for unconstrained numeric controls; also infers the fallback float format's decimal precision.
  - `[Format("...")]` — printf-style format string override for numeric controls (e.g. `"%.0f°"`, `"%d px"`). Overrides the `[Step]`-derived format.
  - `[MaxLength(uint)]` — maximum character count for `string` input fields (default `256`).
  - `[SpacingBefore(count = 1)]` — inserts one or more `ImGui.Spacing()` calls **above** the control.
  - `[SpacingAfter(count = 1)]` — inserts one or more `ImGui.Spacing()` calls **below** the control.
  - `[Indent(amount = 0f)]` — indents the control (or all controls in a group class) using `ImGui.Indent`/`Unindent`; `0` uses ImGui's default spacing. Can be applied at the class or property level.
  - `[HideIf<T>("MemberName")]` — hides the control while the named `bool` member on the same config class is `true`.
  - `[HideIf<T>("MemberName", value)]` — hides the control while the named member equals `value`.
  - `[CustomDrawer<TDrawer>]` — renders the control using a custom `IParameterDrawer` implementation instead of the default; `TDrawer` must implement `IParameterDrawer` and have a public parameterless constructor.
  - `[TwoColumnCustomDrawer<TDrawer>]` — renders the control using an `ITwoColumnParameterDrawer` implementation that participates in the standard two-column label layout.
- Supported `Parameter<T>` value types for JSON persistence: `bool`, `int`, `float`, `double`, `string`, and any `enum`.
- `SettingsStore<TConfig>` additional API:
  - `CopyValuesTo(target, setWithoutNotifying)` — mirrors all parameter values into another store instance.
  - `AddListenerToAll(Action)` / `AddListenerToAll<T>(Action<T?,T?>)` — subscribes to `ValueChanged` on all (or type-matched) parameters; listeners are auto-removed on `Dispose`.
  - `RemoveListenerFromAll(Action)` / `RemoveListenerFromAll<T>(Action<T?,T?>)` — manually unsubscribes listeners.
  - `IDisposable` — always dispose `SettingsStore` to clean up event subscriptions.
- Persistence uses `System.Text.Json` with camelCase property naming and enums serialized as strings.
- Do not introduce unrelated configuration frameworks.
- Nested settings groups are supported: declare a nested `record` decorated with `[UmbraAutoRegisterSettings]` (and optionally `[UmbraCategory]`), then expose it as a `[UmbraSettingsParameter]` property on the parent config. Place `[UmbraSettingsPrefix("...")]` on the **property**, not the nested type. `ConfigDrawer` recurses into nested groups automatically.

Example config class demonstrating the current nested-settings pattern:

````````
using Umbra.Config;
using Umbra.Config.Attributes;

[UmbraAutoRegisterSettings, UmbraSettingsPrefix("example"), UmbraCategory("Example Plugin")]
public partial record PluginConfig
{
    [UmbraSettingsParameter, UmbraDisplayName("Enabled"), UmbraDescription("Is the plugin enabled?")]
    public Parameter<bool> IsEnabled { get; set; } = new(true);

    [UmbraSettingsParameter, UmbraDisplayName("Hotkey"), UmbraDescription("The hotkey to activate the plugin.")]
    public Parameter<int> Hotkey { get; set; } = new(70); // F2 by default

    [UmbraSettingsParameter("volume"), UmbraDisplayName("Volume"), UmbraDescription("The volume level."), UmbraRange(0, 100), UmbraStep(1)]
    public Parameter<int> VolumeLevel { get; set; } = new(50);

    [UmbraSettingsParameter, UmbraDisplayName("Username"), UmbraDescription("The user's name."), UmbraMaxLength(80)]
    public Parameter<string> UserName { get; set; } = new("Player");

    // Preferred: [UmbraSettingsPrefix] on the property, not on NestedConfigGroup itself.
    [UmbraSettingsParameter, UmbraSettingsPrefix("nested"), UmbraCategory("Advanced Settings")]
    public NestedConfigGroup NestingExample { get; set; } = new();
}

[UmbraAutoRegisterSettings]
public partial record NestedConfigGroup
{
    [UmbraSettingsParameter("advancedFeature"), UmbraDisplayName("UseAdvancedFeature"), UmbraDescription("Enable advanced feature?")]
    public Parameter<bool> UseAdvancedFeature { get; set; } = new(false);

    [UmbraSettingsParameter, UmbraDisplayName("MaxItems"), UmbraDescription("Maximum number of items."), UmbraRange(1, 100), UmbraStep(1)]
    public Parameter<int> MaxItems { get; set; } = new(10);
}

## Settings UI — ConfigDrawer
- `ConfigDrawer<TConfig>` (in `Umbra.UI.Config`) renders a full ImGui settings panel from a config instance.
- The draw tree is built once at construction via a single reflection pass; `Draw()` walks the pre-built node list with no per-frame reflection.
- Construct with the `TConfig` instance returned by `SettingsStore<TConfig>.Load()` so that `ParameterMetadata` is already populated.
- Call `Draw()` from inside an active ImGui window or child window each frame.
- `ConfigDrawer<TConfig>` is `IDisposable`; dispose it when the settings window is closed or the plugin unloads.
- Custom controls are implemented via `IParameterDrawer` (in `Umbra.UI.Config.Drawers`) and applied with `[CustomDrawer<TDrawer>]` on the parameter property.
- For two-column-aware custom controls that participate in the standard label layout, use `ITwoColumnParameterDrawer` with `[TwoColumnCustomDrawer<TDrawer>]`.
- To render an entire nested configuration group with a fully custom ImGui layout, apply `[NestedGroupDrawer<TDrawer>]` to the **parent property** that exposes the nested group, and implement `INestedGroupDrawer<T>` in the drawer class:
  - `[NestedGroupDrawer<TDrawer>]` is declared on `AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct`. Placing it on the parent property is the **preferred** approach. Applying it to the nested `record`/`class` is supported for backward compatibility only and acts as a fallback when the property carries no drawer attribute.
  - `TDrawer` must implement `INestedGroupDrawer<T>` (where `T` is the nested group type) and have a public parameterless constructor.
  - When `ConfigDrawer` encounters a property that carries the attribute (or whose type declares it as a fallback), it instantiates the drawer once at build time and calls `Draw(groupInstance)` each frame instead of recursing into the class's individual parameters.
  - The drawer has full ImGui layout control; no label, column alignment, or section header is emitted by the factory.
  - Property-level attributes `[Category]`, `[CollapseAsTree]`, `[SpacingBefore]`, `[SpacingAfter]`, and `[HideIf]` on the **parent property** are still honoured around the drawer output.
  - `INestedGroupDrawer<T>` extends `IDisposable`. The default `Dispose` implementation calls `GC.SuppressFinalize`; override it when the drawer holds resources that must be released on plugin unload.
  - Nested group properties on the parent config should be regular `[SettingsParameter]` object properties; inside the nested group itself, individual persisted values remain `Parameter<T>` properties.

```csharp
// Nested group type: no [NestedGroupDrawer<>] here when using the preferred property-level placement.
[AutoRegisterSettings]
[Category("My Group")]
[CollapseAsTree]
public record MyGroup
{
    [SettingsParameter]
    public Parameter<int> Value { get; set; } = new(42);
}

// Parent config property: apply [NestedGroupDrawer<>] here (preferred placement).
[SettingsParameter]
[SettingsPrefix("myGroup")]
[NestedGroupDrawer<MyGroupDrawer>]
public MyGroup Group { get; set; } = new();

// Custom drawer: full ImGui layout control for the group.
internal class MyGroupDrawer : INestedGroupDrawer<MyGroup>
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

## Plugin panel system

`PluginPanel` (in `Umbra.UI.Panel`) is the recommended top-level UI type for plugins. It composes an ordered list of `IPanelSection` instances under a shared ImGui ID scope and owns their lifetimes. Use `ConfigDrawer<TConfig>` directly only when the plugin needs a settings panel with no live state.

**Section types:**
- `ConfigSection<TConfig>` — wraps `ConfigDrawer<TConfig>` as a panel section. Accepts an optional `idScope` that defaults to `typeof(TConfig).FullName` and falls back to the type name.
- `LiveStateSection<T>` — renders live game state via an `ILiveStateSectionDrawer<T>` declared on the state type. Accepts an optional instance (for hook-written state the plugin owns) or constructs one internally.

**Declaring a live state drawer:**

Apply `[LiveStateSectionDrawer<TDrawer>]` to the state class, not the drawer. The state class is a plain mutable POCO whose fields are written by `[MethodHook]` callbacks and read by the drawer each frame. Use the swap-instance pattern for multi-field updates.

```csharp
[LiveStateSectionDrawer<CameraStatusDrawer>]
public sealed class CameraState
{
    public float      Fov  { get; set; }
    public CameraMode Mode { get; set; }
}

internal sealed class CameraStatusDrawer : ILiveStateSectionDrawer<CameraState>
{
    public void Draw(CameraState state)
    {
        ImGui.Text($"FOV: {state.Fov:F1}");
        ImGui.Text($"Mode: {state.Mode}");
    }
}
```

**Hook-to-state wiring:**

The plugin owns the state instance. The hook class holds a `private static volatile` reference to it, set via `Attach`/`Detach`. Always call `Detach()` in `[PluginExitPoint]`.

```csharp
internal static class FovHooks
{
    private static volatile CameraState? _target;

    internal static void Attach(CameraState target) => _target = target;
    internal static void Detach()                   => _target = null;

    [MethodHook(typeof(app.PlayerCameraFOVCalc), nameof(app.PlayerCameraFOVCalc.getFOV), MethodHookType.Post)]
    public static void OnGetFOVPost(ref ulong retval)
    {
        if (_target is null) return;
        float fov = BitConverter.UInt32BitsToSingle((uint)(retval & 0xFFFFFFFF));
        _target = new CameraState { Fov = fov, Mode = _target.Mode };
    }
}
```

## Plugin lifecycle
- Mark the plugin entry point with `[PluginEntryPoint]` and the exit point with `[PluginExitPoint]` (from `REFrameworkNET.Attributes`).
- Construct `PluginPanel` in the entry point with all required sections; dispose and null it in the exit point.
- The `idScope` passed to `PluginPanel` scopes all ImGui widget IDs for the entire panel. Individual sections may optionally push a sub-scope via their own `idScope` parameter.
- Always call `Save()` before `Dispose()` on `SettingsStore` so the last in-memory values are flushed to disk on unload.
- Null out all static references in the exit point to avoid stale state if the plugin is reloaded in the same process session.

```csharp
    private static PluginPanel?                   _panel;
    private static SettingsStore<PluginConfig>?   _store;
    private static CameraState?                   _cameraState;

    [PluginEntryPoint]
    public static void Load()
    {
        var configPath = GetConfigPath();
        _store       = new SettingsStore<PluginConfig>(configPath);
        var config   = _store.Load();

        _cameraState = new CameraState();
        FovHooks.Attach(_cameraState);

        _panel = new PluginPanel("MyPlugin")
            .Add(new LiveStateSection<CameraState>(_cameraState))
            .Add(new ConfigSection<PluginConfig>(config));
    }

    [PluginExitPoint]
    public static void Unload()
    {
        FovHooks.Detach();
        _cameraState = null;

        _store?.Save();
        _store?.Dispose();
        _store = null;

        _panel?.Dispose();
        _panel = null;
    }
```

## Dependencies and references
- REFramework-related assemblies are referenced from the local `dependencies/reframework` layout.
- The project references:
  - `REFramework.NET` from `dependencies/reframework/api`
  - generated game bindings such as `application`, `viacore`, and `_System.Private.CoreLib` from `dependencies/reframework/generated`
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
