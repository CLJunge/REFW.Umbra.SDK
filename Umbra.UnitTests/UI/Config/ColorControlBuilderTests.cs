using System.Numerics;
using Umbra.Config;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.UnitTests;

[TestClass]
public class ColorControlBuilderTests
{
    [TestMethod]
    public void BuildColor_WhenColorChanges_AssignsValueToParameter()
    {
        var parameter = CreateParameter(new Vector4(1f, 0f, 0f, 1f));
        var ops = new TestColorControlOps();
        var draw = ColorControlBuilder.BuildColor("Color", parameter, new LabelAlignmentGroup(), ops, null, static () => { }, "##test");

        ops.Results.Enqueue((true, new Vector4(0f, 1f, 0f, 1f)));
        draw();

        Assert.AreEqual(new Vector4(0f, 1f, 0f, 1f), parameter.Value);
    }

    [TestMethod]
    public void BuildColor_WhenColorDoesNotChange_ValueIsUnchanged()
    {
        var original = new Vector4(1f, 0f, 0f, 1f);
        var parameter = CreateParameter(original);
        var ops = new TestColorControlOps();
        var draw = ColorControlBuilder.BuildColor("Color", parameter, new LabelAlignmentGroup(), ops, null, static () => { }, "##test");

        ops.Results.Enqueue((false, original));
        draw();

        Assert.AreEqual(original, parameter.Value);
    }

    [TestMethod]
    public void BuildColor_WhenFirstChangeOccurs_FiresBeginNumericEdit()
    {
        var parameter = CreateParameter(new Vector4(1f, 0f, 0f, 1f));
        var ops = new TestColorControlOps();
        var sink = new TestNumericEditSink();
        var draw = ColorControlBuilder.BuildColor("Color", parameter, new LabelAlignmentGroup(), ops, sink, static () => { }, "##test");

        ops.Results.Enqueue((true, new Vector4(0f, 1f, 0f, 1f)));
        draw();

        Assert.AreEqual(1, sink.BeginCount);
    }

    [TestMethod]
    public void BuildColor_WhenChangesStop_FiresEndNumericEdit()
    {
        var parameter = CreateParameter(new Vector4(1f, 0f, 0f, 1f));
        var ops = new TestColorControlOps();
        var sink = new TestNumericEditSink();
        var draw = ColorControlBuilder.BuildColor("Color", parameter, new LabelAlignmentGroup(), ops, sink, static () => { }, "##test");

        ops.Results.Enqueue((true, new Vector4(0f, 1f, 0f, 1f)));
        draw();

        ops.Results.Enqueue((false, new Vector4(0f, 1f, 0f, 1f)));
        draw();

        Assert.AreEqual(1, sink.EndCount);
    }

    [TestMethod]
    public void BuildColor_MultiFrameInteraction_ProducesOneBeginEndPair()
    {
        var parameter = CreateParameter(new Vector4(1f, 0f, 0f, 1f));
        var ops = new TestColorControlOps();
        var sink = new TestNumericEditSink();
        var draw = ColorControlBuilder.BuildColor("Color", parameter, new LabelAlignmentGroup(), ops, sink, static () => { }, "##test");

        ops.Results.Enqueue((true, new Vector4(0.5f, 0f, 0f, 1f)));
        draw();

        ops.Results.Enqueue((true, new Vector4(0f, 0.5f, 0f, 1f)));
        draw();

        ops.Results.Enqueue((true, new Vector4(0f, 1f, 0f, 1f)));
        draw();

        Assert.AreEqual(1, sink.BeginCount);
        Assert.AreEqual(0, sink.EndCount);

        ops.Results.Enqueue((false, new Vector4(0f, 1f, 0f, 1f)));
        draw();

        Assert.AreEqual(1, sink.BeginCount);
        Assert.AreEqual(1, sink.EndCount);
    }

    [TestMethod]
    public void BuildColor_WhenNoChangesOccur_NoBeginEndFires()
    {
        var parameter = CreateParameter(new Vector4(1f, 0f, 0f, 1f));
        var ops = new TestColorControlOps();
        var sink = new TestNumericEditSink();
        var draw = ColorControlBuilder.BuildColor("Color", parameter, new LabelAlignmentGroup(), ops, sink, static () => { }, "##test");

        ops.Results.Enqueue((false, new Vector4(1f, 0f, 0f, 1f)));
        draw();

        ops.Results.Enqueue((false, new Vector4(1f, 0f, 0f, 1f)));
        draw();

        Assert.AreEqual(0, sink.BeginCount);
        Assert.AreEqual(0, sink.EndCount);
    }

