using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Core
{
    public abstract class ContentLayoutViewModelBase : ViewModelBase
    {
        private string _headerKey;
        public string HeaderKey 
        {
            get => _headerKey;
            set => RaiseAndSetIfChanged(ref _headerKey, value);
        }

        protected ObservableCollection<TabItemModel> _tabs = new ObservableCollection<TabItemModel>();
        public IEnumerable<TabItemModel> Tabs { get => _tabs; }
    }
}
