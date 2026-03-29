using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Reflection;
using Umbra;
using Umbra.Config;
using Umbra.Config.UnitTests;

namespace Umbra.UnitTests;
/// <summary>
/// Unit tests for the <see cref = "ReflectionExtensions.GetDrawerAttribute{T}(PropertyInfo)"/> method.
/// </summary>
[TestClass]
public sealed class ReflectionExtensionsTests
{
#region Test Helper Types
    /// <summary>
    /// Base test attribute for testing attribute inheritance scenarios.
    /// </summary>
    private sealed class BaseTestAttribute : Attribute
    {
        public string Value { get; }

        public BaseTestAttribute(string value) => Value = value;
    }

    /// <summary>
    /// Test interface for attributes to test interface-based matching.
    /// </summary>
    private interface ITestAttribute
    {
        string Name { get; }
    }

    /// <summary>
    /// Test attribute implementing an interface.
    /// </summary>
    private sealed class InterfaceImplementingAttribute : Attribute, ITestAttribute
    {
        public string Name { get; }

        public InterfaceImplementingAttribute(string name) => Name = name;
    }

    /// <summary>
    /// Another unrelated test attribute.
    /// </summary>
    private sealed class OtherTestAttribute : Attribute
    {
        public int Id { get; }

        public OtherTestAttribute(int id) => Id = id;
    }

    /// <summary>
    /// Test class with various property attribute configurations.
    /// </summary>
    private sealed class TestClass
    {
        public int NoAttributes { get; set; }

        [BaseTest("base")]
        public int WithBaseAttribute { get; set; }

        public int WithDerivedAttribute { get; set; }

        [InterfaceImplementing("interface")]
        public int WithInterfaceImplementingAttribute { get; set; }

        [OtherTest(42)]
        public int WithOtherAttribute { get; set; }

        [BaseTest("first")]
        [OtherTest(99)]
        public int WithMultipleAttributes { get; set; }

        [OtherTest(1)]
        [BaseTest("second")]
        public int WithMultipleAttributesBaseSecond { get; set; }

