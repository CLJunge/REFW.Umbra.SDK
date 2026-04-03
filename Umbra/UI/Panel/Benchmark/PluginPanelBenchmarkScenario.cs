#if BENCHMARK
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
    /// <summary>
    /// Represents a benchmark scenario where the target panel is collapsed in the ImGui tree.
    /// </summary>
    CollapsedPanel,

    /// <summary>
    /// Represents a benchmark scenario where the target panel is expanded in the ImGui tree,
    /// but isn't interacted with (e.g. no controls are manipulated and no child nodes are expanded).
    /// </summary>
    ExpandedPanel,

    /// <summary>
    /// Represents a benchmark scenario where the target panel is expanded in the ImGui tree and interacted with
    /// (e.g. controls are manipulated and/or child nodes are expanded).
    /// </summary>
    Interactive
}
#endif
