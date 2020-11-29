namespace System.Diagnostics.Tracing
{
    internal static class EventWrittenEventArgsExtensions
    {
        public static T GetVal<T>(this EventWrittenEventArgs e, string fieldName)
        {
            var index = e.PayloadNames.IndexOf(fieldName);

            return (T)e.Payload[index];
        }
    }
}
