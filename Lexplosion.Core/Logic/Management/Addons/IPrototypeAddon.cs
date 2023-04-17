using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Addons
{
    interface IPrototypeAddon
    {
        List<AddonDependencie> Dependecies { get; }

        /// <summary>
        /// Устанавливает версию по умолчанию.
        /// Версия по умолчанию может быть как последней, так и конкретной, если она задана в экземпляре.
        /// </summary>
        void DefineDefaultVersion();

        /// <summary>
        /// Устанавливает последнюю версию для аддона.
        /// </summary>
        void DefineLatestVersion();

        SetValues<InstalledAddonInfo, DownloadAddonRes> Install(TaskArgs taskArgs);

        /// <summary>
        /// Сравнивает самую последнюю версию версию файла аддона с переданной.
        /// </summary>
        /// <param name="addonFileId">Айдишник файла аддона</param>
        /// <returns>true - если последняя версия новее преденной.</returns>
        bool CompareVersions(string addonFileId);

        string ProjectId { get; }

        string WebsiteUrl { get; }

        string AuthorName { get; }

        string Description { get; }

        string Name { get; }

        string LogoUrl { get; }
    }

}
