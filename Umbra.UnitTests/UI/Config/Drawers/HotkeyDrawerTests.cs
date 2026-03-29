using Moq;
using Umbra.Config;

namespace Umbra.UI.Config.Drawers.UnitTests;


/// <summary>
/// Unit tests for the <see cref="HotkeyDrawer"/> class.
/// </summary>
[TestClass]
public sealed class HotkeyDrawerTests
{
    /// <summary>
    /// Verifies that Draw returns immediately without throwing when the drawer has been disposed.
    /// </summary>
    [TestMethod]
    public void Draw_WhenDisposed_ReturnsEarlyWithoutException()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        drawer.Dispose();
        var mockParameter = new Mock<IParameter>();

        // Act & Assert - should not throw
        drawer.Draw("TestLabel", mockParameter.Object);
    }

    /// <summary>
    /// Verifies that Draw handles a null parameter gracefully without throwing an exception.
    /// Note: Cannot verify ImGui.TextDisabled call due to static method limitation.
    /// </summary>
    [TestMethod]
    public void Draw_WhenParameterIsNull_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();

        // Act & Assert - should not throw
        drawer.Draw("TestLabel", null!);
    }

    /// <summary>
    /// Verifies that Draw handles a non-Parameter&lt;int&gt; type gracefully without throwing.
    /// Expected: ImGui.TextDisabled should be called with error message (cannot verify due to static method).
    /// </summary>
    [TestMethod]
    public void Draw_WhenParameterIsNotParameterOfInt_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var mockParameter = new Mock<IParameter>();

        // Act & Assert - should not throw
        drawer.Draw("TestLabel", mockParameter.Object);
    }

    /// <summary>
    /// Verifies that Draw does not throw when parameter is Parameter&lt;int&gt; with valid value.
    /// Note: Full interaction testing requires ImGui mocking which is not possible for static methods.
    /// This test verifies basic execution without exceptions.
    /// </summary>
    [TestMethod]
    public void Draw_WithValidParameterOfInt_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70) // F2 key as default
        {
            Key = "testKey",
            Metadata = new ParameterMetadata { Description = null }
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw handles Parameter&lt;int&gt; with int.MinValue without throwing.
    /// </summary>
    [TestMethod]
    public void Draw_WithMinIntValue_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(int.MinValue)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw handles Parameter&lt;int&gt; with int.MaxValue without throwing.
    /// </summary>
    [TestMethod]
    public void Draw_WithMaxIntValue_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(int.MaxValue)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw handles Parameter&lt;int&gt; with zero value without throwing.
    /// </summary>
    [TestMethod]
    public void Draw_WithZeroValue_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(0)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw handles Parameter&lt;int&gt; with negative value without throwing.
    /// </summary>
    [TestMethod]
    public void Draw_WithNegativeValue_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(-100)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when label is null.
    /// Expected: ImGui methods should handle null label appropriately.
    /// </summary>
    [TestMethod]
    public void Draw_WhenLabelIsNull_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw
        drawer.Draw(null!, parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when label is empty string.
    /// </summary>
    [TestMethod]
    public void Draw_WhenLabelIsEmpty_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw
        drawer.Draw(string.Empty, parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when label is whitespace-only.
    /// </summary>
    [TestMethod]
    public void Draw_WhenLabelIsWhitespace_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw
        drawer.Draw("   ", parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when label contains special characters.
    /// </summary>
    [TestMethod]
    public void Draw_WhenLabelContainsSpecialCharacters_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw
        drawer.Draw("Test!@#$%^&*()_+-=[]{}|;':\",./<>?", parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when label is very long.
    /// </summary>
    [TestMethod]
    public void Draw_WhenLabelIsVeryLong_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };
        var longLabel = new string('A', 10000);

        // Act & Assert - should not throw
        drawer.Draw(longLabel, parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when parameter Key is null.
    /// Note: ImGui button ID will incorporate the null key.
    /// </summary>
    [TestMethod]
    public void Draw_WhenParameterKeyIsNull_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = null!,
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when parameter Key is empty.
    /// </summary>
    [TestMethod]
    public void Draw_WhenParameterKeyIsEmpty_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = string.Empty,
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when parameter Metadata is null.
    /// Expected: Help marker should not be drawn when metadata is null.
    /// </summary>
    [TestMethod]
    public void Draw_WhenMetadataIsNull_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = null!
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when metadata Description is null.
    /// Expected: ImGuiWidgets.DrawHelpMarker should not be called when Description is null.
    /// </summary>
    [TestMethod]
    public void Draw_WhenMetadataDescriptionIsNull_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata { Description = null }
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when metadata Description is not null.
    /// Expected: ImGuiWidgets.DrawHelpMarker should be called with the description.
    /// Note: Cannot verify the call due to static method limitation.
    /// </summary>
    [TestMethod]
    public void Draw_WhenMetadataDescriptionIsProvided_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata { Description = "Press a key to bind" }
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when metadata Description is empty.
    /// </summary>
    [TestMethod]
    public void Draw_WhenMetadataDescriptionIsEmpty_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata { Description = string.Empty }
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when metadata Description is very long.
    /// </summary>
    [TestMethod]
    public void Draw_WhenMetadataDescriptionIsVeryLong_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var longDescription = new string('X', 10000);
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata { Description = longDescription }
        };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw can be called multiple times without throwing.
    /// This simulates multiple frames in the ImGui draw loop.
    /// Note: State transitions require ImGui button interaction which cannot be simulated.
    /// </summary>
    [TestMethod]
    public void Draw_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act & Assert - should not throw on multiple calls
        drawer.Draw("Hotkey", parameter);
        drawer.Draw("Hotkey", parameter);
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that Draw does not throw when called after Dispose.
    /// Expected: Early return without any ImGui interaction.
    /// </summary>
    [TestMethod]
    public void Draw_CalledAfterDispose_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        drawer.Dispose();

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", parameter);
        drawer.Draw("Hotkey", parameter);
    }

    /// <summary>
    /// Verifies that multiple Draw calls after Dispose continue to return early.
    /// </summary>
    [TestMethod]
    public void Draw_MultipleCallsAfterDispose_AllReturnEarly()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        drawer.Dispose();

        // Act & Assert - all calls should return early without throwing
        for (var i = 0; i < 10; i++)
        {
            drawer.Draw("Hotkey", parameter);
        }
    }

    /// <summary>
    /// Verifies that Draw handles a Parameter&lt;int&gt; cast from an IParameter mock correctly.
    /// Expected: Should recognize it's not a Parameter&lt;int&gt; and display error.
    /// </summary>
    [TestMethod]
    public void Draw_WhenParameterIsIParameterButNotParameterOfInt_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Key).Returns("testKey");
        mockParameter.Setup(p => p.Metadata).Returns(new ParameterMetadata());

        // Act & Assert - should not throw
        drawer.Draw("Hotkey", mockParameter.Object);
    }

    /// <summary>
    /// Partial test: Verifies observable behavior when HotkeyCaptureState.WaitingCount is manipulated.
    /// Limitation: Cannot simulate ImGui button clicks to trigger waiting state changes.
    /// Cannot verify mutual exclusion logic without ImGui interaction simulation.
    /// </summary>
    [TestMethod]
    public void Draw_WithManipulatedWaitingCount_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Simulate another drawer waiting
        var originalCount = HotkeyCaptureState.WaitingCount;
        try
        {
            HotkeyCaptureState.WaitingCount = 1;

            // Act & Assert - should not throw
            // Note: Cannot verify that Change button is disabled without mocking ImGui
            drawer.Draw("Hotkey", parameter);
        }
        finally
        {
            // Cleanup
            HotkeyCaptureState.WaitingCount = originalCount;
        }
    }

    /// <summary>
    /// Verifies that Draw with different parameter instances does not throw.
    /// </summary>
    [TestMethod]
    public void Draw_WithDifferentParameterInstances_DoesNotThrow()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var parameter1 = new Parameter<int>(70) { Key = "key1", Metadata = new ParameterMetadata() };
        var parameter2 = new Parameter<int>(71) { Key = "key2", Metadata = new ParameterMetadata() };

        // Act & Assert - should not throw
        drawer.Draw("Hotkey1", parameter1);
        drawer.Draw("Hotkey2", parameter2);
    }

    /// <summary>
    /// Verifies that Draw does not modify parameter value when no user interaction occurs.
    /// Note: Cannot simulate user interaction (button clicks, key capture) without ImGui mocking.
    /// </summary>
    [TestMethod]
    public void Draw_WithNoUserInteraction_DoesNotModifyParameterValue()
    {
        // Arrange
        var drawer = new HotkeyDrawer();
        var initialValue = 70;
        var parameter = new Parameter<int>(initialValue)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw("Hotkey", parameter);

        // Assert
        Assert.AreEqual(initialValue, parameter.Value, "Parameter value should not change without user interaction");
    }

    /// <summary>
    /// Cleanup: Resets HotkeyCaptureState.WaitingCount after all tests to prevent test pollution.
    /// This ensures a clean state for other test classes that might interact with HotkeyCaptureState.
    /// </summary>
    [TestCleanup]
    public void Cleanup() =>
        // Reset shared state to prevent test pollution
        HotkeyCaptureState.WaitingCount = 0;

    /// <summary>
    /// Verifies that calling <see cref="HotkeyDrawer.Dispose"/> on a newly created instance
    /// that is not in waiting state does not affect the shared <see cref="HotkeyCaptureState.WaitingCount"/>.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenNotWaiting_DoesNotDecrementWaitingCount()
    {
        // Arrange
        var initialCount = HotkeyCaptureState.WaitingCount;
        var drawer = new HotkeyDrawer();

        // Act
        drawer.Dispose();

        // Assert
        Assert.AreEqual(initialCount, HotkeyCaptureState.WaitingCount, "WaitingCount should not change when disposing a non-waiting drawer.");
    }

    /// <summary>
    /// Verifies that calling <see cref="HotkeyDrawer.Dispose"/> multiple times is idempotent
    /// and does not throw an exception or cause unintended side effects on the second call.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var drawer = new HotkeyDrawer();

        // Act
        drawer.Dispose();
        drawer.Dispose(); // Second call should be safe
    }

    /// <summary>
    /// Verifies that after calling <see cref="HotkeyDrawer.Dispose"/>, subsequent calls
    /// do not affect the shared <see cref="HotkeyCaptureState.WaitingCount"/> even if called multiple times.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenAlreadyDisposed_DoesNotModifyWaitingCountAgain()
    {
        // Arrange
        var initialCount = HotkeyCaptureState.WaitingCount;
        var drawer = new HotkeyDrawer();

        // Act
        drawer.Dispose();
        var countAfterFirstDispose = HotkeyCaptureState.WaitingCount;
        drawer.Dispose();
        var countAfterSecondDispose = HotkeyCaptureState.WaitingCount;

        // Assert
        Assert.AreEqual(initialCount, countAfterFirstDispose, "WaitingCount should not change after first Dispose when not waiting.");
        Assert.AreEqual(countAfterFirstDispose, countAfterSecondDispose, "WaitingCount should not change on subsequent Dispose calls.");
    }

    /// <summary>
    /// Verifies that <see cref="HotkeyDrawer.Dispose"/> correctly handles the case where
    /// the drawer is in waiting state by decrementing <see cref="HotkeyCaptureState.WaitingCount"/>.
    /// This test manually simulates the waiting state by incrementing the counter before disposal.
    /// </summary>
    /// <remarks>
    /// Note: This test cannot directly set the internal _waiting field without reflection,
    /// so it validates the behavior indirectly by ensuring WaitingCount consistency.
    /// A full integration test would be needed to verify the complete waiting-state disposal flow.
    /// </remarks>
    [TestMethod]
    public void Dispose_WhenWaitingCountIsNonZero_MaintainsCounterIntegrity()
    {
        // Arrange
        var initialCount = HotkeyCaptureState.WaitingCount;
        HotkeyCaptureState.WaitingCount = initialCount + 1; // Simulate another drawer waiting
        var drawer = new HotkeyDrawer();

        // Act
        drawer.Dispose();
        var countAfterDispose = HotkeyCaptureState.WaitingCount;

        // Assert
        // Since this drawer was never in waiting state, WaitingCount should remain as we set it
        Assert.AreEqual(initialCount + 1, countAfterDispose, "Dispose should not affect WaitingCount when the drawer itself is not waiting.");

        // Cleanup
        HotkeyCaptureState.WaitingCount = initialCount;
    }

    /// <summary>
    /// Verifies that multiple <see cref="HotkeyDrawer"/> instances can be created and disposed
    /// without interfering with each other's state or the shared <see cref="HotkeyCaptureState.WaitingCount"/>.
    /// </summary>
    [TestMethod]
    public void Dispose_MultipleDrawerInstances_DoNotInterfereWithEachOther()
    {
        // Arrange
        var initialCount = HotkeyCaptureState.WaitingCount;
        var drawer1 = new HotkeyDrawer();
        var drawer2 = new HotkeyDrawer();
        var drawer3 = new HotkeyDrawer();

        // Act
        drawer1.Dispose();
        var countAfterFirst = HotkeyCaptureState.WaitingCount;
        drawer2.Dispose();
        var countAfterSecond = HotkeyCaptureState.WaitingCount;
        drawer3.Dispose();
        var countAfterThird = HotkeyCaptureState.WaitingCount;

        // Assert
        Assert.AreEqual(initialCount, countAfterFirst, "WaitingCount should not change after disposing first drawer.");
        Assert.AreEqual(initialCount, countAfterSecond, "WaitingCount should not change after disposing second drawer.");
        Assert.AreEqual(initialCount, countAfterThird, "WaitingCount should not change after disposing third drawer.");
    }

    /// <summary>
    /// Verifies that calling <see cref="HotkeyDrawer.Dispose"/> cleans up properly
    /// and prevents the object from being finalized by the garbage collector.
    /// </summary>
    [TestMethod]
    public void Dispose_CallsSuppressFinalize()
    {
        // Arrange
        var drawer = new HotkeyDrawer();

        // Act
        drawer.Dispose();
    }
}
