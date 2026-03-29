using Moq;
using Umbra.Config;

namespace Umbra.UI.Config.Drawers.UnitTests;


/// <summary>
/// Unit tests for <see cref="TwoColumnHotkeyDrawer"/>.
/// </summary>
[TestClass]
public sealed class TwoColumnHotkeyDrawerTests
{
    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> returns early when the drawer is disposed.
    /// </summary>
    [TestMethod]
    public void Draw_DisposedDrawer_ReturnsEarly()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        drawer.Dispose();
        var mockParameter = new Mock<IParameter>();

        // Act - should not throw and should return early without interacting with parameter
        drawer.Draw(mockParameter.Object);

        // Assert
        mockParameter.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> handles null parameter gracefully.
    /// </summary>
    /// <remarks>
    /// This test verifies behavior when a null parameter is passed. Due to the pattern-matching
    /// cast (parameter is not Parameter&lt;int&gt;), the null will fail the type check and ImGui.TextDisabled
    /// will be called. Full validation requires ImGui context which cannot be mocked.
    /// </remarks>
    [TestMethod]
    public void Draw_NullParameter_HandlesGracefully()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();

        // Act - should not throw
        drawer.Draw(null!);

        // Assert - no exception thrown (ImGui call cannot be verified without real context)
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> handles wrong parameter type by showing disabled text.
    /// </summary>
    /// <remarks>
    /// When parameter is not Parameter&lt;int&gt;, ImGui.TextDisabled is called with a message.
    /// The type check logic is tested here; ImGui rendering cannot be verified without a real ImGui context.
    /// </remarks>
    [TestMethod]
    public void Draw_WrongParameterType_ShowsDisabledText()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var mockParameter = new Mock<IParameter>();

        // Act - should not throw
        drawer.Draw(mockParameter.Object);

        // Assert - no exception thrown (ImGui call cannot be verified without real context)
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> handles Parameter&lt;string&gt; as wrong type.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterOfWrongGenericType_ShowsDisabledText()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var stringParameter = new Parameter<string>("test") { Key = "testKey" };

        // Act - should not throw
        drawer.Draw(stringParameter);

