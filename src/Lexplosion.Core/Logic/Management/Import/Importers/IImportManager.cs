using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Import.Importers
{
	interface IImportManager
	{
		/// <summary>
		/// Выполняет подготовку к импорту и получает основную информацию о клиенте.
		/// Во время работы этого метода локальный id еще не будет определен.
		/// </summary>
		/// <param name="progressHandler">Обработчик прогресса</param>
		/// <param name="result">Основная информация о клиенте</param>
		/// <returns>Результат</returns>
		public InstanceInit Prepeare(ProgressHandler progressHandler, out PrepeareResult result);

		/// <summary>
		/// Выполняет сам импорт. Локальный id сборки уже должен быть определен 
		/// вызовом метода <see cref="SetInstanceId"/>.
		/// </summary>
		/// <param name="progressHandler">Обработчик прогресса</param>
		/// <param name="errors">Список ошибок,  которые возники при импорте</param>
		/// <returns>Результат</returns>
		public InstanceInit Import(ProgressHandler progressHandler, out IReadOnlyCollection<string> errors);

		/// <summary>
		/// Устанавливает лкоальный id сборки
		/// </summary>
		public void SetInstanceId(string id);
	}

	public struct PrepeareResult
	{
		public string Name;
		public string LogoPath;
		public MinecraftVersion GameVersionInfo;
		public string Author;
		public string Description;
		public string Summary;
	}
}
