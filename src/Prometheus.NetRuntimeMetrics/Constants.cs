using System;

namespace Prometheus.NetRuntimeMetrics
{
    internal static class Constants
    {
        public static readonly Guid RuntimeEventSourceId = Guid.Parse("5e5bb766-bbfc-5662-0548-1d44fad9bb56");
        public static readonly Guid FrameworkEventSourceId = Guid.Parse("8e9f5090-2d75-4d03-8a81-e5afbf85daf1");
    }
}