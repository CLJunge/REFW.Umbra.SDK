namespace Umbra.UI.LiveState.UnitTests;

/// <summary>
/// Unit tests for <see cref="LiveStateSectionDrawerAttribute{TDrawer}"/>.
/// </summary>
[TestClass]
public sealed class LiveStateSectionDrawerAttributeTests
{
    /// <summary>
    /// Helper class for testing drawer type resolution with a simple class.
    /// </summary>
    private class SimpleDrawer
    {
    }

    /// <summary>
    /// Tests that DrawerType returns the correct Type for a simple concrete class.
    /// Input: Attribute with SimpleDrawer type parameter.
    /// Expected: Returns typeof(SimpleDrawer) and is not null.
    /// </summary>
    [TestMethod]
    public void DrawerType_WithSimpleClass_ReturnsCorrectType()
    {
        // Arrange
        var attribute = new LiveStateSectionDrawerAttribute<SimpleDrawer>();

        // Act
        var result = attribute.DrawerType;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(SimpleDrawer), result);
    }

    /// <summary>
    /// Tests that DrawerType returns the correct Type for a generic type instantiation.
    /// Input: Attribute with List&lt;int&gt; type parameter.
    /// Expected: Returns typeof(List&lt;int&gt;) and is not null.
    /// </summary>
    [TestMethod]
    public void DrawerType_WithGenericTypeInstantiation_ReturnsCorrectType()
    {
        // Arrange
        var attribute = new LiveStateSectionDrawerAttribute<List<int>>();

        // Act
        var result = attribute.DrawerType;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(List<int>), result);
        Assert.IsTrue(result.IsGenericType);
    }

    /// <summary>
    /// Tests that multiple accesses to DrawerType return the same Type instance.
    /// Input: Attribute with SimpleDrawer type parameter, accessed twice.
    /// Expected: Both accesses return the exact same Type instance (reference equality).
    /// </summary>
    [TestMethod]
    public void DrawerType_MultipleAccesses_ReturnsSameInstance()
    {
        // Arrange
        var attribute = new LiveStateSectionDrawerAttribute<SimpleDrawer>();

        // Act
        var firstAccess = attribute.DrawerType;
        var secondAccess = attribute.DrawerType;

        // Assert
        Assert.AreSame(firstAccess, secondAccess);
    }

    /// <summary>
    /// Tests that DrawerType correctly returns Type for different generic instantiations.
    /// Input: Attributes with List&lt;int&gt; and List&lt;string&gt; type parameters.
    /// Expected: Each returns its respective Type and they are not equal.
    /// </summary>
    [TestMethod]
    public void DrawerType_WithDifferentGenericInstantiations_ReturnsDifferentTypes()
    {
        // Arrange
        var attributeInt = new LiveStateSectionDrawerAttribute<List<int>>();
        var attributeString = new LiveStateSectionDrawerAttribute<List<string>>();

        // Act
        var intType = attributeInt.DrawerType;
        var stringType = attributeString.DrawerType;

        // Assert
        Assert.IsNotNull(intType);
        Assert.IsNotNull(stringType);
        Assert.AreNotEqual(intType, stringType);
        Assert.AreEqual(typeof(List<int>), intType);
        Assert.AreEqual(typeof(List<string>), stringType);
    }

    /// <summary>
    /// Tests that the attribute also exposes the drawer type through the interface contract.
    /// </summary>
    [TestMethod]
    public void DrawerType_ThroughInterface_ReturnsSameType()
    {
        // Arrange
        ILiveStateSectionDrawerAttribute attribute = new LiveStateSectionDrawerAttribute<SimpleDrawer>();

        // Act
        var result = attribute.DrawerType;

        // Assert
        Assert.AreEqual(typeof(SimpleDrawer), result);
    }

    /// <summary>
    /// Tests that separate attribute instances for the same drawer type report equal drawer types.
    /// </summary>
    [TestMethod]
    public void DrawerType_MultipleInstancesSameDrawerType_ReturnSameReportedType()
    {
        // Arrange
        var first = new LiveStateSectionDrawerAttribute<SimpleDrawer>();
        var second = new LiveStateSectionDrawerAttribute<SimpleDrawer>();

        // Act
        var firstType = first.DrawerType;
        var secondType = second.DrawerType;

        // Assert
        Assert.AreEqual(firstType, secondType);
        Assert.AreSame(firstType, secondType);
    }

}
