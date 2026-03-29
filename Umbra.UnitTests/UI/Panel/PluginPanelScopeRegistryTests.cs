using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra.UI.Panel;

namespace Umbra.UI.Panel.UnitTests;


/// <summary>
/// Unit tests for <see cref="PluginPanelScopeRegistry.Release"/> method.
/// </summary>
[TestClass]
public partial class PluginPanelScopeRegistryTests
{
    /// <summary>
    /// Tests that Release successfully removes a previously registered scope and allows it to be re-registered.
    /// </summary>
    [TestMethod]
    public void Release_RegisteredScope_AllowsReRegistration()
    {
        // Arrange
        string idScope = $"TestScope_{Guid.NewGuid()}";
        bool firstRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);
        bool secondRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Assert
        Assert.IsTrue(firstRegister, "First registration should succeed.");
        Assert.IsTrue(secondRegister, "Second registration after release should succeed.");

        // Cleanup
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release re-arms duplicate scope warnings by clearing the warned-duplicate tracking state.
    /// </summary>
    [TestMethod]
    public void Release_DuplicateScope_ReArmsDuplicateWarning()
    {
        // Arrange
        string idScope = $"TestScope_{Guid.NewGuid()}";
        bool firstRegister = PluginPanelScopeRegistry.TryRegister(idScope);
        bool duplicateRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);
        bool thirdRegister = PluginPanelScopeRegistry.TryRegister(idScope);
        bool fourthRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Assert
        Assert.IsTrue(firstRegister, "First registration should succeed.");
        Assert.IsFalse(duplicateRegister, "Duplicate registration should fail.");
        Assert.IsTrue(thirdRegister, "Registration after release should succeed.");
        Assert.IsFalse(fourthRegister, "Duplicate registration after re-registration should fail.");

