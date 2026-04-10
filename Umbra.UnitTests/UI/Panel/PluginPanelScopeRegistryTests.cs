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
        var idScope = $"TestScope_{Guid.NewGuid()}";
        var firstRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);
        var secondRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Assert
        Assert.IsTrue(firstRegister, "First registration should succeed.");
        Assert.IsTrue(secondRegister, "Second registration after release should succeed.");

        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release re-arms duplicate scope warnings by clearing the warned-duplicate tracking state.
    /// </summary>
    [TestMethod]
    public void Release_DuplicateScope_ReArmsDuplicateWarning()
    {
        // Arrange
        var idScope = $"TestScope_{Guid.NewGuid()}";
        var firstRegister = PluginPanelScopeRegistry.TryRegister(idScope);
        var duplicateRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);
        var thirdRegister = PluginPanelScopeRegistry.TryRegister(idScope);
        var fourthRegister = PluginPanelScopeRegistry.TryRegister(idScope);

        // Assert
        Assert.IsTrue(firstRegister, "First registration should succeed.");
        Assert.IsFalse(duplicateRegister, "Duplicate registration should fail.");
        Assert.IsTrue(thirdRegister, "Registration after release should succeed.");
        Assert.IsFalse(fourthRegister, "Duplicate registration after re-registration should fail.");

        PluginPanelScopeRegistry.Release(idScope);
    }

    /// <summary>
    /// Tests that Release does not throw when called with a scope that was never registered.
    /// </summary>
    [TestMethod]
    public void Release_NonExistentScope_DoesNotThrow()
    {
        // Arrange
        var idScope = $"NonExistent_{Guid.NewGuid()}";

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
        var idScope = $"TestScope_{Guid.NewGuid()}";
        PluginPanelScopeRegistry.TryRegister(idScope);
        PluginPanelScopeRegistry.Release(idScope);

        // Act & Assert
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
        var scopes = new string[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            scopes[i] = $"TestScope_{Guid.NewGuid()}";
            PluginPanelScopeRegistry.TryRegister(scopes[i]);
        }

        // Act
        System.Threading.Tasks.Parallel.For(0, threadCount, i => PluginPanelScopeRegistry.Release(scopes[i]));

        // Assert
        for (var i = 0; i < threadCount; i++)
        {
            var canReRegister = PluginPanelScopeRegistry.TryRegister(scopes[i]);
            Assert.IsTrue(canReRegister, $"Scope {scopes[i]} should be successfully released and re-registerable.");
            PluginPanelScopeRegistry.Release(scopes[i]);
        }
    }

    /// <summary>
    /// Tests that Release correctly removes scope from both internal collections.
    /// </summary>
    [TestMethod]
    public void Release_RegisteredAndWarnedScope_RemovesFromBothCollections()
    {
        // Arrange
        var idScope = $"TestScope_{Guid.NewGuid()}";
        PluginPanelScopeRegistry.TryRegister(idScope);
        PluginPanelScopeRegistry.TryRegister(idScope);

        // Act
        PluginPanelScopeRegistry.Release(idScope);

        // Assert
        var canReRegister = PluginPanelScopeRegistry.TryRegister(idScope);
        Assert.IsTrue(canReRegister, "Scope should be completely released and re-registerable.");

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
        var uniqueScope = $"UniqueScope_{Guid.NewGuid()}";

        // Act
        var result = PluginPanelScopeRegistry.TryRegister(uniqueScope);

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
        var scope = $"DuplicateScope_{Guid.NewGuid()}";

        // Act
        var firstResult = PluginPanelScopeRegistry.TryRegister(scope);
        var secondResult = PluginPanelScopeRegistry.TryRegister(scope);

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
        var scope = $"TripleDuplicateScope_{Guid.NewGuid()}";

        // Act
        var firstResult = PluginPanelScopeRegistry.TryRegister(scope);
        var secondResult = PluginPanelScopeRegistry.TryRegister(scope);
        var thirdResult = PluginPanelScopeRegistry.TryRegister(scope);

        // Assert
        Assert.IsTrue(firstResult, "First registration should return true.");
        Assert.IsFalse(secondResult, "Second registration should return false.");
        Assert.IsFalse(thirdResult, "Third registration should return false.");
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
        var emptyScope = string.Empty;

        // Act
        var firstResult = PluginPanelScopeRegistry.TryRegister(emptyScope);
        var secondResult = PluginPanelScopeRegistry.TryRegister(emptyScope);

        // Assert
        Assert.IsTrue(firstResult, "First registration of empty string should return true.");
        Assert.IsFalse(secondResult, "Duplicate registration of empty string should return false.");

        PluginPanelScopeRegistry.Release(emptyScope);
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

        // Act
        var result = PluginPanelScopeRegistry.TryRegister(nullScope!);

        // Assert
        Assert.IsTrue(result, "First registration of null should return true.");

        PluginPanelScopeRegistry.Release(nullScope!);
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
        var firstResult = PluginPanelScopeRegistry.TryRegister(nullScope!);
        var secondResult = PluginPanelScopeRegistry.TryRegister(nullScope!);

        // Assert
        Assert.IsTrue(firstResult, "First registration of null should return true.");
        Assert.IsFalse(secondResult, "Duplicate registration of null should return false.");

        PluginPanelScopeRegistry.Release(nullScope!);
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
        var sharedScope = $"ConcurrentScope_{Guid.NewGuid()}";
        var successCount = 0;
        var threadCount = 10;
        var countLock = new object();

        // Act
        System.Threading.Tasks.Parallel.For(0, threadCount, _ =>
        {
            var result = PluginPanelScopeRegistry.TryRegister(sharedScope);
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
    /// Verifies that scopes differing only in case are treated as distinct.
    /// Input: "ScopeA" and "scopea".
    /// Expected: Both register successfully.
    /// </summary>
    [TestMethod]
    public void TryRegister_CaseSensitiveScopes_TreatedAsDistinct()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString();
        var upperScope = $"ScopeA_{guid}";
        var lowerScope = $"scopea_{guid}";

        // Act
        var upperResult = PluginPanelScopeRegistry.TryRegister(upperScope);
        var lowerResult = PluginPanelScopeRegistry.TryRegister(lowerScope);

        // Assert
        Assert.IsTrue(upperResult, "Upper case scope should register successfully.");
        Assert.IsTrue(lowerResult, "Lower case scope should register successfully as distinct from upper case.");
    }

    /// <summary>
    /// Verifies that releasing one scope does not affect a different still-registered scope.
    /// </summary>
    [TestMethod]
    public void Release_OneScope_DoesNotReleaseDifferentScope()
    {
        var scopeA = $"ScopeA_{Guid.NewGuid()}";
        var scopeB = $"ScopeB_{Guid.NewGuid()}";

        var firstA = PluginPanelScopeRegistry.TryRegister(scopeA);
        var firstB = PluginPanelScopeRegistry.TryRegister(scopeB);
        PluginPanelScopeRegistry.Release(scopeA);
        var secondA = PluginPanelScopeRegistry.TryRegister(scopeA);
        var secondB = PluginPanelScopeRegistry.TryRegister(scopeB);

        Assert.IsTrue(firstA);
        Assert.IsTrue(firstB);
        Assert.IsTrue(secondA, "Released scope should be re-registerable.");
        Assert.IsFalse(secondB, "Unreleased scope should still be considered registered.");

        PluginPanelScopeRegistry.Release(scopeA);
        PluginPanelScopeRegistry.Release(scopeB);
    }
}
