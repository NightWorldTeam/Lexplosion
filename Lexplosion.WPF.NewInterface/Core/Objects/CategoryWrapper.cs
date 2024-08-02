using Lexplosion.Logic.Objects;
using System;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public sealed class CategoryWrapper : ViewModelBase
    {
        public event Action<IProjectCategory, bool> SelectedEvent;

        private IProjectCategory _category { get; }
        public string Name { get => _category.Name; }


        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                OnPropertyChanged();
                OnSelectedChanged(value);
            }
        }

        public CategoryWrapper(IProjectCategory category)
        {
            _category = category;
        }

        private void OnSelectedChanged(bool value)
        {
            SelectedEvent?.Invoke(_category, value);
        }

        public override int GetHashCode()
        {
            return _category.GetHashCode();
        }
    }
}
