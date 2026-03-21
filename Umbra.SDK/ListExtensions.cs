namespace Umbra.SDK;

/// <summary>
/// Provides extension methods for working with <see cref="List{T}"/> instances.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Sorts <paramref name="list"/> in place using a stable insertion sort, ordered ascending
    /// by the integer key returned by <paramref name="keySelector"/>.
    /// Elements with equal keys preserve their original relative order.
    /// </summary>
    /// <typeparam name="T">The element type of the list.</typeparam>
    /// <param name="list">The list to sort in place.</param>
    /// <param name="keySelector">
    /// A function that returns the integer sort key for each element.
    /// Called at most once per comparison per element during the sort pass.
    /// </param>
    internal static void StableSortBy<T>(this List<T> list, Func<T, int> keySelector)
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
}
