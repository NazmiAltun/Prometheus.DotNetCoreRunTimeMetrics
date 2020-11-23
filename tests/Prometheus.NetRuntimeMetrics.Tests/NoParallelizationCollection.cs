using Xunit;

namespace Prometheus.NetRuntimeMetrics.Tests
{
    //TODO: Try to get rid of this
    [CollectionDefinition("NoParallelization", DisableParallelization = true)]
    public class NoParallelizationCollection
    {
    }
}
