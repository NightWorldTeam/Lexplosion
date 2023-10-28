using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class LeftMenuControl : ModalViewModelBase
    {
        private readonly ObservableCollection<ModalLeftMenuTabItem> _tabItems = new ObservableCollection<ModalLeftMenuTabItem>();
        public IEnumerable<ModalLeftMenuTabItem> TabItems { get => _tabItems; }


        public ViewModelBase CurrentContent { get; private set; }


        #region Constructors


        public LeftMenuControl()
        {

        }

        public LeftMenuControl(IEnumerable<ModalLeftMenuTabItem> tabItems)
        {
            foreach (var item in tabItems)
            {
                item.SelectedEvent += OnCurrentContentChanged;
                _tabItems.Add(item);
            }
        }


        #endregion Constructors


        public void AddTabItem(string titleKey, string iconKey, ViewModelBase content, bool isEnable = true)
        {
            _tabItems.Add(new ModalLeftMenuTabItem()
            {
                Id = _tabItems.Count,
                TitleKey = titleKey,
                IconKey = iconKey,
                Content = content,
                IsEnable = isEnable,
            });
        }

        private void OnCurrentContentChanged(ViewModelBase content)
        {
            CurrentContent = content;
        }
    }
}
