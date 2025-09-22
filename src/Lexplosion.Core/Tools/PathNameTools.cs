using System.Collections.Generic;
using System.Text;

namespace Lexplosion.Tools
{
	static class PathNameTools
	{
		private static HashSet<char> InvalidFileNameChars = ['<', '>', '”', '|', '?', ' ', '*', ':'];

		/// <summary>
		/// Заменяет символы '”', '|', '?', ' ', '*', ':' и знаки больше/меньше в пути на '_'. 
		/// НЕ ПРИМЕНЯТЬ БЛЯТЬ К ПОЛНОМУ ПУТИ! ПОЛНЫЙ ПУТЬ В ВИНДЕ СОДЕРЖИТ ':' ДЛЯ ОБОЗНАЧЕНИЯ ДИСКА.
		/// Применять только для непонолого пути, где нет обозначения диска, или для имени файла.
		/// </summary>
		/// <param name="path">Путь для валидации</param>
		/// <returns>Валидированный путь</returns>
		public static string EasyValidation(string path)
		{
			//валидируем имя файла
			var newFileName = new StringBuilder();
			foreach (char chr in path)
			{
				if (!InvalidFileNameChars.Contains(chr)) newFileName.Append(chr);
				else newFileName.Append('_');
			}

			return newFileName.ToString();
		}
	}
}
