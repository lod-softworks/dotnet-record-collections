
namespace System.Collections;

partial class RecordCollectionComparer
{
    private sealed class DefaultStrategy : IComparisonStrategy
    {
        public bool Equals(IReadOnlyRecordCollection x, object y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (y is null) return false;

            if (x is IList listX && y is IList listY)
            {
                if (listX.Count != listY.Count) return false;
                for (int i = 0; i < listX.Count; i++)
                {
                    if (!Equals(listX[i], listY[i])) return false;
                }
                return true;
            }

            if (x is IDictionary dictX && y is IDictionary dictY)
            {
                if (dictX.Count != dictY.Count) return false;

                foreach (DictionaryEntry entry in dictX)
                {
                    if (!dictY.Contains(entry.Key)) return false;
                    if (!Equals(dictY[entry.Key], entry.Value)) return false;
                }

                return true;
            }

            if (x is IEnumerable seqX && y is IEnumerable seqY)
            {
                IEnumerator e1 = seqX.GetEnumerator();
                IEnumerator e2 = seqY.GetEnumerator();
                try
                {
                    while (true)
                    {
                        bool m1 = e1.MoveNext();
                        bool m2 = e2.MoveNext();
                        if (m1 != m2) return false;
                        if (!m1) return true;
                        if (!Equals(e1.Current, e2.Current)) return false;
                    }
                }
                finally
                {
                    (e1 as IDisposable)?.Dispose();
                    (e2 as IDisposable)?.Dispose();
                }
            }

            return false;
        }

        public int GetHashCode(IReadOnlyRecordCollection x, int startingHash)
        {
            if (x is IList list)
            {
                unchecked
                {
                    int hash = startingHash;
                    hash = Combine(hash, list.Count);

                    for (int i = 0; i < list.Count; i++)
                    {
                        int itemHash = list[i]?.GetHashCode() ?? default;
                        hash = Combine(hash, Mix(itemHash ^ i));
                    }

                    return hash;
                }
            }

            if (x is IDictionary dictionary)
            {
                unchecked
                {
                    int sum = 0;
                    int xor = 0;

                    foreach (DictionaryEntry entry in dictionary)
                    {
                        int keyHash = entry.Key?.GetHashCode() ?? default;
                        int valueHash = entry.Value?.GetHashCode() ?? default;
                        int entryHash = Combine(Mix(keyHash), Mix(valueHash));

                        sum += entryHash;
                        xor ^= entryHash;
                    }

                    int hash = startingHash;
                    hash = Combine(hash, dictionary.Count);
                    hash = Combine(hash, Mix(sum));
                    hash = Combine(hash, Mix(xor));
                    return hash;
                }
            }

            if (x is IEnumerable enumerable)
            {
                unchecked
                {
                    int hash = startingHash;
                    int i = 0;

                    foreach (object? item in enumerable)
                    {
                        int itemHash = item?.GetHashCode() ?? default;
                        hash = Combine(hash, Mix(itemHash ^ i));
                        i++;
                    }

                    hash = Combine(hash, i);
                    return hash;
                }
            }

            return startingHash;
        }
    }
}
