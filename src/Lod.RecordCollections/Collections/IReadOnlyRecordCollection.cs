namespace System.Collections;

/// <summary>
/// A read-only collection of strongly typed, record-like values.
/// Record collections support value based comparison.
/// </summary>
public interface IReadOnlyRecordCollection
    : IEnumerable
    //, IComparable
    , IStructuralEquatable
    , IStructuralComparable
{
#if NET6_0_OR_GREATER
    /// <summary>
    /// Gets or sets the default comparer for record collections.
    /// </summary>
    /// <remarks>This comparer is used for record collections initialized without specifying a <see cref="IRecordCollectionComparer"/> in their constructor.</remarks>
    public static IRecordCollectionComparer DefaultComparer { get; set; } = new RecordCollectionComparer();

    /// <summary>
    /// Represents a method that creates a clone of a specified element in a record collection.
    /// </summary>
    public static Func<object, object?> ElementCloner { get; set; } = RecordCollectionCloner.TryCloneElement;
#endif

    /// <summary>
    /// Gets the comparer used to determine equality between this collection and other collections.
    /// </summary>
    public IRecordCollectionComparer Comparer { get; }

    /// <summary>
    /// Gets the current number of elements in the collection.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Returns a value indicating whether an <paramref name="other"/> collection is equal to the current instance.
    /// </summary>
    /// <param name="other">The collection to compare the current collection to.</param>
    /// <return>True if the underlying collection's elements are equivalent to the current collection.</return>
    public bool Equals(IReadOnlyRecordCollection? other)
#if NET6_0_OR_GREATER
        => Comparer.Equals(this, other)
#endif
        ;

    /// <summary>
    /// Returns a value indicating whether the <paramref name="left"/> collection is equal to the <paramref name="right"/> collection.
    /// </summary>
    /// <param name="left">The original collection to compare the other collection to.</param>
    /// <param name="right">The collection to compare the current collection to.</param>
    /// <return>True if the underlying collection's elements are equivalent to the current collection.</return>
    public bool Equals(IReadOnlyRecordCollection? left, IReadOnlyRecordCollection? right)
#if NET6_0_OR_GREATER
        => Comparer.Equals(left, right)
#endif
        ;

    /// <summary>
    /// Returns a hash of the underlying elements.
    /// </summary>
    /// <returns>The hash of the underlying elements.</returns>
    public int GetHashCode()
#if NET6_0_OR_GREATER
        => Comparer.GetHashCode(this)
#endif
        ;
}
