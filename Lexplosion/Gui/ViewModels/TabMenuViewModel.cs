using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels
{
    public class TabMenuViewModel : SubmenuViewModel
    {
        public bool IsModpackName { get; private set; } = true;

        private string _header;

        public string Header 
        {
            get => _header; set 
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }

        public InstanceClient InstanceClient { get; private set; }

        public TabMenuViewModel(List<Tab> tabs, string header, InstanceClient instanceClient = null)
        {
            if (instanceClient == null)
                IsModpackName = false;

            InstanceClient = instanceClient;

            Header = header;
            Tabs = new ObservableCollection<Tab>(tabs);
            Console.WriteLine(tabs[0].Header);
            SelectedTab = Tabs[0];
        }
    }
}
