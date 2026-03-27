using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Preferred collision-safe attribute that marks a class or struct for automatic settings registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraAutoRegisterSettingsAttribute : Attribute { }

/// <summary>
/// Preferred collision-safe attribute that sets the visual color style of a button parameter.
/// </summary>
/// <param name="style">The color style to apply to the rendered button.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraButtonStyleAttribute(ButtonStyle style) : Attribute
{
    /// <summary>Gets the visual color style applied to the button.</summary>
    public ButtonStyle Style { get; } = style;
}

/// <summary>
/// Preferred collision-safe attribute that assigns a category name to a settings member or group.
/// </summary>
/// <param name="name">The category name used to group related parameters in the UI.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraCategoryAttribute(string name) : Attribute
{
    /// <summary>Gets the category name used to group related parameters in the UI.</summary>
    public string Name { get; } = name;
}

/// <summary>
/// Preferred collision-safe attribute that renders a settings group as a collapsible tree.
/// </summary>
/// <param name="defaultOpen">Whether the tree node is rendered in its open state by default.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraCollapseAsTreeAttribute(bool defaultOpen = false) : Attribute
{
    /// <summary>Gets whether the tree node starts in its expanded state.</summary>
    public bool DefaultOpen { get; } = defaultOpen;
}

/// <summary>
/// Preferred collision-safe attribute that wraps an entire config in a root tree node.
/// </summary>
/// <param name="label">The label displayed on the root tree node header.</param>
/// <param name="defaultOpen">Whether the root tree node starts expanded.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraConfigRootNodeAttribute(string? label = null, bool defaultOpen = false) : Attribute
{
    /// <summary>Gets the label shown on the root tree node header.</summary>
    public string? Label { get; } = label;

    /// <summary>Gets whether the root tree node starts in its expanded state.</summary>
    public bool DefaultOpen { get; } = defaultOpen;
}

/// <summary>
/// Preferred collision-safe attribute that sets the pixel width of a settings control's editing widget.
/// </summary>
/// <param name="width">The pixel width: <c>0f</c> = type-default, negative = fill available, positive = fixed px.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraControlWidthAttribute(float width) : Attribute
{
    /// <summary>Gets the configured width for the control.</summary>
    public float Width { get; } = width;
}

/// <summary>
/// Preferred collision-safe attribute that supplies fully custom RGBA button colors.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraCustomButtonColorsAttribute : Attribute
{
    /// <summary>Gets the red channel of the normal button color.</summary>
    public float NormalR { get; }
    /// <summary>Gets the green channel of the normal button color.</summary>
    public float NormalG { get; }
    /// <summary>Gets the blue channel of the normal button color.</summary>
    public float NormalB { get; }
    /// <summary>Gets the alpha channel of the normal button color.</summary>
    public float NormalA { get; }

    /// <summary>Gets the red channel of the hovered button color.</summary>
    public float HoveredR { get; }
    /// <summary>Gets the green channel of the hovered button color.</summary>
    public float HoveredG { get; }
    /// <summary>Gets the blue channel of the hovered button color.</summary>
    public float HoveredB { get; }
    /// <summary>Gets the alpha channel of the hovered button color.</summary>
    public float HoveredA { get; }

    /// <summary>Gets the red channel of the active button color.</summary>
    public float ActiveR { get; }
    /// <summary>Gets the green channel of the active button color.</summary>
    public float ActiveG { get; }
    /// <summary>Gets the blue channel of the active button color.</summary>
    public float ActiveB { get; }
    /// <summary>Gets the alpha channel of the active button color.</summary>
    public float ActiveA { get; }

    /// <summary>
    /// Initializes a new <see cref="UmbraCustomButtonColorsAttribute"/> from a single base RGB color.
    /// </summary>
    public UmbraCustomButtonColorsAttribute(float r, float g, float b)
    {
        NormalR = r; NormalG = g; NormalB = b; NormalA = 1f;
        HoveredR = Math.Clamp(r + 0.10f, 0f, 1f);
        HoveredG = Math.Clamp(g + 0.10f, 0f, 1f);
        HoveredB = Math.Clamp(b + 0.10f, 0f, 1f);
        HoveredA = 1f;
        ActiveR = Math.Clamp(r - 0.08f, 0f, 1f);
        ActiveG = Math.Clamp(g - 0.08f, 0f, 1f);
        ActiveB = Math.Clamp(b - 0.08f, 0f, 1f);
        ActiveA = 1f;
    }

    /// <summary>
    /// Initializes a new <see cref="UmbraCustomButtonColorsAttribute"/> with explicit RGBA values
    /// for the normal, hovered, and active button states.
    /// </summary>
    public UmbraCustomButtonColorsAttribute(
        float normalR, float normalG, float normalB, float normalA,
        float hoveredR, float hoveredG, float hoveredB, float hoveredA,
        float activeR, float activeG, float activeB, float activeA)
    {
        NormalR = normalR; NormalG = normalG; NormalB = normalB; NormalA = normalA;
        HoveredR = hoveredR; HoveredG = hoveredG; HoveredB = hoveredB; HoveredA = hoveredA;
        ActiveR = activeR; ActiveG = activeG; ActiveB = activeB; ActiveA = activeA;
    }
}

