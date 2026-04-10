using System.Reflection;


namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="NestedScopePathResolver"/>.
/// </summary>
[TestClass]
public class NestedScopePathResolverTests
{
    /// <summary>
    /// Verifies that when <see cref="TypeDrawMetadata.PropertyDrawMetadata.ConfigPrefix"/> is non-null,
    /// it is selected as the segment and combined with the parent path.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropMetaConfigPrefixIsNonNull_UsesConfigPrefix()
    {
        // Arrange
        var parentPath = "parent.path";
        var expectedSegment = "propConfigPrefix";
        var propMeta = CreatePropertyMetadata(
            configPrefix: expectedSegment,
            configParameterKeyOverride: null,
            propertyName: "TestProperty");
        var propTypeMeta = CreateTypeMetadata(configPrefix: "typePrefix");

        // Act
        var result = NestedScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("parent.path.propConfigPrefix", result);
    }

    /// <summary>
    /// Verifies that when <see cref="TypeDrawMetadata.PropertyDrawMetadata.ConfigPrefix"/> is null,
    /// but <see cref="TypeDrawMetadata.ConfigPrefix"/> is non-null, the type-level prefix is used.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropMetaConfigPrefixIsNullButPropTypeMetaConfigPrefixIsNonNull_UsesTypeLevelPrefix()
    {
        // Arrange
        var parentPath = "parent";
        var expectedSegment = "typeConfigPrefix";
        var propMeta = CreatePropertyMetadata(
            configPrefix: null,
            configParameterKeyOverride: "keyOverride",
            propertyName: "TestProperty");
        var propTypeMeta = CreateTypeMetadata(configPrefix: expectedSegment);

        // Act
        var result = NestedScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("parent.typeConfigPrefix", result);
    }

    /// <summary>
    /// Verifies that when both <see cref="TypeDrawMetadata.PropertyDrawMetadata.ConfigPrefix"/> and
    /// <see cref="TypeDrawMetadata.ConfigPrefix"/> are null, but <see cref="TypeDrawMetadata.PropertyDrawMetadata.ConfigParameterKeyOverride"/>
    /// is non-null, the key override is used as the segment.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenBothPrefixesAreNullButKeyOverrideIsNonNull_UsesKeyOverride()
    {
        // Arrange
        var parentPath = "root";
        var expectedSegment = "customKey";
        var propMeta = CreatePropertyMetadata(
            configPrefix: null,
            configParameterKeyOverride: expectedSegment,
            propertyName: "TestProperty");
        var propTypeMeta = CreateTypeMetadata(configPrefix: null);

        // Act
        var result = NestedScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("root.customKey", result);
    }

    /// <summary>
    /// Verifies that when all prefix and key override values are null, the camelCased property name is used.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenAllMetadataIsNullAndPropertyNameIsPascalCase_UsesCamelCasedPropertyName()
    {
        // Arrange
        var parentPath = "base";
        var propMeta = CreatePropertyMetadata(
            configPrefix: null,
            configParameterKeyOverride: null,
            propertyName: "MyProperty");
        var propTypeMeta = CreateTypeMetadata(configPrefix: null);

        // Act
        var result = NestedScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("base.myProperty", result);
    }

    /// <summary>
    /// Verifies that when the parent path is empty, only the segment is returned without a leading dot.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenParentPathIsEmpty_ReturnsOnlySegment()
    {
        // Arrange
        var parentPath = "";
        var propMeta = CreatePropertyMetadata(
            configPrefix: "segment",
            configParameterKeyOverride: null,
            propertyName: "Property");
        var propTypeMeta = CreateTypeMetadata(configPrefix: null);

        // Act
        var result = NestedScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("segment", result);
    }

    /// <summary>
    /// Verifies that when the parent path is empty and a key override is selected, the result contains only the key override without a leading dot.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenParentPathIsEmptyAndKeyOverrideIsUsed_ReturnsOnlyKeyOverride()
    {
        // Arrange
        var propMeta = CreatePropertyMetadata(
            configPrefix: null,
            configParameterKeyOverride: "overrideKey",
            propertyName: "Property");
        var propTypeMeta = CreateTypeMetadata(configPrefix: null);

        // Act
        var result = NestedScopePathResolver.Resolve(string.Empty, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("overrideKey", result);
    }

    /// <summary>
    /// Verifies that when all metadata is null and the property name is already camelCase, it is preserved.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropertyNameIsAlreadyCamelCase_PreservesPropertyName()
    {
        // Arrange
        var propMeta = CreatePropertyMetadata(
            configPrefix: null,
            configParameterKeyOverride: null,
            propertyName: "dummyProperty");
        var propTypeMeta = CreateTypeMetadata(configPrefix: null);

        // Act
        var result = NestedScopePathResolver.Resolve(string.Empty, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("dummyProperty", result);
    }

    /// <summary>
    /// Verifies that an empty property name is rejected because it cannot contribute a unique scope segment.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropertyNameIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var parentPath = "parent";
        var propMeta = CreatePropertyMetadata(
            configPrefix: null,
            configParameterKeyOverride: null,
            propertyName: "");
        var propTypeMeta = CreateTypeMetadata(configPrefix: null);

        // Act
        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => NestedScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta));

