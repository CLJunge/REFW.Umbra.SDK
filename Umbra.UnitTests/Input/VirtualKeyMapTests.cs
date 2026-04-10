namespace Umbra.Input.UnitTests;

/// <summary>
/// Unit tests for the <see cref="VirtualKeyMap"/> bidirectional UmbraKey ↔ VK mapping.
/// </summary>
[TestClass]
public class VirtualKeyMapTests
{
#pragma warning disable IDE1006 // Naming Styles
    private const int VK_TAB = 0x09;
    private const int VK_RETURN = 0x0D;
    private const int VK_ESCAPE = 0x1B;
    private const int VK_SPACE = 0x20;
    private const int VK_LEFT = 0x25;
    private const int VK_A = 0x41;
    private const int VK_Z = 0x5A;
    private const int VK_F1 = 0x70;
    private const int VK_F5 = 0x74;
    private const int VK_F12 = 0x7B;
    private const int VK_LCONTROL = 0xA2;
    private const int VK_LSHIFT = 0xA0;
    private const int VK_LMENU = 0xA4;
#pragma warning restore IDE1006 // Naming Styles

    /// <summary>
    /// Tests that <see cref="VirtualKeyMap.UmbraKeyToVk"/> returns the correct VK code for known keys.
    /// </summary>
    [TestMethod]
    [DataRow(UmbraKey.Tab, VK_TAB)]
    [DataRow(UmbraKey.Enter, VK_RETURN)]
    [DataRow(UmbraKey.Escape, VK_ESCAPE)]
    [DataRow(UmbraKey.Space, VK_SPACE)]
    [DataRow(UmbraKey.LeftArrow, VK_LEFT)]
    [DataRow(UmbraKey.A, VK_A)]
    [DataRow(UmbraKey.Z, VK_Z)]
    [DataRow(UmbraKey.F1, VK_F1)]
    [DataRow(UmbraKey.F5, VK_F5)]
    [DataRow(UmbraKey.F12, VK_F12)]
    [DataRow(UmbraKey.LeftCtrl, VK_LCONTROL)]
    [DataRow(UmbraKey.LeftShift, VK_LSHIFT)]
    [DataRow(UmbraKey.LeftAlt, VK_LMENU)]
    public void UmbraKeyToVk_KnownKey_ReturnsCorrectVk(UmbraKey umbraKey, int expectedVk)
    {
        // Act
        var result = VirtualKeyMap.UmbraKeyToVk(umbraKey);

        // Assert
        Assert.AreEqual(expectedVk, result);
    }

    /// <summary>
    /// Tests that <see cref="VirtualKeyMap.UmbraKeyToVk"/> returns <c>-1</c> for unmapped keys.
    /// </summary>
    [TestMethod]
    [DataRow(UmbraKey.None)]
    [DataRow((UmbraKey)(-1))]
    [DataRow((UmbraKey)99999)]
    public void UmbraKeyToVk_UnmappedKey_ReturnsNegativeOne(UmbraKey umbraKey)
    {
        // Act
        var result = VirtualKeyMap.UmbraKeyToVk(umbraKey);

        // Assert
        Assert.AreEqual(-1, result);
    }

    /// <summary>
    /// Tests that <see cref="VirtualKeyMap.VkToUmbraKey"/> returns the correct UmbraKey for known VK codes.
    /// </summary>
    [TestMethod]
    [DataRow(VK_TAB, UmbraKey.Tab)]
    [DataRow(VK_RETURN, UmbraKey.Enter)]
    [DataRow(VK_ESCAPE, UmbraKey.Escape)]
    [DataRow(VK_A, UmbraKey.A)]
    [DataRow(VK_F1, UmbraKey.F1)]
    [DataRow(VK_LCONTROL, UmbraKey.LeftCtrl)]
    public void VkToUmbraKey_KnownVk_ReturnsCorrectUmbraKey(int vk, UmbraKey expectedKey)
    {
        // Act
        var result = VirtualKeyMap.VkToUmbraKey(vk);

        // Assert
        Assert.AreEqual(expectedKey, result);
    }

