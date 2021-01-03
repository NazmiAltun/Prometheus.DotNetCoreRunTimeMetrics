using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.DotNetCoreRunTimeMetrics.Collectors;
using Xunit;
using Xunit.Abstractions;

namespace Prometheus.DotNetCoreRunTimeMetrics.Tests.Collectors
{
    public class ThreadPoolStatsCollectorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ThreadPoolStatsCollectorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ThreadPoolStatsShouldBeCollected()
        {
            using var collector = CreateStatsCollector();
            SpamTasksOnThreadPool();
            AssertWorkerThreadPoolAdjustmentReasonCount(collector);
            AssertWorkerThreadCountIncreased(collector);
        }

        private void AssertWorkerThreadPoolAdjustmentReasonCount(ThreadPoolStatsCollector collector)
        {
            using var resetEvent = new AssertionManualResetEvent(() =>
                collector.WorkerThreadPoolAdjustmentReasonCount.Labelled.Any());
            resetEvent.Wait();
            collector.WorkerThreadPoolAdjustmentReasonCount.Labelled.First().Value.Value.Should().BeGreaterThan(0);
        }

        private void AssertWorkerThreadCountIncreased(ThreadPoolStatsCollector collector)
        {
            using var resetEvent = new AssertionManualResetEvent(() =>
                collector.WorkerActiveThreadCount.Value >= 0);
            resetEvent.Wait();
            collector.WorkerActiveThreadCount.Value.Should().BeGreaterThan(0);
        }

        private void SpamTasksOnThreadPool()
        {
            const int taskCount = 1000;

            for (var i = 0; i < taskCount; i++)
            {
                Task.Run(() =>
                {
                    var end = DateTime.Now + TimeSpan.FromSeconds(3);

                    while (DateTime.Now < end)
                    {
                    }
                });
            }
        }

        private ThreadPoolStatsCollector CreateStatsCollector()
        {
            return new ThreadPoolStatsCollector(
                new MetricFactory(new CollectorRegistry()),
                TestCollectorExceptionHandler.Create(_testOutputHelper));
        }
    }
}