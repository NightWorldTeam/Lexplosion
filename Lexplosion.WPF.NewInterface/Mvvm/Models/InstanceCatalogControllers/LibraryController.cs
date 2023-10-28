using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceCatalogControllers
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
                Runtime.DebugWrite("Library Item " + instanceClient.Name);
                _instances.Add(new InstanceModelBase(instanceClient));
            }
        }


        #endregion Constructors


        public void Add(InstanceModelBase instanceModelBase)
        {
            _instances.Add(instanceModelBase);
        }

        /// <summary>
        /// Удаляет сборку из библиотеки.
        /// </summary>
        public void Remove(InstanceModelBase instanceModelBase)
        {
            _instances.Remove(instanceModelBase);
        }

        public InstanceModelBase GetInstance(InstanceClient instanceClient)
        {
            return null;
        }
    }
}
