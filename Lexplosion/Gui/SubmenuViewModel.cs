using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui
{
    public abstract class SubmenuViewModel : VMBase
    {
        public ObservableCollection<Tab<VMBase>> Tabs { get; protected set; } = new ObservableCollection<Tab<VMBase>>();
        public Tab<VMBase> SelectedTab { get; set; } = new Tab<VMBase>();
    }
}
