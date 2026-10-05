using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.MainMenu;

public partial class HomeView : UserControl
{
	private readonly DispatcherTimer _sliderTimer;
	private readonly List<InstanceModelBase> _featuredItems = new List<InstanceModelBase>();

	private int _selectedFeatureIndex;
	private DateTime _lastSlideAt = DateTime.MinValue;
	private bool _isFeatureStripHovered;
	private bool _isTransitionRunning;
	private bool _isActive;
	private bool _stickyUpdatePending;
	private INotifyPropertyChanged? _catalogModelNotifier;


	private HomeViewModel ViewModel
	{
		get => (HomeViewModel)DataContext;
	}


	public HomeView()
	{
		InitializeComponent();

		// The slider ticks only at its actual rotation cadence. Catalog refreshes are
		// observed through PropertyChanged, so there is no 1-second polling/allocation loop.
		_sliderTimer = new DispatcherTimer(DispatcherPriority.Background)
		{
			Interval = TimeSpan.FromSeconds(6)
		};
		_sliderTimer.Tick += SliderTimer_Tick;

	}

	private void HomeView_Loaded(object sender, RoutedEventArgs e)
	{
		if (!IsVisible)
			return;

		ActivateHome();
	}

	private void HomeView_Unloaded(object sender, RoutedEventArgs e)
	{
		DeactivateHome(clearVisuals: true);
	}

