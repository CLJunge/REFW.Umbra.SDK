using Umbra.Config;


namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Tests for <see cref="ITwoColumnParameterDrawer"/>.
/// </summary>
[TestClass]
public class ITwoColumnParameterDrawerTests
{
    /// <summary>
    /// Tests that calling Dispose on a default implementation does not throw an exception.
    /// </summary>
    [TestMethod]
    public void Dispose_DefaultImplementation_DoesNotThrow()
    {
        // Arrange
        ITwoColumnParameterDrawer drawer = new MinimalDrawerImplementation();

        // Act & Assert
        drawer.Dispose();
    }

    /// <summary>
    /// Tests that calling Dispose multiple times on a default implementation does not throw an exception,
    /// verifying idempotent behavior.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        ITwoColumnParameterDrawer drawer = new MinimalDrawerImplementation();

        // Act & Assert
        drawer.Dispose();
        drawer.Dispose();
        drawer.Dispose();
    }

    /// <summary>
    /// Minimal implementation of ITwoColumnParameterDrawer for testing the default Dispose implementation.
    /// </summary>
    private sealed class MinimalDrawerImplementation : ITwoColumnParameterDrawer
    {
        public void Draw(IParameter parameter)
        {
            // Minimal implementation for testing
        }
    }

}
