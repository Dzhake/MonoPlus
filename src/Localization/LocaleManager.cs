using System;
using System.IO;
using System.Linq;

namespace Monod.LocalizationSystem;

public class LocaleManager
{
    /// <summary>
    /// <see cref="Directory"/> path to the directory that this <see cref="LocaleManager"/> loads assets from.
    /// </summary>
    public readonly string Dir;
    
    public LocaleManager(string dir)
    {
        Dir = dir;
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(Dir)) throw new InvalidOperationException("Dir is null");
        string[] files = Directory.EnumerateFiles(Dir, "*", SearchOption.AllDirectories).Select(file => Path.GetRelativePath(Dir, file)).ToArray();

        foreach (string file in files) LoadFile(file);
    }

    public void LoadFile(string file)
    {
        
    }
}