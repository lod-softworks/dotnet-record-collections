using System.Collections.Concurrent;
using System.Reflection;

namespace System.Collections;

/// <summary>
/// A static utility for cloning items within a record collection.
/// </summary>
#if !NET6_0_OR_GREATER
public
#endif
static class RecordCollectionCloner
{
    private static readonly ConcurrentDictionary<Type, MethodBase?> clonerCache = [];
#if !NET6_0_OR_GREATER
    private static Func<object, object?>? elementCloner;
#endif

    /// <summary>
    /// Represents a method that creates a clone of a specified element in a record collection.
    /// </summary>
    public static Func<object, object?> ElementCloner
#if NET6_0_OR_GREATER
    {
        get => IReadOnlyRecordCollection.ElementCloner;
        [Obsolete("Use IReadOnlyRecordCollection.ElementCloner instead.")]
        set => IReadOnlyRecordCollection.ElementCloner = value;
    }
#else
    { get => elementCloner ?? TryCloneElement; set => elementCloner = value; }
#endif

    /// <summary>
    /// Gets the default comparer for record collections.
    /// </summary>
    /// <remarks>
    /// This comparer is used for record collections initialized without specifying a <see cref="IRecordCollectionComparer"/> in their constructor.
    /// In .NET 6.0 or greater, this property proxies to the default comparer defined on <see cref="IReadOnlyRecordCollection"/>.
    /// </remarks>
    public static IRecordCollectionComparer Default
#if NET6_0_OR_GREATER
    {
        get => IReadOnlyRecordCollection.DefaultComparer;
        [Obsolete("Use IReadOnlyRecordCollection.DefaultComparer instead.")]
        set => IReadOnlyRecordCollection.DefaultComparer = value;
    }
#else
    { get; set; } = new RecordCollectionComparer();
#endif

    /// <summary>
    /// Returns a cloned instance of <typeparamref name="TElement"/> if it's a record type.
    /// If the type is not clonable the original instance is returned.
    /// </summary>
    public static TElement? TryCloneElement<TElement>(TElement? obj) =>
        (TElement?)TryCloneElement((object?)obj);

    /// <summary>
    /// Returns a cloned instance of an object if it's a record type.
    /// If the type is not clonable the original instance is returned.
    /// </summary>
    public static object? TryCloneElement(object? obj) =>
        obj != null ? TryCloneElement(obj.GetType(), obj) : null;

    private static object? TryCloneElement(Type type, object obj)
    {
        object? result = null;

        MethodBase? cloner = clonerCache.GetOrAdd(type, GetElementCloneConstructor);

        if (obj != null && cloner != null)
        {
            try
            {
                result = cloner is ConstructorInfo cons
                    ? cons.Invoke([obj,])
                    : cloner.Invoke(null, [obj,]);
            }
            catch { }
        }

        return result ?? obj;
    }

    static MethodBase? GetElementCloneConstructor(Type type) =>
        type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .OrderByDescending(c => c.IsFamily) // Prefer the protected record constructor
            .FirstOrDefault(c =>
            {
                ParameterInfo[] parameters = c.GetParameters();

                return parameters.Length == 1 && parameters[0].ParameterType == type;
            });
}
