using Umbra.Config.Attributes;

namespace Umbra.Config.UnitTests;
/// <summary>
/// Unit tests for <see cref = "SettingsStore{TConfig}.ResetAll"/> method.
/// </summary>
[TestClass]
public partial class SettingsStoreTests
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
            var store = new SettingsStore<TestConfig>(tempPath);
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
    [Attributes.UmbraAutoRegisterSettings]
    internal record TestConfigWithDelegates
    {
        [Attributes.UmbraSettingsParameter]
        public Parameter<int> IntValue { get; set; } = new(42);

        [Attributes.UmbraSettingsParameter]
        public Parameter<Action> DelegateValue { get; set; } = new(() =>
        {
        });

        [Attributes.UmbraSettingsParameter]
        public Parameter<string> StringValue { get; set; } = new("default");
    }

    /// <summary>
    /// Helper configuration class for testing with only delegate parameters.
    /// </summary>
    [Attributes.UmbraAutoRegisterSettings]
    internal record TestConfigOnlyDelegates
    {
        [Attributes.UmbraSettingsParameter]
        public Parameter<Action> DelegateValue1 { get; set; } = new(() =>
        {
        });

        [Attributes.UmbraSettingsParameter]
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
    /// Tests that Dispose sets IsDisposed to true on first call.
    /// </summary>
    [TestMethod]
    public void Dispose_FirstCall_SetsIsDisposedTrue()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore<TestConfig>(tempPath);
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
    /// Tests that Dispose is idempotent and can be called multiple times without error.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore<TestConfig>(tempPath);
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
            var store = new SettingsStore<TestConfig>(tempPath);
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

        var store = new SettingsStore<TestConfig>(filePath);
        // Act
        var result = store.Load();
        // Assert
        Assert.IsNotNull(result, "Load should return a non-null instance");
        Assert.IsTrue(File.Exists(filePath), "Load should create a default config file when none exists");
        // Cleanup
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
        var store = new SettingsStore<TestConfig>(filePath);
        // Assert initial state
        Assert.IsFalse(store.IsLoaded, "Store should start in unloaded state");
        Assert.IsFalse(store.IsDisposed, "Store should not be disposed initially");
        // Act
        var result = store.Load();
        // Assert final state
        Assert.IsTrue(store.IsLoaded, "Store should be in loaded state after Load completes");
        Assert.IsFalse(store.IsDisposed, "Store should still not be disposed");
        Assert.IsNotNull(result, "Load should return a valid config instance");
        // Cleanup
        store.Dispose();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Creates a SettingsStore instance and uses reflection to set internal state for testing.
    /// </summary>
    private static SettingsStore<TestConfig> CreateSettingsStoreWithState(bool isLoaded, bool isDisposed, Dictionary<string, IParameter>? parameters = null)
    {
        var store = new SettingsStore<TestConfig>("test.json");
        var loadedField = typeof(SettingsStore<TestConfig>).GetField("_loaded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        loadedField!.SetValue(store, isLoaded);
        var disposedField = typeof(SettingsStore<TestConfig>).GetField("_disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        disposedField!.SetValue(store, isDisposed);
        if (parameters != null)
        {
            var parametersField = typeof(SettingsStore<TestConfig>).GetField("_parameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
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
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
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
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
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
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
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
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
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
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
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
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
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
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
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
        var store = new SettingsStore<TestConfig>(tempFilePath);
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
            // Cleanup
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
        var store = new SettingsStore<TestConfig>(tempFilePath);
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
            // Cleanup
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
        var store = new SettingsStore<TestConfig>(tempFilePath);
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
            // Cleanup
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
        var store = new SettingsStore<TestConfig>(tempFilePath);
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
            // Cleanup
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    #region Test Configuration Classes
    [UmbraAutoRegisterSettings]
    public partial record TestConfig
    {
        [UmbraSettingsParameter]
        public Parameter<int> Value1 { get; set; } = new(10);

        [UmbraSettingsParameter]
        public Parameter<string> Value2 { get; set; } = new("default");
    }

    #endregion
    /// <summary>
    /// Verifies that <see cref = "SettingsStore{TConfig}.IsDisposed"/> returns <see langword="false"/>
    /// immediately after construction before any disposal has occurred.
    /// </summary>
    [TestMethod]
    public void IsDisposed_AfterConstruction_ReturnsFalse()
    {
        // Arrange & Act
        var store = new SettingsStore<TestConfig>("test.json");
        // Assert
        Assert.IsFalse(store.IsDisposed);
    }

    /// <summary>
    /// Verifies that <see cref = "SettingsStore{TConfig}.IsDisposed"/> returns <see langword="true"/>
    /// after the <see cref = "SettingsStore{TConfig}.Dispose"/> method has been called.
    /// </summary>
    [TestMethod]
    public void IsDisposed_AfterDispose_ReturnsTrue()
    {
        // Arrange
        var store = new SettingsStore<TestConfig>("test.json");
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
            var store = new SettingsStore<TestConfig>(tempPath);
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
    /// Tests that <see cref = "SettingsStore{TConfig}.IsLoaded"/> returns false immediately after construction
    /// and before <see cref = "SettingsStore{TConfig}.Load"/> has been called.
    /// </summary>
    [TestMethod]
    public void IsLoaded_AfterConstruction_ReturnsFalse()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new SettingsStore<TestConfig>(tempPath);
        // Act
        var result = store.IsLoaded;
        // Assert
        Assert.IsFalse(result);
        // Cleanup
        store.Dispose();
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Tests that <see cref = "SettingsStore{TConfig}.IsLoaded"/> returns true after
    /// <see cref = "SettingsStore{TConfig}.Load"/> has been successfully called.
    /// </summary>
    [TestMethod]
    public void IsLoaded_AfterLoadCompletes_ReturnsTrue()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new SettingsStore<TestConfig>(tempPath);
        // Act
        _ = store.Load();
        var result = store.IsLoaded;
        // Assert
        Assert.IsTrue(result);
        // Cleanup
        store.Dispose();
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Tests that <see cref = "SettingsStore{TConfig}.IsLoaded"/> remains true after
    /// <see cref = "SettingsStore{TConfig}.Dispose"/> has been called on a loaded store.
    /// </summary>
    [TestMethod]
    public void IsLoaded_AfterDispose_RemainsTrue()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new SettingsStore<TestConfig>(tempPath);
        _ = store.Load();
        // Act
        store.Dispose();
        var result = store.IsLoaded;
        // Assert
        Assert.IsTrue(result);
        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Tests that <see cref = "SettingsStore{TConfig}.IsLoaded"/> remains false after
    /// <see cref = "SettingsStore{TConfig}.Dispose"/> has been called on a store that was never loaded.
    /// </summary>
    [TestMethod]
    public void IsLoaded_AfterDisposeWithoutLoad_RemainsFalse()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new SettingsStore<TestConfig>(tempPath);
        // Act
        store.Dispose();
        var result = store.IsLoaded;
        // Assert
        Assert.IsFalse(result);
        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Tests that <see cref = "SettingsStore{TConfig}.IsLoaded"/> returns true when a valid persisted
    /// configuration file exists and <see cref = "SettingsStore{TConfig}.Load"/> successfully loads values from it.
    /// </summary>
    [TestMethod]
    public void IsLoaded_AfterLoadingExistingFile_ReturnsTrue()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var initialStore = new SettingsStore<TestConfig>(tempPath);
        _ = initialStore.Load();
        initialStore.Save();
        initialStore.Dispose();
        var store = new SettingsStore<TestConfig>(tempPath);
        // Act
        _ = store.Load();
        var result = store.IsLoaded;
        // Assert
        Assert.IsTrue(result);
        // Cleanup
        store.Dispose();
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Tests that <see cref = "SettingsStore{TConfig}.IsLoaded"/> is not affected by multiple reads
    /// and consistently returns true after <see cref = "SettingsStore{TConfig}.Load"/> completes.
    /// </summary>
    [TestMethod]
    public void IsLoaded_MultipleReads_ConsistentlyReturnsTrue()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new SettingsStore<TestConfig>(tempPath);
        _ = store.Load();
        // Act & Assert - verify multiple reads return the same value
        for (var i = 0; i < 100; i++)
        {
            Assert.IsTrue(store.IsLoaded);
        }

        // Cleanup
        store.Dispose();
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    /// <summary>
    /// Test configuration class with different parameters for overlap testing.
    /// </summary>
    [UmbraAutoRegisterSettings]
    private record DifferentConfig
    {
        [UmbraSettingsParameter]
        public Parameter<int> IntValue { get; set; } = new(0);

        [UmbraSettingsParameter]
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
    [DataRow("C:\\Users\\User Name\\AppData\\Local\\Config Files\\settings.json", "Path with spaces")]
    public void Constructor_ValidFilePath_CreatesInstance(string filePath, string testCase)
    {
        // Act
        var store = new SettingsStore<TestConfig>(filePath);
        // Assert
        Assert.IsNotNull(store, $"Failed for test case: {testCase}");
        Assert.IsFalse(store.IsLoaded, $"Store should not be loaded immediately after construction. Failed for test case: {testCase}");
        Assert.IsFalse(store.IsDisposed, $"Store should not be disposed immediately after construction. Failed for test case: {testCase}");
    }

    /// <summary>
    /// Verifies that the constructor rejects whitespace-only file paths.
    /// </summary>
    [TestMethod]
    public void Constructor_WhitespaceFilePath_ThrowsArgumentException()
    {
        var exception = AssertThrows<ArgumentException>(() => new SettingsStore<TestConfig>("   "));

        Assert.AreEqual("filePath", exception.ParamName);
    }

    /// <summary>
    /// Verifies that an action throws the expected exception type and returns the captured exception.
    /// </summary>
    private static TException AssertThrows<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        Assert.Fail($"Expected exception of type {typeof(TException).Name}.");
        throw new InvalidOperationException("Unreachable");
    }

    /// <summary>
    /// Verifies that <see cref="SettingsStore{TConfig}.Load"/> can only be called once per store instance.
    /// </summary>
    [TestMethod]
    public void Load_CalledTwice_ThrowsInvalidOperationException()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new SettingsStore<TestConfig>(tempPath);
            _ = store.Load();

            AssertThrows<InvalidOperationException>(() => store.Load());
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="SettingsStore{TConfig}.Load"/> leaves the store unloaded when registration fails.
    /// </summary>
    [TestMethod]
    public void Load_WhenRegistrationThrowsDuplicateKey_LeavesStoreUnloaded()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new SettingsStore<DuplicateKeyConfig>(tempPath);

            AssertThrows<InvalidOperationException>(() => store.Load());
            Assert.IsFalse(store.IsLoaded);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="SettingsStore{TConfig}.Save"/> requires a successful load first.
    /// </summary>
    [TestMethod]
    public void Save_BeforeLoad_ThrowsInvalidOperationException()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new SettingsStore<TestConfig>(tempPath);

            AssertThrows<InvalidOperationException>(() => store.Save());
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="SettingsStore{TConfig}.CopyValuesTo"/> rejects a null target.
    /// </summary>
    [TestMethod]
    public void CopyValuesTo_NullTarget_ThrowsArgumentNullException()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new SettingsStore<TestConfig>(tempPath);
            _ = store.Load();

            AssertThrows<ArgumentNullException>(() => store.CopyValuesTo(null!));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Verifies that <see cref="SettingsStore{TConfig}.CopyValuesTo"/> requires a target store that has already loaded.
    /// </summary>
    [TestMethod]
    public void CopyValuesTo_TargetNotLoaded_ThrowsInvalidOperationException()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"source_{Guid.NewGuid()}.json");
        var targetPath = Path.Combine(Path.GetTempPath(), $"target_{Guid.NewGuid()}.json");
        try
        {
            var source = new SettingsStore<TestConfig>(sourcePath);
            var target = new SettingsStore<TestConfig>(targetPath);
            _ = source.Load();

            AssertThrows<InvalidOperationException>(() => source.CopyValuesTo(target));
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
    /// Verifies that silent copy updates target values without raising target listeners.
    /// </summary>
    [TestMethod]
    public void CopyValuesTo_SetWithoutNotifying_DoesNotRaiseTargetListeners()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"source_{Guid.NewGuid()}.json");
        var targetPath = Path.Combine(Path.GetTempPath(), $"target_{Guid.NewGuid()}.json");
        try
        {
            var source = new SettingsStore<TestConfig>(sourcePath);
            var target = new SettingsStore<TestConfig>(targetPath);
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
    /// Verifies that <see cref="SettingsStore{TConfig}.ResetAll"/> skips delegate-backed parameters.
    /// </summary>
    [TestMethod]
    public void ResetAll_WithDelegateParameter_SkipsDelegateParameter()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new SettingsStore<TestConfigWithDelegates>(tempPath);
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

    [UmbraAutoRegisterSettings]
    private sealed class DuplicateKeyConfig
    {
        [UmbraSettingsParameter("duplicate")]
        public Parameter<int> Value1 { get; set; } = new(1);

        [UmbraSettingsParameter("duplicate")]
        public Parameter<int> Value2 { get; set; } = new(2);
    }

}

/// <summary>
/// Unit tests for <see cref = "SettingsStore{TConfig}.RemoveListenerFromAll(Action)"/>.
/// </summary>
[TestClass]
public partial class SettingsStoreTests_RemoveListenerFromAll
{
    /// <summary>
    /// Test configuration class with a single parameter for testing.
    /// </summary>
    [UmbraAutoRegisterSettings]
    internal sealed class TestConfig
    {
        [UmbraSettingsParameter]
        public Parameter<int> TestValue { get; set; } = new(42);
    }

    /// <summary>
    /// Test configuration class with no parameters for edge case testing.
    /// </summary>
    [UmbraAutoRegisterSettings]
    internal sealed class EmptyConfig
    {
    }

    /// <summary>
    /// Test configuration class with multiple parameters.
    /// </summary>
    [UmbraAutoRegisterSettings]
    internal sealed class MultiParameterConfig
    {
        [UmbraSettingsParameter]
        public Parameter<int> Value1 { get; set; } = new(1);

        [UmbraSettingsParameter]
        public Parameter<string> Value2 { get; set; } = new("test");

        [UmbraSettingsParameter]
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
        var store = new SettingsStore<TestConfig>(tempPath);
        var config = store.Load();
        var callCount = 0;
        void listener() => callCount++;
        try
        {
            store.AddListenerToAll(listener);
            // Act - Remove the listener
            store.RemoveListenerFromAll(listener);
            // Change a parameter value to verify listener is not called
            config.TestValue.Set(100);
            // Assert - Listener should not have been called
            Assert.AreEqual(0, callCount, "Listener should not be invoked after removal.");
        }
        finally
        {
            // Cleanup
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
        var store = new SettingsStore<TestConfig>(tempPath);
        var config = store.Load();
        var callCount = 0;
        void listener() => callCount++;
        try
        {
            store.AddListenerToAll(listener);
            store.AddListenerToAll(listener);
            // Act - Remove listener once
            store.RemoveListenerFromAll(listener);
            // Change a parameter value
            config.TestValue.Set(200);
            // Assert - Listener should still be invoked once (second registration remains)
            Assert.AreEqual(1, callCount, "Listener should be invoked once after removing one registration.");
        }
        finally
        {
            // Cleanup
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
        var store = new SettingsStore<TestConfig>(tempPath);
        store.Load();
        static void listener()
        {
            // No-op for testing
        }
        try
        {
            // Act - Remove a listener that was never added
            store.RemoveListenerFromAll(listener);
        }
        finally
        {
            // Cleanup
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
        var store = new SettingsStore<EmptyConfig>(tempPath);
        store.Load();
        static void listener()
        {
            // No-op for testing
        }
        try
        {
            store.AddListenerToAll(listener);
            // Act
            store.RemoveListenerFromAll(listener);
        }
        finally
        {
            // Cleanup
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
        var store = new SettingsStore<MultiParameterConfig>(tempPath);
        var config = store.Load();
        var callCount = 0;
        void listener() => callCount++;
        try
        {
            store.AddListenerToAll(listener);
            // Act - Remove the listener
            store.RemoveListenerFromAll(listener);
            // Change multiple parameter values
            config.Value1.Set(999);
            config.Value2.Set("changed");
            config.Value3.Set(false);
            // Assert - Listener should not have been called for any parameter
            Assert.AreEqual(0, callCount, "Listener should not be invoked after removal from all parameters.");
        }
        finally
        {
            // Cleanup
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
        var store = new SettingsStore<TestConfig>(tempPath);
        var config = store.Load();
        static void listener()
        {
            // No-op for testing
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
            // Cleanup
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
        var store = new SettingsStore<TestConfig>(tempPath);
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
            // Change a parameter value
            config.TestValue.Set(300);
            // Assert - Only listener2 should be invoked
            Assert.AreEqual(0, callCount1, "Listener1 should not be invoked after removal.");
            Assert.AreEqual(1, callCount2, "Listener2 should still be invoked.");
        }
        finally
        {
            // Cleanup
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
        var store = new SettingsStore<TestConfig>(configPath);
        var config = store.Load();
        static void listener(double oldVal, double newVal)
        {
            // No-op for testing
        }
        // Act & Assert (should not throw)
        store.RemoveListenerFromAll((Action<double, double>)listener);
        // Cleanup
        store.Dispose();
        if (System.IO.File.Exists(configPath))
            System.IO.File.Delete(configPath);
    }

}
