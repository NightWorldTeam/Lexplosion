using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Addons
{
	interface IPrototypeAddon
	{
		List<AddonDependencie> Dependecies { get; }

		void DefineDefaultVersion();

		void DefineLatestVersion();

		void DefineSpecificVersion(object versionInfo);

		/// <summary>
		/// Возвращает список версий мода. Берет только те версии, 
		/// которые соотвествуют либо модлоадеру клиента, либо одному из модлоадеров списка, установленного методом <see cref="SetAcceptableModloaders"/>.
		/// </summary>
		/// <returns>Ключ - имя версии, значение - объект версии</returns>
		IDictionary<string, object> GetAllVersions();

		/// <summary>
		/// Устанавливает список допустимых модлоадеров. При скачивании мода идёт поиск самой подходящей версии мода, учитывая модлоадер клиента.
		/// Этот метод добавляет модлоадеры, и с каждым из этих модлоадеров версии мода будут пропускаться.
		/// Сначала проверяются версии мода с модлоадером клиента, если подходящей версии не найдено, то будет идти поиск среду допускаемых молоадеров.
		/// Этот список не должен содержать модлоадер клиента. Может быть null
		/// </summary>
		/// <param name="modloaders">Модлоадеры для разрешения скачивания.</param>
		void SetAcceptableModloaders(IEnumerable<Modloader> modloaders);

		IEnumerable<CategoryBase> LoadCategories();

		SetValues<InstalledAddonInfo, DownloadAddonRes> Install(TaskArgs taskArgs);

		/// <summary>
		/// Сравнивает самую последнюю версию версию файла аддона с переданной.
		/// </summary>
		/// <param name="addonFileId">Айдишник файла аддона</param>
		/// <param name="actionIfTrue">Метод, который будет вызван если последняя версия новее преденной.</param>
		void CompareVersions(string addonFileId, Action actionIfTrue);

		string ProjectId { get; }
		string FileId { get; }
		ProjectSource Source { get; }

		string AuthorName { get; }

		string Description { get; }

		string Name { get; }

		string LogoUrl { get; }

		event Action OnInfoUpdated;

		string GetFullDescription();

		/// <summary>
		/// Возвращает ссылку на страницу мода. 
		/// Может вернуть ее моментально, а может приостанвоить выполнение делая запрос к серваку
		/// </summary>
		string LoadWebsiteUrl();
	}

}
