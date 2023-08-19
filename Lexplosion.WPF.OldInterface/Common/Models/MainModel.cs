using Lexplosion.Common.Models.Objects;
using Lexplosion.Common.ViewModels;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Common.Models
{
    public sealed class LibraryController
    {
        private ObservableCollection<InstanceFormViewModel> _instances { get; } = new ObservableCollection<InstanceFormViewModel>();
        public IEnumerable<InstanceFormViewModel> Instances { get => _instances; }

        public void AddInstance(InstanceFormViewModel instanceFormViewModel)
        {
            _instances.Add(instanceFormViewModel);
        }

        /// <summary>
        /// Проверяет наличие сборки в библиотеке.
        /// </summary>
        /// <param name="instanceClient">Клиент по которому осуществляется проверка</param>
        /// <param name="instanceFormViewModel">Если сборка есть в библиотеке возвращает её InstanceFormViewModel</param>
        /// <returns></returns>
        public bool IsLibraryContainsInstance(InstanceClient instanceClient)
        {
            foreach (var instance in Instances)
            {
                if (instance.Client == instanceClient)
                {
                    return instance.Client == instanceClient;
                }
            }
            return false;
        }

        public bool TryGetInstanceByInstanceClient(InstanceClient instanceClient, out InstanceFormViewModel instanceFormViewModel)
        {
            foreach (var instance in Instances)
            {
                if (instance.Client == instanceClient)
                {
                    instanceFormViewModel = instance;
                    return true;
                }
            }
            instanceFormViewModel = null;
            return false;
        }

        public InstanceFormViewModel GetInstance(InstanceClient instanceClient)
        {
            InstanceFormViewModel result;

            if (TryGetInstanceByInstanceClient(instanceClient, out result))
            {
                return result;
            }
            return null;
        }

        public void RemoveByInstanceClient(InstanceClient instanceClient)
        {
            InstanceFormViewModel viewmodel;
            if (TryGetInstanceByInstanceClient(instanceClient, out viewmodel))
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    _instances.Remove(viewmodel);
                });
            }
        }
    }

    public sealed class CatalogController
    {
        private ObservableCollection<InstanceFormViewModel> _pageInstances = new ObservableCollection<InstanceFormViewModel>();
        public IEnumerable<InstanceFormViewModel> PageInstances { get => _pageInstances; }

        public CatalogController()
        {
        }

        public InstanceFormViewModel GetInstance(InstanceClient instanceClient)
        {
            foreach (var instance in PageInstances)
            {
                if (instance.Client == instanceClient)
                    return instance;
            }
            return null;
        }

        public void Clear()
        {
            _pageInstances.Clear();
        }

        public void AddInstance(InstanceFormViewModel instanceFormViewModel)
        {
            _pageInstances.Add(instanceFormViewModel);
        }
    }

    public sealed class MainModel : VMBase
    {
        private static readonly MainModel _instance = new MainModel();
        public static MainModel Instance => _instance;


        #region some shit


        // TODO: цикличная зависимость убрать нах отсюда.
        private MainViewModel _mainViewModel { get; set; }
        public void SetMainViewModel(MainViewModel mainViewModel)
        {
            if (_mainViewModel == null)
            {
                _mainViewModel = mainViewModel;
            }
        }


        #endregion some shit


        public void AddInstanceForm(InstanceClient instanceClient)
        {
            LibraryController.AddInstance(new InstanceFormViewModel(_mainViewModel, instanceClient));
        }

        public InstanceFormViewModel AddInstanceForm(InstanceClient instanceClient, InstanceDistribution instanceDistribution)
        {
            var instanceFormVM = new InstanceFormViewModel(_mainViewModel, instanceClient, instanceDistribution);
            LibraryController.AddInstance(instanceFormVM);
            return instanceFormVM;
        }


        private InstanceFormViewModel _runningInstance;
        public InstanceFormViewModel RunningInstance
        {
            get => _runningInstance; set
            {
                _runningInstance = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Если запушена сборка true, иначе else.
        /// </summary>
        private static bool _isInstanceRunning = false;
        public bool IsInstanceRunning
        {
            get => _isInstanceRunning; set
            {
                _isInstanceRunning = value;
                OnPropertyChanged();
            }
        }


        public LibraryController LibraryController { get; } = new LibraryController();
        public CatalogController CatalogController { get; } = new CatalogController();

        private MainModel()
        {
        }
    }
}
