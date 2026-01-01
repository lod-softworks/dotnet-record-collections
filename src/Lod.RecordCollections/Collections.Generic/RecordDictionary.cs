namespace System.Collections.Generic;

/// <summary>
/// Represents a strongly typed dictionary of key-value pairs that can be accessed by keys.
/// Record dictionaries support value based comparison of dictionary data.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
public partial class RecordDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>
    where TKey : notnull
    where TValue : IEquatable<TValue>
{
    /// <summary>
    /// Gets the comparer used to compare elements and collections.
    /// </summary>
    public virtual new IRecordCollectionComparer Comparer { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordDictionary{TKey, TValue}"/> class that is empty and has the default initial capacity.
    /// </summary>
    public RecordDictionary()
        : this(comparer: null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordDictionary{TKey, TValue}"/> class that is empty,
    /// has the default initial capacity, and uses the specified comparer.
    /// </summary>
    /// <param name="comparer">The comparer used for record equality.</param>
    public RecordDictionary(IRecordCollectionComparer? comparer)
        : base()
    {
        Comparer = comparer ?? RecordCollectionComparer.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordDictionary{TKey, TValue}"/> class that uses the specified underlying dictionary.
    /// </summary>
    /// <param name="dictionary">An existing <see cref="RecordDictionary{TKey, TValue}"/> to use as the underlying collection.</param>
    public RecordDictionary(Dictionary<TKey, TValue> dictionary)
        : this(dictionary, comparer: null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordDictionary{TKey, TValue}"/> class that uses the specified underlying dictionary
    /// and comparer.
    /// </summary>
    /// <param name="dictionary">An existing <see cref="RecordDictionary{TKey, TValue}"/> to use as the underlying collection.</param>
    /// <param name="comparer">The comparer used for record equality.</param>
    public RecordDictionary(Dictionary<TKey, TValue> dictionary, IRecordCollectionComparer? comparer)
        : base(dictionary)
    {
        Comparer = comparer ?? RecordCollectionComparer.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordDictionary{TKey, TValue}"/> class that
    /// contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new dictionary.</param>
    public RecordDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        : this(collection, comparer: null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordDictionary{TKey, TValue}"/> class that
    /// contains elements copied from the specified collection and uses the specified comparer.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new dictionary.</param>
    /// <param name="comparer">The comparer used for record equality.</param>
    public RecordDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IRecordCollectionComparer? comparer)
#if NET6_0_OR_GREATER
        : base(collection)
#else
        : base(collection.ToDictionary(kv => kv.Key, kv => kv.Value))
#endif
    {
        Comparer = comparer ?? RecordCollectionComparer.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordDictionary{TKey, TValue}"/> class that is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new dictionary can initially store.</param>
    public RecordDictionary(int capacity)
        : this(capacity, comparer: null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordDictionary{TKey, TValue}"/> class that is empty, has the specified initial capacity,
    /// and uses the specified comparer.
    /// </summary>
    /// <param name="capacity">The number of elements that the new dictionary can initially store.</param>
    /// <param name="comparer">The comparer used for record equality.</param>
    public RecordDictionary(int capacity, IRecordCollectionComparer? comparer)
        : base(capacity)
    {
        Comparer = comparer ?? RecordCollectionComparer.Default;
    }
}
