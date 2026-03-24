using System.Collections.Concurrent;
using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.Config.UI;

/// <summary>
/// Caches all class-level metadata attributes consulted by <see cref="ConfigDrawerBuilder.Collect"/>
/// in a single <see cref="MemberInfo.GetCustomAttributes(bool)"/> pass per type, eliminating the five
/// repeated per-attribute <c>GetCustomAttribute</c> calls that would otherwise be made on the same
/// <see cref="Type"/> object at the top of every <c>Collect</c> invocation.
/// </summary>
/// <remarks>
/// Instances are keyed by <see cref="Type"/> identity in a thread-safe static cache. Types are only
/// reflected over once per <see cref="System.AppDomain"/> lifetime; subsequent <c>Collect</c>
/// calls for the same type return the cached result immediately.
/// </remarks>
internal sealed class TypeDrawMetadata
{
    private static readonly ConcurrentDictionary<Type, TypeDrawMetadata> s_cache = new();

    /// <summary>
    /// Category name from <c>CategoryAttribute</c>, or <see langword="null"/> when absent.
    /// Used as the fallback category for all leaf parameters that declare no category of their own.
    /// </summary>
    internal string? Category { get; }

    /// <summary>
    /// Class-level <c>IndentAttribute</c>, or <see langword="null"/> when absent.
    /// Applied as a fallback indent to every parameter control whose own <see cref="ParameterMetadata"/>
    /// carries no <c>IndentAttribute</c>.
    /// </summary>
    internal IndentAttribute? IndentAttr { get; }

    /// <summary>
    /// <c>CollapseAsTreeAttribute</c>, or <see langword="null"/> when absent.
    /// Passed directly to <c>EmitCategoryHeader</c> to control whether the category block renders
    /// as a collapsible <c>ImGui.TreeNode</c> or a flat <c>ImGui.SeparatorText</c>.
    /// </summary>
    internal CollapseAsTreeAttribute? CollapseAttr { get; }

    /// <summary>
    /// <c>LabelMarginAttribute</c>, or <see langword="null"/> when absent.
    /// When present, its <see cref="LabelMarginAttribute.Pixels"/> value overrides the
    /// default label-column width for all parameters in this type.
    /// </summary>
    internal LabelMarginAttribute? LabelMarginAttr { get; }

    /// <summary>
    /// Class-level <see cref="INestedGroupDrawerAttribute"/>, or <see langword="null"/> when absent.
    /// When non-<see langword="null"/>, <c>Collect</c> skips parameter expansion for this type and
    /// delegates rendering entirely to the associated custom drawer.
    /// </summary>
    internal INestedGroupDrawerAttribute? NestedGroupDrawerAttr { get; }

    /// <summary>
    /// Whether the type carries <c>AutoRegisterSettingsAttribute</c>.
    /// Used to detect direct (non-<c>Parameter&lt;T&gt;</c>) nested settings-group properties
    /// during the <c>Collect</c> property loop.
    /// </summary>
    internal bool IsAutoRegisterSettings { get; }

    /// <summary>
    /// The public instance properties of this type, cached from a single
    /// <c>Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)</c> call performed
    /// once in <see cref="Build"/>. Eliminates the repeated <c>PropertyInfo[]</c> array
    /// allocation made at the top of each <see cref="ConfigDrawerBuilder.Collect"/> invocation
    /// for the same type.
    /// </summary>
    internal PropertyInfo[] Properties { get; }

    private TypeDrawMetadata(
        string? category,
        IndentAttribute? indentAttr,
        CollapseAsTreeAttribute? collapseAttr,
        LabelMarginAttribute? labelMarginAttr,
        INestedGroupDrawerAttribute? nestedGroupDrawerAttr,
        bool isAutoRegisterSettings,
        PropertyInfo[] properties)
    {
        Category = category;
        IndentAttr = indentAttr;
        CollapseAttr = collapseAttr;
        LabelMarginAttr = labelMarginAttr;
        NestedGroupDrawerAttr = nestedGroupDrawerAttr;
        IsAutoRegisterSettings = isAutoRegisterSettings;
        Properties = properties;
    }

    /// <summary>
    /// Returns the cached <see cref="TypeDrawMetadata"/> for <paramref name="type"/>,
    /// building and caching it on first access via a single attribute scan.
    /// </summary>
    /// <param name="type">The type to read metadata for.</param>
    internal static TypeDrawMetadata For(Type type) => s_cache.GetOrAdd(type, Build);

    private static TypeDrawMetadata Build(Type type)
    {
        string? category = null;
        IndentAttribute? indentAttr = null;
        CollapseAsTreeAttribute? collapseAttr = null;
        LabelMarginAttribute? labelMarginAttr = null;
        INestedGroupDrawerAttribute? nestedDrawerAttr = null;
        var isAutoRegister = false;

        foreach (var a in type.GetCustomAttributes(inherit: true))
        {
            if (a is CategoryAttribute cat) { category = cat.Name; continue; }
            if (a is IndentAttribute ind) { indentAttr = ind; continue; }
            if (a is CollapseAsTreeAttribute col) { collapseAttr = col; continue; }
            if (a is LabelMarginAttribute lm) { labelMarginAttr = lm; continue; }
            if (a is INestedGroupDrawerAttribute ngd) { nestedDrawerAttr = ngd; continue; }
            if (a is AutoRegisterSettingsAttribute) isAutoRegister = true;
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return new TypeDrawMetadata(category, indentAttr, collapseAttr, labelMarginAttr, nestedDrawerAttr, isAutoRegister, properties);
    }
}