	private void HomeView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!IsLoaded)
			return;

		if (IsVisible)
		{
			ActivateHome();
		}
		else
		{
			// keeps the prepared feature/image bindings while the responsive
			// host is merely Collapsed. Timers and subscriptions still stop, but
			// the next breakpoint crossing does not rebuild the whole Home tree.
			DeactivateHome(clearVisuals: false);
		}
	}

	private void ActivateHome()
	{
		if (_isActive)
			return;

		_isActive = true;
		AttachCatalogModelNotifier();

		// A cached Home may already contain the exact four current catalog items.
		// Avoid force-rebinding every ImageBrush exactly when it becomes visible.
		RefreshFeaturedItems(force: _featuredItems.Count == 0);
		_lastSlideAt = DateTime.Now;
		_sliderTimer.Start();
		QueueStickyFilterUpdate();
	}

	private void DeactivateHome(bool clearVisuals)
	{
		_isActive = false;
		_sliderTimer.Stop();
		DetachCatalogModelNotifier();
		StopFeatureAnimations();

		if (!clearVisuals)
			return;

		ClearFeatureVisuals();
		_featuredItems.Clear();
		_selectedFeatureIndex = 0;
	}

	private void HomeView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		DetachCatalogModelNotifier();
		_selectedFeatureIndex = 0;
		_featuredItems.Clear();

		if (_isActive)
		{
			AttachCatalogModelNotifier();
			RefreshFeaturedItems(force: true);
		}
		else
		{
			ClearFeatureVisuals();
		}

		QueueStickyFilterUpdate();
	}

	private void AttachCatalogModelNotifier()
	{
		var notifier = ViewModel?.Catalog?.Model as INotifyPropertyChanged;
		if (ReferenceEquals(_catalogModelNotifier, notifier))
			return;

		DetachCatalogModelNotifier();
		_catalogModelNotifier = notifier;
		if (_catalogModelNotifier != null)
		{
			_catalogModelNotifier.PropertyChanged += CatalogModel_PropertyChanged;
		}
	}

	private void DetachCatalogModelNotifier()
	{
		if (_catalogModelNotifier != null)
			_catalogModelNotifier.PropertyChanged -= CatalogModel_PropertyChanged;
		_catalogModelNotifier = null;
	}

	private void CatalogModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (!_isActive || (!string.IsNullOrEmpty(e.PropertyName) && e.PropertyName != "Instances"))
			return;

		// CatalogModel can complete a request on a worker thread. Marshal the small
		// featured-card refresh back to the UI thread once, after the page data changed.
		Dispatcher.BeginInvoke(new Action(() =>
		{
			if (_isActive)
			{
				RefreshFeaturedItems(force: true);
				_lastSlideAt = DateTime.Now;
			}
		}), DispatcherPriority.DataBind);
	}

	private void PageScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		QueueStickyFilterUpdate();
	}

	private void CatalogBlock_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		QueueStickyFilterUpdate();
	}

	private void QueueStickyFilterUpdate()
	{
		if (!_isActive || _stickyUpdatePending || Dispatcher == null)
			return;

		_stickyUpdatePending = true;
		Dispatcher.BeginInvoke(new Action(() =>
		{
			_stickyUpdatePending = false;
			UpdateStickyFilter();
		}), DispatcherPriority.Render);
	}

	/// <summary>
	/// Keeps the compact filter card at the top of the viewport only after its
	/// normal position reaches the top. Translation is capped at the catalog end.
	/// </summary>
	private void UpdateStickyFilter()
	{
		if (!_isActive || !IsLoaded || PageScrollViewer == null || FilterPanel == null || FilterStickyAnchor == null || CatalogBlock == null)
			return;

		if (FilterPanel.Visibility != Visibility.Visible || FilterPanel.ActualHeight <= 0)
		{
			FilterStickyTranslate.Y = 0;
			return;
		}

		try
		{
			const double stickyTop = 16.0;
			var anchorPoint = FilterStickyAnchor.TranslatePoint(new Point(0, 0), PageScrollViewer);
			var wantedTranslation = Math.Max(0, stickyTop - anchorPoint.Y);
			var maxTranslation = Math.Max(0, CatalogBlock.ActualHeight - FilterPanel.ActualHeight);
			FilterStickyTranslate.Y = Math.Min(wantedTranslation, maxTranslation);
		}
		catch (InvalidOperationException)
		{
			FilterStickyTranslate.Y = 0;
		}
	}

	/// <summary>
	/// WPF Border.CornerRadius does not clip child content. Keep one geometry per
	/// surface and mutate its Rect instead of allocating a new geometry on every
	/// live-resize event.
	/// </summary>
	private void RoundedClip_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		var element = sender as FrameworkElement;
		if (element == null || e.NewSize.Width <= 0 || e.NewSize.Height <= 0)
			return;

		UpdateRoundedClip(element, 10.0, e.NewSize);
	}

	private static void UpdateRoundedClip(FrameworkElement element, double radius)
	{
		if (element == null || element.ActualWidth <= 0 || element.ActualHeight <= 0)
			return;

		UpdateRoundedClip(element, radius, new Size(element.ActualWidth, element.ActualHeight));
	}

	private static void UpdateRoundedClip(FrameworkElement element, double radius, Size size)
	{
		var geometry = element.Clip as RectangleGeometry;
		if (geometry == null)
		{
			geometry = new RectangleGeometry();
			geometry.RadiusX = radius;
			geometry.RadiusY = radius;
			element.Clip = geometry;
		}

		geometry.Rect = new Rect(0, 0, size.Width, size.Height);
	}

	private void SliderTimer_Tick(object sender, EventArgs e)
	{
		if (!_isActive || !IsVisible || _isFeatureStripHovered || _isTransitionRunning || _featuredItems.Count <= 1)
			return;

		AnimateToFeature((_selectedFeatureIndex + 1) % _featuredItems.Count);
	}

	/// <summary>
	/// Builds the small, above-the-fold Home data set before the responsive host
	/// becomes visible. This is intentionally safe while inactive: no timers or
	/// model subscriptions are started here.
	/// </summary>
	public void PrewarmForResponsiveSwitch()
	{
		RefreshFeaturedItems(force: _featuredItems.Count == 0, allowInactive: true);
	}

	private bool RefreshFeaturedItems(bool force, bool allowInactive = false)
	{
		var viewModel = ViewModel;
		if ((!_isActive && !allowInactive) || viewModel == null || viewModel.Catalog == null || viewModel.Catalog.Model == null)
			return false;

		var latest = viewModel.Catalog.Model.Instances == null
			? new List<InstanceModelBase>()
			: viewModel.Catalog.Model.Instances.Take(4).ToList();

		var changed = force || latest.Count != _featuredItems.Count;
		if (!changed)
		{
			for (var i = 0; i < latest.Count; i++)
			{
				if (!ReferenceEquals(latest[i], _featuredItems[i]))
				{
					changed = true;
					break;
				}
			}
		}

		if (!changed)
		{
			return false;
		}

		_featuredItems.Clear();
		_featuredItems.AddRange(latest);

		if (_featuredItems.Count == 0)
			_selectedFeatureIndex = 0;
		else if (_selectedFeatureIndex >= _featuredItems.Count)
			_selectedFeatureIndex = 0;

		// Only four recommendation visuals are created. The previous version bound
		// the whole page collection and clipped extra rows, which still forced WPF
		// to measure/render unnecessary card visuals during resize.
		RecommendationCards.ItemsSource = _featuredItems.ToArray();
		ApplyFeatureDataContexts();
		return true;
	}

	private void ApplyFeatureDataContexts()
	{
		if (_featuredItems.Count == 0)
		{
			ClearFeatureVisuals();
			return;
		}

		HeroContentHost.DataContext = _featuredItems[_selectedFeatureIndex];

		var sideButtons = new[] { SideFeature0, SideFeature1, SideFeature2 };
		for (var i = 0; i < sideButtons.Length; i++)
		{
			if (_featuredItems.Count <= 1)
			{
				sideButtons[i].DataContext = null;
				sideButtons[i].Visibility = Visibility.Collapsed;
				continue;
			}

			var sourceIndex = (_selectedFeatureIndex + i + 1) % _featuredItems.Count;
			if (sourceIndex == _selectedFeatureIndex)
			{
				sideButtons[i].DataContext = null;
				sideButtons[i].Visibility = Visibility.Collapsed;
				continue;
			}

			sideButtons[i].DataContext = _featuredItems[sourceIndex];
			sideButtons[i].Visibility = Visibility.Visible;
		}
	}

	private void ClearFeatureVisuals()
	{
		if (HeroContentHost != null)
			HeroContentHost.DataContext = null;

		if (SideFeature0 != null)
			SideFeature0.DataContext = null;
		if (SideFeature1 != null)
			SideFeature1.DataContext = null;
		if (SideFeature2 != null)
			SideFeature2.DataContext = null;
		if (RecommendationCards != null)
			RecommendationCards.ItemsSource = null;
	}

	private void AnimateToFeature(int newIndex)
	{
		if (!_isActive || _featuredItems.Count == 0 ||
			newIndex < 0 || newIndex >= _featuredItems.Count)
			return;

		if (newIndex == _selectedFeatureIndex)
		{
			_lastSlideAt = DateTime.Now;
			return;
		}

		_isTransitionRunning = true;

		var fadeOut = new DoubleAnimation
		{
			To = 0,
			Duration = TimeSpan.FromMilliseconds(150),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
		};

		var slideOut = new DoubleAnimation
		{
			To = -24,
			Duration = TimeSpan.FromMilliseconds(150),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
		};

		fadeOut.Completed += (s, e) =>
		{
			if (!_isActive)
			{
				_isTransitionRunning = false;
				return;
			}

			_selectedFeatureIndex = newIndex;
			ApplyFeatureDataContexts();

			HeroContentHost.Opacity = 0;
			HeroTranslate.X = 26;

			var fadeIn = new DoubleAnimation
			{
				From = 0,
				To = 1,
				Duration = TimeSpan.FromMilliseconds(240),
				EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
			};

			var slideIn = new DoubleAnimation
			{
				From = 26,
				To = 0,
				Duration = TimeSpan.FromMilliseconds(240),
				EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
			};

			fadeIn.Completed += (sender2, args2) =>
			{
				_isTransitionRunning = false;
				_lastSlideAt = DateTime.Now;
			};

			HeroContentHost.BeginAnimation(OpacityProperty, fadeIn);
			HeroTranslate.BeginAnimation(TranslateTransform.XProperty, slideIn);
			AnimateSideCards();
		};

		HeroContentHost.BeginAnimation(OpacityProperty, fadeOut);
		HeroTranslate.BeginAnimation(TranslateTransform.XProperty, slideOut);
	}

	private void StopFeatureAnimations()
	{
		_isTransitionRunning = false;

		if (HeroContentHost != null)
		{
			HeroContentHost.BeginAnimation(OpacityProperty, null);
			HeroContentHost.Opacity = 1;
		}

		if (HeroTranslate != null)
		{
			HeroTranslate.BeginAnimation(TranslateTransform.XProperty, null);
			HeroTranslate.X = 0;
		}

		var buttons = new[] { SideFeature0, SideFeature1, SideFeature2 };
		foreach (var button in buttons)
		{
			if (button == null)
				continue;

			button.BeginAnimation(OpacityProperty, null);
			button.Opacity = 1;
		}
	}

	private void AnimateSideCards()
	{
		var buttons = new[] { SideFeature0, SideFeature1, SideFeature2 };
		for (var i = 0; i < buttons.Length; i++)
		{
			var button = buttons[i];
			button.Opacity = 0.65;
			var animation = new DoubleAnimation
			{
				From = 0.65,
				To = 1,
				BeginTime = TimeSpan.FromMilliseconds(i * 35),
				Duration = TimeSpan.FromMilliseconds(180),
				EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
			};
			button.BeginAnimation(OpacityProperty, animation);
		}
	}

	private void SideFeature_Click(object sender, RoutedEventArgs e)
	{
		var button = sender as Button;
		var instance = button == null ? null : button.DataContext as InstanceModelBase;
		if (instance == null)
			return;

		var index = _featuredItems.IndexOf(instance);
		if (index >= 0)
			AnimateToFeature(index);
	}

	private void HeroCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		var viewModel = ViewModel;
		var instance = HeroContentHost.DataContext as InstanceModelBase;
		if (viewModel == null || instance == null)
			return;

		if (viewModel.Catalog.OpenInstanceProfileMenu.CanExecute(instance))
			viewModel.Catalog.OpenInstanceProfileMenu.Execute(instance);
	}

	private void RecommendationCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		var card = sender as Border;
		var instance = card?.DataContext as InstanceModelBase;
		var viewModel = ViewModel;

		if (instance == null || viewModel?.Catalog?.OpenInstanceProfileMenu == null)
			return;

		// The Update button inside the card handles its own mouse click, so this
		// bubbling handler is reached only when the user clicks the card surface.
		var command = viewModel.Catalog.OpenInstanceProfileMenu;
		if (!command.CanExecute(instance))
			return;

		command.Execute(instance);
		e.Handled = true;
	}


	private void FeatureStrip_MouseEnter(object sender, MouseEventArgs e)
	{
		_isFeatureStripHovered = true;
	}

	private void FeatureStrip_MouseLeave(object sender, MouseEventArgs e)
	{
		_isFeatureStripHovered = false;
		_lastSlideAt = DateTime.Now;
	}
}
