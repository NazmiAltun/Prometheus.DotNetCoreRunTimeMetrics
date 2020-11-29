using System;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Caching.Memory;
using Prometheus.Client.Abstractions;
using Prometheus.NetRuntimeMetrics.Abstraction;
using Prometheus.NetRuntimeMetrics.Utils;

namespace Prometheus.NetRuntimeMetrics.Collectors
{
    //Ref: https://docs.microsoft.com/en-us/dotnet/framework/performance/garbage-collection-etw-events#gcstart_v1-event
    internal class GcStatsCollector : RuntimeStatsCollectorBase
    {
        private const int DefaultSamplingRate = 1;
        private const int GCStart_V1 = 1;
        private const int GCEnd_V1 = 2;
        private const int GCHeapStats_V2 = 4;
        private const int GCAllocationTick_V3 = 10;

        private readonly TimeSpan DefaultCacheDuration = TimeSpan.FromSeconds(60);

        private readonly EventTimer _eventTimer;
        private readonly IMemoryCache _memoryCache;

        public GcStatsCollector(
            IMetricFactory metricFactory,
            IMemoryCache memoryCache,
            Action<Exception> errorHandler) : this(metricFactory, memoryCache, errorHandler, Constants.DefaultHistogramBuckets)
        {
        }

        public GcStatsCollector(
            IMetricFactory metricFactory,
            IMemoryCache memoryCache,
            Action<Exception> errorHandler,
            double[] histograms) : base(errorHandler)
        {
            _memoryCache = memoryCache;
            _eventTimer = new EventTimer(
                memoryCache,
                GCStart_V1,
                GCEnd_V1,
                x => Convert.ToInt64(x.Payload[0]), "gc");
            GcReasons = metricFactory.CreateCounter(
                "dotnet_gc_reason_total",
                "A tally of all the reasons that lead to garbage collections being run",
                false,
                "gc_gen",
                "gc_reason",
                "gc_type");
            GcDuration = metricFactory.CreateHistogram(
                "dotnet_gc_duration",
                "The amount of time spent running garbage collections",
                false,
                histograms,
                "gc_gen",
                "gc_reason",
                "gc_type");
            GcHeapSizeInBytes = metricFactory.CreateGauge(
                "dotnet_gc_heap_size_bytes",
                "The current size of all heaps (only updated after a garbage collection)",
                false,
                "gc_gen");
            LargeObjectAllocationTypeTrigger = metricFactory.CreateCounter(
                "dotnet_gc_loh_type_trigger_total",
                "Objects that triggered Large Object Heap allocation",
                false,
                "type_name");
        }

        public IMetricFamily<ICounter> GcReasons { get; }
        public IMetricFamily<IHistogram> GcDuration { get; }
        public IMetricFamily<IGauge> GcHeapSizeInBytes { get; }
        public IMetricFamily<ICounter> LargeObjectAllocationTypeTrigger { get; }

        protected override EventKeywords Keywords => (EventKeywords)0x00000001;
        protected override EventLevel Level => EventLevel.Verbose;
        protected override void ProcessEvent(EventWrittenEventArgs e)
        {
            HandleStartEndEvents(e);
            HandleHeapStatsEvents(e);
            HandleAllocationEvents(e);
        }

        private void HandleAllocationEvents(EventWrittenEventArgs e)
        {
            const uint lohKindFlag = 0x1;

            if (e.EventId != GCAllocationTick_V3 ||
                ((uint)e.Payload[1] & lohKindFlag) != lohKindFlag)
            {
                return;
            }

            LargeObjectAllocationTypeTrigger.WithLabels((string)e.Payload[5]).Inc();
        }

        private void HandleHeapStatsEvents(EventWrittenEventArgs e)
        {
            if (e.EventId != GCHeapStats_V2)
            {
                return;
            }
            GcHeapSizeInBytes.WithLabels("0").Set((ulong)e.Payload[0]);
            GcHeapSizeInBytes.WithLabels("1").Set((ulong)e.Payload[2]);
            GcHeapSizeInBytes.WithLabels("2").Set((ulong)e.Payload[4]);
            GcHeapSizeInBytes.WithLabels("loh").Set((ulong)e.Payload[6]);
        }

        private void HandleStartEndEvents(EventWrittenEventArgs e)
        {
            if (e.EventId != GCStart_V1 && e.EventId != GCEnd_V1)
            {
                return;
            }

            var eventTime = _eventTimer.GetEventTime(e);

            if (eventTime == EventTime.Start)
            {
                var gcInfo = GcStartInfo.FromEventWrittenEventArgs(e);
                GcReasons.WithLabels(gcInfo.ToLabels()).Inc();
                _memoryCache.Set(GetCacheKey(e), gcInfo, DefaultCacheDuration);
            }

            if (eventTime.FinalWithDuration)
            {
                var gcInfo = _memoryCache.Get<GcStartInfo>(GetCacheKey(e));
                GcDuration.WithLabels(gcInfo.ToLabels()).Observe(eventTime.Duration.TotalSeconds);
            }
        }

        private string GetCacheKey(EventWrittenEventArgs e) => $"GcInfo_{e.Payload[0]}";
    }
}