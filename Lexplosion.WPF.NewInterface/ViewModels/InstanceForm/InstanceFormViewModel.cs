using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.ViewModels.InstanceForm
{
    public sealed class InstanceFormViewModel : ViewModelBase
    {
        public object Model { get; }


        private ObservableCollection<object> _lowerButtons;
        public IEnumerable<object> LowerButtons { get => _lowerButtons; }


    }
}
