using System;
using System.Collections.Generic;

namespace Monod.Utils.Collections;

/// <summary>
/// Extensions for <see cref="ICollection{T}"/>, <see cref="IEnumerable{T}"/>, and <see cref="IList{T}"/>. 
/// </summary>
public static class ICollectionExtensions
{
    /// <summary>Adds the elements of the specified collection to the end of the <paramref name="source"/>.</summary>
    /// <param name="source">The collection to the end of which elements of the <paramref name="collection"/> will be added</param>
    /// <param name="collection">The collection whose elements should be added to the end of the <see cref="T:System.Collections.Generic.List{T}" />. The collection itself cannot be <see langword="null" />, but it can contain elements that are <see langword="null" />, if type <typeparamref name="T" /> is a reference type.</param>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="source"/> or <paramref name="collection"/> is <see langword="null"/>.</exception>
    public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(collection);

        if (source is List<T> concreteList)
        {
            concreteList.AddRange(collection);
            return;
        }

        foreach (var element in collection)
            source.Add(element);
    }
}