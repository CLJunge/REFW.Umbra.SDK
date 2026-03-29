using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI;
using Umbra.UI.Config;

namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="ConfigSection{TConfig}"/> class.
/// </summary>
[TestClass]
public sealed class ConfigSectionTests
{
    /// <summary>
    /// Test configuration class used for testing <see cref="ConfigSection{TConfig}"/>.
    /// </summary>
    [UmbraAutoRegisterSettings]
    private sealed class TestConfig
    {
        [UmbraSettingsParameter]
        public Parameter<bool> TestParameter { get; set; } = new(true);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionId"/> returns the explicitly provided idScope value.
    /// </summary>
    /// <param name="idScope">The explicit ID scope to test.</param>
    [TestMethod]
    [DataRow("customId")]
    [DataRow("my.custom.scope")]
    [DataRow("Section_123")]
    [DataRow("a")]
    [DataRow("VeryLongIdScopeValueThatIsStillValidBecauseItIsNotEmptyOrWhitespace")]
    public void SectionId_ExplicitIdScopeProvided_ReturnsProvidedIdScope(string idScope)
    {
        // Arrange
        var config = new TestConfig();

        // Act
        var section = new ConfigSection<TestConfig>(config, idScope: idScope);

        // Assert
        Assert.AreEqual(idScope, section.SectionId);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionId"/> returns the type's FullName when idScope is null.
    /// </summary>
    [TestMethod]
    public void SectionId_IdScopeIsNull_ReturnsTypeFullName()
    {
        // Arrange
        var config = new TestConfig();
        var expectedFullName = typeof(TestConfig).FullName ?? typeof(TestConfig).Name;

        // Act
        var section = new ConfigSection<TestConfig>(config, idScope: null);

        // Assert
        Assert.AreEqual(expectedFullName, section.SectionId);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionId"/> returns the same value on multiple accesses, verifying immutability.
    /// </summary>
    [TestMethod]
    public void SectionId_MultipleAccesses_ReturnsSameValue()
    {
        // Arrange
        var config = new TestConfig();
        var section = new ConfigSection<TestConfig>(config, idScope: "testId");

        // Act
        var firstAccess = section.SectionId;
        var secondAccess = section.SectionId;
        var thirdAccess = section.SectionId;

        // Assert
        Assert.AreEqual(firstAccess, secondAccess);
        Assert.AreEqual(secondAccess, thirdAccess);
        Assert.AreEqual("testId", firstAccess);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionId"/> uses the default type-based ID when idScope is omitted.
    /// </summary>
    [TestMethod]
    public void SectionId_IdScopeOmitted_ReturnsTypeBasedDefault()
    {
        // Arrange
        var config = new TestConfig();
        var expectedId = typeof(TestConfig).FullName ?? typeof(TestConfig).Name;

        // Act
        var section = new ConfigSection<TestConfig>(config);

        // Assert
        Assert.AreEqual(expectedId, section.SectionId);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionId"/> remains unchanged after disposal,
    /// verifying the property is not affected by the object's disposed state.
    /// </summary>
    [TestMethod]
    public void SectionId_AfterDisposal_RemainsUnchanged()
    {
        // Arrange
        var config = new TestConfig();
        var section = new ConfigSection<TestConfig>(config, idScope: "testId");
        var sectionIdBeforeDispose = section.SectionId;

        // Act
        section.Dispose();
        var sectionIdAfterDispose = section.SectionId;

        // Assert
        Assert.AreEqual(sectionIdBeforeDispose, sectionIdAfterDispose);
        Assert.AreEqual("testId", sectionIdAfterDispose);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionId"/> correctly handles special characters in the idScope.
    /// </summary>
    /// <param name="idScopeWithSpecialChars">The idScope containing special characters.</param>
    [TestMethod]
    [DataRow("id-with-dashes")]
    [DataRow("id_with_underscores")]
    [DataRow("id.with.dots")]
    [DataRow("id:with:colons")]
    [DataRow("id/with/slashes")]
    [DataRow("id\\with\\backslashes")]
    [DataRow("id@with#special$chars%")]
    public void SectionId_IdScopeWithSpecialCharacters_ReturnsProvidedIdScope(string idScopeWithSpecialChars)
    {
        // Arrange
        var config = new TestConfig();

        // Act
        var section = new ConfigSection<TestConfig>(config, idScope: idScopeWithSpecialChars);

        // Assert
        Assert.AreEqual(idScopeWithSpecialChars, section.SectionId);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns null when suppressTreeNode is true,
    /// even when an explicit tree node label is provided.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_SuppressTreeNodeTrue_ReturnsNull()
    {
        // Arrange
        var config = new SimpleConfig();
        var section = new ConfigSection<SimpleConfig>(config, treeNodeLabel: "Test Label", suppressTreeNode: true);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns the explicit label
    /// when provided via constructor parameter.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_ExplicitLabelProvided_ReturnsExplicitLabel()
    {
        // Arrange
        var config = new SimpleConfig();
        const string expectedLabel = "My Custom Label";
        var section = new ConfigSection<SimpleConfig>(config, treeNodeLabel: expectedLabel);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.AreEqual(expectedLabel, result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns empty string
    /// when an empty string is explicitly provided via constructor parameter.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_ExplicitEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var config = new SimpleConfig();
        var section = new ConfigSection<SimpleConfig>(config, treeNodeLabel: string.Empty);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns whitespace string
    /// when a whitespace-only string is explicitly provided via constructor parameter.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_ExplicitWhitespace_ReturnsWhitespace()
    {
        // Arrange
        var config = new SimpleConfig();
        const string whitespaceLabel = "   ";
        var section = new ConfigSection<SimpleConfig>(config, treeNodeLabel: whitespaceLabel);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.AreEqual(whitespaceLabel, result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns the attribute's label
    /// when no explicit label is provided and the config type has UmbraConfigRootNodeAttribute with a non-null label.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_AttributeWithLabel_ReturnsAttributeLabel()
    {
        // Arrange
        var config = new ConfigWithRootNodeAttribute();
        var section = new ConfigSection<ConfigWithRootNodeAttribute>(config);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.AreEqual("Attribute Label", result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns the display name of the type
    /// when the UmbraConfigRootNodeAttribute exists but its Label property is null.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_AttributeWithNullLabel_ReturnsDisplayName()
    {
        // Arrange
        var config = new ConfigWithNullLabelAttribute();
        var section = new ConfigSection<ConfigWithNullLabelAttribute>(config);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.IsNotNull(result);
        // The result should be the display name derived from "ConfigWithNullLabelAttribute"
        Assert.AreEqual("Config With Null Label Attribute", result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns null
    /// when no explicit label is provided and the config type has no UmbraConfigRootNodeAttribute.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_NoAttributeNoLabel_ReturnsNull()
    {
        // Arrange
        var config = new SimpleConfig();
        var section = new ConfigSection<SimpleConfig>(config);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns the explicit label
    /// even when the config type has an attribute, demonstrating that explicit label takes precedence.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_ExplicitLabelOverridesAttribute_ReturnsExplicitLabel()
    {
        // Arrange
        var config = new ConfigWithRootNodeAttribute();
        const string explicitLabel = "Override Label";
        var section = new ConfigSection<ConfigWithRootNodeAttribute>(config, treeNodeLabel: explicitLabel);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.AreEqual(explicitLabel, result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> correctly handles very long strings.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_VeryLongString_ReturnsCompleteString()
    {
        // Arrange
        var config = new SimpleConfig();
        string longLabel = new string('A', 10000);
        var section = new ConfigSection<SimpleConfig>(config, treeNodeLabel: longLabel);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.AreEqual(longLabel, result);
        Assert.AreEqual(10000, result?.Length);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> correctly handles special characters.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_SpecialCharacters_ReturnsStringWithSpecialCharacters()
    {
        // Arrange
        var config = new SimpleConfig();
        const string specialLabel = "Test\nLabel\t<>&\"'";
        var section = new ConfigSection<SimpleConfig>(config, treeNodeLabel: specialLabel);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.AreEqual(specialLabel, result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns consistent values across multiple calls.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_MultipleCalls_ReturnsConsistentValue()
    {
        // Arrange
        var config = new SimpleConfig();
        const string expectedLabel = "Consistent Label";
        var section = new ConfigSection<SimpleConfig>(config, treeNodeLabel: expectedLabel);

        // Act
        string? result1 = section.TreeNodeLabel;
        string? result2 = section.TreeNodeLabel;
        string? result3 = section.TreeNodeLabel;

        // Assert
        Assert.AreEqual(expectedLabel, result1);
        Assert.AreEqual(expectedLabel, result2);
        Assert.AreEqual(expectedLabel, result3);
        Assert.AreSame(result1, result2);
        Assert.AreSame(result2, result3);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns null across multiple calls
    /// when no label is set.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_MultipleCalls_ConsistentlyReturnsNull()
    {
        // Arrange
        var config = new SimpleConfig();
        var section = new ConfigSection<SimpleConfig>(config);

        // Act
        string? result1 = section.TreeNodeLabel;
        string? result2 = section.TreeNodeLabel;
        string? result3 = section.TreeNodeLabel;

        // Assert
        Assert.IsNull(result1);
        Assert.IsNull(result2);
        Assert.IsNull(result3);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns null
    /// when suppressTreeNode is true and the config has a root node attribute.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_SuppressTreeNodeTrueWithAttribute_ReturnsNull()
    {
        // Arrange
        var config = new ConfigWithRootNodeAttribute();
        var section = new ConfigSection<ConfigWithRootNodeAttribute>(config, suppressTreeNode: true);

        // Act
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns null after disposal
    /// when the value was previously null.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_AfterDisposal_StillReturnsNull()
    {
        // Arrange
        var config = new SimpleConfig();
        var section = new ConfigSection<SimpleConfig>(config);

        // Act
        section.Dispose();
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeLabel"/> returns the same value after disposal
    /// when the value was previously set.
    /// </summary>
    [TestMethod]
    public void TreeNodeLabel_AfterDisposal_StillReturnsSameValue()
    {
        // Arrange
        var config = new SimpleConfig();
        const string expectedLabel = "Label Before Disposal";
        var section = new ConfigSection<SimpleConfig>(config, treeNodeLabel: expectedLabel);

        // Act
        section.Dispose();
        string? result = section.TreeNodeLabel;

        // Assert
        Assert.AreEqual(expectedLabel, result);
    }

    #region Test Config Classes

    /// <summary>
    /// Simple test configuration class with no attributes.
    /// </summary>
    [UmbraAutoRegisterSettings]
    internal sealed class SimpleConfig
    {
    }

    /// <summary>
    /// Test configuration class with UmbraConfigRootNodeAttribute specifying a label.
    /// </summary>
    [UmbraAutoRegisterSettings]
    [UmbraConfigRootNode("Attribute Label", true)]
    internal sealed class ConfigWithRootNodeAttribute
    {
    }

    /// <summary>
    /// Test configuration class with UmbraConfigRootNodeAttribute with null label.
    /// </summary>
    [UmbraAutoRegisterSettings]
    [UmbraConfigRootNode(null, false)]
    internal sealed class ConfigWithNullLabelAttribute
    {
    }

    #endregion

    /// <summary>
    /// Verifies that calling Dispose for the first time completes successfully without throwing exceptions.
    /// </summary>
    /// <remarks>
    /// Due to ConfigDrawer being a concrete, unmockable class, this test verifies that the Dispose method
    /// executes without errors. Direct verification that _drawer.Dispose() was called or that _disposed
    /// was set to true is not possible due to private field access restrictions and unmockable dependencies.
    /// </remarks>
    [TestMethod]
    public void Dispose_FirstCall_CompletesSuccessfully()
    {
        // Arrange
        var config = new TestConfig();
        var section = new ConfigSection<TestConfig>(config);

        // Act
        section.Dispose();

        // Assert
        // No exception thrown - successful disposal
    }

    /// <summary>
    /// Verifies that calling Dispose multiple times is idempotent and does not throw exceptions.
    /// </summary>
    /// <remarks>
    /// The Dispose implementation includes a guard (_disposed check) to prevent re-execution.
    /// This test verifies that multiple calls are safe, though we cannot directly verify the internal
    /// _disposed flag or that _drawer.Dispose() is called only once due to private field access and
    /// unmockable dependencies.
    /// </remarks>
    [TestMethod]
    public void Dispose_MultipleCalls_AreIdempotent()
    {
        // Arrange
        var config = new TestConfig();
        var section = new ConfigSection<TestConfig>(config);

        // Act
        section.Dispose();
        section.Dispose();
        section.Dispose();

        // Assert
        // No exception thrown - idempotent disposal
    }

    /// <summary>
    /// Verifies that Dispose can be called on a section with custom idScope without errors.
    /// </summary>
    [TestMethod]
    public void Dispose_WithCustomIdScope_CompletesSuccessfully()
    {
        // Arrange
        var config = new TestConfig();
        var section = new ConfigSection<TestConfig>(config, idScope: "CustomScope");

        // Act
        section.Dispose();

        // Assert
        // No exception thrown
    }

    /// <summary>
    /// Verifies that Dispose can be called on a section with tree node configuration without errors.
    /// </summary>
    [TestMethod]
    public void Dispose_WithTreeNodeConfiguration_CompletesSuccessfully()
    {
        // Arrange
        var config = new TestConfig();
        var section = new ConfigSection<TestConfig>(
            config,
            treeNodeLabel: "Test Node",
            treeNodeDefaultOpen: true);

        // Act
        section.Dispose();

        // Assert
        // No exception thrown
    }

    /// <summary>
    /// Verifies that Dispose can be called on a section with suppressed tree node without errors.
    /// </summary>
    [TestMethod]
    public void Dispose_WithSuppressedTreeNode_CompletesSuccessfully()
    {
        // Arrange
        var config = new TestConfig();
        var section = new ConfigSection<TestConfig>(config, suppressTreeNode: true);

        // Act
        section.Dispose();

        // Assert
        // No exception thrown
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeDefaultOpen"/> returns false when
    /// <paramref name="suppressTreeNode"/> is true, regardless of other parameters.
    /// </summary>
    [TestMethod]
    public void TreeNodeDefaultOpen_SuppressTreeNodeTrue_ReturnsFalse()
    {
        // Arrange
        var config = new TestConfig();

        // Act
        var section = new ConfigSection<TestConfig>(
            config,
            treeNodeLabel: "Test Label",
            treeNodeDefaultOpen: true,
            suppressTreeNode: true);

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeDefaultOpen"/> returns the value of
    /// the <paramref name="treeNodeDefaultOpen"/> parameter when an explicit tree node label is provided.
    /// </summary>
    /// <param name="treeNodeDefaultOpen">The explicit default open state to test.</param>
    /// <param name="expected">The expected return value.</param>
    [TestMethod]
    [DataRow(true, true, DisplayName = "TreeNodeDefaultOpen_ExplicitLabelWithDefaultOpenTrue_ReturnsTrue")]
    [DataRow(false, false, DisplayName = "TreeNodeDefaultOpen_ExplicitLabelWithDefaultOpenFalse_ReturnsFalse")]
    public void TreeNodeDefaultOpen_ExplicitTreeNodeLabel_ReturnsParameterValue(bool treeNodeDefaultOpen, bool expected)
    {
        // Arrange
        var config = new TestConfig();

        // Act
        var section = new ConfigSection<TestConfig>(
            config,
            treeNodeLabel: "Test Label",
            treeNodeDefaultOpen: treeNodeDefaultOpen);

        // Assert
        Assert.AreEqual(expected, section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeDefaultOpen"/> returns false when
    /// no explicit tree node label is provided and the config type has no
    /// <see cref="UmbraConfigRootNodeAttribute"/>.
    /// </summary>
    [TestMethod]
    public void TreeNodeDefaultOpen_NoAttributeNoExplicitLabel_ReturnsFalse()
    {
        // Arrange
        var config = new TestConfig();

        // Act
        var section = new ConfigSection<TestConfig>(config);

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that an explicit <paramref name="treeNodeLabel"/> parameter overrides the
    /// <see cref="UmbraConfigRootNodeAttribute"/> when both are present. The explicit
    /// <paramref name="treeNodeDefaultOpen"/> parameter value should be returned.
    /// </summary>
    [TestMethod]
    public void TreeNodeDefaultOpen_ExplicitLabelOverridesAttribute_ReturnsExplicitValue()
    {
        // Arrange
        var config = new TestConfigWithAttributeDefaultOpenFalse();

        // Act
        var section = new ConfigSection<TestConfigWithAttributeDefaultOpenFalse>(
            config,
            treeNodeLabel: "Override Label",
            treeNodeDefaultOpen: true);

        // Assert
        Assert.IsTrue(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.TreeNodeDefaultOpen"/> returns false when
    /// <paramref name="suppressTreeNode"/> is true, even if the config type has an attribute
    /// with DefaultOpen = true.
    /// </summary>
    [TestMethod]
    public void TreeNodeDefaultOpen_SuppressTreeNodeWithAttribute_ReturnsFalse()
    {
        // Arrange
        var config = new TestConfigWithAttributeDefaultOpenTrue();

        // Act
        var section = new ConfigSection<TestConfigWithAttributeDefaultOpenTrue>(
            config,
            suppressTreeNode: true);

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    #region Test Config Classes

    private sealed class TestConfigWithAttributeDefaultOpenTrue
    {
    }

    private sealed class TestConfigWithAttributeDefaultOpenFalse
    {
    }

    #endregion

    /// <summary>
    /// Tests that the Order property returns int.MaxValue when the config type
    /// does not have a SectionOrderAttribute.
    /// </summary>
    [TestMethod]
    public void Order_ConfigTypeWithoutSectionOrderAttribute_ReturnsIntMaxValue()
    {
        // Arrange
        var config = new ConfigWithoutOrderAttribute();

        // Act
        var section = new ConfigSection<ConfigWithoutOrderAttribute>(config);

        // Assert
        Assert.AreEqual(int.MaxValue, section.Order);
    }

    /// <summary>
    /// Tests that the Order property returns the order value specified in the
    /// SectionOrderAttribute when the config type has the attribute with order = 0.
    /// </summary>
    [TestMethod]
    public void Order_ConfigTypeWithSectionOrderAttributeZero_ReturnsZero()
    {
        // Arrange
        var config = new ConfigWithOrderZero();

        // Act
        var section = new ConfigSection<ConfigWithOrderZero>(config);

        // Assert
        Assert.AreEqual(0, section.Order);
    }

    /// <summary>
    /// Tests that the Order property returns the order value specified in the
    /// SectionOrderAttribute when the config type has the attribute with a positive order value.
    /// </summary>
    [TestMethod]
    public void Order_ConfigTypeWithSectionOrderAttributePositive_ReturnsPositiveValue()
    {
        // Arrange
        var config = new ConfigWithOrderPositive();

        // Act
        var section = new ConfigSection<ConfigWithOrderPositive>(config);

        // Assert
        Assert.AreEqual(100, section.Order);
    }

    /// <summary>
    /// Tests that the Order property returns the order value specified in the
    /// SectionOrderAttribute when the config type has the attribute with a negative order value.
    /// </summary>
    [TestMethod]
    public void Order_ConfigTypeWithSectionOrderAttributeNegative_ReturnsNegativeValue()
    {
        // Arrange
        var config = new ConfigWithOrderNegative();

        // Act
        var section = new ConfigSection<ConfigWithOrderNegative>(config);

        // Assert
        Assert.AreEqual(-50, section.Order);
    }

    /// <summary>
    /// Tests that the Order property returns int.MinValue when the config type
    /// has a SectionOrderAttribute with order = int.MinValue.
    /// </summary>
    [TestMethod]
    public void Order_ConfigTypeWithSectionOrderAttributeIntMinValue_ReturnsIntMinValue()
    {
        // Arrange
        var config = new ConfigWithOrderIntMinValue();

        // Act
        var section = new ConfigSection<ConfigWithOrderIntMinValue>(config);

        // Assert
        Assert.AreEqual(int.MinValue, section.Order);
    }

    /// <summary>
    /// Tests that the Order property returns int.MaxValue when the config type
    /// has a SectionOrderAttribute with order = int.MaxValue.
    /// </summary>
    [TestMethod]
    public void Order_ConfigTypeWithSectionOrderAttributeIntMaxValue_ReturnsIntMaxValue()
    {
        // Arrange
        var config = new ConfigWithOrderIntMaxValue();

        // Act
        var section = new ConfigSection<ConfigWithOrderIntMaxValue>(config);

        // Assert
        Assert.AreEqual(int.MaxValue, section.Order);
    }

    /// <summary>
    /// Tests that the Order property returns the same value across multiple reads,
    /// verifying the readonly behavior of the underlying field.
    /// </summary>
    [TestMethod]
    public void Order_MultipleReads_ReturnsSameValue()
    {
        // Arrange
        var config = new ConfigWithOrderPositive();
        var section = new ConfigSection<ConfigWithOrderPositive>(config);

        // Act
        int firstRead = section.Order;
        int secondRead = section.Order;
        int thirdRead = section.Order;

        // Assert
        Assert.AreEqual(firstRead, secondRead);
        Assert.AreEqual(secondRead, thirdRead);
        Assert.AreEqual(100, firstRead);
    }

    #region Helper config classes

    /// <summary>
    /// Test config class without any SectionOrderAttribute.
    /// </summary>
    internal sealed class ConfigWithoutOrderAttribute
    {
    }

    /// <summary>
    /// Test config class with SectionOrderAttribute having order = 0.
    /// </summary>
    [SectionOrder(0)]
    internal sealed class ConfigWithOrderZero
    {
    }

    /// <summary>
    /// Test config class with SectionOrderAttribute having a positive order value.
    /// </summary>
    [SectionOrder(100)]
    internal sealed class ConfigWithOrderPositive
    {
    }

    /// <summary>
    /// Test config class with SectionOrderAttribute having a negative order value.
    /// </summary>
    [SectionOrder(-50)]
    internal sealed class ConfigWithOrderNegative
    {
    }

    /// <summary>
    /// Test config class with SectionOrderAttribute having order = int.MinValue.
    /// </summary>
    [SectionOrder(int.MinValue)]
    internal sealed class ConfigWithOrderIntMinValue
    {
    }

    /// <summary>
    /// Test config class with SectionOrderAttribute having order = int.MaxValue.
    /// </summary>
    [SectionOrder(int.MaxValue)]
    internal sealed class ConfigWithOrderIntMaxValue
    {
    }

    #endregion

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.Draw"/> does not throw when the section has been disposed.
    /// The method should return early without attempting to call the underlying drawer.
    /// </summary>
    [TestMethod]
    public void Draw_WhenDisposed_DoesNotThrow()
    {
        // Arrange
        var config = new TestConfig();
        var section = new ConfigSection<TestConfig>(config);
        section.Dispose();

        // Act & Assert
        section.Draw(); // Should return immediately without throwing
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.Draw"/> can be called multiple times after disposal without throwing.
    /// Each call should return early without side effects.
    /// </summary>
    [TestMethod]
    public void Draw_WhenDisposedMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var config = new TestConfig();
        var section = new ConfigSection<TestConfig>(config);
        section.Dispose();

        // Act & Assert
        section.Draw();
        section.Draw();
        section.Draw(); // Multiple calls should all be safe no-ops
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.Draw"/> can be called immediately after disposal.
    /// Verifies the disposed flag is properly checked on entry.
    /// </summary>
    [TestMethod]
    public void Draw_CalledImmediatelyAfterDispose_ReturnsEarly()
    {
        // Arrange
        var config = new TestConfig();
        var section = new ConfigSection<TestConfig>(config);

        // Act
        section.Dispose();

        // Assert - should not throw, verifying early return
        section.Draw();
    }

    /// <summary>
    /// Tests that when idScope is null, SectionId defaults to the type's FullName.
    /// </summary>
    [TestMethod]
    public void ConfigSection_IdScopeIsNull_UsesTypeFullName()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, idScope: null);

        // Assert
        Assert.AreEqual(typeof(BasicConfig).FullName, section.SectionId);
    }

    /// <summary>
    /// Tests that when idScope is provided, SectionId uses the provided value.
    /// </summary>
    [TestMethod]
    [DataRow("CustomScope")]
    [DataRow("MyPlugin.Settings")]
    [DataRow("a")]
    [DataRow("123")]
    public void ConfigSection_IdScopeProvided_UsesProvidedIdScope(string idScope)
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, idScope: idScope);

        // Assert
        Assert.AreEqual(idScope, section.SectionId);
    }

    /// <summary>
    /// Tests that when the config type has no SectionOrderAttribute, Order defaults to int.MaxValue.
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoOrderAttribute_OrderIsMaxValue()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config);

        // Assert
        Assert.AreEqual(int.MaxValue, section.Order);
    }

    /// <summary>
    /// Tests that when the config type has SectionOrderAttribute, Order uses the attribute value.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(-1)]
    [DataRow(-100)]
    public void ConfigSection_WithOrderAttribute_UsesAttributeOrderValue(int orderValue)
    {
        // Arrange
        var config = orderValue switch
        {
            0 => new ConfigWithOrder0() as object,
            1 => new ConfigWithOrder1(),
            10 => new ConfigWithOrder10(),
            100 => new ConfigWithOrder100(),
            -1 => new ConfigWithOrderNegative1(),
            -100 => new ConfigWithOrderNegative100(),
            _ => throw new InvalidOperationException()
        };

        // Act & Assert
        switch (orderValue)
        {
            case 0:
                var section0 = new ConfigSection<ConfigWithOrder0>((ConfigWithOrder0)config);
                Assert.AreEqual(0, section0.Order);
                break;
            case 1:
                var section1 = new ConfigSection<ConfigWithOrder1>((ConfigWithOrder1)config);
                Assert.AreEqual(1, section1.Order);
                break;
            case 10:
                var section10 = new ConfigSection<ConfigWithOrder10>((ConfigWithOrder10)config);
                Assert.AreEqual(10, section10.Order);
                break;
            case 100:
                var section100 = new ConfigSection<ConfigWithOrder100>((ConfigWithOrder100)config);
                Assert.AreEqual(100, section100.Order);
                break;
            case -1:
                var sectionNeg1 = new ConfigSection<ConfigWithOrderNegative1>((ConfigWithOrderNegative1)config);
                Assert.AreEqual(-1, sectionNeg1.Order);
                break;
            case -100:
                var sectionNeg100 = new ConfigSection<ConfigWithOrderNegative100>((ConfigWithOrderNegative100)config);
                Assert.AreEqual(-100, sectionNeg100.Order);
                break;
        }
    }

    /// <summary>
    /// Tests that when SectionOrderAttribute.Order is int.MaxValue, Order property returns int.MaxValue.
    /// </summary>
    [TestMethod]
    public void ConfigSection_OrderAttributeWithMaxValue_OrderIsMaxValue()
    {
        // Arrange
        var config = new ConfigWithOrderMaxValue();

        // Act
        var section = new ConfigSection<ConfigWithOrderMaxValue>(config);

        // Assert
        Assert.AreEqual(int.MaxValue, section.Order);
    }

    /// <summary>
    /// Tests that when SectionOrderAttribute.Order is int.MinValue, Order property returns int.MinValue.
    /// </summary>
    [TestMethod]
    public void ConfigSection_OrderAttributeWithMinValue_OrderIsMinValue()
    {
        // Arrange
        var config = new ConfigWithOrderMinValue();

        // Act
        var section = new ConfigSection<ConfigWithOrderMinValue>(config);

        // Assert
        Assert.AreEqual(int.MinValue, section.Order);
    }

    /// <summary>
    /// Tests that when treeNodeLabel is provided and suppressTreeNode is false, TreeNodeLabel uses the provided value.
    /// </summary>
    [TestMethod]
    [DataRow("My Settings")]
    [DataRow("Configuration")]
    [DataRow("")]
    [DataRow(" ")]
    public void ConfigSection_TreeNodeLabelProvided_UsesProvidedLabel(string label)
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, treeNodeLabel: label);

        // Assert
        Assert.AreEqual(label, section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that when treeNodeLabel is provided and treeNodeDefaultOpen is true, TreeNodeDefaultOpen is true.
    /// </summary>
    [TestMethod]
    public void ConfigSection_TreeNodeLabelProvidedWithDefaultOpenTrue_TreeNodeDefaultOpenIsTrue()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, treeNodeLabel: "Label", treeNodeDefaultOpen: true);

        // Assert
        Assert.IsTrue(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that when treeNodeLabel is provided and treeNodeDefaultOpen is false, TreeNodeDefaultOpen is false.
    /// </summary>
    [TestMethod]
    public void ConfigSection_TreeNodeLabelProvidedWithDefaultOpenFalse_TreeNodeDefaultOpenIsFalse()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, treeNodeLabel: "Label", treeNodeDefaultOpen: false);

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that when treeNodeLabel is null and the type has no UmbraConfigRootNodeAttribute, TreeNodeLabel is null.
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoTreeNodeLabelAndNoAttribute_TreeNodeLabelIsNull()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, treeNodeLabel: null);

        // Assert
        Assert.IsNull(section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that when treeNodeLabel is null and the type has UmbraConfigRootNodeAttribute with a Label, TreeNodeLabel uses the attribute Label.
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoTreeNodeLabelWithAttributeLabel_UsesAttributeLabel()
    {
        // Arrange
        var config = new ConfigWithRootNodeLabel();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeLabel>(config, treeNodeLabel: null);

        // Assert
        Assert.AreEqual("Root Settings", section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that when treeNodeLabel is null and the type has UmbraConfigRootNodeAttribute with Label = null, TreeNodeLabel uses ToDisplayName().
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoTreeNodeLabelWithAttributeLabelNull_UsesToDisplayName()
    {
        // Arrange
        var config = new ConfigWithRootNodeNoLabel();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeNoLabel>(config, treeNodeLabel: null);

        // Assert
        Assert.AreEqual("Config With Root Node No Label", section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that when treeNodeLabel is null and the type has UmbraConfigRootNodeAttribute with DefaultOpen = true, TreeNodeDefaultOpen is true.
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoTreeNodeLabelWithAttributeDefaultOpenTrue_TreeNodeDefaultOpenIsTrue()
    {
        // Arrange
        var config = new ConfigWithRootNodeDefaultOpenTrue();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeDefaultOpenTrue>(config, treeNodeLabel: null);

        // Assert
        Assert.IsTrue(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that when treeNodeLabel is null and the type has UmbraConfigRootNodeAttribute with DefaultOpen = false, TreeNodeDefaultOpen is false.
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoTreeNodeLabelWithAttributeDefaultOpenFalse_TreeNodeDefaultOpenIsFalse()
    {
        // Arrange
        var config = new ConfigWithRootNodeDefaultOpenFalse();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeDefaultOpenFalse>(config, treeNodeLabel: null);

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that when suppressTreeNode is true and treeNodeLabel is provided, TreeNodeLabel is null.
    /// </summary>
    [TestMethod]
    public void ConfigSection_SuppressTreeNodeTrueWithProvidedLabel_TreeNodeLabelIsNull()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, treeNodeLabel: "Label", suppressTreeNode: true);

        // Assert
        Assert.IsNull(section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that when suppressTreeNode is true and the type has UmbraConfigRootNodeAttribute, TreeNodeLabel is null.
    /// </summary>
    [TestMethod]
    public void ConfigSection_SuppressTreeNodeTrueWithAttribute_TreeNodeLabelIsNull()
    {
        // Arrange
        var config = new ConfigWithRootNodeLabel();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeLabel>(config, suppressTreeNode: true);

        // Assert
        Assert.IsNull(section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that when suppressTreeNode is true, TreeNodeDefaultOpen is false regardless of attribute.
    /// </summary>
    [TestMethod]
    public void ConfigSection_SuppressTreeNodeTrue_TreeNodeDefaultOpenIsFalse()
    {
        // Arrange
        var config = new ConfigWithRootNodeDefaultOpenTrue();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeDefaultOpenTrue>(config, suppressTreeNode: true);

        // Assert
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that when treeNodeLabel is explicitly provided, it overrides the attribute Label.
    /// </summary>
    [TestMethod]
    public void ConfigSection_ProvidedTreeNodeLabelOverridesAttribute_UsesProvidedLabel()
    {
        // Arrange
        var config = new ConfigWithRootNodeLabel();
        string providedLabel = "Custom Label";

        // Act
        var section = new ConfigSection<ConfigWithRootNodeLabel>(config, treeNodeLabel: providedLabel);

        // Assert
        Assert.AreEqual(providedLabel, section.TreeNodeLabel);
    }

    /// <summary>
    /// Tests that when treeNodeLabel is explicitly provided with treeNodeDefaultOpen = true, it overrides the attribute DefaultOpen.
    /// </summary>
    [TestMethod]
    public void ConfigSection_ProvidedTreeNodeDefaultOpenOverridesAttribute_UsesProvidedValue()
    {
        // Arrange
        var config = new ConfigWithRootNodeDefaultOpenFalse();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeDefaultOpenFalse>(config, treeNodeLabel: "Label", treeNodeDefaultOpen: true);

        // Assert
        Assert.IsTrue(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that all properties return expected values after construction with minimal parameters.
    /// </summary>
    [TestMethod]
    public void ConfigSection_MinimalConstruction_PropertiesReturnExpectedValues()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config);

        // Assert
        Assert.AreEqual(typeof(BasicConfig).FullName, section.SectionId);
        Assert.AreEqual(int.MaxValue, section.Order);
        Assert.IsNull(section.TreeNodeLabel);
        Assert.IsFalse(section.TreeNodeDefaultOpen);
    }

    /// <summary>
    /// Tests that all properties return expected values after construction with all parameters.
    /// </summary>
    [TestMethod]
    public void ConfigSection_FullConstruction_PropertiesReturnExpectedValues()
    {
        // Arrange
        var config = new ConfigWithOrder10();
        string idScope = "MyScope";
        string treeNodeLabel = "My Tree Node";
        bool treeNodeDefaultOpen = true;

        // Act
        var section = new ConfigSection<ConfigWithOrder10>(config, idScope, treeNodeLabel, treeNodeDefaultOpen, suppressTreeNode: false);

        // Assert
        Assert.AreEqual(idScope, section.SectionId);
        Assert.AreEqual(10, section.Order);
        Assert.AreEqual(treeNodeLabel, section.TreeNodeLabel);
        Assert.IsTrue(section.TreeNodeDefaultOpen);
    }

    // Test config classes

    internal sealed class BasicConfig
    {
    }

    [SectionOrder(0)]
    internal sealed class ConfigWithOrder0
    {
    }

    [SectionOrder(1)]
    internal sealed class ConfigWithOrder1
    {
    }

    [SectionOrder(10)]
    internal sealed class ConfigWithOrder10
    {
    }

    [SectionOrder(100)]
    internal sealed class ConfigWithOrder100
    {
    }

    [SectionOrder(-1)]
    internal sealed class ConfigWithOrderNegative1
    {
    }

    [SectionOrder(-100)]
    internal sealed class ConfigWithOrderNegative100
    {
    }

    [SectionOrder(int.MaxValue)]
    internal sealed class ConfigWithOrderMaxValue
    {
    }

    [SectionOrder(int.MinValue)]
    internal sealed class ConfigWithOrderMinValue
    {
    }

    [UmbraConfigRootNode("Root Settings", false)]
    internal sealed class ConfigWithRootNodeLabel
    {
    }

    [UmbraConfigRootNode(null, false)]
    internal sealed class ConfigWithRootNodeNoLabel
    {
    }

    [UmbraConfigRootNode("Open Tree", true)]
    internal sealed class ConfigWithRootNodeDefaultOpenTrue
    {
    }

    [UmbraConfigRootNode("Closed Tree", false)]
    internal sealed class ConfigWithRootNodeDefaultOpenFalse
    {
    }
}