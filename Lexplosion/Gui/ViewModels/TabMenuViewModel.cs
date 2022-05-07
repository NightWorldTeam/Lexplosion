using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels
{
    public class TabMenuViewModel : SubmenuViewModel
    {
        private string _header;

        public string Header 
        {
            get => _header; set 
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }

        public TabMenuViewModel(List<Tab> tabs, string header)
        {
            Header = header;
            Tabs = new ObservableCollection<Tab>(tabs);
            Console.WriteLine(tabs[0].Header);
            SelectedTab = Tabs[0];
        }
    }
}
