using Umbra.Config;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Contains focused unit tests for <see cref="VisibilityPredicateResolver"/>.
/// </summary>
[TestClass]
public sealed class VisibilityPredicateResolverTests
{
    /// <summary>
    /// Verifies that a missing hide condition produces a predicate that always keeps the control visible.
    /// </summary>
    [TestMethod]
    public void Build_WithNullHideIf_ReturnsAlwaysTruePredicate()
    {
        var predicate = VisibilityPredicateResolver.Build(null, new TestOwner());

        Assert.IsTrue(predicate());
    }

    /// <summary>
    /// Verifies that a bool hide condition without an explicit value hides only while the member is <see langword="true"/>.
    /// </summary>
    [TestMethod]
    public void Build_WithBoolMemberAndNoExplicitValue_ReevaluatesCurrentValue()
    {
        var owner = new TestOwner { BoolProperty = false };
        var predicate = VisibilityPredicateResolver.Build(new UmbraHideIfAttribute<bool>(nameof(TestOwner.BoolProperty)), owner);

        Assert.IsTrue(predicate());

        owner.BoolProperty = true;

        Assert.IsFalse(predicate());
    }

    /// <summary>
    /// Verifies that explicit comparisons also unwrap <see cref="Parameter{T}"/> members.
    /// </summary>
    [TestMethod]
    public void Build_WithParameterMemberAndExplicitComparison_UsesUnderlyingValue()
    {
        var owner = new TestOwner { ParameterInt = new Parameter<int>(10) };
        var predicate = VisibilityPredicateResolver.Build(new UmbraHideIfAttribute<int>(nameof(TestOwner.ParameterInt), 10), owner);

        Assert.IsFalse(predicate());

        owner.ParameterInt.Set(20);

        Assert.IsTrue(predicate());
    }

    /// <summary>
    /// Verifies that explicit null comparison also works for nullable parameter members.
    /// </summary>
    [TestMethod]
    public void Build_WithNullableParameterAndNullComparison_HidesWhenUnderlyingValueIsNull()
    {
        var owner = new TestOwner { ParameterNullableInt = new Parameter<int?>(null) };
        var predicate = VisibilityPredicateResolver.Build(new UmbraHideIfAttribute<int?>(nameof(TestOwner.ParameterNullableInt), null), owner);

        Assert.IsFalse(predicate());

        owner.ParameterNullableInt.Set(5);

        Assert.IsTrue(predicate());
    }

    /// <summary>
    /// Verifies that explicit comparison hides when the current member value equals the configured comparison value.
    /// </summary>
    [TestMethod]
    public void Build_WithExplicitComparisonValue_HidesOnEquality()
    {
        var owner = new TestOwner { IntProperty = 42 };
        var predicate = VisibilityPredicateResolver.Build(new UmbraHideIfAttribute<int>(nameof(TestOwner.IntProperty), 42), owner);

        Assert.IsFalse(predicate());

        owner.IntProperty = 99;

        Assert.IsTrue(predicate());
    }

    /// <summary>
    /// Verifies that explicit null comparison works for nullable members.
    /// </summary>
    [TestMethod]
    public void Build_WithNullableMemberAndNullComparison_HidesWhenMemberIsNull()
    {
        var owner = new TestOwner { NullableIntProperty = null };
        var predicate = VisibilityPredicateResolver.Build(new UmbraHideIfAttribute<int?>(nameof(TestOwner.NullableIntProperty), null), owner);

        Assert.IsFalse(predicate());

        owner.NullableIntProperty = 10;

        Assert.IsTrue(predicate());
    }

    /// <summary>
    /// Verifies that HideIf can read a private field by name.
    /// </summary>
    [TestMethod]
    public void Build_WithPrivateFieldReference_ReadsPrivateField()
    {
        var owner = new TestOwner();
        owner.SetPrivateField(true);
        var predicate = VisibilityPredicateResolver.Build(new UmbraHideIfAttribute<bool>("_privateField"), owner);

        Assert.IsFalse(predicate());
    }

    /// <summary>
    /// Verifies that invalid member names are ignored and leave the control visible.
    /// </summary>
    [TestMethod]
    public void Build_WithInvalidMemberName_ReturnsAlwaysTruePredicate()
    {
        var predicate = VisibilityPredicateResolver.Build(new UmbraHideIfAttribute<bool>("MissingMember"), new TestOwner());

        Assert.IsTrue(predicate());
    }

    /// <summary>
    /// Verifies that <see cref="Parameter{T}"/> members are unwrapped and reevaluated on each predicate invocation.
    /// </summary>
    [TestMethod]
    public void Build_WithParameterMember_UsesUnderlyingValue()
    {
        var owner = new TestOwner { ParameterBool = new Parameter<bool>(false) };
        var predicate = VisibilityPredicateResolver.Build(new UmbraHideIfAttribute<bool>(nameof(TestOwner.ParameterBool)), owner);

        Assert.IsTrue(predicate());

        owner.ParameterBool.Set(true);

        Assert.IsFalse(predicate());
    }

    /// <summary>
    /// Test owner containing the supported HideIf member shapes used by these tests.
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
