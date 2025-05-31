﻿using System;

 namespace MonoPlus.Utils.Collections;

 /// <summary>
 ///   <para>A simple structure managing an unordered collection of elements.</para>
 /// </summary>
 /// <typeparam name="T">The type of elements in the unordered collection.</typeparam>
 public class ValueBag<T>
 {
     private T[] _items;
     private int _size;

     /// <summary>
     ///   <para>Gets the amount of elements in the bag.</para>
     /// </summary>
     public int Count => _size;

     /// <summary>
     ///   <para>Gets a read-only view of the bag's elements.</para>
     /// </summary>
     public ReadOnlySpan<T> Span => _items.AsSpan(0, _size);

     /// <summary>
     ///   Instances a new <see cref="ValueBag{T}"/>.
     /// </summary>
     public ValueBag() => _items = [];

     /// <summary>
     ///   Instances a new <see cref="ValueBag{T}"/> with specified <paramref name="items"/>.
     /// </summary>
     /// <param name="items"></param>
     public ValueBag(params T[] items)
     {
         _items = items;
         _size = items.Length;
     }

     /// <summary>
     ///   <para>Adds the specified <paramref name="item"/> to the bag.</para>
     /// </summary>
     /// <param name="item">The element to add to the bag.</param>
     public void Add(T item)
     {
         T[] items = _items;
         int size = _size;

         if (size < items.Length)
         {
             _size = size + 1;
             items[size] = item;
             return;
         }
         AddWithResize(item);
     }


     private void AddWithResize(T item)
     {
         int size = _size;

         T[] oldArr = _items;
         T[] newArr = new T[size == 0 ? 4 : size * 2];
         oldArr.CopyTo(newArr, 0);
         _items = newArr;

         _size = size + 1;
         newArr[size] = item;
     }

     /// <summary>
     ///   <para>Removes the specified <paramref name="item"/> from the bag.</para>
     /// </summary>
     /// <param name="item">The element to remove from the bag.</param>
     /// <returns><see langword="true"/>, if the element was successfully removed; otherwise, <see langword="false"/>.</returns>
     public bool Remove(T item)
     {
         int index = Array.IndexOf(_items, item, 0, _size);
         if (index < 0) return false;

         T[] items = _items;
         int size = _size - 1;
         items[index] = items[size];
         items[size] = default!;
         _size = size;
         return true;
     }

     /// <summary>
     ///   <para>Removes an element at the specified <paramref name="index"/> from the bag.</para>
     /// </summary>
     /// <param name="index">The index of an element to remove.</param>
     /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to the bag's <see cref="Count"/>.</exception>
     public void RemoveAt(int index)
     {
         if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException(nameof(index), $"Argument index with value \"{index}\" is less than 0 or greater than {_size - 1}");

         T[] items = _items;
         int size = _size - 1;
         items[index] = items[size];
         items[size] = default!;
         _size = size;
     }

     /// <summary>
     ///   <para>Sets the capacity to the actual number of elements in the bag, if that number is less than a threshold value.</para>
     /// </summary>
     public void TrimExcess()
     {
         int size = _size;
         T[] oldArr = _items;

         if (size >= (int)(oldArr.Length * 0.9)) return;

         T[] newArr = new T[size];
         oldArr.CopyTo(newArr, 0);
         _items = newArr;
     }
 }