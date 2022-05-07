using System.Collections.ObjectModel;

namespace Lexplosion.Gui
{
    public abstract class SubmenuViewModel : VMBase
    {
        public ObservableCollection<Tab> Tabs { get; protected set; }
        public Tab SelectedTab { get; set; }
    }
}
