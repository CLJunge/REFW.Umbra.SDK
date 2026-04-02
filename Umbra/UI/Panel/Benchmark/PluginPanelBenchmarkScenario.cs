using Umbra.UI.Panel;

namespace Umbra.UI.Panel.Benchmark;

/// <summary>
/// Labels the intended expansion state for a <see cref="PluginPanel"/> benchmark run.
/// </summary>
/// <remarks>
/// The selected value is written into exported benchmark artifacts so later analysis can separate
/// collapsed, expanded, and interactive measurements even though the concrete tree-node state is
/// still prepared interactively inside the ImGui runtime.
/// </remarks>
public enum PluginPanelBenchmarkScenario
{
    CollapsedPanel,
    ExpandedPanel,
    Interactive
}
