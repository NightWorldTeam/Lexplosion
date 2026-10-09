using System;
using System.Diagnostics;
using System.IO;

namespace Lexplosion.UI.WPF.Services.Descriptions
{
	/// <summary>Small, thread-safe diagnostics for the instance description pipeline.</summary>
	internal static class OverviewDiagnostics
	{
		private static readonly object Sync = new object();

		// The log intentionally stores no HTML, links, credentials or description text.
		public static string FilePath => Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Lexplosion", "Logs", "instance-overview.log");

		public static void Info(string message) => Write("INFO", message);
		public static void Warn(string message) => Write("WARN", message);
		public static void Error(string stage, Exception error) => Write("ERROR", stage + ": " + error);

		private static void Write(string level, string message)
		{
			string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +
				" [Overview/" + level + "] " + message;
			Debug.WriteLine(line);
			try
			{
				lock (Sync)
				{
					Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
					// Trim on demand to keep the diagnostic file bounded (~1 MiB).
					if (File.Exists(FilePath) && new FileInfo(FilePath).Length > 1024 * 1024)
					{
						var old = File.ReadAllLines(FilePath);
						int keep = Math.Min(900, old.Length);
						File.WriteAllLines(FilePath, new ArraySegment<string>(old, old.Length - keep, keep));
					}
					File.AppendAllText(FilePath, line + Environment.NewLine);
				}
			}
			catch (Exception logException)
			{
				Debug.WriteLine("Overview log write failed: " + logException.Message);
			}
		}
	}
}
