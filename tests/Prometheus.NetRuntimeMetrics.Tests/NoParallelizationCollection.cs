using Xunit;

namespace Prometheus.NetRuntimeMetrics.Tests
{
    [CollectionDefinition("NoParallelization", DisableParallelization = true)]
    public class NoParallelizationCollection
    {
    }
}
