﻿using System;
using System.Linq;
using System.Net.Http;
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
    public class ThreadPoolStatsCollectorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ThreadPoolStatsCollectorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task ThreadPoolStatsShouldBeCollected()
        {
            using var collector = CreateStatsCollector();
            await SpamCpuAndIoBoundTasksToThreadPool();
            AssertWorkerThreadsAreCreated(collector);
           // AssertIoThreadsAreCreated(collector); //TODO:Why this does work on local but not on CI machine?
            collector.ThreadPoolWorkerThreadAdjustmentThroughput.Value.Should().BeGreaterThan(0);
            collector.WorkerThreadPoolAdjustmentReasonCount.WithLabels("Warmup")
                .Value.Should().BeGreaterThan(0);
            collector.WorkerThreadPoolAdjustmentReasonCount.WithLabels("Climbing move")
                .Value.Should().BeGreaterThan(0);
        }

        private void AssertIoThreadsAreCreated(ThreadPoolStatsCollector collector)
        {
            using var assertionManualResetEvent = new AssertionManualResetEvent(() =>
                collector.IoThreadCount.Value > 0);
            assertionManualResetEvent.Wait(TimeSpan.FromSeconds(20));
            collector.IoThreadCount.Value.Should().BeGreaterThan(0);
        }

        private void AssertWorkerThreadsAreCreated(ThreadPoolStatsCollector collector)
        {
            using var assertionManualResetEvent = new AssertionManualResetEvent(() =>
                collector.WorkerActiveThreadCount.Value > 0);
            assertionManualResetEvent.Wait(TimeSpan.FromSeconds(20));
            collector.WorkerActiveThreadCount.Value.Should().BeGreaterThan(0);
        }

        private async Task SpamCpuAndIoBoundTasksToThreadPool()
        {
            const int workerTaskCount = 1000;
            const int ioTaskCount = 1000;

            for (var i = 0; i < workerTaskCount; i++)
            {
                await Task.Run(() =>
                {
                    Thread.Sleep(1);
                });
            }

            using var client = new HttpClient()
            {
                Timeout = TimeSpan.FromMilliseconds(10)
            };
            var httpTasks = Enumerable.Range(1, ioTaskCount)
                .Select(async (_) =>
                {
                    try
                    {
                        await client.GetAsync("http://localhost");
                    }
                    catch(Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                    }
                });

            await Task.WhenAll(httpTasks);
        }

        private ThreadPoolStatsCollector CreateStatsCollector()
        {
            return new ThreadPoolStatsCollector(
                new MetricFactory(new CollectorRegistry()),
                TestCollectorExceptionHandler.Create(_testOutputHelper));
        }
    }
}