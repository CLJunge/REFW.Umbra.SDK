using Umbra.Runtime;

namespace Umbra.UnitTests.Runtime;



/// <summary>
/// Unit tests for <see cref="GameMetadataLoader"/> and the embedded runtime metadata payload.
/// </summary>
[TestClass]
public sealed class GameMetadataLoaderTests
{
    /// <summary>
    /// Verifies that <see cref="GameMetadataLoader.Load"/> returns an entry for every supported
    /// <see cref="REGame"/> value except <see cref="REGame.Unknown"/>.
    /// </summary>
    [TestMethod]
    public void Load_ReturnsMetadataForEverySupportedGameExceptUnknown()
    {
        // Act
        var metadata = GameMetadataLoader.Load();

        // Assert
        foreach (var game in Enum.GetValues<REGame>())
        {
            if (game == REGame.Unknown)
            {
                continue;
            }

            var found = false;
            for (var i = 0; i < metadata.Length; i++)
            {
                if (metadata[i].CompatibleTarget == game)
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, $"Expected metadata for game '{game}'.");
        }
    }

    /// <summary>
    /// Verifies that each embedded metadata display name matches the canonical display name provided
    /// by <see cref="REGameExtensions.GetDisplayName(REGame)"/>.
    /// </summary>
    [TestMethod]
    public void Load_DisplayNamesMatchREGameExtensionValues()
    {
        // Act
        var metadata = GameMetadataLoader.Load();

        // Assert
        for (var i = 0; i < metadata.Length; i++)
        {
            var entry = metadata[i];
            Assert.AreEqual(entry.CompatibleTarget.GetDisplayName(), entry.DisplayName);
        }
    }
}
