using Umbra.Config.Attributes;


namespace Umbra.Config.UnitTests;

/// <summary>
/// Tests for <see cref="ConfigRegistrar.Register{TConfig}(TConfig)"/>.
/// </summary>
[TestClass]
public partial class ConfigRegistrarTests
{
    /// <summary>
    /// Tests that Register returns an empty dictionary when the config type
    /// lacks the <see cref="UmbraAutoRegisterAttribute"/> attribute.
    /// </summary>
    [TestMethod]
    public void Register_ConfigWithoutAttribute_ReturnsEmptyDictionary()
    {
        // Arrange
        var config = new ConfigWithoutAttribute();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that Register returns an empty dictionary when the config has
    /// the attribute but no <see cref="UmbraParameterAttribute"> properties.
    /// </summary>
    [TestMethod]
    public void Register_ConfigWithNoParameters_ReturnsEmptyDictionary()
    {
        // Arrange
        var config = new EmptyConfig();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that Register correctly discovers and registers a single simple parameter.
    /// </summary>
    [TestMethod]
    public void Register_SimpleParameter_RegistersParameterWithCorrectKey()
    {
        // Arrange
        var config = new SimpleConfig();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("enabled"));
        Assert.AreSame(config.Enabled, result["enabled"]);
        Assert.AreEqual("enabled", config.Enabled.Key);
    }

    /// <summary>
    /// Tests that Register correctly applies a class-level prefix to all registered parameters.
    /// </summary>
    [TestMethod]
    public void Register_ConfigWithPrefix_PrependsPrefix()
    {
        // Arrange
        var config = new ConfigWithPrefix();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(2, result);
        Assert.IsTrue(result.ContainsKey("myPrefix.enabled"));
        Assert.IsTrue(result.ContainsKey("myPrefix.value"));
        Assert.AreSame(config.Enabled, result["myPrefix.enabled"]);
        Assert.AreSame(config.Value, result["myPrefix.value"]);
    }

    /// <summary>
    /// Tests that Register correctly applies a class-level category to all registered parameters.
    /// </summary>
    [TestMethod]
    public void Register_ConfigWithCategory_SetsCategory()
    {
        // Arrange
        var config = new ConfigWithCategory();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("enabled"));
        Assert.IsNotNull(config.Enabled.Metadata);
        Assert.AreEqual("TestCategory", config.Enabled.Metadata.Category);
    }

