using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.NetRuntimeMetrics.Collectors;
using Prometheus.NetRuntimeMetrics.Tests.TestHelpers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Prometheus.NetRuntimeMetrics.Tests.Collectors
{
    [Collection("NoParallelization")]
    public class ContentionStatsCollectorTests
    {
        [Fact]
        public void When_NothingIsLocked_Then_NoContention_Should_BeRecorded()
        {
            var lockObj = new object();
            using var collector = CreateStatsCollector();
            lock (lockObj) { }

            collector.ContentionTotal.Value.Should().Be(0);
            collector.ContentionSecondsTotal.Value.Should().Be(0);
        }

        [Fact]
        public async Task When_There_Is_Lock_Contention_Then_Contention_Should_BeRecorded()
        {
            const int taskCount = 3;
            const int sleepMs = 50;
            const double expectedContentionSec = (double)sleepMs * taskCount / 1000;

            var lockObj = new object();

            void Lock()
            {
                lock (lockObj)
                {
                    Thread.Sleep(sleepMs);
                }
            }

            using var collector = CreateStatsCollector();
            var tasks = Enumerable.Range(0, taskCount).Select(x => Task.Run(Lock)).ToArray();
            Task.WaitAll(tasks);
            await ContentionCountShouldBeCollected(collector, taskCount);
            await ContentionSecondsShouldBeCollected(collector, expectedContentionSec);
        }


        private async Task ContentionCountShouldBeCollected(
            ContentionStatsCollector collector, int taskCount)
        {
            await DelayHelper.DelayAsync(() => collector.ContentionTotal.Value < taskCount - 1);
            collector.ContentionTotal.Value.Should().BeGreaterOrEqualTo(taskCount - 1);
        }

        private async Task ContentionSecondsShouldBeCollected(
            ContentionStatsCollector collector, double expectedContentionSec)
        {
            await DelayHelper.DelayAsync(() =>
                collector.ContentionSecondsTotal.Value < expectedContentionSec);

            collector.ContentionSecondsTotal.Value.Should()
                .BeInRange(expectedContentionSec, expectedContentionSec + 0.1);
        }

        private ContentionStatsCollector CreateStatsCollector()
        {
            return new ContentionStatsCollector(
                new MetricFactory(new CollectorRegistry()),
                new MemoryCache(new MemoryCacheOptions()),
                _ => { },
                1);
        }
    }
}