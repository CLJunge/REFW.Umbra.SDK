using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra;


namespace Umbra.UnitTests;

/// <summary>
/// Unit tests for <see cref="ListExtensions"/> class.
/// </summary>
[TestClass]
public class ListExtensionsTests
{
    /// <summary>
    /// Test item class used to verify sort stability and ordering.
    /// </summary>
    private class TestItem
    {
        public int Key { get; set; }
        public int OriginalIndex { get; set; }

        public TestItem(int key, int originalIndex)
        {
            Key = key;
            OriginalIndex = originalIndex;
        }
    }

    /// <summary>
    /// Tests that SortBy returns immediately without modification when the list is empty.
    /// </summary>
    [TestMethod]
    public void SortBy_EmptyList_NoChangeAndNoException()
    {
        // Arrange
        var list = new List<TestItem>();

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(0, list.Count);
    }

    /// <summary>
    /// Tests that SortBy returns immediately without modification when the list contains a single element.
    /// </summary>
    [TestMethod]
    public void SortBy_SingleElement_NoChangeAndNoException()
    {
        // Arrange
        var list = new List<TestItem> { new TestItem(5, 0) };

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual(5, list[0].Key);
        Assert.AreEqual(0, list[0].OriginalIndex);
    }

    /// <summary>
    /// Tests that SortBy correctly sorts a two-element list in ascending order.
    /// </summary>
    [TestMethod]
    public void SortBy_TwoElements_SortsAscending()
    {
        // Arrange
        var list = new List<TestItem>
        {
            new TestItem(10, 0),
            new TestItem(5, 1)
        };

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(2, list.Count);
        Assert.AreEqual(5, list[0].Key);
        Assert.AreEqual(10, list[1].Key);
    }

    /// <summary>
    /// Tests that SortBy correctly sorts a list with fewer than 32 elements using insertion sort path.
    /// </summary>
    [TestMethod]
    public void SortBy_SmallListUnderThreshold_SortsCorrectly()
    {
        // Arrange
        var list = new List<TestItem>();
        for (int i = 10; i > 0; i--)
        {
            list.Add(new TestItem(i, 10 - i));
        }

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(10, list.Count);
        for (int i = 0; i < 10; i++)
        {
            Assert.AreEqual(i + 1, list[i].Key);
        }
    }

    /// <summary>
    /// Tests that SortBy correctly sorts a list with exactly 32 elements (boundary for insertion sort).
    /// </summary>
    [TestMethod]
    public void SortBy_ExactlyThresholdSize_SortsCorrectly()
    {
        // Arrange
        var list = new List<TestItem>();
        for (int i = 32; i > 0; i--)
        {
            list.Add(new TestItem(i, 32 - i));
        }

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(32, list.Count);
        for (int i = 0; i < 32; i++)
        {
            Assert.AreEqual(i + 1, list[i].Key);
        }
    }

    /// <summary>
    /// Tests that SortBy correctly sorts a list with 33 elements (triggers merge sort path).
    /// </summary>
    [TestMethod]
    public void SortBy_OverThreshold_SortsCorrectly()
    {
        // Arrange
        var list = new List<TestItem>();
        for (int i = 33; i > 0; i--)
        {
            list.Add(new TestItem(i, 33 - i));
        }

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(33, list.Count);
        for (int i = 0; i < 33; i++)
        {
            Assert.AreEqual(i + 1, list[i].Key);
        }
    }

    /// <summary>
    /// Tests that SortBy correctly sorts a large list with more than 100 elements.
    /// </summary>
    [TestMethod]
    public void SortBy_LargeList_SortsCorrectly()
    {
        // Arrange
        var list = new List<TestItem>();
        for (int i = 150; i > 0; i--)
        {
            list.Add(new TestItem(i, 150 - i));
        }

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(150, list.Count);
        for (int i = 0; i < 150; i++)
        {
            Assert.AreEqual(i + 1, list[i].Key);
        }
    }

    /// <summary>
    /// Tests that SortBy maintains order when the list is already sorted in ascending order.
    /// </summary>
    [TestMethod]
    public void SortBy_AlreadySorted_RemainsOrdered()
    {
        // Arrange
        var list = new List<TestItem>();
        for (int i = 0; i < 50; i++)
        {
            list.Add(new TestItem(i, i));
        }

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(50, list.Count);
        for (int i = 0; i < 50; i++)
        {
            Assert.AreEqual(i, list[i].Key);
        }
    }

    /// <summary>
    /// Tests that SortBy correctly sorts a list that is in reverse order.
    /// </summary>
    [TestMethod]
    public void SortBy_ReverseSorted_SortsCorrectly()
    {
        // Arrange
        var list = new List<TestItem>();
        for (int i = 50; i > 0; i--)
        {
            list.Add(new TestItem(i, 50 - i));
        }

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(50, list.Count);
        for (int i = 0; i < 50; i++)
        {
            Assert.AreEqual(i + 1, list[i].Key);
        }
    }

