using System.Text.Json;
using System.Text.Json.Serialization;
using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Reads and writes versioned config exchange documents for explicit import/export scenarios.
/// </summary>
internal static class ConfigExchangePersistence
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    internal static void Export(
        string filePath,
        IReadOnlyDictionary<string, IParameter> parameters,
        string schemaId,
        int schemaVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaId);

        try
        {
            EnsureParentDirectoryExists(filePath);

            var values = new Dictionary<string, object?>();
            foreach (var parameter in parameters.Values)
            {
                if (typeof(Delegate).IsAssignableFrom(parameter.ValueType))
                    continue;

                values[parameter.Key] = parameter.GetValue();
            }

            var document = new ConfigExchangeDocument
            {
                FormatVersion = ConfigExchangeDocument.CurrentFormatVersion,
                SchemaId = schemaId,
                SchemaVersion = schemaVersion,
                Values = values
            };

            File.WriteAllText(filePath, JsonSerializer.Serialize(document, _jsonOptions));
            Logger.Info(
                $"ConfigExchangePersistence: exported {values.Count} parameter(s) for schema '{schemaId}' v{schemaVersion} to '{filePath}'.");
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigExchangePersistence: failed to export config to '{filePath}'.");
        }
    }

    internal static ConfigImportReport Import(
        string filePath,
        IReadOnlyDictionary<string, IParameter> parameters,
        string expectedSchemaId,
        int expectedSchemaVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedSchemaId);

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(filePath));
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return Failed($"Config import file '{filePath}' must contain a JSON object at the root.");
            }

            if (TryGetEnvelopeValues(document.RootElement, out var valuesElement, out var report))
            {
                if (report is not null)
                    return report;

                var schemaId = document.RootElement.GetProperty("schemaId").GetString();
                var schemaVersion = document.RootElement.GetProperty("schemaVersion").GetInt32();
                if (!string.Equals(schemaId, expectedSchemaId, StringComparison.Ordinal))
                {
                    return Failed(
                        $"Config import schema '{schemaId ?? "<null>"}' is not compatible with the current config schema '{expectedSchemaId}'.");
                }

                if (schemaVersion > expectedSchemaVersion)
                {
                    return Failed(
                        $"Config import schema version {schemaVersion} is newer than the current config schema version {expectedSchemaVersion}."
                    );
                }

                return ImportValues(
                    valuesElement,
                    parameters,
                    isLegacyDocument: false,
                    schemaId,
                    schemaVersion,
                    filePath,
                    null);
            }

            return ImportValues(
                document.RootElement,
                parameters,
                isLegacyDocument: true,
                schemaId: null,
                schemaVersion: null,
                filePath,
                envelopeRoot: null);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            return Failed($"Config import file '{filePath}' was not found.");
        }
        catch (JsonException ex)
        {
            Logger.Exception(ex, $"ConfigExchangePersistence: failed to parse import file '{filePath}'.");
            return Failed($"Config import file '{filePath}' contains invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigExchangePersistence: failed to import config from '{filePath}'.");
            return Failed($"Config import failed for '{filePath}': {ex.Message}");
        }
    }

    private static bool TryGetEnvelopeValues(
        JsonElement root,
        out JsonElement valuesElement,
        out ConfigImportReport? failureReport)
    {
        valuesElement = default;
        failureReport = null;

        var hasFormatVersion = root.TryGetProperty("formatVersion", out var formatVersionElement);
        var hasSchemaId = root.TryGetProperty("schemaId", out var schemaIdElement);
        var hasSchemaVersion = root.TryGetProperty("schemaVersion", out var schemaVersionElement);
        var hasValues = root.TryGetProperty("values", out valuesElement);
        if (!hasFormatVersion && !hasSchemaId && !hasSchemaVersion && !hasValues)
            return false;

        if (!hasFormatVersion || !hasSchemaId || !hasSchemaVersion || !hasValues)
        {
            failureReport = Failed("Config import document looks like a versioned exchange document but is missing one or more required envelope properties.");
            return true;
        }

        if (formatVersionElement.ValueKind != JsonValueKind.Number
            || !formatVersionElement.TryGetInt32(out var formatVersion))
        {
            failureReport = Failed("Config import document contains an invalid 'formatVersion' value.");
            return true;
        }

        if (formatVersion != ConfigExchangeDocument.CurrentFormatVersion)
        {
            failureReport = Failed(
                $"Config import document format version {formatVersion} is not supported. Expected {ConfigExchangeDocument.CurrentFormatVersion}.");
            return true;
        }

        if (schemaIdElement.ValueKind != JsonValueKind.String)
        {
            failureReport = Failed("Config import document contains an invalid 'schemaId' value.");
            return true;
        }

        var schemaId = schemaIdElement.GetString();
        if (string.IsNullOrWhiteSpace(schemaId))
        {
            failureReport = Failed("Config import document contains an empty 'schemaId' value.");
            return true;
        }

        if (schemaVersionElement.ValueKind != JsonValueKind.Number
            || !schemaVersionElement.TryGetInt32(out var schemaVersion))
        {
            failureReport = Failed("Config import document contains an invalid 'schemaVersion' value.");
            return true;
        }

        if (schemaVersion < 1)
        {
            failureReport = Failed("Config import document contains a 'schemaVersion' value less than 1.");
            return true;
        }

        if (valuesElement.ValueKind != JsonValueKind.Object)
        {
            failureReport = Failed("Config import document contains an invalid 'values' payload. Expected a JSON object.");
            return true;
        }

        return true;
    }

    private static ConfigImportReport ImportValues(
        JsonElement values,
        IReadOnlyDictionary<string, IParameter> parameters,
        bool isLegacyDocument,
        string? schemaId,
        int? schemaVersion,
        string filePath,
        JsonElement? envelopeRoot)
    {
        if (envelopeRoot.HasValue)
        {
            var root = envelopeRoot.Value;
            schemaId = root.GetProperty("schemaId").GetString();
            schemaVersion = root.GetProperty("schemaVersion").GetInt32();
        }

        var issues = new List<ConfigImportIssue>();
        var appliedCount = 0;
        var ignoredCount = 0;
        var rejectedCount = 0;

        foreach (var property in values.EnumerateObject())
        {
            if (!parameters.TryGetValue(property.Name, out var parameter))
            {
                ignoredCount++;
                issues.Add(new ConfigImportIssue
                {
                    Category = "Ignored",
                    Key = property.Name,
                    Message = "The key does not exist in the current config schema."
                });
                continue;
            }

            if (typeof(Delegate).IsAssignableFrom(parameter.ValueType))
            {
                ignoredCount++;
                issues.Add(new ConfigImportIssue
                {
                    Category = "Ignored",
                    Key = property.Name,
                    Message = "Delegate-backed parameters are not importable."
                });
                continue;
            }

            if (!ParameterJsonReader.TryConvert(property.Value, parameter.ValueType, out var convertedValue, out var failureReason))
            {
                rejectedCount++;
                issues.Add(new ConfigImportIssue
                {
                    Category = "Rejected",
                    Key = property.Name,
                    Message = failureReason ?? "The imported value could not be converted to the parameter type."
                });
                continue;
            }

            if (!TryApplyImportedValue(parameter, convertedValue, out failureReason))
            {
                rejectedCount++;
                issues.Add(new ConfigImportIssue
                {
                    Category = "Rejected",
                    Key = property.Name,
                    Message = failureReason ?? "The imported value was rejected by the current parameter."
                });
                continue;
            }

            appliedCount++;
        }

        Logger.Info(
            $"ConfigExchangePersistence: imported {appliedCount} applied, {ignoredCount} ignored, {rejectedCount} rejected key(s) from '{filePath}'.");

        return new ConfigImportReport
        {
            Success = true,
            IsLegacyDocument = isLegacyDocument,
            SchemaId = schemaId,
            SchemaVersion = schemaVersion,
            AppliedCount = appliedCount,
            IgnoredCount = ignoredCount,
            RejectedCount = rejectedCount,
            Issues = issues
        };
    }

    private static bool TryApplyImportedValue(IParameter parameter, object? value, out string? failureReason)
    {
        failureReason = null;
        if (parameter is IParameterValidationState validationState)
            validationState.ClearValidationError();

        try
        {
            parameter.SetValue(value);
        }
        catch (ArgumentException ex)
        {
            failureReason = ex.Message;
            return false;
        }

        if (parameter is IParameterValidationState failedValidationState
            && failedValidationState.HasValidationError)
        {
            failureReason = failedValidationState.ValidationError ?? "The imported value failed validation.";
            return false;
        }

        return true;
    }

    private static ConfigImportReport Failed(string failureReason)
        => new()
        {
            Success = false,
            FailureReason = failureReason
        };

    private static void EnsureParentDirectoryExists(string filePath)
    {
        var directoryPath = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (string.IsNullOrEmpty(directoryPath))
            return;

        Directory.CreateDirectory(directoryPath);
    }
}
