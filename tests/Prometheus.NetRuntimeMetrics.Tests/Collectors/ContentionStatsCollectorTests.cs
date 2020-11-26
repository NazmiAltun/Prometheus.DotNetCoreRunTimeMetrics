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
            const int taskCount = 10;
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
            await Task.WhenAll(tasks);
            ContentionCountShouldBeCollected(collector, taskCount);
            ContentionSecondsShouldBeCollected(collector, expectedContentionSec);
        }


        private void ContentionCountShouldBeCollected(
            ContentionStatsCollector collector, int taskCount)
        {
            DelayHelper.Delay(() => collector.ContentionTotal.Value < taskCount - 1);
            collector.ContentionTotal.Value.Should().BeGreaterOrEqualTo(taskCount - 1);
        }

        private void ContentionSecondsShouldBeCollected(
            ContentionStatsCollector collector, double expectedContentionSec)
        {
            DelayHelper.Delay(() => collector.ContentionSecondsTotal.Value < expectedContentionSec);
            collector.ContentionSecondsTotal.Value.Should().BeGreaterOrEqualTo(expectedContentionSec);
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