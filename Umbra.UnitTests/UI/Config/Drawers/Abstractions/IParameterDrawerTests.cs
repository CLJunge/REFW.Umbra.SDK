using Umbra.Config;

namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Contains focused unit tests for <see cref="IParameterDrawer"/> default disposal behavior.
/// </summary>
[TestClass]
public sealed class IParameterDrawerTests
{
    /// <summary>
    /// Verifies that the default interface implementation of <see cref="IDisposable.Dispose"/> is callable.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledThroughInterface_DoesNotThrow()
    {
        IDisposable drawer = new TestParameterDrawer();

        drawer.Dispose();
    }

    /// <summary>
    /// Verifies that the default interface implementation of <see cref="IDisposable.Dispose"/> is repeatable.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
    {
        IDisposable drawer = new TestParameterDrawer();

        drawer.Dispose();
        drawer.Dispose();
    }

    /// <summary>
    /// Verifies that the drawer remains callable through the interface before disposal.
    /// </summary>
    [TestMethod]
    public void Draw_WhenCalledThroughInterface_DoesNotThrow()
    {
        IParameterDrawer drawer = new TestParameterDrawer();
        IParameter parameter = new Parameter<int>(42);

        drawer.Draw("Label", parameter);
    }

    /// <summary>
    /// Minimal <see cref="IParameterDrawer"/> implementation used to exercise the default dispose behavior.
    /// </summary>
    private sealed class TestParameterDrawer : IParameterDrawer
    {
        public void Draw(string label, IParameter parameter)
        {
            // No-op for testing
        }
    }
}
