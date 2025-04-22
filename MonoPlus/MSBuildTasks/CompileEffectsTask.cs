using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MonoPlus.MSBuild;

/// <summary>
/// Used to compile .fx asset files using MGFXC.
/// </summary>
/// <remarks>
///     <para>Exit codes:
/// 0 — success
/// 1 — reserved
/// 2 — Win32 error after starting MGFXC process
/// 3 — Not enough arguments</para>
/// </remarks>
public class CompileEffectsTask : Task
{
    /// <summary>
    /// List of <see cref="File"/> paths, relative to <see cref="PathToContent"/>, of files which should be compiled
    /// </summary>
    public string? Effects { get; set; }

    /// <summary>
    /// Absolute <see cref="Directory"/> path, where assets should be outputted, before their own path
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Absolute <see cref="Directory"/> path, where assets are located, before their own path.
    /// </summary>
    public string? PathToContent { get; set; }

    /// <summary>
    /// Constant to track changes
    /// </summary>
    public const string Version = "1.0";

    /// <summary>
    /// Executes the task, called by MSBuild
    /// </summary>
    /// <returns><see langword="true"/> on success, <see langword="false"/> otherwise</returns>
    public override bool Execute()
    {
        if (Effects is null)
        {
            Log.LogMessage(MessageImportance.High, "Effects was not specified! Must be list of file paths, relative to *PathToContent*, of files which should be compile");
            Environment.Exit(3);
            return false;
        }
        if (OutputPath is null)
        {
            Log.LogMessage(MessageImportance.High, "OutputPath was not specified! Must be absolute directory path, where assets should be outputted, before their own path");
            Environment.Exit(3);
            return false;
        }
        if (PathToContent is null)
        {
            Log.LogMessage(MessageImportance.High, "PathToContent was not specified! Must be bsolute directory path, where assets are located, before their own path.");
            Environment.Exit(3);
            return false;
        }
        Log.LogMessage(MessageImportance.High, $"Running CompileEffectsTask v{Version}");
        Log.LogMessage(MessageImportance.High, $"Effects: {Effects}");

        string[] effects = Effects.Split(';');

        foreach (string effect in effects)
        {
            string inputPath = Path.Combine(PathToContent, effect);
            string outputPath = Path.ChangeExtension(Path.Combine(OutputPath, effect), ".mgfx");
            if (File.GetLastWriteTimeUtc(inputPath) < File.GetLastWriteTimeUtc(outputPath)) return true;

            Directory.CreateDirectory(outputPath);
            try
            {
                var mgfxProcess = Process.Start("mgfxc", $"\"{inputPath}\" \"{outputPath}\"");
                mgfxProcess.WaitForExit();
                Log.LogMessage(MessageImportance.High, $"Compiled {effect}");
            }
            catch (Exception exception)
            {
                int exit = 1;
                if (exception is Win32Exception) //Probably "The system cannot find the file specified"? I hope that the only case.
                {
                    Log.LogMessage(MessageImportance.High, "(Probably) MGFXC could not be started! Please install it as global dotnet tool, and check that you can run it from CMD (global dotnet tools dir should be at PATH).");
                    exit = 2;
                }
                Log.LogMessage(MessageImportance.High, exception.ToString());
                Environment.Exit(exit);
                return false;
            }
        }
        return true;
    }
}
