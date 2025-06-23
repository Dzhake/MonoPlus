namespace MonoPlus.Utils.Collections.Tests;

[TestClass]
public class IndexedListTests
{
    [TestMethod]
    public void ConstructorCapacity4()
    {
        var list = new IndexedList<int?>();
        Assert.AreEqual(4, list.Capacity);
        Assert.AreEqual(4, list.Count);
    }

    [TestMethod]
    public void ConstructorSpecifiedCapacity()
    {
        var list = new IndexedList<int?>(10);
        Assert.AreEqual(10, list.Capacity);
        Assert.AreEqual(10, list.Count);
    }

    [TestMethod]
    public void ConstructorCopiesElements()
    {
        var collection = new List<int?> { 1, 2, 3 };
        var list = new IndexedList<int?>(collection);
            
        Assert.AreEqual(3, list.Count);
        Assert.AreEqual(1, list[0]);
        Assert.AreEqual(2, list[1]);
        Assert.AreEqual(3, list[2]);
    }

    [TestMethod]
    public void Add()
    {
        var list = new IndexedList<string>
        {
            "first",
            "second"
        };

        Assert.AreEqual("first", list[0]);
        Assert.AreEqual("second", list[1]);
        Assert.IsNull(list[2]);
        Assert.IsNull(list[3]);
    }

    [TestMethod]
    public void AddUsesRemovedIndexes()
    {
        var list = new IndexedList<string>
        {
            "first",
            "second",
            "third"
        };
        list.RemoveAt(1);
            
        list.Add("new");
            
        Assert.AreEqual("first", list[0]);
        Assert.AreEqual("new", list[1]);
        Assert.AreEqual("third", list[2]);
    }

    [TestMethod]
    public void AddReturnsIndex()
    {
        var list = new IndexedList<string>();
        list.Add("first", out int index1);
        list.Add("second", out int index2);
            
        Assert.AreEqual(0, index1);
        Assert.AreEqual(1, index2);
    }

    [TestMethod]
    public void AddHandler()
    {
        var list = new IndexedList<int?>();
        Func<int, int?> handler = index => index * 2 + 1;
        list.Add(handler);
        list.Add(handler);
        list.Add(handler);
        Assert.AreEqual(1, list[0]);
        Assert.AreEqual(3, list[1]);
        Assert.AreEqual(5, list[2]);
    }
    
    [TestMethod]
    public void Remove()
    {
        var list = new IndexedList<string>
        {
            "first",
            "second"
        };

        bool result = list.Remove("first");
            
        Assert.IsTrue(result);
        Assert.IsNull(list[0]);
        Assert.AreEqual("second", list[1]);
    }

    [TestMethod]
    public void RemoveNonExistentItem()
    {
        var list = new IndexedList<string> { "first" };

        bool result = list.Remove("nonexistent");
            
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void RemoveAt()
    {
        var list = new IndexedList<string>
        {
            "first",
            "second"
        };

        list.RemoveAt(0);
            
        Assert.IsNull(list[0]);
        Assert.AreEqual("second", list[1]);
    }

    [TestMethod]
    public void Contains()
    {
        var list = new IndexedList<string> { "first" };

        Assert.IsTrue(list.Contains("first"));
        Assert.IsFalse(list.Contains("second"));
    }

    [TestMethod]
    public void IndexOf()
    {
        var list = new IndexedList<string>
        {
            "first",
            "second"
        };

        Assert.AreEqual(0, list.IndexOf("first"));
        Assert.AreEqual(1, list.IndexOf("second"));
        Assert.AreEqual(-1, list.IndexOf("nonexistent"));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void IndexOfNullItem()
    {
        var list = new IndexedList<string>();
        list.IndexOf(null!);
    }

    [TestMethod]
    public void Insert()
    {
        var list = new IndexedList<string>(2);
        list.Insert(1, "item");
            
        Assert.IsNull(list[0]);
        Assert.AreEqual("item", list[1]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void InsertException()
    {
        var list = new IndexedList<string> { "first" };
        list.Insert(0, "second");
    }

    [TestMethod]
    public void Clear()
    {
        var list = new IndexedList<string>
        {
            "first",
            "second"
        };

        list.Clear();
            
        Assert.IsNull(list[0]);
        Assert.IsNull(list[1]);
    }

    [TestMethod]
    public void CapacityResize()
    {
        var list = new IndexedList<string>(2)
        {
            Capacity = 4
        };

        Assert.AreEqual(4, list.Capacity);
        Assert.AreEqual(4, list.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CapacityException()
    {
        var list = new IndexedList<string>(4);
        list.Capacity = 2;
    }

    [TestMethod]
    public void Indexer()
    {
        var list = new IndexedList<string>
        {
            [0] = "first",
            [1] = "second"
        };

        Assert.AreEqual("first", list[0]);
        Assert.AreEqual("second", list[1]);
    }

    [TestMethod]
    public void AddIfNotFound()
    {
        var list = new IndexedList<string>();
        list.AddIfNotFound("first");
        list.AddIfNotFound("first");
            
        Assert.AreEqual("first", list[0]);
        Assert.IsNull(list[1]);
    }

    [TestMethod]
    public void Enumerator()
    {
        var list = new IndexedList<string>
        {
            "first",
            "second"
        };
        list.RemoveAt(1);
            
        var items = new List<string>();
        foreach (var item in list)
            items.Add(item);
            
        CollectionAssert.AreEqual(new[] { "first" }, items);
    }

    [TestMethod]
    public void CopyTo()
    {
        var list = new IndexedList<string>
        {
            "first",
            "second"
        };

        var array = new string[2];
        list.CopyTo(array, 0);
            
        CollectionAssert.AreEqual(new[] { "first", "second" }, array);
    }

    [TestMethod]
    public void Resize()
    {
        var list = new IndexedList<string>(2)
        {
            "first",
            "second",
            "third" // Should trigger resize
        };

        Assert.AreEqual(4, list.Capacity);
    }
}