using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace MonoPlus.Logging;

public static class Logging
{
    public static LoggingLevelSwitch? LevelSwitch;

    public static void Initialize()
    {
        string outputTemplate = "[{Timestamp:hh:mm:ss} {Level:u3}] [{Module}] {Message}{NewLine}{Exception}";
        MessageTemplateTextFormatter formatter = new(outputTemplate);
        LevelSwitch = new();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatter)
            .WriteTo.File(formatter, $"{AppContext.BaseDirectory}log.txt")
            .Enrich.With(new ModuleTextEnricher())
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

    public static void SetMinimumLogLevel(LogEventLevel level)
    {
        if (LevelSwitch is null)
            throw new InvalidOperationException("SetMinimumLogLevel was called, but LevelSwitch is null!");
        LevelSwitch.MinimumLevel = level;
    }
}
