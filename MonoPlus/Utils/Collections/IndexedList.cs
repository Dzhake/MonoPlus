using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MonoPlus.Utils.Collections;

/// <summary>
/// <b>Read all this text before using.</b>
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// <para>Collection similar to the <see cref="List{T}"/>, but instead of moving elements around when removing elements <see cref="IndexedList{T}"/> places <see langword="null"/> at their place, and when adding new elements searches for indexes with <see langword="null"/>, or resizes collection if needed. Because of this behaviour indexes of the elements in <see cref="IndexedList{T}"/> never change, unless the element is removed, so you can store indexes for any amount of time, allowing you to remove elements from the <see cref="IndexedList{T}"/> much quicker than from the <see cref="List{T}"/>. But if you don't store indexes, then both <see cref="Add(T)"/> and <see cref="Remove"/> operation iterate the collection, unlike only <see cref="List{T}.Remove"/> for the <see cref="List{T}"/>.</para>
/// <para><see cref="IndexedList{T}"/> is also probably much worse if you only modify early indexes of the collection, as <see cref="List{T}"/> would shift items to early indexes and only modify late indexes, and <see cref="IndexedList{T}"/> will iterate all the indexes when adding item to end of the collection.</para>
/// <para><b>DO NOT USE WITH VALUE TYPES!</b> <see cref="IndexedList{T}"/> uses mix of <see langword="default"/> and <see langword="null"/> for empty indexes. For value types those don't match, so collection will be considered having no empty indexes, and whenever you try to add an element it'll resize, because no empty indexes are found. As result, each time you add an element size of the collection doubles. Consider using "T?" (e.g. "<see cref="int"/>?" instead of "<see cref="int"/>" instead, so <see langword="default"/> for it will match <see langword="null"/>.</para>
/// </remarks>
[CollectionBuilder(typeof(IndexedListBuilder), nameof(IndexedListBuilder.Create))]
public class IndexedList<T> : ICollection<T>
{
    /// <summary>
    /// Initializes a new instance of <see cref="IndexedList{T}"/> with capacity 4.
    /// </summary>
    public IndexedList() => array = new T[4];

    /// <summary>
    /// Initializes a new instance of <see cref="IndexedList{T}"/> with capacity set to <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">Capacity of the list.</param>
    public IndexedList(int capacity) => array = new T[capacity];

    /// <summary>
    /// Initializes a new instance of <see cref="IndexedList{T}"/> with same values as in the <paramref name="values"/>.
    /// </summary>
    /// <param name="values">Values to copy to the list.</param>
    public IndexedList(IEnumerable<T> values)
    {
        array = values.ToArray();
        MinFreeIndex = array.Length;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="IndexedList{T}"/> with same values as in the <paramref name="values"/>.
    /// </summary>
    /// <param name="values"></param>
    public IndexedList(ReadOnlySpan<T> values) : this(values.ToArray() as IEnumerable<T>) {}

    /// <summary>
    /// Backend for the list.
    /// </summary>
    protected T?[] array;

    /// <summary>
    /// Value, caching minimum index of empty value in the <see cref="array"/>.
    /// </summary>
    /// <remarks>At any moment, no index empty value might be lower than this value. Methods may use this value to share index where to add new element.</remarks>
    protected int MinFreeIndex;

    /// <summary>
    /// Whether <see cref="MinFreeIndex"/> contains index of empty value.
    /// </summary>
    protected bool FoundMinFreeIndex;

    /// <summary>
    /// Get or set current capacity of the list.
    /// </summary>
    /// <remarks>New capacity can't be smaller than current capacity.</remarks>
    public int Capacity
    {
        get => array.Length;
        set => Resize(value);
    }

    /// <inheritdoc />
    public void Add(T item) => Add(item, out _);

    /// <summary>
    /// Adds an item to the <see cref="IndexedList{T}"/>.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="LinkedList{T}"/>.</param>
    /// <param name="index">Index where the <paramref name="item"/> was added.</param>
    public void Add(T item, out int index)
    {
        FindFreeMinIndex();
        array[MinFreeIndex] = item;
        index = MinFreeIndex;
        MinFreeIndex++;
        FoundMinFreeIndex = false;
    }

    /// <inheritdoc />
    public void Clear() => Array.Clear(array);

    /// <inheritdoc />
    public bool Contains(T item) => IndexOf(item) >= 0;

    /// <inheritdoc />
    public void CopyTo(T[] otherArray, int arrayIndex)
    {
        int pos = arrayIndex;
        foreach (T? item in array)
            if (item is not null)
                otherArray[pos++] = item;
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0) return false;
        RemoveAt(index);
        return true;
    }

