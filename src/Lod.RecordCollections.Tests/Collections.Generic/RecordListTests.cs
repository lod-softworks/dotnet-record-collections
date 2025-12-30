using System.Diagnostics;

namespace Lod.RecordCollections.Tests.Collections.Generic;

[TestClass]
public class RecordListTests
{
    public TestContext TestContext { get; set; } = null!;
    protected Random Random { get; } = new();

    [TestInitialize]
    public void SetUp()
    {
        RecordCollectionComparer.Default = new RecordCollectionComparer();
        RecordCollectionCloner.ElementCloner = RecordCollectionCloner.TryCloneElement;
    }

    private static int GetSizeOrDefault(int @default)
    {
        string? raw = Environment.GetEnvironmentVariable("RECORDCOLLECTION_STRESS_SIZE");
        return int.TryParse(raw, out int size) && size > 0 ? size : @default;
    }

    /// <remarks>This test is a sanity check to ensure that the default List.Equals behavior is as expected.</remarks>
    [TestMethod]
    public void List_SameInts_NotEqualsMatchingList()
    {
        // Arrange
        List<int> list1 = [92, 117, 420,];
        List<int> list2 = [92, 117, 420,];

        // Act
        bool areEqual = list1.Equals(list2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    [RepeatTestMethod(3)]
    [DoNotParallelize]
    public void RecordList_DefaultConstructor_UsesDefaultComparer()
    {
        // Arrange
        TestRecordCollectionComparer comparer = new();
        RecordCollectionComparer.Default = comparer;

        // Act
        RecordList<int> list = [];

        // Assert
        Assert.AreSame(comparer, list.Comparer);
    }

    [TestMethod]
    public void RecordList_CustomComparerConstructor_UsesProvidedComparer()
    {
        // Arrange
        TestRecordCollectionComparer comparer = new();

        // Act
        RecordList<int> list = new(comparer);

        // Assert
        Assert.AreSame(comparer, list.Comparer);
    }

    [TestMethod]
    public void RecordList_Operators_UseTypedEquals()
    {
        OperatorAwareRecordList left = new([92, 117]);
        OperatorAwareRecordList right = new([92, 117]);

        left.Reset();
        _ = left == right;
        Assert.IsTrue(left.TypedEqualsCalled);
        Assert.IsFalse(left.ObjectEqualsCalled);

        left.Reset();
        _ = left != right;
        Assert.IsTrue(left.TypedEqualsCalled);
        Assert.IsFalse(left.ObjectEqualsCalled);
    }

    [TestMethod]
    public void RecordList_SameInts_EqualsMatchingList()
    {
        // Arrange
        RecordList<int> list1 = [92, 117, 420,];
        RecordList<int> list2 = [92, 117, 420,];

        // Act
        bool areEqual = list1.Equals(list2);

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void RecordList_Equals_Matching_SystemList()
    {
        // Arrange
        RecordList<int> record = [1, 2, 3];
        List<int> system = [1, 2, 3];

        // Act & Assert
        Assert.IsTrue(record.Equals(system));
    }

    [TestMethod]
    public void RecordList_SameStrings_EqualsMatchingList()
    {
        // Arrange
        RecordList<string> list1 = ["92", "117", "420",];
        RecordList<string> list2 = ["92", "117", "420",];

        // Act
        bool areEqual = list1.Equals(list2);

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void RecordList_SameRecords_EqualsMatchingList()
    {
        // Arrange
        RecordList<Number> list1 = [new Number(92), new Number(117), new Number(420),];
        RecordList<Number> list2 = [new Number(92), new Number(117), new Number(420),];

        // Act
        bool areEqual = list1.Equals(list2);

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void RecordList_SameInts_DifferentOrder_NotEqualsSimilarList()
    {
        // Arrange
        RecordList<int> list1 = [92, 117, 420,];
        RecordList<int> list2 = [117, 420, 92,];

        // Act
        bool areEqual = list1.Equals(list2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordList_DifferentValues_NotEquals()
    {
        // Arrange
        RecordList<int> list1 = [92, 117, 420];
        RecordList<int> list2 = [92, 117, 421];

        // Act
        bool areEqual = list1.Equals(list2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordList_DifferentSizes_NotEquals()
    {
        // Arrange
        RecordList<int> list1 = [92, 117, 420];
        RecordList<int> list2 = [92, 117];

        // Act
        bool areEqual = list1.Equals(list2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordList_EmptyVsNonEmpty_NotEquals()
    {
        // Arrange
        RecordList<int> list1 = [];
        RecordList<int> list2 = [92, 117, 420];

        // Act
        bool areEqual = list1.Equals(list2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordList_OneValueDifferent_NotEquals()
    {
        // Arrange
        RecordList<int> list1 = [1, 2, 3, 4, 5];
        RecordList<int> list2 = [1, 2, 99, 4, 5];

        // Act
        bool areEqual = list1.Equals(list2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordList_Clone_ClonesRecordElements()
    {
        // Arrange
        Number num1 = new(92);
        Number num2 = new(117);
        Number num3 = new(420);
        RecordList<Number> original = [num1, num2, num3];

        // Act
        RecordList<Number> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreNotSame(original, cloned);
        Assert.HasCount(original.Count, cloned);
        for (int i = 0; i < original.Count; i++)
        {
            Assert.AreNotSame(original[i], cloned[i], $"Element at index {i} should be cloned");
            Assert.AreEqual(original[i].Value, cloned[i].Value, $"Element at index {i} should have same value");
        }
    }

    [TestMethod]
    public void RecordList_Clone_NonRecordElements_NotCloned()
    {
        // Arrange
        string obj1 = Random.Next().ToString();
        string obj2 = Random.Next().ToString();
        string obj3 = Random.Next().ToString();
        RecordList<string> original = [obj1, obj2, obj3];

        // Act
        RecordList<string> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreNotSame(original, cloned);
        Assert.HasCount(original.Count, cloned);
        for (int i = 0; i < original.Count; i++)
        {
            Assert.AreSame(original[i], cloned[i], $"Non-record element at index {i} should not be cloned");
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public void RecordList_Clone_UsesRecordCollectionCloner()
    {
        // Arrange
        TestRecordCollectionCloner testCloner = new();
        RecordCollectionCloner.ElementCloner = testCloner.CloneElement;
        Number num1 = new(92);
        Number num2 = new(117);
        RecordList<Number> original = [num1, num2];

        // Act
        RecordList<Number> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreEqual(2, testCloner.CloneCallCount, "ElementCloner should be called for each element");
        Assert.Contains(num1, testCloner.ClonedObjects);
        Assert.Contains(num2, testCloner.ClonedObjects);
    }

    [TestMethod]
    [DoNotParallelize]
    public void RecordList_Clone_CustomElementCloner_Used()
    {
        // Arrange
        Number num1 = new(92);
        Number num2 = new(117);
        RecordList<Number> original = [num1, num2];
        bool customClonerCalled = false;
        RecordCollectionCloner.ElementCloner = obj =>
        {
            customClonerCalled = true;
            return obj;
        };

        // Act
        RecordList<Number> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.IsTrue(customClonerCalled, "Custom ElementCloner should be called");
    }

    [TestMethod]
    public void RecordList_Clone_EmptyCollection_Clones()
    {
        // Arrange
        RecordList<int> original = [];

        // Act
        RecordList<int> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreNotSame(original, cloned);
        Assert.IsEmpty(cloned);
    }

    [TestMethod]
    public void RecordList_DeserializedNewtonsoft_EqualsReserialized()
    {
        // Arrange
        RecordList<Number> list = [new Number(92), new Number(117), new Number(420),];

        // Act
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
        RecordList<Number>? recordList = Newtonsoft.Json.JsonConvert.DeserializeObject<RecordList<Number>>(json);
        Number[]? systemList = Newtonsoft.Json.JsonConvert.DeserializeObject<Number[]>(json);

        // Assert
        Assert.IsNotNull(recordList, "Deserialized record list is null.");
        Assert.IsNotNull(systemList, "Deserialized list is null.");
        Assert.IsTrue(list.Equals(recordList), "Deserialized list is not equal to the original list.");
        for (int i = 0; i < list.Count; i++)
        {
            Assert.IsTrue(list[i] == recordList[i], "Deserialized list is not a subset of the original list.");
        }
    }

#if !NETFRAMEWORK
    [TestMethod]
    public void RecordList_DeserializedSystemTextJson_EqualsReserialized()
    {
        // Arrange
        RecordList<Number> list = [new Number(92), new Number(117), new Number(420),];

        // Act
        string json = System.Text.Json.JsonSerializer.Serialize(list);
        RecordList<Number>? recordList = System.Text.Json.JsonSerializer.Deserialize<RecordList<Number>>(json);
        Number[]? systemList = Newtonsoft.Json.JsonConvert.DeserializeObject<Number[]>(json);

        // Assert
        Assert.IsNotNull(recordList, "Deserialized record list is null.");
        Assert.IsNotNull(systemList, "Deserialized list is null.");
        Assert.IsTrue(list.Equals(recordList), "Deserialized list is not equal to the original list.");
        for (int i = 0; i < list.Count; i++)
        {
            Assert.IsTrue(list[i] == recordList[i], "Deserialized list is not a subset of the original list.");
        }
    }
#endif

    [TestMethod]
    public void Stress_RecordList_Int32_Compare_And_Time()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);

        RecordList<int> left = new(n);
        RecordList<int> right = new(n);

        for (int i = 0; i < n; i++)
        {
            left.Add(i);
            right.Add(i);
        }

        Stopwatch sw = Stopwatch.StartNew();
        Assert.IsTrue(left.Equals(right));
        sw.Stop();
        TestContext.WriteLine($"RecordList<int>.Equals (n={n:n0}) = {sw.ElapsedMilliseconds:n0} ms");

        sw.Restart();
        _ = left.GetHashCode();
        _ = right.GetHashCode();
        sw.Stop();
        TestContext.WriteLine($"RecordList<int>.GetHashCode x2 (n={n:n0}) = {sw.ElapsedMilliseconds:n0} ms");
    }

    [TestMethod]
    [RepeatTestMethod(10)]
    public void Stress_RecordList_Int32_Random_Compare_And_Time()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);
        Random random = new();

        RecordList<int> left = new(n);
        RecordList<int> right = new(n);

        for (int i = 0; i < n; i++)
        {
            int value = random.Next();
            left.Add(value);
            right.Add(value);
        }

        Stopwatch sw = Stopwatch.StartNew();
        Assert.IsTrue(left.Equals(right));
        sw.Stop();
        TestContext.WriteLine($"RecordList<int>.Equals with random values (n={n:n0}) = {sw.ElapsedMilliseconds:n0} ms");
    }

    [TestMethod]
    [RepeatTestMethod(10)]
    public void Stress_RecordList_Int32_Random_OneDifference_NotEquals()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);
        Random random = new();

        RecordList<int> left = new(n);
        RecordList<int> right = new(n);

        // Add same random values to both collections
        for (int i = 0; i < n; i++)
        {
            int value = random.Next();
            left.Add(value);
            right.Add(value);
        }

        // Introduce one difference at a random position
        int differenceIndex = random.Next(0, n);
        int originalValue = left[differenceIndex];
        int differentValue = random.Next();
        // Ensure the different value is actually different
        while (differentValue == originalValue)
        {
            differentValue = random.Next();
        }
        right[differenceIndex] = differentValue;

        // Act
        bool areEqual = left.Equals(right);

        // Assert
        Assert.IsFalse(areEqual, $"Collections should not be equal with one difference at index {differenceIndex}");
    }

    [TestMethod]
    [RepeatTestMethod(10)]
    public void Stress_RecordList_String_Random_OneDifference_NotEquals()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);
        Random random = new();

        RecordList<string> left = new(n);
        RecordList<string> right = new(n);

        // Add same random values to both collections
        for (int i = 0; i < n; i++)
        {
            string value = random.Next().ToString();
            left.Add(value);
            right.Add(value);
        }

        // Introduce one difference at a random position
        int differenceIndex = random.Next(0, n);
        string originalValue = left[differenceIndex];
        string differentValue = random.Next().ToString();
        // Ensure the different value is actually different
        while (differentValue == originalValue)
        {
            differentValue = random.Next().ToString();
        }
        right[differenceIndex] = differentValue;

        // Act
        bool areEqual = left.Equals(right);

        // Assert
        Assert.IsFalse(areEqual, $"Collections should not be equal with one difference at index {differenceIndex}");
    }

    #region Support Types

    private sealed class OperatorAwareRecordList(IEnumerable<int> values) : RecordList<int>(values)
    {
        public bool TypedEqualsCalled { get; private set; }
        public bool ObjectEqualsCalled { get; private set; }

        public void Reset()
        {
            TypedEqualsCalled = false;
            ObjectEqualsCalled = false;
        }

        public override bool Equals(object? obj)
        {
            ObjectEqualsCalled = true;
            return base.Equals(obj);
        }

        public override bool Equals(RecordList<int>? other)
        {
            TypedEqualsCalled = true;
            return base.Equals(other);
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    #endregion
}
