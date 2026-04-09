using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;
using Umbra.UI.Config.Search;
using Umbra.UI.Config.Transfer;

namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="ConfigSection{TConfig}"/> class.
/// </summary>
[TestClass]
public sealed class ConfigSectionTests
{
    [TestInitialize]
    public void TestInit() => UndoShortcutCoordinator.Reset();

    /// <summary>
    /// Test configuration class used for testing <see cref="ConfigSection{TConfig}"/>.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class TestConfig
    {
        [UmbraParameter]
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
    /// Tests that <see cref="ConfigSection{TConfig}.SectionLabel"/> returns null when suppressTreeNode is true,
    /// even when an explicit tree node label is provided.
    /// </summary>
    [TestMethod]
    public void SectionLabel_SuppressTreeNodeTrue_ReturnsNull()
    {
        // Arrange
        var config = new SimpleConfig();
        var section = new ConfigSection<SimpleConfig>(config, sectionLabel: "Test Label", suppressTreeNode: true);

        // Act
        var result = section.SectionLabel;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionLabel"/> returns the explicit label
    /// when provided via constructor parameter.
    /// </summary>
    [TestMethod]
    public void SectionLabel_ExplicitLabelProvided_ReturnsExplicitLabel()
    {
        // Arrange
        var config = new SimpleConfig();
        const string expectedLabel = "My Custom Label";
        var section = new ConfigSection<SimpleConfig>(config, sectionLabel: expectedLabel);

        // Act
        var result = section.SectionLabel;

        // Assert
        Assert.AreEqual(expectedLabel, result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionLabel"/> returns empty string
    /// when an empty string is explicitly provided via constructor parameter.
    /// </summary>
    [TestMethod]
    public void SectionLabel_ExplicitEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var config = new SimpleConfig();
        var section = new ConfigSection<SimpleConfig>(config, sectionLabel: string.Empty);

        // Act
        var result = section.SectionLabel;

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionLabel"/> returns the attribute's label
    /// when no explicit label is provided and the config type has UmbraConfigRootNodeAttribute with a non-null label.
    /// </summary>
    [TestMethod]
    public void SectionLabel_AttributeWithLabel_ReturnsAttributeLabel()
    {
        // Arrange
        var config = new ConfigWithRootNodeAttribute();
        var section = new ConfigSection<ConfigWithRootNodeAttribute>(config);

        // Act
        var result = section.SectionLabel;

        // Assert
        Assert.AreEqual("Attribute Label", result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionLabel"/> returns the display name of the type
    /// when the UmbraConfigRootNodeAttribute exists but its Label property is null.
    /// </summary>
    [TestMethod]
    public void SectionLabel_AttributeWithNullLabel_ReturnsDisplayName()
    {
        // Arrange
        var config = new ConfigWithNullLabelAttribute();
        var section = new ConfigSection<ConfigWithNullLabelAttribute>(config);

        // Act
        var result = section.SectionLabel;

        // Assert
        Assert.IsNotNull(result);
        // The result should be the display name derived from "ConfigWithNullLabelAttribute"
        Assert.AreEqual("Config With Null Label Attribute", result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionLabel"/> returns null
    /// when no explicit label is provided and the config type has no UmbraConfigRootNodeAttribute.
    /// </summary>
    [TestMethod]
    public void SectionLabel_NoAttributeNoLabel_ReturnsNull()
    {
        // Arrange
        var config = new SimpleConfig();
        var section = new ConfigSection<SimpleConfig>(config);

        // Act
        var result = section.SectionLabel;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionLabel"/> returns the explicit label
    /// even when the config type has an attribute, demonstrating that explicit label takes precedence.
    /// </summary>
    [TestMethod]
    public void SectionLabel_ExplicitLabelOverridesAttribute_ReturnsExplicitLabel()
    {
        // Arrange
        var config = new ConfigWithRootNodeAttribute();
        const string explicitLabel = "Override Label";
        var section = new ConfigSection<ConfigWithRootNodeAttribute>(config, sectionLabel: explicitLabel);

        // Act
        var result = section.SectionLabel;

        // Assert
        Assert.AreEqual(explicitLabel, result);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionLabel"/> returns null
    /// when suppressTreeNode is true and the config has a root node attribute.
    /// </summary>
    [TestMethod]
    public void SectionLabel_SuppressTreeNodeTrueWithAttribute_ReturnsNull()
    {
        // Arrange
        var config = new ConfigWithRootNodeAttribute();
        var section = new ConfigSection<ConfigWithRootNodeAttribute>(config, suppressTreeNode: true);

        // Act
        var result = section.SectionLabel;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that the constructor rejects a null config instance.
    /// </summary>
    [TestMethod]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ConfigSection<TestConfig>(null!));

        Assert.AreEqual("config", exception.ParamName);
    }

    /// <summary>
    /// Tests that the constructor rejects whitespace-only id scopes.
    /// </summary>
    [TestMethod]
    public void Constructor_WhitespaceIdScope_ThrowsArgumentException()
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(() => _ = new ConfigSection<TestConfig>(new TestConfig(), idScope: "   "));

        Assert.AreEqual("idScope", exception.ParamName);
    }

    /// <summary>
    /// Tests that the options-aware constructor succeeds when search-bar support is enabled.
    /// </summary>
    [TestMethod]
    public void Constructor_WithOptions_ConstructsSuccessfully()
    {
        // Arrange
        var config = new TestConfig();
        var options = new ConfigDrawerOptions { Search = new ConfigSearchOptions() };

        // Act
        using var section = new ConfigSection<TestConfig>(config, options, idScope: "search-enabled");

        // Assert
        Assert.AreEqual("search-enabled", section.SectionId);
    }

    /// <summary>
    /// Tests that the additive store-aware factory succeeds without changing existing section behavior.
    /// </summary>
    [TestMethod]
    public void CreateWithStore_WithOptions_ConstructsSuccessfully()
    {
        var config = new TestConfig();
        using var tempDirectory = new TempDirectory();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));
        var options = new ConfigDrawerOptions
        {
            Search = new ConfigSearchOptions(),
            Transfer = new ConfigTransferOptions { Enabled = true }
        };

        using var section = ConfigSection<TestConfig>.CreateWithStore(config, store, options, idScope: "transfer-enabled");

        Assert.AreEqual("transfer-enabled", section.SectionId);
    }

    /// <summary>
    /// Tests that the additive store-aware factory rejects a null store.
    /// </summary>
    [TestMethod]
    public void CreateWithStore_NullStore_ThrowsArgumentNullException()
    {
        var config = new TestConfig();
        var options = new ConfigDrawerOptions { Transfer = new ConfigTransferOptions { Enabled = true } };

        var exception = Assert.ThrowsExactly<ArgumentNullException>(
            () => _ = ConfigSection<TestConfig>.CreateWithStore(config, null!, options));

        Assert.AreEqual("store", exception.ParamName);
    }

    /// <summary>
    /// Tests that the store-aware factory leaves the built-in transfer feature disabled when transfer is not enabled.
    /// </summary>
    [TestMethod]
    public void CreateWithStore_WhenTransferDisabled_DoesNotCreateTransferFeature()
    {
        using var tempDirectory = new TempDirectory();
        var config = new TestConfig();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));

        using var section = ConfigSection<TestConfig>.CreateWithStore(config, store, new ConfigDrawerOptions());

        Assert.IsNull(GetTransferFeature(section));
    }

    /// <summary>
    /// Tests that the store-aware factory creates the built-in transfer feature when transfer is enabled.
    /// </summary>
    [TestMethod]
    public void CreateWithStore_WhenTransferEnabled_CreatesTransferFeature()
    {
        using var tempDirectory = new TempDirectory();
        var config = new TestConfig();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));

        using var section = ConfigSection<TestConfig>.CreateWithStore(
            config,
            store,
            new ConfigDrawerOptions { Transfer = new ConfigTransferOptions { Enabled = true } });

        Assert.IsNotNull(GetTransferFeature(section));
    }

    [TestMethod]
    public void CreateWithStore_WhenTransferEnabled_UsesDefaultTransferTreeNodeOptions()
    {
        using var tempDirectory = new TempDirectory();
        var config = new TestConfig();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));

        using var section = ConfigSection<TestConfig>.CreateWithStore(
            config,
            store,
            new ConfigDrawerOptions { Transfer = new ConfigTransferOptions { Enabled = true } });

        var feature = GetTransferFeature(section);
        Assert.IsNotNull(feature);
        Assert.AreEqual(ConfigTransferOptions.DefaultSectionLabel, feature.SectionLabel);
        Assert.IsFalse(feature.ExpandedByDefault);
        Assert.AreEqual(ConfigTransferPlacement.AfterConfig, feature.Placement);
        Assert.IsTrue(feature.ShowSeparatorBelowButtons);
    }