    /// <summary>
    /// Tests that SortBy preserves the original order when all elements have the same key (stability test).
    /// </summary>
    [TestMethod]
    public void SortBy_AllEqualKeys_PreservesOriginalOrder()
    {
        // Arrange
        var list = new List<TestItem>();
        for (int i = 0; i < 40; i++)
        {
            list.Add(new TestItem(100, i));
        }

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(40, list.Count);
        for (int i = 0; i < 40; i++)
        {
            Assert.AreEqual(100, list[i].Key);
            Assert.AreEqual(i, list[i].OriginalIndex);
        }
    }

    /// <summary>
    /// Tests that SortBy preserves relative order of elements with duplicate keys (stability test).
    /// </summary>
    [TestMethod]
    public void SortBy_DuplicateKeys_PreservesRelativeOrder()
    {
        // Arrange
        var list = new List<TestItem>
        {
            new TestItem(5, 0),
            new TestItem(3, 1),
            new TestItem(5, 2),
            new TestItem(3, 3),
            new TestItem(5, 4),
            new TestItem(1, 5),
            new TestItem(3, 6)
        };

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(7, list.Count);
        Assert.AreEqual(1, list[0].Key);
        Assert.AreEqual(5, list[0].OriginalIndex);

        // Key 3 items should maintain relative order
        Assert.AreEqual(3, list[1].Key);
        Assert.AreEqual(1, list[1].OriginalIndex);
        Assert.AreEqual(3, list[2].Key);
        Assert.AreEqual(3, list[2].OriginalIndex);
        Assert.AreEqual(3, list[3].Key);
        Assert.AreEqual(6, list[3].OriginalIndex);

        // Key 5 items should maintain relative order
        Assert.AreEqual(5, list[4].Key);
        Assert.AreEqual(0, list[4].OriginalIndex);
        Assert.AreEqual(5, list[5].Key);
        Assert.AreEqual(2, list[5].OriginalIndex);
        Assert.AreEqual(5, list[6].Key);
        Assert.AreEqual(4, list[6].OriginalIndex);
    }

    /// <summary>
    /// Tests that SortBy preserves stability with duplicate keys in a list over the threshold size.
    /// </summary>
    [TestMethod]
    public void SortBy_DuplicateKeysLargeList_PreservesRelativeOrder()
    {
        // Arrange
        var list = new List<TestItem>();
        for (int i = 0; i < 50; i++)
        {
            list.Add(new TestItem(i % 5, i));
        }

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(50, list.Count);

        // Verify items are sorted by key
        for (int i = 1; i < 50; i++)
        {
            Assert.IsTrue(list[i - 1].Key <= list[i].Key);
        }

        // Verify stability: items with same key maintain original relative order
        for (int key = 0; key < 5; key++)
        {
            int previousIndex = -1;
            for (int i = 0; i < 50; i++)
            {
                if (list[i].Key == key)
                {
                    if (previousIndex >= 0)
                    {
                        Assert.IsTrue(list[i].OriginalIndex > previousIndex);
                    }
                    previousIndex = list[i].OriginalIndex;
                }
            }
        }
    }

    /// <summary>
    /// Tests that SortBy correctly handles extreme key values including int.MinValue and int.MaxValue.
    /// </summary>
    [TestMethod]
    public void SortBy_ExtremeKeyValues_HandlesCorrectly()
    {
        // Arrange
        var list = new List<TestItem>
        {
            new TestItem(int.MaxValue, 0),
            new TestItem(int.MinValue, 1),
            new TestItem(0, 2),
            new TestItem(int.MaxValue - 1, 3),
            new TestItem(int.MinValue + 1, 4)
        };

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(5, list.Count);
        Assert.AreEqual(int.MinValue, list[0].Key);
        Assert.AreEqual(int.MinValue + 1, list[1].Key);
        Assert.AreEqual(0, list[2].Key);
        Assert.AreEqual(int.MaxValue - 1, list[3].Key);
        Assert.AreEqual(int.MaxValue, list[4].Key);
    }

    /// <summary>
    /// Tests that SortBy correctly handles negative key values.
    /// </summary>
    [TestMethod]
    public void SortBy_NegativeKeys_SortsCorrectly()
    {
        // Arrange
        var list = new List<TestItem>
        {
            new TestItem(-5, 0),
            new TestItem(-10, 1),
            new TestItem(-1, 2),
            new TestItem(-100, 3),
            new TestItem(-50, 4)
        };

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(5, list.Count);
        Assert.AreEqual(-100, list[0].Key);
        Assert.AreEqual(-50, list[1].Key);
        Assert.AreEqual(-10, list[2].Key);
        Assert.AreEqual(-5, list[3].Key);
        Assert.AreEqual(-1, list[4].Key);
    }

