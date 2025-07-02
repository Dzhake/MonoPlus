using Serilog.Core;
using Serilog.Events;

namespace MonoPlus.LogSystem;

/// <summary>
/// Enriches logged text by adding Mod=DD property if absent
/// </summary>
public class ModNameEnricher : ILogEventEnricher
{
    /// <summary>
    /// Enrich the log event.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
        "Mod", "DD"));
    }
}