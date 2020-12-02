using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.DotNetCoreRunTimeMetrics.Collectors;
using Prometheus.DotNetCoreRunTimeMetrics.Tests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Prometheus.DotNetCoreRunTimeMetrics.Tests.Collectors
{
    public class ThreadPoolSchedulingStatsCollectorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ThreadPoolSchedulingStatsCollectorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task When_TasksScheduledOnThreadPool_Then_ThreadPoolStatsShouldBeCollected()
        {
            const int taskCount = 50;
            using var collector = CreateStatsCollector();
            for (var i = 0; i < taskCount; i++)
            {
                await Task.Run(() => { });
            }

            ScheduledTasksShouldBeCounted(collector, taskCount);
        }

        private void ScheduledTasksShouldBeCounted(ThreadPoolSchedulingStatsCollector collector, int taskCount)
        {
            DelayHelper.Delay(() => collector.ScheduledCount.Value < taskCount -1 );
            collector.ScheduledCount.Value.Should().BeGreaterOrEqualTo(taskCount - 1);
            collector.ScheduleDelay.Value.Count.Should().BeGreaterOrEqualTo(taskCount - 1);
            collector.ScheduleDelay.Value.Sum.Should().BeGreaterThan(0);
        }

        private ThreadPoolSchedulingStatsCollector CreateStatsCollector()
        {
            return new ThreadPoolSchedulingStatsCollector(
                new MetricFactory(new CollectorRegistry()),
                new MemoryCache(new MemoryCacheOptions()),
                TestCollectorExceptionHandler.Create(_testOutputHelper));
        }
    }
}