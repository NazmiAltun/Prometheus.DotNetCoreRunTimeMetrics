using Microsoft.Extensions.Caching.Memory;
using Prometheus.Client.Abstractions;
using Prometheus.NetRuntimeMetrics.Abstraction;
using Prometheus.NetRuntimeMetrics.Utils;
using System;
using System.Diagnostics.Tracing;

namespace Prometheus.NetRuntimeMetrics.Collectors
{
    internal class ContentionStatsCollector : StatsCollectorBase
    {
        private const int DefaultSamplingRate = 2;
        private const int EventIdContentionStart = 81;
        private const int EventIdContentionStop = 91;

        private readonly EventTimer _eventTimer;
        private readonly Sampler _sampler;

        public ContentionStatsCollector(
            IMetricFactory metricFactory,
            IMemoryCache memoryCache)
            : this(metricFactory, memoryCache, _ => { })
        {
        }

        public ContentionStatsCollector(
            IMetricFactory metricFactory,
            IMemoryCache memoryCache,
            Action<Exception> errorHandler)
            : this(metricFactory, memoryCache, errorHandler, DefaultSamplingRate)
        {
        }

        public ContentionStatsCollector(
            IMetricFactory metricFactory,
            IMemoryCache memoryCache,
            Action<Exception> errorHandler,
            int sampleEvery) : base(errorHandler)
        {
            _sampler = new Sampler(sampleEvery);
            _eventTimer = new EventTimer(
                memoryCache,
                EventIdContentionStart,
                EventIdContentionStop,
                _ => _.OSThreadId,
                _sampler);

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
            var eventTime = _eventTimer.GetEventTime(e);

            if (eventTime == EventTime.Start)
            {
                ContentionTotal.Inc();
            }

            if (eventTime.HasDuration)
            {
                ContentionSecondsTotal.Inc(
                    eventTime.Duration.TotalSeconds * _sampler.SampleEvery);
            }
        }
    }
}
