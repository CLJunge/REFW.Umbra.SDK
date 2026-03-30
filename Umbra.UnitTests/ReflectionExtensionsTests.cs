namespace Umbra.UnitTests;

/// <summary>
/// Contains focused unit tests for <see cref="ReflectionExtensions"/>.
/// </summary>
[TestClass]
public sealed class ReflectionExtensionsTests
{
    /// <summary>
    /// Verifies that property-level attribute lookup returns an exact matching attribute.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithExactAttribute_ReturnsAttribute()
    {
        var property = typeof(PropertyAttributeContainer).GetProperty(nameof(PropertyAttributeContainer.WithTestAttribute))!;

        var result = property.GetDrawerAttribute<TestAttribute>();

        Assert.IsNotNull(result);
        Assert.AreEqual("Property", result.Name);
    }

    /// <summary>
    /// Verifies that property-level attribute lookup supports interface-based matching.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithInterfaceImplementingAttribute_ReturnsInterface()
    {
        var property = typeof(PropertyAttributeContainer).GetProperty(nameof(PropertyAttributeContainer.WithInterfaceAttribute))!;

        var result = property.GetDrawerAttribute<ITestInterface>();

        Assert.IsNotNull(result);
        Assert.AreEqual(42, result.Value);
    }

    /// <summary>
    /// Verifies that property-level attribute lookup returns <see langword="null"/> when no assignable attribute exists.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_PropertyWithoutMatchingAttribute_ReturnsNull()
    {
        var property = typeof(PropertyAttributeContainer).GetProperty(nameof(PropertyAttributeContainer.WithOtherAttribute))!;

        var result = property.GetDrawerAttribute<TestAttribute>();

        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that type-level attribute lookup returns the first assignable attribute.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_TypeWithMatchingAttribute_ReturnsAttribute()
    {
        var result = typeof(TypeWithTestAttribute).GetDrawerAttribute<TestAttribute>();

        Assert.IsNotNull(result);
        Assert.AreEqual("Type", result.Name);
    }

    /// <summary>
    /// Verifies that type-level attribute lookup supports interface-based matching.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_TypeWithInterfaceImplementingAttribute_ReturnsInterface()
    {
        var result = typeof(TypeWithInterfaceAttribute).GetDrawerAttribute<ITestInterface>();

        Assert.IsNotNull(result);
        Assert.AreEqual(99, result.Value);
    }

    /// <summary>
    /// Verifies that type-level attribute lookup does not walk base types because the implementation uses non-inherited lookup.
    /// </summary>
    [TestMethod]
    public void GetDrawerAttribute_DerivedTypeWithoutOwnAttribute_ReturnsNull()
    {
        var result = typeof(DerivedTypeWithoutAttribute).GetDrawerAttribute<TestAttribute>();

        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that generic-attribute lookup matches the first closed generic instance of the requested open generic type.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_MemberWithMatchingGenericAttribute_ReturnsAttribute()
    {
        var member = typeof(GenericAttributeContainer).GetMethod(nameof(GenericAttributeContainer.WithMatchingGenericAttribute))!;

        var result = member.GetCustomGenericAttribute(typeof(TestGenericAttribute<>));

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<TestGenericAttribute<int>>(result);
    }

    /// <summary>
    /// Verifies that generic-attribute lookup skips unrelated attributes and returns a later matching generic attribute.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_MemberWithMixedAttributes_ReturnsMatchingGenericAttribute()
    {
        var member = typeof(GenericAttributeContainer).GetMethod(nameof(GenericAttributeContainer.WithMixedAttributes))!;

        var result = member.GetCustomGenericAttribute(typeof(TestGenericAttribute<>));

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<TestGenericAttribute<string>>(result);
    }

    /// <summary>
    /// Verifies that generic-attribute lookup returns <see langword="null"/> when the requested type is not an open generic definition used by any attribute.
    /// </summary>
    [TestMethod]
    public void GetCustomGenericAttribute_WithClosedGenericTypeParameter_ReturnsNull()
    {
        var member = typeof(GenericAttributeContainer).GetMethod(nameof(GenericAttributeContainer.WithMatchingGenericAttribute))!;

        var result = member.GetCustomGenericAttribute(typeof(TestGenericAttribute<int>));

        Assert.IsNull(result);
    }

    private sealed class PropertyAttributeContainer
    {
        [Test("Property")]
        public int WithTestAttribute { get; set; }

        [Interface(42)]
        public int WithInterfaceAttribute { get; set; }

        [Other("Other")]
        public int WithOtherAttribute { get; set; }
    }

    [Test("Type")]
    private sealed class TypeWithTestAttribute
    {
    }

    [Interface(99)]
    private sealed class TypeWithInterfaceAttribute
    {
    }

    [Test("Base")]
    private class BaseTypeWithAttribute
    {
    }

    private sealed class DerivedTypeWithoutAttribute : BaseTypeWithAttribute
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    private sealed class GenericAttributeContainer
    {
        [TestGeneric<int>]
        public void WithMatchingGenericAttribute()
        {
            // No-op for testing
        }

        [Other("Other")]
        [OtherGeneric<int>]
        [TestGeneric<string>]
        public void WithMixedAttributes()
        {
            // No-op for testing
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    private sealed class TestAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    [AttributeUsage(AttributeTargets.All)]
    private sealed class OtherAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    private interface ITestInterface
    {
        int Value { get; }
    }

    [AttributeUsage(AttributeTargets.All)]
    private sealed class InterfaceAttribute(int value) : Attribute, ITestInterface
    {
        public int Value { get; } = value;
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    private sealed class TestGenericAttribute<T> : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    private sealed class OtherGenericAttribute<T> : Attribute
    {
    }
}
