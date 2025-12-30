using System.Diagnostics;

namespace Lod.RecordCollections.Tests.Collections.Generic;

[TestClass]
public class RecordDictionaryTests
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

    /// <remarks>This test is a sanity check to ensure that the default Dictionary.Equals behavior is as expected.</remarks>
    [TestMethod]
    public void Dictionary_SameInts_NotEqualsMatchingDictionary()
    {
        // Arrange
        Dictionary<int, string> dictionary1 = new() { { 92, "92" }, { 117, "117" }, { 420, "420" }, };
        Dictionary<int, string> dictionary2 = new() { { 92, "92" }, { 117, "117" }, { 420, "420" }, };

        // Act
        bool areEqual = dictionary1.Equals(dictionary2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    [RepeatTestMethod(3)]
    [DoNotParallelize]
    public void RecordDictionary_DefaultConstructor_UsesDefaultComparer()
    {
        // Arrange
        TestRecordCollectionComparer comparer = new();
        RecordCollectionComparer.Default = comparer;

        // Act
        RecordDictionary<int, string> dictionary = [];

        // Assert
        Assert.AreSame(comparer, dictionary.Comparer);
    }

    [TestMethod]
    public void RecordDictionary_CustomComparerConstructor_UsesProvidedComparer()
    {
        // Arrange
        TestRecordCollectionComparer comparer = new();

        // Act
        RecordDictionary<int, string> dictionary = new(comparer);

        // Assert
        Assert.AreSame(comparer, dictionary.Comparer);
    }

    [TestMethod]
    public void RecordDictionary_Operators_UseTypedEquals()
    {
        OperatorAwareRecordDictionary left = OperatorAwareRecordDictionary.Create();
        OperatorAwareRecordDictionary right = OperatorAwareRecordDictionary.Create();

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
    public void RecordDictionary_SameInts_EqualsMatchingDictionary()
    {
        // Arrange
        RecordDictionary<int, string> dictionary1 = new() { { 92, "92" }, { 117, "117" }, { 420, "420" }, };
        RecordDictionary<int, string> dictionary2 = new() { { 92, "92" }, { 117, "117" }, { 420, "420" }, };

        // Act
        bool areEqual = dictionary1.Equals(dictionary2);

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void RecordDictionary_Equals_Matching_SystemDictionary()
    {
        // Arrange
        RecordDictionary<int, string> record = new() { [1] = "1", [2] = "2", [3] = "3" };
        Dictionary<int, string> system = new() { [3] = "3", [2] = "2", [1] = "1" };

        // Act & Assert
        Assert.IsTrue(record.Equals(system));
    }

    [TestMethod]
    public void RecordDictionary_SameStrings_EqualsMatchingDictionary()
    {
        // Arrange
        RecordDictionary<string, int> dictionary1 = new() { { "92", 92 }, { "117", 117 }, { "420", 420 }, };
        RecordDictionary<string, int> dictionary2 = new() { { "92", 92 }, { "117", 117 }, { "420", 420 }, };

        // Act
        bool areEqual = dictionary1.Equals(dictionary2);

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void RecordDictionary_SameRecords_EqualsMatchingDictionary()
    {
        // Arrange
        RecordDictionary<string, Number> dictionary1 = new() { { "1", new Number(92) }, { "2", new Number(117) }, { "3", new Number(420) }, };
        RecordDictionary<string, Number> dictionary2 = new() { { "1", new Number(92) }, { "2", new Number(117) }, { "3", new Number(420) }, };

        // Act
        bool areEqual = dictionary1.Equals(dictionary2);

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void RecordDictionary_SameInts_DifferentOrder_NotEqualsSimilarDictionary()
    {
        // Arrange
        Dictionary<int, string> dictionary1 = new() { { 92, "92" }, { 117, "117" }, { 420, "420" }, };
        Dictionary<int, string> dictionary2 = new() { { 117, "117" }, { 420, "420" }, { 92, "92" }, };

        // Act
        bool areEqual = dictionary1.Equals(dictionary2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordDictionary_DifferentValues_NotEquals()
    {
        // Arrange
        RecordDictionary<int, string> dictionary1 = new() { { 1, "one" }, { 2, "two" }, { 3, "three" } };
        RecordDictionary<int, string> dictionary2 = new() { { 1, "one" }, { 2, "two" }, { 3, "four" } };

        // Act
        bool areEqual = dictionary1.Equals(dictionary2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordDictionary_DifferentKeys_NotEquals()
    {
        // Arrange
        RecordDictionary<int, string> dictionary1 = new() { { 1, "one" }, { 2, "two" }, { 3, "three" } };
        RecordDictionary<int, string> dictionary2 = new() { { 1, "one" }, { 2, "two" }, { 4, "three" } };

        // Act
        bool areEqual = dictionary1.Equals(dictionary2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordDictionary_DifferentSizes_NotEquals()
    {
        // Arrange
        RecordDictionary<int, string> dictionary1 = new() { { 1, "one" }, { 2, "two" }, { 3, "three" } };
        RecordDictionary<int, string> dictionary2 = new() { { 1, "one" }, { 2, "two" } };

        // Act
        bool areEqual = dictionary1.Equals(dictionary2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordDictionary_EmptyVsNonEmpty_NotEquals()
    {
        // Arrange
        RecordDictionary<int, string> dictionary1 = [];
        RecordDictionary<int, string> dictionary2 = new() { { 1, "one" } };

        // Act
        bool areEqual = dictionary1.Equals(dictionary2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordDictionary_Clone_ClonesRecordKeysAndValues()
    {
        // Arrange
        Number key1 = new(1);
        Number key2 = new(2);
        Number value1 = new(92);
        Number value2 = new(117);
        RecordDictionary<Number, Number> original = new() { { key1, value1 }, { key2, value2 } };

        // Act
        RecordDictionary<Number, Number> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreNotSame(original, cloned);
        Assert.HasCount(original.Count, cloned);
        foreach (KeyValuePair<Number, Number> kvp in original)
        {
            Assert.IsTrue(cloned.ContainsKey(kvp.Key), "Cloned dictionary should contain cloned key");
            Number clonedKey = cloned.Keys.First(k => k.Value == kvp.Key.Value);
            Number clonedValue = cloned[clonedKey];
            Assert.AreNotSame(kvp.Key, clonedKey, "Key should be cloned");
            Assert.AreNotSame(kvp.Value, clonedValue, "Value should be cloned");
            Assert.AreEqual(kvp.Key.Value, clonedKey.Value, "Cloned key should have same value");
            Assert.AreEqual(kvp.Value.Value, clonedValue.Value, "Cloned value should have same value");
        }
    }

    [TestMethod]
    public void RecordDictionary_Clone_NonRecordKeysAndValues_NotCloned()
    {
        // Arrange
        string key1 = Random.Next().ToString();
        string key2 = Random.Next().ToString();
        string value1 = Random.Next().ToString();
        string value2 = Random.Next().ToString();
        RecordDictionary<string, string> original = new() { { key1, value1 }, { key2, value2 } };

        // Act
        RecordDictionary<string, string> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreNotSame(original, cloned);
        Assert.HasCount(original.Count, cloned);
        foreach (KeyValuePair<string, string> kvp in original)
        {
            string clonedKey = cloned.Keys.First(k => ReferenceEquals(k, kvp.Key));
            string clonedValue = cloned[clonedKey];
            Assert.AreEqual(kvp.Key, clonedKey, "Non-record key should not be cloned");
            Assert.AreEqual(kvp.Value, clonedValue, "Non-record value should not be cloned");
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public void RecordDictionary_Clone_UsesRecordCollectionCloner()
    {
        // Arrange
        TestRecordCollectionCloner testCloner = new();
        RecordCollectionCloner.ElementCloner = testCloner.CloneElement;
        Number key1 = new(1);
        Number value1 = new(92);
        Number key2 = new(2);
        Number value2 = new(117);
        RecordDictionary<Number, Number> original = new() { { key1, value1 }, { key2, value2 } };

        // Act
        RecordDictionary<Number, Number> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreEqual(4, testCloner.CloneCallCount, "ElementCloner should be called for each key and value");
        Assert.Contains(key1, testCloner.ClonedObjects);
        Assert.Contains(value1, testCloner.ClonedObjects);
        Assert.Contains(key2, testCloner.ClonedObjects);
        Assert.Contains(value2, testCloner.ClonedObjects);
    }

    [TestMethod]
    [DoNotParallelize]
    public void RecordDictionary_Clone_CustomElementCloner_Used()
    {
        // Arrange
        Number key1 = new(1);
        Number value1 = new(92);
        RecordDictionary<Number, Number> original = new() { { key1, value1 } };
        bool customClonerCalled = false;
        RecordCollectionCloner.ElementCloner = obj =>
        {
            customClonerCalled = true;
            return obj;
        };

        // Act
        RecordDictionary<Number, Number> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.IsTrue(customClonerCalled, "Custom ElementCloner should be called");
    }

    [TestMethod]
    public void RecordDictionary_Clone_EmptyCollection_Clones()
    {
        // Arrange
        RecordDictionary<int, string> original = [];

        // Act
        RecordDictionary<int, string> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreNotSame(original, cloned);
        Assert.IsEmpty(cloned);
    }

    [TestMethod]
    public void RecordDictionary_DeserializedNewtonsoft_EqualsReserialized()
    {
        // Arrange
        RecordDictionary<string, Number> dictionary = new() { { "1", new Number(92) }, { "2", new Number(117) }, { "3", new Number(420) }, };

        // Act
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(dictionary);
        RecordDictionary<string, Number>? recordDictionary = Newtonsoft.Json.JsonConvert.DeserializeObject<RecordDictionary<string, Number>>(json);
        Dictionary<string, Number>? systemDictionary = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Number>>(json);

        // Assert
        Assert.IsNotNull(recordDictionary, "Deserialized record dictionary is null.");
        Assert.IsNotNull(systemDictionary, "Deserialized dictionary is null.");
        Assert.IsTrue(dictionary.Equals(recordDictionary), "Deserialized dictionary is not equal to the original dictionary.");
        foreach (KeyValuePair<string, Number> kv in dictionary)
        {
            Assert.IsTrue(dictionary[kv.Key] == recordDictionary[kv.Key], "Deserialized dictionary is not a subset of the original dictionary.");
        }
    }

#if !NETFRAMEWORK
    [TestMethod]
    public void RecordDictionary_DeserializedSystemTextJson_EqualsReserialized()
    {
        // Arrange
        RecordDictionary<string, Number> dictionary = new() { { "1", new Number(92) }, { "2", new Number(117) }, { "3", new Number(420) }, };

        // Act
        string json = System.Text.Json.JsonSerializer.Serialize(dictionary);
        RecordDictionary<string, Number>? recordDictionary = System.Text.Json.JsonSerializer.Deserialize<RecordDictionary<string, Number>>(json);
        Dictionary<string, Number>? systemDictionary = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Number>>(json);

        // Assert
        Assert.IsNotNull(recordDictionary, "Deserialized record dictionary is null.");
        Assert.IsNotNull(systemDictionary, "Deserialized dictionary is null.");
        Assert.IsTrue(dictionary.Equals(recordDictionary), "Deserialized dictionary is not equal to the original dictionary.");
        foreach (KeyValuePair<string, Number> kv in dictionary)
        {
            Assert.IsTrue(dictionary[kv.Key] == recordDictionary[kv.Key], "Deserialized dictionary is not a subset of the original dictionary.");
        }
    }
#endif

    [TestMethod]
    public void Stress_RecordDictionary_Int32_Int32_Compare_And_Time()
    {
        // Dictionaries are heavier per element; keep default slightly lower unless overridden.
        int n = GetSizeOrDefault(@default: 500_000);

        RecordDictionary<int, int> left = new(capacity: n);
        RecordDictionary<int, int> right = new(capacity: n);

        for (int i = 0; i < n; i++)
        {
            left[i] = i;
            right[i] = i;
        }

        Stopwatch sw = Stopwatch.StartNew();
        Assert.IsTrue(left.Equals(right));
        sw.Stop();
        TestContext.WriteLine($"RecordDictionary<int,int>.Equals (n={n:n0}) = {sw.ElapsedMilliseconds:n0} ms");

        sw.Restart();
        _ = left.GetHashCode();
        _ = right.GetHashCode();
        sw.Stop();
        TestContext.WriteLine($"RecordDictionary<int,int>.GetHashCode x2 (n={n:n0}) = {sw.ElapsedMilliseconds:n0} ms");
    }

    [TestMethod]
    [RepeatTestMethod(10)]
    public void Stress_RecordDictionary_Int32_Int32_Random_Compare_And_Time()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);
        Random random = new();
        HashSet<int> usedKeys = new(capacity: n);

        RecordDictionary<int, int> left = new(capacity: n);
        RecordDictionary<int, int> right = new(capacity: n);

        for (int i = 0; i < n; i++)
        {
            int key;
            do
            {
                key = random.Next();
            } while (!usedKeys.Add(key));

            int value = random.Next();
            left[key] = value;
            right[key] = value;
        }

        Stopwatch sw = Stopwatch.StartNew();
        Assert.IsTrue(left.Equals(right));
        sw.Stop();
        TestContext.WriteLine($"RecordDictionary<int,int>.Equals with random values (n={n:n0}) = {sw.ElapsedMilliseconds:n0} ms");
    }

    [TestMethod]
    [RepeatTestMethod(10)]
    public void Stress_RecordDictionary_Int32_Int32_Random_OneDifference_NotEquals()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);
        Random random = new();
        HashSet<int> usedKeys = new(capacity: n);
        List<int> keys = new(capacity: n);

        RecordDictionary<int, int> left = new(capacity: n);
        RecordDictionary<int, int> right = new(capacity: n);

        // Add same random key-value pairs to both collections
        for (int i = 0; i < n; i++)
        {
            int key;
            do
            {
                key = random.Next();
            } while (!usedKeys.Add(key));

            keys.Add(key);
            int value = random.Next();
            left[key] = value;
            right[key] = value;
        }

        // Introduce one difference at a random key
        int differenceKey = keys[random.Next(0, n)];
        int originalValue = left[differenceKey];
        int differentValue = random.Next();
        // Ensure the different value is actually different
        while (differentValue == originalValue)
        {
            differentValue = random.Next();
        }
        right[differenceKey] = differentValue;

        // Act
        bool areEqual = left.Equals(right);

        // Assert
        Assert.IsFalse(areEqual, $"Collections should not be equal with one difference at key {differenceKey}");
    }

    [TestMethod]
    [RepeatTestMethod(10)]
    public void Stress_RecordDictionary_String_String_Random_OneDifference_NotEquals()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);
        Random random = new();
        HashSet<string> usedKeys = new(capacity: n);
        List<string> keys = new(capacity: n);

        RecordDictionary<string, string> left = new(capacity: n);
        RecordDictionary<string, string> right = new(capacity: n);

        // Add same random key-value pairs to both collections
        for (int i = 0; i < n; i++)
        {
            string key;
            do
            {
                key = random.Next().ToString();
            } while (!usedKeys.Add(key));

            keys.Add(key);
            string value = random.Next().ToString();
            left[key] = value;
            right[key] = value;
        }

        // Introduce one difference at a random key
        string differenceKey = keys[random.Next(0, n)];
        string originalValue = left[differenceKey];
        string differentValue = random.Next().ToString();
        // Ensure the different value is actually different
        while (differentValue == originalValue)
        {
            differentValue = random.Next().ToString();
        }
        right[differenceKey] = differentValue;

        // Act
        bool areEqual = left.Equals(right);

        // Assert
        Assert.IsFalse(areEqual, $"Collections should not be equal with one difference at key {differenceKey}");
    }

    #region Support Types

    private sealed class OperatorAwareRecordDictionary(Dictionary<int, string> values) : RecordDictionary<int, string>(values)
    {
        public bool TypedEqualsCalled { get; private set; }
        public bool ObjectEqualsCalled { get; private set; }

        public static OperatorAwareRecordDictionary Create() =>
            new(new Dictionary<int, string> { { 1, "1" }, { 2, "2" } });

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

        public override bool Equals(RecordDictionary<int, string>? other)
        {
            TypedEqualsCalled = true;
            return base.Equals(other);
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    #endregion
}
