using System;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.Config.Attributes;
using Umbra.UI.Config;

namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="TypeDrawMetadata"/> class.
/// </summary>
[TestClass]
public sealed class TypeDrawMetadataTests
{
    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> returns a non-null <see cref="TypeDrawMetadata"/>
    /// instance when passed a valid type.
    /// </summary>
    [TestMethod]
    public void For_WithValidSimpleType_ReturnsNonNullMetadata()
    {
        // Arrange
        Type testType = typeof(SimpleTestClass);

        // Act
        TypeDrawMetadata result = TypeDrawMetadata.For(testType);

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
        Type testType = typeof(CachingTestClass);

        // Act
        TypeDrawMetadata firstCall = TypeDrawMetadata.For(testType);
        TypeDrawMetadata secondCall = TypeDrawMetadata.For(testType);

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
        Type firstType = typeof(FirstTestClass);
        Type secondType = typeof(SecondTestClass);

        // Act
        TypeDrawMetadata firstMetadata = TypeDrawMetadata.For(firstType);
        TypeDrawMetadata secondMetadata = TypeDrawMetadata.For(secondType);

        // Assert
        Assert.AreNotSame(firstMetadata, secondMetadata, "Different types should produce different metadata instances.");
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> correctly handles a type
    /// decorated with config attributes, returning non-null metadata.
    /// </summary>
    [TestMethod]
    public void For_WithTypeHavingAttributes_ReturnsNonNullMetadata()
    {
        // Arrange
        Type testType = typeof(AttributedTestClass);

        // Act
        TypeDrawMetadata result = TypeDrawMetadata.For(testType);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Properties);
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> handles an interface type,
    /// returning non-null metadata.
    /// </summary>
    [TestMethod]
    public void For_WithInterfaceType_ReturnsNonNullMetadata()
    {
        // Arrange
        Type testType = typeof(ITestInterface);

        // Act
        TypeDrawMetadata result = TypeDrawMetadata.For(testType);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> handles a struct (value type),
    /// returning non-null metadata.
    /// </summary>
    [TestMethod]
    public void For_WithStructType_ReturnsNonNullMetadata()
    {
        // Arrange
        Type testType = typeof(TestStruct);

        // Act
        TypeDrawMetadata result = TypeDrawMetadata.For(testType);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> handles a generic type,
    /// returning non-null metadata.
    /// </summary>
    [TestMethod]
    public void For_WithGenericType_ReturnsNonNullMetadata()
    {
        // Arrange
        Type testType = typeof(GenericTestClass<int>);

        // Act
        TypeDrawMetadata result = TypeDrawMetadata.For(testType);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> handles an abstract type,
    /// returning non-null metadata.
    /// </summary>
    [TestMethod]
    public void For_WithAbstractType_ReturnsNonNullMetadata()
    {
        // Arrange
        Type testType = typeof(AbstractTestClass);

        // Act
        TypeDrawMetadata result = TypeDrawMetadata.For(testType);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> handles a sealed type,
    /// returning non-null metadata.
    /// </summary>
    [TestMethod]
    public void For_WithSealedType_ReturnsNonNullMetadata()
    {
        // Arrange
        Type testType = typeof(SealedTestClass);

        // Act
        TypeDrawMetadata result = TypeDrawMetadata.For(testType);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> handles an enum type,
    /// returning non-null metadata.
    /// </summary>
    [TestMethod]
    public void For_WithEnumType_ReturnsNonNullMetadata()
    {
        // Arrange
        Type testType = typeof(TestEnum);

        // Act
        TypeDrawMetadata result = TypeDrawMetadata.For(testType);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> handles an array type,
    /// returning non-null metadata.
    /// </summary>
    [TestMethod]
    public void For_WithArrayType_ReturnsNonNullMetadata()
    {
        // Arrange
        Type testType = typeof(int[]);

        // Act
        TypeDrawMetadata result = TypeDrawMetadata.For(testType);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TypeDrawMetadata.For"/> is thread-safe when multiple threads
    /// request metadata for the same type concurrently.
    /// </summary>
    [TestMethod]
    public void For_ConcurrentCallsWithSameType_ReturnsSameCachedInstanceThreadSafely()
    {
        // Arrange
        Type testType = typeof(ConcurrencyTestClass);
        const int threadCount = 10;
        TypeDrawMetadata?[] results = new TypeDrawMetadata[threadCount];
        System.Threading.CountdownEvent countdown = new System.Threading.CountdownEvent(threadCount);

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            int index = i;
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                results[index] = TypeDrawMetadata.For(testType);
                countdown.Signal();
            });
        }

        countdown.Wait();

        // Assert
        TypeDrawMetadata firstResult = results[0]!;
        Assert.IsNotNull(firstResult);

        for (int i = 1; i < threadCount; i++)
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

    [UmbraAutoRegisterSettings]
    [UmbraCategory("Test Category")]
    internal class AttributedTestClass
    {
        public int TestProperty { get; set; }
    }

    internal interface ITestInterface
    {
    }

    internal struct TestStruct
    {
    }

    internal class GenericTestClass<T>
    {
    }

    internal abstract class AbstractTestClass
    {
    }

    internal sealed class SealedTestClass
    {
    }

    internal enum TestEnum
    {
        Value1,
        Value2
    }

    internal class ConcurrencyTestClass
    {
    }

    #endregion
}
