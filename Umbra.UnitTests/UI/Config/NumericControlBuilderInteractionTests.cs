using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Interaction-focused unit tests for <see cref="NumericControlBuilder"/> grouped undo behavior.
/// </summary>
public partial class NumericControlBuilderTests
{
    /// <summary>
    /// Tests that one slider interaction spanning multiple frames produces one grouped undo record that restores the mouse-down value.
    /// </summary>
    [TestMethod]
    public void BuildInt_WhenSliderInteractionSpansMultipleFrames_UndoRestoresInitialMouseDownValue()
    {
        var storePath = Path.GetTempFileName();
        var store = new ConfigStore<NumericUndoConfig>(storePath);
        try
        {
            var config = store.Load();
            using var undo = new ConfigUndoStack<NumericUndoConfig>(store);
            var ops = new TestNumericControlOps();
            var alignGroup = new LabelAlignmentGroup();
            var draw = NumericControlBuilder.BuildInt("Value", config.IntValue, alignGroup, ops, undo, static () => { }, "##numeric-slider-test");

            ops.SliderIntResults.Enqueue((true, 20));
            ops.ItemActivatedResults.Enqueue(true);
            ops.ItemDeactivatedResults.Enqueue(false);
            draw();

            ops.SliderIntResults.Enqueue((true, 30));
            ops.ItemActivatedResults.Enqueue(false);
            ops.ItemDeactivatedResults.Enqueue(false);
            draw();

            ops.SliderIntResults.Enqueue((false, 30));
            ops.ItemActivatedResults.Enqueue(false);
            ops.ItemDeactivatedResults.Enqueue(true);
            draw();

            Assert.AreEqual(30, config.IntValue.Value);
            Assert.AreEqual(1, undo.Count);
            var record = undo.Peek();
            Assert.IsNotNull(record);
            Assert.AreEqual(10, record.OldValue);
            Assert.AreEqual(30, record.NewValue);
            Assert.IsTrue(undo.TryUndo());
            Assert.AreEqual(10, config.IntValue.Value);
        }
        finally
        {
            store.Dispose();
            if (File.Exists(storePath))
                File.Delete(storePath);
        }
    }

    /// <summary>
    /// Tests that a drag control begins grouped undo tracking before the first assigned value and ends it on item deactivation.
    /// </summary>
    [TestMethod]
    public void BuildFloat_WhenDragInteractionBegins_CapturesInitialValueBeforeAssignmentAndEndsOnDeactivation()
    {
        var parameter = CreateFloatParameter(1.5f, min: null, max: null, step: 0.5, format: "%.2f");
        var ops = new TestNumericControlOps();
        var sink = new TestNumericEditUndoSink();
        var draw = NumericControlBuilder.BuildFloat("Value", parameter, new LabelAlignmentGroup(), ops, sink, static () => { }, "##numeric-drag-test");

        ops.DragFloatResults.Enqueue((true, 2.0f));
        ops.ItemActivatedResults.Enqueue(true);
        ops.ItemDeactivatedResults.Enqueue(false);
        draw();

        Assert.AreEqual(1, sink.BeginCount);
        Assert.AreEqual(1.5f, (float)sink.BeginValues[0]!);
        Assert.AreEqual(2.0f, parameter.Value);
        Assert.AreEqual(0, sink.EndCount);

        ops.DragFloatResults.Enqueue((false, 2.0f));
        ops.ItemActivatedResults.Enqueue(false);
        ops.ItemDeactivatedResults.Enqueue(true);
        draw();

        Assert.AreEqual(1, sink.EndCount);
        Assert.AreEqual(2.0f, (float)sink.EndValues[0]!);
    }

    [UmbraAutoRegister]
    private sealed class NumericUndoConfig
    {
        [UmbraParameter]
        [UmbraRange(0, 100)]
        public Parameter<int> IntValue { get; set; } = new(10);
    }

    private sealed class TestNumericControlOps : INumericControlOps
    {
        internal Queue<(bool Changed, int Value)> SliderIntResults { get; } = new();
        internal Queue<(bool Changed, float Value)> DragFloatResults { get; } = new();
        internal Queue<bool> ItemActivatedResults { get; } = new();
        internal Queue<bool> ItemDeactivatedResults { get; } = new();

        public bool SliderInt(string label, ref int value, int min, int max, string format)
        {
            _ = label;
            _ = min;
            _ = max;
            _ = format;
            if (SliderIntResults.Count == 0)
                return false;

            var next = SliderIntResults.Dequeue();
            value = next.Value;
            return next.Changed;
        }

        public bool DragInt(string label, ref int value, float speed, int min, int max, string format)
        {
            _ = label;
            _ = speed;
            _ = min;
            _ = max;
            _ = format;
            _ = value;
            return false;
        }

        public bool SliderFloat(string label, ref float value, float min, float max, string format)
        {
            _ = label;
            _ = min;
            _ = max;
            _ = format;
            _ = value;
            return false;
        }

        public bool DragFloat(string label, ref float value, float speed, float min, float max, string format)
        {
            _ = label;
            _ = speed;
            _ = min;
            _ = max;
            _ = format;
            if (DragFloatResults.Count == 0)
                return false;

            var next = DragFloatResults.Dequeue();
            value = next.Value;
            return next.Changed;
        }

        public bool SliderDouble(string label, ref double value, double min, double max, string format)
        {
            _ = label;
            _ = min;
            _ = max;
            _ = format;
            _ = value;
            return false;
        }

        public bool DragDouble(string label, ref double value, float speed, string format)
        {
            _ = label;
            _ = speed;
            _ = format;
            _ = value;
            return false;
        }

        public bool IsItemActivated()
            => ItemActivatedResults.Count > 0 && ItemActivatedResults.Dequeue();

        public bool IsItemDeactivated()
            => ItemDeactivatedResults.Count > 0 && ItemDeactivatedResults.Dequeue();
    }

    private sealed class TestNumericEditUndoSink : INumericEditUndoSink
    {
        internal List<object?> BeginValues { get; } = [];
        internal List<object?> EndValues { get; } = [];
        internal int BeginCount { get; private set; }
        internal int EndCount { get; private set; }

        public void BeginNumericEdit(IParameter parameter)
        {
            BeginCount++;
            BeginValues.Add(parameter.GetValue());
        }

        public void EndNumericEdit(IParameter parameter)
        {
            EndCount++;
            EndValues.Add(parameter.GetValue());
        }
    }
}
