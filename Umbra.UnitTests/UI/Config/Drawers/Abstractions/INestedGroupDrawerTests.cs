namespace Umbra.UI.Config.Drawers.UnitTests;
/// <summary>
/// Unit tests for <see cref="INestedGroupDrawer{T}"/>.
/// </summary>
[TestClass]
public class INestedGroupDrawerTests
{
    /// <summary>
    /// Tests that the default Dispose implementation completes without throwing an exception.
    /// </summary>
    [TestMethod]
    public void Dispose_WithReferenceType_CompletesWithoutException()
    {
        // Arrange
        INestedGroupDrawer<string> drawer = new TestNestedGroupDrawer<string>();

        // Act & Assert
        drawer.Dispose();
    }

    /// <summary>
    /// Tests that the default Dispose implementation can be called multiple times without throwing an exception, verifying idempotency.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_CompletesWithoutException()
    {
        // Arrange
        INestedGroupDrawer<object> drawer = new TestNestedGroupDrawer<object>();

        // Act & Assert
        drawer.Dispose();
        drawer.Dispose();
        drawer.Dispose();
    }

    /// <summary>
    /// Tests that the interface implementation is also exposed as <see cref="IDisposable"/>.
    /// </summary>
    [TestMethod]
    public void Dispose_CastToIDisposable_CompletesWithoutException()
    {
        // Arrange
        IDisposable drawer = new TestNestedGroupDrawer<object>();

        // Act & Assert
        drawer.Dispose();
    }

    /// <summary>
    /// Minimal helper implementation of <see cref="INestedGroupDrawer{T}"/> for testing the default Dispose method.
    /// </summary>
    /// <typeparam name="T">The nested configuration group type.</typeparam>
    private sealed class TestNestedGroupDrawer<T> : INestedGroupDrawer<T>
    {
        public void Draw(T groupInstance)
        {
            // Minimal implementation for testing purposes
        }
    }

}
