using System;
using System.Diagnostics.Tracing;
using Prometheus.Client.Abstractions;
using Prometheus.NetRuntimeMetrics.Abstraction;

namespace Prometheus.NetRuntimeMetrics.Collectors
{
    internal class ExceptionStatsCollector : RuntimeStatsCollectorBase
    {
        private const int EventIdExceptionThrown = 80;

        public ExceptionStatsCollector(
            IMetricFactory metricFactory)
            : this(metricFactory, _ => { })
        {
        }

        public ExceptionStatsCollector(
            IMetricFactory metricFactory,
            Action<Exception> errorHandler) : base(errorHandler)
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

        protected override void ProcessEvent(EventWrittenEventArgs e)
        {
            if (e.EventId == EventIdExceptionThrown)
            {
                ExceptionCount.WithLabels((string)e.Payload[0]).Inc();
            }
        }
    }
}
