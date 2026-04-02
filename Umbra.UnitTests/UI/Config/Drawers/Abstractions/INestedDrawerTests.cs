namespace Umbra.UI.Config.Drawers.UnitTests;
/// <summary>
/// Unit tests for <see cref="INestedDrawer{T}"/>.
/// </summary>
[TestClass]
public class INestedDrawerTests
{
    /// <summary>
    /// Tests that the default Dispose implementation completes without throwing an exception.
    /// </summary>
    [TestMethod]
    public void Dispose_WithReferenceType_CompletesWithoutException()
    {
        // Arrange
        INestedDrawer<string> drawer = new TestNestedDrawer<string>();

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
        INestedDrawer<object> drawer = new TestNestedDrawer<object>();

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
        IDisposable drawer = new TestNestedDrawer<object>();

        // Act & Assert
        drawer.Dispose();
    }

    /// <summary>
    /// Minimal helper implementation of <see cref="INestedDrawer{T}"/> for testing the default Dispose method.
    /// </summary>
    /// <typeparam name="T">The nested configuration group type.</typeparam>
    private sealed class TestNestedDrawer<T> : INestedDrawer<T>
    {
        public void Draw(T groupInstance)
        {
            // Minimal implementation for testing purposes
        }
    }

}
