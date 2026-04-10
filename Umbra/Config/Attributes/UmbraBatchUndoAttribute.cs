namespace Umbra.Config.Attributes;

/// <summary>
/// Marks a <see cref="Parameter{T}">Parameter&lt;Action&gt;</see> property as a batch-undo candidate.
/// When a <see cref="ConfigUndoStack{TConfig}"/> is constructed, it automatically wraps the current
/// delegate value with <see cref="ConfigUndoStack{TConfig}.WrapWithBatch"/> so that invoking the
/// action produces a single atomic undo entry instead of one entry per changed parameter.
/// </summary>
/// <remarks>
/// <para>
/// This attribute is read by <see cref="ParameterMetadataReader"/> during
/// <see cref="ConfigStore{TConfig}.Load()"/> and stored as <see cref="ParameterMetadata.BatchUndoLabel"/>.
/// The undo stack consumes the label at construction time to discover and wrap eligible delegates.
/// </para>
/// <para>
/// The wrapping uses <see cref="IParameter.SetValueWithoutNotify"/> so no <see cref="IParameter.ValueChanged"/>
/// event is raised during auto-wrapping.
/// </para>
/// </remarks>
/// <param name="label">The human-readable label shown in the undo toast and stored in the batch record.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UmbraBatchUndoAttribute(string label) : Attribute
{
    /// <summary>
    /// Gets the human-readable label for the batch undo entry produced when the decorated action is invoked.
    /// </summary>
    /// <value>The label displayed in undo toasts and stored in <see cref="ConfigBatchChangeRecord.BatchLabel"/>.</value>
    public string Label { get; } = label;
}
