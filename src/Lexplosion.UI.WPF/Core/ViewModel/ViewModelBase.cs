﻿using Lexplosion.UI.WPF.Core.ViewModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lexplosion.UI.WPF.Core
{
    public class ViewModelBase : ObservableObject
    {
        protected bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }
    }
}
