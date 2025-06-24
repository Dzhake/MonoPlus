using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace MonoPlus.ModSystem;

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
    private readonly FileSystemWatcher? watcher;

    /// <summary>
    /// <see cref="ModConfig"/> for same <see cref="Mod"/> as this <see cref="ModAssemblyLoadContext"/>
    /// </summary>
    private readonly Mod mod;

    /// <summary>
    /// Instances a new <see cref="ModAssemblyLoadContext"/>.
    /// </summary>
    /// <param name="mod">Mod related to the newly created assembly context.</param>
    /// <exception cref="InvalidOperationException"><paramref name="mod.Config.AssemblyFile"/> is <see langword="null"/></exception>
    public ModAssemblyLoadContext(Mod mod) : base(isCollectible: true)
    {
        ModConfig config = mod.Config;
        if (config.AssemblyFile is null) throw new InvalidOperationException("Trying to create ModAssemblyLoadContext with config which has null AssemblyFile");
        this.mod = mod;

        if (!MonoPlusMain.HotReload) return;
        watcher = new(Path.Combine(mod.Directory, Path.GetDirectoryName(config.AssemblyFile) ?? ""), Path.GetFileName(config.AssemblyFile));
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += OnFileChanged;
        
        watcher.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (watcher != sender) return;
        ModManager.ReloadAssembly(mod);
        watcher.EnableRaisingEvents = false; //since assembly is going to be reloaded, "this" will be disposed, so we don't need watcher anymore. Also fixes issue with events being raised twice.
    }

    /// <summary>
    /// Disposes the <see cref="ModAssemblyLoadContext"/>.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    public void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref disposed, true) || !disposing) return;
        
        watcher?.Dispose();
        Unload();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(false);
        GC.SuppressFinalize(this);
    }
}
