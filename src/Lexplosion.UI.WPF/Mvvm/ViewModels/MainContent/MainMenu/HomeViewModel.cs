using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;

/// <summary>
/// View model for the maximized Catalog home page.
/// The actual catalog instances and filter state are reused from CatalogViewModel;
/// nothing is duplicated into a second data source.
/// </summary>
public sealed class HomeViewModel : ViewModelBase
{
	#region Properties


	public CatalogViewModel Catalog { get; }
	public LibraryViewModel Library { get; }

	private bool _isFilterPanelVisible = true;
	public bool IsFilterPanelVisible
	{
		get => _isFilterPanelVisible;
		set
		{
			if (_isFilterPanelVisible == value)
				return;

			_isFilterPanelVisible = value;
			OnPropertyChanged();
		}
	}


	#endregion Properties


	#region Commands


	private RelayCommand? _toggleFilterPanelCommand;
	public ICommand ToggleFilterPanelCommand
	{
		get => RelayCommand.GetCommand(ref _toggleFilterPanelCommand, () =>
		{
			IsFilterPanelVisible = !IsFilterPanelVisible;
		});
	}


	private RelayCommand? _updateInstanceCommand;
	public ICommand UpdateInstanceCommand
	{
		get => RelayCommand.GetCommand<InstanceModelBase>(ref _updateInstanceCommand, instance =>
		{
			if (instance == null)
				return;

			instance.Update();
		});
	}

	private RelayCommand? _refreshCatalogCommand;
	public ICommand RefreshCatalogCommand
	{
		get => RelayCommand.GetCommand(ref _refreshCatalogCommand, () =>
		{
			Catalog.SearchCommand.Execute(Catalog.Model.SearchFilter ?? string.Empty);
		});
	}


	#endregion Commands


	public HomeViewModel(CatalogViewModel catalog, LibraryViewModel library)
	{
		Catalog = catalog;
		Library = library;
	}
}
