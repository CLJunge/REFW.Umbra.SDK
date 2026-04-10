namespace Umbra.UI.Toast;

/// <summary>
/// Identifies the severity of a toast notification displayed by the in-game overlay.
/// </summary>
public enum ToastLevel
{
    /// <summary>
    /// A neutral informational notification.
    /// </summary>
    Info,

    /// <summary>
    /// A positive confirmation notification.
    /// </summary>
    Success,

    /// <summary>
    /// A cautionary notification.
    /// </summary>
    Warning,

    /// <summary>
    /// An error notification.
    /// </summary>
    Error
}
