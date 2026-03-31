namespace Umbra.Config.Attributes.UnitTests;

/// <summary>
/// Tests for <see cref="UmbraNestedGroupDrawerAttribute{TDrawer}"/>.
/// </summary>
[TestClass]
public sealed class UmbraNestedGroupDrawerAttributeTests
{
    /// <summary>
    /// Tests that the DrawerType property returns the correct Type for the specified TDrawer.
    /// Input: UmbraNestedGroupDrawerAttribute instantiated with TestDrawer1.
    /// Expected: DrawerType returns typeof(TestDrawer1).
    /// </summary>
    [TestMethod]
    public void DrawerType_WithTestDrawer1_ReturnsTestDrawer1Type()
    {
        // Arrange
        var attribute = new UmbraNestedGroupDrawerAttribute<TestDrawer1>();

        // Act
        var result = attribute.DrawerType;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(TestDrawer1), result);
    }

    /// <summary>
    /// Tests that multiple accesses to DrawerType return the same Type instance.
    /// Input: Multiple calls to DrawerType on the same attribute instance.
    /// Expected: All calls return the identical Type object reference.
    /// </summary>
    [TestMethod]
    public void DrawerType_MultipleAccesses_ReturnsSameTypeInstance()
    {
        // Arrange
        var attribute = new UmbraNestedGroupDrawerAttribute<TestDrawer1>();

        // Act
        var result1 = attribute.DrawerType;
        var result2 = attribute.DrawerType;

        // Assert
        Assert.AreSame(result1, result2);
    }

    /// <summary>
    /// Tests that the drawer type is also exposed correctly through the interface contract.
    /// </summary>
    [TestMethod]
    public void DrawerType_ThroughInterface_ReturnsCorrectType()
    {
        // Arrange
        INestedGroupDrawerAttribute attribute = new UmbraNestedGroupDrawerAttribute<TestDrawer1>();

        // Act
        var result = attribute.DrawerType;

        // Assert
        Assert.AreEqual(typeof(TestDrawer1), result);
    }

    /// <summary>
    /// Tests that different drawer generic arguments produce different reported drawer types.
    /// </summary>
    [TestMethod]
    public void DrawerType_WithDifferentDrawerTypes_ReturnsDifferentTypes()
    {
        // Arrange
        var first = new UmbraNestedGroupDrawerAttribute<TestDrawer1>();
        var second = new UmbraNestedGroupDrawerAttribute<TestDrawer2>();

        // Act
        var firstType = first.DrawerType;
        var secondType = second.DrawerType;

        // Assert
        Assert.AreEqual(typeof(TestDrawer1), firstType);
        Assert.AreEqual(typeof(TestDrawer2), secondType);
        Assert.AreNotEqual(firstType, secondType);
    }

    /// <summary>
    /// Test drawer class for testing purposes.
    /// </summary>
    private sealed class TestDrawer1
    {
    }

    /// <summary>
    /// Alternate test drawer class for validating generic type variation.
    /// </summary>
    private sealed class TestDrawer2
    {
    }

}
