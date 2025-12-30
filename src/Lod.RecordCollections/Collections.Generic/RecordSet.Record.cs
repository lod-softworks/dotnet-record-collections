using System.Runtime.CompilerServices;
using System.Text;

namespace System.Collections.Generic;

partial class RecordSet<T>
{
    private static IEnumerable<T> CloneEnumerable(IEnumerable<T> source) =>
        source.Select(original => RecordCollectionCloner.ElementCloner(original) is T value ? value : original);

    /// <summary>
    /// Gets the record equality contract for this collection.
    /// </summary>
    // [RecordImp!]: This needs to be protected, virtual, returning it's own type to meet the `record` spec.
    protected virtual Type EqualityContract => typeof(RecordSet<T>);

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordSet{T}"/> class that uses records from an existing collection.
    /// </summary>
    /// <param name="original">An existing <see cref="RecordSet{T}"/> to clone into the new record.</param>
    // [RecordImp!]: This needs to be protected, non-null with no null checks to meet the `record` spec.
    protected RecordSet(RecordSet<T> original)
        : base(CloneEnumerable(original))
    {
        Comparer = original?.Comparer ?? RecordCollectionComparer.Default;
    }

    /// <inheritdoc/>
    // [RecordImp!]: This needs to be overriden to meet the `record` spec.
    public override int GetHashCode() =>
        Comparer.GetHashCode(this);

    /// <inheritdoc/>
    // [RecordImp!]: This needs to be overriden to meet the `record` spec.
    public override bool Equals(object? obj) =>
        Comparer.Equals(this, obj);

    /// <summary>
    /// Returns a value indicating whether the collection is equal to another <see cref="HashSet{T}"/>.
    /// </summary>
    /// <param name="other"/>
    /// <returns/>
    // [RecordImp!]: This needs to be public, non-virtual to meet the `record` spec.
    public bool Equals(HashSet<T> other) =>
        Comparer.Equals(this, other);

    /// <summary>
    /// Returns a value indicating whether the collection is equal to another <see cref="RecordSet{T}"/>.
    /// </summary>
    /// <param name="other"/>
    /// <returns/>
    // [RecordImp!]: This needs to be public, virtual to meet the `record` spec.
    public virtual bool Equals(RecordSet<T>? other) =>
        Comparer.Equals(this, other);

    /// <summary>
    /// Appends the specified <paramref name="builder"/> with value information for the collection.
    /// </summary>
    /// <param name="builder"></param>
    // [RecordImp!]: This needs to be protected, virtual to meet the `record` spec.
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        builder.Append($"Count = {Count}");
        return true;
    }

    /// <summary>
    /// Returns a value indicating whether two <see cref="RecordSet{T}"/> represent the same collection of records.
    /// </summary>
    // [RecordImp!]: This operator is required to meet the `record` spec.
    public static bool operator ==(RecordSet<T> left, RecordSet<T> right) =>
        left?.Equals(right) ?? right?.Equals(left) ?? false;

    /// <summary>
    /// Returns a value indicating whether two <see cref="RecordSet{T}"/> represent a different collection of records.
    /// </summary>
    // [RecordImp!]: This operator is required to meet the `record` spec.
    public static bool operator !=(RecordSet<T> left, RecordSet<T> right) =>
        !(left?.Equals(right) ?? right?.Equals(left) ?? false);
}
