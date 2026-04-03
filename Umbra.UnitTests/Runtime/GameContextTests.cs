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
    /// <summary>
    /// Verifies that <see cref="GameContext.CurrentGame"/> returns the matched game when the
    /// metadata contains an executable name matching the current test host process.
    /// </summary>
    [TestMethod]
    public void CurrentGame_WhenMetadataMatchesCurrentProcess_ReturnsMatchedGame()
    {
        using var scope = new GameContextStateScope();
        scope.SetCurrentGameMetadata(
            new GameMetadata
            {
                CompatibleTarget = REGame.RE4,
                DisplayName = REGame.RE4.GetDisplayName(),
                ExecutableName = Process.GetCurrentProcess().ProcessName
            });

        // Assert
        Assert.AreEqual(REGame.RE4, GameContext.CurrentGame);
    }

    /// <summary>
    /// Verifies that <see cref="GameContext.CurrentGame"/> returns <see cref="REGame.Unknown"/>
    /// when the metadata contains no executable name matching the current process.
    /// </summary>
    [TestMethod]
    public void CurrentGame_WhenMetadataDoesNotMatchCurrentProcess_ReturnsUnknown()
    {
        using var scope = new GameContextStateScope();
        scope.SetCurrentGameMetadata(
            new GameMetadata
            {
                CompatibleTarget = REGame.RE4,
                DisplayName = REGame.RE4.GetDisplayName(),
                ExecutableName = $"unmatched-{Guid.NewGuid():N}"
            });

        // Assert
        Assert.AreEqual(REGame.Unknown, GameContext.CurrentGame);
    }

    private sealed class GameContextStateScope : IDisposable
    {
        private static readonly FieldInfo s_currentGameMetadataField = typeof(GameContext).GetField("_currentGameMetadata", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Failed to find GameContext metadata field.");

        private readonly GameMetadata? _originalMetadata;

        public GameContextStateScope()
        {
            _originalMetadata = (GameMetadata?)s_currentGameMetadataField.GetValue(null);
        }

        public void SetCurrentGameMetadata(GameMetadata metadata)
        {
            s_currentGameMetadataField.SetValue(null, metadata);
        }

        public void Dispose()
        {
            s_currentGameMetadataField.SetValue(null, _originalMetadata);
        }
    }
}
