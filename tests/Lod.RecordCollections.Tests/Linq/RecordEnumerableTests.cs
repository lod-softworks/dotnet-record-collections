namespace Lod.RecordCollections.Tests.Linq;

[TestClass]
public class RecordEnumerableTests
{
    #region ToRecordList

    [TestMethod]
    public void ToRecordList_ValidEnumerable_ReturnsRecordList()
    {
        // Arrange
        IEnumerable<Number> enumerable = Enumerable.Range(1, 10).Select(i => new Number(i));

        // Act
        RecordList<Number> recordList = enumerable.ToRecordList();

        // Assert
        Assert.IsNotNull(recordList);
        Assert.HasCount(enumerable.Count(), recordList);
    }

    [TestMethod]
    public void ToRecordList_NullEnumerable_ThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<Number> enumerable = null!;
        RecordList<Number>? recordList = null;
        Exception? exception = null;

        // Act
        try
        {
            recordList = enumerable.ToRecordList();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(recordList);
        Assert.IsNotNull(exception);
        Assert.IsInstanceOfType<ArgumentNullException>(exception);
    }

    #endregion

    #region ToRecordDictionary
    #pragma warning disable IDE0039 // Use local function

    [TestMethod]
    public void ToRecordDictionary_ValidParams_ReturnsRecordDictionary()
    {
        // Arrange
        IEnumerable<Number> enumerable = Enumerable.Range(1, 10).Select(i => new Number(i));

        // Act
        RecordDictionary<int, Number> recordDictionary = enumerable.ToRecordDictionary(kv => kv.Value);

        // Assert
        Assert.IsNotNull(recordDictionary);
        Assert.HasCount(enumerable.Count(), recordDictionary);
    }

    [TestMethod]
    public void ToRecordDictionary_NullEnumerableAndValidKeySelector_ThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<Number> enumerable = null!;
        RecordDictionary<int, Number>? recordDictionary = null;
        Func<Number, int> keySelector = r => r.Value;
        Exception? exception = null;

        // Act
        try
        {
            recordDictionary = enumerable.ToRecordDictionary(keySelector);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(recordDictionary);
        Assert.IsNotNull(exception);
        Assert.IsInstanceOfType<ArgumentNullException>(exception);
    }

    [TestMethod]
    public void ToRecordDictionary_ValidEnumerableAndNullKeySelector_ThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<Number> enumerable = Enumerable.Range(1, 10).Select(i => new Number(i));
        RecordDictionary<int, Number>? recordDictionary = null;
        Func<Number, int> keySelector = null!;
        Exception? exception = null;

        // Act
        try
        {
            recordDictionary = enumerable.ToRecordDictionary(keySelector);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(recordDictionary);
        Assert.IsNotNull(exception);
        Assert.IsInstanceOfType<ArgumentNullException>(exception);
    }

    [TestMethod]
    public void ToRecordDictionary_NullEnumerableAndValidKeySelectorAndValidElementSelector_ThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<Number> enumerable = null!;
        RecordDictionary<int, Number>? recordDictionary = null;
        Func<Number, int> keySelector = r => r.Value;
        Func<Number, Number> elementSelector = r => r with { };
        Exception? exception = null;

        // Act
        try
        {
            recordDictionary = enumerable.ToRecordDictionary(keySelector, elementSelector);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(recordDictionary);
        Assert.IsNotNull(exception);
        Assert.IsInstanceOfType<ArgumentNullException>(exception);
    }

    [TestMethod]
    public void ToRecordDictionary_ValidEnumerableAndNullKeySelectorAndValidElementSelector_ThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<Number> enumerable = Enumerable.Range(0, 10).Select(i => new Number(i));
        RecordDictionary<int, Number>? recordDictionary = null;
        Func<Number, int> keySelector = null!;
        Func<Number, Number> elementSelector = r => r with { };
        Exception? exception = null;

        // Act
        try
        {
            recordDictionary = enumerable.ToRecordDictionary(keySelector, elementSelector);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(recordDictionary);
        Assert.IsNotNull(exception);
        Assert.IsInstanceOfType<ArgumentNullException>(exception);
    }

