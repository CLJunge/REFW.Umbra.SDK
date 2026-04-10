# Copilot instructions for REFW.Umbra

## Priority order
When generating or modifying code for this repository:
1. Follow this file first.
2. Use `README.md` for the current architecture and runtime model.
3. Prefer patterns from the same subsystem and from `Umbra.SamplePlugin/` before inventing new ones.
4. When guidance is missing, prioritize consistency with existing code over external best practices or newer patterns.

## Project overview
- `Umbra` is a support library for managed `REFramework.NET` plugins and mods for RE Engine games.
- Code runs **inside the game process** through the REFramework managed host, not as a standalone app or service.
- The solution contains:
  - `Umbra/` - reusable runtime, config, logging, and UI code
  - `Umbra.SamplePlugin/` - the reference implementation for preferred plugin patterns
  - `Umbra.UnitTests/` - MSTest-based unit tests for runtime, config, UI, logging, and lifecycle behavior

## Technology and compatibility
- Target framework: `.NET 10`
- Platform: `x64`
- Nullable reference types: enabled
- UI: `Hexa.NET.ImGui`
- Host/runtime APIs: `REFramework.NET` and generated bindings under `dependencies/reframework`
- Persistence: `Umbra.Config` with `System.Text.Json`
- Do not introduce language or framework features that are inconsistent with the repo's current target/framework setup.

## Architecture and boundaries
- **Enforce SRP aggressively.** Each class, method, and file should have one clear reason to change.
- Keep solutions small, explicit, and dependency-light.
- Do not introduce app-style infrastructure such as DI containers, hosted services, or unrelated abstraction layers.
- Prefer instance-based plugin behavior on a type derived from `UmbraPlugin`.
- Keep REFramework entry points and callbacks on a separate static host class and delegate through `PluginHost<TPlugin>`.
- Use the concrete plugin type as `TPlugin` so single-instance mutex behavior stays stable.

## Code patterns to preserve
- Preserve repository style: file-scoped namespaces, spaces for indentation, private fields as `_camelCase`, concise utility-style C#.
- **Never use LINQ** in this repository.
- Prefer resilient behavior over hard failures because plugin code executes in-process with the game.
- Update affected XML documentation when changing public APIs or externally visible behavior.
- Prefer existing abstractions and utilities before adding new ones.
- **`init` accessor + `readonly` backing field is valid and intentional.** C# 9+ `init` accessors have the same write rights as constructors and may assign `readonly` fields. Do not flag or suggest removing `readonly` from a backing field assigned only in an `init` accessor — this is the standard pattern for validated, immutable-after-construction properties in this codebase.

## Logging, hooks, and runtime state
- Use `PluginLogger` for plugin-scoped logging. Declare it as `private static readonly PluginLogger _log = new("PluginName");` on the plugin class.
- Use the static `Logger` only for shared runtime/bootstrap diagnostics.
- For exceptions in plugin code, prefer `PluginLogger.Exception(...)`.
- `[MethodHook]` methods and REFramework callbacks must be `static`.
- Hooks may run on different threads. For multi-field shared state, use the **swap-instance pattern**: build a new state object, then replace the shared reference in one write.
- Use `private static volatile` for shared callback state when cross-thread visibility matters.
- Always detach or clear static callback state during unload so post-unload callbacks become safe no-ops.

## UI and configuration
- Use **ImGui** for in-game UI. Do not introduce WPF, WinForms, MAUI, Blazor, or ASP.NET UI patterns.
- Prefer `PluginPanel` as the top-level plugin UI when config and live state are shown together.
- Use `ConfigSection<TConfig>` / `LiveStateSection<T>` with `PluginPanel`; use `ConfigDrawer<TConfig>` directly only for config-only surfaces.
- Prefer the existing `Umbra.Config` model:
  - mark config types with `[UmbraAutoRegister]`
  - store values in `Parameter<T>` properties marked with `[UmbraParameter]`
  - load and save through `ConfigStore<TConfig>`
  - use `ConfigSaveController` for config persistence only after `Load()` completes
- For nested config groups, put `[UmbraPrefix("...")]` on the parent property, not the nested type.
- Reuse existing helpers where applicable: `KeyboardInput`, `ImGuiWidgets`, and drawers under `Umbra.UI.Config.Drawers`.
- Optional features should only receive related dependencies when that feature is actually enabled; for undo, the input source should be supplied only when undo is used.

## Testing patterns
- Match the existing `MSTest` style: `[TestClass]`, `[TestMethod]`, and `[DataRow]` where appropriate.
- Follow the established `Arrange` / `Act` / `Assert` structure used across `Umbra.UnitTests`.
- Prefer small, focused tests named by behavior and scenario.
- `Assert.ThrowsException` does not exist here; use `Assert.ThrowsExactly` for exception assertions.
- Use existing test seams and test doubles already present in the repo; do not add unnecessary abstractions just for tests.

## Implementation process and validation
- Fully process the implementation plan; do not stop after partial phase execution.
- After each implementation phase, run a validation step consisting of building the project and running tests in the Release configuration.
- If the build or tests report errors, fix the errors and repeat the validation step until no errors remain.
- Once validation passes, reevaluate the completed phase to ensure every planned item for that phase was fully implemented.
- If the phase is fully implemented, update the implementation plan with the concrete implementation details for that phase.
- After updating the implementation plan, post a short summary of what was completed for that phase.
- When running tests in this repository, use the Release configuration.

## Optional features in `ConfigDrawerOptions` / `ConfigSection<TConfig>`
The following optional features are toggled through `ConfigDrawerOptions` when constructing a `ConfigSection<TConfig>` or `ConfigDrawer<TConfig>`. Each is enabled by supplying a non-null options instance (or, for Transfer, setting `Enabled = true`).

| Feature | Options type | Enabled when |
|---|---|---|
| **Search** | `ConfigSearchOptions` | `ConfigDrawerOptions.Search` is non-null |
| **Transfer** | `ConfigTransferOptions` | `ConfigDrawerOptions.Transfer` is non-null **and** `Enabled` is `true` |
| **Undo** | `ConfigUndoOptions` | `ConfigDrawerOptions.Undo` is non-null (store must be `ConfigStore<TConfig>`) |
| **Save Controller** | *(auto-created)* | Section is created via `ConfigSection<TConfig>.CreateWithStore` and the store is `IConfigStore<TConfig>` |
| **Toast (Undo)** | `ConfigToastOptions` | `ConfigUndoOptions.Toast` is non-null |

In `DEBUG` builds, `ConfigSectionDebugOverlay` renders a compact status block at the top of every `ConfigSection<TConfig>.Draw()` call listing which of the four primary features (Search, Transfer, Undo, Save Controller) are enabled or disabled. This overlay can be suppressed by passing `enableDebugOverlay: false` to any `ConfigSection<TConfig>` constructor or `CreateWithStore` factory method.

## Key resources
- `README.md` - architecture, runtime model, setup, and usage
- `Umbra.SamplePlugin/` - source of truth for plugin layout, config wiring, deferred save usage, and panel composition
- `Umbra/Config/` - settings registration, persistence, deferred save
- `Umbra/Logging/` - `PluginLogger`, `Logger`, log levels
- `Umbra/UI/` - config UI, panel system, live-state sections, ImGui widgets
- `Umbra/Runtime/` - game detection, managed object resolution, lifecycle helpers
- `scripts/` - setup and local deployment helpers
- `Directory.Build.targets` - Visual Studio local build hooks for dev workflows
