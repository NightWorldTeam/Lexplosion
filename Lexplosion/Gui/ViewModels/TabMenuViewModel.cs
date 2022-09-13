using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class TabMenuViewModel : SubmenuViewModel
    {
        private bool _isInstance;
        public bool IsInstance 
        {   
            get => _isInstance; private set 
            {
                _isInstance = value;
                OnPropertyChanged();
            }
        }

        private string _header;
        /// <summary>
        /// Заголовок страницы.
        /// </summary>
        public string Header 
        {
            get => _header; private set 
            {
                _header = value;
                OnPropertyChanged();
            }
        }

        public InstanceFormViewModel InstanceFormVM { get; }

        public InstanceClient InstanceClient { get; private set; }

        public TabMenuViewModel(IList<Tab> tabs, string header, int selectedTabIndex = 0, InstanceFormViewModel instanceFormViewModel = null)
        {
            if (instanceFormViewModel != null) 
            { 
                InstanceFormVM = instanceFormViewModel;
                InstanceClient = instanceFormViewModel.Client;
                IsInstance = true;
            }
            else IsInstance = false;

            Header = header;
            Tabs = new ObservableCollection<Tab>(tabs);
            if (tabs.Count > 0)
                SelectedTab = Tabs[selectedTabIndex];
        }
    }
}
