using DiscordRPC.Events;
using Lexplosion.Gui.ModalWindow;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels
{
    public class CustomTab : VMBase
    {
        private readonly CustomTabsMenuViewModel _menu;

        public static ushort TabsCount { get; private set; }
        public ushort Id { get; }
        public string Text { get; }
        public string Icon { get; }
        public VMBase Page { get; }

        public CustomTab(string text, string icon, VMBase page)
        {
            Text = text;
            Icon = icon;
            Page = page;
        }

        public CustomTab(CustomTab tab, CustomTabsMenuViewModel customTabsMenu) 
        {
            _menu = customTabsMenu;
            Id = TabsCount;
            TabsCount++;
            Text = tab.Text;
            Icon = tab.Icon;
            Page = tab.Page;
            IsSelected = Id == 0;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                _menu.CurrentPage = Page;
                OnPropertyChanged();
            }
        }
    }

    public class CustomTabsMenuViewModel : ModalVMBase
    {
        #region ModalProperties

        public override double Width => 620;
        public override double Height => 420;

        #endregion ModalProperties

        public List<CustomTab> Tabs { get; } = new List<CustomTab>();

        public VMBase CurrentPage { get; set; } 

        public CustomTabsMenuViewModel(List<CustomTab> tabs)
        {
            foreach (var tab in tabs) 
            {
                Tabs.Add(new CustomTab(tab, this));
            }
        }
    }
}
