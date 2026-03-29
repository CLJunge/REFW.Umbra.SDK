using Umbra.Config;


namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Unit tests for <see cref="IParameterDrawer"/> interface.
/// </summary>
[TestClass]
public class IParameterDrawerTests
{
    /// <summary>
    /// Tests that the default Dispose implementation does not throw an exception when called.
    /// Input: A concrete implementation of IParameterDrawer with default Dispose.
    /// Expected: Dispose completes without throwing.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledOnce_DoesNotThrow()
    {
        // Arrange
        IParameterDrawer drawer = new TestParameterDrawer();

        // Act & Assert
        drawer.Dispose();
    }

    /// <summary>
    /// Tests that the default Dispose implementation can be called multiple times without throwing.
    /// Input: A concrete implementation of IParameterDrawer with default Dispose called twice.
    /// Expected: Both calls complete without throwing, demonstrating idempotency.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        IParameterDrawer drawer = new TestParameterDrawer();

        // Act & Assert
        drawer.Dispose();
        drawer.Dispose();
    }

    /// <summary>
    /// Tests that the default Dispose implementation executes correctly through explicit interface cast.
    /// Input: A concrete implementation cast explicitly to IDisposable.
    /// Expected: Dispose completes without throwing when invoked through IDisposable reference.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledThroughExplicitInterfaceCast_DoesNotThrow()
    {
        // Arrange
        IParameterDrawer drawer = new TestParameterDrawer();
        IDisposable disposable = drawer;

        // Act & Assert
        disposable.Dispose();
    }

    /// <summary>
    /// Minimal concrete implementation of <see cref="IParameterDrawer"/> for testing default Dispose behavior.
    /// Does not override Dispose, allowing the default interface implementation to be tested.
    /// </summary>
    private sealed class TestParameterDrawer : IParameterDrawer
    {
        public void Draw(string label, IParameter parameter)
        {
            // Minimal implementation - no action required for Dispose testing.
        }
    }
}
