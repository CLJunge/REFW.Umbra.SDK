using Umbra.Config;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Contains focused unit tests for <see cref="DisablePredicateResolver"/>.
/// </summary>
[TestClass]
public sealed class DisablePredicateResolverTests
{
    /// <summary>
    /// Verifies that a missing disable condition produces a predicate that never disables the control.
    /// </summary>
    [TestMethod]
    public void Build_WithNullDisableIf_ReturnsAlwaysFalsePredicate()
    {
        var predicate = DisablePredicateResolver.Build(null, new TestOwner());

        Assert.IsFalse(predicate());
    }

    /// <summary>
    /// Verifies that a bool disable condition without an explicit value disables only while the member is <see langword="true"/>.
    /// </summary>
    [TestMethod]
    public void Build_WithBoolMemberAndNoExplicitValue_ReevaluatesCurrentValue()
    {
        var owner = new TestOwner { BoolProperty = false };
        var predicate = DisablePredicateResolver.Build(new UmbraDisableIfAttribute<bool>(nameof(TestOwner.BoolProperty)), owner);

        Assert.IsFalse(predicate());

        owner.BoolProperty = true;

        Assert.IsTrue(predicate());
    }

    /// <summary>
    /// Verifies that explicit comparisons also unwrap <see cref="Parameter{T}"/> members.
    /// </summary>
    [TestMethod]
    public void Build_WithParameterMemberAndExplicitComparison_UsesUnderlyingValue()
    {
        var owner = new TestOwner { ParameterInt = new Parameter<int>(10) };
        var predicate = DisablePredicateResolver.Build(new UmbraDisableIfAttribute<int>(nameof(TestOwner.ParameterInt), 10), owner);

        Assert.IsTrue(predicate());

        owner.ParameterInt.Set(20);

        Assert.IsFalse(predicate());
    }

    /// <summary>
    /// Verifies that explicit null comparison also works for nullable parameter members.
    /// </summary>
    [TestMethod]
    public void Build_WithNullableParameterAndNullComparison_DisablesWhenUnderlyingValueIsNull()
    {
        var owner = new TestOwner { ParameterNullableInt = new Parameter<int?>(null) };
        var predicate = DisablePredicateResolver.Build(new UmbraDisableIfAttribute<int?>(nameof(TestOwner.ParameterNullableInt), null), owner);

        Assert.IsTrue(predicate());

        owner.ParameterNullableInt.Set(5);

        Assert.IsFalse(predicate());
    }

    /// <summary>
    /// Verifies that explicit comparison disables when the current member value equals the configured comparison value.
    /// </summary>
    [TestMethod]
    public void Build_WithExplicitComparisonValue_DisablesOnEquality()
    {
        var owner = new TestOwner { IntProperty = 42 };
        var predicate = DisablePredicateResolver.Build(new UmbraDisableIfAttribute<int>(nameof(TestOwner.IntProperty), 42), owner);

        Assert.IsTrue(predicate());

        owner.IntProperty = 99;

        Assert.IsFalse(predicate());
    }

    /// <summary>
    /// Verifies that explicit null comparison works for nullable members.
    /// </summary>
    [TestMethod]
    public void Build_WithNullableMemberAndNullComparison_DisablesWhenMemberIsNull()
    {
        var owner = new TestOwner { NullableIntProperty = null };
        var predicate = DisablePredicateResolver.Build(new UmbraDisableIfAttribute<int?>(nameof(TestOwner.NullableIntProperty), null), owner);

        Assert.IsTrue(predicate());

        owner.NullableIntProperty = 10;

        Assert.IsFalse(predicate());
    }

    /// <summary>
    /// Verifies that DisableIf can read a private field by name.
    /// </summary>
    [TestMethod]
    public void Build_WithPrivateFieldReference_ReadsPrivateField()
    {
        var owner = new TestOwner();
        owner.SetPrivateField(true);
        var predicate = DisablePredicateResolver.Build(new UmbraDisableIfAttribute<bool>("_privateField"), owner);

        Assert.IsTrue(predicate());
    }

    /// <summary>
    /// Verifies that explicit comparison also works for private fields and reevaluates after the field changes.
    /// </summary>
    [TestMethod]
    public void Build_WithPrivateFieldAndExplicitComparison_ReevaluatesCurrentValue()
    {
        var owner = new TestOwner();
        owner.SetPrivateField(false);
        var predicate = DisablePredicateResolver.Build(new UmbraDisableIfAttribute<bool>("_privateField", false), owner);

        Assert.IsTrue(predicate());

        owner.SetPrivateField(true);

        Assert.IsFalse(predicate());
    }

    /// <summary>
    /// Verifies that invalid member names are ignored and leave the control enabled.
    /// </summary>
    [TestMethod]
    public void Build_WithInvalidMemberName_ReturnsAlwaysFalsePredicate()
    {
        var predicate = DisablePredicateResolver.Build(new UmbraDisableIfAttribute<bool>("MissingMember"), new TestOwner());

        Assert.IsFalse(predicate());
    }

    /// <summary>
    /// Verifies that <see cref="Parameter{T}"/> members are unwrapped and reevaluated on each predicate invocation.
    /// </summary>
    [TestMethod]
    public void Build_WithParameterMember_UsesUnderlyingValue()
    {
        var owner = new TestOwner { ParameterBool = new Parameter<bool>(false) };
        var predicate = DisablePredicateResolver.Build(new UmbraDisableIfAttribute<bool>(nameof(TestOwner.ParameterBool)), owner);

        Assert.IsFalse(predicate());

        owner.ParameterBool.Set(true);

        Assert.IsTrue(predicate());
    }

    /// <summary>
    /// Test owner containing the supported DisableIf member shapes used by these tests.
    /// </summary>
    private sealed class TestOwner
    {
        private bool _privateField;

        public bool BoolProperty { get; set; }

        public int IntProperty { get; set; }

        public int? NullableIntProperty { get; set; }

        public Parameter<bool> ParameterBool { get; set; } = new(false);

        public Parameter<int> ParameterInt { get; set; } = new(0);

        public Parameter<int?> ParameterNullableInt { get; set; } = new(null);

        public void SetPrivateField(bool value) => _privateField = value;
    }
}
