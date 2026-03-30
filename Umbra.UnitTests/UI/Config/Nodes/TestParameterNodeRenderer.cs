namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Records <see cref="ParameterNode"/> spacing operations for unit tests.
/// </summary>
internal sealed class TestParameterNodeRenderer : IParameterNodeRenderer
{
    public int SpacingCount { get; private set; }

    public void Spacing() => SpacingCount++;
}
