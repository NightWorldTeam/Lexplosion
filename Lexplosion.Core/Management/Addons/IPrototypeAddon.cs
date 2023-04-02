using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management.Addons
{
    interface IPrototypeAddon
    {
        List<AddonDependencie> Dependecies { get; }

        void DefineDefaultVersion();

        void DefineLatestVersion();

        ValuePair<InstalledAddonInfo, DownloadAddonRes> Install(TaskArgs taskArgs);

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
