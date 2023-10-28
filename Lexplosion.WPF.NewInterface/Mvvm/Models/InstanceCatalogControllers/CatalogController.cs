using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceCatalogControllers
{
    public sealed class CatalogController : IInstanceCatalog
    {
        public static CatalogController Instance { get; } = new CatalogController();


        private ObservableCollection<InstanceModelBase> _instances = new ObservableCollection<InstanceModelBase>();
        public IEnumerable<InstanceModelBase> Instances { get => _instances; }


        #region Constructors


        private CatalogController()
        {

        }


        #endregion Construcotors


        public void Add(InstanceModelBase instanceModelBase)
        {
            throw new NotImplementedException();
        }

        public InstanceModelBase GetInstance(InstanceClient instanceClient)
        {
            throw new NotImplementedException();
        }
    }
}