    /// <summary>
    /// The number of elements contained in <see cref="IndexedList{T}"/>. <b>This includes empty elements</b>.
    /// </summary>
    public int Count => array.Length;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds the item to the list if it's not in the list already.
    /// </summary>
    /// <param name="item">Item to find in the list.</param>
    public void AddIfNotFound(T item)
    {
        if (IndexOf(item, true) >= 0) return;
        array[MinFreeIndex] = item;
        MinFreeIndex++;
        FoundMinFreeIndex = false;
    }

    /// <summary>
    /// Returns index of the item in the specified
    /// </summary>
    /// <param name="item">TODO</param>
    /// <param name="findFreeIndex"></param>
    /// <returns>Zero-based index of the <paramref name="item"/> in the list, or -1 if not found.</returns>
    public int IndexOf(T item, bool findFreeIndex = false)
    {
        ArgumentNullException.ThrowIfNull(item);
        bool foundMinFreeIndex = FoundMinFreeIndex;
        int result = -1;

        for (int i = 0; i < array.Length; i++)
        {
            T? existing = array[i];
            if (existing is null)
            {
                if (foundMinFreeIndex) continue;
                MinFreeIndex = i;
                foundMinFreeIndex = true;
                FoundMinFreeIndex = true;
                if (result > 0) return result;
            }
            else if (existing.Equals(item))
            {
                if (!findFreeIndex) return i;
                result = i;
            }
        }

        if (!findFreeIndex || foundMinFreeIndex) return result;

        //if we got here then no empty indexes were found, but we need them.
        MinFreeIndex = array.Length;
        FoundMinFreeIndex = true;
        Resize();

        return result;
    }

    /// <summary>
    /// Inserts item to the list at specified <paramref name="index"/>, <b>or throws an exception if <paramref name="index"/> is already taken!</b>
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The object to insert into the list.</param>
    /// <exception cref="ArgumentException"></exception>
    public void Insert(int index, T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)array.Length);
        if (array[index] is not null) throw new ArgumentException($"Index {index} is already taken!");
        array[index] = item;
    }

    /// <summary>
    /// Removes element at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Zero-based index of the element in the <see cref="IndexedList{T}"/>.</param>
    public void RemoveAt(int index)
    {
        array[index] = default(T);
        if (index < MinFreeIndex) MinFreeIndex = index;
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    public T? this[int index]
    {
        get => array[index];
        set => array[index] = value;
    }

    /// <summary>
    /// Sets <see cref="MinFreeIndex"/> to lowest free index in the <see cref="LinkedList{T}"/>, resizing <see cref="array"/> if needed.
    /// </summary>
    protected void FindFreeMinIndex()
    {
        if (FoundMinFreeIndex) return;
        while (MinFreeIndex < array.Length && array[MinFreeIndex] is not null) MinFreeIndex++;
        if (MinFreeIndex == array.Length) Resize();
    }

    /// <summary>
    /// Reszies the <see cref="array"/> to be bigger than before at least by 1 index.
    /// </summary>
    protected void Resize() => Resize(array.Length >= 4 ? array.Length * 2 : 4);

    /// <summary>
    /// Reszies the <see cref="array"/> to make it's size match <paramref name="newCapacity"/>.
    /// </summary>
    /// <param name="newCapacity">New capacity of the list.</param>
    /// <exception cref="ArgumentException"><paramref name="newCapacity"/> is less than current capacity.</exception>
    protected void Resize(int newCapacity)
    {
        if (newCapacity < array.Length)
            throw new ArgumentException($"New capacity ({newCapacity}) can't be less than current capacity ({array.Length})", nameof(newCapacity));
        T?[] newArray = new T[newCapacity];
        array.CopyTo(newArray);
        array = newArray;
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => new IndexedListEnumerator(array);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Enumerator for <see cref="IndexedList{T}"/>.
    /// </summary>
    /// <param name="array"><see cref="IndexedList{T}.array"/></param>
    protected sealed class IndexedListEnumerator(T?[] array) : IEnumerator<T>
    {
        /// <summary>
        /// Current index of enumerator.
        /// </summary>
        private int index = -1;

        /// <inheritdoc />
        public bool MoveNext()
        {
            index++;
            while (index < array.Length && array[index] is null) index++;
            return index < array.Length;
        }

        /// <inheritdoc />
        public void Reset()
        {
            index = -1;
        }

        /// <inheritdoc />
        public T Current => array[index]!;

        object? IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose() {}
    }
}

/// <summary>
/// Collection builder for <see cref="IndexedList{T}"/>, to make it work with collection expressions.
/// </summary>
public class IndexedListBuilder
{
    /// <summary>
    /// Initializes a new instance of <see cref="IndexedList{T}"/> with specified values.
    /// </summary>
    /// <typeparam name="T">Generic type for the <see cref="IndexedList{T}"/>.</typeparam>
    /// <param name="values">Values to copy.</param>
    /// <returns>Initialized instance of the new <see cref="IndexedList{T}"/>.</returns>
    public static IndexedList<T> Create<T>(ReadOnlySpan<T> values) => new(values);
}