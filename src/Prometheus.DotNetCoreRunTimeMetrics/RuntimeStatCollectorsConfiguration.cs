namespace Prometheus.DotNetCoreRunTimeMetrics
{
    public class RuntimeStatCollectorsConfiguration
    {
        public static readonly RuntimeStatCollectorsConfiguration Default = new RuntimeStatCollectorsConfiguration
        {
            HistogramBuckets = new[] { 0.001, 0.01, 0.05, 0.1, 0.5, 1, 10 },
        };
        public double[] HistogramBuckets { get; set; }
    }
}