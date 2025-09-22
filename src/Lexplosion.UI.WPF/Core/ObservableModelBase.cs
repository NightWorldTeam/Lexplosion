using Lexplosion.UI.WPF.Core.ViewModel;
using System;

namespace Lexplosion.UI.WPF.Core
{
    public abstract class ObservableModelBase : ObservableObject
    {
        public abstract event Action<object> Notify;
    }
}
