using System;
using System.Diagnostics.Tracing;
using Prometheus.Client.Abstractions;
using Prometheus.NetRuntimeMetrics.Abstraction;
using Prometheus.NetRuntimeMetrics.Utils;

namespace Prometheus.NetRuntimeMetrics.Collectors
{
    internal class ContentionStatsCollector : RuntimeStatsCollectorBase
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
                .CreateGauge(
                    "dotnet_contention_seconds_total",
                    "The total amount of time spent contending locks");

            ContentionTotal = metricFactory
                .CreateCounter(
                    "dotnet_contention_total",
                    "The number of locks contended");
        }
        public IGauge ContentionSecondsTotal { get; }
        public ICounter ContentionTotal { get; }
        protected override EventKeywords Keywords => (EventKeywords)0x4000;
        protected override EventLevel Level => EventLevel.Informational;

        protected override void ProcessEvent(EventWrittenEventArgs e)
        {
            if (e.EventId == EventIdContentionStop && _sampler.ShouldSample())
            {
                ContentionTotal.Inc();
                ContentionSecondsTotal.Set((double) e.Payload[2] / 1000000000 * _sampler.SampleEvery);
            }
        }
    }
}