    /// <summary>
    /// Tests that Register correctly applies both prefix and category together.
    /// </summary>
    [TestMethod]
    public void Register_ConfigWithPrefixAndCategory_AppliesBoth()
    {
        // Arrange
        var config = new ConfigWithPrefixAndCategory();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("pre.enabled"));
        Assert.IsNotNull(config.Enabled.Metadata);
        Assert.AreEqual("Cat", config.Enabled.Metadata.Category);
    }

    /// <summary>
    /// Tests that Register uses a custom key override when specified on a parameter attribute.
    /// </summary>
    [TestMethod]
    public void Register_ParameterWithKeyOverride_UsesOverride()
    {
        // Arrange
        var config = new ConfigWithKeyOverride();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("customKey"));
        Assert.AreSame(config.Enabled, result["customKey"]);
    }

    /// <summary>
    /// Tests that Register correctly discovers and registers multiple parameters
    /// from the same configuration object.
    /// </summary>
    [TestMethod]
    public void Register_MultipleParameters_RegistersAll()
    {
        // Arrange
        var config = new ConfigWithMultipleParameters();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(3, result);
        Assert.IsTrue(result.ContainsKey("enabled"));
        Assert.IsTrue(result.ContainsKey("count"));
        Assert.IsTrue(result.ContainsKey("name"));
        Assert.AreSame(config.Enabled, result["enabled"]);
        Assert.AreSame(config.Count, result["count"]);
        Assert.AreSame(config.Name, result["name"]);
    }

    /// <summary>
    /// Tests that Register correctly recurses into a nested group
    /// and registers nested parameters with a combined prefix.
    /// </summary>
    [TestMethod]
    public void Register_NestedGroup_RegistersNestedParameters()
    {
        // Arrange
        var config = new ConfigWithNestedGroup();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(2, result);
        Assert.IsTrue(result.ContainsKey("topLevel"));
        Assert.IsTrue(result.ContainsKey("nested.nestedValue"));
        Assert.AreSame(config.TopLevel, result["topLevel"]);
        Assert.AreSame(config.Nested.NestedValue, result["nested.nestedValue"]);
    }

    /// <summary>
    /// Tests that Register correctly applies a property-level prefix to a nested group.
    /// </summary>
    [TestMethod]
    public void Register_NestedGroupWithPropertyPrefix_AppliesPropertyPrefix()
    {
        // Arrange
        var config = new ConfigWithNestedPropertyPrefix();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("customPrefix.nestedValue"));
        Assert.AreSame(config.Nested.NestedValue, result["customPrefix.nestedValue"]);
    }

    /// <summary>
    /// Tests that Register correctly combines multiple levels of prefixes
    /// when dealing with deeply nested group.
    /// </summary>
    [TestMethod]
    public void Register_DeeplyNestedGroup_CombinesPrefixesCorrectly()
    {
        // Arrange
        var config = new ConfigWithDeeplyNestedGroup();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("root.level1.level2.deepValue"));
        Assert.AreSame(config.Level1.Level2.DeepValue, result["root.level1.level2.deepValue"]);
    }

    /// <summary>
    /// Tests that Register handles circular references gracefully without
    /// infinite recursion by tracking visited objects.
    /// </summary>
    [TestMethod]
    public void Register_CircularReference_DoesNotInfinitelyRecurse()
    {
        // Arrange
        var config = new ConfigWithCircularReference();
        config.Other.Other = config; // Create circular reference

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.ContainsKey("value1"));
        Assert.IsTrue(result.ContainsKey("other.value2"));
    }

    /// <summary>
    /// Tests that Register skips properties with null values.
    /// </summary>
    [TestMethod]
    public void Register_NullPropertyValue_SkipsProperty()
    {
        // Arrange
        var config = new ConfigWithNullProperty
        {
            Nested = null
        };

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("topLevel"));
    }

    /// <summary>
    /// Tests that Register correctly handles empty prefix and category values.
    /// </summary>
    [TestMethod]
    public void Register_EmptyPrefixAndCategory_HandlesGracefully()
    {
        // Arrange
        var config = new ConfigWithEmptyPrefix();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("enabled"));
    }

    /// <summary>
    /// Tests that Register returns a new dictionary instance.
    /// </summary>
    [TestMethod]
    public void Register_ReturnsNewDictionaryInstance()
    {
        // Arrange
        var config = new SimpleConfig();

        // Act
        var result1 = ConfigRegistrar.Register(config);
        var result2 = ConfigRegistrar.Register(config);

        // Assert
        Assert.AreNotSame(result1, result2);
    }

    /// <summary>
    /// Tests that Register only processes public instance properties.
    /// </summary>
    [TestMethod]
    public void Register_OnlyProcessesPublicInstanceProperties()
    {
        // Arrange
        var config = new ConfigWithPrivateProperty();

        // Act
        var result = ConfigRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("publicParameter"));
    }

    /// <summary>
    /// Tests that Register throws when two parameters resolve to the same fully-qualified key.
    /// </summary>
    [TestMethod]
    public void Register_DuplicateResolvedKeys_ThrowsInvalidOperationException()
    {
        var config = new ConfigWithDuplicateKeys();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => ConfigRegistrar.Register(config));

        Assert.Contains("Duplicate parameter key 'enabled'", exception.Message);
        Assert.Contains(nameof(ConfigWithDuplicateKeys.Enabled1), exception.Message);
        Assert.Contains(nameof(ConfigWithDuplicateKeys.Enabled2), exception.Message);
    }

    /// <summary>
    /// Tests that a property-level nested-group prefix wins over a type-level prefix.
    /// </summary>
    [TestMethod]
    public void Register_PropertyPrefixOverridesNestedTypePrefix()
    {
        var config = new ConfigWithPropertyAndTypePrefix();

        var result = ConfigRegistrar.Register(config);

        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("propertyPrefix.nestedValue"));
        Assert.IsFalse(result.ContainsKey("typePrefix.nestedValue"));
    }

    /// <summary>
    /// Tests that a property-level category wins over nested-type and inherited categories.
    /// </summary>
    [TestMethod]
    public void Register_PropertyCategoryOverridesNestedTypeAndInheritedCategory()
    {
        var config = new ConfigWithPropertyAndTypeCategory();

        _ = ConfigRegistrar.Register(config);

        Assert.AreEqual("PropertyCategory", config.Nested.NestedValue.Metadata.Category);
    }

    /// <summary>
    /// Tests that an explicitly empty key override is rejected.
    /// </summary>
    [TestMethod]
    public void Register_EmptyKeyOverride_ThrowsInvalidOperationException()
    {
        var config = new ConfigWithEmptyKeyOverride();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => ConfigRegistrar.Register(config));

        Assert.Contains("empty string", exception.Message);
        Assert.Contains(nameof(ConfigWithEmptyKeyOverride.Enabled), exception.Message);
    }

    /// <summary>
    /// Tests that an explicitly empty nested property prefix is rejected.
    /// </summary>
    [TestMethod]
    public void Register_EmptyNestedPropertyPrefix_ThrowsInvalidOperationException()
    {
        var config = new ConfigWithEmptyNestedPropertyPrefix();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => ConfigRegistrar.Register(config));

        Assert.Contains("empty string", exception.Message);
        Assert.Contains(nameof(ConfigWithEmptyNestedPropertyPrefix.Nested), exception.Message);
    }

    /// <summary>
    /// Tests that an explicitly empty nested type prefix is rejected.
    /// </summary>
    [TestMethod]
    public void Register_EmptyNestedTypePrefix_ThrowsInvalidOperationException()
    {
        var config = new ConfigWithEmptyNestedTypePrefix();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => ConfigRegistrar.Register(config));

        Assert.Contains("empty string", exception.Message);
        Assert.Contains(nameof(EmptyPrefixNestedGroup), exception.Message);
    }

    /// <summary>
    /// Tests that nested objects without the auto-register attribute are ignored.
    /// </summary>
    [TestMethod]
    public void Register_NestedObjectWithoutAutoRegisterAttribute_IsIgnored()
    {
        var config = new ConfigWithNonAutoRegisterNestedObject();

        var result = ConfigRegistrar.Register(config);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that a nested type-level category is used when the parent property declares no category.
    /// </summary>
    [TestMethod]
    public void Register_NestedTypeCategoryWithoutPropertyCategory_UsesNestedTypeCategory()
    {
        var config = new ConfigWithNestedTypeCategoryOnly();

        var result = ConfigRegistrar.Register(config);

        Assert.HasCount(1, result);
        Assert.AreEqual("NestedTypeCategory", config.Nested.NestedValue.Metadata.Category);
    }

    /// <summary>
    /// Tests that a nested type-level prefix is used when the parent property declares no prefix.
    /// </summary>
    [TestMethod]
    public void Register_NestedTypePrefixWithoutPropertyPrefix_UsesNestedTypePrefix()
    {
        var config = new ConfigWithNestedTypePrefixOnly();

        var result = ConfigRegistrar.Register(config);

        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("typeOnlyPrefix.nestedValue"));
    }

    // Test helper classes
    [UmbraAutoRegister]
    internal class SimpleConfig
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    internal class ConfigWithoutAttribute
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegister]
    internal class EmptyConfig
    {
    }

    [UmbraAutoRegister]
    [UmbraPrefix("myPrefix")]
    internal class ConfigWithPrefix
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);

        [UmbraParameter]
        public Parameter<int> Value { get; set; } = new(42);
    }

    [UmbraAutoRegister]
    [UmbraCategory("TestCategory")]
    internal class ConfigWithCategory
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegister]
    [UmbraPrefix("pre")]
    [UmbraCategory("Cat")]
    internal class ConfigWithPrefixAndCategory
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegister]
    internal class ConfigWithKeyOverride
    {
        [UmbraParameter("customKey")]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegister]
    internal class ConfigWithMultipleParameters
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);

        [UmbraParameter]
        public Parameter<int> Count { get; set; } = new(10);

        [UmbraParameter]
        public Parameter<string> Name { get; set; } = new("test");
    }

    [UmbraAutoRegister]
    internal class ConfigWithNestedGroup
    {
        [UmbraParameter]
        public Parameter<bool> TopLevel { get; set; } = new(true);

        [UmbraParameter]
        [UmbraPrefix("nested")]
        public NestedGroup Nested { get; set; } = new();
    }

    [UmbraAutoRegister]
    internal class NestedGroup
    {
        [UmbraParameter]
        public Parameter<int> NestedValue { get; set; } = new(100);
    }

    [UmbraAutoRegister]
    internal class ConfigWithNestedPropertyPrefix
    {
        [UmbraParameter]
        [UmbraPrefix("customPrefix")]
        public NestedGroup Nested { get; set; } = new();
    }

    [UmbraAutoRegister]
    [UmbraPrefix("root")]
    internal class ConfigWithDeeplyNestedGroup
    {
        [UmbraParameter]
        [UmbraPrefix("level1")]
        public Level1Group Level1 { get; set; } = new();
    }

    [UmbraAutoRegister]
    internal class Level1Group
    {
        [UmbraParameter]
        [UmbraPrefix("level2")]
        public Level2Group Level2 { get; set; } = new();
    }

    [UmbraAutoRegister]
    internal class Level2Group
    {
        [UmbraParameter]
        public Parameter<string> DeepValue { get; set; } = new("deep");
    }

    [UmbraAutoRegister]
    internal class ConfigWithDuplicateKeys
    {
        [UmbraParameter("enabled")]
        public Parameter<bool> Enabled1 { get; set; } = new(true);

        [UmbraParameter("enabled")]
        public Parameter<bool> Enabled2 { get; set; } = new(false);
    }

    [UmbraAutoRegister]
    internal class ConfigWithCircularReference
    {
        [UmbraParameter]
        public Parameter<int> Value1 { get; set; } = new(1);

        [UmbraParameter]
        [UmbraPrefix("other")]
        public ConfigWithCircularReference2 Other { get; set; } = new();
    }

    [UmbraAutoRegister]
    internal class ConfigWithCircularReference2
    {
        [UmbraParameter]
        public Parameter<int> Value2 { get; set; } = new(2);

        [UmbraParameter]
        [UmbraPrefix("circular")]
        public ConfigWithCircularReference? Other { get; set; }
    }

    [UmbraAutoRegister]
    internal class ConfigWithNullProperty
    {
        [UmbraParameter]
        public Parameter<bool> TopLevel { get; set; } = new(true);

        [UmbraParameter]
        [UmbraPrefix("nested")]
        public NestedGroup? Nested { get; set; } = new();
    }

    [UmbraAutoRegister]
    [UmbraPrefix("")]
    [UmbraCategory("")]
    internal class ConfigWithEmptyPrefix
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegister]
    internal class ConfigWithPrivateProperty
    {
        [UmbraParameter]
        public Parameter<bool> PublicParameter { get; set; } = new(true);

        [UmbraParameter]
        private Parameter<bool> PrivateParameter { get; set; } = new(false);
    }

    [UmbraAutoRegister]
    internal class ConfigWithPropertyAndTypePrefix
    {
        [UmbraParameter]
        [UmbraPrefix("propertyPrefix")]
        public NestedGroupWithTypePrefix Nested { get; set; } = new();
    }

    [UmbraAutoRegister]
    [UmbraPrefix("typePrefix")]
    internal class NestedGroupWithTypePrefix
    {
        [UmbraParameter]
        public Parameter<int> NestedValue { get; set; } = new(5);
    }

    [UmbraAutoRegister]
    [UmbraCategory("InheritedCategory")]
    internal class ConfigWithPropertyAndTypeCategory
    {
        [UmbraParameter]
        [UmbraCategory("PropertyCategory")]
        public NestedGroupWithTypeCategory Nested { get; set; } = new();
    }

    [UmbraAutoRegister]
    [UmbraCategory("TypeCategory")]
    internal class NestedGroupWithTypeCategory
    {
        [UmbraParameter]
        public Parameter<int> NestedValue { get; set; } = new(7);
    }

    [UmbraAutoRegister]
    internal class ConfigWithEmptyKeyOverride
    {
        [UmbraParameter("")]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegister]
    internal class ConfigWithEmptyNestedPropertyPrefix
    {
        [UmbraParameter]
        [UmbraPrefix("")]
        public NestedGroup Nested { get; set; } = new();
    }

    [UmbraAutoRegister]
    internal class ConfigWithEmptyNestedTypePrefix
    {
        [UmbraParameter]
        public EmptyPrefixNestedGroup Nested { get; set; } = new();
    }

    [UmbraAutoRegister]
    [UmbraPrefix("")]
    internal class EmptyPrefixNestedGroup
    {
        [UmbraParameter]
        public Parameter<int> NestedValue { get; set; } = new(9);
    }

    [UmbraAutoRegister]
    internal class ConfigWithNonAutoRegisterNestedObject
    {
        [UmbraParameter]
        public NonAutoRegisterNestedObject Nested { get; set; } = new();
    }

    [UmbraAutoRegister]
    internal class ConfigWithNestedTypeCategoryOnly
    {
        [UmbraParameter]
        public NestedGroupWithStandaloneCategory Nested { get; set; } = new();
    }

    [UmbraAutoRegister]
    [UmbraCategory("NestedTypeCategory")]
    internal class NestedGroupWithStandaloneCategory
    {
        [UmbraParameter]
        public Parameter<int> NestedValue { get; set; } = new(13);
    }

    [UmbraAutoRegister]
    internal class ConfigWithNestedTypePrefixOnly
    {
        [UmbraParameter]
        public NestedGroupWithStandalonePrefix Nested { get; set; } = new();
    }

    [UmbraAutoRegister]
    [UmbraPrefix("typeOnlyPrefix")]
    internal class NestedGroupWithStandalonePrefix
    {
        [UmbraParameter]
        public Parameter<int> NestedValue { get; set; } = new(17);
    }

    internal class NonAutoRegisterNestedObject
    {
        [UmbraParameter]
        public Parameter<int> NestedValue { get; set; } = new(11);
    }
}
