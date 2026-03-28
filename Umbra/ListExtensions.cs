namespace Umbra;

/// <summary>
/// Provides extension methods for working with <see cref="List{T}"/> instances.
/// </summary>
internal static class ListExtensions
{
    private const int StableInsertionSortThreshold = 32;

    /// <summary>
    /// Sorts <paramref name="list"/> in place, ordered ascending by the integer key returned by
    /// <paramref name="keySelector"/>. Elements with equal keys preserve their original relative
    /// order.
    /// </summary>
    /// <remarks>
    /// Uses a stable insertion sort for small lists to minimize overhead and switches to a stable
    /// merge sort for larger lists to reduce worst-case cost.
    /// </remarks>
    /// <typeparam name="T">The element type of the list.</typeparam>
    /// <param name="list">The list to sort in place.</param>
    /// <param name="keySelector">A function that returns the integer sort key for each element.</param>
    internal static void SortBy<T>(this List<T> list, Func<T, int> keySelector)
    {
        if (list.Count < 2)
            return;

        if (list.Count <= StableInsertionSortThreshold)
        {
            StableInsertionSortBy(list, keySelector);
            return;
        }

        StableMergeSortBy(list, keySelector);
    }

    private static void StableInsertionSortBy<T>(List<T> list, Func<T, int> keySelector)
    {
        for (var i = 1; i < list.Count; i++)
        {
            var item = list[i];
            var itemKey = keySelector(item);
            var j = i - 1;

            while (j >= 0 && keySelector(list[j]) > itemKey)
            {
                list[j + 1] = list[j];
                j--;
            }

            list[j + 1] = item;
        }
    }

    private static void StableMergeSortBy<T>(List<T> list, Func<T, int> keySelector)
    {
        var count = list.Count;
        var items = new T[count];
        var keys = new int[count];

        for (var i = 0; i < count; i++)
        {
            var item = list[i];
            items[i] = item;
            keys[i] = keySelector(item);
        }

        var itemBuffer = new T[count];
        var keyBuffer = new int[count];

        var sourceItems = items;
        var sourceKeys = keys;
        var destinationItems = itemBuffer;
        var destinationKeys = keyBuffer;

        for (var width = 1; width < count; width <<= 1)
        {
            for (var left = 0; left < count; left += width << 1)
            {
                var middle = left + width;
                if (middle > count)
                    middle = count;

                var right = left + (width << 1);
                if (right > count)
                    right = count;

                MergeRuns(
                    sourceItems,
                    sourceKeys,
                    destinationItems,
                    destinationKeys,
                    left,
                    middle,
                    right);
            }

            var swapItems = sourceItems;
            sourceItems = destinationItems;
            destinationItems = swapItems;

            var swapKeys = sourceKeys;
            sourceKeys = destinationKeys;
            destinationKeys = swapKeys;
        }

        for (var i = 0; i < count; i++)
            list[i] = sourceItems[i];
    }

    private static void MergeRuns<T>(
        T[] sourceItems,
        int[] sourceKeys,
        T[] destinationItems,
        int[] destinationKeys,
        int left,
        int middle,
        int right)
    {
        if (middle >= right || sourceKeys[middle - 1] <= sourceKeys[middle])
        {
            CopyRange(sourceItems, sourceKeys, destinationItems, destinationKeys, left, right);
            return;
        }

        var i = left;
        var j = middle;
        var k = left;

        while (i < middle && j < right)
        {
            if (sourceKeys[i] <= sourceKeys[j])
            {
                destinationItems[k] = sourceItems[i];
                destinationKeys[k] = sourceKeys[i];
                i++;
            }
            else
            {
                destinationItems[k] = sourceItems[j];
                destinationKeys[k] = sourceKeys[j];
                j++;
            }

            k++;
        }

        while (i < middle)
        {
            destinationItems[k] = sourceItems[i];
            destinationKeys[k] = sourceKeys[i];
            i++;
            k++;
        }

        while (j < right)
        {
            destinationItems[k] = sourceItems[j];
            destinationKeys[k] = sourceKeys[j];
            j++;
            k++;
        }
    }

    private static void CopyRange<T>(
        T[] sourceItems,
        int[] sourceKeys,
        T[] destinationItems,
        int[] destinationKeys,
        int start,
        int end)
    {
        for (var i = start; i < end; i++)
        {
            destinationItems[i] = sourceItems[i];
            destinationKeys[i] = sourceKeys[i];
        }
    }
}
