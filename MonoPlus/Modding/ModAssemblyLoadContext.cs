using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace MonoPlus.Modding;

/// <summary>
/// Represents <see cref="Assembly"/> for <see cref="Mod"/>s
/// </summary>
public class ModAssemblyLoadContext : AssemblyLoadContext, IDisposable
{
    /// <summary>
    /// Indicates whether instance is disposing/disposed, to prevent two <see cref="Dispose()"/> calls at same time
    /// </summary>
    private bool disposed;


    /// <summary>
    /// Instances a new <see cref="ModAssemblyLoadContext"/>
    /// </summary>
    public ModAssemblyLoadContext() : base(isCollectible: true)
    {
    }

    /// <summary>
    /// Unloads the context, 
    /// </summary>
    /// <param name="disposing"></param>
    public void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref disposed, true) || !disposing) return;
        
        Unload();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName name)
    {
        return null;
    }
}
