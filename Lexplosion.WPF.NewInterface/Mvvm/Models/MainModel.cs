using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models
{
    public sealed class MainModel : ViewModelBase
    {
        private readonly ObservableCollection<InstanceModelBase> _libraryInstances;
        public IReadOnlyCollection<InstanceModelBase> LibraryInstances { get => _libraryInstances; }

        
        public MainModel()
        {
            _libraryInstances = new ObservableCollection<InstanceModelBase>(
                InstanceClient.GetInstalledInstances().Select(z => CreateNewInstanceModelBase(z)));
        }


        public void AddToLibrary(InstanceModelBase instanceModelBase)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _libraryInstances.Add(instanceModelBase);
            });
        }

        public void AddToLibrary(InstanceClient instanceClient)
        {
            AddToLibrary(CreateNewInstanceModelBase(instanceClient));
        }

        public void RemoveFromLibrary(InstanceModelBase instanceModelBase)
        {
            Runtime.TaskRun(() => {
                // задержка, чтобы успела анимация проиграть, так или иначе сборка удалена, далее только "визуальная часть".
                Thread.Sleep(320);
                App.Current.Dispatcher.Invoke(() => {
                    _libraryInstances.Remove(instanceModelBase);
                });
            });
            
        }

        private InstanceModelBase CreateNewInstanceModelBase(InstanceClient instanceClient) 
        {
            var s = new InstanceModelBase(instanceClient);
            s.DeletedEvent += RemoveFromLibrary;
            return s;
        }
    }
}
