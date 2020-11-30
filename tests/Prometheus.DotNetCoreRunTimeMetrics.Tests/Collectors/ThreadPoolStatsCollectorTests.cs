using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.DotNetCoreRunTimeMetrics.Collectors;
using Prometheus.DotNetCoreRunTimeMetrics.Tests.TestHelpers;
using Xunit;

namespace Prometheus.DotNetCoreRunTimeMetrics.Tests.Collectors
{
    public class ThreadPoolStatsCollectorTests
    {
        [Fact]
        public async Task ThreadPoolStatsShouldBeCollected()
        {
            using var collector = CreateStatsCollector();
            await SpamCpuAndIoBoundTasksToThreadPool();
            await AssertWorkerThreadsAreCreated(collector);
            collector.ThreadPoolWorkerThreadAdjustmentThroughput.Value.Should().BeGreaterThan(0);
            collector.IoThreadCount.Value.Should().BeGreaterThan(0);
            collector.WorkerThreadPoolAdjustmentReasonCount.WithLabels("Warmup")
                .Value.Should().BeGreaterThan(0);
            collector.WorkerThreadPoolAdjustmentReasonCount.WithLabels("Climbing move")
                .Value.Should().BeGreaterThan(0);
        }

        private async Task AssertWorkerThreadsAreCreated(ThreadPoolStatsCollector collector)
        {
            collector.WorkerActiveThreadCount.Value.Should().BeGreaterThan(0);
            await DelayHelper.DelayAsync(() => collector.WorkerActiveThreadCount.Value <= 0);
        }

        private async Task SpamCpuAndIoBoundTasksToThreadPool()
        {
            const int workerTaskCount = 1000;
            const int ioTaskCount = 50;

            for (var i = 0; i < workerTaskCount; i++)
            {
                await Task.Run(() =>
                {
                    Thread.Sleep(1);
                });
            }

            using var client = new HttpClient();
            var httpTasks = Enumerable.Range(1, ioTaskCount)
                .Select(_ => client.GetAsync("http://google.com"));

            await Task.WhenAll(httpTasks);
        }

        private ThreadPoolStatsCollector CreateStatsCollector()
        {
            return new ThreadPoolStatsCollector(
                new MetricFactory(new CollectorRegistry()),
                e => throw e);
        }
    }
}