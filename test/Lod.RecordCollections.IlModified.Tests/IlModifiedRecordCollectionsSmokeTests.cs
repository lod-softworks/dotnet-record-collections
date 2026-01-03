namespace Lod.RecordCollections.IlModified.Tests;

[TestClass]
public class IlModifiedRecordCollectionsSmokeTests
{
    [TestMethod]
    public void WithExpression_ClonesRecordDictionary()
    {
        var original = new RecordDictionary<int, int>
        {
            [1] = 10,
            [2] = 20,
        };

        // This only compiles if the referenced binary is record-compliant (i.e., IL-modified).
        var clone = original with { };

        Assert.IsTrue(original.Equals(clone));
        Assert.AreNotSame(original, clone);
        Assert.HasCount(original.Count, clone);

        clone[3] = 30;
        Assert.HasCount(2, original);
        Assert.HasCount(3, clone);
    }

    [TestMethod]
    public void WithExpression_ClonesRecordList()
    {
        var original = new RecordList<int>([1, 2, 3,]);

        // This only compiles if the referenced binary is record-compliant (i.e., IL-modified).
        var clone = original with { };

        CollectionAssert.AreEqual(original, clone);
        Assert.IsTrue(original.Equals(clone));
        Assert.AreNotSame(original, clone);

        clone.Add(4);
        Assert.HasCount(3, original);
        Assert.HasCount(4, clone);
    }

    [TestMethod]
    public void WithExpression_ClonesRecordQueue()
    {
        var original = new RecordQueue<int>([1, 2, 3,]);

        // This only compiles if the referenced binary is record-compliant (i.e., IL-modified).
        var clone = original with { };

        CollectionAssert.AreEqual(original, clone);
        Assert.IsTrue(original.Equals(clone));
        Assert.AreNotSame(original, clone);

        clone.Enqueue(4);
        Assert.HasCount(3, original);
        Assert.HasCount(4, clone);
    }

    [TestMethod]
    public void WithExpression_ClonesRecordSet()
    {
        var original = new RecordSet<int>([1, 2, 3,]);

        // This only compiles if the referenced binary is record-compliant (i.e., IL-modified).
        var clone = original with { };

        CollectionAssert.AreEqual(original, clone);
        Assert.IsTrue(original.Equals(clone));
        Assert.AreNotSame(original, clone);

        clone.Add(4);
        Assert.HasCount(3, original);
        Assert.HasCount(4, clone);
    }

    [TestMethod]
    public void WithExpression_ClonesRecordStack()
    {
        var original = new RecordStack<int>([1, 2, 3,]);

        // This only compiles if the referenced binary is record-compliant (i.e., IL-modified).
        var clone = original with { };

        CollectionAssert.AreEqual(original, clone);
        Assert.IsTrue(original.Equals(clone));
        Assert.AreNotSame(original, clone);

        clone.Push(4);
        Assert.HasCount(3, original);
        Assert.HasCount(4, clone);
    }
}
