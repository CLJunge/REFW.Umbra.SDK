using Umbra.Config.Attributes;

namespace Umbra.Config.UnitTests;
/// <summary>
/// Unit tests for <see cref = "ConfigStore{TConfig}.ResetAll"/> method.
/// </summary>
[TestClass]
public partial class ConfigStoreTests
{
    /// <summary>
    /// Tests that ResetAll successfully executes when the store is loaded and not disposed.
    /// </summary>
    [TestMethod]
    public void ResetAll_WhenLoadedAndNotDisposed_Succeeds()
    {
        // Arrange
        var tempPath = System.IO.Path.GetTempFileName();
        try
        {
            var store = new ConfigStore<TestConfig>(tempPath);
            store.Load();
            // Act
            store.ResetAll();
            // Assert - no exception thrown
            Assert.IsTrue(store.IsLoaded);
            Assert.IsFalse(store.IsDisposed);
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
                System.IO.File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Helper configuration class for testing with delegate parameters.
    /// </summary>
    [Attributes.UmbraAutoRegister]
    internal record TestConfigWithDelegates
    {
        [Attributes.UmbraParameter]
        public Parameter<int> IntValue { get; set; } = new(42);

        [Attributes.UmbraParameter]
        public Parameter<Action> DelegateValue { get; set; } = new(() =>
        {
        });

        [Attributes.UmbraParameter]
        public Parameter<string> StringValue { get; set; } = new("default");
    }

    /// <summary>
    /// Helper configuration class for testing with only delegate parameters.
    /// </summary>
    [Attributes.UmbraAutoRegister]
    internal record TestConfigOnlyDelegates
    {
        [Attributes.UmbraParameter]
        public Parameter<Action> DelegateValue1 { get; set; } = new(() =>
        {
        });

        [Attributes.UmbraParameter]
        public Parameter<Action<int>> DelegateValue2 { get; set; } = new(_ =>
        {
        });
    }

    /// <summary>
    /// Helper configuration class for testing with no parameters.
    /// </summary>
    internal record TestConfigEmpty
    {
    }

    /// <summary>
    /// Unsupported config-store implementation used to verify the interface-based copy contract.
    /// </summary>
    private sealed class UnsupportedConfigStoreTarget : IConfigStore<TestConfig>
    {
        public bool IsLoaded => true;

        public bool IsDisposed => false;

        public TestConfig Load() => throw new NotSupportedException();

        public void Save() => throw new NotSupportedException();

        public void Export(string filePath) => throw new NotSupportedException();

        public ConfigImportReport Import(string filePath, ConfigImportOptions? options = null)
            => throw new NotSupportedException();

        public void CopyValuesTo(IConfigStore<TestConfig> target, bool setWithoutNotifying = false)
            => throw new NotSupportedException();

        public void AddListenerToAll(Action listener) => throw new NotSupportedException();

        public void AddListenerToAll<T>(Action<T?, T?> listener) => throw new NotSupportedException();

        public void AddListenerToAll(Func<IParameter, bool> predicate, Action listener) => throw new NotSupportedException();

        public void RemoveListenerFromAll(Action listener) => throw new NotSupportedException();

        public void RemoveListenerFromAll<T>(Action<T?, T?> listener) => throw new NotSupportedException();

        public void RemoveListenerFromAll(Func<IParameter, bool> predicate, Action listener) => throw new NotSupportedException();

        public void ResetAll() => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// Tests that Dispose sets IsDisposed to true on first call.
    /// </summary>
    [TestMethod]
    public void Dispose_FirstCall_SetsIsDisposedTrue()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        try
        {
            var store = new ConfigStore<TestConfig>(tempPath);
            // Act
            store.Dispose();
            // Assert
            Assert.IsTrue(store.IsDisposed);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.Export(string)"/> requires a successful load first.
    /// </summary>
    [TestMethod]
    public void Export_BeforeLoad_ThrowsInvalidOperationException()
    {
        var runtimePath = Path.Combine(Path.GetTempPath(), $"runtime_{Guid.NewGuid()}.json");
        var exportPath = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<TestConfig>(runtimePath);

            Assert.ThrowsExactly<InvalidOperationException>(() => store.Export(exportPath));
        }
        finally
        {
            if (File.Exists(runtimePath))
                File.Delete(runtimePath);
            if (File.Exists(exportPath))
                File.Delete(exportPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.Import(string, ConfigImportOptions?)"/> requires a successful load first.
    /// </summary>
    [TestMethod]
    public void Import_BeforeLoad_ThrowsInvalidOperationException()
    {
        var runtimePath = Path.Combine(Path.GetTempPath(), $"runtime_{Guid.NewGuid()}.json");
        var importPath = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid()}.json");
        try
        {
            File.WriteAllText(importPath, "{}");
            var store = new ConfigStore<TestConfig>(runtimePath);

            Assert.ThrowsExactly<InvalidOperationException>(() => store.Import(importPath));
        }
        finally
        {
            if (File.Exists(runtimePath))
                File.Delete(runtimePath);
            if (File.Exists(importPath))
                File.Delete(importPath);
        }
    }

    /// <summary>
    /// Verifies that export uses the declared config-schema version metadata from the config type.
    /// </summary>
    [TestMethod]
    public void Export_WhenConfigTypeDeclaresSchemaVersion_WritesDeclaredVersionToExchangeDocument()
    {
        var runtimePath = Path.Combine(Path.GetTempPath(), $"runtime_{Guid.NewGuid()}.json");
        var exportPath = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<VersionedConfig>(runtimePath);
            _ = store.Load();

            store.Export(exportPath);

            using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(exportPath));
            Assert.AreEqual(7, document.RootElement.GetProperty("schemaVersion").GetInt32());
            Assert.AreEqual(typeof(VersionedConfig).FullName, document.RootElement.GetProperty("schemaId").GetString());
        }
        finally
        {
            if (File.Exists(runtimePath))
                File.Delete(runtimePath);
            if (File.Exists(exportPath))
                File.Delete(exportPath);
        }
    }

    /// <summary>
    /// Verifies that reset still affects only non-delegate parameters after the registered-
    /// parameter operations are delegated out of the store type.
    /// </summary>
    [TestMethod]
    public void ResetAll_WithDelegateParameters_ResetsOnlyNonDelegateValues()
    {
        var runtimePath = Path.Combine(Path.GetTempPath(), $"runtime_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<TestConfigWithDelegates>(runtimePath);
            var config = store.Load();
            var originalDelegate = config.DelegateValue.Value;

            config.IntValue.Set(100);
            config.StringValue.Set("changed");

            store.ResetAll();

            Assert.AreEqual(42, config.IntValue.Value);
            Assert.AreEqual("default", config.StringValue.Value);
            Assert.AreSame(originalDelegate, config.DelegateValue.Value);
        }
        finally
        {
            if (File.Exists(runtimePath))
                File.Delete(runtimePath);
        }
    }

    /// <summary>
    /// Verifies that silent copy still updates matching values without raising target listeners.
    /// </summary>
    [TestMethod]
    public void CopyValuesTo_WhenSetWithoutNotifyingTrue_UpdatesTargetWithoutRaisingListeners()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"source_{Guid.NewGuid()}.json");
        var targetPath = Path.Combine(Path.GetTempPath(), $"target_{Guid.NewGuid()}.json");
        try
        {
            var source = new ConfigStore<TestConfig>(sourcePath);
            var sourceConfig = source.Load();
            var target = new ConfigStore<TestConfig>(targetPath);
            var targetConfig = target.Load();
            var listenerCalls = 0;

            sourceConfig.Value1.Set(321);
            sourceConfig.Value2.Set("copied");
            target.AddListenerToAll(() => listenerCalls++);

            source.CopyValuesTo(target, setWithoutNotifying: true);

            Assert.AreEqual(321, targetConfig.Value1.Value);
            Assert.AreEqual("copied", targetConfig.Value2.Value);
            Assert.AreEqual(0, listenerCalls);
        }
        finally
        {
            if (File.Exists(sourcePath))
                File.Delete(sourcePath);
            if (File.Exists(targetPath))
                File.Delete(targetPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.Export(string)"/> rejects calls after disposal.
    /// </summary>
    [TestMethod]
    public void Export_AfterDispose_ThrowsObjectDisposedException()
    {
        var runtimePath = Path.Combine(Path.GetTempPath(), $"runtime_{Guid.NewGuid()}.json");
        var exportPath = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<TestConfig>(runtimePath);
            _ = store.Load();
            store.Dispose();

            Assert.ThrowsExactly<ObjectDisposedException>(() => store.Export(exportPath));
        }
        finally
        {
            if (File.Exists(runtimePath))
                File.Delete(runtimePath);
            if (File.Exists(exportPath))
                File.Delete(exportPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.Import(string, ConfigImportOptions?)"/> rejects calls after disposal.
    /// </summary>
    [TestMethod]
    public void Import_AfterDispose_ThrowsObjectDisposedException()
    {
        var runtimePath = Path.Combine(Path.GetTempPath(), $"runtime_{Guid.NewGuid()}.json");
        var importPath = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid()}.json");
        try
        {
            File.WriteAllText(importPath, "{}");
            var store = new ConfigStore<TestConfig>(runtimePath);
            _ = store.Load();
            store.Dispose();

            Assert.ThrowsExactly<ObjectDisposedException>(() => store.Import(importPath));
        }
        finally
        {
            if (File.Exists(runtimePath))
                File.Delete(runtimePath);
            if (File.Exists(importPath))
                File.Delete(importPath);
        }
    }

    /// <summary>
    /// Tests that Dispose is idempotent and can be called multiple times without error.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        try
        {
            var store = new ConfigStore<TestConfig>(tempPath);
            // Act - call Dispose multiple times
            store.Dispose();
            store.Dispose();
            store.Dispose();
            // Assert - no exception thrown and IsDisposed remains true
            Assert.IsTrue(store.IsDisposed);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Dispose can be called on a store that has been loaded with no file present.
    /// </summary>
    [TestMethod]
    public void Dispose_AfterLoadWithNoFile_CompletesSuccessfully()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        try
        {
            var store = new ConfigStore<TestConfig>(tempPath);
            store.Load();
            // Act
            store.Dispose();
            // Assert
            Assert.IsTrue(store.IsDisposed);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that Load creates a default config file when no file exists at the specified path.
    /// This is an integration test that exercises the full Save flow.
    /// </summary>
    [TestMethod]
    public void Load_WhenNoFileExists_CreatesDefaultFile()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var store = new ConfigStore<TestConfig>(filePath);
        // Act
        var result = store.Load();
        // Assert
        Assert.IsNotNull(result, "Load should return a non-null instance");
        Assert.IsTrue(File.Exists(filePath), "Load should create a default config file when none exists");
        store.Dispose();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Verifies that Load properly transitions the store state from unloaded to loaded
    /// without allowing intermediate invalid states.
    /// </summary>
    [TestMethod]
    public void Load_TransitionsStateCorrectly()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(filePath);
        // Assert initial state
        Assert.IsFalse(store.IsLoaded, "Store should start in unloaded state");
        Assert.IsFalse(store.IsDisposed, "Store should not be disposed initially");
        // Act
        var result = store.Load();
        // Assert final state
        Assert.IsTrue(store.IsLoaded, "Store should be in loaded state after Load completes");
        Assert.IsFalse(store.IsDisposed, "Store should still not be disposed");
        Assert.IsNotNull(result, "Load should return a valid config instance");
        store.Dispose();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Creates a ConfigStore instance and uses reflection to set internal state for testing.
    /// </summary>
    private static ConfigStore<TestConfig> CreateConfigStoreWithState(bool isLoaded, bool isDisposed, Dictionary<string, IParameter>? parameters = null)
    {
        var store = new ConfigStore<TestConfig>("test.json");
        var loadedField = typeof(ConfigStore<TestConfig>).GetField("_loaded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        loadedField!.SetValue(store, isLoaded);
        var disposedField = typeof(ConfigStore<TestConfig>).GetField("_disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        disposedField!.SetValue(store, isDisposed);
        if (parameters != null)
        {
            var parametersField = typeof(ConfigStore<TestConfig>).GetField("_parameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var existingParameters = (Dictionary<string, IParameter>)parametersField!.GetValue(store)!;
            existingParameters.Clear();
            foreach (var kvp in parameters)
            {
                existingParameters[kvp.Key] = kvp.Value;
            }
        }

        return store;
    }

    /// <summary>
    /// Helper method to create a Parameter{T} instance for testing.
    /// </summary>
    private static Parameter<T> CreateParameter<T>(string key, T defaultValue)
    {
        var parameter = new Parameter<T>(defaultValue)
        {
            Key = key
        };
        return parameter;
    }

    /// <summary>
    /// Verifies that AddListenerToAll succeeds when the parameter collection is empty.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_EmptyParameters_Succeeds()
    {
        // Arrange
        var parameters = new Dictionary<string, IParameter>();
        var store = CreateConfigStoreWithState(isLoaded: true, isDisposed: false, parameters);
        var listenerCalled = false;
        void listener(int oldVal, int newVal) => listenerCalled = true;
        // Act
        store.AddListenerToAll((Action<int, int>)listener);
        // Assert - No exception thrown, listener was not called
        Assert.IsFalse(listenerCalled);
    }

    /// <summary>
    /// Verifies that AddListenerToAll subscribes only to parameters matching the type argument.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_MixedParameterTypes_SubscribesOnlyToMatchingType()
    {
        // Arrange
        var intParam = CreateParameter("intParam", 42);
        var stringParam = CreateParameter("stringParam", "test");
        var boolParam = CreateParameter("boolParam", true);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "intParam",
                intParam
            },
            {
                "stringParam",
                stringParam
            },
            {
                "boolParam",
                boolParam
            }
        };
        var store = CreateConfigStoreWithState(isLoaded: true, isDisposed: false, parameters);
        var intListenerCallCount = 0;
        void intListener(int oldVal, int newVal) => intListenerCallCount++;
        // Act
        store.AddListenerToAll((Action<int, int>)intListener);
        intParam.Set(100);
        stringParam.Set("changed");
        boolParam.Set(false);
        // Assert
        Assert.AreEqual(1, intListenerCallCount, "Int listener should be called once");
    }

    /// <summary>
    /// Verifies that AddListenerToAll subscribes to all parameters of the matching type.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_MultipleMatchingParameters_SubscribesToAll()
    {
        // Arrange
        var intParam1 = CreateParameter("intParam1", 1);
        var intParam2 = CreateParameter("intParam2", 2);
        var intParam3 = CreateParameter("intParam3", 3);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "intParam1",
                intParam1
            },
            {
                "intParam2",
                intParam2
            },
            {
                "intParam3",
                intParam3
            }
        };
        var store = CreateConfigStoreWithState(isLoaded: true, isDisposed: false, parameters);
        var callCount = 0;
        void listener(int oldVal, int newVal) => callCount++;
        // Act
        store.AddListenerToAll((Action<int, int>)listener);
        intParam1.Set(10);
        intParam2.Set(20);
        intParam3.Set(30);
        // Assert
        Assert.AreEqual(3, callCount, "Listener should be called for each parameter change");
    }

    /// <summary>
    /// Verifies that the listener receives correct old and new values when a parameter changes.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_ParameterChanges_ListenerReceivesCorrectValues()
    {
        // Arrange
        var parameter = CreateParameter("testParam", 42);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "testParam",
                parameter
            }
        };
        var store = CreateConfigStoreWithState(isLoaded: true, isDisposed: false, parameters);
        int? capturedOldValue = null;
        int? capturedNewValue = null;
        void listener(int oldVal, int newVal)
        {
            capturedOldValue = oldVal;
            capturedNewValue = newVal;
        }
        // Act
        store.AddListenerToAll((Action<int, int>)listener);
        parameter.Set(100);
        // Assert
        Assert.AreEqual(42, capturedOldValue, "Old value should be the original value");
        Assert.AreEqual(100, capturedNewValue, "New value should be the updated value");
    }

    /// <summary>
    /// Verifies that AddListenerToAll can be called multiple times with the same listener.
    /// Each subscription is tracked independently per the documentation.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_SameListenerAddedTwice_BothSubscriptionsActive()
    {
        // Arrange
        var parameter = CreateParameter("intParam", 10);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "intParam",
                parameter
            }
        };
        var store = CreateConfigStoreWithState(isLoaded: true, isDisposed: false, parameters);
        var callCount = 0;
        void listener(int oldVal, int newVal) => callCount++;
        // Act
        store.AddListenerToAll((Action<int, int>)listener);
        store.AddListenerToAll((Action<int, int>)listener);
        parameter.Set(20);
        // Assert
        Assert.AreEqual(2, callCount, "Listener should be called twice since it was added twice");
    }

    /// <summary>
    /// Verifies that AddListenerToAll does not subscribe to parameters of non-matching types
    /// when no matching parameters exist.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_NoMatchingParameters_NoSubscriptions()
    {
        // Arrange
        var stringParam = CreateParameter("stringParam", "test");
        var boolParam = CreateParameter("boolParam", true);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "stringParam",
                stringParam
            },
            {
                "boolParam",
                boolParam
            }
        };
        var store = CreateConfigStoreWithState(isLoaded: true, isDisposed: false, parameters);
        var intListenerCallCount = 0;
        void intListener(int oldVal, int newVal) => intListenerCallCount++;
        // Act
        store.AddListenerToAll((Action<int, int>)intListener);
        stringParam.Set("changed");
        boolParam.Set(false);
        // Assert
        Assert.AreEqual(0, intListenerCallCount, "Int listener should not be called for non-int parameters");
    }

