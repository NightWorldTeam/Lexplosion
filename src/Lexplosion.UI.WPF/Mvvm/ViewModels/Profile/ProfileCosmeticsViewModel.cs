using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Collections.ObjectModel;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Profile
{
	public class CosmeticItem
	{
		public event Action<CosmeticItem, bool> SelectedChanged;

		public string CoverUrl { get; } = "https://night-world.org/assets/cosmetic/103.png";

		private bool _isSelected;
		public bool IsSelected
		{
			get => _isSelected; set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					SelectedChanged?.Invoke(this, value);
				}
			}
		}

		public string GroupNameKey { get; }

        public CosmeticItem(int i)
        {
			GroupNameKey = i % 2 == 0 ? "Head" : "Body";
		}
    }

	public class ProfileCosmeticModel : ObservableObject
	{
		public ObservableCollection<CosmeticItem> Cosmetics { get; } = new();

		private object _selectedItemCategory;
		public object SelectedItemCategory
		{
			get => _selectedItemCategory; set 
			{
				_selectedItemCategory = value;
				OnPropertyChanged();
			}
		}

		public ProfileCosmeticModel()
		{
			for (var i = 0; i < 20; i++)
				Cosmetics.Add(new(i));
		}
	}

	public sealed class ProfileCosmeticsViewModel : ViewModelBase
	{
		public ProfileCosmeticModel Model { get; }

		public ProfileCosmeticsViewModel()
		{
			Model = new();
		}
	}
}
