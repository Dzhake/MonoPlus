using System;

namespace Monod.Utils.General;

/// <summary>
/// Class, which is meant to be used like an <see cref="Enum"/>, but with the ability to add more values at runtime.
/// </summary>
public class ExtEnum
{
    /// <summary>
    /// Current amount of the values in this <see cref="ExtEnum"/>.
    /// </summary>
    public int Count => count;

    /// <summary>
    /// Private backing field for <see cref="Count"/>. Current amount of the values in this <see cref="ExtEnum"/>.
    /// </summary>
    protected int count;

    /// <summary>
    /// Add new unnamed value to this <see cref="ExtEnum"/>, and return it. It's guaranteed that returned value is unique for this <see cref="ExtEnum"/>.
    /// </summary>
    /// <returns>New unique value.</returns>
    public int AddValue() => count++;
}