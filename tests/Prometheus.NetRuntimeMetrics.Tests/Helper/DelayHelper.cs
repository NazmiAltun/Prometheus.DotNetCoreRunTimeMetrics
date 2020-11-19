using System;
using System.Threading.Tasks;

namespace Prometheus.NetRuntimeMetrics.Tests.Helper
{
    internal static class DelayHelper
    {
        private const int DelayMs = 10;
        private const int TryCount = 5000;

        public static async Task DelayAsync(Func<bool> conditionFunc)
        {
            for (var i = 0; i < TryCount && conditionFunc(); i++)
            {
                await Task.Delay(DelayMs);
            }
        }
    }
}
