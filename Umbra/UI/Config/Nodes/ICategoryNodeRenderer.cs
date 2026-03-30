namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level rendering operations required by <see cref="CategoryNode"/>.
/// </summary>
/// <remarks>
/// This seam isolates category-node control flow from native ImGui calls so unit tests can verify
/// header/tree behavior and indent balancing without requiring an active ImGui frame.
/// </remarks>
internal interface ICategoryNodeRenderer
{
    /// <summary>
    /// Applies indentation before drawing the category and its children.
    /// </summary>
    /// <param name="amount">The indentation width in pixels, or <c>0</c> for the host default.</param>
    void Indent(float amount);

    /// <summary>
    /// Removes indentation after the category has been drawn.
    /// </summary>
    /// <param name="amount">The indentation width in pixels, or <c>0</c> for the host default.</param>
    void Unindent(float amount);

    /// <summary>
    /// Renders the non-collapsible category header.
    /// </summary>
    /// <param name="label">The category label to display.</param>
    void SeparatorText(string label);

    /// <summary>
    /// Renders the category as a collapsible tree node.
    /// </summary>
    /// <param name="label">The category label to display.</param>
    /// <param name="defaultOpen">Whether the tree node should default to its expanded state.</param>
    /// <returns><see langword="true"/> when the node is open and children should be drawn.</returns>
    bool TreeNode(string label, bool defaultOpen);

    /// <summary>
    /// Pops the current tree node after its children have been drawn.
    /// </summary>
    void TreePop();
}
