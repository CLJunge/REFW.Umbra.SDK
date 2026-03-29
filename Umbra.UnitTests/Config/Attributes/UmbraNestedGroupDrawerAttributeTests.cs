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
    /// Tests that the DrawerType property returns the correct Type for a different TDrawer.
    /// Input: UmbraNestedGroupDrawerAttribute instantiated with TestDrawer2.
    /// Expected: DrawerType returns typeof(TestDrawer2).
    /// </summary>
    [TestMethod]
    public void DrawerType_WithTestDrawer2_ReturnsTestDrawer2Type()
    {
        // Arrange
        var attribute = new UmbraNestedGroupDrawerAttribute<TestDrawer2>();

        // Act
        var result = attribute.DrawerType;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(TestDrawer2), result);
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
    /// Tests that different instances of the same generic attribute return the same Type.
    /// Input: Two separate instances of UmbraNestedGroupDrawerAttribute with the same TDrawer.
    /// Expected: Both instances return the same Type object.
    /// </summary>
    [TestMethod]
    public void DrawerType_DifferentInstancesSameGenericType_ReturnSameType()
    {
        // Arrange
        var attribute1 = new UmbraNestedGroupDrawerAttribute<TestDrawer1>();
        var attribute2 = new UmbraNestedGroupDrawerAttribute<TestDrawer1>();

        // Act
        var result1 = attribute1.DrawerType;
        var result2 = attribute2.DrawerType;

        // Assert
        Assert.AreEqual(result1, result2);
        Assert.AreSame(result1, result2);
    }

    /// <summary>
    /// Tests that DrawerType returns a Type that is a class type.
    /// Input: UmbraNestedGroupDrawerAttribute instantiated with a class type.
    /// Expected: DrawerType.IsClass is true, matching the generic constraint.
    /// </summary>
    [TestMethod]
    public void DrawerType_ReturnsClassType_IsClassReturnsTrue()
    {
        // Arrange
        var attribute = new UmbraNestedGroupDrawerAttribute<TestDrawer1>();

        // Act
        var result = attribute.DrawerType;

        // Assert
        Assert.IsTrue(result.IsClass);
    }

    /// <summary>
    /// Tests that DrawerType returns the correct full name for the drawer type.
    /// Input: UmbraNestedGroupDrawerAttribute instantiated with TestDrawer1.
    /// Expected: DrawerType.FullName contains the expected type name.
    /// </summary>
    [TestMethod]
    public void DrawerType_ReturnsTypeWithCorrectFullName_ContainsTestDrawer1()
    {
        // Arrange
        var attribute = new UmbraNestedGroupDrawerAttribute<TestDrawer1>();

        // Act
        var result = attribute.DrawerType;

        // Assert
        Assert.IsNotNull(result.FullName);
        Assert.IsTrue(result.FullName.Contains(nameof(TestDrawer1)));
    }

    /// <summary>
    /// Test drawer class for testing purposes.
    /// </summary>
    private sealed class TestDrawer1
    {
    }

    /// <summary>
    /// Another test drawer class for testing purposes.
    /// </summary>
    private sealed class TestDrawer2
    {
    }
}