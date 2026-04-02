namespace Umbra.Config;

/// <summary>
/// Formats a concise debugger display string for <see cref="ParameterMetadata"/>.
/// </summary>
/// <remarks>
/// This type isolates debugger-only presentation logic from <see cref="ParameterMetadata"/>,
/// leaving that type focused on immutable metadata storage.
/// </remarks>
internal static class ParameterMetadataDebuggerDisplayFormatter
{
    /// <summary>
    /// Builds a concise, human-readable summary of the populated fields on <paramref name="metadata"/>.
    /// </summary>
    /// <param name="metadata">The metadata instance to summarize.</param>
    /// <returns>
    /// A comma-separated string of key-value pairs for each populated metadata property, with no
    /// trailing comma or whitespace.
    /// </returns>
    internal static string Format(ParameterMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var parts = new List<string>();

        if (!string.IsNullOrEmpty(metadata.Category)) parts.Add($"Category: {metadata.Category}");
        if (!string.IsNullOrEmpty(metadata.DisplayName)) parts.Add($"DisplayName: {metadata.DisplayName}");
        if (!string.IsNullOrEmpty(metadata.Description)) parts.Add($"Description: {metadata.Description}");
        if (metadata.MaxLength.HasValue) parts.Add($"MaxLength: {metadata.MaxLength.Value}");
        if (metadata.Min.HasValue) parts.Add($"Min: {metadata.Min.Value}");
        if (metadata.Max.HasValue) parts.Add($"Max: {metadata.Max.Value}");
        if (metadata.Step.HasValue) parts.Add($"Step: {metadata.Step.Value}");
        if (!string.IsNullOrEmpty(metadata.Format)) parts.Add($"Format: {metadata.Format}");
        if (metadata.ButtonStyle.HasValue) parts.Add($"ButtonStyle: {metadata.ButtonStyle.Value}");
        if (metadata.CustomButtonColors.HasValue) parts.Add($"CustomButtonColors: N={metadata.CustomButtonColors.Value.Normal} H={metadata.CustomButtonColors.Value.Hovered} A={metadata.CustomButtonColors.Value.Active}");
        if (metadata.ControlWidth.HasValue) parts.Add($"ControlWidth: {metadata.ControlWidth.Value}");
        if (metadata.MultilineLines.HasValue) parts.Add($"MultilineLines: {metadata.MultilineLines.Value}");
        if (metadata.Order.HasValue) parts.Add($"Order: {metadata.Order.Value}");
        if (metadata.SpacingBefore > 0) parts.Add($"SpacingBefore: {metadata.SpacingBefore}");
        if (metadata.SpacingAfter > 0) parts.Add($"SpacingAfter: {metadata.SpacingAfter}");
        if (metadata.Indent.HasValue) parts.Add($"Indent: {metadata.Indent.Value}");
        if (metadata.DrawerType is not null) parts.Add($"Drawer: {metadata.DrawerType.Name}");
        if (metadata.TwoColumnDrawerType is not null) parts.Add($"TwoColumnDrawer: {metadata.TwoColumnDrawerType.Name}");
        if (metadata.HideIf is not null) parts.Add($"HideIf: {metadata.HideIf.MemberName}");
        if (metadata.InferredFloatFormat != "%.2f") parts.Add($"InferredFloatFormat: {metadata.InferredFloatFormat}");
        if (metadata.HiddenLabel is not null) parts.Add($"HiddenLabel: {metadata.HiddenLabel}");

        return string.Join(", ", parts);
    }
}
