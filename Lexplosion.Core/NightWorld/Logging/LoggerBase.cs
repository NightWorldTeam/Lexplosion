using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace NightWorld.Logging
{
	public abstract class LoggerBase
	{
		public abstract void WriteLine(object line, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static string CreateLogString(object line, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			string prefix = $"[{Path.GetFileName(sourceFilePath)}:{memberName}:{sourceLineNumber}]";

			var nowTime = DateTimeOffset.Now;
			string time = $"[{nowTime.Hour}:{nowTime.Minute}:{nowTime.Second}.{nowTime.Millisecond}]";

			return $"{time} {prefix} {line}";
		}
	}
}
