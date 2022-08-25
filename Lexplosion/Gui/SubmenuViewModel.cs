using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui
{
    public abstract class SubmenuViewModel : VMBase
    {
        public ObservableCollection<Tab> Tabs { get; protected set; } = new ObservableCollection<Tab>();
        public Tab SelectedTab { get; set; } = new Tab();
    }
}
