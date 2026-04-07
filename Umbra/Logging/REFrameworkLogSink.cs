using REFrameworkNET;

namespace Umbra.Logging;

/// <summary>
/// Forwards Umbra log messages to the active REFramework managed host.
/// </summary>
/// <remarks>
/// This is the production sink used for in-process plugin logging. Tests can replace it through <see cref="Logger.SetLogSink(ILogSink)"/> with a sink that does not depend on REFramework host APIs.
/// </remarks>
internal sealed class REFrameworkLogSink : ILogSink
{
    /// <inheritdoc/>
    public void Info(string message) => API.LogInfo(message);

    /// <inheritdoc/>
    /// <remarks>
    /// REFramework does not expose a dedicated debug log level, so debug messages are forwarded to the informational channel.
    /// </remarks>
    public void Debug(string message) => API.LogInfo(message);

    /// <inheritdoc/>
    public void Warning(string message) => API.LogWarning(message);

    /// <inheritdoc/>
    public void Error(string message) => API.LogError(message);
}
