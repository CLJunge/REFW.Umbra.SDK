namespace Umbra.Logging.UnitTests;


/// <summary>
/// Unit tests for <see cref="PluginLogger"/>.
/// </summary>
[TestClass]
public sealed class PluginLoggerTests
{
    /// <summary>
    /// Tests that the constructor stores the provided prefix format and minimum level independently across instances.
    /// </summary>
    [TestMethod]
    public void InstanceProperties_MultipleLoggers_RemainIndependent()
    {
        var first = new PluginLogger("First")
        {
            PrefixFormat = "<{0}>",
            MinLevel = LogLevel.Warning
        };
        var second = new PluginLogger("Second")
        {
            PrefixFormat = "[{0}]",
            MinLevel = LogLevel.Error
        };

        Assert.AreEqual("First", first.Prefix);
        Assert.AreEqual("<{0}>", first.PrefixFormat);
        Assert.AreEqual(LogLevel.Warning, first.MinLevel);
        Assert.AreEqual("Second", second.Prefix);
        Assert.AreEqual("[{0}]", second.PrefixFormat);
        Assert.AreEqual(LogLevel.Error, second.MinLevel);
    }

    /// <summary>
    /// Tests that the constructor correctly handles various string values including empty and whitespace.
    /// Input: Various string edge cases (empty, whitespace, long strings, special characters).
    /// Expected: The Prefix property is set to the exact value provided.
    /// </summary>
    [TestMethod]
    [DataRow("", DisplayName = "Empty string")]
    [DataRow(" ", DisplayName = "Single space")]
    [DataRow("  ", DisplayName = "Multiple spaces")]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    [DataRow("\r\n", DisplayName = "Carriage return and newline")]
    [DataRow("   \t\n   ", DisplayName = "Mixed whitespace")]
    [DataRow("A", DisplayName = "Single character")]
    [DataRow("Plugin123", DisplayName = "Alphanumeric")]
    [DataRow("My-Plugin_v2.0", DisplayName = "With special characters")]
    [DataRow("🎮Plugin🎯", DisplayName = "With Unicode emoji")]
    [DataRow("Plugin\u0000WithNull", DisplayName = "With null character")]
    [DataRow("Very.Long.Deeply.Nested.Plugin.Name.With.Many.Segments.That.Exceeds.Normal.Length.Expectations.And.Continues.For.Testing.Purposes.Only", DisplayName = "Very long string")]
    public void Constructor_VariousStringValues_SetsPrefixCorrectly(string prefix)
    {
        // Act
        var logger = new PluginLogger(prefix);

        // Assert
        Assert.AreEqual(prefix, logger.Prefix);
    }

    /// <summary>
    /// Tests that the constructor handles null prefix even though the parameter is non-nullable.
    /// Input: null value for prefix parameter.
    /// Expected: The Prefix property is set to null without throwing an exception.
    /// </summary>
    [TestMethod]
    public void Constructor_NullPrefix_SetsPrefixToNull()
    {
        // Arrange
        string? prefix = null;

        // Act
        var logger = new PluginLogger(prefix!);

        // Assert
        Assert.IsNull(logger.Prefix);
    }

    /// <summary>
    /// Tests that the constructor correctly handles strings with control characters.
    /// Input: A string containing various control characters.
    /// Expected: The Prefix property preserves all control characters.
    /// </summary>
    [TestMethod]
    public void Constructor_StringWithControlCharacters_PreservesControlCharacters()
    {
        // Arrange
        var prefixWithControls = "Plugin\u0001\u0002\u0003\u001F";

        // Act
        var logger = new PluginLogger(prefixWithControls);

        // Assert
        Assert.AreEqual(prefixWithControls, logger.Prefix);
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when MinLevel is set to None,
    /// filtering out the info message before formatting occurs.
    /// </summary>
    [TestMethod]
    public void Info_MinLevelNone_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.None
        };

        // Act & Assert
        logger.Info("Test {0}", "value");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when MinLevel is set to Info
    /// and format string is valid.
    /// </summary>
    /// <remarks>
    /// Note: This test verifies exception-safe behavior but cannot verify the actual logging
    /// call due to static Logger dependencies that cannot be mocked with Moq.
    /// </remarks>
    [TestMethod]
    public void Info_MinLevelInfoValidFormat_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {0}", "value");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given a null format string.
    /// The method should catch the ArgumentNullException from string.Format and return silently.
    /// </summary>
    [TestMethod]
    public void Info_NullFormat_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info(null!, "value");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given an empty format string.
    /// </summary>
    [TestMethod]
    public void Info_EmptyFormat_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info(string.Empty);
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given a whitespace-only format string.
    /// </summary>
    [TestMethod]
    public void Info_WhitespaceFormat_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("   ", "value");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given an invalid format string
    /// with mismatched braces. The method should catch the FormatException and return silently.
    /// </summary>
    [TestMethod]
    public void Info_InvalidFormatMismatchedBraces_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {0", "value");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given a format string with
    /// a placeholder index that exceeds the args array length. The method should catch the
    /// FormatException and return silently.
    /// </summary>
    [TestMethod]
    public void Info_FormatIndexOutOfRange_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {0} {1} {2}", "value1");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given a format string with
    /// negative placeholder index. The method should catch the FormatException and return silently.
    /// </summary>
    [TestMethod]
    public void Info_FormatNegativeIndex_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {-1}", "value");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given a null args array.
    /// The method should catch the ArgumentNullException from string.Format and return silently.
    /// </summary>
    [TestMethod]
    public void Info_NullArgs_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {0}", null!);
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given an empty args array
    /// with a format string that expects arguments. The method should catch the FormatException
    /// and return silently.
    /// </summary>
    [TestMethod]
    public void Info_EmptyArgsWithPlaceholders_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {0}", Array.Empty<object>());
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given args array containing null elements.
    /// </summary>
    [TestMethod]
    public void Info_ArgsWithNullElements_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {0} {1}", null!, null!);
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given a very long format string.
    /// </summary>
    [TestMethod]
    public void Info_VeryLongFormatString_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };
        var longFormat = new string('a', 10000) + " {0}";

