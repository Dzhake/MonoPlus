using System;

namespace MonoPlus.Modding;

/// <summary>
/// Thrown if none of loading <see cref="Mod"/>s could satisfish <see cref="Dep"/> for <see cref="Requester"/>
/// </summary>
public class ModDependencyNotFoundException : Exception
{
    /// <summary>
    /// <see cref="ModId"/> of <see cref="Mod"/> which requested <see cref="Dep"/>
    /// </summary>
    public ModId Requester;

    /// <summary>
    /// <see cref="ModDep"/> which could not be satisfied
    /// </summary>
    public ModDep Dep;

    /// <inheritdoc/>  
    public override string Message => $"Could not find dependency {Dep} for {Requester}";
    /// <summary>
    /// Instances a new <see cref="ModDependencyNotFoundException"/> with specified <see cref="Requester"/> and <see cref="Dep"/>
    /// </summary>
    /// <param name="requester"><see cref="ModId"/> of <see cref="Mod"/> which requested <see cref="Dep"/></param>
    /// <param name="dep"><see cref="ModDep"/> which could not be satisfied</param>
    public ModDependencyNotFoundException(ModId requester, ModDep dep)
    {
        Requester = requester;
        Dep = dep;
    }
}
