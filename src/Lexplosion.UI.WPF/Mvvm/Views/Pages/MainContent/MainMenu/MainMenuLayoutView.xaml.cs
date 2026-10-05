using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.MainMenu;

/// <summary>
/// Main menu shell. The left panel is not modified.
/// Catalog switches to Home according to the REAL content-column width.
/// v11 prewarms the Home visual tree while the compact catalog is idle so
/// crossing the responsive breakpoint does not construct the whole page
/// inside a live-resize frame.
/// </summary>
public partial class MainMenuLayoutView : UserControl
{
	private const double HomeEnterContentWidth = 1080.0;
	private const double HomeExitContentWidth = 1020.0;

	private MainMenuLayoutViewModel? _subscribedViewModel;
	private bool _isHomeMode;
	private bool _hasAppliedResponsiveMode;

	// One prepared HomeView is kept for the lifetime of this MainMenuLayoutView.
	// It is Collapsed while the compact catalog is shown, so it does not take
	// part in live resize measure/arrange. Keeping the instance avoids the
	// expensive XAML/image/list construction exactly at the breakpoint.
	private HomeView? _cachedHomeView;
	private MainMenuLayoutViewModel? _cachedHomeOwner;
	private DispatcherOperation? _homePrewarmOperation;

	public MainMenuLayoutView()
	{
		InitializeComponent();
	}

	private void MainMenuLayoutView_Loaded(object sender, RoutedEventArgs e)
	{
		SubscribeToViewModel((MainMenuLayoutViewModel)DataContext);
		UpdateResponsiveMode(force: true);
		ScheduleHomePrewarm();

		var da = new DoubleAnimation
		{
			From = 0,
			To = 1,
			Duration = TimeSpan.FromSeconds(0.25)
		};
		BeginAnimation(OpacityProperty, da);
	}

	private void MainMenuLayoutView_Unloaded(object sender, RoutedEventArgs e)
	{
		SubscribeToViewModel(null);
		CancelHomePrewarm();

		if (SectionContentHost != null)
			SectionContentHost.Content = null;
		if (ResponsiveHomeHost != null)
			ResponsiveHomeHost.Content = null;

		_cachedHomeView = null;
		_cachedHomeOwner = null;
	}

	private void MainMenuLayoutView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		SubscribeToViewModel(e.NewValue as MainMenuLayoutViewModel);

		if (!ReferenceEquals(_cachedHomeOwner, e.NewValue))
		{
			CancelHomePrewarm();

			if (ResponsiveHomeHost != null)
			{
				ResponsiveHomeHost.Content = null;
			}

			_cachedHomeView = null;
			_cachedHomeOwner = null;
		}

