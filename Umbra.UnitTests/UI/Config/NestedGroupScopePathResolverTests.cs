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
    /// Verifies behavior when the property name is empty (edge case).
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropertyNameIsEmpty_ReturnsParentPath()
    {
        // Arrange
        var parentPath = "parent";
        var propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: null,
            propertyName: "");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        var result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("parent", result);
    }

    /// <summary>
    /// Verifies that when the settings prefix is an empty string, it short-circuits the fallback chain
    /// and returns the empty string segment combined with parent path.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropMetaSettingsPrefixIsEmptyString_UsesEmptyString()
    {
        // Arrange
        var parentPath = "parent";
        var propMeta = CreatePropertyMetadata(
            settingsPrefix: "",
            settingsParameterKeyOverride: "override",
            propertyName: "Property");
        var propTypeMeta = CreateTypeMetadata(settingsPrefix: "typePrefix");

        // Act
        var result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("parent", result);
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
        var propertyInfo = typeof(TestPropertyHolder).GetProperty(nameof(TestPropertyHolder.DummyProperty))!;
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
    }
}
