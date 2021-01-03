using System.Diagnostics.Tracing;
using Microsoft.Extensions.Caching.Memory;
using Prometheus.Client.Abstractions;
using Prometheus.DotNetCoreRunTimeMetrics.Abstraction;
using Prometheus.DotNetCoreRunTimeMetrics.Utils;

namespace Prometheus.DotNetCoreRunTimeMetrics.Collectors
{
    internal class ThreadPoolSchedulingStatsCollector : FrameworkStatsCollectorBase
    {
        private const int EventIdThreadPoolEnqueueWork = 30;
        private const int EventIdThreadPoolDequeueWork = 31;

        private readonly EventTimer _eventTimer;

        public ThreadPoolSchedulingStatsCollector(
            IMetricFactory metricFactory,
            IMemoryCache memoryCache,
            ICollectorExceptionHandler errorHandler) : this(metricFactory, memoryCache, errorHandler, RuntimeStatCollectorsConfiguration.Default)
        {
        }

        public ThreadPoolSchedulingStatsCollector(
            IMetricFactory metricFactory,
            IMemoryCache memoryCache,
            ICollectorExceptionHandler errorHandler,
            RuntimeStatCollectorsConfiguration configuration) : base(errorHandler)
        {
            _eventTimer = new EventTimer(
                memoryCache,
                EventIdThreadPoolEnqueueWork,
                EventIdThreadPoolDequeueWork,
                x => (long)x.Payload[0],
                "tpoolsched");
            ScheduledCount = metricFactory.CreateCounter("dotnet_threadpool_scheduled_total", "The total number of items the thread pool has been instructed to execute");
            ScheduleDelay = metricFactory.CreateHistogram(
                "dotnet_threadpool_scheduling_delay_seconds",
                "A breakdown of the latency experienced between an item being scheduled for execution on the thread pool and it starting execution.",
                buckets: configuration.HistogramBuckets);
        }

        public ICounter ScheduledCount { get; }
        public IHistogram ScheduleDelay { get; }
        protected override EventKeywords Keywords => (EventKeywords)0x0002;
        protected override EventLevel Level => EventLevel.Verbose;

        protected override bool IsInitialized => ScheduledCount != null &&
                                                 ScheduleDelay != null;

        protected override void ProcessEvent(EventWrittenEventArgs e)
        {
            var eventTime = _eventTimer.GetEventTime(e);

            if (eventTime == EventTime.Start)
            {
                ScheduledCount.Inc();
            }

            if (eventTime.FinalWithDuration)
            {
                ScheduleDelay.Observe(eventTime.Duration.TotalSeconds);
            }
        }
    }
}