		_hasAppliedResponsiveMode = false;
		_isHomeMode = false;
		UpdateResponsiveMode(force: true);
		ScheduleHomePrewarm();
	}

	private void MainMenuLayoutView_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (IsLoaded) UpdateResponsiveMode(force: false);
	}

	private void SubscribeToViewModel(MainMenuLayoutViewModel? viewModel)
	{
		if (ReferenceEquals(_subscribedViewModel, viewModel))
			return;

		if (_subscribedViewModel != null)
		{
			_subscribedViewModel.PropertyChanged -= ViewModel_PropertyChanged;
		}

		_subscribedViewModel = viewModel;

		if (_subscribedViewModel != null)
		{
			_subscribedViewModel.PropertyChanged += ViewModel_PropertyChanged;
		}
	}

	private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(MainMenuLayoutViewModel.IsHomeSelected) || e.PropertyName == nameof(MainMenuLayoutViewModel.Content))
		{
			UpdateResponsiveMode(force: true);

			if (_subscribedViewModel != null && _subscribedViewModel.IsHomeSelected)
			{
				ScheduleHomePrewarm();
			}
		}
	}

	/// <summary>
	/// Prewarm is deliberately dispatched at ApplicationIdle. The cost is paid once
	/// after the compact page is already on screen, not while the user is dragging
	/// the window through the Home breakpoint.
	/// </summary>
	private void ScheduleHomePrewarm()
	{
		if (!IsLoaded || _isHomeMode || _homePrewarmOperation != null)
			return;

		var viewModel = DataContext as MainMenuLayoutViewModel;
		if (viewModel == null || !viewModel.IsHomeSelected)
			return;

		_homePrewarmOperation = Dispatcher.BeginInvoke(new Action(() =>
		{
			_homePrewarmOperation = null;
			PrewarmHomeView();
		}), DispatcherPriority.ApplicationIdle);
	}

	private void CancelHomePrewarm()
	{
		if (_homePrewarmOperation == null)
			return;

		if (_homePrewarmOperation.Status == DispatcherOperationStatus.Pending)
		{
			_homePrewarmOperation.Abort();
		}

		_homePrewarmOperation = null;
	}

	private void PrewarmHomeView()
	{
		if (!IsLoaded || _isHomeMode)
			return;

		var viewModel = DataContext as MainMenuLayoutViewModel;
		if (viewModel == null || !viewModel.IsHomeSelected)
		{
			return;
		}

		var home = EnsureCachedHomeView(viewModel);
		if (home == null)
		{
			return;
		}

		// Populate the four feature/recommendation bindings while hidden.
		home.PrewarmForResponsiveSwitch();

		// Run the expensive first measure/arrange once at idle. Because the view is
		// not yet visible this does not disturb the compact catalog resize path.
		var contentWidth = Math.Max(HomeEnterContentWidth + 40.0, Math.Max(0.0, ActualWidth - (LeftPanelColumn?.ActualWidth ?? 0.0)));
		var contentHeight = Math.Max(720.0, ActualHeight);
		var prewarmSize = new Size(contentWidth, contentHeight);

		home.Measure(prewarmSize);
		home.Arrange(new Rect(new Point(0, 0), prewarmSize));
		home.UpdateLayout();

		// Connect the already-built tree to the collapsed host. Visibility remains
		// Collapsed, so subsequent compact-mode resizes do not measure it.
		if (!ReferenceEquals(ResponsiveHomeHost.Content, home))
		{
			ResponsiveHomeHost.Content = home;
		}
	}

	private HomeView? EnsureCachedHomeView(MainMenuLayoutViewModel viewModel)
	{
		if (viewModel == null)
		{
			return null;
		}

		if (_cachedHomeView != null && ReferenceEquals(_cachedHomeOwner, viewModel))
		{
			return _cachedHomeView;
		}

		if (ResponsiveHomeHost != null)
		{
			ResponsiveHomeHost.Content = null;
		}

		_cachedHomeOwner = viewModel;
		_cachedHomeView = new HomeView
		{
			DataContext = viewModel.Home
		};

		return _cachedHomeView;
	}

	private void UpdateResponsiveMode(bool force)
	{
		if (!IsLoaded || SectionContentHost == null || ResponsiveHomeHost == null || LeftPanelColumn == null)
			return;

		var viewModel = DataContext as MainMenuLayoutViewModel;
		if (viewModel == null)
		{
			return;
		}

		var contentWidth = Math.Max(0.0, ActualWidth - LeftPanelColumn.ActualWidth);

		bool useHome;
		if (!viewModel.IsHomeSelected)
		{
			useHome = false;
		}
		else if (_isHomeMode)
		{
			useHome = contentWidth >= HomeExitContentWidth;
		}
		else
		{
			useHome = contentWidth >= HomeEnterContentWidth;
		}

		if (!force && _hasAppliedResponsiveMode && useHome == _isHomeMode)
		{
			return;
		}

		_hasAppliedResponsiveMode = true;
		_isHomeMode = useHome;

		if (useHome)
		{
			CancelHomePrewarm();

			var home = EnsureCachedHomeView(viewModel);
			if (home != null)
			{
				// If the user resized immediately after launch and idle prewarm has not
				// happened yet, at least populate the small top feature set before show.
				home.PrewarmForResponsiveSwitch();
				if (!ReferenceEquals(ResponsiveHomeHost.Content, home))
				{
					ResponsiveHomeHost.Content = home;
				}
			}

			// Important v11 change: do not destroy CatalogView at the breakpoint.
			// Destruction/unsubscription/GC work was part of the visible hitch. A
			// Collapsed tree does not participate in layout, so keeping it cached is
			// cheap during wide-mode resize and makes the reverse switch instant too.
			SectionContentHost.Visibility = Visibility.Collapsed;
			ResponsiveHomeHost.Visibility = Visibility.Visible;
		}
		else
		{
			ResponsiveHomeHost.Visibility = Visibility.Collapsed;

			if (!ReferenceEquals(SectionContentHost.Content, viewModel.Content))
			{
				SectionContentHost.Content = viewModel.Content;
			}
			SectionContentHost.Visibility = Visibility.Visible;

			if (viewModel.IsHomeSelected)
			{
				ScheduleHomePrewarm();
			}
		}
	}

	private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
	}
}
