using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Prometheus.Client.Abstractions;
using Prometheus.NetRuntimeMetrics.Abstraction;

namespace Prometheus.NetRuntimeMetrics.Collectors
{
    //ref:https://docs.microsoft.com/en-us/dotnet/framework/performance/thread-pool-etw-events
    internal class ThreadPoolStatsCollector : RuntimeStatsCollectorBase
    {
        private const string ActiveWorkerThreadCountFieldName = "ActiveWorkerThreadCount";
        private const string RetiredWorkerThreadCountFieldName = "RetiredWorkerThreadCount";
        private const string ThroughputFieldName = "Throughput";
        private const string ReasonFieldName = "Reason";
        private const string NewWorkerThreadCountFieldName = "NewWorkerThreadCount";
        private const string IoThreadCountFieldName = "IOThreadCount";
        private const string IoRetiredThreadFieldName = "RetiredIOThreads";

        private const int ThreadPoolWorkerThreadStartEventId = 50;
        private const int ThreadPoolWorkerThreadStopEventId = 51;
        private const int ThreadPoolWorkerThreadRetirementStartEventId = 52;
        private const int ThreadPoolWorkerThreadRetirementStopEventId = 53;
        private const int ThreadPoolWorkerThreadAdjustmentSampleEventId = 54;
        private const int ThreadPoolWorkerThreadAdjustmentEventId = 55;
        private const int IoThreadCreateV1EventId = 44;
        private const int IoThreadTerminateEventId = 45;
        private const int IoThreadRetireV1EventId = 46;
        private const int IoThreadUnretireV1EventId = 47;

        private static readonly Dictionary<uint, string> WorkerThreadReasonTable = new Dictionary<uint, string>
        {
            {0x00 ,"Warmup" },
            {0x01 ,"Initializing" },
            {0x02 ,"Random move" },
            {0x03 ,"Climbing move" },
            {0x04 ,"Change point" },
            {0x05 ,"Stabilizing" },
            {0x06 ,"Starvation" },
            {0x07 ,"Thread timed out" },
        };

        public ThreadPoolStatsCollector(
            IMetricFactory metricFactory,
            Action<Exception> errorHandler) : base(errorHandler)
        {
            WorkerActiveThreadCount = metricFactory.CreateGauge(
                "dotnet_thread_pool_active_worker_thread_total",
                "Total number of active worker threads in the thread pool");
            WorkerRetiredThreadCount = metricFactory.CreateGauge(
                "dotnet_thread_pool_retired_worker_thread_total",
                "Total number of retired worker threads in the thread pool");
            ThreadPoolWorkerThreadAdjustmentThroughput = metricFactory.CreateGauge(
                "dotnet_thread_pool_worker_thread_adjustment_throughput",
                "Refers to the collection of information for one sample; that is, a measurement of throughput with a certain concurrency level, in an instant of time.");
            WorkerThreadPoolAdjustmentReasonCount = metricFactory.CreateCounter(
                "dotnet_thread_pool_worker_thread_adjustment_reason",
                "Records a change in control, when the thread injection (hill-climbing) algorithm determines that a change in concurrency level is in place.",
                false,
                "reason");
            IoThreadCount = metricFactory.CreateGauge(
                "dotnet_thread_pool_io_thread_total",
                "Number of I/O threads in the thread pool, including this one");
            IoRetiredCount = metricFactory.CreateGauge(
                "dotnet_thread_pool_retired_io_thread_total",
                "Number of retired I/O threads.");
        }

        public IGauge WorkerActiveThreadCount { get; }
        public IGauge WorkerRetiredThreadCount { get; }
        public IGauge ThreadPoolWorkerThreadAdjustmentThroughput { get; }
        public IMetricFamily<ICounter> WorkerThreadPoolAdjustmentReasonCount { get; }
        public IGauge IoThreadCount { get; }
        public IGauge IoRetiredCount { get; }

        protected override EventKeywords Keywords => (EventKeywords)0x10000;
        protected override EventLevel Level => EventLevel.Informational;

        protected override void ProcessEvent(EventWrittenEventArgs e)
        {
            HandleWorkerThreadStartAndStop(e);
            HandleWorkerThreadAdjustment(e);
            HandleWorkerThreadAdjustmentEvent(e);
            HandleIoThreadEvents(e);
        }

        private void HandleIoThreadEvents(EventWrittenEventArgs e)
        {
            if (e.EventId != IoThreadCreateV1EventId &&
                e.EventId != IoThreadRetireV1EventId &&
                e.EventId != IoThreadTerminateEventId &&
                e.EventId != IoThreadUnretireV1EventId)
            {
                return;
            }
            IoThreadCount.Set(e.GetVal<uint>(IoThreadCountFieldName));
            IoRetiredCount.Set(e.GetVal<uint>(IoRetiredThreadFieldName));
        }

        private void HandleWorkerThreadAdjustmentEvent(EventWrittenEventArgs e)
        {
            if (e.EventId != ThreadPoolWorkerThreadAdjustmentEventId)
            {
                return;
            }

            WorkerThreadPoolAdjustmentReasonCount
                .WithLabels(WorkerThreadReasonTable[e.GetVal<uint>(ReasonFieldName)])
                .Inc(e.GetVal<uint>(NewWorkerThreadCountFieldName));
        }

        private void HandleWorkerThreadAdjustment(EventWrittenEventArgs e)
        {
            if (e.EventId != ThreadPoolWorkerThreadAdjustmentSampleEventId)
            {
                return;
            }
            ThreadPoolWorkerThreadAdjustmentThroughput.Set(e.GetVal<double>(ThroughputFieldName));
        }

        private void HandleWorkerThreadStartAndStop(EventWrittenEventArgs e)
        {
            if (e.EventId != ThreadPoolWorkerThreadStartEventId &&
                e.EventId != ThreadPoolWorkerThreadStopEventId &&
                e.EventId != ThreadPoolWorkerThreadRetirementStartEventId &&
                e.EventId != ThreadPoolWorkerThreadRetirementStopEventId)
            {
                return;
            }

            WorkerActiveThreadCount.Set(e.GetVal<uint>(ActiveWorkerThreadCountFieldName));
            WorkerRetiredThreadCount.Set(e.GetVal<uint>(RetiredWorkerThreadCountFieldName));
        }
    }
}
