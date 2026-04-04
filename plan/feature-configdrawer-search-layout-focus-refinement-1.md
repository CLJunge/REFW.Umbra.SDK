---
goal: Refine ConfigDrawer search UI layout to fill remaining row width and focus matched controls after search navigation
version: 1.0
date_created: 2026-04-04
last_updated: 2026-04-04
owner: GitHub Copilot
status: Planned
tags: [feature, ui, config, search, layout, focus, optimization]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

This plan refines the completed `ConfigDrawer<TConfig>` search feature so the search input fills the remaining horizontal width on the same row as the `Previous` and `Next` buttons, and focused search results transfer keyboard focus to the matched control so users can interact immediately. The refinement must keep the current search/filter/highlight/navigation behavior intact, remain efficient in-process, and recompute search-row layout only when first drawn or when the available row width changes.

## 1. Requirements & Constraints

- **REQ-001**: Keep the search navigation buttons on the same row as the search box.
- **REQ-002**: Make the search input fill only the remaining horizontal width after the button widths and item spacing are accounted for.
- **REQ-003**: Cache the computed search input width per `ConfigDrawer<TConfig>` instance.
- **REQ-004**: Recompute the cached search input width only on first draw and when the available row width changes.
- **REQ-005**: Keep the existing search query, highlight, force-open, and scroll-into-view behavior unchanged unless explicitly refined by this plan.
- **REQ-006**: When a focused search result is rendered, attempt to move keyboard focus to the first interactive control in that row so the user can interact immediately.
- **REQ-007**: Keyboard focus transfer must happen both when a search query first focuses a result and when the user navigates with previous/next buttons.
- **REQ-008**: Focus transfer must remain resilient for rows whose draw actions emit labels before the interactive control.
- **REQ-009**: The refinement must remain testable without an active ImGui frame.
- **REQ-010**: Public API shape should remain unchanged unless an additional narrow internal seam is required for rendering or focus.
- **CON-001**: Follow `.github/copilot-instructions.md` and keep the solution SRP-focused and dependency-light.
- **CON-002**: Do not introduce LINQ.
- **CON-003**: Preserve the current `ConfigDrawer<TConfig>` / `ConfigSection<TConfig>` architecture and the existing flat-search-index model.
- **CON-004**: Do not recompute search-row button/input widths every frame when the row width is unchanged.
- **PAT-001**: Reuse and extend existing rendering seams such as `IConfigDrawerRenderer` and `IParameterNodeRenderer` rather than calling new ImGui APIs directly from tests.
- **PAT-002**: Preserve current constructor overloads and current default behavior for callers that do not opt into search.
- **PAT-003**: Prefer per-drawer local cached layout state over static shared caches.

## 2. Implementation Steps

### Implementation Phase 1

