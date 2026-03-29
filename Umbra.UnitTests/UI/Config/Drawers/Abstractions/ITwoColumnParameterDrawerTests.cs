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
    /// Tests that Dispose can be called on an implementation with a finalizer,
    /// verifying GC.SuppressFinalize is invoked without error.
    /// </summary>
    [TestMethod]
    public void Dispose_ImplementationWithFinalizer_DoesNotThrow()
    {
        // Arrange
        ITwoColumnParameterDrawer drawer = new DrawerWithFinalizer();

        // Act & Assert
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

    /// <summary>
    /// Implementation of ITwoColumnParameterDrawer with a finalizer to test GC.SuppressFinalize behavior.
    /// </summary>
    private sealed class DrawerWithFinalizer : ITwoColumnParameterDrawer
    {
#pragma warning disable CA1821 // Remove empty Finalizers - intentionally empty for testing Dispose pattern
        ~DrawerWithFinalizer()
        {
            // Finalizer to ensure GC.SuppressFinalize is meaningful
        }
#pragma warning restore CA1821

        public void Draw(IParameter parameter)
        {
            // Minimal implementation for testing
        }
    }
}
