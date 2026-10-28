using System;
using System.Globalization;
using System.Text;

namespace Lexplosion.QA
{
    /// <summary>Only developer-defined operation names and timings; never user data.</summary>
    public sealed class PerformanceEvent
    {
        public DateTime TimestampUtc { get; }
        public string Operation { get; }
        public double Milliseconds { get; }
        public int ThreadId { get; }

        public PerformanceEvent(DateTime timestampUtc, string operation, double milliseconds, int threadId)
        {
            TimestampUtc = timestampUtc;
            Operation = Sanitize(operation);
            Milliseconds = milliseconds;
            ThreadId = threadId;
        }

        public string ToLogLine()
        {
            return TimestampUtc.ToString("O", CultureInfo.InvariantCulture) +
                " | " + Operation +
                " | " + Milliseconds.ToString("F1", CultureInfo.InvariantCulture) +
                "ms | thread=" + ThreadId.ToString(CultureInfo.InvariantCulture);
        }

        private static string Sanitize(string operation)
        {
            if (string.IsNullOrEmpty(operation)) return "unknown";
            var text = new StringBuilder();
            foreach (char c in operation)
            {
                if (text.Length == 80) break;
                text.Append(char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-'
                    ? c : '_');
            }
            return text.ToString();
        }
    }
}