- **GOAL-001**: Add a focused per-drawer layout cache for the search row.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Umbra/UI/Config/ConfigDrawerSearchLayoutState.cs`. Store `float LastAvailableWidth`, `float PreviousButtonWidth`, `float NextButtonWidth`, `float SearchInputWidth`, and an initialization flag. |  |  |
| TASK-002 | Extend `Umbra/UI/Config/Abstractions/IConfigDrawerRenderer.cs` with narrow layout-measurement operations required by the drawer search row: available row width, item spacing, text/button width measurement, and next-item-width assignment. Keep the seam limited to what `ConfigDrawer<TConfig>` uses directly. |  |  |
| TASK-003 | Implement the new renderer operations in `Umbra/UI/Config/Rendering/ImGuiConfigRenderContext.cs` using the equivalent ImGui calls. |  |  |
| TASK-004 | Update `Umbra.UnitTests/UI/Config/TestConfigDrawerScope.cs` to record the new layout operations so the search-row sizing behavior can be validated without ImGui. |  |  |

### Implementation Phase 2

- **GOAL-002**: Render the search row using cached remaining-width calculation.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Update `Umbra/UI/Config/ConfigDrawer.cs` to own a `ConfigDrawerSearchLayoutState` only when search is enabled. |  |  |
| TASK-006 | Refactor `ConfigDrawer.DrawSearchBar()` so it lays out the row as: `Previous` button, `Next` button, then search input on the same row. Use `SetNextItemWidth(...)` so the input fills the remaining width after cached button widths and spacing are subtracted. |  |  |
| TASK-007 | Recompute the cached search-row widths only when the drawer is first drawn or when the current available row width differs from the cached width. Clamp the computed input width to a safe minimum greater than zero. |  |  |
| TASK-008 | Keep the existing navigation behavior intact while reordering the visual layout so button-click handling still updates `ConfigDrawerSearchState` deterministically. |  |  |

### Implementation Phase 3

- **GOAL-003**: Transfer keyboard focus to the focused search result control.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Extend `Umbra/UI/Config/Nodes/Abstractions/IParameterNodeRenderer.cs` with a narrow keyboard-focus operation, such as `SetKeyboardFocusHere()`. |  |  |
| TASK-010 | Implement that focus operation in `Umbra/UI/Config/Rendering/ImGuiConfigRenderContext.cs` using the appropriate ImGui call. |  |  |
| TASK-011 | Update `Umbra.UnitTests/UI/Config/Nodes/TestParameterNodeRenderer.cs` and `Umbra.UnitTests/UI/Config/TestConfigDrawerScope.cs` to record focus requests. |  |  |
| TASK-012 | Update `Umbra/UI/Config/Nodes/ParameterNode.cs` so when the node is the focused search result and is about to render, it requests keyboard focus for the upcoming interactive control exactly once per pending focus transfer. Keep scroll-into-view and highlight behavior intact. |  |  |
| TASK-013 | Extend `Umbra/UI/Config/ConfigDrawerSearchState.cs` or `Umbra/UI/Config/ConfigSearchRenderState.cs` with a dedicated pending-focus result identifier so focus transfer is explicit and can be consumed separately from scroll transfer if needed. |  |  |

### Implementation Phase 4

- **GOAL-004**: Validate layout caching, remaining-width behavior, and focused-control handoff.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Add `ConfigDrawer<TConfig>` tests in `Umbra.UnitTests/UI/Config/ConfigDrawerTests.cs` verifying that the search row keeps the buttons on the same row and assigns the remaining width to the input. |  |  |
| TASK-015 | Add `ConfigDrawer<TConfig>` tests verifying that the cached search-row width is reused when the available width is unchanged and recomputed when the available width changes. |  |  |
| TASK-016 | Add `ParameterNode` tests in `Umbra.UnitTests/UI/Config/Nodes/ParameterNodeTests.cs` verifying that focused results request keyboard focus exactly once when they consume a pending focus transfer. |  |  |
| TASK-017 | Add drawer-level tests verifying that initial query match focus and previous/next navigation both result in a focus request for the focused node. |  |  |
| TASK-018 | Run `dotnet build REFW.Umbra.slnx -c Debug` and `dotnet test Umbra.UnitTests/Umbra.UnitTests.csproj -c Debug` before marking the refinement complete. |  |  |

## 3. Alternatives

- **ALT-001**: Put the search box on its own full-width row and move buttons below it. Not chosen because the requested layout keeps the buttons on the same row.
- **ALT-002**: Recompute the input width every draw call. Not chosen because the requested behavior explicitly prefers cached width calculation that updates only when needed.
- **ALT-003**: Focus the result row visually but do not move keyboard focus to the control. Not chosen because the user explicitly wants immediate interaction with the matched control.
- **ALT-004**: Push focus logic into each individual control builder instead of `ParameterNode`. Not chosen because focused-result selection already lives at the node/search layer, and the first interactive control in the row should be handled consistently at that boundary.

## 4. Dependencies

- **DEP-001**: `Umbra/UI/Config/ConfigDrawer.cs`
- **DEP-002**: `Umbra/UI/Config/Abstractions/IConfigDrawerRenderer.cs`
- **DEP-003**: `Umbra/UI/Config/Rendering/ImGuiConfigRenderContext.cs`
- **DEP-004**: `Umbra/UI/Config/ConfigDrawerSearchState.cs`
- **DEP-005**: `Umbra/UI/Config/ConfigSearchRenderState.cs`
- **DEP-006**: `Umbra/UI/Config/Nodes/ParameterNode.cs`
- **DEP-007**: `Umbra/UI/Config/Nodes/Abstractions/IParameterNodeRenderer.cs`
- **DEP-008**: `Umbra.UnitTests/UI/Config/TestConfigDrawerScope.cs`
- **DEP-009**: `Umbra.UnitTests/UI/Config/TestDrawNode.cs`
- **DEP-010**: `Umbra.UnitTests/UI/Config/Nodes/TestParameterNodeRenderer.cs`

## 5. Files

- **FILE-001**: `Umbra/UI/Config/ConfigDrawer.cs` - search-row layout cache ownership, remaining-width layout, and navigation/focus refinement.
- **FILE-002**: `Umbra/UI/Config/ConfigDrawerSearchLayoutState.cs` - new per-drawer cached search-row layout state.
- **FILE-003**: `Umbra/UI/Config/Abstractions/IConfigDrawerRenderer.cs` - additional narrow search-row measurement and width APIs.
- **FILE-004**: `Umbra/UI/Config/Rendering/ImGuiConfigRenderContext.cs` - ImGui implementations for width measurement, next-item width, and keyboard focus handoff.
- **FILE-005**: `Umbra/UI/Config/ConfigDrawerSearchState.cs` - pending focus transfer state if needed.
- **FILE-006**: `Umbra/UI/Config/ConfigSearchRenderState.cs` - expose pending focus semantics to searchable nodes if needed.
- **FILE-007**: `Umbra/UI/Config/Nodes/Abstractions/IParameterNodeRenderer.cs` - keyboard focus seam.
- **FILE-008**: `Umbra/UI/Config/Nodes/ParameterNode.cs` - consume pending focus requests for the focused result.
- **FILE-009**: `Umbra.UnitTests/UI/Config/TestConfigDrawerScope.cs` - record layout-width and focus operations.
- **FILE-010**: `Umbra.UnitTests/UI/Config/Nodes/TestParameterNodeRenderer.cs` - record focus requests.
- **FILE-011**: `Umbra.UnitTests/UI/Config/ConfigDrawerTests.cs` - search-row layout and navigation/focus integration coverage.
- **FILE-012**: `Umbra.UnitTests/UI/Config/Nodes/ParameterNodeTests.cs` - focused-control handoff coverage.

## 6. Testing

- **TEST-001**: Verify the search row keeps `Previous` and `Next` on the same row as the search input.
- **TEST-002**: Verify the search input receives the remaining row width after button widths and spacing are reserved.
- **TEST-003**: Verify the cached search-row width is reused when available width is unchanged.
- **TEST-004**: Verify the cached search-row width is recomputed when available width changes.
- **TEST-005**: Verify focused results request keyboard focus for the matched control exactly once per pending focus transfer.
- **TEST-006**: Verify initial search-result focus triggers both scroll and focus requests.
- **TEST-007**: Verify previous/next navigation triggers focus transfer for the newly focused result.
- **TEST-008**: Verify all existing search-related tests remain green after the refinement.

## 7. Risks & Assumptions

- **RISK-001**: If button-width measurement is not cached consistently, the drawer may still recompute layout more often than intended.
- **RISK-002**: If focus is requested too early or too late relative to the row draw action, the wrong widget may receive focus or focus may be lost entirely.
- **RISK-003**: Some composite or custom controls may not react identically to first-widget keyboard focus, even though the common row behavior improves.
- **ASSUMPTION-001**: The `Previous` and `Next` labels remain stable, so their measured widths can be cached safely per drawer instance.
- **ASSUMPTION-002**: The first interactive widget emitted by a parameter row is the correct focus target for the vast majority of Umbra config controls.
- **ASSUMPTION-003**: Available search-row width changes only when the host window or content region width changes, which is sufficient for cached recomputation.

## 8. Related Specifications / Further Reading

- `.github/copilot-instructions.md`
- `README.md`
- `Umbra/UI/Config/ConfigDrawer.cs`
- `Umbra/UI/Config/Nodes/ParameterNode.cs`
- `Umbra/UI/Config/Rendering/ImGuiConfigRenderContext.cs`
