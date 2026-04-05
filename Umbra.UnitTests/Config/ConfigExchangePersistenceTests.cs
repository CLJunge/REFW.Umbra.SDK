using System.Text.Json;
using Umbra.Config.Attributes;

namespace Umbra.Config.UnitTests;

/// <summary>
/// Unit tests for config import/export behavior exposed by <see cref="ConfigStore{TConfig}"/>.
/// </summary>
[TestClass]
public sealed class ConfigExchangePersistenceTests
{
    private string _testDirectory = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), nameof(ConfigExchangePersistenceTests), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (!Directory.Exists(_testDirectory))
            return;

        try
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
        catch
        {
            // Ignore cleanup errors.
        }
    }

    /// <summary>
    /// Verifies that export writes the versioned envelope and skips delegate-backed parameters.
    /// </summary>
    [TestMethod]
    public void Export_WritesVersionedEnvelopeAndSkipsDelegateParameters()
    {
        var runtimePath = Path.Combine(_testDirectory, "runtime.json");
        var exportPath = Path.Combine(_testDirectory, "export.json");
        var store = new ConfigStore<ExchangeConfig>(runtimePath);

        try
        {
            var config = store.Load();
            config.NumberValue.Set(9);
            config.TextValue.Set("updated");
            store.Export(exportPath);

            using var document = JsonDocument.Parse(File.ReadAllText(exportPath));
            var root = document.RootElement;
            var values = root.GetProperty("values");

            Assert.AreEqual(ConfigExchangeDocument.CurrentFormatVersion, root.GetProperty("formatVersion").GetInt32());
            Assert.AreEqual(typeof(ExchangeConfig).FullName, root.GetProperty("schemaId").GetString());
            Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
            Assert.AreEqual(9, values.GetProperty("exchange.numberValue").GetInt32());
            Assert.AreEqual("updated", values.GetProperty("exchange.textValue").GetString());
            Assert.AreEqual(5, values.GetProperty("exchange.rangeValue").GetInt32());
            Assert.IsFalse(values.TryGetProperty("exchange.actionValue", out _));
        }
        finally
        {
            store.Dispose();
        }
    }

    /// <summary>
    /// Verifies that import accepts the legacy flat JSON shape, applies matching keys, and ignores unknown keys.
    /// </summary>
    [TestMethod]
    public void Import_LegacyFlatJson_AppliesMatchingKeysAndIgnoresUnknownKeys()
    {
        var runtimePath = Path.Combine(_testDirectory, "runtime.json");
        var importPath = Path.Combine(_testDirectory, "legacy-import.json");
        File.WriteAllText(importPath, """
{
  "exchange.numberValue": 7,
  "exchange.textValue": "legacy",
  "exchange.unknownValue": true
}
""");

        var store = new ConfigStore<ExchangeConfig>(runtimePath);
        try
        {
            var config = store.Load();

            var report = store.Import(importPath);

            Assert.IsTrue(report.Success);
            Assert.IsTrue(report.IsLegacyDocument);
            Assert.AreEqual(2, report.AppliedCount);
            Assert.AreEqual(1, report.IgnoredCount);
            Assert.AreEqual(0, report.RejectedCount);
            Assert.IsTrue(report.Saved);
            Assert.AreEqual(7, config.NumberValue.Value);
            Assert.AreEqual("legacy", config.TextValue.Value);

            using var runtimeDocument = JsonDocument.Parse(File.ReadAllText(runtimePath));
            Assert.AreEqual(7, runtimeDocument.RootElement.GetProperty("exchange.numberValue").GetInt32());
            Assert.AreEqual("legacy", runtimeDocument.RootElement.GetProperty("exchange.textValue").GetString());
        }
        finally
        {
            store.Dispose();
        }
    }

    /// <summary>
    /// Verifies that versioned imports reject newer schema versions before mutating the live store.
    /// </summary>
    [TestMethod]
    public void Import_VersionedEnvelopeWithNewerSchemaVersion_ReturnsFailureAndDoesNotMutate()
    {
        var runtimePath = Path.Combine(_testDirectory, "runtime.json");
        var importPath = Path.Combine(_testDirectory, "future-envelope.json");
        File.WriteAllText(importPath, $$"""
{
  "formatVersion": 1,
  "schemaId": "{{typeof(ExchangeConfig).FullName}}",
  "schemaVersion": 3,
  "values": {
    "exchange.numberValue": 99
  }
}
""");

        var store = new ConfigStore<ExchangeConfig>(runtimePath);
        try
        {
            var config = store.Load();

            var report = store.Import(importPath, new ConfigImportOptions { SaveAfterImport = false });

            Assert.IsFalse(report.Success);
            Assert.AreEqual(3, config.NumberValue.Value);
            Assert.IsFalse(report.Saved);
        }
        finally
        {
            store.Dispose();
        }
    }

    /// <summary>
    /// Verifies that versioned imports reject mismatched schema identifiers before mutating the live store.
    /// </summary>
    [TestMethod]
    public void Import_VersionedEnvelopeWithMismatchedSchemaId_ReturnsFailureAndDoesNotMutate()
    {
        var runtimePath = Path.Combine(_testDirectory, "runtime.json");
        var importPath = Path.Combine(_testDirectory, "mismatched-envelope.json");
        File.WriteAllText(importPath, """
{
  "formatVersion": 1,
  "schemaId": "Umbra.Config.UnitTests.OtherConfig",
  "schemaVersion": 2,
  "values": {
    "exchange.textValue": "wrong"
  }
}
""");

        var store = new ConfigStore<ExchangeConfig>(runtimePath);
        try
        {
            var config = store.Load();

            var report = store.Import(importPath, new ConfigImportOptions { SaveAfterImport = false });

            Assert.IsFalse(report.Success);
            Assert.AreEqual("default", config.TextValue.Value);
            Assert.IsFalse(report.Saved);
        }
        finally
        {
            store.Dispose();
        }
    }

    /// <summary>
    /// Verifies that import rejects values that fail current parameter validation and preserves the prior value.
    /// </summary>
    [TestMethod]
    public void Import_VersionedEnvelopeWithInvalidValidatedValue_RejectsValueAndPreservesPreviousState()
    {
        var runtimePath = Path.Combine(_testDirectory, "runtime.json");
        var importPath = Path.Combine(_testDirectory, "invalid-envelope.json");
        File.WriteAllText(importPath, $$"""
{
  "formatVersion": 1,
  "schemaId": "{{typeof(ExchangeConfig).FullName}}",
  "schemaVersion": 2,
  "values": {
    "exchange.rangeValue": 99,
    "exchange.textValue": "still-valid"
  }
}
""");

        var store = new ConfigStore<ExchangeConfig>(runtimePath);
        try
        {
            var config = store.Load();

            var report = store.Import(importPath, new ConfigImportOptions { SaveAfterImport = false });

            Assert.IsTrue(report.Success);
            Assert.AreEqual(1, report.AppliedCount);
            Assert.AreEqual(0, report.IgnoredCount);
            Assert.AreEqual(1, report.RejectedCount);
            Assert.AreEqual(5, config.RangeValue.Value);
            Assert.AreEqual("still-valid", config.TextValue.Value);
            Assert.AreEqual("Rejected", report.Issues[0].Category);
            Assert.AreEqual("exchange.rangeValue", report.Issues[0].Key);
        }
        finally
        {
            store.Dispose();
        }
    }

    /// <summary>
    /// Verifies that disabling SaveAfterImport keeps the runtime file unchanged while updating the live config instance.
    /// </summary>
    [TestMethod]
    public void Import_SaveAfterImportDisabled_LeavesRuntimeFileUnchanged()
    {
        var runtimePath = Path.Combine(_testDirectory, "runtime.json");
        var importPath = Path.Combine(_testDirectory, "save-disabled.json");
        File.WriteAllText(importPath, $$"""
{
  "formatVersion": 1,
  "schemaId": "{{typeof(ExchangeConfig).FullName}}",
  "schemaVersion": 2,
  "values": {
    "exchange.textValue": "memory-only"
  }
}
""");

        var store = new ConfigStore<ExchangeConfig>(runtimePath);
        try
        {
            var config = store.Load();

            var report = store.Import(importPath, new ConfigImportOptions { SaveAfterImport = false });

            Assert.IsTrue(report.Success);
            Assert.IsFalse(report.Saved);
            Assert.AreEqual("memory-only", config.TextValue.Value);

            using var runtimeDocument = JsonDocument.Parse(File.ReadAllText(runtimePath));
            Assert.AreEqual("default", runtimeDocument.RootElement.GetProperty("exchange.textValue").GetString());
        }
        finally
        {
            store.Dispose();
        }
    }

    /// <summary>
    /// Verifies that malformed versioned envelopes fail before any values are applied.
    /// </summary>
    [TestMethod]
    public void Import_MalformedEnvelope_ReturnsFailure()
    {
        var runtimePath = Path.Combine(_testDirectory, "runtime.json");
        var importPath = Path.Combine(_testDirectory, "malformed-envelope.json");
        File.WriteAllText(importPath, """
{
  "formatVersion": 1,
  "schemaId": "Umbra.Config.UnitTests.ExchangeConfig"
}
""");

        var store = new ConfigStore<ExchangeConfig>(runtimePath);
        try
        {
            _ = store.Load();

            var report = store.Import(importPath, new ConfigImportOptions { SaveAfterImport = false });

            Assert.IsFalse(report.Success);
            Assert.IsFalse(report.Saved);
            Assert.IsFalse(string.IsNullOrWhiteSpace(report.FailureReason));
        }
        finally
        {
            store.Dispose();
        }
    }

    [UmbraAutoRegister]
    [UmbraConfigVersion(2)]
    [UmbraPrefix("exchange")]
    private sealed class ExchangeConfig
    {
        [UmbraParameter]
        public Parameter<int> NumberValue { get; set; } = new(3);

        [UmbraParameter]
        public Parameter<string> TextValue { get; set; } = new("default");

        [UmbraParameter]
        [UmbraRange(0, 10)]
        public Parameter<int> RangeValue { get; set; } = new(5);

        [UmbraParameter]
        public Parameter<Action> ActionValue { get; set; } = new(() =>
        {
        });
    }
}
