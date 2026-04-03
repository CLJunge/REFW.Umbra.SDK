using System.Diagnostics;
using System.Reflection;
using Umbra.Runtime.Models;

namespace Umbra.Runtime.UnitTests;

/// <summary>
/// Unit tests for <see cref="GameContext"/> that validate runtime game detection behavior.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class GameContextTests
{
    private static readonly FieldInfo s_gameMetadataField = typeof(GameContext).GetField("_gameMetadata", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Failed to find GameContext metadata field.");

    private static readonly FieldInfo s_currentGameField = typeof(GameContext).GetField("_currentGame", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Failed to find GameContext current game field.");

    private static readonly MethodInfo s_detectCurrentGameMethod = typeof(GameContext).GetMethod("DetectCurrentGame", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Failed to find GameContext detection method.");

    /// <summary>
    /// Verifies that <see cref="GameContext.GetCurrentGame"/> returns the injected game when the
    /// metadata contains an executable name matching the current test host process.
    /// </summary>
    [TestMethod]
    public void GetCurrentGame_WhenInjectedMetadataMatchesCurrentProcess_ReturnsMatchedGame()
    {
        using var scope = new GameContextStateScope();
        scope.SetMetadata(
            new GameMetadata
            {
                CompatibleTarget = REGame.RE4,
                DisplayName = REGame.RE4.GetDisplayName(),
                ExecutableName = Process.GetCurrentProcess().ProcessName
            });
        scope.SetCurrentGame(REGame.Unknown);

        // Act
        DetectCurrentGame();

        // Assert
        Assert.AreEqual(REGame.RE4, GameContext.GetCurrentGame());
    }

    /// <summary>
    /// Verifies that <see cref="GameContext.GetCurrentGame"/> remains <see cref="REGame.Unknown"/>
    /// when the injected metadata contains no executable name matching the current process.
    /// </summary>
    [TestMethod]
    public void GetCurrentGame_WhenInjectedMetadataDoesNotMatchCurrentProcess_RemainsUnknown()
    {
        using var scope = new GameContextStateScope();
        scope.SetMetadata(
            new GameMetadata
            {
                CompatibleTarget = REGame.RE4,
                DisplayName = REGame.RE4.GetDisplayName(),
                ExecutableName = $"unmatched-{Guid.NewGuid():N}"
            });
        scope.SetCurrentGame(REGame.Unknown);

        // Act
        DetectCurrentGame();

        // Assert
        Assert.AreEqual(REGame.Unknown, GameContext.GetCurrentGame());
    }

    private static Dictionary<REGame, GameMetadata> GetMetadataDictionary()
        => (Dictionary<REGame, GameMetadata>)(s_gameMetadataField.GetValue(null)
            ?? throw new InvalidOperationException("GameContext metadata dictionary was null."));

    private static void DetectCurrentGame()
        => s_detectCurrentGameMethod.Invoke(null, null);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    private sealed class GameContextStateScope : IDisposable
    {
        private readonly Dictionary<REGame, GameMetadata> _originalMetadata = [];
        private readonly REGame _originalCurrentGame = (REGame)(s_currentGameField.GetValue(null)
            ?? throw new InvalidOperationException("GameContext current game value was null."));

        public GameContextStateScope()
        {
            var metadata = GetMetadataDictionary();
            foreach (var entry in metadata)
            {
                _originalMetadata.Add(entry.Key, entry.Value);
            }
        }

        public void SetMetadata(params GameMetadata[] metadata)
        {
            var dictionary = GetMetadataDictionary();
            dictionary.Clear();

            foreach (var entry in metadata)
            {
                dictionary[entry.CompatibleTarget] = entry;
            }
        }

        public void SetCurrentGame(REGame game)
            => s_currentGameField.SetValue(null, game);

        public void Dispose()
        {
            var dictionary = GetMetadataDictionary();
            dictionary.Clear();

            foreach (var entry in _originalMetadata)
            {
                dictionary[entry.Key] = entry.Value;
            }

            SetCurrentGame(_originalCurrentGame);
        }
    }
}
