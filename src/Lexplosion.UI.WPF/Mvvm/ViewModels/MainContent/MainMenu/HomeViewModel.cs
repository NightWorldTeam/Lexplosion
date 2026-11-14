using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;

/// <summary>
/// View model for maximized Home. The list and filters share CatalogViewModel,
/// while popular recommendations use a separate in-memory cache.
/// </summary>
public sealed class HomeViewModel : ViewModelBase
{
	private readonly IInstanceController _featuredController;
	private bool _featuredLoaded;
	private bool _isFeaturedLoading;
	private IReadOnlyList<InstanceModelBase> _displayedCatalogInstances = Array.Empty<InstanceModelBase>();

	#region Properties


	public CatalogViewModel Catalog { get; }
	public LibraryViewModel Library { get; }

	public IEnumerable<InstanceModelBase> FeaturedInstances => _featuredController.Instances;

	// A stable snapshot for Home only. CatalogModel clears and adds to its live
	// collection as pages arrive; rendering that live collection causes jumps.
	public IReadOnlyList<InstanceModelBase> DisplayedCatalogInstances
	{
		get => _displayedCatalogInstances;
		private set
		{
			_displayedCatalogInstances = value;
			OnPropertyChanged();
		}
	}

	public bool IsFeaturedLoading
	{
		get => _isFeaturedLoading;
		private set
		{
			if (_isFeaturedLoading == value)
				return;
			_isFeaturedLoading = value;
			OnPropertyChanged();
		}
	}

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


	public HomeViewModel(CatalogViewModel catalog, LibraryViewModel library, IInstanceController featuredController)
	{
		Catalog = catalog;
		Library = library;
		_featuredController = featuredController;

		_displayedCatalogInstances = Catalog.Model.Instances.ToArray();
		Catalog.Model.PropertyChanged += CatalogModel_PropertyChanged;
	}

	private void CatalogModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		// CatalogModel raises this once after its controller has received every
		// item. Keep the previous Home page untouched until that point.
		if (e.PropertyName != nameof(Catalog.Model.Instances))
			return;

		App.Current.Dispatcher.Invoke(() =>
		{
			DisplayedCatalogInstances = Catalog.Model.Instances.ToArray();
		});
	}

	public void EnsureFeaturedLoaded()
	{
		if (_featuredLoaded || IsFeaturedLoading)
			return;

		IsFeaturedLoading = true;
		Runtime.TaskRun(() =>
		{
			try
			{
				// Independent from the catalog's search, filters, source and page.
				// Fetch the four most popular CurseForge modpacks once per Home VM.
				var search = new CurseforgeSearchParams(
					string.Empty, string.Empty, DefaultSearchParams.EmptyCategoriesList,
					4, 0, CfSortField.Popularity);
				var result = Runtime.ClientsManager.GetOutsideInstances(InstanceSource.Curseforge, search);

				App.Current.Dispatcher.Invoke(() =>
				{
					_featuredController.Clear();
					foreach (var instance in result)
						_featuredController.Add(instance);

					_featuredLoaded = true;
					OnPropertyChanged(nameof(FeaturedInstances));
					IsFeaturedLoading = false;
				});
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite($"Failed to load Home recommendations: {ex}");
				App.Current.Dispatcher.BeginInvoke(new Action(() => IsFeaturedLoading = false));
			}
		});
	}
}
