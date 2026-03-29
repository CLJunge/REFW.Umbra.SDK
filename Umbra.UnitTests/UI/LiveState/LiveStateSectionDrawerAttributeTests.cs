using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Umbra.UI.LiveState;


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
    /// Helper class for testing drawer type resolution with a sealed class.
    /// </summary>
    private sealed class SealedDrawer
    {
    }

    /// <summary>
    /// Helper class for testing drawer type resolution with a nested class.
    /// </summary>
    private class NestedDrawer
    {
        public class InnerDrawer
        {
        }
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
        Type result = attribute.DrawerType;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(SimpleDrawer), result);
    }

    /// <summary>
    /// Tests that DrawerType returns the correct Type for a sealed class.
    /// Input: Attribute with SealedDrawer type parameter.
    /// Expected: Returns typeof(SealedDrawer) and is not null.
    /// </summary>
    [TestMethod]
    public void DrawerType_WithSealedClass_ReturnsCorrectType()
    {
        // Arrange
        var attribute = new LiveStateSectionDrawerAttribute<SealedDrawer>();

        // Act
        Type result = attribute.DrawerType;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(SealedDrawer), result);
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
        Type result = attribute.DrawerType;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(List<int>), result);
        Assert.IsTrue(result.IsGenericType);
    }

    /// <summary>
    /// Tests that DrawerType returns the correct Type for a nested class.
    /// Input: Attribute with NestedDrawer.InnerDrawer type parameter.
    /// Expected: Returns typeof(NestedDrawer.InnerDrawer) and is not null.
    /// </summary>
    [TestMethod]
    public void DrawerType_WithNestedClass_ReturnsCorrectType()
    {
        // Arrange
        var attribute = new LiveStateSectionDrawerAttribute<NestedDrawer.InnerDrawer>();

        // Act
        Type result = attribute.DrawerType;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(NestedDrawer.InnerDrawer), result);
        Assert.IsTrue(result.IsNested);
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
        Type firstAccess = attribute.DrawerType;
        Type secondAccess = attribute.DrawerType;

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
        Type intType = attributeInt.DrawerType;
        Type stringType = attributeString.DrawerType;

        // Assert
        Assert.IsNotNull(intType);
        Assert.IsNotNull(stringType);
        Assert.AreNotEqual(intType, stringType);
        Assert.AreEqual(typeof(List<int>), intType);
        Assert.AreEqual(typeof(List<string>), stringType);
    }

    /// <summary>
    /// Tests that DrawerType returns a Type with the expected properties for a simple class.
    /// Input: Attribute with SimpleDrawer type parameter.
    /// Expected: Returned Type has IsClass=true, IsAbstract=false, IsSealed=false.
    /// </summary>
    [TestMethod]
    public void DrawerType_WithSimpleClass_ReturnsTypeWithExpectedProperties()
    {
        // Arrange
        var attribute = new LiveStateSectionDrawerAttribute<SimpleDrawer>();

        // Act
        Type result = attribute.DrawerType;

        // Assert
        Assert.IsTrue(result.IsClass);
        Assert.IsFalse(result.IsAbstract);
        Assert.IsFalse(result.IsSealed);
        Assert.IsFalse(result.IsInterface);
    }

    /// <summary>
    /// Tests that DrawerType returns a Type with IsSealed=true for a sealed class.
    /// Input: Attribute with SealedDrawer type parameter.
    /// Expected: Returned Type has IsClass=true and IsSealed=true.
    /// </summary>
    [TestMethod]
    public void DrawerType_WithSealedClass_ReturnsTypeWithIsSealedTrue()
    {
        // Arrange
        var attribute = new LiveStateSectionDrawerAttribute<SealedDrawer>();

        // Act
        Type result = attribute.DrawerType;

        // Assert
        Assert.IsTrue(result.IsClass);
        Assert.IsTrue(result.IsSealed);
    }
}