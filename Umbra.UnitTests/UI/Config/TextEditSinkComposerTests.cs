using Moq;
using Umbra.Config;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Contains unit tests for <see cref="TextEditSinkComposer"/>.
/// </summary>
[TestClass]
public sealed class TextEditSinkComposerTests
{
    /// <summary>
    /// Verifies that composing two null sinks returns null.
    /// </summary>
    [TestMethod]
    public void Compose_BothNull_ReturnsNull()
    {
        // Act
        var result = TextEditSinkComposer.Compose(null, null);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that composing a non-null first sink with a null second returns the first.
    /// </summary>
    [TestMethod]
    public void Compose_FirstNonNull_SecondNull_ReturnsFirst()
    {
        // Arrange
        var first = new Mock<ITextEditSink>(MockBehavior.Strict).Object;

        // Act
        var result = TextEditSinkComposer.Compose(first, null);

        // Assert
        Assert.AreSame(first, result);
    }

    /// <summary>
    /// Verifies that composing a null first sink with a non-null second returns the second.
    /// </summary>
    [TestMethod]
    public void Compose_FirstNull_SecondNonNull_ReturnsSecond()
    {
        // Arrange
        var second = new Mock<ITextEditSink>(MockBehavior.Strict).Object;

        // Act
        var result = TextEditSinkComposer.Compose(null, second);

        // Assert
        Assert.AreSame(second, result);
    }

    /// <summary>
    /// Verifies that composing two non-null sinks returns a composite that forwards to both.
    /// </summary>
    [TestMethod]
    public void Compose_BothNonNull_ReturnsComposite_ForwardsToBoth()
    {
        // Arrange
        var firstMock = new Mock<ITextEditSink>(MockBehavior.Strict);
        var secondMock = new Mock<ITextEditSink>(MockBehavior.Strict);
        var parameter = new Mock<IParameter>(MockBehavior.Loose).Object;

        firstMock.Setup(s => s.BeginTextEdit(parameter));
        secondMock.Setup(s => s.BeginTextEdit(parameter));
        firstMock.Setup(s => s.EndTextEdit(parameter));
        secondMock.Setup(s => s.EndTextEdit(parameter));

        var composite = TextEditSinkComposer.Compose(firstMock.Object, secondMock.Object);

        // Act
        Assert.IsNotNull(composite);
        composite.BeginTextEdit(parameter);
        composite.EndTextEdit(parameter);

        // Assert
        firstMock.Verify(s => s.BeginTextEdit(parameter), Times.Once);
        secondMock.Verify(s => s.BeginTextEdit(parameter), Times.Once);
        firstMock.Verify(s => s.EndTextEdit(parameter), Times.Once);
        secondMock.Verify(s => s.EndTextEdit(parameter), Times.Once);
    }
}
