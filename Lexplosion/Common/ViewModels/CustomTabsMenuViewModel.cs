using Lexplosion.Common.ModalWindow;
using System.Collections.Generic;

namespace Lexplosion.Common.ViewModels
{
    public class CustomTab : VMBase
    {
        private readonly CustomTabsMenuViewModel _menu;

        public static ushort TabsCount { get; private set; }
        public ushort Id { get; }
        public string Text { get; }
        public string Icon { get; }
        public VMBase Page { get; }
        public bool IsEnable { get; }

        public CustomTab(string text, string icon, VMBase page, bool isEnable = true)
        {
            Text = text;
            Icon = icon;
            Page = page;
            IsEnable = isEnable;
        }

        public CustomTab(CustomTab tab, CustomTabsMenuViewModel customTabsMenu)
        {
            _menu = customTabsMenu;
            Id = TabsCount;
            TabsCount++;
            Text = tab.Text;
            Icon = tab.Icon;
            Page = tab.Page;
            IsEnable = tab.IsEnable;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                if (value) _menu.CurrentPage = Page;
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

        private VMBase _currentPage;
        public VMBase CurrentPage
        {
            get => _currentPage; set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public CustomTabsMenuViewModel(List<CustomTab> tabs)
        {
            foreach (var tab in tabs)
            {
                Tabs.Add(new CustomTab(tab, this));
            }
            Tabs[0].IsSelected = true;
        }
    }
}
