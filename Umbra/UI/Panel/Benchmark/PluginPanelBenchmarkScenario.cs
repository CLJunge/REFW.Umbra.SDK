#if BENCHMARK
namespace Umbra.UI.Panel.Benchmark;

/// <summary>
/// Labels the intended interaction state for a <see cref="PluginPanel"/> benchmark run.
/// </summary>
/// <remarks>
/// The selected value is exported with each run so later analysis can separate collapsed, expanded, and interactive measurements even though the concrete ImGui tree state is still driven at runtime.
/// </remarks>
public enum PluginPanelBenchmarkScenario
{
    /// <summary>
    /// The benchmark target panel is expected to remain collapsed.
    /// </summary>
    CollapsedPanel,

    /// <summary>
    /// The benchmark target panel is expected to remain expanded without interactive manipulation.
    /// </summary>
    ExpandedPanel,

    /// <summary>
    /// The benchmark target panel is expected to be expanded and interacted with during measurement.
    /// </summary>
    Interactive
}
#endif
