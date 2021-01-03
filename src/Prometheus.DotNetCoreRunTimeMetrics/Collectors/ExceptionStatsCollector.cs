using System.Diagnostics;
using System.Diagnostics.Tracing;
using Prometheus.Client.Abstractions;
using Prometheus.DotNetCoreRunTimeMetrics.Abstraction;
using Prometheus.DotNetCoreRunTimeMetrics.Extensions;

namespace Prometheus.DotNetCoreRunTimeMetrics.Collectors
{
    internal class ExceptionStatsCollector : RuntimeStatsCollectorBase
    {
        private const int EventIdExceptionThrown = 80;
        private const string ExceptionTypeFieldName = "ExceptionType";

        public ExceptionStatsCollector(
            IMetricFactory metricFactory,
            ICollectorExceptionHandler errorHandler) : base(errorHandler)
        {
            ExceptionCount = metricFactory.CreateCounter(
                "dotnet_exceptions_total",
                "Count of exceptions broken down by type",
                false,
                "type");
        }

        public IMetricFamily<ICounter> ExceptionCount { get; }
        protected override EventKeywords Keywords => (EventKeywords)0x00008000;
        protected override EventLevel Level => EventLevel.Informational;
        protected override bool IsInitialized => ExceptionCount != null;

        protected override void ProcessEvent(EventWrittenEventArgs e)
        {
            if (e.EventId == EventIdExceptionThrown)
            {
                ExceptionCount.WithLabels(e.GetVal<string>(ExceptionTypeFieldName)).Inc();
            }
            else
            {
                Debug.WriteLine($"EventId ={e.EventId}");
            }
        }
    }
}
