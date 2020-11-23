using Microsoft.Extensions.Caching.Memory;
using Prometheus.Client.Abstractions;
using Prometheus.NetRuntimeMetrics.Abstraction;
using Prometheus.NetRuntimeMetrics.Utils;
using System;
using System.Diagnostics.Tracing;

namespace Prometheus.NetRuntimeMetrics.Collectors
{
    internal class ThreadPoolSchedulingStatsCollector : StatsCollectorBase
    {
        private const int DefaultSamplingRate = 1;
        private const int EventIdThreadPoolEnqueueWork = 30;
        private const int EventIdThreadPoolDequeueWork = 31;

        private readonly EventTimer _eventTimer;
        private readonly Sampler _sampler;

        public ThreadPoolSchedulingStatsCollector(
            IMetricFactory metricFactory,
            IMemoryCache memoryCache,
            Action<Exception> errorHandler) : this(metricFactory, memoryCache, errorHandler, Constants.DefaultHistogramBuckets, DefaultSamplingRate)
        {
        }

        public ThreadPoolSchedulingStatsCollector(
            IMetricFactory metricFactory,
            IMemoryCache memoryCache,
            Action<Exception> errorHandler,
            double[] histogramBuckets,
            int sampleEvery) : base(errorHandler)
        {
            _sampler = new Sampler(sampleEvery);
            _eventTimer = new EventTimer(
                memoryCache,
                EventIdThreadPoolEnqueueWork,
                EventIdThreadPoolDequeueWork,
                x => (long)x.Payload[0],
                _sampler);
            ScheduledCount = metricFactory.CreateCounter("dotnet_threadpool_scheduled_total", "The total number of items the thread pool has been instructed to execute");
            ScheduleDelay = metricFactory.CreateHistogram(
                "dotnet_threadpool_scheduling_delay_seconds",
                "A breakdown of the latency experienced between an item being scheduled for execution on the thread pool and it starting execution.",
                buckets: histogramBuckets);
        }

        public ICounter ScheduledCount { get; }
        public IHistogram ScheduleDelay { get; }
        public override Guid EventSourceGuid => Constants.FrameworkEventSourceId;
        public override EventKeywords Keywords => (EventKeywords)FrameworkEventKeywords.ThreadPool;
        public override EventLevel Level => EventLevel.Verbose;

        public override void ProcessEvent(EventWrittenEventArgs e)
        {
            var eventTime = _eventTimer.GetEventTime(e);
            if (eventTime == EventTime.Start)
            {
                ScheduledCount.Inc();
            }

            if (eventTime.FinalWithDuration)
            {
                ScheduleDelay.Observe(eventTime.Duration.TotalSeconds, _sampler.SampleEvery);
            }
        }
    }
}
