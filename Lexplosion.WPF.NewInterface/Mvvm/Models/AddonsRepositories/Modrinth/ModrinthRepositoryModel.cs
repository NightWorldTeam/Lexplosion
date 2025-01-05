﻿using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories
{
    public sealed class ModrinthRepositoryModel : AddonsRepositoryModelBase
    {
        public override ReadOnlyCollection<int> PageSizes { get; } = new ReadOnlyCollection<int>(
        [
             6, 10, 16, 20, 50, 100
        ]);

        public ReadOnlyCollection<string> SortByItems { get; } = new ReadOnlyCollection<string>([
            "Relevance", "Donwload count", "Follow count", "Recently published", "Recently updated"
        ]);


        #region Constructors


        public ModrinthRepositoryModel(BaseInstanceData instanceData, AddonType addonType, bool isDefaultSelected)
            : base(ProjectSource.Modrinth, instanceData, addonType, isDefaultSelected)
        {

        }


        #endregion Constructors


        #region Public Methods

        protected override ISearchParams BuildSearchParams()
        {
            return new ModrinthSearchParams(SearchFilter, _instanceData.GameVersion.ToString(),
                SelectedCategories, (int)PageSize, (int)CurrentPageIndex, (ModrinthSortField)SelectedSortByIndex,
                new List<Modloader> { _instanceData.Modloader.ToModloader() });
        }

        protected override List<IProjectCategory> GetCategories()
        {
            return ModrinthApi.GetCategories().ToList<IProjectCategory>();
        }

        public void InstallAddon(InstanceAddon modrinthProjectInfo)
        {

        }

        public void ApplyCategories()
        {
            LoadContent();
        }


        #endregion Public Methods


        #region Private Methods



        #endregion Private Methods
    }
}
