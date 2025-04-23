using System.IO;
using System.Text.RegularExpressions;

namespace Lexplosion.Core.Tools
{
	public static class JavaHelper
	{
		public enum JavaPathCheckResult
		{
			Success,
			EmptyOrNull,
			JaveExeDoesNotExists,
			WrongExe,
			PathDoesNotExists
		}

		/// <summary>
		/// Проверяет путь до джавы. Если путь корректен приводит его к нужному виду.
		/// Если он некорректен, возращает тип ошибки.
		/// </summary>
		/// <param name="path">Начальный путь</param>
		/// <param name="correctPath">Корректный путь</param>
		/// <returns>Результат проверки.</returns>
		public static JavaPathCheckResult TryValidateJavaPath(string path, out string correctPath)
		{
			correctPath = string.Empty;

			// Если путь пустой
			if (string.IsNullOrWhiteSpace(path))
				return JavaPathCheckResult.EmptyOrNull;

			// заменяем два обратных слеша на один
			correctPath = path.Replace('\\', '/');
			// сокращаем n-ное количество слешей до 1
			correctPath = Regex.Replace(correctPath, @"\/+", "/").Trim();
			// убираем слеш в конце
			correctPath = correctPath.TrimEnd('/');

			if (!Directory.Exists(correctPath) && !correctPath.EndsWith(".exe"))
				return JavaPathCheckResult.PathDoesNotExists;

			// делим путь на части
			var pathParts = correctPath.Split('/');
			var endIndex = pathParts.Length - 1;

			// если конец пути не в папке bin
			if (pathParts[endIndex - 1] == "bin")
			{
				// проверяем оканчивается ли путь .exe, если нет возвращаем отсутствие java .exe
				if (!pathParts[endIndex].Contains(".exe"))
					return JavaPathCheckResult.JaveExeDoesNotExists;

				return IsJavaPathFileHasProblem(correctPath, pathParts[endIndex]);
			}
			else
			{
				if (correctPath.EndsWith(".exe"))
					return IsJavaPathFileHasProblem(correctPath, pathParts[endIndex]);

				if (!Directory.Exists(correctPath))
					return JavaPathCheckResult.PathDoesNotExists;

				if (pathParts[endIndex] == "bin")
				{
					correctPath += "/javaw.exe";
					if (!File.Exists(correctPath))
						return JavaPathCheckResult.JaveExeDoesNotExists;
				}
				else
				{
					correctPath += "/bin/javaw.exe";
					if (!File.Exists(correctPath))
						return JavaPathCheckResult.JaveExeDoesNotExists;
				}
			}

			return JavaPathCheckResult.Success;
		}

		private static JavaPathCheckResult IsJavaPathFileHasProblem(string path, string fileName)
		{
			// проверяем оканчивается ли путь нужными .exe
			if (fileName == "javaw.exe" || fileName == "java.exe")
			{
				// если нужного .exe не существует
				if (!File.Exists(path))
					return JavaPathCheckResult.JaveExeDoesNotExists;

				return JavaPathCheckResult.Success;
			}
			return JavaPathCheckResult.WrongExe;
		}
	}

}
