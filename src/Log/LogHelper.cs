using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.SystemConsole.Themes;

namespace Monod.LogSystem;

/// <summary>
/// Small helper class for initializing <see cref="Log"/>.
/// </summary>
public static class LogHelper
{
    /// <summary>
    /// <see cref="File"/> path to file where <see cref="Log"/> logs
    /// </summary>
    public static readonly string LogFile = $"{AppContext.BaseDirectory}log.txt";
    /// <summary>
    /// Allows switching minimum printed <see cref="LogEventLevel"/> (Use <see cref="SetMinimumLogLevel"/>)
    /// </summary>
    public static LoggingLevelSwitch? LevelSwitch;

    /// <summary>
    /// Initializes <see cref="Log.Logger"/>
    /// </summary>
    public static void Initialize()
    {
        File.Create(LogFile).Close(); //erases file

        string outputTemplate = "[{Timestamp:hh:mm:ss} {Level:u3}] [{Mod}] {Message}{NewLine}{Exception}";
        MessageTemplateTextFormatter formatter = new(outputTemplate);
        LevelSwitch = new();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: outputTemplate, theme: ConsoleTheme, levelSwitch: LevelSwitch)
            .WriteTo.File(formatter, LogFile, levelSwitch: LevelSwitch)
            .Enrich.With(new ModNameEnricher())
            .MinimumLevel.ControlledBy(LevelSwitch)
            .CreateLogger();
    }

    /// <summary>
    /// Write some information related to system (e.g. EntryAssembly.FullName, OS, SystemMemory)
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Assembly.GetEntryAssembly"/> is null</exception>
    public static void WriteStartupInfo()
    {
        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is null) throw new InvalidOperationException("Hello..? Entry assembly is null..??");
        Log.Information("Entry Assembly: {AssemblyName}", entryAssembly.FullName);
        Log.Information("OS: {OS} ({OSID})", RuntimeInformation.OSDescription, RuntimeInformation.RuntimeIdentifier);
        Log.Information("SystemMemory: {Memory} MB\r\n", GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024f / 1024f);
        
    }

    /// <summary>
    /// Switches minimum printed <see cref="LogEventLevel"/> to <paramref name="level"/>
    /// </summary>
    /// <param name="level">New minimum <see cref="LogEventLevel"/>. Messages of this level and higher will be printed to console and log.txt</param>
    /// <exception cref="InvalidOperationException">LevelSwitch is null</exception>
    public static void SetMinimumLogLevel(LogEventLevel level)
    {
        if (LevelSwitch is null)
            throw new InvalidOperationException("LevelSwitch is null");
        LevelSwitch.MinimumLevel = level;
    }

    /// <summary>
    /// <see cref="AnsiConsoleTheme"/> used by ConsoleSink
    /// </summary>
    public static AnsiConsoleTheme ConsoleTheme { get; } = new (
    new Dictionary<ConsoleThemeStyle, string>
    {
        [ConsoleThemeStyle.Text] = "\e[38;5;0253m",
        [ConsoleThemeStyle.SecondaryText] = "\e[38;5;0246m",
        [ConsoleThemeStyle.TertiaryText] = "\e[38;5;0242m",
        [ConsoleThemeStyle.Invalid] = "\e[33;1m",
        [ConsoleThemeStyle.Null] = "\e[38;5;0038m",
        [ConsoleThemeStyle.Name] = "\e[38;5;0081m",
        [ConsoleThemeStyle.String] = "\e[38;5;0216m",
        [ConsoleThemeStyle.Number] = "\e[38;5;151m",
        [ConsoleThemeStyle.Boolean] = "\e[38;5;0038m",
        [ConsoleThemeStyle.Scalar] = "\e[38;5;0079m",
        [ConsoleThemeStyle.LevelVerbose] = "\e[38;2;130;130;130m",
        [ConsoleThemeStyle.LevelDebug] = "\e[38;2;55;213;84m",
        [ConsoleThemeStyle.LevelInformation] = "\e[38;2;55;213;213m",
        [ConsoleThemeStyle.LevelWarning] = "\e[38;2;255;255;0m",
        [ConsoleThemeStyle.LevelError] = "\e[38;2;255;0;0m",
        [ConsoleThemeStyle.LevelFatal] = "\e[38;2;255;0;0m\e[48;2;255;255;0m",
    });
}
