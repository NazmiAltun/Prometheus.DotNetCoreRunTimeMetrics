using Prometheus.Client.Abstractions;
using Prometheus.NetRuntimeMetrics.Abstraction;
using System;
using System.Diagnostics.Tracing;

namespace Prometheus.NetRuntimeMetrics.Collectors
{
    internal class ExceptionStatsCollector : StatsCollectorBase
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

        public override Guid EventSourceGuid => Constants.RuntimeEventSourceId;

        public override EventKeywords Keywords => (EventKeywords)RunTimeEventKeywords.Exception;

        public override EventLevel Level => EventLevel.Informational;

        public override void ProcessEvent(EventWrittenEventArgs e)
        {
            if (e.EventId == EventIdExceptionThrown)
            {
                ExceptionCount.WithLabels((string)e.Payload[0]).Inc();
            }
        }
    }
}
