using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class CatalogViewModel : ViewModelBase
    {
        private ObservableCollection<InstanceModelBase> _instances = new ObservableCollection<InstanceModelBase>();
        public IEnumerable<InstanceModelBase> Instances { get => _instances; }


        #region Commands





        #endregion Commands


        #region Constructors


        public CatalogViewModel()
        {
            var test = InstanceClient.GetInstance(InstanceSource.Curseforge, "829758");

            var categories = new ObservableCollection<IProjectCategory>(
                CategoriesManager.GetModpackCategories(EnumManager.InstanceSourceToProjectSource(InstanceSource.Curseforge))
            );

            //var test1 = InstanceClient.GetOutsideInstances(InstanceSource.Nightworld, 10, 0, categories[0]);

            //for (var i = 0; i < test1.Count; i++) 
            //{ 
            //    _instances.Add(new InstanceModelBase(test1[i]));
            //}
        }


        #endregion Consturctors
    }
}
