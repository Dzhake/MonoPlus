using System;

namespace MonoPlus.Modding;

/// <summary>
/// Thrown if two <see cref="Mod"/>s have each other as dependencies
/// </summary>
public class CyclicModDependencyException : Exception
{
    /// <summary>
    /// <see cref="ModID"/> of mod which has <see cref="SecondID"/> as dependency
    /// </summary>
    public ModID FirstID;

    /// <summary>
    /// <see cref="ModID"/> of mod which has <see cref="FirstID"/> as dependency
    /// </summary>
    public ModID SecondID;

    /// <inheritdoc/>
    public override string Message => $"Cycle mod dependency: {FirstID}, {SecondID}";

    /// <summary>
    /// Instances a new <see cref="CyclicModDependencyException"/> with specified <see cref="FirstID"/> and <see cref="SecondID"/>
    /// </summary>
    /// <param name="firstId"></param>
    /// <param name="secondId"></param>
    public CyclicModDependencyException(ModID firstId, ModID secondId)
    {
        FirstID = firstId;
        SecondID = secondId;
    }
}