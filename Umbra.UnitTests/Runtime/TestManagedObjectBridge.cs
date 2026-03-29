namespace Umbra.Runtime.UnitTests;

/// <summary>
/// Provides deterministic managed-object resolution behavior for unit tests.
/// </summary>
internal sealed class TestManagedObjectBridge : IManagedObjectBridge
{
    private readonly Dictionary<(ulong Address, Type TargetType), object?> _results = [];
    private readonly Dictionary<(ulong Address, Type TargetType), Exception> _exceptions = [];

    /// <summary>
    /// Gets the number of bridge resolution attempts performed by the current test.
    /// </summary>
    public int InvocationCount { get; private set; }

    /// <summary>
    /// Registers a successful or unsuccessful typed resolution result for the specified address.
    /// </summary>
    /// <typeparam name="T">The managed reference type associated with the configured result.</typeparam>
    /// <param name="address">The address that should produce the configured result.</param>
    /// <param name="value">
    /// The value that should be returned for the address. Use <see langword="null"/> to simulate a
    /// type mismatch or unresolved object.
    /// </param>
    public void SetResult<T>(ulong address, T? value) where T : class
        => _results[(address, typeof(T))] = value;

    /// <summary>
    /// Registers an exception that should be thrown when the specified address and target type are
    /// resolved.
    /// </summary>
    /// <typeparam name="T">The managed reference type associated with the configured failure.</typeparam>
    /// <param name="address">The address that should trigger the exception.</param>
    /// <param name="exception">The exception to throw.</param>
    public void SetException<T>(ulong address, Exception exception) where T : class
        => _exceptions[(address, typeof(T))] = exception;

    /// <inheritdoc/>
    public bool TryResolve<T>(ulong address, out T? value) where T : class
    {
        InvocationCount++;

        var key = (address, typeof(T));
        if (_exceptions.TryGetValue(key, out var exception))
            throw exception;

        if (_results.TryGetValue(key, out var result))
        {
            value = (T?)result;
            return value is not null;
        }

        value = null;
        return false;
    }
 }
