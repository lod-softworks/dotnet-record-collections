using System.Diagnostics;

namespace Lod.RecordCollections.Tests.Collections.Generic;

[TestClass]
public class RecordStackTests
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

    /// <remarks>This test is a sanity check to ensure that the default Stack.Equals behavior is as expected.</remarks>
    [TestMethod]
    public void Stack_SameInts_NotEqualsMatchingStack()
    {
        // Arrange
        Stack<int> stack1 = new([92, 117, 420]);
        Stack<int> stack2 = new([92, 117, 420]);

        // Act
        bool areEqual = stack1.Equals(stack2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    [RepeatTestMethod(3)]
    [DoNotParallelize]
    public void RecordStack_DefaultConstructor_UsesDefaultComparer()
    {
        // Arrange
        TestRecordCollectionComparer comparer = new();
        RecordCollectionComparer.Default = comparer;

        // Act
        RecordStack<int> stack = new();

        // Assert
        Assert.AreSame(comparer, stack.Comparer);
    }

    [TestMethod]
    public void RecordStack_CustomComparerConstructor_UsesProvidedComparer()
    {
        // Arrange
        TestRecordCollectionComparer comparer = new();

        // Act
        RecordStack<int> stack = new(comparer);

        // Assert
        Assert.AreSame(comparer, stack.Comparer);
    }

    [TestMethod]
    public void RecordStack_Operators_UseTypedEquals()
    {
        OperatorAwareRecordStack left = new([92, 117]);
        OperatorAwareRecordStack right = new([92, 117]);

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
    public void RecordStack_SameInts_EqualsMatchingStack()
    {
        // Arrange
        RecordStack<int> stack1 = new([92, 117, 420]);
        RecordStack<int> stack2 = new([92, 117, 420]);

        // Act
        bool areEqual = stack1.Equals(stack2);

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void RecordStack_Equals_Matching_SystemStack()
    {
        // Arrange
        RecordStack<int> record = new([1, 2, 3]);
        Stack<int> system = new([1, 2, 3]);

        // Act & Assert
        Assert.IsTrue(record.Equals(system));
    }

    [TestMethod]
    public void RecordStack_SameStrings_EqualsMatchingStack()
    {
        // Arrange
        RecordStack<string> stack1 = new(["92", "117", "420"]);
        RecordStack<string> stack2 = new(["92", "117", "420"]);

        // Act
        bool areEqual = stack1.Equals(stack2);

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void RecordStack_SameRecords_EqualsMatchingStack()
    {
        // Arrange
        RecordStack<Number> stack1 = new([new Number(92), new Number(117), new Number(420)]);
        RecordStack<Number> stack2 = new([new Number(92), new Number(117), new Number(420)]);

        // Act
        bool areEqual = stack1.Equals(stack2);

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void RecordStack_SameInts_DifferentOrder_NotEqualsSimilarStack()
    {
        // Arrange
        RecordStack<int> stack1 = new([92, 117, 420]);
        RecordStack<int> stack2 = new([420, 117, 92]);

        // Act
        bool areEqual = stack1.Equals(stack2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordStack_DifferentValues_NotEquals()
    {
        // Arrange
        RecordStack<int> stack1 = new([92, 117, 420]);
        RecordStack<int> stack2 = new([92, 117, 421]);

        // Act
        bool areEqual = stack1.Equals(stack2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordStack_DifferentSizes_NotEquals()
    {
        // Arrange
        RecordStack<int> stack1 = new([92, 117, 420]);
        RecordStack<int> stack2 = new([92, 117]);

        // Act
        bool areEqual = stack1.Equals(stack2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordStack_EmptyVsNonEmpty_NotEquals()
    {
        // Arrange
        RecordStack<int> stack1 = new();
        RecordStack<int> stack2 = new([92, 117, 420]);

        // Act
        bool areEqual = stack1.Equals(stack2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordStack_OneValueDifferent_NotEquals()
    {
        // Arrange
        RecordStack<int> stack1 = new([1, 2, 3, 4, 5]);
        RecordStack<int> stack2 = new([1, 2, 99, 4, 5]);

        // Act
        bool areEqual = stack1.Equals(stack2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void RecordStack_Clone_ClonesRecordElements()
    {
        // Arrange
        Number num1 = new(92);
        Number num2 = new(117);
        Number num3 = new(420);
        RecordStack<Number> original = new([num1, num2, num3]);

        // Act
        RecordStack<Number> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreNotSame(original, cloned);
        Assert.HasCount(original.Count, cloned);
        Number[] originalArray = [.. original];
        Number[] clonedArray = [.. cloned];
        for (int i = 0; i < originalArray.Length; i++)
        {
            Assert.AreNotSame(originalArray[i], clonedArray[i], $"Element at index {i} should be cloned");
            Assert.AreEqual(originalArray[i].Value, clonedArray[i].Value, $"Element at index {i} should have same value");
        }
    }

    [TestMethod]
    public void RecordStack_Clone_NonRecordElements_NotCloned()
    {
        // Arrange
        string obj1 = Random.Next().ToString();
        string obj2 = Random.Next().ToString();
        string obj3 = Random.Next().ToString();
        RecordStack<string> original = new([obj1, obj2, obj3]);

        // Act
        RecordStack<string> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreNotSame(original, cloned);
        Assert.HasCount(original.Count, cloned);
        string[] originalArray = [.. original];
        string[] clonedArray = [.. cloned];
        for (int i = 0; i < originalArray.Length; i++)
        {
            Assert.AreSame(originalArray[i], clonedArray[i], $"Non-record element at index {i} should not be cloned");
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public void RecordStack_Clone_UsesRecordCollectionCloner()
    {
        // Arrange
        TestRecordCollectionCloner testCloner = new();
        RecordCollectionCloner.ElementCloner = testCloner.CloneElement;
        Number num1 = new(92);
        Number num2 = new(117);
        RecordStack<Number> original = new([num1, num2]);

        // Act
        RecordStack<Number> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreEqual(2, testCloner.CloneCallCount, "ElementCloner should be called for each element");
        Assert.Contains(num1, testCloner.ClonedObjects);
        Assert.Contains(num2, testCloner.ClonedObjects);
    }

    [TestMethod]
    [DoNotParallelize]
    public void RecordStack_Clone_CustomElementCloner_Used()
    {
        // Arrange
        Number num1 = new(92);
        Number num2 = new(117);
        RecordStack<Number> original = new([num1, num2]);
        bool customClonerCalled = false;
        RecordCollectionCloner.ElementCloner = obj =>
        {
            customClonerCalled = true;
            return obj;
        };

        // Act
        RecordStack<Number> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.IsTrue(customClonerCalled, "Custom ElementCloner should be called");
    }

    [TestMethod]
    public void RecordStack_Clone_EmptyCollection_Clones()
    {
        // Arrange
        RecordStack<int> original = new();

        // Act
        RecordStack<int> cloned = TestRecordCollectionCloner.Clone(original);

        // Assert
        Assert.AreNotSame(original, cloned);
        Assert.IsEmpty(cloned);
    }

    [TestMethod]
    public void RecordStack_PushPop_OrderPreserved()
    {
        // Arrange
        RecordStack<int> stack = new();
        stack.Push(92);
        stack.Push(117);
        stack.Push(420);

        // Act
        int third = stack.Pop();
        int second = stack.Pop();
        int first = stack.Pop();

        // Assert
        Assert.AreEqual(420, third);
        Assert.AreEqual(117, second);
        Assert.AreEqual(92, first);
    }

    [TestMethod]
    public void RecordStack_DeserializedNewtonsoft_HasSameCount()
    {
        // Note: Stack<T> serialization inherently reverses element order.
        // JSON serializers iterate the stack (LIFO order), then push elements during
        // deserialization in that order, effectively reversing the stack.
        // This test verifies count is preserved; element order comparison is not possible
        // without custom serialization converters.

        // Arrange
        RecordStack<Number> stack = new([new Number(92), new Number(117), new Number(420)]);

        // Act
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(stack);
        RecordStack<Number>? recordStack = Newtonsoft.Json.JsonConvert.DeserializeObject<RecordStack<Number>>(json);
        Stack<Number>? systemStack = Newtonsoft.Json.JsonConvert.DeserializeObject<Stack<Number>>(json);

        // Assert
        Assert.IsNotNull(recordStack, "Deserialized record stack is null.");
        Assert.IsNotNull(systemStack, "Deserialized stack is null.");
        Assert.HasCount(stack.Count, recordStack, "Deserialized stack count does not match.");

        // Verify the same elements exist (order will be reversed due to Stack serialization behavior)
        var originalElements = stack.ToHashSet();
        var deserializedElements = recordStack.ToHashSet();
        Assert.IsTrue(originalElements.SetEquals(deserializedElements), "Deserialized stack has different elements.");
    }

#if !NETFRAMEWORK
    [TestMethod]
    public void RecordStack_DeserializedSystemTextJson_HasSameCount()
    {
        // Arrange
        RecordStack<Number> stack = new([new Number(92), new Number(117), new Number(420)]);

        // Act
        string json = System.Text.Json.JsonSerializer.Serialize(stack);
        RecordStack<Number>? recordStack = System.Text.Json.JsonSerializer.Deserialize<RecordStack<Number>>(json);
        Stack<Number>? systemStack = System.Text.Json.JsonSerializer.Deserialize<Stack<Number>>(json);

        // Assert
        Assert.IsNotNull(recordStack, "Deserialized record stack is null.");
        Assert.IsNotNull(systemStack, "Deserialized stack is null.");
        Assert.HasCount(stack.Count, recordStack, "Deserialized stack count does not match.");

        // Verify the same elements exist (order will be reversed due to Stack serialization behavior)
        var originalElements = stack.ToHashSet();
        var deserializedElements = recordStack.ToHashSet();
        Assert.IsTrue(originalElements.SetEquals(deserializedElements), "Deserialized stack has different elements.");
    }
#endif

    [TestMethod]
    public void Stress_RecordStack_Int32_Compare_And_Time()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);

        RecordStack<int> left = new(capacity: n);
        RecordStack<int> right = new(capacity: n);

        for (int i = 0; i < n; i++)
        {
            left.Push(i);
            right.Push(i);
        }

        Stopwatch sw = Stopwatch.StartNew();
        Assert.IsTrue(left.Equals(right));
        sw.Stop();
        TestContext.WriteLine($"RecordStack<int>.Equals (n={n:n0}) = {sw.ElapsedMilliseconds:n0} ms");
    }

    [TestMethod]
    [RepeatTestMethod(10)]
    public void Stress_RecordStack_Int32_Random_Compare_And_Time()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);
        Random random = new();

        RecordStack<int> left = new(capacity: n);
        RecordStack<int> right = new(capacity: n);

        for (int i = 0; i < n; i++)
        {
            int value = random.Next();
            left.Push(value);
            right.Push(value);
        }

        Stopwatch sw = Stopwatch.StartNew();
        Assert.IsTrue(left.Equals(right));
        sw.Stop();
        TestContext.WriteLine($"RecordStack<int>.Equals with random values (n={n:n0}) = {sw.ElapsedMilliseconds:n0} ms");
    }

    [TestMethod]
    [RepeatTestMethod(10)]
    public void Stress_RecordStack_Int32_Random_OneDifference_NotEquals()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);
        Random random = new();
        List<int> values = new(capacity: n);

        RecordStack<int> left = new(capacity: n);
        RecordStack<int> right = new(capacity: n);

        // Add same random values to both collections
        for (int i = 0; i < n; i++)
        {
            int value = random.Next();
            values.Add(value);
            left.Push(value);
            right.Push(value);
        }

        // Rebuild right stack with one difference at a random position
        right = new RecordStack<int>(capacity: n);
        int differenceIndex = random.Next(0, n);
        int originalValue = values[differenceIndex];
        int differentValue = random.Next();
        // Ensure the different value is actually different
        while (differentValue == originalValue)
        {
            differentValue = random.Next();
        }

        // Push values in reverse order (stack is LIFO)
        for (int i = n - 1; i >= 0; i--)
        {
            if (i == differenceIndex)
            {
                right.Push(differentValue);
            }
            else
            {
                right.Push(values[i]);
            }
        }

        // Act
        bool areEqual = left.Equals(right);

        // Assert
        Assert.IsFalse(areEqual, $"Collections should not be equal with one difference at index {differenceIndex}");
    }

    [TestMethod]
    [RepeatTestMethod(10)]
    public void Stress_RecordStack_String_Random_OneDifference_NotEquals()
    {
        int n = GetSizeOrDefault(@default: 1_000_000);
        Random random = new();
        List<string> values = new(capacity: n);

        RecordStack<string> left = new(capacity: n);
        RecordStack<string> right = new(capacity: n);

        // Add same random values to both collections
        for (int i = 0; i < n; i++)
        {
            string value = random.Next().ToString();
            values.Add(value);
            left.Push(value);
            right.Push(value);
        }

        // Rebuild right stack with one difference at a random position
        right = new RecordStack<string>(capacity: n);
        int differenceIndex = random.Next(0, n);
        string originalValue = values[differenceIndex];
        string differentValue = random.Next().ToString();
        // Ensure the different value is actually different
        while (differentValue == originalValue)
        {
            differentValue = random.Next().ToString();
        }

        // Push values in reverse order (stack is LIFO)
        for (int i = n - 1; i >= 0; i--)
        {
            if (i == differenceIndex)
            {
                right.Push(differentValue);
            }
            else
            {
                right.Push(values[i]);
            }
        }

        // Act
        bool areEqual = left.Equals(right);

        // Assert
        Assert.IsFalse(areEqual, $"Collections should not be equal with one difference at index {differenceIndex}");
    }

    #region Support Types

    private sealed class OperatorAwareRecordStack(IEnumerable<int> values) : RecordStack<int>(values)
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

        public override bool Equals(RecordStack<int>? other)
        {
            TypedEqualsCalled = true;
            return base.Equals(other);
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    #endregion
}

