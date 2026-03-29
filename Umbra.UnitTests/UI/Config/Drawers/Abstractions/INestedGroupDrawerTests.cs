namespace Umbra.UI.Config.Drawers.UnitTests;

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    /// Tests that the default Dispose implementation completes without throwing an exception when called on a drawer with a value type parameter.
    /// </summary>
    [TestMethod]
    public void Dispose_WithValueType_CompletesWithoutException()
    {
        // Arrange
        INestedGroupDrawer<int> drawer = new TestNestedGroupDrawer<int>();

        // Act & Assert
        drawer.Dispose();
    }

    /// <summary>
    /// Tests that the default Dispose implementation completes without throwing an exception when called on a drawer with a nullable reference type parameter.
    /// </summary>
    [TestMethod]
    public void Dispose_WithNullableReferenceType_CompletesWithoutException()
    {
        // Arrange
        INestedGroupDrawer<string?> drawer = new TestNestedGroupDrawer<string?>();

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
    /// Tests that the default Dispose implementation completes without throwing an exception for a complex reference type.
    /// </summary>
    [TestMethod]
    public void Dispose_WithComplexReferenceType_CompletesWithoutException()
    {
        // Arrange
        INestedGroupDrawer<TestComplexType> drawer = new TestNestedGroupDrawer<TestComplexType>();

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

    /// <summary>
    /// Complex helper type used for testing generic type parameter variations.
    /// </summary>
    private sealed class TestComplexType
    {
        public int Value { get; set; }
        public string? Name { get; set; }
    }
}