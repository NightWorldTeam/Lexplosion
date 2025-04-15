using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;

namespace Lexplosion.WPF.NewInterface.Core
{
    public abstract class ObservableModelBase : ObservableObject
    {
        public abstract event Action<object> Notify;
    }
}
