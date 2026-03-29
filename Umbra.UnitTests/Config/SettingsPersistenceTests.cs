using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.Config;

namespace Umbra.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="SettingsPersistence"/> class.
/// </summary>
[TestClass]
public class SettingsPersistenceTests
{
    private string _testDirectory = string.Empty;

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
    /// Tests that Load returns Success when the JSON file contains an empty object.
    /// </summary>
    [TestMethod]
    public void Load_EmptyJsonObject_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{}";
        File.WriteAllText(filePath, json);

        var parameters = new Dictionary<string, IParameter>();

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
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
    /// Tests that Load returns Success when the parameters dictionary is empty.
    /// </summary>
    [TestMethod]
    public void Load_EmptyParametersDictionary_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""testKey"": ""testValue""}";
        File.WriteAllText(filePath, json);

        var parameters = new Dictionary<string, IParameter>();

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
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
    /// Tests that Load returns MissingFile when the parent directory does not exist.
    /// </summary>
    [TestMethod]
    public void Load_DirectoryNotFound_ReturnsMissingFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent", "settings.json");
        var parameters = new Dictionary<string, IParameter>();

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.MissingFile, result);
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
            result == SettingsPersistence.LoadResult.RecoveredToDefaults ||
            result == SettingsPersistence.LoadResult.Failed,
            "Expected RecoveredToDefaults or Failed for malformed JSON");
    }

    /// <summary>
    /// Tests that Load handles an empty file gracefully.
    /// </summary>
    [TestMethod]
    public void Load_EmptyFile_ReturnsRecoveredToDefaultsOrFailed()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        File.WriteAllText(filePath, string.Empty);

        var parameters = new Dictionary<string, IParameter>();

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.IsTrue(
            result == SettingsPersistence.LoadResult.RecoveredToDefaults ||
            result == SettingsPersistence.LoadResult.Failed,
            "Expected RecoveredToDefaults or Failed for empty file");
    }

    /// <summary>
    /// Tests that Load handles a file containing only whitespace.
    /// </summary>
    [TestMethod]
    public void Load_WhitespaceOnlyFile_ReturnsRecoveredToDefaultsOrFailed()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        File.WriteAllText(filePath, "   \t\n   ");

        var parameters = new Dictionary<string, IParameter>();

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.IsTrue(
            result == SettingsPersistence.LoadResult.RecoveredToDefaults ||
            result == SettingsPersistence.LoadResult.Failed,
            "Expected RecoveredToDefaults or Failed for whitespace-only file");
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
    /// Tests that Load handles JSON with array values correctly.
    /// </summary>
    [TestMethod]
    public void Load_JsonWithArrayValue_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key1"": [1, 2, 3]}";
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
    /// Tests that Load handles a large number of parameters correctly.
    /// </summary>
    [TestMethod]
    public void Load_LargeNumberOfParameters_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var jsonBuilder = new System.Text.StringBuilder("{");
        var parameters = new Dictionary<string, IParameter>();

        for (int i = 0; i < 1000; i++)
        {
            if (i > 0) jsonBuilder.Append(',');
            jsonBuilder.Append($"\"key{i}\": {i}");

            var paramMock = new Mock<IParameter>();
            paramMock.SetupGet(p => p.ValueType).Returns(typeof(int));
            parameters[$"key{i}"] = paramMock.Object;
        }

        jsonBuilder.Append('}');
        File.WriteAllText(filePath, jsonBuilder.ToString());

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
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
    /// Tests that Load handles Unicode characters in JSON values.
    /// </summary>
    [TestMethod]
    public void Load_JsonWithUnicodeCharacters_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key1"": ""日本語テキスト 🎮""}";
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
    /// Tests that Load handles very long file paths correctly (when supported by the OS).
    /// </summary>
    [TestMethod]
    public void Load_VeryLongFilePath_HandlesGracefully()
    {
        // Arrange
        var longPath = Path.Combine(_testDirectory, new string('a', 200), "settings.json");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(longPath)!);
            File.WriteAllText(longPath, @"{""key"": ""value""}");

            var parameterMock = new Mock<IParameter>();
            parameterMock.SetupGet(p => p.ValueType).Returns(typeof(string));
            var parameters = new Dictionary<string, IParameter>
            {
                ["key"] = parameterMock.Object
            };

            // Act
            var result = SettingsPersistence.Load(longPath, parameters);

            // Assert
            Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        }
        catch (PathTooLongException)
        {
            // Some file systems may not support very long paths; test is inconclusive in that case
            Assert.Inconclusive("File system does not support long paths");
        }
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
    /// Tests that Load handles JSON with extreme numeric values correctly.
    /// </summary>
    [TestMethod]
    public void Load_JsonWithExtremeNumericValues_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = $@"{{""minInt"": {int.MinValue}, ""maxInt"": {int.MaxValue}, ""zero"": 0}}";
        File.WriteAllText(filePath, json);

        var minIntParamMock = new Mock<IParameter>();
        minIntParamMock.SetupGet(p => p.ValueType).Returns(typeof(int));

        var maxIntParamMock = new Mock<IParameter>();
        maxIntParamMock.SetupGet(p => p.ValueType).Returns(typeof(int));

        var zeroParamMock = new Mock<IParameter>();
        zeroParamMock.SetupGet(p => p.ValueType).Returns(typeof(int));

        var parameters = new Dictionary<string, IParameter>
        {
            ["minInt"] = minIntParamMock.Object,
            ["maxInt"] = maxIntParamMock.Object,
            ["zero"] = zeroParamMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        minIntParamMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
        maxIntParamMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
        zeroParamMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that Load handles JSON with negative numeric values correctly.
    /// </summary>
    [TestMethod]
    public void Load_JsonWithNegativeNumericValues_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""negInt"": -42, ""negFloat"": -3.14}";
        File.WriteAllText(filePath, json);

        var negIntParamMock = new Mock<IParameter>();
        negIntParamMock.SetupGet(p => p.ValueType).Returns(typeof(int));

        var negFloatParamMock = new Mock<IParameter>();
        negFloatParamMock.SetupGet(p => p.ValueType).Returns(typeof(float));

        var parameters = new Dictionary<string, IParameter>
        {
            ["negInt"] = negIntParamMock.Object,
            ["negFloat"] = negFloatParamMock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        negIntParamMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
        negFloatParamMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
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
    /// Tests that Load correctly applies multiple values from JSON to multiple parameters.
    /// </summary>
    [TestMethod]
    public void Load_MultipleMatchingParameters_AppliesAll()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key1"": ""value1"", ""key2"": 42, ""key3"": true}";
        File.WriteAllText(filePath, json);

        var param1Mock = new Mock<IParameter>();
        param1Mock.SetupGet(p => p.ValueType).Returns(typeof(string));

        var param2Mock = new Mock<IParameter>();
        param2Mock.SetupGet(p => p.ValueType).Returns(typeof(int));

        var param3Mock = new Mock<IParameter>();
        param3Mock.SetupGet(p => p.ValueType).Returns(typeof(bool));

        var parameters = new Dictionary<string, IParameter>
        {
            ["key1"] = param1Mock.Object,
            ["key2"] = param2Mock.Object,
            ["key3"] = param3Mock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        param1Mock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
        param2Mock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
        param3Mock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that Load handles a mix of matching and non-matching keys correctly.
    /// </summary>
    [TestMethod]
    public void Load_MixedMatchingAndNonMatchingKeys_AppliesOnlyMatching()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key1"": ""value1"", ""unmatchedKey"": ""ignored"", ""key2"": 42}";
        File.WriteAllText(filePath, json);

        var param1Mock = new Mock<IParameter>();
        param1Mock.SetupGet(p => p.ValueType).Returns(typeof(string));

        var param2Mock = new Mock<IParameter>();
        param2Mock.SetupGet(p => p.ValueType).Returns(typeof(int));

        var parameters = new Dictionary<string, IParameter>
        {
            ["key1"] = param1Mock.Object,
            ["key2"] = param2Mock.Object
        };

        // Act
        var result = SettingsPersistence.Load(filePath, parameters);

        // Assert
        Assert.AreEqual(SettingsPersistence.LoadResult.Success, result);
        param1Mock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
        param2Mock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that Load handles JSON with very long string values.
    /// </summary>
    [TestMethod]
    public void Load_JsonWithVeryLongStringValue_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var longString = new string('a', 10000);
        var json = $@"{{""key"": ""{longString}""}}";
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
        parameterMock.Verify(p => p.SetValueWithoutNotify(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that Load handles reading from a read-only file.
    /// </summary>
    [TestMethod]
    public void Load_ReadOnlyFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "settings.json");
        var json = @"{""key"": ""value""}";
        File.WriteAllText(filePath, json);

        var fileInfo = new FileInfo(filePath);
        fileInfo.IsReadOnly = true;

        try
        {
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
        finally
        {
            fileInfo.IsReadOnly = false;
        }
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
    /// Tests that Save successfully writes a single non-delegate parameter to disk.
    /// </summary>
    [TestMethod]
    public void Save_SingleParameter_WritesJsonFile()
    {
        // Arrange
        string tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
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
            string jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(1, deserialized.Count);
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
        string tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var mockDelegateParam = new Mock<IParameter>();
            mockDelegateParam.Setup(p => p.Key).Returns("delegateKey");
            mockDelegateParam.Setup(p => p.ValueType).Returns(typeof(Action));
            mockDelegateParam.Setup(p => p.GetValue()).Returns(new Action(() => { }));

            var mockNormalParam = new Mock<IParameter>();
            mockNormalParam.Setup(p => p.Key).Returns("normalKey");
            mockNormalParam.Setup(p => p.ValueType).Returns(typeof(string));
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
            string jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(1, deserialized.Count, "Only non-delegate parameter should be saved");
            Assert.IsFalse(deserialized.ContainsKey("delegateKey"), "Delegate parameter should be filtered");
            Assert.IsTrue(deserialized.ContainsKey("normalKey"), "Normal parameter should be saved");
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
        string tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
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
            string jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(3, deserialized.Count);
            Assert.AreEqual(100, deserialized["intKey"].GetInt32());
            Assert.AreEqual("test", deserialized["stringKey"].GetString());
            Assert.AreEqual(true, deserialized["boolKey"].GetBoolean());
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
        string tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var parameters = new Dictionary<string, IParameter>();

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            Assert.IsTrue(File.Exists(tempPath));
            string jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(0, deserialized.Count);
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
        string tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
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
            string jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(1, deserialized.Count);
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
        string tempDir = Path.Combine(Path.GetTempPath(), $"umbra_test_dir_{Guid.NewGuid()}");
        string tempPath = Path.Combine(tempDir, "settings.json");
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
    /// Tests that Save handles various delegate-derived types correctly by filtering them all out.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(Action))]
    [DataRow(typeof(Func<int>))]
    [DataRow(typeof(EventHandler))]
    public void Save_VariousDelegateTypes_AllFilteredOut(Type delegateType)
    {
        // Arrange
        string tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var mockDelegateParam = new Mock<IParameter>();
            mockDelegateParam.Setup(p => p.Key).Returns("delegateKey");
            mockDelegateParam.Setup(p => p.ValueType).Returns(delegateType);

            var mockNormalParam = new Mock<IParameter>();
            mockNormalParam.Setup(p => p.Key).Returns("normalKey");
            mockNormalParam.Setup(p => p.ValueType).Returns(typeof(int));
            mockNormalParam.Setup(p => p.GetValue()).Returns(42);

            var parameters = new Dictionary<string, IParameter>
            {
                ["delegateKey"] = mockDelegateParam.Object,
                ["normalKey"] = mockNormalParam.Object
            };

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            string jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(1, deserialized.Count);
            Assert.IsFalse(deserialized.ContainsKey("delegateKey"));
            Assert.IsTrue(deserialized.ContainsKey("normalKey"));
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Save handles overwriting an existing file correctly.
    /// </summary>
    [TestMethod]
    public void Save_ExistingFile_OverwritesContent()
    {
        // Arrange
        string tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
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
            string jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(1, deserialized.Count);
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
        string invalidPath = new string(Path.GetInvalidPathChars()[0], 5);
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
    /// Tests that Save handles parameter keys with special characters correctly.
    /// </summary>
    [TestMethod]
    public void Save_ParameterKeysWithSpecialCharacters_SavedCorrectly()
    {
        // Arrange
        string tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
        try
        {
            var mockParam1 = new Mock<IParameter>();
            mockParam1.Setup(p => p.Key).Returns("key.with.dots");
            mockParam1.Setup(p => p.ValueType).Returns(typeof(int));
            mockParam1.Setup(p => p.GetValue()).Returns(1);

            var mockParam2 = new Mock<IParameter>();
            mockParam2.Setup(p => p.Key).Returns("key-with-dashes");
            mockParam2.Setup(p => p.ValueType).Returns(typeof(int));
            mockParam2.Setup(p => p.GetValue()).Returns(2);

            var mockParam3 = new Mock<IParameter>();
            mockParam3.Setup(p => p.Key).Returns("key_with_underscores");
            mockParam3.Setup(p => p.ValueType).Returns(typeof(int));
            mockParam3.Setup(p => p.GetValue()).Returns(3);

            var parameters = new Dictionary<string, IParameter>
            {
                ["key.with.dots"] = mockParam1.Object,
                ["key-with-dashes"] = mockParam2.Object,
                ["key_with_underscores"] = mockParam3.Object
            };

            // Act
            SettingsPersistence.Save(tempPath, parameters);

            // Assert
            string jsonContent = File.ReadAllText(tempPath);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(3, deserialized.Count);
            Assert.IsTrue(deserialized.ContainsKey("key.with.dots"));
            Assert.IsTrue(deserialized.ContainsKey("key-with-dashes"));
            Assert.IsTrue(deserialized.ContainsKey("key_with_underscores"));
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Save handles parameters containing complex objects correctly.
    /// </summary>
    [TestMethod]
    public void Save_ComplexObjectParameter_SerializedCorrectly()
    {
        // Arrange
        string tempPath = Path.Combine(Path.GetTempPath(), $"umbra_test_{Guid.NewGuid()}.json");
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
            string jsonContent = File.ReadAllText(tempPath);
            Assert.IsTrue(jsonContent.Contains("\"name\""), "JSON should contain property names in camelCase");
            Assert.IsTrue(jsonContent.Contains("\"value\""));
            Assert.IsTrue(jsonContent.Contains("\"nested\""));
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}