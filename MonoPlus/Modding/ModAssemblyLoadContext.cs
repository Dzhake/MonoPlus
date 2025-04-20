using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

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
    /// <see cref="FileSystemWatcher"/> which watches for .dll <see cref="File"/>
    /// </summary>
    private FileSystemWatcher watcher;

    /// <summary>
    /// <see cref="ModConfig"/> for same <see cref="Mod"/> as this <see cref="ModAssemblyLoadContext"/>
    /// </summary>
    private ModConfig Config;

    /// <summary>
    /// Instances a new <see cref="ModAssemblyLoadContext"/>
    /// </summary>
    public ModAssemblyLoadContext(ModConfig config) : base(isCollectible: true)
    {
        if (config.AssemblyFile is null) throw new InvalidOperationException("Trying to create ModAssemblyLoadContext with config which has null AssemblyFile");
        
        Config = config;

        watcher = new(config.ModDirectory, config.AssemblyFile);
        watcher.Changed += OnFileChanged;
        watcher.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (watcher != sender) return;
        ModLoader.ReloadMod(Config);
    }

    /// <summary>
    /// Unloads the context, 
    /// </summary>
    /// <param name="disposing"></param>
    public void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref disposed, true) || !disposing) return;
        
        watcher.Dispose();
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
