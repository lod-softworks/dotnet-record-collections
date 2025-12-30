# Record Collections

![Build](https://github.com/lod-softworks/dotnet-record-collections/actions/workflows/build-and-test.yml/badge.svg) 
![NuGet Version](https://img.shields.io/nuget/v/Lod.RecordCollections?logo=nuget&label=NuGet%20Release&link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FLod.RecordCollections) ![NuGet Downloads](https://img.shields.io/nuget/dt/Lod.RecordCollections?logo=nuget&label=Downloads&link=https%3A%2F%2Fwww.nuget.org%2Fstats%2Fpackages%2FLod.RecordCollections%3Fgroupby%3DVersion)

Generic collections that implement value-based equality for use with C# records. Drop-in replacements for standard collections that work correctly with record equality comparison.

## Available Collections

All collection types reside in the `System.Collections.Generic` namespace:

  - **RecordList\<T\>** - inherits from `List<T>`
  - **RecordDictionary\<TKey, TValue\>** - inherits from `Dictionary<TKey, TValue>`
  - **RecordSet\<T\>** - inherits from `HashSet<T>`
  - **RecordStack\<T\>** - inherits from `Stack<T>`
  - **RecordQueue\<T\>** - inherits from `Queue<T>`

## Quick Example

```csharp
record NumberInfo(IList<int> Numbers);

// Standard collections fail equality checks
var nums1 = new NumberInfo() { Numbers = new List<int>() { 1, 2, 3 } };
var nums2 = new NumberInfo() { Numbers = new List<int>() { 1, 2, 3 } };
Console.WriteLine(nums1 == nums2); // False - uses instance hash code

// Record collections pass equality checks
var nums3 = new NumberInfo() { Numbers = new RecordList<int>() { 1, 2, 3 } };
var nums4 = new NumberInfo() { Numbers = new RecordList<int>() { 1, 2, 3 } };
Console.WriteLine(nums3 == nums4); // True - uses value-based hash code
```

## Why Record Collections?

C# [records](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record) provide value-based equality comparison for reference types by examining the hash code of each property. This works great for structural data types, but collections in .NET are reference types that use instance-based hash codes. When you include a standard collection in a record, equality comparisons fail because the collection's hash code is based on the instance, not its contents.

Record Collections solve this by providing value-based equality for collections, allowing records containing collections to correctly compare based on their contents rather than instance identity. The collections behave identically to their parent classes (e.g., updating a `RecordList<T>` while enumerating still throws `InvalidOperationException`).

## Drop-in Replacements

Each Record Collection inherits directly from its corresponding standard collection type, making them true drop-in replacements. You can use them anywhere you would use the standard collection:

```csharp
// Works with existing interfaces and base types
IList<int> standardList = new RecordList<int>();
List<int> list = new RecordList<int>();
ICollection<int> collection = new RecordSet<int>();

// All standard methods and properties work identically
var recordList = new RecordList<int> { 1, 2, 3 };
recordList.Add(4);
recordList.Remove(2);
var count = recordList.Count;
var first = recordList[0];
```

This allows you to utilized Record Collections without changing existing data models, interface contracts, or other other cascading code while also allowing you to do so without impact as the methods, properties, and behaviors of the collections are identical to their base types.

## Cloning

Record Collections are implemented as `record class` types, which means they support cloning via the `with` expression. When a collection is cloned, it attempts to clone each underlying element to create an entirely new collection:

- If an element is a record or value type, it will be cloned
- If an element does not support cloning (e.g., a regular class), the original element reference is retained
- This ensures that collections of records create deep copies, while collections of regular classes maintain reference semantics

```csharp
var original = new RecordList<Person> 
{ 
    new Person("Alice"), 
    new Person("Bob") 
};

var cloned = original with { }; // Creates new collection with cloned elements

// If Person is a record, cloned contains new Person instances
// If Person is a regular class, cloned retains the original Person instances
```

Customize collection and element cloning by overriding the cloner delegate:

```csharp
// .NET 6.0 or greater
IReadOnlyRecordCollection.ElementCloner = obj => MyCustomCloner.Clone(obj, deep: true);

// .NET Framework 4.8 or .NET Standard 2.0
RecordCollectionCloner.ElementCloner = obj => MyCustomCloner.Clone(obj, deep: true);
```

## LINQ Extension Methods

The `RecordEnumerable` class (in the `System.Linq` namespace) provides extension methods similar to `Enumerable`, but for creating Record Collections from any `IEnumerable<T>`. These methods mirror the standard LINQ `ToList()`, `ToDictionary()`, etc., but return Record Collections instead:

- **ToRecordList** - Creates a new `RecordList<T>` from an `IEnumerable<T>`
- **ToRecordList** - Creates a new `RecordDictionary<TKey, TValue>` from an `IEnumerable<T> `and key/value delegate selectors.
- **ToRecordSet** - Creates a new `RecordSet<T>` from an `IEnumerable<T>`
- **ToRecordStack** - Creates a new `RecordStack<T>` from an `IEnumerable<T>`
- **ToRecordQueue** - Creates a new `RecordQueue<T>` from an `IEnumerable<T>`

```csharp
var numbers = Enumerable.Range(1, 10).Select(i => new Number(i));

// Convert to Record Collections
var recordList = numbers.ToRecordList();
var recordSet = numbers.ToRecordSet();
var recordStack = numbers.ToRecordStack();
var recordQueue = numbers.ToRecordQueue();

// Create RecordDictionary with key selector
var recordDict = numbers.ToRecordDictionary(n => n.Value);

// Create RecordDictionary with key and element selectors
var recordDict2 = numbers.ToRecordDictionary(
    keySelector: n => n.Value,
    elementSelector: n => n with { Value = n.Value * 2 }
);
```

All methods require elements to implement `IEquatable<T>` (value or record-like types) and follow the same null-handling and exception behavior as their standard LINQ counterparts.

## Custom Comparers

You can customize how record collections compare equality by using a custom comparer. There are two ways to specify a comparer:

### Overriding the Default Comparer

You can set a default comparer that will be used by all record collections created without explicitly specifying a comparer:

```csharp
// .NET 6.0 or greater
IReadOnlyRecordCollection.DefaultComparer = new MyCustomComparer();

// .NET Framework 4.8 or .NET Standard 2.0
RecordCollectionComparer.Default = new MyCustomComparer();
```

### Instance-Based Constructors

All record collection types support constructors that accept an `IRecordCollectionComparer` parameter, allowing you to specify a comparer for individual instances:

```csharp
var customComparer = new MyCustomComparer();

// Empty collection with custom comparer
var list = new RecordList<int>(customComparer);

// Collection from existing data with custom comparer
var stack = new RecordStack<int>([1, 2, 3], customComparer);

// Collection with capacity and custom comparer
var queue = new RecordQueue<int>(capacity: 100, customComparer);
```

When a comparer is not provided, collections will use the default comparer (either the global default or `RecordCollectionComparer.Default`).

## ⚠ Drawbacks & Warnings

### Performance Considerations

**Hash Code Computation:**
The library computes hash codes by iterating through all elements in the collection. For ordered collections (lists, queues, stacks), each element's hash is combined with its position index. For unordered collections (sets, dictionaries), element hashes are combined using sum and xor operations. This results in O(n) time complexity where n is the number of elements.

**Equality Comparison:**
Equality comparisons first check the collection count (O(1) fast path). If counts match, the implementation performs element-by-element comparison:
- **Ordered collections** (lists, queues, stacks): Sequential comparison where order matters
- **Sets**: Uses `SetEquals` which compares all elements regardless of order
- **Dictionaries**: Iterates through all key-value pairs, checking each entry

Both hash code computation and equality comparison are O(n) operations. While the count check provides a fast path for unequal-sized collections, comparing large collections with identical sizes requires examining every element.

**Recommendations:**
The library is intended to provide a default but extendable implementation of common collections. It works great for data models with small to medium-sized collections (typically hundreds to low thousands of elements). When used with very large collections (hundreds of thousands or millions of elements), performance will deteriorate, especially when:
- Computing hash codes frequently (e.g., in hash-based collections or dictionaries)
- Comparing collections that are equal or differ only in the last element
- Using collections as keys in dictionaries or as elements in hash sets

For large-scale scenarios, consider using custom comparers that implement caching or other optimization strategies.

**Note:** See the [Cloning](#cloning) section for details on how collection cloning works.
