namespace Umbra.Config.Attributes;

/// <summary>
/// Marks a class or struct so that its settings parameters are automatically
/// discovered and registered when <see cref="SettingsStore{TConfig}.Load()"/>
/// is called on a <c>SettingsStore</c> that wraps the decorated type.
/// </summary>
/// <remarks>
/// If this attribute is absent from the root config type passed to
/// <see cref="SettingsStore{TConfig}.Load()"/>, no parameters are discovered
/// and the returned instance will hold only its property default values.
/// Nested group types exposed via <c>UmbraSettingsParameterAttribute</c> properties must also
/// carry this attribute to be traversed.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraAutoRegisterSettingsAttribute : Attribute { }
