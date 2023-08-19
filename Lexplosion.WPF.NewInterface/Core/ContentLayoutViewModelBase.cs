using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Core
{
    public abstract class ContentLayoutViewModelBase : ViewModelBase
    {
        protected ObservableCollection<TabItemModel> _tabs = new ObservableCollection<TabItemModel>();
        public IEnumerable<TabItemModel> Tabs { get => _tabs; }
    }
}