        // Assert
        Assert.Contains("resolves to an empty scope segment", exception.Message);
        Assert.Contains("TestPropertyHolder.", exception.Message);
    }

    /// <summary>
    /// Verifies that an explicitly empty property-level config prefix is rejected.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropMetaConfigPrefixIsEmptyString_ThrowsInvalidOperationException()
    {
        // Arrange
        var parentPath = "parent";
        var propMeta = CreatePropertyMetadata(
            configPrefix: "",
            configParameterKeyOverride: "override",
            propertyName: "Property");
        var propTypeMeta = CreateTypeMetadata(configPrefix: "typePrefix");

        // Assert
        Assert.ThrowsExactly<InvalidOperationException>(
            () => NestedScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta));
    }

    /// <summary>
    /// Verifies that an explicitly empty type-level config prefix is rejected.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropTypeMetaConfigPrefixIsEmptyString_ThrowsInvalidOperationException()
    {
        // Arrange
        var parentPath = "parent";
        var propMeta = CreatePropertyMetadata(
            configPrefix: null,
            configParameterKeyOverride: "override",
            propertyName: "Property");
        var propTypeMeta = CreateTypeMetadata(configPrefix: "");

        // Assert
        Assert.ThrowsExactly<InvalidOperationException>(
            () => NestedScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta));
    }

    /// <summary>
    /// Verifies that an explicitly empty key override is rejected.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenConfigParameterKeyOverrideIsEmptyString_ThrowsInvalidOperationException()
    {
        // Arrange
        var parentPath = "parent";
        var propMeta = CreatePropertyMetadata(
            configPrefix: null,
            configParameterKeyOverride: "",
            propertyName: "Property");
        var propTypeMeta = CreateTypeMetadata(configPrefix: null);

        // Assert
        Assert.ThrowsExactly<InvalidOperationException>(
            () => NestedScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta));
    }

    /// <summary>
    /// Helper method to create a <see cref="TypeDrawMetadata.PropertyDrawMetadata"/> instance
    /// with the minimal required parameters and a stable value accessor for testing.
    /// </summary>
    private static TypeDrawMetadata.PropertyDrawMetadata CreatePropertyMetadata(
        string? configPrefix,
        string? configParameterKeyOverride,
        string propertyName)
    {
        PropertyInfo propertyInfo;

        var realProperty = typeof(TestPropertyHolder).GetProperty(propertyName);
        if (realProperty != null)
        {
            propertyInfo = realProperty;
        }
        else
        {
            propertyInfo = new MockPropertyInfo(propertyName);
        }

        return new TypeDrawMetadata.PropertyDrawMetadata(
            property: propertyInfo,
            propertyType: typeof(object),
            getValue: owner => propertyInfo.GetValue(owner),
            isParameter: false,
            category: null,
            indentAttr: null,
            collapseAttr: null,
            labelMarginAttr: null,
            nestedDrawerAttr: null,
            hideIf: null,
            disableIf: null,
            order: 0,
            spacingBefore: 0,
            spacingAfter: 0,
            configPrefix: configPrefix,
            configParameterKeyOverride: configParameterKeyOverride);
    }

    /// <summary>
    /// Helper method to create a <see cref="TypeDrawMetadata"/> instance
    /// with minimal required parameters for testing.
    /// </summary>
    private static TypeDrawMetadata CreateTypeMetadata(string? configPrefix)
    {
        return (TypeDrawMetadata)Activator.CreateInstance(
            typeof(TypeDrawMetadata),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new object?[]
            {
                null,
                configPrefix,
                null,
                null,
                null,
                null,
                false,
                Array.Empty<TypeDrawMetadata.PropertyDrawMetadata>()
            },
            null)!;
    }

    /// <summary>
    /// Helper class used to obtain a valid <see cref="PropertyInfo"/> instance for testing.
    /// </summary>
    private class TestPropertyHolder
    {
        public object DummyProperty { get; set; } = new();
        public object MyProperty { get; set; } = new();
    }

    /// <summary>
    /// Mock PropertyInfo for testing edge cases with specific property names.
    /// </summary>
    private class MockPropertyInfo : PropertyInfo
    {
        private readonly string _name;

        public MockPropertyInfo(string name)
        {
            _name = name;
        }

        public override string Name => _name;
        public override Type PropertyType => typeof(object);
        public override PropertyAttributes Attributes => PropertyAttributes.None;
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override Type? DeclaringType => typeof(TestPropertyHolder);
        public override Type? ReflectedType => typeof(TestPropertyHolder);

        public override MethodInfo[] GetAccessors(bool nonPublic) => Array.Empty<MethodInfo>();
        public override MethodInfo? GetGetMethod(bool nonPublic) => null;
        public override ParameterInfo[] GetIndexParameters() => Array.Empty<ParameterInfo>();
        public override MethodInfo? GetSetMethod(bool nonPublic) => null;
        public override object? GetValue(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? index, System.Globalization.CultureInfo? culture) => null;
        public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, object?[]? index, System.Globalization.CultureInfo? culture) { }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => Array.Empty<object>();
        public override object[] GetCustomAttributes(bool inherit) => Array.Empty<object>();
        public override bool IsDefined(Type attributeType, bool inherit) => false;
    }
}
