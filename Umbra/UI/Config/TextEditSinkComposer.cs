using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Composes zero or more <see cref="ITextEditSink"/> instances into a single sink that
/// forwards interaction boundary events to all of them.
/// </summary>
internal static class TextEditSinkComposer
{
    /// <summary>
    /// Returns a single <see cref="ITextEditSink"/> that forwards events to both sinks,
    /// or returns the non-null one when only one is supplied.
    /// </summary>
    /// <param name="first">The first sink, or <see langword="null"/>.</param>
    /// <param name="second">The second sink, or <see langword="null"/>.</param>
    /// <returns>
    /// A composite sink when both are non-null, the non-null one when exactly one is supplied,
    /// or <see langword="null"/> when both are null.
    /// </returns>
    internal static ITextEditSink? Compose(ITextEditSink? first, ITextEditSink? second)
    {
        if (first is null) return second;
        if (second is null) return first;
        return new CompositeSink(first, second);
    }

    private sealed class CompositeSink(ITextEditSink first, ITextEditSink second) : ITextEditSink
    {
        public void BeginTextEdit(IParameter parameter)
        {
            first.BeginTextEdit(parameter);
            second.BeginTextEdit(parameter);
        }

        public void EndTextEdit(IParameter parameter)
        {
            first.EndTextEdit(parameter);
            second.EndTextEdit(parameter);
        }
    }
}
