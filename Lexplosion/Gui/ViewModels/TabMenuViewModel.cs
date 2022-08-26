using Lexplosion.Gui.Views.CustomControls;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels
{
    public class TabMenuViewModel : SubmenuViewModel
    {
        public bool IsInstance { get; private set; } = true;

        private string _header;
        /// <summary>
        /// Заголовок страницы.
        /// </summary>
        public string Header 
        {
            get => _header; set 
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }

        public InstanceFormViewModel InstanceFormVM 
        {
            get;
        }

        public InstanceClient InstanceClient { get; private set; }

        public TabMenuViewModel(IList<Tab> tabs, string header, InstanceClient instanceClient = null, InstanceFormViewModel instanceFormViewModel = null)
        {
            if (instanceClient == null)
                IsInstance = false;

            if (instanceFormViewModel != null)
                InstanceFormVM = instanceFormViewModel;

            InstanceClient = instanceClient;

            Header = header;
            Tabs = new ObservableCollection<Tab>(tabs);
            SelectedTab = Tabs[0];
        }
    }
}
