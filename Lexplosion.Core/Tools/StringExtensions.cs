namespace System
{
	static class StringExtensions
	{
		public static int ToInt32(this string str)
		{
			Int32.TryParse(str, out int result);
			return result;
		}

		public static string ReplaceFirst(this string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}

			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public static string ReplaceLast(this string value, string search, string replace)
		{
			int place = value.LastIndexOf(search);

			if (place == -1)
				return value;

			return value.Remove(place, search.Length).Insert(place, replace);
		}

		public static string Truncate(this string value, int maxLength, string truncationSuffix = "…")
		{
			return value.Length > maxLength ? value.Substring(0, maxLength) + truncationSuffix : value;
		}

		public static string TruncateWithoutSuffix(this string value, int maxLength)
		{
			return value.Length > maxLength ? value.Substring(0, maxLength) : value;
		}

		public static string RemoveLastChars(this string value, int count)
		{
			if (value.Length < 1) return value;
			return value.Remove(value.Length - count);
		}
	}
}
