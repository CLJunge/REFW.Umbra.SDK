namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="TypeDrawMetadata"/> class.
/// </summary>
[TestClass]
public sealed class TypeDrawMetadataTests
{
    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    /// </summary>
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> returns a non-null <see cref="TypeDrawMetadata"/>
    /// instance when passed a valid type.
    /// </summary>
    [TestMethod]
    public void For_WithValidSimpleType_ReturnsNonNullMetadata()
    {
        // Arrange
        var testType = typeof(SimpleTestClass);

        // Act
        var result = TypeDrawMetadata.For(testType);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> returns the same cached instance
    /// when called multiple times with the same type, verifying caching behavior.
    /// </summary>
    [TestMethod]
    public void For_CalledTwiceWithSameType_ReturnsSameCachedInstance()
    {
        // Arrange
        var testType = typeof(CachingTestClass);

        // Act
        var firstCall = TypeDrawMetadata.For(testType);
        var secondCall = TypeDrawMetadata.For(testType);

        // Assert
        Assert.AreSame(firstCall, secondCall, "Expected the same cached instance to be returned.");
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> returns different instances
    /// for different types, ensuring proper cache key differentiation.
    /// </summary>
    [TestMethod]
    public void For_WithDifferentTypes_ReturnsDifferentInstances()
    {
        // Arrange
        var firstType = typeof(FirstTestClass);
        var secondType = typeof(SecondTestClass);

        // Act
        var firstMetadata = TypeDrawMetadata.For(firstType);
        var secondMetadata = TypeDrawMetadata.For(secondType);

        // Assert
        Assert.AreNotSame(firstMetadata, secondMetadata, "Different types should produce different metadata instances.");
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> is thread-safe when multiple threads
    /// request metadata for the same type concurrently.
    /// </summary>
    [TestMethod]
    public void For_ConcurrentCallsWithSameType_ReturnsSameCachedInstanceThreadSafely()
    {
        // Arrange
        var testType = typeof(ConcurrencyTestClass);
        const int threadCount = 10;
        var results = new TypeDrawMetadata[threadCount];
        var countdown = new System.Threading.CountdownEvent(threadCount);

        // Act
        for (var i = 0; i < threadCount; i++)
        {
            var index = i;
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                results[index] = TypeDrawMetadata.For(testType);
                countdown.Signal();
            });
        }

        countdown.Wait(TestContext?.CancellationToken ?? default);

        // Assert
        var firstResult = results[0]!;
        Assert.IsNotNull(firstResult);

        for (var i = 1; i < threadCount; i++)
        {
            Assert.AreSame(firstResult, results[i], $"Thread {i} returned a different instance.");
        }
    }

    #region Helper Test Classes

    internal class SimpleTestClass
    {
    }

    internal class CachingTestClass
    {
    }

    internal class FirstTestClass
    {
    }

    internal class SecondTestClass
    {
    }

    internal class ConcurrencyTestClass
    {
    }

    #endregion
}
