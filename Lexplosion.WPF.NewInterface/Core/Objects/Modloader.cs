using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class Modloader : ObservableObject
    {
        public event Action<Modloader, bool> SelectedChanged;

        #region Properties


        public string Name { get; }

        public Lexplosion.Modloader EnumValue { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                SelectedChanged?.Invoke(this, _isSelected);
                OnPropertyChanged();
            }
        }


        #endregion Properties


        public Modloader(string name, Lexplosion.Modloader enumValue)
        {
            Name = name;
            EnumValue = enumValue;
        }
    }
}
