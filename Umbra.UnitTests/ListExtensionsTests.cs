namespace Umbra.UnitTests;

/// <summary>
/// Contains focused unit tests for <see cref="ListExtensions"/>.
/// </summary>
[TestClass]
public sealed class ListExtensionsTests
{
    /// <summary>
    /// Test item used to verify ordering and sort stability.
    /// </summary>
    private sealed class TestItem(int key, int originalIndex)
    {
        public int Key { get; } = key;

        public int OriginalIndex { get; } = originalIndex;
    }

    /// <summary>
    /// Verifies that lists with fewer than two items are left unchanged.
    /// </summary>
    [TestMethod]
    public void SortBy_ListWithFewerThanTwoItems_LeavesListUnchanged()
    {
        var empty = new List<TestItem>();
        var single = new List<TestItem> { new(5, 0) };

        empty.SortBy(item => item.Key);
        single.SortBy(item => item.Key);

        Assert.IsEmpty(empty);
        Assert.HasCount(1, single);
        Assert.AreEqual(5, single[0].Key);
        Assert.AreEqual(0, single[0].OriginalIndex);
    }

    /// <summary>
    /// Verifies that small lists are sorted ascending.
    /// </summary>
    [TestMethod]
    public void SortBy_SmallList_SortsAscending()
    {
        var list = new List<TestItem>();
        for (var i = 10; i > 0; i--)
            list.Add(new TestItem(i, 10 - i));

        list.SortBy(item => item.Key);

        Assert.HasCount(10, list);
        for (var i = 0; i < list.Count; i++)
            Assert.AreEqual(i + 1, list[i].Key);
    }

    /// <summary>
    /// Verifies that larger lists are sorted ascending.
    /// </summary>
    [TestMethod]
    public void SortBy_LargeList_SortsAscending()
    {
        var list = new List<TestItem>();
        for (var i = 33; i > 0; i--)
            list.Add(new TestItem(i, 33 - i));

        list.SortBy(item => item.Key);

        Assert.HasCount(33, list);
        for (var i = 0; i < list.Count; i++)
            Assert.AreEqual(i + 1, list[i].Key);
    }

    /// <summary>
    /// Verifies that sorting remains stable when duplicate keys are present.
    /// </summary>
    [TestMethod]
    public void SortBy_DuplicateKeys_PreservesRelativeOrder()
    {
        var list = new List<TestItem>();
        for (var i = 0; i < 50; i++)
            list.Add(new TestItem(i % 5, i));

        list.SortBy(item => item.Key);

        Assert.HasCount(50, list);
        for (var i = 1; i < list.Count; i++)
            Assert.IsGreaterThanOrEqualTo(list[i - 1].Key, list[i].Key);

        for (var key = 0; key < 5; key++)
        {
            var previousIndex = -1;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Key != key)
                    continue;

                if (previousIndex >= 0)
                    Assert.IsLessThan(previousIndex, list[i].OriginalIndex);

                previousIndex = list[i].OriginalIndex;
            }
        }
    }

    /// <summary>
    /// Verifies that sorting handles mixed negative, zero, and extreme integer keys.
    /// </summary>
    [TestMethod]
    public void SortBy_MixedIntegerKeys_SortsCorrectly()
    {
        var list = new List<TestItem>
        {
            new(int.MaxValue, 0),
            new(-10, 1),
            new(0, 2),
            new(int.MinValue, 3),
            new(10, 4),
            new(0, 5),
        };

        list.SortBy(item => item.Key);

        Assert.AreEqual(int.MinValue, list[0].Key);
        Assert.AreEqual(-10, list[1].Key);
        Assert.AreEqual(0, list[2].Key);
        Assert.AreEqual(0, list[3].Key);
        Assert.AreEqual(10, list[4].Key);
        Assert.AreEqual(int.MaxValue, list[5].Key);
        Assert.AreEqual(2, list[2].OriginalIndex);
        Assert.AreEqual(5, list[3].OriginalIndex);
    }

    private static readonly string[] expectedOrder = ["a", "ab", "test", "hello", "world"];

    /// <summary>
    /// Verifies that sorting works for other element types using the provided key selector.
    /// </summary>
    [TestMethod]
    public void SortBy_StringsByLength_SortsBySelectedKey()
    {
        var list = new List<string> { "hello", "a", "world", "ab", "test" };

        list.SortBy(static value => value.Length);

        CollectionAssert.AreEqual(expectedOrder, list);
    }
}