    /// <summary>
    /// Tests that SortBy correctly handles a mix of positive, negative, and zero key values.
    /// </summary>
    [TestMethod]
    public void SortBy_MixedPositiveNegativeZero_SortsCorrectly()
    {
        // Arrange
        var list = new List<TestItem>
        {
            new TestItem(10, 0),
            new TestItem(-10, 1),
            new TestItem(0, 2),
            new TestItem(5, 3),
            new TestItem(-5, 4),
            new TestItem(0, 5),
            new TestItem(15, 6),
            new TestItem(-15, 7)
        };

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(8, list.Count);
        Assert.AreEqual(-15, list[0].Key);
        Assert.AreEqual(-10, list[1].Key);
        Assert.AreEqual(-5, list[2].Key);
        Assert.AreEqual(0, list[3].Key);
        Assert.AreEqual(0, list[4].Key);
        Assert.AreEqual(5, list[5].Key);
        Assert.AreEqual(10, list[6].Key);
        Assert.AreEqual(15, list[7].Key);

        // Verify stability for zero keys
        Assert.AreEqual(2, list[3].OriginalIndex);
        Assert.AreEqual(5, list[4].OriginalIndex);
    }

    /// <summary>
    /// Tests that SortBy correctly sorts a list of integers using an identity key selector.
    /// </summary>
    [TestMethod]
    public void SortBy_IntegerList_SortsCorrectly()
    {
        // Arrange
        var list = new List<int> { 5, 2, 8, 1, 9, 3, 7, 4, 6 };

        // Act
        list.SortBy(x => x);

        // Assert
        Assert.AreEqual(9, list.Count);
        for (int i = 0; i < 9; i++)
        {
            Assert.AreEqual(i + 1, list[i]);
        }
    }

    /// <summary>
    /// Tests that SortBy correctly sorts a list of strings by their length.
    /// </summary>
    [TestMethod]
    public void SortBy_StringsByLength_SortsCorrectly()
    {
        // Arrange
        var list = new List<string> { "hello", "a", "world", "ab", "test" };

        // Act
        list.SortBy(s => s.Length);

        // Assert
        Assert.AreEqual(5, list.Count);
        Assert.AreEqual("a", list[0]);
        Assert.AreEqual("ab", list[1]);
        Assert.AreEqual("test", list[2]);
        Assert.AreEqual("hello", list[3]);
        Assert.AreEqual("world", list[4]);
    }

    /// <summary>
    /// Tests that SortBy handles a boundary case with exactly 31 elements (just under threshold).
    /// </summary>
    [TestMethod]
    public void SortBy_ThirtyOneElements_SortsCorrectly()
    {
        // Arrange
        var list = new List<TestItem>();
        for (int i = 31; i > 0; i--)
        {
            list.Add(new TestItem(i, 31 - i));
        }

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(31, list.Count);
        for (int i = 0; i < 31; i++)
        {
            Assert.AreEqual(i + 1, list[i].Key);
        }
    }

    /// <summary>
    /// Tests that SortBy correctly handles a random permutation of values.
    /// </summary>
    [TestMethod]
    public void SortBy_RandomPermutation_SortsCorrectly()
    {
        // Arrange
        var list = new List<TestItem>
        {
            new TestItem(15, 0), new TestItem(3, 1), new TestItem(22, 2), new TestItem(8, 3),
            new TestItem(45, 4), new TestItem(1, 5), new TestItem(67, 6), new TestItem(12, 7),
            new TestItem(9, 8), new TestItem(33, 9), new TestItem(5, 10), new TestItem(88, 11)
        };

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(12, list.Count);
        for (int i = 1; i < 12; i++)
        {
            Assert.IsTrue(list[i - 1].Key <= list[i].Key);
        }
    }

    /// <summary>
    /// Tests that SortBy with zero as keys sorts correctly.
    /// </summary>
    [TestMethod]
    public void SortBy_AllZeroKeys_PreservesOriginalOrder()
    {
        // Arrange
        var list = new List<TestItem>();
        for (int i = 0; i < 20; i++)
        {
            list.Add(new TestItem(0, i));
        }

        // Act
        list.SortBy(item => item.Key);

        // Assert
        Assert.AreEqual(20, list.Count);
        for (int i = 0; i < 20; i++)
        {
            Assert.AreEqual(0, list[i].Key);
            Assert.AreEqual(i, list[i].OriginalIndex);
        }
    }
}