    /// <summary>
    /// Tests that <see cref="VirtualKeyMap.VkToUmbraKey"/> returns <see cref="UmbraKey.None"/> for unmapped VK codes.
    /// </summary>
    [TestMethod]
    [DataRow(0x00)]
    [DataRow(0xFF)]
    [DataRow(0x07)]
    public void VkToUmbraKey_UnmappedVk_ReturnsNone(int vk)
    {
        // Act
        var result = VirtualKeyMap.VkToUmbraKey(vk);

        // Assert
        Assert.AreEqual(UmbraKey.None, result);
    }

    /// <summary>
    /// Tests that <see cref="VirtualKeyMap.GetTrackedVirtualKeys"/> returns a non-empty array.
    /// </summary>
    [TestMethod]
    public void GetTrackedVirtualKeys_ReturnsNonEmptyArray()
    {
        // Act
        var result = VirtualKeyMap.GetTrackedVirtualKeys();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotEmpty(result, "Expected at least one tracked virtual key");
    }

    /// <summary>
    /// Tests that <see cref="VirtualKeyMap.GetTrackedVirtualKeys"/> contains no duplicate VK codes.
    /// </summary>
    [TestMethod]
    public void GetTrackedVirtualKeys_ContainsNoDuplicates()
    {
        // Arrange
        var result = VirtualKeyMap.GetTrackedVirtualKeys();
        var seen = new HashSet<int>();

        // Act / Assert
        foreach (var vk in result)
            Assert.IsTrue(seen.Add(vk), $"Duplicate VK code {vk} found in tracked keys");
    }

    /// <summary>
    /// Tests round-trip mapping: UmbraKey → VK → UmbraKey returns the original key
    /// for all keys that have a unique VK mapping (excludes KeypadEnter which shares VK_RETURN with Enter).
    /// </summary>
    [TestMethod]
    [DataRow(UmbraKey.A)]
    [DataRow(UmbraKey.Z)]
    [DataRow(UmbraKey.F1)]
    [DataRow(UmbraKey.F12)]
    [DataRow(UmbraKey.Tab)]
    [DataRow(UmbraKey.Escape)]
    [DataRow(UmbraKey.Space)]
    [DataRow(UmbraKey.LeftCtrl)]
    [DataRow(UmbraKey.LeftShift)]
    [DataRow(UmbraKey.LeftAlt)]
    [DataRow(UmbraKey.Enter)]
    [DataRow(UmbraKey.Backspace)]
    [DataRow(UmbraKey.Delete)]
    public void RoundTrip_UmbraKeyToVkAndBack_ReturnsOriginalKey(UmbraKey originalKey)
    {
        // Act
        var vk = VirtualKeyMap.UmbraKeyToVk(originalKey);
        var roundTripped = VirtualKeyMap.VkToUmbraKey(vk);

        // Assert
        Assert.AreEqual(originalKey, roundTripped, $"Round-trip failed for {originalKey} (VK=0x{vk:X2})");
    }

    /// <summary>
    /// Tests that letter keys A-Z all have valid VK mappings.
    /// </summary>
    [TestMethod]
    public void UmbraKeyToVk_AllLetterKeys_HaveValidMapping()
    {
        // Arrange / Act / Assert
        for (var i = 0; i < 26; i++)
        {
            var key = (UmbraKey)((int)UmbraKey.A + i);
            var vk = VirtualKeyMap.UmbraKeyToVk(key);
            Assert.AreNotEqual(-1, vk, $"UmbraKey {key} has no VK mapping");
            Assert.AreEqual(VK_A + i, vk, $"UmbraKey {key} has wrong VK mapping");
        }
    }

    /// <summary>
    /// Tests that F1-F12 all have valid VK mappings.
    /// </summary>
    [TestMethod]
    public void UmbraKeyToVk_FunctionKeysF1ToF12_HaveValidMapping()
    {
        // Arrange / Act / Assert
        for (var i = 0; i < 12; i++)
        {
            var key = (UmbraKey)((int)UmbraKey.F1 + i);
            var vk = VirtualKeyMap.UmbraKeyToVk(key);
            Assert.AreNotEqual(-1, vk, $"UmbraKey {key} has no VK mapping");
            Assert.AreEqual(VK_F1 + i, vk, $"UmbraKey {key} has wrong VK mapping");
        }
    }
}
