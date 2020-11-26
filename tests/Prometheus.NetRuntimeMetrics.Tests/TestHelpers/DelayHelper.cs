using System;
using System.Threading;
using System.Threading.Tasks;

namespace Prometheus.NetRuntimeMetrics.Tests.TestHelpers
{
    internal static class DelayHelper
    {
        private const int DelayMs = 10;
        private const int TryCount = 500;

        public static void Delay(Func<bool> conditionFunc)
        {
            for (var i = 0; i < TryCount && conditionFunc(); i++)
            {
                Thread.Sleep(DelayMs);
            }
        }

        public static async Task DelayAsync(Func<bool> conditionFunc)
        {
            for (var i = 0; i < TryCount && conditionFunc(); i++)
            {
                await Task.Delay(DelayMs);
            }
        }
    }
}
