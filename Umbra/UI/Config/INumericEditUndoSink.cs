using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Receives numeric edit interaction boundaries so undo-enabled config UI can group one slider or drag gesture into one undo record.
/// </summary>
internal interface INumericEditUndoSink
{
    /// <summary>
    /// Signals that a numeric edit interaction has started for <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The numeric parameter whose interaction has started.</param>
    void BeginNumericEdit(IParameter parameter);

    /// <summary>
    /// Signals that a numeric edit interaction has ended for <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The numeric parameter whose interaction has ended.</param>
    void EndNumericEdit(IParameter parameter);
}
