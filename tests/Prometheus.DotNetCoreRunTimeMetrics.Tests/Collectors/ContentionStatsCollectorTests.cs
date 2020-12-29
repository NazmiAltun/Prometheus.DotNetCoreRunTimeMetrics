using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.DotNetCoreRunTimeMetrics.Collectors;
using Xunit;
using Xunit.Abstractions;

namespace Prometheus.DotNetCoreRunTimeMetrics.Tests.Collectors
{
    public class ContentionStatsCollectorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ContentionStatsCollectorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task When_NothingIsLocked_Then_NoContention_Should_BeRecorded()
        {
            using var collector = CreateStatsCollector();

            var lockObj = new object();

            void Lock()
            {
                lock (lockObj)
                {
                }
            }

            await Task.Run(Lock);
            collector.ContentionTotal.Value.Should().Be(0);
            collector.ContentionSecondsTotal.Value.Should().Be(0);
        }

        [Fact]
        public void When_There_Is_Lock_Contention_Then_Contention_Should_BeRecorded()
        {
            const int threadCount = 50;
            const int sleepMs = 10;
            const double expectedContentionSec = (double)sleepMs * threadCount / 1000;
            var lockObj = new object();

            using var collector = CreateStatsCollector();
            void Lock(int sleepTimeout)
            {
                lock (lockObj)
                {
                    Thread.Sleep(sleepTimeout);
                }
            }

            var threads = new List<Thread>();
            for (var i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() =>
                {
                    Lock(sleepMs);
                });
                thread.Start();
                threads.Add(thread);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            collector.ContentionTotal.Value.Should().Be(threadCount - 1);
            collector.ContentionSecondsTotal.Value.Should().BeGreaterOrEqualTo(expectedContentionSec - 0.25);
        }

        private ContentionStatsCollector CreateStatsCollector()
        {
            return new ContentionStatsCollector(
                new MetricFactory(new CollectorRegistry()),
                TestCollectorExceptionHandler.Create(_testOutputHelper));
        }
    }
}