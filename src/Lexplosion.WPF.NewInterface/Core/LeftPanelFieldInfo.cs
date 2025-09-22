using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;

namespace Lexplosion.WPF.NewInterface.Core
{
    public class LeftPanelFieldInfo : ObservableObject
    {
        public string Name { get; }
        public string Value { get; private set; }
        public bool IsLoading { get; private set; }

        public LeftPanelFieldInfo(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Загружает value в потоке отличном от UIThread.
        /// Вызывая Func<object> в другом потоке
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public LeftPanelFieldInfo(string name, Func<object> value)
        {
            IsLoading = true;
            Name = name;
            Runtime.TaskRun(() => 
            {
                Value = value?.Invoke().ToString();
                OnPropertyChanged(nameof(Value));
                IsLoading = false;
                OnPropertyChanged(nameof(IsLoading));
            });
        }
    }
}
