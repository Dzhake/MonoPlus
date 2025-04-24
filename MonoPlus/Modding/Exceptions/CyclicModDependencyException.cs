using System;

namespace MonoPlus.Modding;

/// <summary>
/// Thrown if two <see cref="Mod"/>s have each other as dependencies
/// </summary>
public class CyclicModDependencyException : Exception
{
    /// <summary>
    /// <see cref="ModId"/> of mod which has <see cref="SecondId"/> as dependency
    /// </summary>
    public ModId FirstId;

    /// <summary>
    /// <see cref="ModId"/> of mod which has <see cref="FirstId"/> as dependency
    /// </summary>
    public ModId SecondId;

    /// <inheritdoc/>
    public override string Message => $"Cycle mod dependency: {FirstId}, {SecondId}";

    /// <summary>
    /// Instances a new <see cref="CyclicModDependencyException"/> with specified <see cref="FirstId"/> and <see cref="SecondId"/>
    /// </summary>
    /// <param name="firstId"></param>
    /// <param name="secondId"></param>
    public CyclicModDependencyException(ModId firstId, ModId secondId)
    {
        FirstId = firstId;
        SecondId = secondId;
    }
}