    [TestMethod]
    public void ToRecordDictionary_NullEnumerableValidKeySelectorAndValidElementSelector_ThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<Number> enumerable = Enumerable.Range(0, 10).Select(i => new Number(i));
        RecordDictionary<int, Number>? recordDictionary = null;
        Func<Number, int> keySelector = r => r.Value;
        Func<Number, Number> elementSelector = null!;
        Exception? exception = null;

        // Act
        try
        {
            recordDictionary = enumerable.ToRecordDictionary(keySelector, elementSelector);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(recordDictionary);
        Assert.IsNotNull(exception);
        Assert.IsInstanceOfType<ArgumentNullException>(exception);
    }

    #pragma warning restore IDE0039 // Use local function
    #endregion

    #region ToRecordSet

    [TestMethod]
    public void ToRecordSet_ValidEnumerable_ReturnsRecordSet()
    {
        // Arrange
        IEnumerable<Number> enumerable = Enumerable.Range(1, 10).Select(i => new Number(i));

        // Act
        RecordSet<Number> recordSet = enumerable.ToRecordSet();

        // Assert
        Assert.IsNotNull(recordSet);
        Assert.HasCount(enumerable.Count(), recordSet);
    }

    [TestMethod]
    public void ToRecordSet_NullEnumerable_ThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<Number> enumerable = null!;
        RecordSet<Number>? recordSet = null;
        Exception? exception = null;

        // Act
        try
        {
            recordSet = enumerable.ToRecordSet();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(recordSet);
        Assert.IsNotNull(exception);
        Assert.IsInstanceOfType<ArgumentNullException>(exception);
    }

    #endregion

    #region ToRecordStack

    [TestMethod]
    public void ToRecordStack_ValidEnumerable_ReturnsRecordStack()
    {
        // Arrange
        IEnumerable<Number> enumerable = Enumerable.Range(1, 10).Select(i => new Number(i));

        // Act
        RecordStack<Number> recordStack = enumerable.ToRecordStack();

        // Assert
        Assert.IsNotNull(recordStack);
        Assert.HasCount(enumerable.Count(), recordStack);
    }

    [TestMethod]
    public void ToRecordStack_NullEnumerable_ThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<Number> enumerable = null!;
        RecordStack<Number>? recordStack = null;
        Exception? exception = null;

        // Act
        try
        {
            recordStack = enumerable.ToRecordStack();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(recordStack);
        Assert.IsNotNull(exception);
        Assert.IsInstanceOfType<ArgumentNullException>(exception);
    }

    [TestMethod]
    public void ToRecordStack_ValidEnumerable_PreservesOrder()
    {
        // Arrange
        IEnumerable<int> enumerable = [1, 2, 3, 4, 5];

        // Act
        RecordStack<int> recordStack = enumerable.ToRecordStack();

        // Assert
        Assert.IsNotNull(recordStack);
        Assert.HasCount(5, recordStack);
        // Stack is LIFO, so last element should be on top
        Assert.AreEqual(5, recordStack.Peek());
    }

    #endregion

    #region ToRecordQueue

    [TestMethod]
    public void ToRecordQueue_ValidEnumerable_ReturnsRecordQueue()
    {
        // Arrange
        IEnumerable<Number> enumerable = Enumerable.Range(1, 10).Select(i => new Number(i));

        // Act
        RecordQueue<Number> recordQueue = enumerable.ToRecordQueue();

        // Assert
        Assert.IsNotNull(recordQueue);
        Assert.HasCount(enumerable.Count(), recordQueue);
    }

    [TestMethod]
    public void ToRecordQueue_NullEnumerable_ThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<Number> enumerable = null!;
        RecordQueue<Number>? recordQueue = null;
        Exception? exception = null;

        // Act
        try
        {
            recordQueue = enumerable.ToRecordQueue();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(recordQueue);
        Assert.IsNotNull(exception);
        Assert.IsInstanceOfType<ArgumentNullException>(exception);
    }

    [TestMethod]
    public void ToRecordQueue_ValidEnumerable_PreservesOrder()
    {
        // Arrange
        IEnumerable<int> enumerable = [1, 2, 3, 4, 5];

        // Act
        RecordQueue<int> recordQueue = enumerable.ToRecordQueue();

        // Assert
        Assert.IsNotNull(recordQueue);
        Assert.HasCount(5, recordQueue);
        // Queue is FIFO, so first element should be at front
        Assert.AreEqual(1, recordQueue.Peek());
    }

    #endregion
}
