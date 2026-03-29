using System.Reflection;


namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="NestedGroupScopePathResolver"/>.
/// </summary>
[TestClass]
public class NestedGroupScopePathResolverTests
{
    /// <summary>
    /// Verifies that when <see cref="TypeDrawMetadata.PropertyDrawMetadata.SettingsPrefix"/> is non-null,
    /// it is selected as the segment and combined with the parent path.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropMetaSettingsPrefixIsNonNull_UsesSettingsPrefix()
    {
        // Arrange
        var parentPath = "parent.path";
        var expectedSegment = "propSettingsPrefix";
        var propMeta = CreatePropertyMetadata(
            settingsPrefix: expectedSegment,
            settingsParameterKeyOverride: null,
            propertyName: "TestProperty");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: "typePrefix");

        // Act
        var result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("parent.path.propSettingsPrefix", result);
    }

    /// <summary>
    /// Verifies that when <see cref="TypeDrawMetadata.PropertyDrawMetadata.SettingsPrefix"/> is null,
    /// but <see cref="TypeDrawMetadata.SettingsPrefix"/> is non-null, the type-level prefix is used.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropMetaSettingsPrefixIsNullButPropTypeMetaSettingsPrefixIsNonNull_UsesTypeLevelPrefix()
    {
        // Arrange
        var parentPath = "parent";
        var expectedSegment = "typeSettingsPrefix";
        var propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: "keyOverride",
            propertyName: "TestProperty");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: expectedSegment);

        // Act
        var result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("parent.typeSettingsPrefix", result);
    }

    /// <summary>
    /// Verifies that when both <see cref="TypeDrawMetadata.PropertyDrawMetadata.SettingsPrefix"/> and
    /// <see cref="TypeDrawMetadata.SettingsPrefix"/> are null, but <see cref="TypeDrawMetadata.PropertyDrawMetadata.SettingsParameterKeyOverride"/>
    /// is non-null, the key override is used as the segment.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenBothPrefixesAreNullButKeyOverrideIsNonNull_UsesKeyOverride()
    {
        // Arrange
        var parentPath = "root";
        var expectedSegment = "customKey";
        var propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: expectedSegment,
            propertyName: "TestProperty");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        var result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

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
            settingsPrefix: null,
            settingsParameterKeyOverride: null,
            propertyName: "MyProperty");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        var result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

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
            settingsPrefix: "segment",
            settingsParameterKeyOverride: null,
            propertyName: "Property");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        var result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("segment", result);
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
            settingsPrefix: null,
            settingsParameterKeyOverride: null,
            propertyName: "");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Assert
        AssertThrowsInvalidOperationException(
            () => NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta));
    }

    /// <summary>
    /// Verifies that an explicitly empty property-level settings prefix is rejected.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropMetaSettingsPrefixIsEmptyString_ThrowsInvalidOperationException()
    {
        // Arrange
        var parentPath = "parent";
        var propMeta = CreatePropertyMetadata(
            settingsPrefix: "",
            settingsParameterKeyOverride: "override",
            propertyName: "Property");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: "typePrefix");

        // Assert
        AssertThrowsInvalidOperationException(
            () => NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta));
    }

    /// <summary>
    /// Verifies that an explicitly empty type-level settings prefix is rejected.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropTypeMetaSettingsPrefixIsEmptyString_ThrowsInvalidOperationException()
    {
        // Arrange
        var parentPath = "parent";
        var propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: "override",
            propertyName: "Property");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: "");

        // Assert
        AssertThrowsInvalidOperationException(
            () => NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta));
    }

    /// <summary>
    /// Verifies that an explicitly empty key override is rejected.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenSettingsParameterKeyOverrideIsEmptyString_ThrowsInvalidOperationException()
    {
        // Arrange
        var parentPath = "parent";
        var propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: "",
            propertyName: "Property");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Assert
        AssertThrowsInvalidOperationException(
            () => NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta));
    }

    private static void AssertThrowsInvalidOperationException(Action action)
    {
        try
        {
            action();
            Assert.Fail("Expected InvalidOperationException.");
        }
        catch (InvalidOperationException)
        {
        }
    }

    /// <summary>
    /// Helper method to create a <see cref="TypeDrawMetadata.PropertyDrawMetadata"/> instance
    /// with minimal required parameters for testing.
    /// </summary>
    private static TypeDrawMetadata.PropertyDrawMetadata CreatePropertyMetadata(
        string? settingsPrefix,
        string? settingsParameterKeyOverride,
        string propertyName)
    {
        PropertyInfo propertyInfo;
        
        // Try to use a real property from TestPropertyHolder, otherwise use a mock
        var realProperty = typeof(TestPropertyHolder).GetProperty(propertyName);
        if (realProperty != null)
        {
            propertyInfo = realProperty;
        }
        else
        {
            // For property names not in TestPropertyHolder, create a mock
            propertyInfo = new MockPropertyInfo(propertyName);
        }
        
        return new TypeDrawMetadata.PropertyDrawMetadata(
            property: propertyInfo,
            propertyType: typeof(object),
            isParameter: false,
            category: null,
            indentAttr: null,
            collapseAttr: null,
            labelMarginAttr: null,
            nestedGroupDrawerAttr: null,
            hideIf: null,
            order: 0,
            spacingBefore: 0,
            spacingAfter: 0,
            settingsPrefix: settingsPrefix,
            settingsParameterKeyOverride: settingsParameterKeyOverride);
    }

    /// <summary>
    /// Helper method to create a <see cref="TypeDrawMetadata"/> instance
    /// with minimal required parameters for testing.
    /// </summary>
    private static TypeDrawMetadata CreateTypeMetadata(string? settingsPrefix)
    {
        return (TypeDrawMetadata)Activator.CreateInstance(
            typeof(TypeDrawMetadata),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new object?[]
            {
                null, // category
                settingsPrefix, // settingsPrefix
                null, // indentAttr
                null, // collapseAttr
                null, // labelMarginAttr
                null, // nestedGroupDrawerAttr
                false, // isAutoRegisterSettings
                Array.Empty<TypeDrawMetadata.PropertyDrawMetadata>() // properties
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
