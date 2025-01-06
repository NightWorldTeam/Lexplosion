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

        /// <summary>
        /// Добавляет допустимый модлоадер. При скачивании мода идёт поиск самой подходящей версии мода, учитывая модлоадер клиента.
        /// Этот метод добавляет модлоадер, версии мода с которым будут пропускаться.
        /// Сначала проверяются версии мода с модлоадером клиента, если подходящей версии не найдено, то будет идти поиск среду допускаемых молоадеров по порядку добавления.
        /// </summary>
        /// <param name="modloader">Модлоадер для разрешения скачивания.</param>
        void AddAcceptableModloader(Modloader modloader);

        /// <summary>
        /// Аналогично <see cref="AddAcceptableModloader"/>, только удаляет.
        /// </summary>
        void RemoveAcceptableModloader(Modloader modloader);

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

        string WebsiteUrl { get; }

        string AuthorName { get; }

        string Description { get; }

        string Name { get; }

        string LogoUrl { get; }

        event Action OnInfoUpdated;

        string GetFullDescription();
    }

}
