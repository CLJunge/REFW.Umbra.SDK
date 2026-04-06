
using Umbra.Config;
using Umbra.Config.Attributes;

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

    /// <summary>
    /// Tests that class-level metadata is populated from the corresponding attributes.
    /// </summary>
    [TestMethod]
    public void For_AutoRegisteredTypeWithClassAttributes_PopulatesClassMetadata()
    {
        // Arrange
        var testType = typeof(AttributedConfig);

        // Act
        var result = TypeDrawMetadata.For(testType);

        // Assert
        Assert.IsTrue(result.IsAutoRegisterConfig);
        Assert.AreEqual("Root Category", result.Category);
        Assert.AreEqual("rootPrefix", result.ConfigPrefix);
        Assert.IsNotNull(result.IndentAttr);
        Assert.IsNotNull(result.CollapseAttr);
        Assert.IsNotNull(result.LabelMarginAttr);
    }

    /// <summary>
    /// Tests that property-level metadata captures parameter and nested-group information.
    /// </summary>
    [TestMethod]
    public void For_TypeWithAnnotatedProperties_PopulatesPropertyMetadata()
    {
        // Arrange
        var result = TypeDrawMetadata.For(typeof(AttributedConfig));
        var parameterProperty = Array.Find(result.Properties, static p => p.Property.Name == nameof(AttributedConfig.Enabled));
        var nestedProperty = Array.Find(result.Properties, static p => p.Property.Name == nameof(AttributedConfig.Nested));

        // Assert
        Assert.IsNotNull(parameterProperty);
        Assert.IsTrue(parameterProperty.IsParameter);
        Assert.AreEqual("Enabled Category", parameterProperty.Category);
        Assert.AreEqual(7, parameterProperty.Order);
        Assert.AreEqual(1, parameterProperty.SpacingBefore);
        Assert.AreEqual(2, parameterProperty.SpacingAfter);
        Assert.IsNotNull(parameterProperty.HideIf);

        Assert.IsNotNull(nestedProperty);
        Assert.IsFalse(nestedProperty.IsParameter);
        Assert.AreEqual("nestedPrefix", nestedProperty.ConfigPrefix);
        Assert.AreEqual("nestedKey", nestedProperty.ConfigParameterKeyOverride);
        Assert.IsNotNull(nestedProperty.CollapseAttr);
        Assert.IsNotNull(nestedProperty.IndentAttr);
        Assert.IsNotNull(nestedProperty.LabelMarginAttr);
    }

    /// <summary>
    /// Tests that property metadata exposes a cached getter which reads the current boxed property value.
    /// </summary>
    [TestMethod]
    public void For_TypeWithAnnotatedProperties_CachedGetterReadsCurrentPropertyValue()
    {
        // Arrange
        var config = new AttributedConfig();
        var metadata = TypeDrawMetadata.For(typeof(AttributedConfig));
        var parameterProperty = Array.Find(metadata.Properties, static p => p.Property.Name == nameof(AttributedConfig.Enabled));
        var nestedProperty = Array.Find(metadata.Properties, static p => p.Property.Name == nameof(AttributedConfig.Nested));

        // Act
        var parameterValue = parameterProperty!.GetValue(config);
        var nestedValue = nestedProperty!.GetValue(config);

        // Assert
        Assert.AreSame(config.Enabled, parameterValue);
        Assert.AreSame(config.Nested, nestedValue);
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

    [UmbraAutoRegister]
    [UmbraCategory("Root Category")]
    [UmbraPrefix("rootPrefix")]
    [UmbraIndent(5f)]
    [UmbraCollapseAsTree]
    [UmbraLabelMargin(12f)]
    internal sealed class AttributedConfig
    {
        [UmbraParameter]
        [UmbraCategory("Enabled Category")]
        [UmbraParameterOrder(7)]
        [UmbraSpacingBefore(1)]
        [UmbraSpacingAfter(2)]
        [UmbraHideIf<bool>(nameof(HideEnabled))]
        public Parameter<bool> Enabled { get; set; } = new(true);

        [UmbraParameter("nestedKey")]
        [UmbraPrefix("nestedPrefix")]
        [UmbraIndent(3f)]
        [UmbraCollapseAsTree]
        [UmbraLabelMargin(4f)]
        public NestedAttributedConfig Nested { get; set; } = new();

        public bool HideEnabled { get; set; }
    }

    [UmbraAutoRegister]
    internal sealed class NestedAttributedConfig
    {
        [UmbraParameter]
        public Parameter<int> Value { get; set; } = new(1);
    }

    #endregion
}
