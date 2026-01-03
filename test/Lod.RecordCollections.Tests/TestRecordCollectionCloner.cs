using System.Collections;
using System.Reflection;

namespace Lod.RecordCollections.Tests;

internal sealed class TestRecordCollectionCloner
{
    public int CloneCallCount { get; private set; }
    public List<object> ClonedObjects { get; } = [];

    public object? CloneElement(object? obj)
    {
        CloneCallCount++;
        if (obj != null)
        {
            ClonedObjects.Add(obj);
        }
        return RecordCollectionCloner.TryCloneElement(obj);
    }

    public void Reset()
    {
        CloneCallCount = 0;
        ClonedObjects.Clear();
    }

    /// <summary>
    /// Invokes the protected constructor that handles cloning for a record collection.
    /// </summary>
    /// <remarks>This is used in place of the standard &quote;with&quote; expression as the IL injection has not happened on the collection yet.</remarks>
    public static TCollection Clone<TCollection>(TCollection original) where TCollection : notnull
    {
        Type type = typeof(TCollection);
        ConstructorInfo? constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(c =>
            {
                ParameterInfo[] parameters = c.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == type;
            });

        return constructor != null
            ? (TCollection)constructor.Invoke([original])
            : throw new InvalidOperationException($"Could not find protected cloning constructor for {type.Name}");
    }
}

