namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines the numeric control and item-lifecycle operations used by built-in numeric config controls.
/// </summary>
internal interface INumericControlOps
{
    bool SliderInt(string label, ref int value, int min, int max, string format);

    bool DragInt(string label, ref int value, float speed, int min, int max, string format);

    bool SliderFloat(string label, ref float value, float min, float max, string format);

    bool DragFloat(string label, ref float value, float speed, float min, float max, string format);

    bool SliderDouble(string label, ref double value, double min, double max, string format);

    bool DragDouble(string label, ref double value, float speed, string format);

    bool IsItemActivated();

    bool IsItemDeactivated();
}