/// <summary>
/// Preferred collision-safe attribute that binds a custom parameter drawer.
/// </summary>
/// <typeparam name="TDrawer">The custom <see cref="IParameterDrawer"/> implementation to use.</typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraCustomDrawerAttribute<TDrawer> : Attribute, ICustomDrawerAttribute
    where TDrawer : IParameterDrawer, new()
{
    /// <summary>Gets the type of the custom drawer used to render this parameter.</summary>
    public Type DrawerType => typeof(TDrawer);
}

/// <summary>
/// Preferred collision-safe attribute that supplies descriptive help text for a settings member.
/// </summary>
/// <param name="text">The descriptive text to display.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraDescriptionAttribute(string text) : Attribute
{
    /// <summary>Gets the description text for the parameter.</summary>
    public string Text { get; } = text;
}

/// <summary>
/// Preferred collision-safe attribute that supplies a human-readable UI label for a settings member.
/// </summary>
/// <param name="name">The display name to show in the UI.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraDisplayNameAttribute(string name) : Attribute
{
    /// <summary>Gets the display name of the parameter.</summary>
    public string Name { get; } = name;
}

/// <summary>
/// Preferred collision-safe attribute that overrides the ImGui/printf format string for a numeric parameter.
/// </summary>
/// <param name="format">A printf-style format string compatible with ImGui.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraFormatAttribute(string format) : Attribute
{
    /// <summary>Gets the printf-style format string used to display the parameter's value.</summary>
    public string Format { get; } = format;
}

/// <summary>
/// Preferred collision-safe attribute that hides a settings parameter when a named member satisfies a condition.
/// </summary>
/// <typeparam name="T">The type of the member value to compare against.</typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraHideIfAttribute<T> : Attribute, IHideIfAttribute
{
    /// <summary>Gets the name of the property or field on the configuration class to evaluate.</summary>
    public string MemberName { get; }

    /// <summary>
    /// Gets the value to compare the member against. When <see cref="HasValue"/> is
    /// <see langword="false"/>, the member is treated as a plain <c>bool</c>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets a value indicating whether an explicit comparison value was provided.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Hides this parameter while the named <c>bool</c> member on the configuration class is <c>true</c>.
    /// </summary>
    public UmbraHideIfAttribute(string memberName)
    {
        MemberName = memberName;
        Value = default;
        HasValue = false;
    }

    /// <summary>
    /// Hides this parameter while the named member on the configuration class equals <paramref name="value"/>.
    /// </summary>
    public UmbraHideIfAttribute(string memberName, T value)
    {
        MemberName = memberName;
        Value = value;
        HasValue = true;
    }

    /// <inheritdoc/>
    object? IHideIfAttribute.BoxedValue => Value;
}

/// <summary>
/// Preferred collision-safe attribute that indents a parameter or settings group in the UI.
/// </summary>
/// <param name="amount">The indentation width in pixels, or <c>0</c> to use ImGui's default indent spacing.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraIndentAttribute(float amount = 0f) : Attribute
{
    /// <summary>Gets the indentation width in pixels.</summary>
    public float Amount { get; } = amount;
}

/// <summary>
/// Preferred collision-safe attribute that adds extra margin between the label column and editing widget.
/// </summary>
/// <param name="pixels">Extra pixels to insert between the label column and the editing widget.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraLabelMarginAttribute(float pixels) : Attribute
{
    /// <summary>Gets the extra pixel gap inserted between the label column and the editing widget.</summary>
    public float Pixels { get; } = pixels;
}

