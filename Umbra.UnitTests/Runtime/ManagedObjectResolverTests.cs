namespace Umbra.Runtime.UnitTests;


/// <summary>
/// Unit tests for <see cref="ManagedObjectResolver"/> static utility class.
/// </summary>
[TestClass]
public class ManagedObjectResolverTests
{
    /// <summary>
    /// Verifies that <see cref="ManagedObjectResolver.TryResolve{T}"/> returns false
    /// and sets the output value to null when the address parameter is zero.
    /// This is the early-exit path that does not invoke external dependencies.
    /// </summary>
    /// <param name="address">The address value to test (zero or equivalent).</param>
    [TestMethod]
    [DataRow(0ul)]
    public void TryResolve_AddressIsZero_ReturnsFalseAndSetsValueToNull(ulong address)
    {
        // Arrange
        // (No external dependencies called for zero address)

        // Act
        var result = ManagedObjectResolver.TryResolve<object>(address, out var value);

        // Assert
        Assert.IsFalse(result, "TryResolve should return false when address is zero.");
        Assert.IsNull(value, "Output value should be null when address is zero.");
    }


    /// <summary>
    /// Tests that Resolve returns null when the address is zero.
    /// </summary>
    /// <remarks>
    /// Per TryResolve documentation, address == 0 returns false immediately without
    /// entering the exception-handling path, which should result in Resolve returning null.
    /// </remarks>
    [TestMethod]
    public void Resolve_AddressIsZero_ReturnsNull()
    {
        // Arrange
        ulong address = 0;

        // Act
        var result = ManagedObjectResolver.Resolve<object>(address);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Resolve with a maximum ulong address returns null when the address is invalid.
    /// </summary>
    /// <remarks>
    /// Without a valid REFramework context, ManagedObject.ToManagedObject will throw or return
    /// an object that fails TryAs, resulting in null from Resolve.
    /// </remarks>
    [TestMethod]
    public void Resolve_AddressIsMaxValue_ReturnsNull()
    {
        // Arrange
        var address = ulong.MaxValue;

        // Act
        var result = ManagedObjectResolver.Resolve<object>(address);

        // Assert
        // Without REFramework runtime, this will return null due to exception or invalid cast
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Resolve with a typical non-zero address returns null outside REFramework context.
    /// </summary>
    /// <remarks>
    /// This test documents that arbitrary addresses will fail to resolve without the game runtime.
    /// </remarks>
    [TestMethod]
    public void Resolve_NonZeroAddressWithoutGameContext_ReturnsNull()
    {
        // Arrange
        ulong address = 0x1234567890ABCDEF;

        // Act
        var result = ManagedObjectResolver.Resolve<object>(address);

        // Assert
        // Without REFramework runtime, ManagedObject.ToManagedObject will fail
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Resolve can be called with different class types satisfying the constraint.
    /// </summary>
    /// <remarks>
    /// Verifies the method compiles and executes with various reference types.
    /// </remarks>
    [TestMethod]
    public void Resolve_WithDifferentReferenceTypes_ReturnsNull()
    {
        // Arrange
        ulong address = 0x1000;

        // Act & Assert - verifies method can be called with different types
        var resultObject = ManagedObjectResolver.Resolve<object>(address);
        var resultString = ManagedObjectResolver.Resolve<string>(address);
        var resultException = ManagedObjectResolver.Resolve<Exception>(address);

        Assert.IsNull(resultObject);
        Assert.IsNull(resultString);
        Assert.IsNull(resultException);
    }

}
