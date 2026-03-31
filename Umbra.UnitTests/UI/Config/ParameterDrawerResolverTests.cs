using Moq;
using Umbra.Config;
using Umbra.UI.Config.Drawers;


namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="ParameterDrawerResolver"/>.
/// </summary>
[TestClass]
public class ParameterDrawerResolverTests
{
    /// <summary>
    /// Tests that TryResolve returns null when no custom drawer types are specified in metadata.
    /// </summary>
    [TestMethod]
    public void TryResolve_NoCustomDrawers_ReturnsNull()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = null,
            TwoColumnCustomDrawerType = null
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that TryResolve successfully creates a draw action and resource when a valid
    /// CustomDrawerType is specified.
    /// </summary>
    [TestMethod]
    public void TryResolve_WithValidCustomDrawerType_ReturnsDrawActionAndResource()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = typeof(TestParameterDrawer),
            TwoColumnCustomDrawerType = null
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Value.draw);
        Assert.IsNotNull(result.Value.resource);
        Assert.IsInstanceOfType<IParameterDrawer>(result.Value.resource);
    }

    /// <summary>
    /// Tests that TryResolve successfully creates a draw action and resource when a valid
    /// TwoColumnCustomDrawerType is specified.
    /// </summary>
    [TestMethod]
    public void TryResolve_WithValidTwoColumnDrawerType_ReturnsDrawActionAndResource()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = null,
            TwoColumnCustomDrawerType = typeof(TestTwoColumnDrawer)
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Value.draw);
        Assert.IsNotNull(result.Value.resource);
        Assert.IsInstanceOfType<ITwoColumnParameterDrawer>(result.Value.resource);
    }

    /// <summary>
    /// Tests that when both CustomDrawerType and TwoColumnCustomDrawerType are present,
    /// CustomDrawerType takes priority (highest priority).
    /// </summary>
    [TestMethod]
    public void TryResolve_BothDrawerTypesPresent_CustomDrawerTakesPriority()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = typeof(TestParameterDrawer),
            TwoColumnCustomDrawerType = typeof(TestTwoColumnDrawer)
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<TestParameterDrawer>(result.Value.resource);
        Assert.IsNotInstanceOfType<TestTwoColumnDrawer>(result.Value.resource);
    }

    /// <summary>
    /// Tests that when CustomDrawerType instantiation fails, the method falls through
    /// to check TwoColumnCustomDrawerType.
    /// </summary>
    [TestMethod]
    public void TryResolve_CustomDrawerTypeThrowsOnInstantiation_ChecksTwoColumnDrawer()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = typeof(DrawerWithNoParameterlessConstructor),
            TwoColumnCustomDrawerType = typeof(TestTwoColumnDrawer)
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<TestTwoColumnDrawer>(result.Value.resource);
    }

    /// <summary>
    /// Tests that when CustomDrawerType is not an IParameterDrawer, the method falls through
    /// to check TwoColumnCustomDrawerType.
    /// </summary>
    [TestMethod]
    public void TryResolve_CustomDrawerTypeNotImplementingInterface_ChecksTwoColumnDrawer()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = typeof(NotADrawer),
            TwoColumnCustomDrawerType = typeof(TestTwoColumnDrawer)
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<TestTwoColumnDrawer>(result.Value.resource);
    }

    /// <summary>
    /// Tests that when TwoColumnCustomDrawerType instantiation fails and no CustomDrawerType
    /// is present, the method returns null.
    /// </summary>
    [TestMethod]
    public void TryResolve_TwoColumnDrawerTypeThrowsOnInstantiation_ReturnsNull()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = null,
            TwoColumnCustomDrawerType = typeof(DrawerWithNoParameterlessConstructor)
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that when both CustomDrawerType and TwoColumnCustomDrawerType fail to instantiate,
    /// the method returns null.
    /// </summary>
    [TestMethod]
    public void TryResolve_BothDrawerTypesFail_ReturnsNull()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = typeof(DrawerWithNoParameterlessConstructor),
            TwoColumnCustomDrawerType = typeof(DrawerWithNoParameterlessConstructor)
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that when TwoColumnCustomDrawerType does not implement ITwoColumnParameterDrawer,
    /// the method returns null.
    /// </summary>
    [TestMethod]
    public void TryResolve_TwoColumnDrawerTypeNotImplementingInterface_ReturnsNull()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = null,
            TwoColumnCustomDrawerType = typeof(NotADrawer)
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that the returned draw action for a CustomDrawer correctly invokes the drawer's
    /// Draw method with the provided label and parameter.
    /// </summary>
    [TestMethod]
    public void TryResolve_ValidCustomDrawer_DrawActionInvokesDrawMethod()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = typeof(TestParameterDrawer),
            TwoColumnCustomDrawerType = null
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);
        var drawer = (TestParameterDrawer)result!.Value.resource!;
        result.Value.draw();

        // Assert
        Assert.IsTrue(drawer.DrawCalled);
        Assert.AreEqual(label, drawer.LastLabel);
        Assert.AreEqual(mockParameter.Object, drawer.LastParameter);
    }

    /// <summary>
    /// Tests that the returned resource is properly disposable.
    /// </summary>
    [TestMethod]
    public void TryResolve_ValidDrawer_ResourceIsDisposable()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = typeof(TestParameterDrawer),
            TwoColumnCustomDrawerType = null
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, label, alignGroup);
        var resource = result!.Value.resource;

        // Assert
        Assert.IsNotNull(resource);
        Assert.IsInstanceOfType<IDisposable>(resource);

        // Verify Dispose doesn't throw
        resource.Dispose();
    }

    /// <summary>
    /// Tests that resolving a two-column drawer returns a callable draw action without invoking ImGui during construction.
    /// </summary>
    [TestMethod]
    public void TryResolve_ValidTwoColumnDrawer_ReturnsDeferredDrawActionAndResource()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = null,
            TwoColumnCustomDrawerType = typeof(TestTwoColumnDrawer),
            HiddenLabel = "##explicit"
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, "Test Label", alignGroup);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Value.draw);
        Assert.IsInstanceOfType<TestTwoColumnDrawer>(result.Value.resource);
    }

    /// <summary>
    /// Tests that an empty label is forwarded unchanged to a custom drawer.
    /// </summary>
    [TestMethod]
    public void TryResolve_CustomDrawer_EmptyLabel_IsForwardedUnchanged()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            CustomDrawerType = typeof(TestParameterDrawer),
            TwoColumnCustomDrawerType = null
        };
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.Metadata).Returns(metadata);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var result = ParameterDrawerResolver.TryResolve(mockParameter.Object, string.Empty, alignGroup);
        var drawer = (TestParameterDrawer)result!.Value.resource!;
        result.Value.draw();

        // Assert
        Assert.IsTrue(drawer.DrawCalled);
        Assert.AreEqual(string.Empty, drawer.LastLabel);
    }

    #region Helper Classes

    /// <summary>
    /// Test implementation of IParameterDrawer for testing purposes.
    /// </summary>
    private class TestParameterDrawer : IParameterDrawer
    {
        public bool DrawCalled { get; private set; }
        public string? LastLabel { get; private set; }
        public IParameter? LastParameter { get; private set; }

        public void Draw(string label, IParameter parameter)
        {
            DrawCalled = true;
            LastLabel = label;
            LastParameter = parameter;
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Test implementation of ITwoColumnParameterDrawer for testing purposes.
    /// </summary>
    private class TestTwoColumnDrawer : ITwoColumnParameterDrawer
    {
        public bool DrawCalled { get; private set; }
        public IParameter? LastParameter { get; private set; }

        public void Draw(IParameter parameter)
        {
            DrawCalled = true;
            LastParameter = parameter;
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Test class that has no parameterless constructor, used to test instantiation failure.
    /// </summary>
    private class DrawerWithNoParameterlessConstructor : IParameterDrawer
    {
        public DrawerWithNoParameterlessConstructor(int dummy)
        {
            // No-op for testing
        }

        public void Draw(string label, IParameter parameter)
        {
            // No-op for testing
        }

        public void Dispose()
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Test class that does not implement any drawer interface, used to test cast failure.
    /// </summary>
    private class NotADrawer
    {
    }

    #endregion
}
