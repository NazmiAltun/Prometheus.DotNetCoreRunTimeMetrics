using Prometheus.Client.Abstractions;
using Prometheus.NetRuntimeMetrics.Abstraction;
using Prometheus.NetRuntimeMetrics.Utils;
using System;
using System.Diagnostics.Tracing;

namespace Prometheus.NetRuntimeMetrics.Collectors
{
    internal class ContentionStatsCollector : StatsCollectorBase
    {
        private const int DefaultSamplingRate = 1;
        private const int EventIdContentionStart = 81;
        private const int EventIdContentionStop = 91;

        private readonly Sampler _sampler;

        public ContentionStatsCollector(IMetricFactory metricFactory)
            : this(metricFactory, _ => { })
        {
        }

        public ContentionStatsCollector(
            IMetricFactory metricFactory,
            Action<Exception> errorHandler)
            : this(metricFactory, errorHandler, DefaultSamplingRate)
        {
        }

        public ContentionStatsCollector(
            IMetricFactory metricFactory,
            Action<Exception> errorHandler,
            int sampleEvery) : base(errorHandler)
        {
            _sampler = new Sampler(sampleEvery);

            ContentionSecondsTotal = metricFactory
                .CreateCounter(
                    "dotnet_contention_seconds_total",
                    "The total amount of time spent contending locks");

            ContentionTotal = metricFactory
                .CreateCounter(
                    "dotnet_contention_total",
                    "The number of locks contended");
        }
        public ICounter ContentionSecondsTotal { get; }
        public ICounter ContentionTotal { get; }
        public override Guid EventSourceGuid => Constants.RuntimeEventSourceId;
        public override EventKeywords Keywords => (EventKeywords)RunTimeEventKeywords.Contention;
        public override EventLevel Level => EventLevel.Informational;

        public override void ProcessEvent(EventWrittenEventArgs e)
        {
            if (e.EventId == EventIdContentionStop && _sampler.ShouldSample())
            {
                ContentionTotal.Inc();
                ContentionSecondsTotal.Inc((double)e.Payload[2] / 1000000000 * _sampler.SampleEvery);
            }
        }
    }
}
