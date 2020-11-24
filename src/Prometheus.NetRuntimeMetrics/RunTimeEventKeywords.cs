using System;

namespace Prometheus.NetRuntimeMetrics
{
    //Taken from : https://docs.microsoft.com/en-us/dotnet/framework/performance/clr-etw-keywords-and-levels
    [Flags]
    internal enum RunTimeEventKeywords : long
    {
        GC = 0x00000001,
        Jit = 0x00000010,
        Contention = 0x4000,
        Exception = 0x00008000,
        Threading = 0x00010000,
    }
}
