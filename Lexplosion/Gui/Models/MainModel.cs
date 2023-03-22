using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management.Instances;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.Models
{
    public sealed class MainModel : VMBase
    {
        public ObservableCollection<InstanceFormViewModel> LibraryInstances { get; } = new ObservableCollection<InstanceFormViewModel>();

        public ObservableCollection<InstanceFormViewModel> CurrentInstanceCatalog { get; } = new ObservableCollection<InstanceFormViewModel>();

        /// <summary>
        /// Проверяет наличие сборки в библиотеке.
        /// </summary>
        /// <param name="instanceClient">Клиент по которому осуществляется проверка</param>
        /// <param name="instanceFormViewModel">Если сборка есть в библиотеке возвращает её InstanceFormViewModel</param>
        /// <returns></returns>
        public bool IsLibraryContainsInstance(InstanceClient instanceClient, out InstanceFormViewModel instanceFormViewModel)
        {
            foreach (var instance in LibraryInstances)
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

        public bool IsLibraryContainsInstance(InstanceClient instanceClient)
        {
            foreach (var instance in LibraryInstances)
            {
                if (instance.Client == instanceClient)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveInstanceFromLibrary(InstanceClient instanceClient)
        {
            InstanceFormViewModel viewmodel;
            if (IsLibraryContainsInstance(instanceClient, out viewmodel))
            {
                if (viewmodel != null)
                    LibraryInstances.Remove(viewmodel);
            }
        }

        public InstanceFormViewModel GetInstance(InstanceClient instanceClient)
        {
            InstanceFormViewModel viewmodel;
            if (IsLibraryContainsInstance(instanceClient, out viewmodel))
            {
                if (viewmodel != null)
                    return viewmodel;
            }
            return null;
        }

        public bool IsCatalogInstanceContains(InstanceClient instanceClient)
        {
            foreach (var instance in LibraryInstances)
            {
                if (instance.Client == instanceClient)
                    return true;
            }
            return false;
        }

        public InstanceFormViewModel GetCatalogInstance(InstanceClient instanceClient)
        {
            foreach (var instance in CurrentInstanceCatalog)
            {
                if (instance.Client == instanceClient)
                    return instance;
            }
            return null;
        }

        public void ClearCatalogInstanceList()
        {
            CurrentInstanceCatalog.Clear();
        }
    }
}
