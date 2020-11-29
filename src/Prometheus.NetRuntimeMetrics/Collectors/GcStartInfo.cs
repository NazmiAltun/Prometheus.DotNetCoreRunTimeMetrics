using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Prometheus.NetRuntimeMetrics.Collectors
{
    internal readonly struct GcStartInfo
    {
        private const string GenerationFieldName = "Depth";
        private const string ReasonFieldName = "Reason";
        private const string TypeFieldName = "Type";

        private static readonly Dictionary<uint, string> GcReasonMapping = new Dictionary<uint, string>
        {
            {0x0, "Small object heap allocation" },
            {0x1, "Induced" },
            {0x2, "Low Memory" },
            {0x3, "Empty" },
            {0x4, "Large object heap allocation" },
            {0x5, "Out of space (for small object heap)" },
            {0x6, "Out of space (for large object heap)" },
            {0x7, "Induced but not forced as blocking" },
        };

        private static readonly Dictionary<uint, string> GcTypeMapping = new Dictionary<uint, string>
        {
            {0x0 ,"Blocking Foreground" },
            {0x1 ,"NonBlocking Background" },
            {0x2 ,"Blocking Background" }
        };

        public static GcStartInfo FromEventWrittenEventArgs(EventWrittenEventArgs e)
        {
            return new GcStartInfo(
                e.GetVal<uint>(GenerationFieldName),
                e.GetVal<uint>(ReasonFieldName),
                e.GetVal<uint>(TypeFieldName));
        }

        private readonly uint _type;
        private readonly uint _generation;
        private readonly uint _reason;

        private GcStartInfo(uint generation, uint reason, uint type)
        {
            _type = type;
            _generation = generation;
            _reason = reason;
        }

        public string[] ToLabels()
        {
            return new[]
            {
                _generation > 2 ? "LOH" : _generation.ToString(),
                GcReasonMapping.TryGetValue(_reason , out var reasonStr) ? reasonStr : $"unknown-{_reason}",
                GcTypeMapping[_type]
            };
        }
    }
}