using MonoPlus.Utils.Collections;
using Newtonsoft.Json.Linq;

namespace MonoPlus.Tests;

[TestClass]
public sealed class IndexedListTests
{
    [TestMethod]
    public void TestCtorValues()
    {
        List<int> values = [2, 4, 6, 8, 10];
        IndexedList<int?> list = new(values.Select<int, int?>(value => value));
        for (var i = 0; i < values.Count; i++)
            Assert.AreEqual(values[i], list[i]);
    }

    [TestMethod]
    public void TestGetCapacity()
    {
        IndexedList<int?> list = [2, 4, 6, 8, 10];
        Assert.AreEqual(8, list.Capacity);
    }

    public void TestAdd()
    {
        List<int> values = [2, 4, 6, 8, 10];
        IndexedList<int?> list = new();
        foreach (int value in values)
            list.Add(value);

        for (var i = 0; i < values.Count; i++)
            Assert.AreEqual(values[i], list[i]);
    }

    public void TestAddOut()
    {
        IndexedList<int?> list = new();
        Assert.AreEqual(4, list.Capacity);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);
        list.RemoveAt(2);
        Assert.AreEqual(null, list[2]);
        list.Add(5, out int index);
        Assert.AreEqual(2, index);
    }

    public void TestClear()
    {
        IndexedList<int?> list = [2, 4, 6, 8, 10];
        Assert.AreEqual(5, list.Capacity);
        list.Clear();
        for (int i = 0; i < list.Capacity - 1; i++)
            Assert.AreEqual(null, list[i]);
    }

    [TestMethod]
    public void TestCapacityIncrease()
    {
        IndexedList<int?> list = new();
        Assert.AreEqual(4, list.Capacity);
        for (int i = 0; i < 10; i++)
            list.Add(i);
        Assert.AreEqual(16, list.Capacity);
    }

    [TestMethod]
    public void TestSetCapacityException()
    {
        IndexedList<int?> list = [2, 4, 6, 8, 10];
        Assert.ThrowsException<ArgumentException>(() => list.Capacity = 4);
    }

    [TestMethod]
    public void TestSetCapacity()
    {
        IndexedList<int?> list = [2, 4, 6, 8, 10];
        list.Capacity = 13;
        Assert.AreEqual(13, list.Capacity);
        for (int i = 0; i < 15; i++)
            list.Add(i);
        Assert.AreEqual(26, list.Capacity);
    }

    [TestMethod]
    public void TestEnumerator()
    {
        List<int> values = [0, 2, 4, 6, 10];
        IndexedList<int?> list = [0, null, 2, 4, 6, null, 10, null];
        int i = 0;
        foreach (int? value in list)
        {
            Assert.AreEqual(values[i], value);
            i++;
        }
        Assert.AreEqual(5, i);
    }

    [TestMethod]
    public void TestIndexOf()
    {
        IndexedList<int?> list = [2, 4, 6, 8, 10];
        Assert.AreEqual(3, list.IndexOf(8));
    }

    [TestMethod]
    public void TestAddIfNotFound()
    {
        IndexedList<int?> list = [2, 4, 6, 8, 10];
        list.AddIfNotFound(8);
        list.AddIfNotFound(3);
        int i = 0;
        foreach (int? _ in list) i++;
        Assert.AreEqual(6, i);
    }

    [TestMethod]
    public void TestInsertException()
    {
        IndexedList<int?> list = [2];
        Assert.ThrowsException<ArgumentException>(() => list.Insert(0, 2));
    }

    [TestMethod]
    public void TestInsert()
    {
        IndexedList<int?> list = [2, 4, null, 8, 10];
        list.Insert(2, 6);
        Assert.AreEqual(6, list[2]);
    }
}
