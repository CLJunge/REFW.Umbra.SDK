namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Records <see cref="ParameterNode"/> layout operations for unit tests.
/// </summary>
internal sealed class TestParameterNodeRenderer : IParameterNodeRenderer
{
    public int SpacingCount { get; private set; }
    public int IndentCount { get; private set; }
    public int UnindentCount { get; private set; }
    public float? LastIndentAmount { get; private set; }
    public float? LastUnindentAmount { get; private set; }

    public void Spacing() => SpacingCount++;

    public void Indent(float amount)
    {
        IndentCount++;
        LastIndentAmount = amount;
    }

    public void Unindent(float amount)
    {
        UnindentCount++;
        LastUnindentAmount = amount;
    }
}
