using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config;


namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="VisibilityPredicateResolver"/>.
/// </summary>
[TestClass]
public class VisibilityPredicateResolverTests
{
    /// <summary>
    /// Tests that Build returns a predicate that always returns true when hideIf is null.
    /// </summary>
    [TestMethod]
    public void Build_NullHideIf_ReturnsAlwaysTruePredicate()
    {
        // Arrange
        var owner = new TestOwner();

        // Act
        var predicate = VisibilityPredicateResolver.Build(null, owner);
        var result = predicate();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates to true when a boolean member is false and no explicit value is provided.
    /// </summary>
    [TestMethod]
    public void Build_BoolMemberFalseNoExplicitValue_ReturnsTrue()
    {
        // Arrange
        var owner = new TestOwner { BoolProperty = false };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.BoolProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates to false when a boolean member is true and no explicit value is provided.
    /// </summary>
    [TestMethod]
    public void Build_BoolMemberTrueNoExplicitValue_ReturnsFalse()
    {
        // Arrange
        var owner = new TestOwner { BoolProperty = true };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.BoolProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates to true when a boolean member is null and no explicit value is provided.
    /// </summary>
    [TestMethod]
    public void Build_BoolMemberNullNoExplicitValue_ReturnsTrue()
    {
        // Arrange
        var owner = new TestOwner { NullableBoolProperty = null };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.NullableBoolProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates to false when member value equals the explicit comparison value.
    /// </summary>
    [TestMethod]
    public void Build_MemberEqualsExplicitValue_ReturnsFalse()
    {
        // Arrange
        var owner = new TestOwner { IntProperty = 42 };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.IntProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(42);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates to true when member value does not equal the explicit comparison value.
    /// </summary>
    [TestMethod]
    public void Build_MemberNotEqualsExplicitValue_ReturnsTrue()
    {
        // Arrange
        var owner = new TestOwner { IntProperty = 99 };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.IntProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(42);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates correctly when string member equals explicit value.
    /// </summary>
    [TestMethod]
    public void Build_StringMemberEqualsExplicitValue_ReturnsFalse()
    {
        // Arrange
        var owner = new TestOwner { StringProperty = "test" };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.StringProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns("test");

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates correctly when string member does not equal explicit value.
    /// </summary>
    [TestMethod]
    public void Build_StringMemberNotEqualsExplicitValue_ReturnsTrue()
    {
        // Arrange
        var owner = new TestOwner { StringProperty = "other" };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.StringProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns("test");

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates correctly when enum member equals explicit value.
    /// </summary>
    [TestMethod]
    public void Build_EnumMemberEqualsExplicitValue_ReturnsFalse()
    {
        // Arrange
        var owner = new TestOwner { EnumProperty = TestEnum.Option2 };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.EnumProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(TestEnum.Option2);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates correctly when nullable member is null and comparison value is null.
    /// </summary>
    [TestMethod]
    public void Build_NullableMemberNullCompareNull_ReturnsFalse()
    {
        // Arrange
        var owner = new TestOwner { NullableIntProperty = null };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.NullableIntProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates correctly when nullable member is null and comparison value is not null.
    /// </summary>
    [TestMethod]
    public void Build_NullableMemberNullCompareNotNull_ReturnsTrue()
    {
        // Arrange
        var owner = new TestOwner { NullableIntProperty = null };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.NullableIntProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(10);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates correctly when nullable member has value and comparison value is null.
    /// </summary>
    [TestMethod]
    public void Build_NullableMemberHasValueCompareNull_ReturnsTrue()
    {
        // Arrange
        var owner = new TestOwner { NullableIntProperty = 10 };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.NullableIntProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that evaluates correctly when accessing a private field.
    /// </summary>
    [TestMethod]
    public void Build_PrivateField_AccessesCorrectly()
    {
        // Arrange
        var owner = new TestOwner();
        owner.SetPrivateField(true);
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns("_privateField");
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build returns a predicate that always returns true when member is not found.
    /// </summary>
    [TestMethod]
    public void Build_InvalidMemberName_ReturnsAlwaysTruePredicate()
    {
        // Arrange
        var owner = new TestOwner();
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns("NonExistentMember");
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Build correctly unwraps Parameter&lt;T&gt; wrapper and evaluates the inner value.
    /// </summary>
    [TestMethod]
    public void Build_ParameterWrappedBool_EvaluatesInnerValue()
    {
        // Arrange
        var owner = new TestOwner { ParameterBool = new Parameter<bool>(true) };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.ParameterBool));
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build correctly unwraps Parameter&lt;T&gt; and compares with explicit value.
    /// </summary>
    [TestMethod]
    public void Build_ParameterWrappedIntWithExplicitValue_ComparesCorrectly()
    {
        // Arrange
        var owner = new TestOwner { ParameterInt = new Parameter<int>(100) };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.ParameterInt));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(100);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build predicate re-evaluates when owner member value changes.
    /// </summary>
    [TestMethod]
    public void Build_MemberValueChanges_PredicateReflectsNewValue()
    {
        // Arrange
        var owner = new TestOwner { BoolProperty = false };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.BoolProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var resultBefore = predicate();
        owner.BoolProperty = true;
        var resultAfter = predicate();

        // Assert
        Assert.IsTrue(resultBefore);
        Assert.IsFalse(resultAfter);
    }

    /// <summary>
    /// Tests that Build predicate re-evaluates when Parameter&lt;T&gt; value changes.
    /// </summary>
    [TestMethod]
    public void Build_ParameterValueChanges_PredicateReflectsNewValue()
    {
        // Arrange
        var owner = new TestOwner { ParameterBool = new Parameter<bool>(false) };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.ParameterBool));
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var resultBefore = predicate();
        owner.ParameterBool.Set(true);
        var resultAfter = predicate();

        // Assert
        Assert.IsTrue(resultBefore);
        Assert.IsFalse(resultAfter);
    }

    /// <summary>
    /// Tests that Build uses cached accessor for the same owner type and member name.
    /// </summary>
    [TestMethod]
    public void Build_SameOwnerTypeAndMemberName_UsesCachedAccessor()
    {
        // Arrange
        var owner1 = new TestOwner { IntProperty = 10 };
        var owner2 = new TestOwner { IntProperty = 20 };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.IntProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(10);

        // Act
        var predicate1 = VisibilityPredicateResolver.Build(mockHideIf.Object, owner1);
        var predicate2 = VisibilityPredicateResolver.Build(mockHideIf.Object, owner2);
        var result1 = predicate1();
        var result2 = predicate2();

        // Assert
        Assert.IsFalse(result1); // 10 == 10
        Assert.IsTrue(result2);  // 20 != 10
    }

    /// <summary>
    /// Tests that Build works correctly with int.MinValue.
    /// </summary>
    [TestMethod]
    public void Build_IntMinValue_HandlesCorrectly()
    {
        // Arrange
        var owner = new TestOwner { IntProperty = int.MinValue };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.IntProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(int.MinValue);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build works correctly with int.MaxValue.
    /// </summary>
    [TestMethod]
    public void Build_IntMaxValue_HandlesCorrectly()
    {
        // Arrange
        var owner = new TestOwner { IntProperty = int.MaxValue };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.IntProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(int.MaxValue);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build works correctly with empty string.
    /// </summary>
    [TestMethod]
    public void Build_EmptyString_HandlesCorrectly()
    {
        // Arrange
        var owner = new TestOwner { StringProperty = string.Empty };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.StringProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(string.Empty);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build works correctly with null string member.
    /// </summary>
    [TestMethod]
    public void Build_NullString_HandlesCorrectly()
    {
        // Arrange
        var owner = new TestOwner { StringProperty = null };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.StringProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns((object?)null);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build works correctly with whitespace string.
    /// </summary>
    [TestMethod]
    public void Build_WhitespaceString_HandlesCorrectly()
    {
        // Arrange
        var owner = new TestOwner { StringProperty = "   " };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.StringProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns("   ");

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build works correctly with double.NaN.
    /// </summary>
    [TestMethod]
    public void Build_DoubleNaN_HandlesCorrectly()
    {
        // Arrange
        var owner = new TestOwner { DoubleProperty = double.NaN };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.DoubleProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(double.NaN);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        // NaN != NaN in standard equality, so predicate should return true
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Build works correctly with double.PositiveInfinity.
    /// </summary>
    [TestMethod]
    public void Build_DoublePositiveInfinity_HandlesCorrectly()
    {
        // Arrange
        var owner = new TestOwner { DoubleProperty = double.PositiveInfinity };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.DoubleProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(double.PositiveInfinity);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build works correctly with double.NegativeInfinity.
    /// </summary>
    [TestMethod]
    public void Build_DoubleNegativeInfinity_HandlesCorrectly()
    {
        // Arrange
        var owner = new TestOwner { DoubleProperty = double.NegativeInfinity };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.DoubleProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(double.NegativeInfinity);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Build works correctly with zero integer value.
    /// </summary>
    [TestMethod]
    public void Build_ZeroIntValue_HandlesCorrectly()
    {
        // Arrange
        var owner = new TestOwner { IntProperty = 0 };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.IntProperty));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(0);

        // Act
        var predicate = VisibilityPredicateResolver.Build(mockHideIf.Object, owner);
        var result = predicate();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Test helper class with various property and field types for testing visibility predicate resolution.
    /// </summary>
    internal class TestOwner
    {
        private bool _privateField;

        public bool BoolProperty { get; set; }
        public bool? NullableBoolProperty { get; set; }
        public int IntProperty { get; set; }
        public int? NullableIntProperty { get; set; }
        public string? StringProperty { get; set; }
        public TestEnum EnumProperty { get; set; }
        public double DoubleProperty { get; set; }
        public Parameter<bool> ParameterBool { get; set; } = new(false);
        public Parameter<int> ParameterInt { get; set; } = new(0);

        public void SetPrivateField(bool value) => _privateField = value;
    }

    /// <summary>
    /// Test enum for testing enum member visibility conditions.
    /// </summary>
    internal enum TestEnum
    {
        Option1,
        Option2,
        Option3
    }
}