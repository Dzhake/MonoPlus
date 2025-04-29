using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace MonoPlus.Logging;

/// <summary>
/// Small helper class for initializing <see cref="Log"/>.
/// </summary>
public static class LoggingHelper
{
    /// <summary>
    /// Allows switching minimum printed <see cref="LogEventLevel"/> (Use <see cref="SetMinimumLogLevel"/>)
    /// </summary>
    public static LoggingLevelSwitch? LevelSwitch;

    /// <summary>
    /// Initializes <see cref="Log.Logger"/>
    /// </summary>
    public static void Initialize()
    {
        string outputTemplate = "[{Timestamp:hh:mm:ss} {Level:u3}] [{Mod}] {Message}{NewLine}{Exception}";
        MessageTemplateTextFormatter formatter = new(outputTemplate);
        LevelSwitch = new();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatter)
            .WriteTo.File(formatter, $"{AppContext.BaseDirectory}log.txt")
            .Enrich.With(new ModNameEnricher())
            .MinimumLevel.ControlledBy(LevelSwitch)
            .CreateLogger();

        WriteStartupInfo();
    }

    private static void WriteStartupInfo()
    {
        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is null) throw new InvalidOperationException("Hello..? Entry assembly is null..??");
        Log.Information("Entry Assembly: {AssemblyName}", entryAssembly.FullName);
        Log.Information("OS: {OS} ({OSID})", RuntimeInformation.OSDescription, RuntimeInformation.RuntimeIdentifier);
        Log.Information("SystemMemory: {Memory} MB", GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024f / 1024f);
    }

    /// <summary>
    /// Switches minimum printed <see cref="LogEventLevel"/> to <paramref name="level"/>
    /// </summary>
    /// <param name="level">New minimum <see cref="LogEventLevel"/>. Messages of this level and higher will be printed to console and log.txt</param>
    /// <exception cref="InvalidOperationException">LevelSwitch is null</exception>
    public static void SetMinimumLogLevel(LogEventLevel level)
    {
        if (LevelSwitch is null)
            throw new InvalidOperationException("SetMinimumLogLevel was called, but LevelSwitch is null!");
        LevelSwitch.MinimumLevel = level;
    }
}
