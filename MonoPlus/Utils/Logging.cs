﻿using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace MonoPlus;

public static class Logging
{
    public static LoggingLevelSwitch? LevelSwitch;

    public static void Initialize()
    {
        string outputTemplate = "[{Timestamp:hh:mm:ss} {Level:u3}] {Message}{NewLine}{Exception}";
        MessageTemplateTextFormatter formatter = new(outputTemplate);
        LevelSwitch = new();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatter)
            .WriteTo.File(formatter, $"{AppContext.BaseDirectory}log.txt")
            .MinimumLevel.ControlledBy(LevelSwitch)
            .CreateLogger();
    }

    public static void SetMinimumLogLevel(LogEventLevel level)
    {
        if (LevelSwitch is null)
            throw new InvalidOperationException("SetMinimumLogLevel was called, but LevelSwitch is null!");
        LevelSwitch.MinimumLevel = level;
    }
}
