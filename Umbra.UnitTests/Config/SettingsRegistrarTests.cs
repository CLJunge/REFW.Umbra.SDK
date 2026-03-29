using Umbra.Config.Attributes;


namespace Umbra.Config.UnitTests;

/// <summary>
/// Tests for <see cref="SettingsRegistrar.Register{TConfig}(TConfig)"/>.
/// </summary>
[TestClass]
public partial class SettingsRegistrarTests
{
    /// <summary>
    /// Tests that Register returns an empty dictionary when the config type
    /// lacks the [UmbraAutoRegisterSettings] attribute.
    /// </summary>
    [TestMethod]
    public void Register_ConfigWithoutAttribute_ReturnsEmptyDictionary()
    {
        // Arrange
        var config = new ConfigWithoutAttribute();

        // Act
        var result = SettingsRegistrar.Register(config);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that Register returns an empty dictionary when the config has
    /// the attribute but no [UmbraSettingsParameter] properties.
    /// </summary>
    [TestMethod]
    public void Register_ConfigWithNoParameters_ReturnsEmptyDictionary()
    {
        // Arrange
        var config = new EmptyConfig();

        // Act
        var result = SettingsRegistrar.Register(config);

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
        var result = SettingsRegistrar.Register(config);

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
        var result = SettingsRegistrar.Register(config);

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
        var result = SettingsRegistrar.Register(config);

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
        var result = SettingsRegistrar.Register(config);

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
        var result = SettingsRegistrar.Register(config);

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
        var result = SettingsRegistrar.Register(config);

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
    /// Tests that Register correctly recurses into a nested settings group
    /// and registers nested parameters with a combined prefix.
    /// </summary>
    [TestMethod]
    public void Register_NestedSettings_RegistersNestedParameters()
    {
        // Arrange
        var config = new ConfigWithNestedSettings();

        // Act
        var result = SettingsRegistrar.Register(config);

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
    public void Register_NestedSettingsWithPropertyPrefix_AppliesPropertyPrefix()
    {
        // Arrange
        var config = new ConfigWithNestedPropertyPrefix();

        // Act
        var result = SettingsRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("customPrefix.nestedValue"));
        Assert.AreSame(config.Nested.NestedValue, result["customPrefix.nestedValue"]);
    }

    /// <summary>
    /// Tests that Register correctly combines multiple levels of prefixes
    /// when dealing with deeply nested settings.
    /// </summary>
    [TestMethod]
    public void Register_DeeplyNestedSettings_CombinesPrefixesCorrectly()
    {
        // Arrange
        var config = new ConfigWithDeeplyNestedSettings();

        // Act
        var result = SettingsRegistrar.Register(config);

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
        var result = SettingsRegistrar.Register(config);

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
        var result = SettingsRegistrar.Register(config);

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
        var result = SettingsRegistrar.Register(config);

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
        var result1 = SettingsRegistrar.Register(config);
        var result2 = SettingsRegistrar.Register(config);

        // Assert
        Assert.AreNotSame(result1, result2);
    }

    /// <summary>
    /// Tests that Register correctly handles a config with no properties at all.
    /// </summary>
    [TestMethod]
    public void Register_ConfigWithNoProperties_ReturnsEmptyDictionary()
    {
        // Arrange
        var config = new ConfigWithNoProperties();

        // Act
        var result = SettingsRegistrar.Register(config);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
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
        var result = SettingsRegistrar.Register(config);

        // Assert
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("publicParameter"));
    }

    // Test helper classes
    [UmbraAutoRegisterSettings]
    internal class SimpleConfig
    {
        [UmbraSettingsParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    internal class ConfigWithoutAttribute
    {
        [UmbraSettingsParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegisterSettings]
    internal class EmptyConfig
    {
    }

    [UmbraAutoRegisterSettings]
    [UmbraSettingsPrefix("myPrefix")]
    internal class ConfigWithPrefix
    {
        [UmbraSettingsParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);

        [UmbraSettingsParameter]
        public Parameter<int> Value { get; set; } = new(42);
    }

    [UmbraAutoRegisterSettings]
    [UmbraCategory("TestCategory")]
    internal class ConfigWithCategory
    {
        [UmbraSettingsParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegisterSettings]
    [UmbraSettingsPrefix("pre")]
    [UmbraCategory("Cat")]
    internal class ConfigWithPrefixAndCategory
    {
        [UmbraSettingsParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegisterSettings]
    internal class ConfigWithKeyOverride
    {
        [UmbraSettingsParameter("customKey")]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegisterSettings]
    internal class ConfigWithMultipleParameters
    {
        [UmbraSettingsParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);

        [UmbraSettingsParameter]
        public Parameter<int> Count { get; set; } = new(10);

        [UmbraSettingsParameter]
        public Parameter<string> Name { get; set; } = new("test");
    }

    [UmbraAutoRegisterSettings]
    internal class ConfigWithNestedSettings
    {
        [UmbraSettingsParameter]
        public Parameter<bool> TopLevel { get; set; } = new(true);

        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("nested")]
        public NestedGroup Nested { get; set; } = new();
    }

    [UmbraAutoRegisterSettings]
    internal class NestedGroup
    {
        [UmbraSettingsParameter]
        public Parameter<int> NestedValue { get; set; } = new(100);
    }

    [UmbraAutoRegisterSettings]
    internal class ConfigWithNestedPropertyPrefix
    {
        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("customPrefix")]
        public NestedGroup Nested { get; set; } = new();
    }

    [UmbraAutoRegisterSettings]
    [UmbraSettingsPrefix("root")]
    internal class ConfigWithDeeplyNestedSettings
    {
        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("level1")]
        public Level1Group Level1 { get; set; } = new();
    }

    [UmbraAutoRegisterSettings]
    internal class Level1Group
    {
        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("level2")]
        public Level2Group Level2 { get; set; } = new();
    }

    [UmbraAutoRegisterSettings]
    internal class Level2Group
    {
        [UmbraSettingsParameter]
        public Parameter<string> DeepValue { get; set; } = new("deep");
    }

    [UmbraAutoRegisterSettings]
    internal class ConfigWithDuplicateKeys
    {
        [UmbraSettingsParameter("enabled")]
        public Parameter<bool> Enabled1 { get; set; } = new(true);

        [UmbraSettingsParameter("enabled")]
        public Parameter<bool> Enabled2 { get; set; } = new(false);
    }

    [UmbraAutoRegisterSettings]
    internal class ConfigWithCircularReference
    {
        [UmbraSettingsParameter]
        public Parameter<int> Value1 { get; set; } = new(1);

        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("other")]
        public ConfigWithCircularReference2 Other { get; set; } = new();
    }

    [UmbraAutoRegisterSettings]
    internal class ConfigWithCircularReference2
    {
        [UmbraSettingsParameter]
        public Parameter<int> Value2 { get; set; } = new(2);

        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("circular")]
        public ConfigWithCircularReference? Other { get; set; }
    }

    [UmbraAutoRegisterSettings]
    internal class ConfigWithNullProperty
    {
        [UmbraSettingsParameter]
        public Parameter<bool> TopLevel { get; set; } = new(true);

        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("nested")]
        public NestedGroup? Nested { get; set; } = new();
    }

    [UmbraAutoRegisterSettings]
    [UmbraSettingsPrefix("")]
    [UmbraCategory("")]
    internal class ConfigWithEmptyPrefix
    {
        [UmbraSettingsParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    [UmbraAutoRegisterSettings]
    internal class ConfigWithNoProperties
    {
    }

    [UmbraAutoRegisterSettings]
    internal class ConfigWithPrivateProperty
    {
        [UmbraSettingsParameter]
        public Parameter<bool> PublicParameter { get; set; } = new(true);

        [UmbraSettingsParameter]
        private Parameter<bool> PrivateParameter { get; set; } = new(false);
    }
}