/// <summary>
/// Preferred collision-safe attribute that limits the maximum string length for an input field.
/// </summary>
/// <param name="length">The maximum number of characters the input field will accept.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraMaxLengthAttribute(uint length) : Attribute
{
    /// <summary>Gets the maximum number of characters allowed in the input field.</summary>
    public uint Length { get; } = length;
}

/// <summary>
/// Preferred collision-safe attribute that switches a string parameter to a multi-line text box.
/// </summary>
/// <param name="lines">The visible line count used to derive the control height.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraMultilineAttribute(int lines = 3) : Attribute
{
    /// <summary>Gets the number of visible text lines used to calculate the control height.</summary>
    public int Lines { get; } = lines;
}

/// <summary>
/// Preferred collision-safe attribute that binds a custom nested-group drawer.
/// </summary>
/// <typeparam name="TDrawer">The nested-group drawer implementation to use.</typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraNestedGroupDrawerAttribute<TDrawer> : Attribute, INestedGroupDrawerAttribute
    where TDrawer : class, new()
{
    /// <summary>Gets the type of the nested-group drawer.</summary>
    public Type DrawerType => typeof(TDrawer);
}

/// <summary>
/// Preferred collision-safe attribute that controls parameter ordering within a category context.
/// </summary>
/// <param name="order">The sort key. Lower values appear first within the same category context.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraParameterOrderAttribute(int order) : Attribute
{
    /// <summary>Gets the sort key for this parameter within its category context.</summary>
    public int Order { get; } = order;
}

/// <summary>
/// Preferred collision-safe attribute that declares numeric min/max bounds.
/// </summary>
/// <param name="min">The minimum allowable value.</param>
/// <param name="max">The maximum allowable value.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraRangeAttribute(double min, double max) : Attribute
{
    /// <summary>Gets the minimum allowable value for the parameter.</summary>
    public double Min { get; } = min;

    /// <summary>Gets the maximum allowable value for the parameter.</summary>
    public double Max { get; } = max;
}

/// <summary>
/// Preferred collision-safe attribute that marks a public instance property as a settings parameter.
/// </summary>
/// <param name="keyOverride">An optional explicit key used to store and retrieve this parameter.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraSettingsParameterAttribute(string? keyOverride = null) : Attribute
{
    /// <summary>Gets the explicit key override for this parameter.</summary>
    public string? KeyOverride { get; } = keyOverride;
}

/// <summary>
/// Preferred collision-safe attribute that prefixes parameter keys within a settings group.
/// </summary>
/// <param name="prefix">The prefix string to prepend to each parameter key.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraSettingsPrefixAttribute(string prefix) : Attribute
{
    /// <summary>Gets the prefix string applied to parameter keys in the decorated scope.</summary>
    public string Prefix { get; } = prefix;
}

/// <summary>
/// Preferred collision-safe attribute that inserts spacing below a parameter.
/// </summary>
/// <param name="count">The number of spacing lines to insert.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraSpacingAfterAttribute(int count = 1) : Attribute
{
    /// <summary>Gets the number of spacing lines to insert after the parameter control.</summary>
    public int Count { get; } = count;
}

/// <summary>
/// Preferred collision-safe attribute that inserts spacing above a parameter.
/// </summary>
/// <param name="count">The number of spacing lines to insert.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraSpacingBeforeAttribute(int count = 1) : Attribute
{
    /// <summary>Gets the number of spacing lines to insert before the parameter control.</summary>
    public int Count { get; } = count;
}

/// <summary>
/// Preferred collision-safe attribute that sets drag speed for unconstrained numeric controls.
/// </summary>
/// <param name="step">The drag speed for unconstrained numeric controls.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraStepAttribute(double step) : Attribute
{
    /// <summary>Gets the drag speed for the parameter.</summary>
    public double Step { get; } = step;
}

/// <summary>
/// Preferred collision-safe attribute that binds a two-column custom parameter drawer.
/// </summary>
/// <typeparam name="TDrawer">The two-column custom drawer implementation to use.</typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraTwoColumnCustomDrawerAttribute<TDrawer> : Attribute, ITwoColumnCustomDrawerAttribute
    where TDrawer : ITwoColumnParameterDrawer, new()
{
    /// <summary>Gets the type of the two-column custom drawer.</summary>
    public Type DrawerType => typeof(TDrawer);
}