    [TestMethod]
    public void CreateWithStore_WhenTransferOptionsOverridePresentation_UsesConfiguredTreeNodeOptions()
    {
        using var tempDirectory = new TempDirectory();
        var config = new TestConfig();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));

        using var section = ConfigSection<TestConfig>.CreateWithStore(
            config,
            store,
            new ConfigDrawerOptions
            {
                Transfer = new ConfigTransferOptions
                {
                    Enabled = true,
                    SectionLabel = "Transfer Controls",
                    ExpandedByDefault = true,
                    Placement = ConfigTransferPlacement.BeforeConfig
                }
            });

        var feature = GetTransferFeature(section);
        Assert.IsNotNull(feature);
        Assert.AreEqual("Transfer Controls", feature.SectionLabel);
        Assert.IsTrue(feature.ExpandedByDefault);
        Assert.AreEqual(ConfigTransferPlacement.BeforeConfig, feature.Placement);
        Assert.IsTrue(feature.ShowSeparatorBelowButtons);
    }

    [TestMethod]
    public void CreateWithStore_WhenTransferSeparatorDisabled_UsesConfiguredSeparatorOption()
    {
        using var tempDirectory = new TempDirectory();
        var config = new TestConfig();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));

        using var section = ConfigSection<TestConfig>.CreateWithStore(
            config,
            store,
            new ConfigDrawerOptions
            {
                Transfer = new ConfigTransferOptions
                {
                    Enabled = true,
                    ShowSeparatorBelowButtons = false
                }
            });

        var feature = GetTransferFeature(section);
        Assert.IsNotNull(feature);
        Assert.IsFalse(feature.ShowSeparatorBelowButtons);
    }

    // --- Undo stack wiring ---

    /// <summary>
    /// Tests that the factory creates an undo stack when <see cref="ConfigDrawerOptions.Undo"/> is non-null
    /// and the store is a <see cref="ConfigStore{TConfig}"/>.
    /// </summary>
    [TestMethod]
    public void CreateWithStore_WithUndoOptionsAndConfigStore_CreatesUndoStack()
    {
        using var tempDirectory = new TempDirectory();
        var storePath = Path.Combine(tempDirectory.Path, "undo-config.json");
        var store = new ConfigStore<TestConfig>(storePath);
        var config = store.Load();
        var options = new ConfigDrawerOptions { Undo = new ConfigUndoOptions() };

        using var section = ConfigSection<TestConfig>.CreateWithStore(config, store, options, idScope: "undo-test");

        Assert.IsNotNull(GetUndoStack(section));
        store.Dispose();
    }

    /// <summary>
    /// Tests that the factory does not create an undo stack when <see cref="ConfigDrawerOptions.Undo"/> is null.
    /// </summary>
    [TestMethod]
    public void CreateWithStore_WithoutUndoOptions_UndoStackIsNull()
    {
        using var tempDirectory = new TempDirectory();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));

        using var section = ConfigSection<TestConfig>.CreateWithStore(
            new TestConfig(), store, new ConfigDrawerOptions());

        Assert.IsNull(GetUndoStack(section));
    }

    /// <summary>
    /// Tests that the factory does not create an undo stack when the store is not a
    /// <see cref="ConfigStore{TConfig}"/>, even when undo options are provided.
    /// </summary>
    [TestMethod]
    public void CreateWithStore_WithUndoOptionsButNonConfigStore_UndoStackIsNull()
    {
        using var tempDirectory = new TempDirectory();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));
        var options = new ConfigDrawerOptions { Undo = new ConfigUndoOptions() };

        using var section = ConfigSection<TestConfig>.CreateWithStore(
            new TestConfig(), store, options, idScope: "no-undo");

        Assert.IsNull(GetUndoStack(section));
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.UndoStack"/> exposes the undo stack created by the factory.
    /// </summary>
    [TestMethod]
    public void UndoStack_Property_ExposesCreatedUndoStack()
    {
        using var tempDirectory = new TempDirectory();
        var storePath = Path.Combine(tempDirectory.Path, "undo-config.json");
        var store = new ConfigStore<TestConfig>(storePath);
        var config = store.Load();
        var options = new ConfigDrawerOptions { Undo = new ConfigUndoOptions() };

        using var section = ConfigSection<TestConfig>.CreateWithStore(config, store, options, idScope: "prop-test");

        Assert.IsNotNull(section.UndoStack);
        Assert.AreSame(GetUndoStack(section), section.UndoStack);
        store.Dispose();
    }

    /// <summary>
    /// Tests that disposing the section also nulls the undo stack field.
    /// </summary>
    [TestMethod]
    public void Dispose_WithUndoStack_NullsUndoStack()
    {
        using var tempDirectory = new TempDirectory();
        var storePath = Path.Combine(tempDirectory.Path, "undo-config.json");
        var store = new ConfigStore<TestConfig>(storePath);
        var config = store.Load();
        var options = new ConfigDrawerOptions { Undo = new ConfigUndoOptions() };

        var section = ConfigSection<TestConfig>.CreateWithStore(config, store, options, idScope: "dispose-test");
        Assert.IsNotNull(section.UndoStack);

        section.Dispose();

        Assert.IsNull(section.UndoStack);
        store.Dispose();
    }

    /// <summary>
    /// Tests that the built-in undo shortcut restores the latest value change when undo is available.
    /// </summary>
    [TestMethod]
    public void TryHandleBuiltInUndo_WhenShortcutPressedAndUndoAvailable_RestoresPreviousValue()
    {
        using var tempDirectory = new TempDirectory();
        var storePath = Path.Combine(tempDirectory.Path, "undo-shortcut.json");
        var store = new ConfigStore<TestConfig>(storePath);
        var config = store.Load();
        var inputSource = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };

        using var section = ConfigSection<TestConfig>.CreateWithStore(
            config,
            store,
            new ConfigDrawerOptions { Undo = new ConfigUndoOptions(), UndoInputSource = inputSource },
            idScope: "undo-shortcut",
            sectionLabel: null,
            expandedByDefault: false,
            suppressTreeNode: false);

        config.TestParameter.Value = false;

        section.TryHandleBuiltInUndo();

        Assert.IsTrue(config.TestParameter.Value);
        Assert.AreEqual(1, inputSource.WantsTextInputCheckCount);
        Assert.AreEqual(1, inputSource.DefaultUndoShortcutCheckCount);
        store.Dispose();
    }

    /// <summary>
    /// Tests that built-in undo does nothing when the section does not own an undo stack.
    /// </summary>
    [TestMethod]
    public void TryHandleBuiltInUndo_WhenUndoStackIsMissing_DoesNotQueryInputSource()
    {
        var inputSource = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };
        using var section = new ConfigSection<TestConfig>(
            new TestConfig(),
            new ConfigDrawerOptions { UndoInputSource = inputSource },
            idScope: "missing-undo",
            sectionLabel: null,
            expandedByDefault: false,
            suppressTreeNode: false);

        section.TryHandleBuiltInUndo();

        Assert.AreEqual(0, inputSource.WantsTextInputCheckCount);
        Assert.AreEqual(0, inputSource.DefaultUndoShortcutCheckCount);
    }

    /// <summary>
    /// Tests that built-in undo does nothing when the owned undo stack is empty.
    /// </summary>
    [TestMethod]
    public void TryHandleBuiltInUndo_WhenUndoStackIsEmpty_DoesNotQueryInputSource()
    {
        using var tempDirectory = new TempDirectory();
        var storePath = Path.Combine(tempDirectory.Path, "undo-empty.json");
        var store = new ConfigStore<TestConfig>(storePath);
        var config = store.Load();
        var inputSource = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };

        using var section = ConfigSection<TestConfig>.CreateWithStore(
            config,
            store,
            new ConfigDrawerOptions { Undo = new ConfigUndoOptions(), UndoInputSource = inputSource },
            idScope: "empty-undo",
            sectionLabel: null,
            expandedByDefault: false,
            suppressTreeNode: false);

        section.TryHandleBuiltInUndo();

        Assert.IsTrue(config.TestParameter.Value);
        Assert.AreEqual(1, inputSource.WantsTextInputCheckCount);
        Assert.AreEqual(1, inputSource.DefaultUndoShortcutCheckCount);
        store.Dispose();
    }

    /// <summary>
    /// Tests that built-in undo is suppressed while text input is actively handling editing shortcuts.
    /// </summary>
    [TestMethod]
    public void TryHandleBuiltInUndo_WhenTextInputIsActive_DoesNotUndo()
    {
        using var tempDirectory = new TempDirectory();
        var storePath = Path.Combine(tempDirectory.Path, "undo-text-input.json");
        var store = new ConfigStore<TestConfig>(storePath);
        var config = store.Load();
        var inputSource = new TestUndoShortcutInputSource
        {
            DefaultUndoShortcutPressed = true,
            WantsTextInputState = true
        };

        using var section = ConfigSection<TestConfig>.CreateWithStore(
            config,
            store,
            new ConfigDrawerOptions { Undo = new ConfigUndoOptions(), UndoInputSource = inputSource },
            idScope: "text-input-undo",
            sectionLabel: null,
            expandedByDefault: false,
            suppressTreeNode: false);

        config.TestParameter.Value = false;

        section.TryHandleBuiltInUndo();

        Assert.IsFalse(config.TestParameter.Value);
        Assert.AreEqual(1, inputSource.WantsTextInputCheckCount);
        Assert.AreEqual(0, inputSource.DefaultUndoShortcutCheckCount);
        store.Dispose();
    }

    /// <summary>
    /// Tests that the built-in redo shortcut re-applies the previously undone value change.
    /// </summary>
    [TestMethod]
    public void TryHandleBuiltInUndo_RedoShortcutAfterUndo_RestoresValue()
    {
        using var tempDirectory = new TempDirectory();
        var storePath = Path.Combine(tempDirectory.Path, "redo-shortcut.json");
        var store = new ConfigStore<TestConfig>(storePath);
        var config = store.Load();
        var inputSource = new TestUndoShortcutInputSource();

        using var section = ConfigSection<TestConfig>.CreateWithStore(
            config,
            store,
            new ConfigDrawerOptions { Undo = new ConfigUndoOptions(), UndoInputSource = inputSource },
            idScope: "redo-shortcut",
            sectionLabel: null,
            expandedByDefault: false,
            suppressTreeNode: false);

        config.TestParameter.Value = false;

        // Undo via shortcut
        inputSource.DefaultUndoShortcutPressed = true;
        section.TryHandleBuiltInUndo();
        Assert.IsTrue(config.TestParameter.Value);

        // Redo via shortcut — need a new tick so coordinator doesn't deduplicate
        UndoShortcutCoordinator.Reset();
        UndoShortcutCoordinator.Register((IUndoStackHandle)section.UndoStack!);
        inputSource.DefaultUndoShortcutPressed = false;
        inputSource.DefaultRedoShortcutPressed = true;
        section.TryHandleBuiltInUndo();
        Assert.IsFalse(config.TestParameter.Value);

        store.Dispose();
    }

    /// <summary>
    /// Tests that the section preserves caller-enabled search while suppressing the wrapped drawer root node.
    /// </summary>
    [TestMethod]
    public void Constructor_WithOptions_PreservesSearchBarAndSuppressesWrappedDrawerRootNode()
    {
        // Arrange
        var config = new RootWrappedSectionConfig();
        var options = new ConfigDrawerOptions { Search = new ConfigSearchOptions() };

        // Act
        using var section = new ConfigSection<RootWrappedSectionConfig>(config, options, idScope: "search-enabled");
        var drawer = GetDrawer(section);
        var nodes = GetTopLevelNodes(drawer);
        var searchState = GetSearchState(drawer);

        // Assert
        Assert.IsNotNull(searchState);
        Assert.IsFalse(nodes.Exists(static node => node is RootTreeNode));
    }

    /// <summary>
    /// Tests that the options-aware constructor rejects a null options instance.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new TestConfig();

        // Act
        var exception = Assert.ThrowsExactly<ArgumentNullException>(
            () => _ = new ConfigSection<TestConfig>(config, options: null!));

        // Assert
        Assert.AreEqual("options", exception.ParamName);
    }

    /// <summary>
    /// Tests that an explicit tree-node label also controls the default-open flag.
    /// </summary>
    [TestMethod]
    public void ExpandedByDefault_ExplicitLabelProvided_UsesExplicitFlag()
    {
        var config = new ConfigWithRootNodeAttribute();
        var section = new ConfigSection<ConfigWithRootNodeAttribute>(config, sectionLabel: "Explicit", expandedByDefault: false);

        Assert.IsFalse(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.SectionLabel"/> returns the same value after disposal
    /// when the value was previously set.
    /// </summary>
    [TestMethod]
    public void SectionLabel_AfterDisposal_StillReturnsSameValue()
    {
        // Arrange
        var config = new SimpleConfig();
        const string expectedLabel = "Label Before Disposal";
        var section = new ConfigSection<SimpleConfig>(config, sectionLabel: expectedLabel);

        // Act
        section.Dispose();
        var result = section.SectionLabel;

        // Assert
        Assert.AreEqual(expectedLabel, result);
    }

    #region Test Config Classes

    /// <summary>
    /// Simple test configuration class with no attributes.
    /// </summary>
    [UmbraAutoRegister]
    internal sealed class SimpleConfig
    {
    }

    /// <summary>
    /// Test configuration class with UmbraConfigRootNodeAttribute specifying a label.
    /// </summary>
    [UmbraAutoRegister]
    [UmbraRootNode("Attribute Label", true)]
    internal sealed class ConfigWithRootNodeAttribute
    {
    }

    /// <summary>
    /// Test configuration class that declares a root node and exposes a parameter for section option tests.
    /// </summary>
    [UmbraAutoRegister]
    [UmbraRootNode("Section Root", true)]
    internal sealed class RootWrappedSectionConfig
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    /// <summary>
    /// Minimal transfer-capable store stub used by the store-aware section factory tests.
    /// </summary>
    private sealed class TestConfigTransferStore(string filePath) : IConfigTransferStore
    {
        public string FilePath => filePath;

        public bool IsLoaded => true;

        public bool IsDisposed => false;

        public void Export(string filePath)
        {
        }

        public ConfigImportReport Import(string filePath, ConfigImportOptions? options = null)
            => new();
    }

    private static ConfigTransferFeature? GetTransferFeature<T>(ConfigSection<T> section)
        where T : class, new()
        => (ConfigTransferFeature?)typeof(ConfigSection<T>)
            .GetField("_transferFeature", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(section);

    private static ConfigUndoStack<T>? GetUndoStack<T>(ConfigSection<T> section)
        where T : class, new()
        => (ConfigUndoStack<T>?)typeof(ConfigSection<T>)
            .GetField("_undoStack", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(section);

    private sealed class TempDirectory : IDisposable
    {
        internal TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);

            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Test configuration class with UmbraConfigRootNodeAttribute with null label.
    /// </summary>
    [UmbraAutoRegister]
    [UmbraRootNode(null, false)]
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
    /// Tests that a feature section should be hidden while search is active.
    /// </summary>
    [TestMethod]
    public void ShouldDrawFeatureSection_HasFeatureAndActiveSearch_ReturnsFalse()
    {
        // Act
        var shouldDraw = ConfigSection<TestConfig>.ShouldDrawFeatureSection(hasFeature: true, hasActiveSearchQuery: true);

        // Assert
        Assert.IsFalse(shouldDraw);
    }

    /// <summary>
    /// Tests that a feature section should be shown when the feature exists and search is inactive.
    /// </summary>
    [TestMethod]
    public void ShouldDrawFeatureSection_HasFeatureAndNoActiveSearch_ReturnsTrue()
    {
        // Act
        var shouldDraw = ConfigSection<TestConfig>.ShouldDrawFeatureSection(hasFeature: true, hasActiveSearchQuery: false);

        // Assert
        Assert.IsTrue(shouldDraw);
    }

    /// <summary>
    /// Tests that a feature section should remain hidden when no feature exists.
    /// </summary>
    [TestMethod]
    public void ShouldDrawFeatureSection_NoFeature_ReturnsFalse()
    {
        // Act
        var shouldDraw = ConfigSection<TestConfig>.ShouldDrawFeatureSection(hasFeature: false, hasActiveSearchQuery: false);

        // Assert
        Assert.IsFalse(shouldDraw);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.ExpandedByDefault"/> returns false when
    /// <paramref name="suppressTreeNode"/> is true, regardless of other parameters.
    /// </summary>
    [TestMethod]
    public void ExpandedByDefault_SuppressTreeNodeTrue_ReturnsFalse()
    {
        // Arrange
        var config = new TestConfig();

        // Act
        var section = new ConfigSection<TestConfig>(
            config,
            sectionLabel: "Test Label",
            expandedByDefault: true,
            suppressTreeNode: true);

        // Assert
        Assert.IsFalse(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.ExpandedByDefault"/> returns the value of
    /// the <paramref name="expandedByDefault"/> parameter when an explicit tree node label is provided.
    /// </summary>
    /// <param name="expandedByDefault">The explicit default open state to test.</param>
    /// <param name="expected">The expected return value.</param>
    [TestMethod]
    [DataRow(true, true, DisplayName = "ExpandedByDefault_ExplicitLabelWithDefaultOpenTrue_ReturnsTrue")]
    [DataRow(false, false, DisplayName = "ExpandedByDefault_ExplicitLabelWithDefaultOpenFalse_ReturnsFalse")]
    public void ExpandedByDefault_ExplicitSectionLabel_ReturnsParameterValue(bool expandedByDefault, bool expected)
    {
        // Arrange
        var config = new TestConfig();

        // Act
        var section = new ConfigSection<TestConfig>(
            config,
            sectionLabel: "Test Label",
            expandedByDefault: expandedByDefault);

        // Assert
        Assert.AreEqual(expected, section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.ExpandedByDefault"/> returns false when
    /// no explicit tree node label is provided and the config type has no
    /// <see cref="UmbraRootNodeAttribute"/>.
    /// </summary>
    [TestMethod]
    public void ExpandedByDefault_NoAttributeNoExplicitLabel_ReturnsFalse()
    {
        // Arrange
        var config = new TestConfig();

        // Act
        var section = new ConfigSection<TestConfig>(config);

        // Assert
        Assert.IsFalse(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that an explicit <paramref name="sectionLabel"/> parameter overrides the
    /// <see cref="UmbraRootNodeAttribute"/> when both are present. The explicit
    /// <paramref name="expandedByDefault"/> parameter value should be returned.
    /// </summary>
    [TestMethod]
    public void ExpandedByDefault_ExplicitLabelOverridesAttribute_ReturnsExplicitValue()
    {
        // Arrange
        var config = new TestConfigWithAttributeDefaultOpenFalse();

        // Act
        var section = new ConfigSection<TestConfigWithAttributeDefaultOpenFalse>(
            config,
            sectionLabel: "Override Label",
            expandedByDefault: true);

        // Assert
        Assert.IsTrue(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that <see cref="ConfigSection{TConfig}.ExpandedByDefault"/> returns false when
    /// <paramref name="suppressTreeNode"/> is true, even if the config type has an attribute
    /// with DefaultOpen = true.
    /// </summary>
    [TestMethod]
    public void ExpandedByDefault_SuppressTreeNodeWithAttribute_ReturnsFalse()
    {
        // Arrange
        var config = new TestConfigWithAttributeDefaultOpenTrue();

        // Act
        var section = new ConfigSection<TestConfigWithAttributeDefaultOpenTrue>(
            config,
            suppressTreeNode: true);

        // Assert
        Assert.IsFalse(section.ExpandedByDefault);
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
    /// Tests that the Order property returns int.MaxValue when the config type
    /// has a SectionOrderAttribute with order = int.MaxValue.
    /// </summary>
    [TestMethod]
    public void Order_ConfigTypeWithSectionOrderAttributeIntMaxValue_ReturnsIntMaxValue()
    {
        // Arrange
        var config = new ConfigWithOrderMaxValue();

        // Act
        var section = new ConfigSection<ConfigWithOrderMaxValue>(config);

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
        var firstRead = section.Order;
        var secondRead = section.Order;
        var thirdRead = section.Order;

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
    [UmbraSectionOrder(0)]
    internal sealed class ConfigWithOrderZero
    {
    }

    /// <summary>
    /// Test config class with SectionOrderAttribute having a positive order value.
    /// </summary>
    [UmbraSectionOrder(100)]
    internal sealed class ConfigWithOrderPositive
    {
    }

    /// <summary>
    /// Test config class with SectionOrderAttribute having order = int.MaxValue.
    /// </summary>
    [UmbraSectionOrder(int.MaxValue)]
    internal sealed class ConfigWithOrderMaxValue
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
    [DataRow("MyPlugin.Config")]
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
    public void ConfigSection_WithOrderAttribute_UsesAttributeOrderValue(int orderValue)
    {
        // Arrange
        var config = orderValue switch
        {
            0 => new ConfigWithOrder0() as object,
            1 => new ConfigWithOrder1(),
            10 => new ConfigWithOrder10(),
            100 => new ConfigWithOrder100(),
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
    /// Tests that when sectionLabel is provided and suppressTreeNode is false, SectionLabel uses the provided value.
    /// </summary>
    [TestMethod]
    [DataRow("My Config")]
    [DataRow("Configuration")]
    [DataRow("")]
    [DataRow(" ")]
    public void ConfigSection_SectionLabelProvided_UsesProvidedLabel(string label)
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, sectionLabel: label);

        // Assert
        Assert.AreEqual(label, section.SectionLabel);
    }

    /// <summary>
    /// Tests that when sectionLabel is provided and expandedByDefault is true, ExpandedByDefault is true.
    /// </summary>
    [TestMethod]
    public void ConfigSection_SectionLabelProvidedWithDefaultOpenTrue_ExpandedByDefaultIsTrue()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, sectionLabel: "Label", expandedByDefault: true);

        // Assert
        Assert.IsTrue(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that when sectionLabel is provided and expandedByDefault is false, ExpandedByDefault is false.
    /// </summary>
    [TestMethod]
    public void ConfigSection_SectionLabelProvidedWithDefaultOpenFalse_ExpandedByDefaultIsFalse()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, sectionLabel: "Label", expandedByDefault: false);

        // Assert
        Assert.IsFalse(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that when sectionLabel is null and the type has no UmbraConfigRootNodeAttribute, SectionLabel is null.
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoSectionLabelAndNoAttribute_SectionLabelIsNull()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, sectionLabel: null);

        // Assert
        Assert.IsNull(section.SectionLabel);
    }

    /// <summary>
    /// Tests that when sectionLabel is null and the type has UmbraConfigRootNodeAttribute with a Label, SectionLabel uses the attribute Label.
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoSectionLabelWithAttributeLabel_UsesAttributeLabel()
    {
        // Arrange
        var config = new ConfigWithRootNodeLabel();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeLabel>(config, sectionLabel: null);

        // Assert
        Assert.AreEqual("Root Config", section.SectionLabel);
    }

    /// <summary>
    /// Tests that when sectionLabel is null and the type has UmbraConfigRootNodeAttribute with Label = null, SectionLabel uses ToDisplayName().
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoSectionLabelWithAttributeLabelNull_UsesToDisplayName()
    {
        // Arrange
        var config = new ConfigWithRootNodeNoLabel();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeNoLabel>(config, sectionLabel: null);

        // Assert
        Assert.AreEqual("Config With Root Node No Label", section.SectionLabel);
    }

    /// <summary>
    /// Tests that when sectionLabel is null and the type has UmbraConfigRootNodeAttribute with DefaultOpen = true, ExpandedByDefault is true.
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoSectionLabelWithAttributeDefaultOpenTrue_ExpandedByDefaultIsTrue()
    {
        // Arrange
        var config = new ConfigWithRootNodeDefaultOpenTrue();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeDefaultOpenTrue>(config, sectionLabel: null);

        // Assert
        Assert.IsTrue(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that when sectionLabel is null and the type has UmbraConfigRootNodeAttribute with DefaultOpen = false, ExpandedByDefault is false.
    /// </summary>
    [TestMethod]
    public void ConfigSection_NoSectionLabelWithAttributeDefaultOpenFalse_ExpandedByDefaultIsFalse()
    {
        // Arrange
        var config = new ConfigWithRootNodeDefaultOpenFalse();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeDefaultOpenFalse>(config, sectionLabel: null);

        // Assert
        Assert.IsFalse(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that when suppressTreeNode is true and sectionLabel is provided, SectionLabel is null.
    /// </summary>
    [TestMethod]
    public void ConfigSection_SuppressTreeNodeTrueWithProvidedLabel_SectionLabelIsNull()
    {
        // Arrange
        var config = new BasicConfig();

        // Act
        var section = new ConfigSection<BasicConfig>(config, sectionLabel: "Label", suppressTreeNode: true);

        // Assert
        Assert.IsNull(section.SectionLabel);
    }

    /// <summary>
    /// Tests that when suppressTreeNode is true, ExpandedByDefault is false regardless of attribute.
    /// </summary>
    [TestMethod]
    public void ConfigSection_SuppressTreeNodeTrue_ExpandedByDefaultIsFalse()
    {
        // Arrange
        var config = new ConfigWithRootNodeDefaultOpenTrue();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeDefaultOpenTrue>(config, suppressTreeNode: true);

        // Assert
        Assert.IsFalse(section.ExpandedByDefault);
    }

    /// <summary>
    /// Tests that when sectionLabel is explicitly provided with expandedByDefault = true, it overrides the attribute DefaultOpen.
    /// </summary>
    [TestMethod]
    public void ConfigSection_ProvidedExpandedByDefaultOverridesAttribute_UsesProvidedValue()
    {
        // Arrange
        var config = new ConfigWithRootNodeDefaultOpenFalse();

        // Act
        var section = new ConfigSection<ConfigWithRootNodeDefaultOpenFalse>(config, sectionLabel: "Label", expandedByDefault: true);

        // Assert
        Assert.IsTrue(section.ExpandedByDefault);
    }

    // Test config classes

    internal sealed class BasicConfig
    {
    }

    [UmbraSectionOrder(0)]
    internal sealed class ConfigWithOrder0
    {
    }

    [UmbraSectionOrder(1)]
    internal sealed class ConfigWithOrder1
    {
    }

    [UmbraSectionOrder(10)]
    internal sealed class ConfigWithOrder10
    {
    }

    [UmbraSectionOrder(100)]
    internal sealed class ConfigWithOrder100
    {
    }

    [UmbraRootNode("Root Config", false)]
    internal sealed class ConfigWithRootNodeLabel
    {
    }

    [UmbraRootNode(null, false)]
    internal sealed class ConfigWithRootNodeNoLabel
    {
    }

    [UmbraRootNode("Open Tree", true)]
    internal sealed class ConfigWithRootNodeDefaultOpenTrue
    {
    }

    [UmbraRootNode("Closed Tree", false)]
    internal sealed class ConfigWithRootNodeDefaultOpenFalse
    {
    }

    private static ConfigDrawer<TConfig> GetDrawer<TConfig>(ConfigSection<TConfig> section) where TConfig : class, new() => TestReflectionHelper.GetRequiredPrivateFieldValue<ConfigSection<TConfig>, ConfigDrawer<TConfig>>(section, "_drawer");

    private static List<IDrawNode> GetTopLevelNodes<TConfig>(ConfigDrawer<TConfig> drawer) where TConfig : class => TestReflectionHelper.GetRequiredPrivateFieldValue<ConfigDrawer<TConfig>, List<IDrawNode>>(drawer, "_nodes");

    private static ConfigDrawerSearchState? GetSearchState<TConfig>(ConfigDrawer<TConfig> drawer) where TConfig : class
    {
        var controller = TestReflectionHelper.GetRequiredPrivateFieldValue<ConfigDrawer<TConfig>, object>(drawer, "_searchController");
        return TestReflectionHelper.GetRequiredPrivatePropertyValue<object, ConfigDrawerSearchState>(controller, "CurrentState");
    }

    private static class TestReflectionHelper
    {
        public static TValue GetRequiredPrivateFieldValue<TInstance, TValue>(TInstance instance, string fieldName)
            where TInstance : class
            where TValue : class
        {
            var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field);

            var value = field.GetValue(instance) as TValue;
            Assert.IsNotNull(value);

            return value;
        }

        public static TValue? GetRequiredPrivatePropertyValue<TInstance, TValue>(TInstance instance, string propertyName)
            where TInstance : class
            where TValue : class
        {
            var property = instance.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(property);

            return property.GetValue(instance) as TValue;
        }
    }
}
