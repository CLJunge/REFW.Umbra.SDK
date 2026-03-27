namespace Umbra.Config.Attributes;

/// <summary>
/// Marks a class or struct so that its settings parameters are automatically
/// discovered and registered when <see cref="SettingsStore{TConfig}.Load()"/>
/// is called on a <c>SettingsStore</c> that wraps the decorated type.
/// Apply this attribute to any settings group that should participate in
/// auto-registration without requiring manual registration calls.
/// </summary>
/// <remarks>
/// If this attribute is absent from the root config type passed to
/// <see cref="SettingsStore{TConfig}.Load()"/>, no parameters are discovered
/// and the returned instance will hold only its property default values.
/// Nested group types exposed via <c>SettingsParameterAttribute</c> properties must also
/// carry this attribute to be traversed, even when other nested-group behaviour such as
/// <c>[SettingsPrefix]</c>, <c>[Category]</c>, <c>[CollapseAsTree]</c>, or
/// <c>[NestedGroupDrawer]</c> is declared on the parent property instead of on the nested type.
/// The built-in registrar walks only public instance properties; fields are not part of the
/// automatic discovery path.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
[Obsolete("Use UmbraAutoRegisterSettingsAttribute instead for the collision-safe Umbra-prefixed name.")]
public class AutoRegisterSettingsAttribute : Attribute { }