    /// <summary>
    /// Verifies that multiple listeners of different types can be added without interference.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_MultipleListenersDifferentTypes_AllWorkIndependently()
    {
        // Arrange
        var intParam = CreateParameter("intParam", 42);
        var stringParam = CreateParameter("stringParam", "test");
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "intParam",
                intParam
            },
            {
                "stringParam",
                stringParam
            }
        };
        var store = CreateConfigStoreWithState(isLoaded: true, isDisposed: false, parameters);
        var intCallCount = 0;
        var stringCallCount = 0;
        void intListener(int oldVal, int newVal) => intCallCount++;
        void stringListener(string? oldVal, string? newVal) => stringCallCount++;
        // Act
        store.AddListenerToAll((Action<int, int>)intListener);
        store.AddListenerToAll((Action<string?, string?>)stringListener);
        intParam.Set(100);
        stringParam.Set("updated");
        // Assert
        Assert.AreEqual(1, intCallCount, "Int listener should be called once");
        Assert.AreEqual(1, stringCallCount, "String listener should be called once");
    }

    /// <summary>
    /// Verifies that listener is not invoked when SetWithoutNotify is used on a parameter.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_SetWithoutNotify_ListenerNotInvoked()
    {
        // Arrange
        var parameter = CreateParameter("intParam", 10);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "intParam",
                parameter
            }
        };
        var store = CreateConfigStoreWithState(isLoaded: true, isDisposed: false, parameters);
        var callCount = 0;
        void listener(int oldVal, int newVal) => callCount++;
        // Act
        store.AddListenerToAll((Action<int, int>)listener);
        parameter.SetWithoutNotify(20);
        // Assert
        Assert.AreEqual(0, callCount, "Listener should not be called when SetWithoutNotify is used");
    }

    /// <summary>
    /// Tests that AddListenerToAll successfully subscribes the listener to all parameters and the listener is invoked when parameter values change.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_ValidListener_SubscribesToAllParameters()
    {
        // Arrange
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempFilePath);
        var config = store.Load();
        var callCount = 0;
        void listener() => callCount++;
        // Act
        store.AddListenerToAll(listener);
        config.Value1.Set(100);
        try
        {
            // Assert
            Assert.AreEqual(1, callCount, "Listener should have been invoked once when parameter changed.");
        }
        finally
        {
            store.Dispose();
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    /// <summary>
    /// Tests that AddListenerToAll tracks subscriptions independently when the same listener is added multiple times.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_SameListenerAddedTwice_TracksIndependently()
    {
        // Arrange
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempFilePath);
        var config = store.Load();
        var callCount = 0;
        void listener() => callCount++;
        // Act
        store.AddListenerToAll(listener);
        store.AddListenerToAll(listener);
        config.Value1.Set(200);
        try
        {
            // Assert
            Assert.AreEqual(2, callCount, "Listener should have been invoked twice (once per subscription) when parameter changed.");
        }
        finally
        {
            store.Dispose();
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    /// <summary>
    /// Tests that AddListenerToAll invokes the listener for changes to multiple parameters.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_MultipleParameters_InvokesListenerForEach()
    {
        // Arrange
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempFilePath);
        var config = store.Load();
        var callCount = 0;
        void listener() => callCount++;
        // Act
        store.AddListenerToAll(listener);
        config.Value1.Set(300);
        config.Value2.Set("changed");
        try
        {
            // Assert
            Assert.AreEqual(2, callCount, "Listener should have been invoked for each parameter change.");
        }
        finally
        {
            store.Dispose();
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    /// <summary>
    /// Tests that AddListenerToAll correctly registers cleanup that unsubscribes the listener when the store is disposed.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_Dispose_UnsubscribesListener()
    {
        // Arrange
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempFilePath);
        var config = store.Load();
        var callCount = 0;
        void listener() => callCount++;
        store.AddListenerToAll(listener);
        // Act
        store.Dispose();
        config.Value1.Set(500);
        try
        {
            // Assert
            Assert.AreEqual(0, callCount, "Listener should not be invoked after store is disposed.");
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    #region Test Configuration Classes
    [UmbraAutoRegister]
    public partial record TestConfig
    {
        [UmbraParameter]
        public Parameter<int> Value1 { get; set; } = new(10);

        [UmbraParameter]
        public Parameter<string> Value2 { get; set; } = new("default");
    }

    [UmbraAutoRegister]
    [UmbraConfigVersion(7)]
    public partial record VersionedConfig
    {
        [UmbraParameter]
        public Parameter<int> Value { get; set; } = new(10);
    }

    #endregion
    /// <summary>
    /// Verifies that <see cref = "ConfigStore{TConfig}.IsDisposed"/> returns <see langword="false"/>
    /// immediately after construction before any disposal has occurred.
    /// </summary>
    [TestMethod]
    public void IsDisposed_AfterConstruction_ReturnsFalse()
    {
        // Arrange & Act
        var store = new ConfigStore<TestConfig>("test.json");
        // Assert
        Assert.IsFalse(store.IsDisposed);
    }

    /// <summary>
    /// Verifies that <see cref = "ConfigStore{TConfig}.IsDisposed"/> returns <see langword="true"/>
    /// after the <see cref = "ConfigStore{TConfig}.Dispose"/> method has been called.
    /// </summary>
    [TestMethod]
    public void IsDisposed_AfterDispose_ReturnsTrue()
    {
        // Arrange
        var store = new ConfigStore<TestConfig>("test.json");
        // Act
        store.Dispose();
        // Assert
        Assert.IsTrue(store.IsDisposed);
    }

    /// <summary>
    /// Verifies that Save can be called multiple times on a loaded store without errors.
    /// </summary>
    [TestMethod]
    public void Save_WhenCalledMultipleTimes_SucceedsWithoutError()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<TestConfig>(tempPath);
            var config = store.Load();
            // Act
            store.Save();
            store.Save();
            store.Save();
            // Assert
            Assert.IsTrue(File.Exists(tempPath), "Configuration file should exist after multiple saves.");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref = "ConfigStore{TConfig}.IsLoaded"/> returns false immediately after construction
    /// and before <see cref = "ConfigStore{TConfig}.Load"/> has been called.
    /// </summary>
    [TestMethod]
    public void IsLoaded_AfterConstruction_ReturnsFalse()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempPath);
        // Act
        var result = store.IsLoaded;
        // Assert
        Assert.IsFalse(result);
        store.Dispose();
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Tests that <see cref = "ConfigStore{TConfig}.IsLoaded"/> returns true after
    /// <see cref = "ConfigStore{TConfig}.Load"/> has been successfully called.
    /// </summary>
    [TestMethod]
    public void IsLoaded_AfterLoadCompletes_ReturnsTrue()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempPath);
        // Act
        _ = store.Load();
        var result = store.IsLoaded;
        // Assert
        Assert.IsTrue(result);
        store.Dispose();
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Tests that <see cref = "ConfigStore{TConfig}.IsLoaded"/> remains true after
    /// <see cref = "ConfigStore{TConfig}.Dispose"/> has been called on a loaded store.
    /// </summary>
    [TestMethod]
    public void IsLoaded_AfterDispose_RemainsTrue()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempPath);
        _ = store.Load();
        // Act
        store.Dispose();
        var result = store.IsLoaded;
        // Assert
        Assert.IsTrue(result);
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Tests that <see cref = "ConfigStore{TConfig}.IsLoaded"/> remains false after
    /// <see cref = "ConfigStore{TConfig}.Dispose"/> has been called on a store that was never loaded.
    /// </summary>
    [TestMethod]
    public void IsLoaded_AfterDisposeWithoutLoad_RemainsFalse()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempPath);
        // Act
        store.Dispose();
        var result = store.IsLoaded;
        // Assert
        Assert.IsFalse(result);
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Tests that <see cref = "ConfigStore{TConfig}.IsLoaded"/> returns true when a valid persisted
    /// configuration file exists and <see cref = "ConfigStore{TConfig}.Load"/> successfully loads values from it.
    /// </summary>
    [TestMethod]
    public void IsLoaded_AfterLoadingExistingFile_ReturnsTrue()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var initialStore = new ConfigStore<TestConfig>(tempPath);
        _ = initialStore.Load();
        initialStore.Save();
        initialStore.Dispose();
        var store = new ConfigStore<TestConfig>(tempPath);
        // Act
        _ = store.Load();
        var result = store.IsLoaded;
        // Assert
        Assert.IsTrue(result);
        store.Dispose();
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Tests that <see cref = "ConfigStore{TConfig}.IsLoaded"/> is not affected by multiple reads
    /// and consistently returns true after <see cref = "ConfigStore{TConfig}.Load"/> completes.
    /// </summary>
    [TestMethod]
    public void IsLoaded_MultipleReads_ConsistentlyReturnsTrue()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempPath);
        _ = store.Load();
        // Act & Assert - verify multiple reads return the same value
        for (var i = 0; i < 100; i++)
        {
            Assert.IsTrue(store.IsLoaded);
        }

        store.Dispose();
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Test configuration class with different parameters for overlap testing.
    /// </summary>
    [UmbraAutoRegister]
    private record DifferentConfig
    {
        [UmbraParameter]
        public Parameter<int> IntValue { get; set; } = new(0);

        [UmbraParameter]
        public Parameter<double> DoubleValue { get; set; } = new(3.14);
    }

    /// <summary>
    /// Tests that the constructor succeeds and initializes the store with valid file paths.
    /// </summary>
    /// <param name = "filePath">The valid file path to test.</param>
    /// <param name = "testCase">Description of the test case.</param>
    [TestMethod]
    [DataRow("a", "Single character")]
    [DataRow("config.json", "Simple filename")]
    [DataRow("C:\\Users\\config.json", "Absolute Windows path")]
    [DataRow("/home/user/config.json", "Absolute Unix path")]
    [DataRow("./config.json", "Relative path")]
    [DataRow("../config.json", "Parent relative path")]
    [DataRow("my config.json", "Filename with spaces")]
    [DataRow("config@#$.json", "Filename with special characters")]
    [DataRow("X:\\very\\long\\nested\\path\\to\\some\\configuration\\file\\location\\config.json", "Long path")]
    [DataRow("C:\\Users\\User Name\\AppData\\Local\\Config Files\\config.json", "Path with spaces")]
    public void Constructor_ValidFilePath_CreatesInstance(string filePath, string testCase)
    {
        // Act
        var store = new ConfigStore<TestConfig>(filePath);
        // Assert
        Assert.IsNotNull(store, $"Failed for test case: {testCase}");
        Assert.IsFalse(store.IsLoaded, $"Store should not be loaded immediately after construction. Failed for test case: {testCase}");
        Assert.IsFalse(store.IsDisposed, $"Store should not be disposed immediately after construction. Failed for test case: {testCase}");
    }

    /// <summary>
    /// Verifies that the constructor rejects a null file path.
    /// </summary>
    [TestMethod]
    public void Constructor_NullFilePath_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ConfigStore<TestConfig>(null!));

        Assert.AreEqual("filePath", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects whitespace-only file paths.
    /// </summary>
    [TestMethod]
    public void Constructor_WhitespaceFilePath_ThrowsArgumentException()
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(() => _ = new ConfigStore<TestConfig>("   "));

        Assert.AreEqual("filePath", exception.ParamName);
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.Load"/> can only be called once per store instance.
    /// </summary>
    [TestMethod]
    public void Load_CalledTwice_ThrowsInvalidOperationException()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<TestConfig>(tempPath);
            _ = store.Load();

            Assert.ThrowsExactly<InvalidOperationException>(() => store.Load());
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.Load"/> leaves the store unloaded when registration fails.
    /// </summary>
    [TestMethod]
    public void Load_WhenRegistrationThrowsDuplicateKey_LeavesStoreUnloaded()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<DuplicateKeyConfig>(tempPath);

            Assert.ThrowsExactly<InvalidOperationException>(() => store.Load());
            Assert.IsFalse(store.IsLoaded);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.Save"/> requires a successful load first.
    /// </summary>
    [TestMethod]
    public void Save_BeforeLoad_ThrowsInvalidOperationException()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<TestConfig>(tempPath);

            Assert.ThrowsExactly<InvalidOperationException>(() => store.Save());
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.Import(string, ConfigImportOptions?)"/> rejects envelope documents whose schema identifier does not match the current config type.
    /// </summary>
    [TestMethod]
    public void Import_MismatchedSchemaId_ReturnsFailedReportAndLeavesValuesUnchanged()
    {
        var runtimePath = Path.Combine(Path.GetTempPath(), $"runtime_{Guid.NewGuid()}.json");
        var importPath = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid()}.json");
        try
        {
            File.WriteAllText(importPath, """
{
  "formatVersion": 1,
  "schemaId": "Umbra.Config.UnitTests.OtherConfig",
  "schemaVersion": 1,
  "values": {
    "value1": 123
  }
}
""");

            var store = new ConfigStore<TestConfig>(runtimePath);
            var config = store.Load();

            var report = store.Import(importPath, new ConfigImportOptions { SaveAfterImport = false });

            Assert.IsFalse(report.Success);
            Assert.AreEqual(10, config.Value1.Value);
        }
        finally
        {
            if (File.Exists(runtimePath))
                File.Delete(runtimePath);
            if (File.Exists(importPath))
                File.Delete(importPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.Import(string, ConfigImportOptions?)"/> rejects newer schema versions before applying any values.
    /// </summary>
    [TestMethod]
    public void Import_NewerSchemaVersion_ReturnsFailedReportAndLeavesValuesUnchanged()
    {
        var runtimePath = Path.Combine(Path.GetTempPath(), $"runtime_{Guid.NewGuid()}.json");
        var importPath = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid()}.json");
        try
        {
            File.WriteAllText(importPath, $$"""
{
  "formatVersion": 1,
  "schemaId": "{{typeof(TestConfig).FullName}}",
  "schemaVersion": 2,
  "values": {
    "value1": 123
  }
}
""");

            var store = new ConfigStore<TestConfig>(runtimePath);
            var config = store.Load();

            var report = store.Import(importPath, new ConfigImportOptions { SaveAfterImport = false });

            Assert.IsFalse(report.Success);
            Assert.AreEqual(10, config.Value1.Value);
        }
        finally
        {
            if (File.Exists(runtimePath))
                File.Delete(runtimePath);
            if (File.Exists(importPath))
                File.Delete(importPath);
        }
    }

    /// <summary>
    /// Verifies that successful import can persist the accepted final state to the runtime config file.
    /// </summary>
    [TestMethod]
    public void Import_SaveAfterImportEnabled_PersistsAcceptedValues()
    {
        var runtimePath = Path.Combine(Path.GetTempPath(), $"runtime_{Guid.NewGuid()}.json");
        var importPath = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid()}.json");
        try
        {
            File.WriteAllText(importPath, $$"""
{
  "formatVersion": 1,
  "schemaId": "{{typeof(TestConfig).FullName}}",
  "schemaVersion": 1,
  "values": {
    "value1": 123,
    "value2": "imported"
  }
}
""");

            var store = new ConfigStore<TestConfig>(runtimePath);
            var config = store.Load();

            var report = store.Import(importPath);

            Assert.IsTrue(report.Success);
            Assert.IsTrue(report.Saved);
            Assert.AreEqual(123, config.Value1.Value);
            Assert.AreEqual("imported", config.Value2.Value);

            using var runtimeDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(runtimePath));
            Assert.AreEqual(123, runtimeDocument.RootElement.GetProperty("value1").GetInt32());
            Assert.AreEqual("imported", runtimeDocument.RootElement.GetProperty("value2").GetString());
        }
        finally
        {
            if (File.Exists(runtimePath))
                File.Delete(runtimePath);
            if (File.Exists(importPath))
                File.Delete(importPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.CopyValuesTo"/> rejects a null target.
    /// </summary>
    [TestMethod]
    public void CopyValuesTo_NullTarget_ThrowsArgumentNullException()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<TestConfig>(tempPath);
            _ = store.Load();

            Assert.ThrowsExactly<ArgumentNullException>(() => store.CopyValuesTo(null!));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.CopyValuesTo"/> requires a target store that has already loaded.
    /// </summary>
    [TestMethod]
    public void CopyValuesTo_TargetNotLoaded_ThrowsInvalidOperationException()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"source_{Guid.NewGuid()}.json");
        var targetPath = Path.Combine(Path.GetTempPath(), $"target_{Guid.NewGuid()}.json");
        try
        {
            var source = new ConfigStore<TestConfig>(sourcePath);
            var target = new ConfigStore<TestConfig>(targetPath);
            _ = source.Load();

            Assert.ThrowsExactly<InvalidOperationException>(() => source.CopyValuesTo(target));
        }
        finally
        {
            if (File.Exists(sourcePath))
                File.Delete(sourcePath);
            if (File.Exists(targetPath))
                File.Delete(targetPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="IConfigStore{TConfig}.CopyValuesTo(IConfigStore{TConfig}, bool)"/>
    /// rejects target implementations that do not participate in Umbra's internal copy-target contract.
    /// </summary>
    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Interface-based tests are necessary to verify behavior through the public interface surface.")]
    public void CopyValuesTo_InterfaceTargetWithoutCopyContract_ThrowsInvalidOperationException()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"source_{Guid.NewGuid()}.json");
        try
        {
            IConfigStore<TestConfig> source = new ConfigStore<TestConfig>(sourcePath);
            _ = source.Load();
            var unsupportedTarget = new UnsupportedConfigStoreTarget();

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => source.CopyValuesTo(unsupportedTarget));

            Assert.Contains("supports Umbra parameter-map copy operations", exception.Message);
            source.Dispose();
        }
        finally
        {
            if (File.Exists(sourcePath))
                File.Delete(sourcePath);
        }
    }

    /// <summary>
    /// Verifies that silent copy updates target values without raising target listeners.
    /// </summary>
    [TestMethod]
    public void CopyValuesTo_SetWithoutNotifying_DoesNotRaiseTargetListeners()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"source_{Guid.NewGuid()}.json");
        var targetPath = Path.Combine(Path.GetTempPath(), $"target_{Guid.NewGuid()}.json");
        try
        {
            var source = new ConfigStore<TestConfig>(sourcePath);
            var target = new ConfigStore<TestConfig>(targetPath);
            var sourceConfig = source.Load();
            var targetConfig = target.Load();
            var callCount = 0;

            sourceConfig.Value1.Set(123);
            sourceConfig.Value2.Set("copied");
            target.AddListenerToAll(() => callCount++);

            source.CopyValuesTo(target, setWithoutNotifying: true);

            Assert.AreEqual(123, targetConfig.Value1.Value);
            Assert.AreEqual("copied", targetConfig.Value2.Value);
            Assert.AreEqual(0, callCount);

            source.Dispose();
            target.Dispose();
        }
        finally
        {
            if (File.Exists(sourcePath))
                File.Delete(sourcePath);
            if (File.Exists(targetPath))
                File.Delete(targetPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="IConfigStore{TConfig}.CopyValuesTo"/> is available and usable
    /// through the public interface surface.
    /// </summary>
    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Interface-based tests are necessary to verify behavior through the public interface surface.")]
    public void CopyValuesTo_ThroughInterface_CopiesValuesToTargetStore()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"source_{Guid.NewGuid()}.json");
        var targetPath = Path.Combine(Path.GetTempPath(), $"target_{Guid.NewGuid()}.json");
        try
        {
            IConfigStore<TestConfig> source = new ConfigStore<TestConfig>(sourcePath);
            var target = new ConfigStore<TestConfig>(targetPath);
            var sourceConfig = source.Load();
            var targetConfig = target.Load();

            sourceConfig.Value1.Set(321);
            sourceConfig.Value2.Set("via-interface");

            source.CopyValuesTo(target);

            Assert.AreEqual(321, targetConfig.Value1.Value);
            Assert.AreEqual("via-interface", targetConfig.Value2.Value);

            source.Dispose();
            target.Dispose();
        }
        finally
        {
            if (File.Exists(sourcePath))
                File.Delete(sourcePath);
            if (File.Exists(targetPath))
                File.Delete(targetPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigStore{TConfig}.ResetAll"/> skips delegate-backed parameters.
    /// </summary>
    [TestMethod]
    public void ResetAll_WithDelegateParameter_SkipsDelegateParameter()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<TestConfigWithDelegates>(tempPath);
            var config = store.Load();
            var originalDelegate = config.DelegateValue.Value;

            config.IntValue.Set(100);
            config.StringValue.Set("changed");
            config.DelegateValue.Set(() => { });

            store.ResetAll();

            Assert.AreEqual(42, config.IntValue.Value);
            Assert.AreEqual("default", config.StringValue.Value);
            Assert.AreNotSame(originalDelegate, config.DelegateValue.Value);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [UmbraAutoRegister]
    private sealed class DuplicateKeyConfig
    {
        [UmbraParameter("duplicate")]
        public Parameter<int> Value1 { get; set; } = new(1);

        [UmbraParameter("duplicate")]
        public Parameter<int> Value2 { get; set; } = new(2);
    }

}

/// <summary>
/// Unit tests for <see cref = "ConfigStore{TConfig}.RemoveListenerFromAll(Action)"/>.
/// </summary>
[TestClass]
public partial class ConfigStoreTests_RemoveListenerFromAll
{
    /// <summary>
    /// Test configuration class with a single parameter for testing.
    /// </summary>
    [UmbraAutoRegister]
    internal sealed class TestConfig
    {
        [UmbraParameter]
        public Parameter<int> TestValue { get; set; } = new(42);
    }

    /// <summary>
    /// Test configuration class with no parameters for edge case testing.
    /// </summary>
    [UmbraAutoRegister]
    internal sealed class EmptyConfig
    {
    }

    /// <summary>
    /// Test configuration class with multiple parameters.
    /// </summary>
    [UmbraAutoRegister]
    internal sealed class MultiParameterConfig
    {
        [UmbraParameter]
        public Parameter<int> Value1 { get; set; } = new(1);

        [UmbraParameter]
        public Parameter<string> Value2 { get; set; } = new("test");

        [UmbraParameter]
        public Parameter<bool> Value3 { get; set; } = new(true);
    }

    /// <summary>
    /// Verifies that RemoveListenerFromAll successfully removes a previously added listener
    /// that was registered via AddListenerToAll, and the listener is no longer invoked.
    /// </summary>
    [TestMethod]
    public void RemoveListenerFromAll_TrackedListener_RemovesListenerSuccessfully()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempPath);
        var config = store.Load();
        var callCount = 0;
        void listener() => callCount++;
        try
        {
            store.AddListenerToAll(listener);
            // Act - Remove the listener
            store.RemoveListenerFromAll(listener);
            config.TestValue.Set(100);
            // Assert - Listener should not have been called
            Assert.AreEqual(0, callCount, "Listener should not be invoked after removal.");
        }
        finally
        {
            store.Dispose();
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that RemoveListenerFromAll removes only one instance of a listener
    /// when the same listener has been added multiple times.
    /// </summary>
    [TestMethod]
    public void RemoveListenerFromAll_ListenerAddedMultipleTimes_RemovesOneInstance()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempPath);
        var config = store.Load();
        var callCount = 0;
        void listener() => callCount++;
        try
        {
            store.AddListenerToAll(listener);
            store.AddListenerToAll(listener);
            // Act - Remove listener once
            store.RemoveListenerFromAll(listener);
            config.TestValue.Set(200);
            // Assert - Listener should still be invoked once (second registration remains)
            Assert.AreEqual(1, callCount, "Listener should be invoked once after removing one registration.");
        }
        finally
        {
            store.Dispose();
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that RemoveListenerFromAll completes without error when removing a listener
    /// that was never added to the store.
    /// </summary>
    [TestMethod]
    public void RemoveListenerFromAll_ListenerNotAdded_CompletesWithoutError()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempPath);
        store.Load();
        static void listener()
        {
        }
        try
        {
            // Act - Remove a listener that was never added
            store.RemoveListenerFromAll(listener);
        }
        finally
        {
            store.Dispose();
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that RemoveListenerFromAll works correctly with an empty parameter set.
    /// </summary>
    [TestMethod]
    public void RemoveListenerFromAll_EmptyParameterSet_CompletesWithoutError()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<EmptyConfig>(tempPath);
        store.Load();
        static void listener()
        {
        }
        try
        {
            store.AddListenerToAll(listener);
            // Act
            store.RemoveListenerFromAll(listener);
        }
        finally
        {
            store.Dispose();
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that RemoveListenerFromAll unsubscribes the listener from all parameters
    /// when there are multiple parameters in the store.
    /// </summary>
    [TestMethod]
    public void RemoveListenerFromAll_MultipleParameters_RemovesFromAll()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<MultiParameterConfig>(tempPath);
        var config = store.Load();
        var callCount = 0;
        void listener() => callCount++;
        try
        {
            store.AddListenerToAll(listener);
            // Act - Remove the listener
            store.RemoveListenerFromAll(listener);
            config.Value1.Set(999);
            config.Value2.Set("changed");
            config.Value3.Set(false);
            // Assert - Listener should not have been called for any parameter
            Assert.AreEqual(0, callCount, "Listener should not be invoked after removal from all parameters.");
        }
        finally
        {
            store.Dispose();
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that RemoveListenerFromAll can be called multiple times with the same listener
    /// without throwing an exception.
    /// </summary>
    [TestMethod]
    public void RemoveListenerFromAll_CalledMultipleTimes_CompletesWithoutError()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempPath);
        var config = store.Load();
        static void listener()
        {
        }
        try
        {
            store.AddListenerToAll(listener);
            // Act - Remove the same listener multiple times
            store.RemoveListenerFromAll(listener);
            store.RemoveListenerFromAll(listener);
        }
        finally
        {
            store.Dispose();
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that RemoveListenerFromAll removes the correct listener when multiple
    /// different listeners are registered.
    /// </summary>
    [TestMethod]
    public void RemoveListenerFromAll_MultipleDifferentListeners_RemovesCorrectOne()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(tempPath);
        var config = store.Load();
        var callCount1 = 0;
        var callCount2 = 0;
        void listener1() => callCount1++;
        void listener2() => callCount2++;
        try
        {
            store.AddListenerToAll(listener1);
            store.AddListenerToAll(listener2);
            // Act - Remove only listener1
            store.RemoveListenerFromAll(listener1);
            config.TestValue.Set(300);
            // Assert - Only listener2 should be invoked
            Assert.AreEqual(0, callCount1, "Listener1 should not be invoked after removal.");
            Assert.AreEqual(1, callCount2, "Listener2 should still be invoked.");
        }
        finally
        {
            store.Dispose();
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that RemoveListenerFromAll completes without error when no parameters match the type.
    /// </summary>
    [TestMethod]
    public void RemoveListenerFromAll_NoMatchingParameters_CompletesWithoutError()
    {
        // Arrange
        var configPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new ConfigStore<TestConfig>(configPath);
        var config = store.Load();
        static void listener(double oldVal, double newVal)
        {
        }
        // Act & Assert (should not throw)
        store.RemoveListenerFromAll((Action<double, double>)listener);
        store.Dispose();
        if (System.IO.File.Exists(configPath))
            System.IO.File.Delete(configPath);
    }

}
