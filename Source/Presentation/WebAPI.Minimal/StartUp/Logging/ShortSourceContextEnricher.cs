using Serilog.Core;
using Serilog.Events;

namespace WebAPI.Minimal.StartUp.Logging;

public class ShortSourceContextEnricher : ILogEventEnricher
{
    public const string PropertyName = "ShortSourceContext";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue("SourceContext", out var context))
            return;

        var fullName = context.ToString().Trim('"');
        var parts = fullName.Split('.');

        if (parts.Length >= 2)
        {
            var project = parts.First();
            var className = parts.Last();
            var formatted = $"{project}.{className}";

            var shortContext = propertyFactory.CreateProperty(PropertyName, formatted);
            logEvent.AddOrUpdateProperty(shortContext);
        }
    }
}