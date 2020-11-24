using System;
using System.Threading;

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
    }
}
