using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine
{

    /// <summary>
    /// Unified console & AppInsights tracer
    /// </summary>
    public class DebugTracer
    {
        private TelemetryClient AppInsights { get; set; }


        #region Constructors

        private DebugTracer() : this(string.Empty, string.Empty)
        {
        }
        public DebugTracer(string appInsightsKey, string context)
        {
            if (!string.IsNullOrEmpty(appInsightsKey))
            {
                this.AppInsights = new TelemetryClient() { InstrumentationKey = appInsightsKey };
            }
            if (!string.IsNullOrEmpty(context))
            {
                AppInsights.Context.Operation.Name = context;
            }
        }

        public static DebugTracer ConsoleOnlyTracer() { return new DebugTracer(); }


        #endregion

        public void TrackException(Exception ex)
        {
            if (AppInsights != null)
            {
                AppInsights.TrackException(ex);
            }
        }

        public void TrackTrace(string sayWut, Microsoft.ApplicationInsights.DataContracts.SeverityLevel severityLevel)
        {
            Console.WriteLine(sayWut);

            if (AppInsights != null)
            {
                AppInsights.TrackTrace(sayWut, severityLevel);
            }
        }

        public void TrackTrace(string sayWut)
        {
            TrackTrace(sayWut, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information);
        }

        public void TrackEvent(AnalyticsEvent analyticsEvent, string context)
        {
            string desc = "unknown";
            switch (analyticsEvent)
            {
                case AnalyticsEvent.Unknown:
                    break;
                case AnalyticsEvent.AzureAIQuery:
                    desc = "Azure AI query";
                    break;
                default:
                    break;
            }

            Console.WriteLine($"New '{desc}' event; context: '{context}'.");
            if (AppInsights != null)
            {
                string eventName = Enum.GetName(typeof(AnalyticsEvent), analyticsEvent);
                if (string.IsNullOrEmpty(context))
                {
                    AppInsights.TrackEvent(eventName, new Dictionary<string, string>() { { "context", context } });
                }
                else
                {
                    AppInsights.TrackEvent(eventName);
                }
            }
        }

        public enum AnalyticsEvent
        {
            Unknown,
            AzureAIQuery
        }
    }
}
