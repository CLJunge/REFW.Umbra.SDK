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
    /// Tests that Resolve returns null when the address is ulong.MinValue (which equals zero).
    /// </summary>
    [TestMethod]
    public void Resolve_AddressIsMinValue_ReturnsNull()
    {
        // Arrange
        var address = ulong.MinValue;

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
    /// Tests that Resolve works with different reference types as the generic parameter.
    /// </summary>
    /// <remarks>
    /// Verifies the generic constraint 'where T : class' is satisfied and the method
    /// can be invoked with various reference types.
    /// </remarks>
    [TestMethod]
    [DataRow(0UL)]
    [DataRow(1UL)]
    [DataRow(ulong.MaxValue)]
    public void Resolve_WithStringType_ReturnsNull(ulong address)
    {
        // Arrange
        // (address provided via DataRow)

        // Act
        var result = ManagedObjectResolver.Resolve<string>(address);

        // Assert
        // All addresses should return null without REFramework runtime
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Resolve handles boundary address values correctly.
    /// </summary>
    /// <remarks>
    /// Tests various boundary values for ulong to ensure consistent null handling
    /// outside the game runtime context.
    /// </remarks>
    [TestMethod]
    [DataRow(0UL, DisplayName = "Zero")]
    [DataRow(1UL, DisplayName = "One")]
    [DataRow(0xFFUL, DisplayName = "Byte max")]
    [DataRow(0xFFFFUL, DisplayName = "UShort max")]
    [DataRow(0xFFFFFFFFUL, DisplayName = "UInt max")]
    [DataRow(0xFFFFFFFFFFFFFFFFUL, DisplayName = "ULong max")]
    public void Resolve_BoundaryAddressValues_ReturnsNull(ulong address)
    {
        // Arrange
        // (address provided via DataRow)

        // Act
        var result = ManagedObjectResolver.Resolve<object>(address);

        // Assert
        // Without REFramework runtime, all addresses either fail early (0) or throw/fail cast
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

    /// <summary>
    /// Tests that Resolve returns null for a sequence of sequential addresses.
    /// </summary>
    /// <remarks>
    /// Documents behavior across a range of addresses without game context.
    /// </remarks>
    [TestMethod]
    public void Resolve_SequentialAddresses_AllReturnNull()
    {
        // Arrange & Act & Assert
        for (ulong address = 0; address < 10; address++)
        {
            var result = ManagedObjectResolver.Resolve<object>(address);
            Assert.IsNull(result, $"Expected null for address {address}");
        }
    }

    /// <summary>
    /// Tests that Resolve handles large address values without throwing unexpected exceptions.
    /// </summary>
    [TestMethod]
    public void Resolve_LargeAddressValues_HandlesGracefully()
    {
        // Arrange
        ulong[] largeAddresses =
        {
            0x7FFFFFFFFFFFFFFFUL,
            0x8000000000000000UL,
            0xFFFFFFFFFFFFFFFEUL,
            0xFFFFFFFFFFFFFFFFUL
        };

        // Act & Assert
        foreach (var address in largeAddresses)
        {
            var result = ManagedObjectResolver.Resolve<object>(address);
            Assert.IsNull(result, $"Expected null for large address 0x{address:X}");
        }
    }
}