        [BaseTest("base")]
        [OtherTest(123)]
        public int WithMixedAttributes { get; set; }
    }

#endregion
    /// <summary>
    /// Tests that GetDrawerAttribute returns null when the property has no custom attributes.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithNoAttributes_ReturnsNull()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributes))!;
        // Act
        BaseTestAttribute? result = property.GetDrawerAttribute<BaseTestAttribute>();
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetDrawerAttribute returns the matching attribute when property has one exact match.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithMatchingAttribute_ReturnsAttribute()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.WithBaseAttribute))!;
        // Act
        BaseTestAttribute? result = property.GetDrawerAttribute<BaseTestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("base", result.Value);
    }

    /// <summary>
    /// Tests that GetDrawerAttribute returns null when property has attributes but none match the requested type.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithNonMatchingAttributes_ReturnsNull()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.WithOtherAttribute))!;
        // Act
        BaseTestAttribute? result = property.GetDrawerAttribute<BaseTestAttribute>();
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetDrawerAttribute returns the attribute when searching by interface it implements.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithInterfaceImplementingAttribute_SearchingForInterface_ReturnsAttribute()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.WithInterfaceImplementingAttribute))!;
        // Act
        ITestAttribute? result = property.GetDrawerAttribute<ITestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("interface", result.Name);
    }

    /// <summary>
    /// Tests that GetDrawerAttribute returns the first matching attribute when multiple attributes are present.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithMultipleAttributes_FirstMatching_ReturnsFirstMatch()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.WithMultipleAttributes))!;
        // Act
        BaseTestAttribute? result = property.GetDrawerAttribute<BaseTestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("first", result.Value);
    }

    /// <summary>
    /// Tests that GetDrawerAttribute returns the first matching attribute even when it appears second in declaration order.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithMultipleAttributes_SecondMatching_ReturnsMatch()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.WithMultipleAttributesBaseSecond))!;
        // Act
        BaseTestAttribute? result = property.GetDrawerAttribute<BaseTestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("second", result.Value);
    }

    /// <summary>
    /// Tests that GetDrawerAttribute returns the first attribute matching the requested type among mixed attributes.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithMixedAttributes_ReturnsFirstMatchingType()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.WithMixedAttributes))!;
        // Act
        BaseTestAttribute? result = property.GetDrawerAttribute<BaseTestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("base", result.Value);
    }

    /// <summary>
    /// Tests that GetDrawerAttribute can differentiate and return the correct type when multiple attribute types are present.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithMixedAttributes_SearchingForOther_ReturnsOtherAttribute()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.WithMixedAttributes))!;
        // Act
        OtherTestAttribute? result = property.GetDrawerAttribute<OtherTestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(123, result.Id);
    }

    /// <summary>
    /// Tests that GetDrawerAttribute returns the attribute when searching for the exact interface implementation type.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithInterfaceImplementingAttribute_SearchingForExactType_ReturnsAttribute()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.WithInterfaceImplementingAttribute))!;
        // Act
        InterfaceImplementingAttribute? result = property.GetDrawerAttribute<InterfaceImplementingAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("interface", result.Name);
    }

    /// <summary>
    /// Tests that GetDrawerAttribute works correctly with Attribute base class as the search type.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithAttribute_SearchingForAttributeBaseClass_ReturnsAttribute()
    {
        // Arrange
        PropertyInfo property = typeof(TestClass).GetProperty(nameof(TestClass.WithBaseAttribute))!;
        // Act
        Attribute? result = property.GetDrawerAttribute<Attribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(BaseTestAttribute));
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute returns null when the type has no custom attributes.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_TypeWithNoAttributes_ReturnsNull()
    {
        // Arrange
        Type type = typeof(TypeWithNoAttributes);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute returns the attribute when the type has an exact matching attribute.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_TypeWithMatchingAttribute_ReturnsAttribute()
    {
        // Arrange
        Type type = typeof(TypeWithTestAttribute);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test", result.Name);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute returns null when the type has attributes but none match the requested type.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_TypeWithNonMatchingAttributes_ReturnsNull()
    {
        // Arrange
        Type type = typeof(TypeWithOtherAttribute);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute returns the first matching attribute when multiple attributes of the same type are present.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_TypeWithMultipleSameAttributes_ReturnsFirst()
    {
        // Arrange
        Type type = typeof(TypeWithMultipleTestAttributes);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("First", result.Name);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute returns an attribute that implements the requested interface.
    /// This tests the interface-based matching scenario described in the XML documentation.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_AttributeImplementsInterface_ReturnsAttribute()
    {
        // Arrange
        Type type = typeof(TypeWithInterfaceAttribute);
        // Act
        ITestInterface? result = type.GetDrawerAttribute<ITestInterface>();
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(AttributeImplementingInterface));
        Assert.AreEqual(42, result.Value);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute returns the first attribute assignable to the requested type
    /// when multiple attributes are present and a later one matches.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_TypeWithMixedAttributes_ReturnsFirstMatch()
    {
        // Arrange
        Type type = typeof(TypeWithMixedAttributes);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Mixed", result.Name);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute works correctly with sealed types.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_SealedType_ReturnsAttribute()
    {
        // Arrange
        Type type = typeof(SealedTypeWithAttribute);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Sealed", result.Name);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute works correctly with abstract types.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_AbstractType_ReturnsAttribute()
    {
        // Arrange
        Type type = typeof(AbstractTypeWithAttribute);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Abstract", result.Name);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute works correctly with interface types.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_InterfaceType_ReturnsAttribute()
    {
        // Arrange
        Type type = typeof(IInterfaceWithAttribute);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Interface", result.Name);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute returns null for a derived type when the base class has an attribute
    /// but inherit=false is used (as per the implementation).
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_DerivedTypeWithBaseAttribute_ReturnsNull()
    {
        // Arrange
        Type type = typeof(DerivedTypeWithoutAttribute);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute works correctly with generic type definitions.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_GenericTypeDefinition_ReturnsAttribute()
    {
        // Arrange
        Type type = typeof(GenericTypeWithAttribute<>);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Generic", result.Name);
    }

    /// <summary>
    /// Verifies that GetDrawerAttribute works correctly with constructed generic types.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_ConstructedGenericType_ReturnsAttribute()
    {
        // Arrange
        Type type = typeof(GenericTypeWithAttribute<int>);
        // Act
        TestAttribute? result = type.GetDrawerAttribute<TestAttribute>();
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Generic", result.Name);
    }

    // Helper types for testing
    internal class TypeWithNoAttributes
    {
    }

    [Test("Test")]
    internal class TypeWithTestAttribute
    {
    }

    [Other("Other")]
    internal class TypeWithOtherAttribute
    {
    }

    [Test("First")]
    [Test("Second")]
    internal class TypeWithMultipleTestAttributes
    {
    }

    [AttributeImplementingInterface(42)]
    internal class TypeWithInterfaceAttribute
    {
    }

    [Other("Other")]
    [Test("Mixed")]
    internal class TypeWithMixedAttributes
    {
    }

    [Test("Sealed")]
    internal sealed class SealedTypeWithAttribute
    {
    }

    [Test("Abstract")]
    internal abstract class AbstractTypeWithAttribute
    {
    }

    [Test("Interface")]
    internal interface IInterfaceWithAttribute
    {
    }

    [Test("Base")]
    internal class BaseTypeWithAttribute
    {
    }

    internal class DerivedTypeWithoutAttribute : BaseTypeWithAttribute
    {
    }

    internal class TypeWithDerivedAttribute
    {
    }

    [Test("Generic")]
    internal class GenericTypeWithAttribute<T>
    {
    }

    // Test attributes
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    internal class TestAttribute : Attribute
    {
        public string Name { get; }

        public TestAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal class OtherAttribute : Attribute
    {
        public string Name { get; }

        public OtherAttribute(string name)
        {
            Name = name;
        }
    }

    internal interface ITestInterface
    {
        int Value { get; }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal class AttributeImplementingInterface : Attribute, ITestInterface
    {
        public int Value { get; }

        public AttributeImplementingInterface(int value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns the correct generic attribute when a matching attribute is present.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_MemberWithMatchingGenericAttribute_ReturnsAttribute()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var testAttribute = new TestGenericAttribute<int>();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { testAttribute });
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(testAttribute, result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns null when the member has no custom attributes.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_MemberWithNoAttributes_ReturnsNull()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(Array.Empty<object>());
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns null when the member has only non-generic attributes.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_MemberWithOnlyNonGenericAttributes_ReturnsNull()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var nonGenericAttribute = new TestNonGenericAttribute();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { nonGenericAttribute });
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns null when the member has a generic attribute with a different generic type definition.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_MemberWithNonMatchingGenericAttribute_ReturnsNull()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var otherGenericAttribute = new OtherGenericAttribute<string>();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { otherGenericAttribute });
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns the first matching attribute when multiple attributes are present and the first one matches.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_MultipleAttributesFirstMatches_ReturnsFirstMatchingAttribute()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var firstAttribute = new TestGenericAttribute<int>();
        var secondAttribute = new TestNonGenericAttribute();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { firstAttribute, secondAttribute });
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(firstAttribute, result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns the matching attribute when multiple attributes are present and a later one matches.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_MultipleAttributesLaterMatches_ReturnsMatchingAttribute()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var firstAttribute = new TestNonGenericAttribute();
        var secondAttribute = new TestGenericAttribute<string>();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { firstAttribute, secondAttribute });
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(secondAttribute, result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns the first instance when the same generic attribute type appears multiple times.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_MultipleMatchingAttributes_ReturnsFirstInstance()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var firstAttribute = new TestGenericAttribute<int>();
        var secondAttribute = new TestGenericAttribute<string>();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { firstAttribute, secondAttribute });
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(firstAttribute, result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns null when a closed generic type is passed as the genericType parameter.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_ClosedGenericTypeAsParameter_ReturnsNull()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var testAttribute = new TestGenericAttribute<int>();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { testAttribute });
        var closedGenericType = typeof(TestGenericAttribute<int>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(closedGenericType);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns null when a non-generic type is passed as the genericType parameter.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_NonGenericTypeAsParameter_ReturnsNull()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var testAttribute = new TestGenericAttribute<int>();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { testAttribute });
        var nonGenericType = typeof(TestNonGenericAttribute);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(nonGenericType);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns null when genericType parameter is null.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_NullGenericType_ReturnsNull()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var testAttribute = new TestGenericAttribute<int>();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { testAttribute });
        Type? nullGenericType = null;
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(nullGenericType!);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute correctly identifies a generic attribute among a mix of generic and non-generic attributes.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_MixOfGenericAndNonGenericAttributes_ReturnsMatchingGenericAttribute()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var nonGeneric1 = new TestNonGenericAttribute();
        var generic1 = new OtherGenericAttribute<int>();
        var targetGeneric = new TestGenericAttribute<double>();
        var nonGeneric2 = new AnotherNonGenericAttribute();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { nonGeneric1, generic1, targetGeneric, nonGeneric2 });
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(targetGeneric, result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute handles non-Attribute objects in the custom attributes array (defensive check).
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_AttributesContainNonAttributeObject_SkipsNonAttributeObjects()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var nonAttributeObject = new object ();
        var testAttribute = new TestGenericAttribute<int>();
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { nonAttributeObject, testAttribute });
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(testAttribute, result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute returns null when only non-Attribute objects are present in custom attributes.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_OnlyNonAttributeObjects_ReturnsNull()
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var nonAttributeObject1 = new object ();
        var nonAttributeObject2 = "string";
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { nonAttributeObject1, nonAttributeObject2 });
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetCustomGenericAttribute correctly matches generic attributes with different type parameters.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(int))]
    [DataRow(typeof(string))]
    [DataRow(typeof(double))]
    [DataRow(typeof(object))]
    public void GetCustomGenericAttribute_GenericAttributeWithVariousTypeParameters_ReturnsAttribute(Type typeParameter)
    {
        // Arrange
        var mockMember = new Mock<MemberInfo>();
        var attributeType = typeof(TestGenericAttribute<>).MakeGenericType(typeParameter);
        var testAttribute = (Attribute)Activator.CreateInstance(attributeType)!;
        mockMember.Setup(m => m.GetCustomAttributes(false)).Returns(new object[] { testAttribute });
        var genericTypeDef = typeof(TestGenericAttribute<>);
        // Act
        var result = mockMember.Object.GetCustomGenericAttribute(genericTypeDef);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(testAttribute, result);
    }

#region Helper Attribute Classes
    /// <summary>
    /// Test generic attribute for unit testing purposes.
    /// </summary>
    /// <typeparam name = "T">Type parameter for the generic attribute.</typeparam>
    private sealed class TestGenericAttribute<T> : Attribute
    {
    }

    /// <summary>
    /// Another test generic attribute with a different generic type definition.
    /// </summary>
    /// <typeparam name = "T">Type parameter for the generic attribute.</typeparam>
    private sealed class OtherGenericAttribute<T> : Attribute
    {
    }

    /// <summary>
    /// Test non-generic attribute for unit testing purposes.
    /// </summary>
    private sealed class TestNonGenericAttribute : Attribute
    {
    }

    /// <summary>
    /// Another test non-generic attribute for unit testing purposes.
    /// </summary>
    private sealed class AnotherNonGenericAttribute : Attribute
    {
    }
#endregion
}