    [TestMethod]
    public void BuildColor_NullSink_DoesNotThrow()
    {
        var parameter = CreateParameter(new Vector4(1f, 0f, 0f, 1f));
        var ops = new TestColorControlOps();
        var draw = ColorControlBuilder.BuildColor("Color", parameter, new LabelAlignmentGroup(), ops, null, static () => { }, "##test");

        ops.Results.Enqueue((true, new Vector4(0f, 1f, 0f, 1f)));
        draw();

        ops.Results.Enqueue((false, new Vector4(0f, 1f, 0f, 1f)));
        draw();

        Assert.AreEqual(new Vector4(0f, 1f, 0f, 1f), parameter.Value);
    }

    [TestMethod]
    public void BuildColor_TwoSeparateInteractions_ProducesTwoBeginEndPairs()
    {
        var parameter = CreateParameter(new Vector4(1f, 0f, 0f, 1f));
        var ops = new TestColorControlOps();
        var sink = new TestNumericEditSink();
        var draw = ColorControlBuilder.BuildColor("Color", parameter, new LabelAlignmentGroup(), ops, sink, static () => { }, "##test");

        ops.Results.Enqueue((true, new Vector4(0f, 1f, 0f, 1f)));
        draw();
        ops.Results.Enqueue((false, new Vector4(0f, 1f, 0f, 1f)));
        draw();

        Assert.AreEqual(1, sink.BeginCount);
        Assert.AreEqual(1, sink.EndCount);

        ops.Results.Enqueue((true, new Vector4(0f, 0f, 1f, 1f)));
        draw();
        ops.Results.Enqueue((false, new Vector4(0f, 0f, 1f, 1f)));
        draw();

        Assert.AreEqual(2, sink.BeginCount);
        Assert.AreEqual(2, sink.EndCount);
    }

    [TestMethod]
    public void BuildColor_DragPauseWithMouseHeld_DoesNotFireEnd()
    {
        var parameter = CreateParameter(new Vector4(1f, 0f, 0f, 1f));
        var ops = new TestColorControlOps();
        var sink = new TestNumericEditSink();
        var draw = ColorControlBuilder.BuildColor("Color", parameter, new LabelAlignmentGroup(), ops, sink, static () => { }, "##test");

        ops.Results.Enqueue((true, new Vector4(0f, 1f, 0f, 1f)));
        ops.MouseDownResults.Enqueue(true);
        draw();
        Assert.AreEqual(1, sink.BeginCount);

        ops.Results.Enqueue((false, new Vector4(0f, 1f, 0f, 1f)));
        ops.MouseDownResults.Enqueue(true);
        draw();
        Assert.AreEqual(0, sink.EndCount);

        ops.Results.Enqueue((true, new Vector4(0f, 0.5f, 0f, 1f)));
        ops.MouseDownResults.Enqueue(true);
        draw();
        Assert.AreEqual(0, sink.EndCount);

        ops.Results.Enqueue((false, new Vector4(0f, 0.5f, 0f, 1f)));
        ops.MouseDownResults.Enqueue(false);
        draw();
        Assert.AreEqual(1, sink.EndCount);
        Assert.AreEqual(1, sink.BeginCount);
    }

    private static Parameter<Vector4> CreateParameter(Vector4 value)
    {
        return new Parameter<Vector4>(value)
        {
            Key = "testColor",
            Metadata = new ParameterMetadata
            {
                HiddenLabel = "##testColor"
            }
        };
    }

    private sealed class TestColorControlOps : IColorControlOps
    {
        internal Queue<(bool Changed, Vector4 Value)> Results { get; } = new();
        internal Queue<bool> MouseDownResults { get; } = new();

        public bool ColorEdit4(string label, ref Vector4 value)
        {
            _ = label;
            if (Results.Count == 0)
                return false;

            var (Changed, Value) = Results.Dequeue();
            value = Value;
            return Changed;
        }

        public bool IsMouseDown() => MouseDownResults.Count != 0 && MouseDownResults.Dequeue();
    }

    private sealed class TestNumericEditSink : INumericEditSink
    {
        internal int BeginCount { get; private set; }
        internal int EndCount { get; private set; }

        public void BeginNumericEdit(IParameter parameter) => BeginCount++;
        public void EndNumericEdit(IParameter parameter) => EndCount++;
    }
}
