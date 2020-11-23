using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.NetRuntimeMetrics.Collectors;
using Prometheus.NetRuntimeMetrics.Tests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Prometheus.NetRuntimeMetrics.Tests.Collectors
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
            const int taskCount = 20;
            using var collector = CreateStatsCollector();
            //TODO: Not sure about this test. Might be flaky?
            await Task.WhenAll(Enumerable.Range(0, taskCount).Select(x => Task.Run(() => x)));
            await ScheduledTasksShouldBeCounted(collector, taskCount);
        }

        private async Task ScheduledTasksShouldBeCounted(ThreadPoolSchedulingStatsCollector collector, int taskCount)
        {
            await DelayHelper.DelayAsync(() => collector.ScheduledCount.Value < taskCount);
            collector.ScheduledCount.Value.Should().BeGreaterOrEqualTo(taskCount);
            collector.ScheduleDelay.Value.Count.Should().BeGreaterOrEqualTo(taskCount);
            collector.ScheduleDelay.Value.Sum.Should().BeGreaterThan(0);

        }

        private ThreadPoolSchedulingStatsCollector CreateStatsCollector()
        {
            return new ThreadPoolSchedulingStatsCollector(
                new MetricFactory(new CollectorRegistry()),
                new MemoryCache(new MemoryCacheOptions()),
                _ => { });
        }
    }
}