using System.Text.Json;
using Moq;

namespace Umbra.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="SettingsPersistence"/> class.
/// </summary>
[TestClass]
public class SettingsPersistenceTests
{
    private string _testDirectory = string.Empty;

    private enum TestEnum
    {
        First,
        Second
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "SettingsPersistenceTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Tests that Load returns Success when the settings file exists and contains valid JSON.
    /// </summary>
    [TestMethod]
    public void Load_ValidJsonFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""testKey"": ""testValue""}";
        File.WriteAllText(filePath, json);

        var parameterMock = new Mock<IParameter>();
        parameterMock.SetupGet(p => p.ValueType).Returns(typeof(string));
        var parameters = new Dictionary<string, IParameter>
        {
            ["testKey"] = parameterMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        parameterMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that Load returns Success when JSON deserialization results in null dictionary.
    /// </summary>
    [TestMethod]
    public void Load_JsonDeserializesToNull_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = "null";
        File.WriteAllText(filePath, json);

        var parameters = new Dictionary<string, IParameter>();

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
    }

    /// <summary>
    /// Tests that Load applies only values that have matching keys in the parameters dictionary.
    /// </summary>
    [TestMethod]
    public void Load_PartialMatchingKeys_AppliesOnlyMatchingParameters()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key1"": 42, ""key2"": ""value2"", ""key3"": true}";
        File.WriteAllText(filePath, json);

        var param1Mock = new Mock<IParameter>();
        param1Mock.SetupGet(p => p.ValueType).Returns(typeof(int));

        var param3Mock = new Mock<IParameter>();
        param3Mock.SetupGet(p => p.ValueType).Returns(typeof(bool));

        var parameters = new Dictionary<string, IParameter>
        {
            ["key1"] = param1Mock.Object,
            ["key3"] = param3Mock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        param1Mock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
        param3Mock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that Load returns Success when no keys in JSON match the parameters dictionary.
    /// </summary>
    [TestMethod]
    public void Load_NoMatchingKeys_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""unmatchedKey"": ""value""}";
        File.WriteAllText(filePath, json);

        var parameterMock = new Mock<IParameter>();
        parameterMock.SetupGet(p => p.ValueType).Returns(typeof(string));
        var parameters = new Dictionary<string, IParameter>
        {
            ["differentKey"] = parameterMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        parameterMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Never);
    }

    /// <summary>
    /// Tests that Load returns MissingFile when the specified file does not exist.
    /// </summary>
    [TestMethod]
    public void Load_FileNotFound_ReturnsMissingFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.json");
        var parameters = new Dictionary<string, IParameter>();

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.MissingFile, result);
    }

    /// <summary>
    /// Tests that Load returns Success for an empty JSON object and applies no values.
    /// </summary>
    [TestMethod]
    public void Load_EmptyJsonObject_ReturnsSuccess()
    {
        var filePath = Path.Combine(_testDirectory, "settings.json");
        File.WriteAllText(filePath, "{}");

        var parameterMock = new Mock<IParameter>();
        parameterMock.SetupGet(p => p.ValueType).Returns(typeof(string));
        var parameters = new Dictionary<string, IParameter>
        {
            ["key"] = parameterMock.Object
        };

        var result = SettingsPersistence.Load(filePath, parameters);

        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        parameterMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object?>()), Times.Never);
    }

    /// <summary>
    /// Tests that Load returns RecoveredToDefaults or Failed when the JSON file is malformed.
    /// The exact result depends on whether the backup succeeds.
    /// </summary>
    [TestMethod]
    public void Load_MalformedJson_ReturnsRecoveredToDefaultsOrFailed()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var invalidJson = @"{""key"": invalid}";
        File.WriteAllText(filePath, invalidJson);

        var parameters = new Dictionary<string, IParameter>();

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.IsTrue(
            result is SettingsPersistence.LoadResult.RecoveredToDefaults or
            SettingsPersistence.LoadResult.Failed,
            "Expected RecoveredToDefaults or Failed for malformed JSON");
    }

    /// <summary>
    /// Tests that Load handles JSON with nested objects correctly.
    /// </summary>
    [TestMethod]
    public void Load_NestedJsonObject_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key1"": {""nested"": ""value""}}";
        File.WriteAllText(filePath, json);

        var parameterMock = new Mock<IParameter>();
        parameterMock.SetupGet(p => p.ValueType).Returns(typeof(object));
        var parameters = new Dictionary<string, IParameter>
        {
            ["key1"] = parameterMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
    }

    /// <summary>
    /// Tests that Load handles JSON with null values correctly.
    /// </summary>
    [TestMethod]
    public void Load_JsonWithNullValue_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key1"": null}";
        File.WriteAllText(filePath, json);

        var parameterMock = new Mock<IParameter>();
        parameterMock.SetupGet(p => p.ValueType).Returns(typeof(string));
        var parameters = new Dictionary<string, IParameter>
        {
            ["key1"] = parameterMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        parameterMock.Verify(p => p.SetValueWithoutNotify(null), Times.Once);
    }

    /// <summary>
    /// Tests that Load handles JSON with numeric values correctly.
    /// </summary>
    [TestMethod]
    public void Load_JsonWithNumericValue_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""intKey"": 42, ""floatKey"": 3.14, ""doubleKey"": 2.718}";
        File.WriteAllText(filePath, json);

        var intParamMock = new Mock<IParameter>();
        intParamMock.SetupGet(p => p.ValueType).Returns(typeof(int));

        var floatParamMock = new Mock<IParameter>();
        floatParamMock.SetupGet(p => p.ValueType).Returns(typeof(float));

        var doubleParamMock = new Mock<IParameter>();
        doubleParamMock.SetupGet(p => p.ValueType).Returns(typeof(double));

        var parameters = new Dictionary<string, IParameter>
        {
            ["intKey"] = intParamMock.Object,
            ["floatKey"] = floatParamMock.Object,
            ["doubleKey"] = doubleParamMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        intParamMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
        floatParamMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
        doubleParamMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that Load handles JSON with boolean values correctly.
    /// </summary>
    [TestMethod]
    public void Load_JsonWithBooleanValue_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""boolKey1"": true, ""boolKey2"": false}";
        File.WriteAllText(filePath, json);

        var boolParam1Mock = new Mock<IParameter>();
        boolParam1Mock.SetupGet(p => p.ValueType).Returns(typeof(bool));

        var boolParam2Mock = new Mock<IParameter>();
        boolParam2Mock.SetupGet(p => p.ValueType).Returns(typeof(bool));

        var parameters = new Dictionary<string, IParameter>
        {
            ["boolKey1"] = boolParam1Mock.Object,
            ["boolKey2"] = boolParam2Mock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        boolParam1Mock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
        boolParam2Mock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that Load handles special characters in JSON string values.
    /// </summary>
    [TestMethod]
    public void Load_JsonWithSpecialCharacters_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key1"": ""value with \""quotes\"" and \\ backslash""}";
        File.WriteAllText(filePath, json);

        var parameterMock = new Mock<IParameter>();
        parameterMock.SetupGet(p => p.ValueType).Returns(typeof(string));
        var parameters = new Dictionary<string, IParameter>
        {
            ["key1"] = parameterMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        parameterMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that Load handles a file with BOM (Byte Order Mark) correctly.
    /// </summary>
    [TestMethod]
    public void Load_FileWithBOM_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key"": ""value""}";
        File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);

        var parameterMock = new Mock<IParameter>();
        parameterMock.SetupGet(p => p.ValueType).Returns(typeof(string));
        var parameters = new Dictionary<string, IParameter>
        {
            ["key"] = parameterMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        parameterMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that Load handles JSON with case variations in keys (camelCase policy).
    /// </summary>
    [TestMethod]
    public void Load_JsonWithCaseVariations_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""TestKey"": ""value1"", ""testkey"": ""value2"", ""TESTKEY"": ""value3""}";
        File.WriteAllText(filePath, json);

        var paramMock1 = new Mock<IParameter>();
        paramMock1.SetupGet(p => p.ValueType).Returns(typeof(string));

        var paramMock2 = new Mock<IParameter>();
        paramMock2.SetupGet(p => p.ValueType).Returns(typeof(string));

        var paramMock3 = new Mock<IParameter>();
        paramMock3.SetupGet(p => p.ValueType).Returns(typeof(string));

        var parameters = new Dictionary<string, IParameter>
        {
            ["TestKey"] = paramMock1.Object,
            ["testkey"] = paramMock2.Object,
            ["TESTKEY"] = paramMock3.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
    }

    /// <summary>
    /// Tests that Load handles a file path with forward slashes on Windows.
    /// </summary>
    [TestMethod]
    public void Load_FilePathWithForwardSlashes_HandlesCorrectly()
    {
        // Arrange
        var filePath = _testDirectory.Replace('\\', '/') + "/settings.json";
        var json = @"{""key"": ""value""}";

        var normalizedPath = Path.Combine(_testDirectory, "settings.json");
        File.WriteAllText(normalizedPath, json);

        var parameterMock = new Mock<IParameter>();
        parameterMock.SetupGet(p => p.ValueType).Returns(typeof(string));
        var parameters = new Dictionary<string, IParameter>
        {
            ["key"] = parameterMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
    }

    /// <summary>
    /// Tests that Load handles JSON with duplicate keys (last value wins).
    /// </summary>
    [TestMethod]
    public void Load_JsonWithDuplicateKeys_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key"": ""value1"", ""key"": ""value2""}";
        File.WriteAllText(filePath, json);

        var parameterMock = new Mock<IParameter>();
        parameterMock.SetupGet(p => p.ValueType).Returns(typeof(string));
        var parameters = new Dictionary<string, IParameter>
        {
            ["key"] = parameterMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        parameterMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.AtLeastOnce);
    }

    /// <summary>
    /// Tests that Load treats parameter-application failures as unreadable content and attempts recovery.
    /// </summary>
    [TestMethod]
    public void Load_WhenParameterAssignmentThrows_ReturnsRecoveredToDefaultsOrFailed()
    {
        var filePath = Path.Combine(_testDirectory, "settings.json");
        File.WriteAllText(filePath, @"{""key"": 42}");

        var parameterMock = new Mock<IParameter>();
        parameterMock.SetupGet(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.SetValueWithoutNotify(It.IsAny<object?>())).Throws(new InvalidOperationException("apply failed"));
        var parameters = new Dictionary<string, IParameter>
        {
            ["key"] = parameterMock.Object
        };

        var result = SettingsPersistence.Load(filePath, parameters);

        Assert.IsTrue(
            result is SettingsPersistence.LoadResult.RecoveredToDefaults or SettingsPersistence.LoadResult.Failed,
            "Expected recovery or failure when parameter assignment throws.");
    }

    /// <summary>
    /// Tests that Save successfully writes a single non-delegate parameter to disk.
    /// </summary>
    [TestMethod]
    public void Save_SingleParameter_WritesJsonFile()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var mockParam = new Mock<IParameter>();
            mockParam.Setup(p => p.Key).Returns("testKey");
            mockParam.Setup(p => p.ValueType).Returns(typeof(int));
            mockParam.Setup(p => p.GetValue()).Returns(42);

            var parameters = new Dictionary<string, IParameter>
            {
                ["testKey"] = mockParam.Object
            };

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            Assert.IsTrue(File.Exists(tempPath), "Settings file should be created");
            var jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.HasCount(1, deserialized);
            Assert.IsTrue(deserialized.ContainsKey("testKey"));
            Assert.AreEqual(42, deserialized["testKey"].GetInt32());
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Save correctly filters out delegate parameters and only persists non-delegate types.
    /// </summary>
    [TestMethod]
    public void Save_DelegateParameter_IsFilteredOut()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var mockDelegateParam = new Mock<IParameter>();
            mockDelegateParam.SetupGet(p => p.Key).Returns("delegateKey");
            mockDelegateParam.SetupGet(p => p.ValueType).Returns(typeof(Action));

            var mockNormalParam = new Mock<IParameter>();
            mockNormalParam.SetupGet(p => p.Key).Returns("normalKey");
            mockNormalParam.SetupGet(p => p.ValueType).Returns(typeof(string));
            mockNormalParam.Setup(p => p.GetValue()).Returns("value");

            var parameters = new Dictionary<string, IParameter>
            {
                ["delegateKey"] = mockDelegateParam.Object,
                ["normalKey"] = mockNormalParam.Object
            };

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            Assert.IsTrue(File.Exists(tempPath));
            var jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.HasCount(1, deserialized, "Only non-delegate parameter should be saved");
            Assert.IsFalse(deserialized.ContainsKey("delegateKey"), "Delegate parameter should be filtered");
            Assert.IsTrue(deserialized.ContainsKey("normalKey"), "Normal parameter should be saved");
            mockDelegateParam.Verify(p => p.GetValue(), Times.Never, "Delegate parameter value should not be read");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Save serializes enum values as strings.
    /// </summary>
    [TestMethod]
    public void Save_EnumParameter_WritesEnumNameAsString()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var mockParam = new Mock<IParameter>();
            mockParam.Setup(p => p.Key).Returns("enumKey");
            mockParam.Setup(p => p.ValueType).Returns(typeof(TestEnum));
            mockParam.Setup(p => p.GetValue()).Returns(TestEnum.Second);

            var parameters = new Dictionary<string, IParameter>
            {
                ["enumKey"] = mockParam.Object
            };

            SettingsPersistence.Save(tempPath, parameters);

            var jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual("Second", deserialized["enumKey"].GetString());
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Save successfully persists multiple parameters with various types.
    /// </summary>
    [TestMethod]
    public void Save_MultipleParameters_AllWrittenToFile()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var mockIntParam = new Mock<IParameter>();
            mockIntParam.Setup(p => p.Key).Returns("intKey");
            mockIntParam.Setup(p => p.ValueType).Returns(typeof(int));
            mockIntParam.Setup(p => p.GetValue()).Returns(100);

            var mockStringParam = new Mock<IParameter>();
            mockStringParam.Setup(p => p.Key).Returns("stringKey");
            mockStringParam.Setup(p => p.ValueType).Returns(typeof(string));
            mockStringParam.Setup(p => p.GetValue()).Returns("test");

            var mockBoolParam = new Mock<IParameter>();
            mockBoolParam.Setup(p => p.Key).Returns("boolKey");
            mockBoolParam.Setup(p => p.ValueType).Returns(typeof(bool));
            mockBoolParam.Setup(p => p.GetValue()).Returns(true);

            var parameters = new Dictionary<string, IParameter>
            {
                ["intKey"] = mockIntParam.Object,
                ["stringKey"] = mockStringParam.Object,
                ["boolKey"] = mockBoolParam.Object
            };

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            Assert.IsTrue(File.Exists(tempPath));
            var jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.HasCount(3, deserialized);
            Assert.AreEqual(100, deserialized["intKey"].GetInt32());
            Assert.AreEqual("test", deserialized["stringKey"].GetString());
            Assert.IsTrue(deserialized["boolKey"].GetBoolean());
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Save handles an empty parameters dictionary by creating an empty JSON file.
    /// </summary>
    [TestMethod]
    public void Save_EmptyParameters_CreatesEmptyJsonObject()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var parameters = new Dictionary<string, IParameter>();

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            Assert.IsTrue(File.Exists(tempPath));
            var jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.IsEmpty(deserialized);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Save handles parameters containing null values correctly.
    /// </summary>
    [TestMethod]
    public void Save_ParameterWithNullValue_WritesNullToJson()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var mockParam = new Mock<IParameter>();
            mockParam.Setup(p => p.Key).Returns("nullKey");
            mockParam.Setup(p => p.ValueType).Returns(typeof(string));
            mockParam.Setup(p => p.GetValue()).Returns((object?)null);

            var parameters = new Dictionary<string, IParameter>
            {
                ["nullKey"] = mockParam.Object
            };

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            Assert.IsTrue(File.Exists(tempPath));
            var jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.HasCount(1, deserialized);
            Assert.AreEqual(JsonValueKind.Null, deserialized["nullKey"].ValueKind);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Save creates the parent directory when it does not exist.
    /// </summary>
    [TestMethod]
    public void Save_NonExistentDirectory_CreatesDirectory()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"umbra_test_dir_{Guid.NewGuid()}");
        var tempPath = Path.Combine(tempDir, "settings.json");
        try
        {
            Assert.IsFalse(Directory.Exists(tempDir), "Directory should not exist initially");

            var mockParam = new Mock<IParameter>();
            mockParam.Setup(p => p.Key).Returns("key");
            mockParam.Setup(p => p.ValueType).Returns(typeof(int));
            mockParam.Setup(p => p.GetValue()).Returns(1);

            var parameters = new Dictionary<string, IParameter>
            {
                ["key"] = mockParam.Object
            };

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            Assert.IsTrue(Directory.Exists(tempDir), "Parent directory should be created");
            Assert.IsTrue(File.Exists(tempPath), "Settings file should be created");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    /// <summary>
    /// Tests that Save handles overwriting an existing file correctly.
    /// </summary>
    [TestMethod]
    public void Save_ExistingFile_OverwritesContent()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            File.WriteAllText(tempPath, "{\"oldKey\": \"oldValue\"}");

            var mockParam = new Mock<IParameter>();
            mockParam.Setup(p => p.Key).Returns("newKey");
            mockParam.Setup(p => p.ValueType).Returns(typeof(string));
            mockParam.Setup(p => p.GetValue()).Returns("newValue");

            var parameters = new Dictionary<string, IParameter>
            {
                ["newKey"] = mockParam.Object
            };

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            var jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.HasCount(1, deserialized);
            Assert.IsFalse(deserialized.ContainsKey("oldKey"));
            Assert.IsTrue(deserialized.ContainsKey("newKey"));
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Save does not throw when an exception occurs during file operations.
    /// The method catches all exceptions internally and logs them via Logger.Exception.
    /// </summary>
    /// <remarks>
    /// This test uses an invalid path to trigger an exception. The Save method catches
    /// exceptions internally, so this test verifies graceful failure handling.
    /// Note: Cannot verify Logger.Exception call since it's a static method that cannot be mocked.
    /// </remarks>
    [TestMethod]
    public void Save_InvalidPath_DoesNotThrow()
    {
        // Arrange
        var invalidPath = new string(Path.GetInvalidPathChars()[0], 5);
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.Key).Returns("key");
        mockParam.Setup(p => p.ValueType).Returns(typeof(int));
        mockParam.Setup(p => p.GetValue()).Returns(1);

        var parameters = new Dictionary<string, IParameter>
        {
            ["key"] = mockParam.Object
        };

        // Act & Assert
        // Should not throw - exception is caught internally
        SettingsPersistence.Save(invalidPath, parameters);
    }

    /// <summary>
    /// Tests that Save handles parameters containing complex objects correctly.
    /// </summary>
    [TestMethod]
    public void Save_ComplexObjectParameter_SerializedCorrectly()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var complexObject = new { Name = "Test", Value = 42, Nested = new { Flag = true } };

            var mockParam = new Mock<IParameter>();
            mockParam.Setup(p => p.Key).Returns("complexKey");
            mockParam.Setup(p => p.ValueType).Returns(complexObject.GetType());
            mockParam.Setup(p => p.GetValue()).Returns(complexObject);

            var parameters = new Dictionary<string, IParameter>
            {
                ["complexKey"] = mockParam.Object
            };

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            Assert.IsTrue(File.Exists(tempPath));
            var jsonContent = File.ReadAllText(tempPath);
            Assert.Contains("\"name\"", jsonContent, "JSON should contain property names in camelCase");
            Assert.Contains("\"value\"", jsonContent);
            Assert.Contains("\"nested\"", jsonContent);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Save swallows exceptions thrown while reading parameter values.
    /// </summary>
    [TestMethod]
    public void Save_WhenParameterGetValueThrows_DoesNotThrow()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var mockParam = new Mock<IParameter>();
            mockParam.Setup(p => p.Key).Returns("key");
            mockParam.Setup(p => p.ValueType).Returns(typeof(int));
            mockParam.Setup(p => p.GetValue()).Throws(new InvalidOperationException("boom"));

            var parameters = new Dictionary<string, IParameter>
            {
                ["key"] = mockParam.Object
            };

            SettingsPersistence.Save(tempPath, parameters);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}
