using System.Collections.Generic;
using System.IO;

namespace Lexplosion.Tools
{
	public class DirectoryHelper
	{
		public static bool IsDirectoryEmpty(string path)
		{
			IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
			using (IEnumerator<string> en = items.GetEnumerator())
			{
				return !en.MoveNext();
			}
		}

		public static bool DirectoryNameIsValid(string dir)
		{
			try
			{
				new DirectoryInfo(dir);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
