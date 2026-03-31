namespace Umbra.Config.Attributes.UnitTests;

/// <summary>
/// Contains focused unit tests for the <see cref="UmbraHideIfAttribute{T}"/> class.
/// </summary>
[TestClass]
public sealed class UmbraHideIfAttributeTests
{
    /// <summary>
    /// Verifies that the value constructor preserves the member name, stores the provided value,
    /// and marks the attribute as having an explicit comparison value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithExplicitValue_SetsProperties()
    {
        var attribute = new UmbraHideIfAttribute<int>("TestMember", 42);

        Assert.AreEqual("TestMember", attribute.MemberName);
        Assert.AreEqual(42, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the value constructor preserves a <see langword="null"/> reference value
    /// and still marks the attribute as having an explicit comparison value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithExplicitNullReferenceValue_SetsProperties()
    {
        var attribute = new UmbraHideIfAttribute<string?>("TestMember", null);

        Assert.AreEqual("TestMember", attribute.MemberName);
        Assert.IsNull(attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the single-parameter constructor leaves a reference-typed comparison value unset.
    /// </summary>
    [TestMethod]
    public void Constructor_WithoutExplicitValue_ForReferenceType_UsesNullAndHasValueFalse()
    {
        var attribute = new UmbraHideIfAttribute<string>("TestMember");

        Assert.AreEqual("TestMember", attribute.MemberName);
        Assert.IsNull(attribute.Value);
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the single-parameter constructor leaves a value-typed comparison value at its default
    /// while still reporting that no explicit comparison value was supplied.
    /// </summary>
    [TestMethod]
    public void Constructor_WithoutExplicitValue_ForValueType_UsesDefaultValueAndHasValueFalse()
    {
        var attribute = new UmbraHideIfAttribute<int>("TestMember");

        Assert.AreEqual("TestMember", attribute.MemberName);
        Assert.AreEqual(0, attribute.Value);
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the boxed value returned through <see cref="IHideIfAttribute"/> matches an explicit value.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithExplicitValue_ReturnsBoxedValue()
    {
        IHideIfAttribute attribute = new UmbraHideIfAttribute<DayOfWeek>("TestMember", DayOfWeek.Friday);

        Assert.AreEqual(DayOfWeek.Friday, attribute.BoxedValue);
        Assert.IsInstanceOfType<DayOfWeek>(attribute.BoxedValue);
    }

    /// <summary>
    /// Verifies that the boxed value returned through <see cref="IHideIfAttribute"/> is <see langword="null"/>
    /// when the single-parameter constructor is used for a reference type.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithoutExplicitValue_ForReferenceType_ReturnsNull()
    {
        IHideIfAttribute attribute = new UmbraHideIfAttribute<string>("TestMember");

        Assert.IsNull(attribute.BoxedValue);
    }

    /// <summary>
    /// Verifies that the boxed value returned through <see cref="IHideIfAttribute"/> is the boxed default value
    /// when the single-parameter constructor is used for a value type.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithoutExplicitValue_ForValueType_ReturnsBoxedDefault()
    {
        IHideIfAttribute attribute = new UmbraHideIfAttribute<int>("TestMember");

        Assert.AreEqual(0, attribute.BoxedValue);
        Assert.IsInstanceOfType<int>(attribute.BoxedValue);
    }

    /// <summary>
    /// Verifies that an explicit null nullable value is still treated as an explicit comparison value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithExplicitNullNullableValue_SetsHasValueTrueAndBoxedValueNull()
    {
        IHideIfAttribute attribute = new UmbraHideIfAttribute<int?>("TestMember", null);

        Assert.AreEqual("TestMember", attribute.MemberName);
        Assert.IsTrue(attribute.HasValue);
        Assert.IsNull(attribute.BoxedValue);
    }

    /// <summary>
    /// Verifies that the constructor preserves empty member names without modification.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyMemberName_PreservesMemberName()
    {
        var attribute = new UmbraHideIfAttribute<bool>(string.Empty);

        Assert.AreEqual(string.Empty, attribute.MemberName);
        Assert.IsFalse(attribute.HasValue);
    }
}