        // Assert - no exception thrown (ImGui call cannot be verified without real context)
    }

    /// <summary>
    /// Tests that WaitingCount is not incremented when drawer is disposed before Draw is called.
    /// </summary>
    [TestMethod]
    public void Draw_DisposedBeforeDraw_DoesNotModifyWaitingCount()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(70) { Key = "testHotkey" };
        var initialCount = HotkeyCaptureState.WaitingCount;
        drawer.Dispose();

        // Act
        drawer.Draw(parameter);

        // Assert
        Assert.AreEqual(initialCount, HotkeyCaptureState.WaitingCount);
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> with valid Parameter&lt;int&gt; does not throw.
    /// </summary>
    /// <remarks>
    /// This test validates that the basic path with a correct parameter type executes without exception.
    /// Full interaction testing (button clicks, key capture) requires a real ImGui context and cannot be
    /// performed in unit tests. Integration tests with ImGui initialized are required for comprehensive coverage.
    /// </remarks>
    [TestMethod]
    public void Draw_ValidParameterInt_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(70) { Key = "testHotkey" };

        // Act & Assert - should not throw
        // Note: ImGui.Text, ImGui.Button, and KeyboardInput interactions cannot be tested
        // without a real ImGui context. This test only verifies no exception is thrown.
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> with Parameter&lt;int&gt; having int.MinValue does not throw.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterValueIntMinValue_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(int.MinValue) { Key = "minValueKey" };

        // Act & Assert
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> with Parameter&lt;int&gt; having int.MaxValue does not throw.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterValueIntMaxValue_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(int.MaxValue) { Key = "maxValueKey" };

        // Act & Assert
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> with Parameter&lt;int&gt; having zero value does not throw.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterValueZero_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(0) { Key = "zeroKey" };

        // Act & Assert
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> with Parameter&lt;int&gt; having negative value does not throw.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterValueNegative_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(-1) { Key = "negativeKey" };

        // Act & Assert
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> with empty parameter key does not throw.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterKeyEmpty_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(70) { Key = "" };

        // Act & Assert
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> with null parameter key does not throw.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterKeyNull_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(70) { Key = null! };

        // Act & Assert
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> with very long parameter key does not throw.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterKeyVeryLong_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(70) { Key = new string('a', 10000) };

        // Act & Assert
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> with special characters in parameter key does not throw.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterKeySpecialCharacters_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(70) { Key = "test##key$$special@@chars!!" };

        // Act & Assert
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that multiple calls to <see cref="TwoColumnHotkeyDrawer.Draw"/> with the same parameter do not throw.
    /// </summary>
    [TestMethod]
    public void Draw_MultipleCallsSameParameter_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(70) { Key = "testKey" };

        // Act & Assert
        drawer.Draw(parameter);
        drawer.Draw(parameter);
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that <see cref="TwoColumnHotkeyDrawer.Draw"/> called after Dispose is idempotent.
    /// </summary>
    [TestMethod]
    public void Draw_CalledAfterDispose_IsIdempotent()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var parameter = new Parameter<int>(70) { Key = "testKey" };
        drawer.Dispose();

        // Act & Assert
        drawer.Draw(parameter);
        drawer.Draw(parameter);
        drawer.Draw(parameter);
    }

    /// <summary>
    /// Tests that Dispose can be called multiple times without error.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();

        // Act & Assert
        drawer.Dispose();
        drawer.Dispose();
        drawer.Dispose();
    }

    /// <summary>
    /// Tests that Dispose decrements WaitingCount when drawer is in waiting state.
    /// </summary>
    /// <remarks>
    /// This test verifies the synchronization logic for the shared WaitingCount.
    /// Due to the static nature of HotkeyCaptureState.WaitingCount and the dependency on ImGui button clicks
    /// to enter waiting state, this test can only verify disposal behavior, not the full state transition.
    /// Integration tests with ImGui are required for comprehensive coverage of waiting state transitions.
    /// </remarks>
    [TestMethod]
    public void Dispose_WhenNotWaiting_DoesNotDecrementWaitingCount()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        var initialCount = HotkeyCaptureState.WaitingCount;

        // Act
        drawer.Dispose();

        // Assert
        Assert.AreEqual(initialCount, HotkeyCaptureState.WaitingCount);
    }

    /// <summary>
    /// Ensures the static WaitingCount field is reset before each test to avoid cross-test contamination.
    /// </summary>
    [TestInitialize]
    public void Initialize() => HotkeyCaptureState.WaitingCount = 0;

    /// <summary>
    /// Verifies that calling Dispose on a newly created drawer (with _waiting = false by default)
    /// sets the object as disposed and does not decrement the shared WaitingCount.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledOnNewDrawer_SetsDisposedAndDoesNotDecrementWaitingCount()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        HotkeyCaptureState.WaitingCount = 5;

        // Act
        drawer.Dispose();

        // Assert
        Assert.AreEqual(5, HotkeyCaptureState.WaitingCount, "WaitingCount should not be decremented when _waiting is false.");
    }

    /// <summary>
    /// Verifies that calling Dispose multiple times is idempotent and does not cause side effects
    /// such as multiple decrements of the shared WaitingCount.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        HotkeyCaptureState.WaitingCount = 3;

        // Act
        drawer.Dispose();
        drawer.Dispose();
        drawer.Dispose();

        // Assert
        Assert.AreEqual(3, HotkeyCaptureState.WaitingCount, "WaitingCount should remain unchanged after multiple Dispose calls on a non-waiting drawer.");
    }

    /// <summary>
    /// Verifies that Dispose does not throw an exception when called on a freshly instantiated drawer.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledOnFreshInstance_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();

        // Act & Assert
        drawer.Dispose(); // Should not throw
    }

    /// <summary>
    /// Verifies that Dispose does not throw when WaitingCount is already zero.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenWaitingCountIsZero_DoesNotThrow()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        HotkeyCaptureState.WaitingCount = 0;

        // Act & Assert
        drawer.Dispose(); // Should not throw
    }

    /// <summary>
    /// Verifies that Dispose can be safely called after another drawer has already modified WaitingCount.
    /// Tests that the drawer does not interfere with the shared counter when it is not in waiting mode.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenOtherDrawersHaveModifiedWaitingCount_DoesNotInterfere()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer();
        HotkeyCaptureState.WaitingCount = 2;

        // Act
        drawer.Dispose();

        // Assert
        Assert.AreEqual(2, HotkeyCaptureState.WaitingCount, "Drawer should not modify WaitingCount when it is not in waiting mode.");
    }

    /// <summary>
    /// Verifies that multiple independent drawers can be disposed without interfering with each other
    /// when none of them are in waiting mode.
    /// </summary>
    [TestMethod]
    public void Dispose_MultipleDrawersNotWaiting_AllDisposeCleanly()
    {
        // Arrange
        var drawer1 = new TwoColumnHotkeyDrawer();
        var drawer2 = new TwoColumnHotkeyDrawer();
        var drawer3 = new TwoColumnHotkeyDrawer();
        HotkeyCaptureState.WaitingCount = 0;

        // Act
        drawer1.Dispose();
        drawer2.Dispose();
        drawer3.Dispose();

        // Assert
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount, "WaitingCount should remain zero after disposing multiple non-waiting drawers.");
    }

    // NOTE: Testing the scenario where _waiting = true and Dispose decrements WaitingCount
    // requires setting the private _waiting field to true. This can only be achieved by:
    // 1. Calling Draw() with a proper ImGui context and simulating user interaction (clicking the "Change" button)
    // 2. Using reflection (explicitly prohibited by test generation requirements)
    //
    // Since ImGui is a static class marked as "Cannot be mocked" and reflection is not allowed,
    // the _waiting = true disposal path cannot be fully tested with the current constraints.
    //
    // In a real-world scenario, this would be tested through integration tests with a live ImGui context,
    // or the class design could be refactored to make _waiting testable (e.g., protected virtual for test subclassing,
    // or constructor injection of initial state for testing purposes).
}