        // Cleanup
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release does not throw when called with a scope that was never registered.
    /// </summary>
    [TestMethod]
    public void Release_NonExistentScope_DoesNotThrow()
    {
        // Arrange
        string idScope = $"NonExistent_{Guid.NewGuid()}";

        // Act & Assert
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release can be called multiple times on the same scope without throwing.
    /// </summary>
    [TestMethod]
    public void Release_AlreadyReleasedScope_DoesNotThrow()
    {
        // Arrange
        string idScope = $"TestScope_{Guid.NewGuid()}";
        PluginPanelScopeRegistry.TryRegister(idScope);
        PluginPanelScopeRegistry.Release(idScope);

        // Act & Assert
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release handles empty string scope correctly.
    /// </summary>
    [TestMethod]
    public void Release_EmptyString_DoesNotThrow()
    {
        // Arrange
        string idScope = string.Empty;
        PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);
        bool reRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Assert
        Assert.IsTrue(reRegister, "Should allow re-registration after release.");

        // Cleanup
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release handles whitespace-only string scopes correctly.
    /// </summary>
    /// <param name="idScope">The whitespace string to test.</param>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void Release_WhitespaceString_DoesNotThrow(string idScope)
    {
        // Arrange
        PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);
        bool reRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Assert
        Assert.IsTrue(reRegister, "Should allow re-registration after release.");

        // Cleanup
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release handles very long scope strings correctly.
    /// </summary>
    [TestMethod]
    public void Release_VeryLongString_DoesNotThrow()
    {
        // Arrange
        string idScope = new string('a', 10000);
        PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);
        bool reRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Assert
        Assert.IsTrue(reRegister, "Should allow re-registration after release.");

        // Cleanup
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release handles scope strings with special characters correctly.
    /// </summary>
    /// <param name="idScope">The scope string with special characters to test.</param>
    [TestMethod]
    [DataRow("!@#$%^&*()")]
    [DataRow("plugin:test")]
    [DataRow("plugin/panel")]
    [DataRow("plugin\\panel")]
    [DataRow("plugin.panel")]
    [DataRow("plugin-panel")]
    [DataRow("plugin_panel")]
    [DataRow("<PluginPanel>")]
    [DataRow("[PluginPanel]")]
    public void Release_SpecialCharacters_DoesNotThrow(string idScope)
    {
        // Arrange
        PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);
        bool reRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Assert
        Assert.IsTrue(reRegister, "Should allow re-registration after release.");

        // Cleanup
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release handles scope strings with unicode characters correctly.
    /// </summary>
    /// <param name="idScope">The scope string with unicode characters to test.</param>
    [TestMethod]
    [DataRow("测试")]
    [DataRow("プラグイン")]
    [DataRow("🎮")]
    [DataRow("Плагин")]
    public void Release_UnicodeCharacters_DoesNotThrow(string idScope)
    {
        // Arrange
        PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);
        bool reRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Assert
        Assert.IsTrue(reRegister, "Should allow re-registration after release.");

        // Cleanup
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release is thread-safe when called concurrently from multiple threads.
    /// </summary>
    [TestMethod]
    public void Release_ConcurrentCalls_IsThreadSafe()
    {
        // Arrange
        const int threadCount = 10;
        string[] scopes = new string[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            scopes[i] = $"TestScope_{Guid.NewGuid()}";
            PluginPanelScopeRegistry.TryRegister(scopes[i]);
        }

        // Act
        System.Threading.Tasks.Parallel.For(0, threadCount, i =>
        {
            PluginPanelScopeRegistry.Release(scopes[i]);
        });

        // Assert
        for (int i = 0; i < threadCount; i++)
        {
            bool canReRegister = PluginPanelScopeRegistry.TryRegister(scopes[i]);
            Assert.IsTrue(canReRegister, $"Scope {scopes[i]} should be successfully released and re-registerable.");
            PluginPanelScopeRegistry.Release(scopes[i]);
        }
    }

    /// <summary>
    /// Tests that Release handles control characters in scope strings correctly.
    /// </summary>
    [TestMethod]
    public void Release_ControlCharacters_DoesNotThrow()
    {
        // Arrange
        string idScope = "scope\0with\u0001control\u0002chars";
        PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);
        bool reRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Assert
        Assert.IsTrue(reRegister, "Should allow re-registration after release.");

        // Cleanup
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release correctly removes scope from both internal collections.
    /// </summary>
    [TestMethod]
    public void Release_RegisteredAndWarnedScope_RemovesFromBothCollections()
    {
        // Arrange
        string idScope = $"TestScope_{Guid.NewGuid()}";
        PluginPanelScopeRegistry.TryRegister(idScope);
        PluginPanelScopeRegistry.TryRegister(idScope); // Trigger warning tracking

        // Act
        PluginPanelScopeRegistry.Release(idScope);

        // Assert
        bool canReRegister = PluginPanelScopeRegistry.TryRegister(idScope);
        Assert.IsTrue(canReRegister, "Scope should be completely released and re-registerable.");

        // Cleanup
        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Verifies that registering a new unique scope returns true, indicating successful registration.
    /// Input: A unique scope string.
    /// Expected: Returns true.
    /// </summary>
    [TestMethod]
    public void TryRegister_NewUniqueScope_ReturnsTrue()
    {
        // Arrange
        string uniqueScope = $"UniqueScope_{Guid.NewGuid()}";

        // Act
        bool result = PluginPanelScopeRegistry.TryRegister(uniqueScope);

        // Assert
        Assert.IsTrue(result, "First registration of a unique scope should return true.");
    }

    /// <summary>
    /// Verifies that registering a duplicate scope returns false on the second attempt.
    /// Input: Same scope string registered twice.
    /// Expected: First call returns true, second call returns false.
    /// </summary>
    [TestMethod]
    public void TryRegister_DuplicateScope_SecondRegistrationReturnsFalse()
    {
        // Arrange
        string scope = $"DuplicateScope_{Guid.NewGuid()}";

        // Act
        bool firstResult = PluginPanelScopeRegistry.TryRegister(scope);
        bool secondResult = PluginPanelScopeRegistry.TryRegister(scope);

        // Assert
        Assert.IsTrue(firstResult, "First registration should return true.");
        Assert.IsFalse(secondResult, "Second registration of the same scope should return false.");
    }

    /// <summary>
    /// Verifies that registering the same scope three times returns false for the second and third attempts.
    /// Input: Same scope string registered three times.
    /// Expected: First call returns true, second and third calls return false.
    /// </summary>
    [TestMethod]
    public void TryRegister_ThirdDuplicateRegistration_ReturnsFalse()
    {
        // Arrange
        string scope = $"TripleDuplicateScope_{Guid.NewGuid()}";

        // Act
        bool firstResult = PluginPanelScopeRegistry.TryRegister(scope);
        bool secondResult = PluginPanelScopeRegistry.TryRegister(scope);
        bool thirdResult = PluginPanelScopeRegistry.TryRegister(scope);

        // Assert
        Assert.IsTrue(firstResult, "First registration should return true.");
        Assert.IsFalse(secondResult, "Second registration should return false.");
        Assert.IsFalse(thirdResult, "Third registration should return false.");
    }

    /// <summary>
    /// Verifies that multiple different scopes can be registered independently, each returning true.
    /// Input: Multiple unique scope strings.
    /// Expected: Each registration returns true.
    /// </summary>
    [TestMethod]
    public void TryRegister_MultipleDifferentScopes_EachReturnsTrue()
    {
        // Arrange
        string scope1 = $"MultiScope1_{Guid.NewGuid()}";
        string scope2 = $"MultiScope2_{Guid.NewGuid()}";
        string scope3 = $"MultiScope3_{Guid.NewGuid()}";

        // Act
        bool result1 = PluginPanelScopeRegistry.TryRegister(scope1);
        bool result2 = PluginPanelScopeRegistry.TryRegister(scope2);
        bool result3 = PluginPanelScopeRegistry.TryRegister(scope3);

        // Assert
        Assert.IsTrue(result1, "First unique scope should register successfully.");
        Assert.IsTrue(result2, "Second unique scope should register successfully.");
        Assert.IsTrue(result3, "Third unique scope should register successfully.");
    }

    /// <summary>
    /// Verifies that an empty string can be registered as a valid scope.
    /// Input: Empty string.
    /// Expected: First call returns true, second call returns false.
    /// </summary>
    [TestMethod]
    public void TryRegister_EmptyString_ReturnsTrue()
    {
        // Arrange
        string emptyScope = string.Empty;

        // Act
        bool firstResult = PluginPanelScopeRegistry.TryRegister(emptyScope);
        bool secondResult = PluginPanelScopeRegistry.TryRegister(emptyScope);

        // Assert
        Assert.IsTrue(firstResult, "First registration of empty string should return true.");
        Assert.IsFalse(secondResult, "Duplicate registration of empty string should return false.");
    }

    /// <summary>
    /// Verifies that whitespace-only strings can be registered as valid scopes.
    /// Input: Whitespace-only strings (single space, multiple spaces, tab, newline).
    /// Expected: Each unique whitespace string registers successfully on first attempt.
    /// </summary>
    [TestMethod]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void TryRegister_WhitespaceString_ReturnsTrue(string whitespaceScope)
    {
        // Arrange
        // Use unique whitespace combined with GUID to avoid cross-test pollution
        string uniqueWhitespace = whitespaceScope + Guid.NewGuid().ToString();

        // Act
        bool result = PluginPanelScopeRegistry.TryRegister(uniqueWhitespace);

        // Assert
        Assert.IsTrue(result, $"Whitespace string '{whitespaceScope}' should register successfully.");
    }

    /// <summary>
    /// Verifies that very long strings can be registered as valid scopes without errors.
    /// Input: String with 10,000 characters.
    /// Expected: Returns true on first registration.
    /// </summary>
    [TestMethod]
    public void TryRegister_VeryLongString_ReturnsTrue()
    {
        // Arrange
        string longScope = new string('A', 10000) + Guid.NewGuid().ToString();

        // Act
        bool result = PluginPanelScopeRegistry.TryRegister(longScope);

        // Assert
        Assert.IsTrue(result, "Very long string should register successfully.");
    }

    /// <summary>
    /// Verifies that strings with special characters can be registered as valid scopes.
    /// Input: Strings containing special characters.
    /// Expected: Each registers successfully on first attempt.
    /// </summary>
    [TestMethod]
    [DataRow("Scope@#$%")]
    [DataRow("Scope<>?")]
    [DataRow("Scope|\\/:*")]
    [DataRow("Scope\"'`")]
    public void TryRegister_SpecialCharacters_ReturnsTrue(string specialScope)
    {
        // Arrange
        string uniqueSpecial = specialScope + Guid.NewGuid().ToString();

        // Act
        bool result = PluginPanelScopeRegistry.TryRegister(uniqueSpecial);

        // Assert
        Assert.IsTrue(result, $"Scope with special characters '{specialScope}' should register successfully.");
    }

    /// <summary>
    /// Verifies that null input is handled without throwing an exception.
    /// Input: null string.
    /// Expected: Method completes without exception; return value depends on HashSet&lt;string&gt; behavior with null.
    /// </summary>
    [TestMethod]
    public void TryRegister_NullScope_DoesNotThrow()
    {
        // Arrange
        string? nullScope = null;

        // Act & Assert
        try
        {
            bool result = PluginPanelScopeRegistry.TryRegister(nullScope!);
            // HashSet<string> can store null, so first call should return true
            Assert.IsTrue(result, "First registration of null should return true.");
        }
        catch (Exception ex)
        {
            Assert.Fail($"TryRegister should not throw exception for null input, but threw: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifies that duplicate null registrations are detected correctly.
    /// Input: null string registered twice.
    /// Expected: First call returns true, second call returns false.
    /// </summary>
    [TestMethod]
    public void TryRegister_NullScopeDuplicate_SecondReturnsFalse()
    {
        // Arrange
        string? nullScope = null;

        // Act
        bool firstResult = PluginPanelScopeRegistry.TryRegister(nullScope!);
        bool secondResult = PluginPanelScopeRegistry.TryRegister(nullScope!);

        // Assert
        Assert.IsTrue(firstResult, "First registration of null should return true.");
        Assert.IsFalse(secondResult, "Duplicate registration of null should return false.");
    }

    /// <summary>
    /// Verifies that the method does not throw exceptions for any valid string input.
    /// Input: Various valid scope strings.
    /// Expected: No exceptions thrown.
    /// </summary>
    [TestMethod]
    [DataRow("ValidScope1")]
    [DataRow("ValidScope2")]
    [DataRow("MyPlugin.Panel")]
    [DataRow("Com.Example.Plugin.FullName")]
    public void TryRegister_ValidScopes_DoesNotThrow(string scope)
    {
        // Arrange
        string uniqueScope = scope + Guid.NewGuid().ToString();

        // Act & Assert
        try
        {
            bool result = PluginPanelScopeRegistry.TryRegister(uniqueScope);
            Assert.IsTrue(result, "Valid scope should register successfully.");
        }
        catch (Exception ex)
        {
            Assert.Fail($"TryRegister should not throw exception for valid scope, but threw: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifies thread safety by attempting concurrent registrations of the same scope.
    /// Input: Multiple threads attempting to register the same scope.
    /// Expected: Only one thread should successfully register (return true), others should return false.
    /// </summary>
    [TestMethod]
    public void TryRegister_ConcurrentRegistrations_OnlyOneSucceeds()
    {
        // Arrange
        string sharedScope = $"ConcurrentScope_{Guid.NewGuid()}";
        int successCount = 0;
        int threadCount = 10;
        object countLock = new object();

        // Act
        System.Threading.Tasks.Parallel.For(0, threadCount, _ =>
        {
            bool result = PluginPanelScopeRegistry.TryRegister(sharedScope);
            if (result)
            {
                lock (countLock)
                {
                    successCount++;
                }
            }
        });

        // Assert
        Assert.AreEqual(1, successCount, "Only one thread should successfully register the scope.");
    }

    /// <summary>
    /// Verifies that registering scopes with Unicode characters works correctly.
    /// Input: Strings containing Unicode characters.
    /// Expected: Each registers successfully on first attempt.
    /// </summary>
    [TestMethod]
    [DataRow("Scope_日本語")]
    [DataRow("Scope_Ελληνικά")]
    [DataRow("Scope_Русский")]
    [DataRow("Scope_العربية")]
    [DataRow("Scope_🎮")]
    public void TryRegister_UnicodeCharacters_ReturnsTrue(string unicodeScope)
    {
        // Arrange
        string uniqueUnicode = unicodeScope + Guid.NewGuid().ToString();

        // Act
        bool result = PluginPanelScopeRegistry.TryRegister(uniqueUnicode);

        // Assert
        Assert.IsTrue(result, $"Scope with Unicode characters '{unicodeScope}' should register successfully.");
    }

    /// <summary>
    /// Verifies that scopes differing only in case are treated as distinct.
    /// Input: "ScopeA" and "scopea".
    /// Expected: Both register successfully.
    /// </summary>
    [TestMethod]
    public void TryRegister_CaseSensitiveScopes_TreatedAsDistinct()
    {
        // Arrange
        string guid = Guid.NewGuid().ToString();
        string upperScope = $"ScopeA_{guid}";
        string lowerScope = $"scopea_{guid}";

        // Act
        bool upperResult = PluginPanelScopeRegistry.TryRegister(upperScope);
        bool lowerResult = PluginPanelScopeRegistry.TryRegister(lowerScope);

        // Assert
        Assert.IsTrue(upperResult, "Upper case scope should register successfully.");
        Assert.IsTrue(lowerResult, "Lower case scope should register successfully as distinct from upper case.");
    }
}