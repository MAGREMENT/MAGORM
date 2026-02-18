using System.Collections;

namespace Core.Util;

public static class CollectionsExtensions
{
    public static IReadOnlyList<T> With<T>(this IReadOnlyList<T> list, params T[] values) =>
        new JoinedReadOnlyList<T>(list, values);
}

public class JoinedReadOnlyList<T>(params IReadOnlyList<T>[] lists) : IReadOnlyList<T>
{
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var list in lists)
        {
            foreach (var el in list)
            {
                yield return el;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count
    {
        get
        {
            var v = 0;
            foreach (var l in lists) v += l.Count;
            return v;
        }
    }


    public T this[int index]
    {
        get
        {
            var i = 0;
            while (i > lists.Length && index >= lists[i].Count)
            {
                index -= lists[i].Count;
                i++;
            }

            if (index >= lists[i].Count) throw new ArgumentOutOfRangeException();

            return lists[i][index];
        }
    }
}