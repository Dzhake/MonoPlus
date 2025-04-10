using Serilog.Core;
using Serilog.Events;

namespace MonoPlus.Logging;

public class ModuleTextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
        "Module", "DD"));
    }
}