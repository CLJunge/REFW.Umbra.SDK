using REFrameworkNET;

namespace Umbra.Logging;

/// <summary>
/// Emits Umbra log messages through the REFramework managed host.
/// </summary>
/// <remarks>
/// This is the production sink used at runtime inside the game process. Unit tests can replace it
/// through <see cref="Logger.SetLogSink(ILogSink)"/> with an in-memory sink that does not depend on
/// the REFramework host being active.
/// </remarks>
internal sealed class REFrameworkLogSink : ILogSink
{
    /// <inheritdoc/>
    public void Info(string message) => API.LogInfo(message);

    /// <inheritdoc/>
    public void Warning(string message) => API.LogWarning(message);

    /// <inheritdoc/>
    public void Error(string message) => API.LogError(message);
}
