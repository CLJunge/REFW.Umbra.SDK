using Umbra.Runtime;

namespace Umbra.UnitTests.Runtime;


/// <summary>
/// Unit tests for <see cref="REGameExtensions"/>.
/// </summary>
[TestClass]
public sealed class REGameExtensionsTests
{
    /// <summary>
    /// Verifies that <see cref="REGameExtensions.GetDisplayName(REGame)"/> rejects the unsupported
    /// <see cref="REGame.Unknown"/> value.
    /// </summary>
    [TestMethod]
    public void GetDisplayName_WhenGameIsUnknown_ThrowsArgumentOutOfRangeException() =>
        // Act + Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => REGame.Unknown.GetDisplayName());
}
