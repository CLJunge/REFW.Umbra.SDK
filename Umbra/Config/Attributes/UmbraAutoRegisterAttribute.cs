namespace Umbra.Config.Attributes;

/// <summary>
/// Marks a configuration type so Umbra discovers its annotated parameter properties during <see cref="ConfigStore{TConfig}.Load()"/>.
/// </summary>
/// <remarks>
/// The registration pipeline traverses only types marked with this attribute. Nested group types exposed through <see cref="UmbraParameterAttribute"/> properties must also carry it, otherwise their child parameters are ignored.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraAutoRegisterAttribute : Attribute { }
