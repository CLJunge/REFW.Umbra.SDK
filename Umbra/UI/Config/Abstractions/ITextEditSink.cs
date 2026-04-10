using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Receives text edit interaction boundaries so config UI subsystems can react to the start and end of a text input gesture.
/// </summary>
internal interface ITextEditSink
{
    /// <summary>
    /// Signals that a text edit interaction has started for <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The text parameter whose interaction has started.</param>
    void BeginTextEdit(IParameter parameter);

    /// <summary>
    /// Signals that a text edit interaction has ended for <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The text parameter whose interaction has ended.</param>
    void EndTextEdit(IParameter parameter);
}
