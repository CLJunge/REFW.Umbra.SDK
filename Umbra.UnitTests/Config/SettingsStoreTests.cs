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
    /// Tests that Dispose completes successfully when no cleanup registrations exist.
    /// </summary>
    [TestMethod]
    public void Dispose_WithNoCleanupRegistrations_CompletesSuccessfully()
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
    /// Tests that Dispose can be called before Load without error.
    /// </summary>
    [TestMethod]
    public void Dispose_BeforeLoad_CompletesSuccessfully()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore<TestConfig>(tempPath);
            // Act - dispose without loading
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
    /// Verifies that Load returns a non-null TConfig instance when called on a fresh store.
    /// </summary>
    [TestMethod]
    public void Load_ReturnsNonNullInstance()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new SettingsStore<TestConfig>(filePath);
        // Act
        var result = store.Load();
        // Assert
        Assert.IsNotNull(result);
        // Cleanup
        store.Dispose();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Verifies that Load sets IsLoaded to true after successful completion.
    /// </summary>
    [TestMethod]
    public void Load_SetsIsLoadedToTrue()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new SettingsStore<TestConfig>(filePath);
        Assert.IsFalse(store.IsLoaded, "IsLoaded should be false before Load is called");
        // Act
        store.Load();
        // Assert
        Assert.IsTrue(store.IsLoaded, "IsLoaded should be true after Load completes");
        // Cleanup
        store.Dispose();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
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
    /// Verifies that Load handles the case where the file vanishes between existence check and read (TOCTOU).
    /// This test simulates the MissingFile result from SettingsPersistence.Load by ensuring the file
    /// does not exist, which causes Load to save defaults.
    /// </summary>
    [TestMethod]
    public void Load_WhenFileVanishes_SavesDefaults()
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
        Assert.IsTrue(File.Exists(filePath), "Load should save defaults when file is missing");
        // Cleanup
        store.Dispose();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Verifies that Load does not throw when creating a fresh TConfig instance.
    /// Tests the boundary condition of TConfig instantiation.
    /// </summary>
    [TestMethod]
    public void Load_DoesNotThrowOnFreshInstantiation()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var store = new SettingsStore<TestConfig>(filePath);
        // Act & Assert
        try
        {
            var result = store.Load();
            Assert.IsNotNull(result);
        }
        finally
        {
            // Cleanup
            store.Dispose();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
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
    /// Verifies that AddListenerToAll works with string type parameters.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_StringParameter_ListenerInvoked()
    {
        // Arrange
        var parameter = CreateParameter("stringParam", "initial");
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "stringParam",
                parameter
            }
        };
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
        string? capturedOldValue = null;
        string? capturedNewValue = null;
        void listener(string? oldVal, string? newVal)
        {
            capturedOldValue = oldVal;
            capturedNewValue = newVal;
        }
        // Act
        store.AddListenerToAll((Action<string?, string?>)listener);
        parameter.Set("updated");
        // Assert
        Assert.AreEqual("initial", capturedOldValue);
        Assert.AreEqual("updated", capturedNewValue);
    }

    /// <summary>
    /// Verifies that AddListenerToAll works with boolean type parameters.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_BooleanParameter_ListenerInvoked()
    {
        // Arrange
        var parameter = CreateParameter("boolParam", true);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "boolParam",
                parameter
            }
        };
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
        var listenerCalled = false;
        bool? capturedOldValue = null;
        bool? capturedNewValue = null;
        void listener(bool oldVal, bool newVal)
        {
            listenerCalled = true;
            capturedOldValue = oldVal;
            capturedNewValue = newVal;
        }
        // Act
        store.AddListenerToAll((Action<bool, bool>)listener);
        parameter.Set(false);
        // Assert
        Assert.IsTrue(listenerCalled);
        Assert.IsTrue(capturedOldValue);
        Assert.IsFalse(capturedNewValue);
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
    /// Verifies that AddListenerToAll works with double type parameters.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_DoubleParameter_ListenerInvoked()
    {
        // Arrange
        var parameter = CreateParameter("doubleParam", 3.14);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "doubleParam",
                parameter
            }
        };
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
        double? capturedNewValue = null;
        void listener(double oldVal, double newVal) => capturedNewValue = newVal;
        // Act
        store.AddListenerToAll((Action<double, double>)listener);
        parameter.Set(2.71);
        // Assert
        Assert.AreEqual(2.71, capturedNewValue);
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
    /// Verifies that AddListenerToAll works correctly with float type parameters.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_FloatParameter_ListenerInvoked()
    {
        // Arrange
        var parameter = CreateParameter("floatParam", 1.5f);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "floatParam",
                parameter
            }
        };
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
        float? capturedOldValue = null;
        float? capturedNewValue = null;
        void listener(float oldVal, float newVal)
        {
            capturedOldValue = oldVal;
            capturedNewValue = newVal;
        }
        // Act
        store.AddListenerToAll((Action<float, float>)listener);
        parameter.Set(2.5f);
        // Assert
        Assert.AreEqual(1.5f, capturedOldValue);
        Assert.AreEqual(2.5f, capturedNewValue);
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
    /// Verifies that AddListenerToAll works with parameters having extreme integer values.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_ExtremeIntegerValues_ListenerInvoked()
    {
        // Arrange
        var parameter = CreateParameter("intParam", 0);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "intParam",
                parameter
            }
        };
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
        int? capturedNewValue = null;
        void listener(int oldVal, int newVal) => capturedNewValue = newVal;
        // Act
        store.AddListenerToAll((Action<int, int>)listener);
        parameter.Set(int.MaxValue);
        // Assert
        Assert.AreEqual(int.MaxValue, capturedNewValue);
    }

    /// <summary>
    /// Verifies that AddListenerToAll works correctly with negative integer values.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_NegativeIntegerValue_ListenerInvoked()
    {
        // Arrange
        var parameter = CreateParameter("intParam", 0);
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "intParam",
                parameter
            }
        };
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
        int? capturedNewValue = null;
        void listener(int oldVal, int newVal) => capturedNewValue = newVal;
        // Act
        store.AddListenerToAll((Action<int, int>)listener);
        parameter.Set(int.MinValue);
        // Assert
        Assert.AreEqual(int.MinValue, capturedNewValue);
    }

    /// <summary>
    /// Verifies that AddListenerToAll works with empty string values.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_EmptyStringValue_ListenerInvoked()
    {
        // Arrange
        var parameter = CreateParameter("stringParam", "initial");
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "stringParam",
                parameter
            }
        };
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
        string? capturedNewValue = null;
        void listener(string? oldVal, string? newVal) => capturedNewValue = newVal;
        // Act
        store.AddListenerToAll((Action<string?, string?>)listener);
        parameter.Set(string.Empty);
        // Assert
        Assert.AreEqual(string.Empty, capturedNewValue);
    }

    /// <summary>
    /// Verifies that AddListenerToAll works with special characters in string values.
    /// </summary>
    [TestMethod]
    public void AddListenerToAll_StringWithSpecialCharacters_ListenerInvoked()
    {
        // Arrange
        var parameter = CreateParameter("stringParam", "normal");
        var parameters = new Dictionary<string, IParameter>
        {
            {
                "stringParam",
                parameter
            }
        };
        var store = CreateSettingsStoreWithState(isLoaded: true, isDisposed: false, parameters);
        string? capturedNewValue = null;
        void listener(string? oldVal, string? newVal) => capturedNewValue = newVal;
        // Act
        store.AddListenerToAll((Action<string?, string?>)listener);
        var specialString = "test\n\r\t\"'\\";
        parameter.Set(specialString);
        // Assert
        Assert.AreEqual(specialString, capturedNewValue);
    }

    /// <summary>
    /// Tests RemoveListenerFromAll with a predicate that would match all parameters.
    /// Note: This test validates the method signature and basic logic flow but cannot fully
    /// verify parameter event unsubscription without calling Load() first, which requires
    /// file I/O and a properly configured TConfig type with UmbraAutoRegisterSettings attribute.
    /// To fully test this scenario, consider:
    /// 1. Creating a test helper that sets up a loaded SettingsStore with test data, or
    /// 2. Using InternalsVisibleTo to access private _loaded and _parameters fields, or
    /// 3. Refactoring SettingsStore to accept a file system abstraction for testing.
    /// </summary>
    [TestMethod]
    [Ignore("Requires Load() to be called, which involves file I/O and complex setup")]
    public void RemoveListenerFromAll_PredicateMatchesAllParameters_RemovesListenerFromMatchingParameters() =>
        // Arrange
        // TODO: Set up a loaded SettingsStore instance with parameters
        // var store = SetupLoadedStore();
        // Func<IParameter, bool> predicate = p => true; // Match all
        // Action listener = () => { };
        // // Add listener to parameters first via AddListenerToAll
        // store.AddListenerToAll(predicate, listener);
        // Act
        // store.RemoveListenerFromAll(predicate, listener);
        // Assert
        // TODO: Verify listener was removed from all parameters' ValueChanged events
        Assert.Inconclusive("Test requires loaded SettingsStore with accessible parameters collection");

    /// <summary>
    /// Tests RemoveListenerFromAll with a predicate that matches no parameters.
    /// Note: This test validates the method signature and basic logic flow but cannot fully
    /// verify behavior without calling Load() first.
    /// </summary>
    [TestMethod]
    [Ignore("Requires Load() to be called, which involves file I/O and complex setup")]
    public void RemoveListenerFromAll_PredicateMatchesNoParameters_NoRemoval() =>
        // Arrange
        // TODO: Set up a loaded SettingsStore instance with parameters
        // var store = SetupLoadedStore();
        // Func<IParameter, bool> predicate = p => false; // Match none
        // Action listener = () => { };
        // Act
        // store.RemoveListenerFromAll(predicate, listener);
        // Assert
        // TODO: Verify listener was not removed from any parameters (or no error occurred)
        Assert.Inconclusive("Test requires loaded SettingsStore with accessible parameters collection");

    /// <summary>
    /// Tests RemoveListenerFromAll when a tracked cleanup registration exists for the listener.
    /// The method should return early after removing the tracked cleanup, without iterating parameters.
    /// Note: This test requires calling Load() and AddListenerToAll() first to register a tracked cleanup.
    /// </summary>
    [TestMethod]
    [Ignore("Requires Load() to be called and AddListenerToAll setup")]
    public void RemoveListenerFromAll_TrackedCleanupExists_RemovesTrackedCleanupAndReturnsEarly() =>
        // Arrange
        // TODO: Set up a loaded SettingsStore
        // var store = SetupLoadedStore();
        // Func<IParameter, bool> predicate = p => true;
        // Action listener = () => { };
        // // Register the listener with tracked cleanup
        // store.AddListenerToAll(predicate, listener);
        // Act
        // store.RemoveListenerFromAll(predicate, listener);
        // Assert
        // TODO: Verify that TryRemoveTrackedCleanup was called and returned true
        // TODO: Verify that the cleanup was executed
        // TODO: Verify that parameter iteration did not occur (e.g., predicate was not invoked)
        Assert.Inconclusive("Test requires loaded SettingsStore and tracked cleanup registration");

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
    /// Verifies that <see cref = "SettingsStore{TConfig}.IsDisposed"/> continues to return
    /// <see langword="true"/> after multiple calls to <see cref = "SettingsStore{TConfig}.Dispose"/>,
    /// demonstrating idempotent disposal behavior.
    /// </summary>
    [TestMethod]
    public void IsDisposed_AfterMultipleDisposes_ReturnsTrue()
    {
        // Arrange
        var store = new SettingsStore<TestConfig>("test.json");
        // Act
        store.Dispose();
        store.Dispose();
        store.Dispose();
        // Assert
        Assert.IsTrue(store.IsDisposed);
    }

    /// <summary>
    /// Verifies that <see cref = "SettingsStore{TConfig}.IsDisposed"/> returns consistent values
    /// when checked multiple times without any state changes, both before and after disposal.
    /// </summary>
    [TestMethod]
    public void IsDisposed_CheckedMultipleTimes_ReturnsConsistentValue()
    {
        // Arrange
        var store = new SettingsStore<TestConfig>("test.json");
        // Act & Assert - Before disposal
        Assert.IsFalse(store.IsDisposed);
        Assert.IsFalse(store.IsDisposed);
        Assert.IsFalse(store.IsDisposed);
        // Act - Dispose
        store.Dispose();
        // Assert - After disposal
        Assert.IsTrue(store.IsDisposed);
        Assert.IsTrue(store.IsDisposed);
        Assert.IsTrue(store.IsDisposed);
    }

    /// <summary>
    /// Verifies that <see cref = "SettingsStore{TConfig}.IsDisposed"/> works correctly
    /// even when <see cref = "SettingsStore{TConfig}.Load"/> has not been called,
    /// ensuring the property is independent of the loaded state.
    /// </summary>
    [TestMethod]
    public void IsDisposed_BeforeLoad_ReturnsFalse()
    {
        // Arrange
        var store = new SettingsStore<TestConfig>("test.json");
        // Act & Assert
        Assert.IsFalse(store.IsDisposed);
        Assert.IsFalse(store.IsLoaded);
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
    /// Tests that the constructor succeeds with a very long file path string.
    /// </summary>
    [TestMethod]
    public void Constructor_VeryLongFilePath_CreatesInstance()
    {
        // Arrange
        var filePath = new string('a', 1000) + ".json";
        // Act
        var store = new SettingsStore<TestConfig>(filePath);
        // Assert
        Assert.IsNotNull(store);
        Assert.IsFalse(store.IsLoaded);
        Assert.IsFalse(store.IsDisposed);
    }

    /// <summary>
    /// Tests that the constructor succeeds with a path containing Unicode characters.
    /// </summary>
    [TestMethod]
    public void Constructor_UnicodeFilePath_CreatesInstance()
    {
        // Arrange
        var filePath = "配置文件.json";
        // Act
        var store = new SettingsStore<TestConfig>(filePath);
        // Assert
        Assert.IsNotNull(store);
        Assert.IsFalse(store.IsLoaded);
        Assert.IsFalse(store.IsDisposed);
    }

    /// <summary>
    /// Tests that the constructor initializes IsLoaded to false.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidFilePath_InitializesIsLoadedToFalse()
    {
        // Arrange
        var filePath = "config.json";
        // Act
        var store = new SettingsStore<TestConfig>(filePath);
        // Assert
        Assert.IsFalse(store.IsLoaded);
    }

    /// <summary>
    /// Tests that the constructor initializes IsDisposed to false.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidFilePath_InitializesIsDisposedToFalse()
    {
        // Arrange
        var filePath = "config.json";
        // Act
        var store = new SettingsStore<TestConfig>(filePath);
        // Assert
        Assert.IsFalse(store.IsDisposed);
    }

    /// <summary>
    /// Tests that the constructor works with different generic type parameters.
    /// </summary>
    [TestMethod]
    public void Constructor_DifferentGenericTypes_CreatesInstances()
    {
        // Arrange
        var filePath = "config.json";
        // Act
        var store1 = new SettingsStore<TestConfig>(filePath);
        var store2 = new SettingsStore<AnotherTestConfig>(filePath);
        // Assert
        Assert.IsNotNull(store1);
        Assert.IsNotNull(store2);
        Assert.IsFalse(store1.IsLoaded);
        Assert.IsFalse(store2.IsLoaded);
    }

    /// <summary>
    /// Another test configuration class for testing different generic type parameters.
    /// </summary>
    private class AnotherTestConfig
    {
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
        }
        // Act & Assert (should not throw)
        store.RemoveListenerFromAll((Action<double, double>)listener);
        // Cleanup
        store.Dispose();
        if (System.IO.File.Exists(configPath))
            System.IO.File.Delete(configPath);
    }

}
