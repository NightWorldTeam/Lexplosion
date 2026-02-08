using System.Linq;

namespace Lexplosion.Core.Extensions
{
	public static class StringExtensions
	{
		public static string FirstCharToUpper(this string input) => input switch
		{
			null => input,
			"" => input,
			_ => input[0].ToString().ToUpper() + input.Substring(1)
		};

		public static bool IsHexColor(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return false;

			// Проверяем, что строка начинается с '#'
			if (!input.StartsWith("#"))
				return false;

			// Проверяем длину строки (4 для #RGB, 7 для #RRGGBB)
			if (input.Length != 4 && input.Length != 7)
				return false;

			bool result = false;

			foreach (var ch in input.Skip(1))
			{
				result = (ch >= '0' && ch <= '9') ||
						 (ch >= 'a' && ch <= 'f') ||
						 (ch >= 'A' && ch <= 'F');

				if (!result)
					return false;
			}

			return result;
		}
	}
}
