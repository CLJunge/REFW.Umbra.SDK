using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra.UI.Config;


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
        string parentPath = "parent.path";
        string expectedSegment = "propSettingsPrefix";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: expectedSegment,
            settingsParameterKeyOverride: null,
            propertyName: "TestProperty");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: "typePrefix");

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

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
        string parentPath = "parent";
        string expectedSegment = "typeSettingsPrefix";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: "keyOverride",
            propertyName: "TestProperty");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: expectedSegment);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

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
        string parentPath = "root";
        string expectedSegment = "customKey";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: expectedSegment,
            propertyName: "TestProperty");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

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
        string parentPath = "base";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: null,
            propertyName: "MyProperty");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("base.myProperty", result);
    }

    /// <summary>
    /// Verifies that when all metadata is null and the property name is already camelCase,
    /// it is used as-is.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenAllMetadataIsNullAndPropertyNameIsAlreadyCamelCase_UsesPropertyNameAsIs()
    {
        // Arrange
        string parentPath = "base";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: null,
            propertyName: "myProperty");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

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
        string parentPath = "";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: "segment",
            settingsParameterKeyOverride: null,
            propertyName: "Property");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("segment", result);
    }

    /// <summary>
    /// Verifies behavior when the parent path is null (edge case).
    /// </summary>
    [TestMethod]
    public void Resolve_WhenParentPathIsNull_ReturnsOnlySegment()
    {
        // Arrange
        string? parentPath = null;
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: "segment",
            settingsParameterKeyOverride: null,
            propertyName: "Property");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath!, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("segment", result);
    }

    /// <summary>
    /// Verifies that when the property name is a single uppercase character, camelCase conversion produces
    /// a lowercase character.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropertyNameIsSingleUppercaseCharacter_ReturnsLowercaseCharacter()
    {
        // Arrange
        string parentPath = "path";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: null,
            propertyName: "X");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("path.x", result);
    }

    /// <summary>
    /// Verifies behavior when the property name is empty (edge case).
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropertyNameIsEmpty_ReturnsParentPath()
    {
        // Arrange
        string parentPath = "parent";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: null,
            propertyName: "");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("parent", result);
    }

    /// <summary>
    /// Verifies correct behavior when both parent path and segment are empty strings.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenBothParentPathAndSegmentAreEmpty_ReturnsEmptyString()
    {
        // Arrange
        string parentPath = "";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: null,
            settingsParameterKeyOverride: null,
            propertyName: "");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("", result);
    }

    /// <summary>
    /// Verifies that when the settings prefix is an empty string, it short-circuits the fallback chain
    /// and returns the empty string segment combined with parent path.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropMetaSettingsPrefixIsEmptyString_UsesEmptyString()
    {
        // Arrange
        string parentPath = "parent";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: "",
            settingsParameterKeyOverride: "override",
            propertyName: "Property");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: "typePrefix");

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("parent", result);
    }

    /// <summary>
    /// Verifies correct path combination when parent path has multiple segments.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenParentPathHasMultipleSegments_CombinesCorrectly()
    {
        // Arrange
        string parentPath = "root.level1.level2";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: "child",
            settingsParameterKeyOverride: null,
            propertyName: "Property");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("root.level1.level2.child", result);
    }

    /// <summary>
    /// Verifies that special characters in the segment are preserved in the resulting path.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenSegmentContainsSpecialCharacters_PreservesCharacters()
    {
        // Arrange
        string parentPath = "parent";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: "segment-with_special$chars",
            settingsParameterKeyOverride: null,
            propertyName: "Property");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("parent.segment-with_special$chars", result);
    }

    /// <summary>
    /// Verifies that whitespace-only strings in the settings prefix are used as-is.
    /// </summary>
    [TestMethod]
    public void Resolve_WhenPropMetaSettingsPrefixIsWhitespace_UsesWhitespace()
    {
        // Arrange
        string parentPath = "parent";
        TypeDrawMetadata.PropertyDrawMetadata propMeta = CreatePropertyMetadata(
            settingsPrefix: "   ",
            settingsParameterKeyOverride: null,
            propertyName: "Property");
        TypeDrawMetadata propTypeMeta = CreateTypeMetadata(settingsPrefix: null);

        // Act
        string result = NestedGroupScopePathResolver.Resolve(parentPath, propMeta, propTypeMeta);

        // Assert
        Assert.AreEqual("parent.   ", result);
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
        PropertyInfo propertyInfo = typeof(TestPropertyHolder).GetProperty(nameof(TestPropertyHolder.DummyProperty))!;
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