        // Act & Assert
        logger.Info(longFormat, "value");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given a format string with
    /// special characters and control characters.
    /// </summary>
    [TestMethod]
    public void Info_FormatWithSpecialCharacters_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test\n\r\t{0}\0", "value");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when given multiple valid placeholders
    /// and matching args count.
    /// </summary>
    [TestMethod]
    public void Info_MultipleValidPlaceholders_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {0} {1} {2} {3}", "a", "b", "c", "d");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when args contain complex objects.
    /// </summary>
    [TestMethod]
    public void Info_ArgsWithComplexObjects_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };
        var obj = new { Name = "Test", Value = 42 };

        // Act & Assert
        logger.Info("Test {0}", obj);
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when format string contains
    /// duplicate placeholders with the same index.
    /// </summary>
    [TestMethod]
    public void Info_DuplicatePlaceholders_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {0} {0} {0}", "value");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when format string uses
    /// format specifiers with placeholders.
    /// </summary>
    [TestMethod]
    public void Info_PlaceholdersWithFormatSpecifiers_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {0:N2} {1:X}", 123.456, 255);
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when format string contains
    /// escaped braces.
    /// </summary>
    [TestMethod]
    public void Info_EscapedBraces_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {{0}} {0}", "value");
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when args array contains
    /// extreme numeric values.
    /// </summary>
    [TestMethod]
    public void Info_ArgsWithExtremeNumericValues_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test {0} {1} {2} {3} {4}", int.MinValue, int.MaxValue, double.NaN, double.PositiveInfinity, double.NegativeInfinity);
    }

    /// <summary>
    /// Tests that Info with format and args does not throw when format has no placeholders
    /// but args are provided.
    /// </summary>
    [TestMethod]
    public void Info_NoPlaceholdersWithArgs_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert
        logger.Info("Test message", "unused", "args");
    }

    /// <summary>
    /// Tests that Info does not throw when Logger is disabled globally.
    /// The method should return early without attempting to log.
    /// </summary>
    [TestMethod]
    public void Info_LoggerDisabled_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            PluginLogger logger = new("TestPlugin");

            // Act & Assert
            logger.Info("Test message");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning returns early when the minimum level filters warning messages out.
    /// </summary>
    [TestMethod]
    public void Warning_MinLevelNone_DoesNotThrow()
    {
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new("TestPlugin") { MinLevel = LogLevel.None };

            logger.Warning("Suppressed warning");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info does not throw when MinLevel is set above Info level.
    /// The method should return early without attempting to log.
    /// </summary>
    /// <param name="minLevel">The minimum log level to test.</param>
    [TestMethod]
    [DataRow(LogLevel.Warning)]
    [DataRow(LogLevel.Error)]
    [DataRow(LogLevel.None)]
    public void Info_MinLevelAboveInfo_DoesNotThrow(LogLevel minLevel)
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new("TestPlugin") { MinLevel = minLevel };

            // Act & Assert
            logger.Info("Test message");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info does not throw when provided with various valid message inputs.
    /// The method is exception-safe and should handle all inputs gracefully.
    /// </summary>
    /// <param name="message">The message to log.</param>
    [TestMethod]
    [DataRow("Simple message")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("Message with special characters: !@#$%^&*()")]
    [DataRow("Message with unicode: 日本語")]
    [DataRow("Message\nwith\nnewlines")]
    [DataRow("Message\twith\ttabs")]
    public void Info_VariousMessages_DoesNotThrow(string message)
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new("TestPlugin") { MinLevel = LogLevel.Info };

            // Act & Assert
            logger.Info(message);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info does not throw when provided with a null message.
    /// The method is exception-safe and should handle null gracefully.
    /// </summary>
    [TestMethod]
    public void Info_NullMessage_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new("TestPlugin") { MinLevel = LogLevel.Info };

            // Act & Assert
            logger.Info(null!);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info does not throw when provided with an extremely long message.
    /// The method should handle large strings without issues.
    /// </summary>
    [TestMethod]
    public void Info_VeryLongMessage_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new("TestPlugin") { MinLevel = LogLevel.Info };
            string longMessage = new('A', 100000);

            // Act & Assert
            logger.Info(longMessage);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info correctly applies prefix formatting when a prefix is set.
    /// The method should prepend the prefix to the message without throwing.
    /// </summary>
    [TestMethod]
    public void Info_WithPrefix_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new("MyPlugin") { MinLevel = LogLevel.Info };

            // Act & Assert
            logger.Info("Test message");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info works correctly when no prefix is set.
    /// The method should log the message as-is without throwing.
    /// </summary>
    [TestMethod]
    public void Info_WithoutPrefix_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new() { Prefix = null, MinLevel = LogLevel.Info };

            // Act & Assert
            logger.Info("Test message");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info works correctly with an empty prefix.
    /// The method should log the message without prefix formatting.
    /// </summary>
    [TestMethod]
    public void Info_EmptyPrefix_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new() { Prefix = string.Empty, MinLevel = LogLevel.Info };

            // Act & Assert
            logger.Info("Test message");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info works correctly with a custom prefix format.
    /// The method should apply the custom format without throwing.
    /// </summary>
    [TestMethod]
    public void Info_CustomPrefixFormat_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new("MyPlugin")
            {
                PrefixFormat = "<<{0}>>",
                MinLevel = LogLevel.Info
            };

            // Act & Assert
            logger.Info("Test message");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info does not throw when both Logger is disabled and MinLevel is above Info.
    /// The method should return early on the first condition check.
    /// </summary>
    [TestMethod]
    public void Info_BothConditionsFail_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            PluginLogger logger = new("TestPlugin") { MinLevel = LogLevel.Warning };

            // Act & Assert
            logger.Info("Test message");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info respects Logger suppression scopes.
    /// When Logger is suppressed, IsEnabled is false and the method should return early.
    /// </summary>
    [TestMethod]
    public void Info_LoggerSuppressed_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new("TestPlugin") { MinLevel = LogLevel.Info };

            using (Logger.Suppress())
            {
                // Act & Assert
                logger.Info("Test message");
            }
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info does not throw with special control characters in the message.
    /// The method should handle control characters gracefully.
    /// </summary>
    [TestMethod]
    public void Info_MessageWithControlCharacters_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new("TestPlugin") { MinLevel = LogLevel.Info };
            var messageWithControlChars = "Test\0message\u0001with\u001Fcontrol\u007Fchars";

            // Act & Assert
            logger.Info(messageWithControlChars);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info does not throw when prefix contains format specifiers.
    /// The FormatMessage method should handle the prefix format correctly.
    /// </summary>
    [TestMethod]
    public void Info_PrefixWithFormatSpecifiers_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            PluginLogger logger = new("TestPlugin")
            {
                PrefixFormat = "[{0}]",
                MinLevel = LogLevel.Info
            };

            // Act & Assert
            logger.Info("Test message");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Verifies that the parameterless constructor initializes all properties to their expected default values.
    /// Tests Prefix (null), PrefixFormat ("[{0}]"), and MinLevel (LogLevel.Info).
    /// </summary>
    [TestMethod]
    [DataRow(null, "[{0}]", LogLevel.Info, DisplayName = "Default values: Prefix=null, PrefixFormat=[{0}], MinLevel=Info")]
    public void PluginLogger_ParameterlessConstructor_InitializesPropertiesToDefaults(string? expectedPrefix, string expectedPrefixFormat, LogLevel expectedMinLevel)
    {
        // Act
        var logger = new PluginLogger();

        // Assert
        Assert.AreEqual(expectedPrefix, logger.Prefix);
        Assert.AreEqual(expectedPrefixFormat, logger.PrefixFormat);
        Assert.AreEqual(expectedMinLevel, logger.MinLevel);
    }

    /// <summary>
    /// Verifies that multiple instances created by the parameterless constructor are independent
    /// and each maintains its own property values without interference.
    /// </summary>
    [TestMethod]
    public void PluginLogger_ParameterlessConstructor_CreatesIndependentInstances()
    {
        // Act
        var logger1 = new PluginLogger();
        var logger2 = new PluginLogger();

        logger1.Prefix = "Plugin1";
        logger1.PrefixFormat = "[{0}]:";
        logger1.MinLevel = LogLevel.Warning;

        // Assert
        Assert.AreEqual("Plugin1", logger1.Prefix);
        Assert.AreEqual("[{0}]:", logger1.PrefixFormat);
        Assert.AreEqual(LogLevel.Warning, logger1.MinLevel);

        Assert.IsNull(logger2.Prefix);
        Assert.AreEqual("[{0}]", logger2.PrefixFormat);
        Assert.AreEqual(LogLevel.Info, logger2.MinLevel);
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> does not throw when global logging
    /// is disabled via <see cref="Logger.Enabled"/>.
    /// </summary>
    [TestMethod]
    public void Warning_WhenLoggerDisabled_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = global::Umbra.Logging.Logger.Enabled;
        try
        {
            global::Umbra.Logging.Logger.Enabled = false;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert
            logger.Warning("Test message");
        }
        finally
        {
            global::Umbra.Logging.Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> does not throw when <see cref="PluginLogger.MinLevel"/>
    /// is set to <see cref="LogLevel.Warning"/> and logging is enabled.
    /// </summary>
    /// <param name="message">The message to log.</param>
    [TestMethod]
    [DataRow("Normal message")]
    [DataRow("")]
    [DataRow("Very long message that exceeds typical buffer sizes and contains many characters to test boundary conditions in the logging system implementation")]
    [DataRow("Message with special chars: !@#$%^&*()")]
    [DataRow("Message\nwith\nnewlines")]
    [DataRow("Message\twith\ttabs")]
    [DataRow("Message with unicode: 你好世界 🎉")]
    public void Warning_WhenMinLevelIsWarningAndEnabledWithVariousMessages_DoesNotThrow(string message)
    {
        // Arrange
        var originalEnabled = global::Umbra.Logging.Logger.Enabled;
        try
        {
            global::Umbra.Logging.Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Warning
            };

            // Act & Assert
            logger.Warning(message);
        }
        finally
        {
            global::Umbra.Logging.Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> does not throw when the message is null,
    /// demonstrating exception-safe behavior.
    /// </summary>
    [TestMethod]
    public void Warning_WithNullMessage_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = global::Umbra.Logging.Logger.Enabled;
        try
        {
            global::Umbra.Logging.Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Warning
            };

            // Act & Assert
            logger.Warning(null!);
        }
        finally
        {
            global::Umbra.Logging.Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> correctly prepends the prefix when
    /// <see cref="PluginLogger.Prefix"/> is set.
    /// </summary>
    [TestMethod]
    public void Warning_WithPrefix_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = global::Umbra.Logging.Logger.Enabled;
        try
        {
            global::Umbra.Logging.Logger.Enabled = true;
            var logger = new PluginLogger("MyPlugin")
            {
                MinLevel = LogLevel.Warning
            };

            // Act & Assert
            logger.Warning("Test message");
        }
        finally
        {
            global::Umbra.Logging.Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> does not throw when
    /// <see cref="PluginLogger.Prefix"/> is null.
    /// </summary>
    [TestMethod]
    public void Warning_WithNullPrefix_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = global::Umbra.Logging.Logger.Enabled;
        try
        {
            global::Umbra.Logging.Logger.Enabled = true;
            var logger = new PluginLogger
            {
                Prefix = null,
                MinLevel = LogLevel.Warning
            };

            // Act & Assert
            logger.Warning("Test message");
        }
        finally
        {
            global::Umbra.Logging.Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> does not throw when
    /// <see cref="PluginLogger.Prefix"/> is an empty string.
    /// </summary>
    [TestMethod]
    public void Warning_WithEmptyPrefix_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = global::Umbra.Logging.Logger.Enabled;
        try
        {
            global::Umbra.Logging.Logger.Enabled = true;
            var logger = new PluginLogger
            {
                Prefix = string.Empty,
                MinLevel = LogLevel.Warning
            };

            // Act & Assert
            logger.Warning("Test message");
        }
        finally
        {
            global::Umbra.Logging.Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> does not throw when
    /// <see cref="PluginLogger.PrefixFormat"/> is set to a custom format.
    /// </summary>
    [TestMethod]
    public void Warning_WithCustomPrefixFormat_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = global::Umbra.Logging.Logger.Enabled;
        try
        {
            global::Umbra.Logging.Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                PrefixFormat = "({0})",
                MinLevel = LogLevel.Warning
            };

            // Act & Assert
            logger.Warning("Test message");
        }
        finally
        {
            global::Umbra.Logging.Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> does not throw even when
    /// <see cref="PluginLogger.PrefixFormat"/> contains an invalid format string,
    /// demonstrating exception-safe behavior where format exceptions are caught.
    /// </summary>
    [TestMethod]
    public void Warning_WithInvalidPrefixFormat_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = global::Umbra.Logging.Logger.Enabled;
        try
        {
            global::Umbra.Logging.Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                PrefixFormat = "{0} {1} {2}", // Format expects 3 args but only 1 provided
                MinLevel = LogLevel.Warning
            };

            // Act & Assert
            logger.Warning("Test message");
        }
        finally
        {
            global::Umbra.Logging.Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> does not throw when both
    /// message and prefix contain special formatting characters.
    /// </summary>
    [TestMethod]
    public void Warning_WithSpecialCharactersInMessageAndPrefix_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = global::Umbra.Logging.Logger.Enabled;
        try
        {
            global::Umbra.Logging.Logger.Enabled = true;
            var logger = new PluginLogger("Plugin{0}")
            {
                MinLevel = LogLevel.Warning
            };

            // Act & Assert
            logger.Warning("Message with {braces} and %specials%");
        }
        finally
        {
            global::Umbra.Logging.Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> does not throw when
    /// whitespace-only message is provided.
    /// </summary>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void Warning_WithWhitespaceOnlyMessage_DoesNotThrow(string message)
    {
        // Arrange
        var originalEnabled = global::Umbra.Logging.Logger.Enabled;
        try
        {
            global::Umbra.Logging.Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Warning
            };

            // Act & Assert
            logger.Warning(message);
        }
        finally
        {
            global::Umbra.Logging.Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> returns early without logging
    /// when <see cref="Logger.Enabled"/> is set to false.
    /// </summary>
    [TestMethod]
    public void Error_LoggerDisabled_ReturnsEarlyWithoutLogging()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act - should return early without calling API.LogError
            logger.Error("Test error message");

            // Assert - method completes without exception
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> returns early without logging
    /// when logging is suppressed via <see cref="Logger.Suppress"/>.
    /// </summary>
    [TestMethod]
    public void Error_LoggerSuppressed_ReturnsEarlyWithoutLogging()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            using (Logger.Suppress())
            {
                // Act - should return early due to suppression
                logger.Error("Test error message");
            }

            // Assert - method completes without exception
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> returns early without logging
    /// when <see cref="PluginLogger.MinLevel"/> is set to <see cref="LogLevel.None"/>,
    /// which is greater than <see cref="LogLevel.Error"/>.
    /// </summary>
    [TestMethod]
    public void Error_MinLevelNone_ReturnsEarlyWithoutLogging()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.None
            };

            // Act - should return early because None (3) > Error (2)
            logger.Error("Test error message");

            // Assert - method completes without exception
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> correctly uses a custom
    /// <see cref="PluginLogger.PrefixFormat"/> when formatting the message.
    /// </summary>
    [TestMethod]
    public void Error_WithCustomPrefixFormat_FormatsCorrectly()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                Prefix = "CustomPlugin",
                PrefixFormat = "<<{0}>>",
                MinLevel = LogLevel.Info
            };

            // Act - should format message as "<<CustomPlugin>> Test error message"
            logger.Error("Test error message");

            // Assert - method completes without exception
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> does not throw an exception
    /// when called with an empty message string.
    /// </summary>
    [TestMethod]
    public void Error_EmptyMessage_DoesNotThrow()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - should not throw
            logger.Error(string.Empty);
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> does not throw an exception
    /// when called with a whitespace-only message string.
    /// </summary>
    [TestMethod]
    public void Error_WhitespaceMessage_DoesNotThrow()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - should not throw
            logger.Error("   ");
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> does not throw an exception
    /// when called with a very long message string.
    /// </summary>
    [TestMethod]
    public void Error_LongMessage_DoesNotThrow()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var longMessage = new string('A', 10000);

            // Act & Assert - should not throw
            logger.Error(longMessage);
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> does not throw an exception
    /// when called with a message containing special characters.
    /// </summary>
    [TestMethod]
    public void Error_SpecialCharacters_DoesNotThrow()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - should not throw
            logger.Error("Test message with special chars: \n\r\t\0 © ™ ® § ¶");
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> does not throw an exception
    /// when called with various message and prefix combinations, demonstrating exception-safe behavior.
    /// </summary>
    /// <param name="prefix">The prefix value to test.</param>
    /// <param name="message">The message value to test.</param>
    /// <param name="minLevel">The minimum log level to test.</param>
    [TestMethod]
    [DataRow(null, "Test message", LogLevel.Info)]
    [DataRow("", "Test message", LogLevel.Info)]
    [DataRow("Prefix", "Test message", LogLevel.Info)]
    [DataRow("Prefix", "", LogLevel.Info)]
    [DataRow("Prefix", "Test message", LogLevel.Warning)]
    [DataRow("Prefix", "Test message", LogLevel.Error)]
    [DataRow("Prefix", "Test message", LogLevel.None)]
    [DataRow("VeryLongPrefixWithManyCharacters", "VeryLongMessageWithManyCharacters", LogLevel.Info)]
    public void Error_VariousCombinations_DoesNotThrow(string? prefix, string message, LogLevel minLevel)
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger
            {
                Prefix = prefix,
                MinLevel = minLevel
            };

            // Act & Assert - should not throw
            logger.Error(message);
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> swallows exceptions
    /// when an invalid <see cref="PluginLogger.PrefixFormat"/> causes string.Format to throw.
    /// </summary>
    [TestMethod]
    public void Error_InvalidPrefixFormat_SwallowsException()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                Prefix = "MyPlugin",
                PrefixFormat = "{1}", // Invalid: expects 2 arguments but only 1 (Prefix) is provided
                MinLevel = LogLevel.Info
            };

            // Act & Assert - should swallow FormatException and not throw
            logger.Error("Test error message");
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> continues to work correctly
    /// when multiple successive calls are made with different conditions.
    /// </summary>
    [TestMethod]
    public void Error_MultipleSuccessiveCalls_WorksCorrectly()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - multiple calls should all complete without exception
            logger.Error("First message");
            logger.Error("Second message");
            logger.Error("Third message");

            logger.MinLevel = LogLevel.None;
            logger.Error("Fourth message - should not log");

            logger.MinLevel = LogLevel.Error;
            logger.Error("Fifth message - should log");
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Verifies that <see cref="PluginLogger.Error(string)"/> respects changes to
    /// <see cref="Logger.Enabled"/> mid-execution across multiple calls.
    /// </summary>
    [TestMethod]
    public void Error_LoggerEnabledToggled_RespectsChanges()
    {
        // Arrange
        var originalEnabledState = Logger.Enabled;
        try
        {
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - toggle Logger.Enabled between calls
            Logger.Enabled = true;
            logger.Error("Message when enabled");

            Logger.Enabled = false;
            logger.Error("Message when disabled - should not log");

            Logger.Enabled = true;
            logger.Error("Message when re-enabled");
        }
        finally
        {
            Logger.Enabled = originalEnabledState;
        }
    }

    /// <summary>
    /// Tests that Exception with null exception parameter does not throw due to exception-safe design.
    /// Expected: Method completes without throwing even though accessing null.GetType() would fail.
    /// </summary>
    [TestMethod]
    public void Exception_NullException_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };

        // Act & Assert - should not throw
        logger.Exception(null!, "Test message");
    }

    /// <summary>
    /// Tests that Exception with null message parameter does not throw due to exception-safe design.
    /// Expected: Method completes without throwing even if FormatMessage encounters null.
    /// </summary>
    [TestMethod]
    public void Exception_NullMessage_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw
        logger.Exception(ex, null!);
    }

    /// <summary>
    /// Tests that Exception with both null parameters does not throw due to exception-safe design.
    /// Expected: Method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_NullExceptionAndNullMessage_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };

        // Act & Assert - should not throw
        logger.Exception(null!, null!);
    }

    /// <summary>
    /// Tests that Exception returns early when MinLevel is set above Error (None).
    /// Expected: Method returns immediately without processing the log message.
    /// </summary>
    [TestMethod]
    public void Exception_MinLevelNone_ReturnsEarly()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.None // Above Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act - should return early due to MinLevel check
        logger.Exception(ex, "Test message");

        // Assert - method completes (early return path taken)
        Assert.IsNotNull(logger);
    }

    /// <summary>
    /// Tests Exception with an empty message string.
    /// Expected: Method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_EmptyMessage_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw
        logger.Exception(ex, string.Empty);
    }

    /// <summary>
    /// Tests Exception with a whitespace-only message.
    /// Expected: Method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_WhitespaceMessage_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw
        logger.Exception(ex, "   ");
    }

    /// <summary>
    /// Tests Exception with a very long message string.
    /// Expected: Method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_VeryLongMessage_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");
        var longMessage = new string('A', 10000);

        // Act & Assert - should not throw
        logger.Exception(ex, longMessage);
    }

    /// <summary>
    /// Tests Exception with a message containing special characters.
    /// Expected: Method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_MessageWithSpecialCharacters_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw
        logger.Exception(ex, "Message with\nnewline\ttab\rcarriage return and \0null char");
    }

    /// <summary>
    /// Tests Exception with null Prefix (no prefix formatting).
    /// Expected: Method completes without throwing, message formatted without prefix.
    /// </summary>
    [TestMethod]
    public void Exception_NullPrefix_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger
        {
            Prefix = null,
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw
        logger.Exception(ex, "Test message");
    }

    /// <summary>
    /// Tests Exception with empty Prefix (no prefix formatting).
    /// Expected: Method completes without throwing, message formatted without prefix.
    /// </summary>
    [TestMethod]
    public void Exception_EmptyPrefix_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger
        {
            Prefix = string.Empty,
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw
        logger.Exception(ex, "Test message");
    }

    /// <summary>
    /// Tests Exception with a configured Prefix.
    /// Expected: Method completes without throwing, message formatted with prefix.
    /// </summary>
    [TestMethod]
    public void Exception_WithPrefix_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw
        logger.Exception(ex, "Test message");
    }

    /// <summary>
    /// Tests Exception with custom PrefixFormat.
    /// Expected: Method completes without throwing, respects custom format.
    /// </summary>
    [TestMethod]
    public void Exception_CustomPrefixFormat_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            PrefixFormat = "<<{0}>>",
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw
        logger.Exception(ex, "Test message");
    }

    /// <summary>
    /// Tests Exception with invalid PrefixFormat (exception-safe).
    /// Expected: Method completes without throwing due to try-catch.
    /// </summary>
    [TestMethod]
    public void Exception_InvalidPrefixFormat_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            PrefixFormat = "{0} {1} {2}", // Expects 3 args but only 1 provided
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw (exception is caught)
        logger.Exception(ex, "Test message");
    }

    /// <summary>
    /// Tests Exception with an exception that has null Message property.
    /// Expected: Method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_ExceptionWithNullMessage_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var ex = new CustomExceptionWithNullMessage();

        // Act & Assert - should not throw
        logger.Exception(ex, "Context message");
    }

    /// <summary>
    /// Tests Exception with an exception that has null StackTrace property.
    /// Expected: Method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_ExceptionWithNullStackTrace_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        // A newly created exception that hasn't been thrown has null StackTrace
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw
        logger.Exception(ex, "Context message");
    }

    /// <summary>
    /// Tests Exception with different exception types.
    /// Expected: Method completes without throwing for various exception types.
    /// </summary>
    [TestMethod]
    [DataRow(typeof(ArgumentException))]
    [DataRow(typeof(ArgumentNullException))]
    [DataRow(typeof(InvalidOperationException))]
    [DataRow(typeof(NotSupportedException))]
    [DataRow(typeof(FormatException))]
    [DataRow(typeof(DivideByZeroException))]
    public void Exception_VariousExceptionTypes_DoesNotThrow(Type exceptionType)
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var ex = (Exception)Activator.CreateInstance(exceptionType, "Test exception")!;

        // Act & Assert - should not throw
        logger.Exception(ex, "Context message");
    }

    /// <summary>
    /// Tests Exception with an exception that has a very long stack trace.
    /// Expected: Method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_ExceptionWithLongStackTrace_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        Exception? ex = null;
        try
        {
            GenerateDeepStackTrace(100);
        }
        catch (Exception caught)
        {
            ex = caught;
        }

        // Act & Assert - should not throw
        logger.Exception(ex!, "Context message");
    }

    /// <summary>
    /// Tests Exception with inner exceptions (nested).
    /// Expected: Method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_ExceptionWithInnerException_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var innerEx = new InvalidOperationException("Inner exception");
        var outerEx = new ApplicationException("Outer exception", innerEx);

        // Act & Assert - should not throw
        logger.Exception(outerEx, "Context message");
    }

    /// <summary>
    /// Tests that Exception is called multiple times without issues.
    /// Expected: Method can be called repeatedly without errors.
    /// </summary>
    [TestMethod]
    public void Exception_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw on repeated calls
        for (var i = 0; i < 10; i++)
        {
            logger.Exception(ex, $"Message {i}");
        }
    }

    /// <summary>
    /// Helper method to generate deep stack traces for testing.
    /// </summary>
    private static void GenerateDeepStackTrace(int depth)
    {
        if (depth <= 0)
        {
            throw new InvalidOperationException("Deep stack trace");
        }
        GenerateDeepStackTrace(depth - 1);
    }

    /// <summary>
    /// Helper exception class with null Message for testing edge cases.
    /// </summary>
    private sealed class CustomExceptionWithNullMessage : Exception
    {
        public override string Message => null!;
    }
    /// <summary>
    /// Stores the original Logger.Enabled value before each test.
    /// </summary>
    private bool _originalLoggerEnabled;

    /// <summary>
    /// Gets the in-memory sink installed for the current test.
    /// </summary>
    private static TestLogSink CurrentSink => (TestLogSink)Logger.GetLogSink();

    /// <summary>
    /// Restores the Logger.Enabled state before each test.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _originalLoggerEnabled = Logger.Enabled;
        Logger.SetLogSink(new TestLogSink());
        Logger.Enabled = true;
    }

    /// <summary>
    /// Restores the Logger.Enabled state after each test.
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        Logger.ResetLogSink();
        Logger.Enabled = _originalLoggerEnabled;
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Info(string)"/> writes a prefixed informational message
    /// to the active sink when logging is enabled.
    /// </summary>
    [TestMethod]
    public void Info_WithPrefix_WritesFormattedMessageToSink()
    {
        // Arrange
        var logger = new PluginLogger("MyPlugin")
        {
            MinLevel = LogLevel.Info
        };

        // Act
        logger.Info("Hello");

        // Assert
        Assert.HasCount(1, CurrentSink.InfoMessages);
        Assert.AreEqual("[MyPlugin] Hello", CurrentSink.InfoMessages[0]);
        Assert.IsEmpty(CurrentSink.WarningMessages);
        Assert.IsEmpty(CurrentSink.ErrorMessages);
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Warning(string)"/> writes a warning message using the
    /// configured prefix format.
    /// </summary>
    [TestMethod]
    public void Warning_WithCustomPrefixFormat_WritesFormattedMessageToSink()
    {
        // Arrange
        var logger = new PluginLogger("MyPlugin")
        {
            PrefixFormat = "<<{0}>>",
            MinLevel = LogLevel.Warning
        };

        // Act
        logger.Warning("Careful");

        // Assert
        Assert.IsEmpty(CurrentSink.InfoMessages);
        Assert.HasCount(1, CurrentSink.WarningMessages);
        Assert.AreEqual("<<MyPlugin>> Careful", CurrentSink.WarningMessages[0]);
        Assert.IsEmpty(CurrentSink.ErrorMessages);
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Error(string)"/> does not write to the sink when the
    /// minimum level filters out error logging.
    /// </summary>
    [TestMethod]
    public void Error_WhenMinLevelIsNone_DoesNotWriteToSink()
    {
        // Arrange
        var logger = new PluginLogger("MyPlugin")
        {
            MinLevel = LogLevel.None
        };

        // Act
        logger.Error("Hidden");

        // Assert
        Assert.IsEmpty(CurrentSink.InfoMessages);
        Assert.IsEmpty(CurrentSink.WarningMessages);
        Assert.IsEmpty(CurrentSink.ErrorMessages);
    }

    /// <summary>
    /// Tests that <see cref="PluginLogger.Exception(Exception, string)"/> writes a prefixed error
    /// message and exception details to the active sink.
    /// </summary>
    [TestMethod]
    public void Exception_WithPrefix_WritesExceptionDetailsToErrorSink()
    {
        // Arrange
        var logger = new PluginLogger("MyPlugin")
        {
            MinLevel = LogLevel.Error
        };
        var exception = new InvalidOperationException("Boom");

        // Act
        logger.Exception(exception, "Context");

        // Assert
        Assert.IsEmpty(CurrentSink.InfoMessages);
        Assert.IsEmpty(CurrentSink.WarningMessages);
        Assert.HasCount(1, CurrentSink.ErrorMessages);
        Assert.Contains("[MyPlugin] Context", CurrentSink.ErrorMessages[0]);
        Assert.Contains("InvalidOperationException: Boom", CurrentSink.ErrorMessages[0]);
        Assert.Contains("Stack Trace:", CurrentSink.ErrorMessages[0]);
    }

    /// <summary>
    /// Tests that Error with format and args returns early without throwing
    /// when Logger.IsEnabled is false.
    /// </summary>
    [TestMethod]
    public void Error_LoggerDisabled_ReturnsEarlyWithoutException()
    {
        // Arrange
        Logger.Enabled = false;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw
        logger.Error("Test {0}", "arg");
    }

    /// <summary>
    /// Tests that Error with format and args returns early without throwing
    /// when MinLevel is set to None (higher than Error).
    /// </summary>
    [TestMethod]
    public void Error_MinLevelNone_ReturnsEarlyWithoutException()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.None
        };

        // Act & Assert - should not throw
        logger.Error("Test {0}", "arg");
    }

    /// <summary>
    /// Tests that Error with format and args completes without throwing
    /// when Logger is enabled and MinLevel allows error logging.
    /// </summary>
    [TestMethod]
    public void Error_ValidConditions_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Error
        };

        // Act & Assert - should not throw
        logger.Error("Test message: {0}", "value");
    }

    /// <summary>
    /// Tests that Error with format and args handles null format string
    /// by catching the exception and returning without throwing.
    /// </summary>
    [TestMethod]
    public void Error_NullFormat_SuppressesException()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw despite null format
        logger.Error(null!, new object[] { "arg" });
    }

    /// <summary>
    /// Tests that Error with format and args handles null args array
    /// by catching the exception and returning without throwing.
    /// </summary>
    [TestMethod]
    public void Error_NullArgs_SuppressesException()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw despite null args
        logger.Error("Test {0}", null!);
    }

    /// <summary>
    /// Tests that Error with format and args handles empty format string
    /// without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Error_EmptyFormat_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw
        logger.Error(string.Empty);
    }

    /// <summary>
    /// Tests that Error with format and args handles format string with no placeholders
    /// but with args provided, completing without throwing.
    /// </summary>
    [TestMethod]
    public void Error_NoPlaceholdersWithArgs_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw, extra args are ignored
        logger.Error("Test message", "unused", "args");
    }

    /// <summary>
    /// Tests that Error with format and args suppresses FormatException
    /// when format string has more placeholders than provided arguments.
    /// </summary>
    [TestMethod]
    public void Error_TooFewArgs_SuppressesFormatException()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw despite format mismatch
        logger.Error("Test {0} {1} {2}", "arg1");
    }

    /// <summary>
    /// Tests that Error with format and args handles more arguments than placeholders
    /// without throwing (extra args are ignored by string.Format).
    /// </summary>
    [TestMethod]
    public void Error_TooManyArgs_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw, extra args are ignored
        logger.Error("Test {0}", "arg1", "arg2", "arg3");
    }

    /// <summary>
    /// Tests that Error with format and args suppresses FormatException
    /// when format string has invalid syntax.
    /// </summary>
    [TestMethod]
    public void Error_InvalidFormatSyntax_SuppressesFormatException()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw despite invalid format
        logger.Error("Test {0", "arg");
    }

    /// <summary>
    /// Tests that Error with format and args works correctly with valid format
    /// and matching arguments when MinLevel is Info (lowest level).
    /// </summary>
    [TestMethod]
    public void Error_ValidFormatWithMatchingArgs_MinLevelInfo_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw
        logger.Error("Error code: {0}, Message: {1}", 404, "Not Found");
    }

    /// <summary>
    /// Tests that Error with format and args works correctly with valid format
    /// and matching arguments when MinLevel is Warning.
    /// </summary>
    [TestMethod]
    public void Error_ValidFormatWithMatchingArgs_MinLevelWarning_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Warning
        };

        // Act & Assert - should not throw
        logger.Error("Error: {0}", "Something went wrong");
    }

    /// <summary>
    /// Tests that Error with format and args works correctly with valid format
    /// and matching arguments when MinLevel is exactly Error.
    /// </summary>
    [TestMethod]
    public void Error_ValidFormatWithMatchingArgs_MinLevelError_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Error
        };

        // Act & Assert - should not throw
        logger.Error("Critical error: {0}", "System failure");
    }

    /// <summary>
    /// Tests that Error with format and args suppresses exceptions
    /// when args contain null values.
    /// </summary>
    [TestMethod]
    public void Error_ArgsContainNull_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw, null is formatted as empty string
        logger.Error("Value: {0}", (object?)null!);
    }

    /// <summary>
    /// Tests that Error with format and args handles format string with special characters
    /// without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Error_FormatWithSpecialCharacters_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw
        logger.Error("Error: {0} - Line: {1}", "File not found", 42);
    }

    /// <summary>
    /// Tests that Error with format and args handles very long format strings
    /// without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Error_VeryLongFormatString_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };
        var longFormat = new string('x', 10000) + " {0}";

        // Act & Assert - should not throw
        logger.Error(longFormat, "end");
    }

    /// <summary>
    /// Tests that Error with format and args handles empty args array
    /// when format has no placeholders without throwing.
    /// </summary>
    [TestMethod]
    public void Error_EmptyArgsArray_NoPlaceholders_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw
        logger.Error("Simple message", Array.Empty<object>());
    }

    /// <summary>
    /// Tests that Error with format and args suppresses FormatException
    /// when format string has negative index placeholder.
    /// </summary>
    [TestMethod]
    public void Error_NegativeIndexPlaceholder_SuppressesFormatException()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw despite invalid placeholder
        logger.Error("Test {-1}", "arg");
    }

    /// <summary>
    /// Tests that Error with format and args handles format with escaped braces
    /// without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Error_EscapedBraces_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw
        logger.Error("Object: {{ {0} }}", "value");
    }

    /// <summary>
    /// Tests that Error with format and args handles multiple format placeholders
    /// with matching arguments without throwing.
    /// </summary>
    [TestMethod]
    public void Error_MultiplePlaceholders_MatchingArgs_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        PluginLogger logger = new()
        {
            MinLevel = LogLevel.Info
        };

        // Act & Assert - should not throw
        logger.Error("Values: {0}, {1}, {2}, {3}, {4}", 1, 2, 3, 4, 5);
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) returns early when Logger.IsEnabled is false.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_WhenLoggerDisabled_ReturnsWithoutLogging()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act
            logger.Exception(exception, "Format: {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) returns early when MinLevel is greater than Error.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_WhenMinLevelAboveError_ReturnsWithoutLogging()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.None
            };
            var exception = new InvalidOperationException("Test exception");

            // Act
            logger.Exception(exception, "Format: {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) returns early when both Logger.IsEnabled is false and MinLevel is above Error.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_WhenBothDisabledAndMinLevelAboveError_ReturnsWithoutLogging()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.None
            };
            var exception = new InvalidOperationException("Test exception");

            // Act
            logger.Exception(exception, "Format: {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles null format string gracefully without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_NullFormatString_ReturnsSilentlyWithoutException()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act
            logger.Exception(exception, null!, "arg1", "arg2");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles mismatched format arguments gracefully without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_MismatchedFormatArguments_ReturnsSilentlyWithoutException()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - format string expects 2 args but only 1 provided
            logger.Exception(exception, "Format: {0} {1}", "value1");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles invalid format string gracefully without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_InvalidFormatString_ReturnsSilentlyWithoutException()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - invalid format specifier
            logger.Exception(exception, "Format: {0:INVALID}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles too many format arguments without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_TooManyArguments_FormatsCorrectlyWithoutException()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - format string expects 1 arg but 3 provided (extra args ignored by string.Format)
            logger.Exception(exception, "Format: {0}", "value1", "value2", "value3");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles empty args array correctly.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_EmptyArgsArray_FormatsCorrectlyWithoutException()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - no format placeholders, empty args
            logger.Exception(exception, "No placeholders");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles null args array gracefully.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_NullArgsArray_ReturnsSilentlyWithoutException()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - null args array
            logger.Exception(exception, "Format: {0}", null!);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles null exception gracefully.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_NullException_HandlesGracefullyWithoutException()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act - null exception (the overload will handle it)
            logger.Exception(null!, "Format: {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) formats message correctly with valid format string and args.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_ValidFormatAndArgs_FormatsAndLogsCorrectly()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - valid format with args
            logger.Exception(exception, "Error occurred: {0} at {1}", "TestOperation", DateTime.Now.ToString());
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) works with empty format string.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_EmptyFormatString_HandlesCorrectly()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - empty format string
            logger.Exception(exception, string.Empty, "arg1");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) works with whitespace-only format string.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_WhitespaceFormatString_HandlesCorrectly()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - whitespace format string
            logger.Exception(exception, "   ", "arg1");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles args containing null values.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_ArgsContainingNull_FormatsCorrectly()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - args containing null
            logger.Exception(exception, "Value: {0}, Null: {1}", "test", null!);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) honors MinLevel when set to Info.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_MinLevelInfo_LogsException()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act
            logger.Exception(exception, "Format: {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles format string with special characters.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_FormatStringWithSpecialCharacters_FormatsCorrectly()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - format string with special chars
            logger.Exception(exception, "Error\n\t{0}: {1}", "Code", 404);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles very long format string.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_VeryLongFormatString_HandlesCorrectly()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");
            var longFormat = new string('A', 10000) + " {0}";

            // Act
            logger.Exception(exception, longFormat, "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles complex exception types.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_ComplexExceptionType_HandlesCorrectly()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var innerException = new InvalidOperationException("Inner exception");
            var exception = new AggregateException("Aggregate exception", innerException);

            // Act
            logger.Exception(exception, "Error: {0}", "TestError");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles multiple format placeholders correctly.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_MultipleFormatPlaceholders_FormatsCorrectly()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act
            logger.Exception(exception, "{0} {1} {2} {3} {4}", "One", "Two", "Three", "Four", "Five");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Exception(Exception, string, params object[]) handles format string with braces that don't match placeholder pattern.
    /// </summary>
    [TestMethod]
    public void Exception_WithFormatArgs_FormatStringWithUnmatchedBraces_HandlesGracefully()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };
            var exception = new InvalidOperationException("Test exception");

            // Act - unmatched braces (should cause string.Format to throw)
            logger.Exception(exception, "Error: {0} extra {", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning with valid format and args does not throw when logger is enabled and MinLevel allows warnings.
    /// </summary>
    [TestMethod]
    public void Warning_ValidFormatAndArgs_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - should not throw
            logger.Warning("Test message: {0}", "value");
            logger.Warning("Multiple args: {0}, {1}, {2}", 1, "two", 3.0);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning returns early without throwing when Logger.Enabled is false.
    /// </summary>
    [TestMethod]
    public void Warning_LoggerDisabled_ReturnsEarlyWithoutThrowing()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - should return early, no exception
            logger.Warning("Test message: {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning returns early without throwing when MinLevel is Error (greater than Warning).
    /// </summary>
    [TestMethod]
    public void Warning_MinLevelError_ReturnsEarlyWithoutThrowing()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Error
            };

            // Act & Assert - should return early due to MinLevel filtering, no exception
            logger.Warning("Test message: {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning returns early without throwing when MinLevel is None (greater than Warning).
    /// </summary>
    [TestMethod]
    public void Warning_MinLevelNone_ReturnsEarlyWithoutThrowing()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.None
            };

            // Act & Assert - should return early due to MinLevel filtering, no exception
            logger.Warning("Test message: {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning does not throw when MinLevel is Warning (equal to the log level).
    /// </summary>
    [TestMethod]
    public void Warning_MinLevelWarning_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Warning
            };

            // Act & Assert - MinLevel equals Warning, should proceed and not throw
            logger.Warning("Test message: {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning does not throw when MinLevel is Info (less than Warning).
    /// </summary>
    [TestMethod]
    public void Warning_MinLevelInfo_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - MinLevel is Info, should proceed and not throw
            logger.Warning("Test message: {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning handles invalid format string gracefully without throwing.
    /// </summary>
    [TestMethod]
    public void Warning_InvalidFormatString_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - invalid format strings should be caught internally
            logger.Warning("{0", "value");
            logger.Warning("{{0}}", "value");
            logger.Warning("{0:}", "value");
            logger.Warning("{1}", "value"); // format expects arg at index 1, only provided index 0
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning handles mismatched arguments (too few) gracefully without throwing.
    /// </summary>
    [TestMethod]
    public void Warning_TooFewArguments_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - too few args should be caught internally
            logger.Warning("Test {0} {1} {2}", "arg1");
            logger.Warning("{0} {1}", "single");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning handles too many arguments gracefully without throwing.
    /// </summary>
    [TestMethod]
    public void Warning_TooManyArguments_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - extra args are ignored by string.Format
            logger.Warning("Test {0}", "arg1", "arg2", "arg3");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning handles null format string gracefully without throwing.
    /// </summary>
    [TestMethod]
    public void Warning_NullFormatString_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - null format should be caught internally
            logger.Warning(null!, "arg1");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning handles null args array gracefully without throwing.
    /// </summary>
    [TestMethod]
    public void Warning_NullArgsArray_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - null args array should be handled
            logger.Warning("Test message", null!);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning handles empty args array with format string that has no placeholders.
    /// </summary>
    [TestMethod]
    public void Warning_EmptyArgsWithNoPlaceholders_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert
            logger.Warning("Test message with no placeholders");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning handles null elements in args array gracefully.
    /// </summary>
    [TestMethod]
    public void Warning_NullElementsInArgs_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - null elements should format as empty string
            logger.Warning("Test {0} {1}", null!, "value");
            logger.Warning("Test {0}", (object?)null!);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning handles various argument types correctly.
    /// </summary>
    [TestMethod]
    public void Warning_VariousArgumentTypes_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - various types should be formatted correctly
            logger.Warning("Int: {0}, Double: {1}, Bool: {2}", 42, 3.14, true);
            logger.Warning("String: {0}, Char: {1}", "text", 'c');
            logger.Warning("Object: {0}", new object());
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning handles format strings with special characters.
    /// </summary>
    [TestMethod]
    public void Warning_FormatWithSpecialCharacters_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - escaped braces and special chars
            logger.Warning("Test {{escaped}} {0}", "value");
            logger.Warning("Newline\n{0}\tTab", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning is suppressed when both Logger is disabled and MinLevel filters.
    /// </summary>
    [TestMethod]
    public void Warning_LoggerDisabledAndMinLevelFilters_ReturnsEarly()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Error
            };

            // Act & Assert - both conditions prevent logging
            logger.Warning("Test {0}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning with empty format string and no args does not throw.
    /// </summary>
    [TestMethod]
    public void Warning_EmptyFormatString_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert
            logger.Warning(string.Empty);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning with whitespace-only format string does not throw.
    /// </summary>
    [TestMethod]
    public void Warning_WhitespaceFormatString_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert
            logger.Warning("   ", "value");
            logger.Warning("\t\n\r", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Warning with format string containing only braces does not throw.
    /// </summary>
    [TestMethod]
    public void Warning_FormatStringOnlyBraces_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert - invalid format should be caught
            logger.Warning("{", "value");
            logger.Warning("}", "value");
            logger.Warning("{}", "value");
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests Warning with format specifiers for different numeric types.
    /// </summary>
    [TestMethod]
    public void Warning_NumericFormatSpecifiers_DoesNotThrow()
    {
        // Arrange
        var originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            var logger = new PluginLogger("TestPlugin")
            {
                MinLevel = LogLevel.Info
            };

            // Act & Assert
            logger.Warning("Decimal: {0:D}", 42);
            logger.Warning("Hex: {0:X}", 255);
            logger.Warning("Fixed: {0:F2}", 3.14159);
            logger.Warning("Currency: {0:C}", 1234.56);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that an informational log with a null prefix does not introduce a leading prefix or space.
    /// </summary>
    [TestMethod]
    public void Info_WithNullPrefix_WritesMessageWithoutLeadingWhitespace()
    {
        var logger = new PluginLogger
        {
            Prefix = null,
            MinLevel = LogLevel.Info
        };

        logger.Info("Hello");

        Assert.HasCount(1, CurrentSink.InfoMessages);
        Assert.AreEqual("Hello", CurrentSink.InfoMessages[0]);
    }

    /// <summary>
    /// Tests that an informational log with an empty prefix does not introduce a leading prefix or space.
    /// </summary>
    [TestMethod]
    public void Info_WithEmptyPrefix_WritesMessageWithoutLeadingWhitespace()
    {
        var logger = new PluginLogger
        {
            Prefix = string.Empty,
            MinLevel = LogLevel.Info
        };

        logger.Info("Hello");

        Assert.HasCount(1, CurrentSink.InfoMessages);
        Assert.AreEqual("Hello", CurrentSink.InfoMessages[0]);
    }

    /// <summary>
    /// Tests that separate logger instances keep prefix and minimum-level configuration isolated.
    /// </summary>
    [TestMethod]
    public void MultipleInstances_KeepPrefixAndMinLevelIsolated()
    {
        var infoLogger = new PluginLogger("InfoPlugin")
        {
            MinLevel = LogLevel.Info
        };
        var warningLogger = new PluginLogger("WarningPlugin")
        {
            MinLevel = LogLevel.Warning
        };

        infoLogger.Info("Info");
        warningLogger.Info("Hidden");
        warningLogger.Warning("Warning");

        Assert.HasCount(1, CurrentSink.InfoMessages);
        Assert.AreEqual("[InfoPlugin] Info", CurrentSink.InfoMessages[0]);
        Assert.HasCount(1, CurrentSink.WarningMessages);
        Assert.AreEqual("[WarningPlugin] Warning", CurrentSink.WarningMessages[0]);
        Assert.IsEmpty(CurrentSink.ErrorMessages);
    }

    /// <summary>
    /// Tests that informational logging remains exception-safe when the underlying sink throws.
    /// </summary>
    [TestMethod]
    public void Info_WhenSinkThrows_DoesNotThrow()
    {
        Logger.SetLogSink(new ThrowingLogSink());
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        logger.Info("Hello");
    }

    /// <summary>
    /// Tests that exception logging remains exception-safe when the underlying sink throws.
    /// </summary>
    [TestMethod]
    public void Exception_WhenSinkThrows_DoesNotThrow()
    {
        Logger.SetLogSink(new ThrowingLogSink());
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Error
        };

        logger.Exception(new InvalidOperationException("Boom"), "Context");
    }

    /// <summary>
    /// Tests that formatted informational logging swallows exceptions thrown while formatting arguments.
    /// </summary>
    [TestMethod]
    public void Info_WhenArgumentToStringThrows_DoesNotThrow()
    {
        var logger = new PluginLogger("TestPlugin")
        {
            MinLevel = LogLevel.Info
        };

        logger.Info("Value: {0}", new ThrowingToStringValue());
    }

    /// <summary>
    /// Sink that throws for every write path.
    /// </summary>
    private sealed class ThrowingLogSink : ILogSink
    {
        public void Info(string message) => throw new InvalidOperationException("sink failed");

        public void Warning(string message) => throw new InvalidOperationException("sink failed");

        public void Error(string message) => throw new InvalidOperationException("sink failed");
    }

    /// <summary>
    /// Value whose <see cref="object.ToString"/> implementation throws.
    /// </summary>
    private sealed class ThrowingToStringValue
    {
        public override string ToString() => throw new InvalidOperationException("ToString failed");
    }
}
