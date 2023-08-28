using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Models.InstanceCatalogControllers
{
    public sealed class LibraryController : IInstanceCatalog
    {
        public static LibraryController Instance { get; } = new LibraryController();


        private ObservableCollection<InstanceModelBase> _instances = new ObservableCollection<InstanceModelBase>();
        public IEnumerable<InstanceModelBase> Instances { get => _instances; }


        #region Constructors


        private LibraryController()
        {
            foreach (var instanceClient in InstanceClient.GetInstalledInstances())
            {
                _instances.Add(new InstanceModelBase(instanceClient));
            }
        }


        #endregion Constructors


        public void Add(InstanceModelBase instanceModelBase)
        {
            _instances.Add(instanceModelBase);
        }

        public InstanceModelBase GetInstance(InstanceClient instanceClient)
        {
            return null;
        }
    }
}
