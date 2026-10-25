using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile;
using Lexplosion.UI.WPF.Services.Descriptions;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.InstanceProfile
{
    public partial class InstanceProfileOverviewView : UserControl
    {
		public InstanceProfileOverviewView()
		{
			InitializeComponent();
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			DataContextChanged += OnDataContextChanged;
			OverviewDiagnostics.Info("NavigationFullDataFix: view constructed");
		}

		private void SetStatus(string status)
		{
			BrowserStatus.Text = status;
			DescriptionLoader.Visibility = Visibility.Visible;
			// HwndHost paints above WPF controls even at a lower ZIndex.
			// Hiding only the browser (not its parent Grid) lets WebView2 finish
			// initializing/navigating without its blank native frame showing.
			if (_browser != null) _browser.Visibility = Visibility.Hidden;
			LoadingOverlay.Visibility = Visibility.Visible;
			_documentVisible = false;
			_revealScheduled = false;
			_documentNavigationStarted = false;
			_currentDocumentNavigationId = 0;
			OverviewDiagnostics.Info("Status: " + status);
		}

		private void ShowFailure(string message)
		{
			SetStatus(message);
			DescriptionLoader.Visibility = Visibility.Collapsed;
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			OverviewDiagnostics.Info("DataContextChanged; type=" + (e.NewValue?.GetType().FullName ?? "null"));
			AttachModel();
			RequestRender();
		}

		private void AttachModel()
		{
			if (_model != null)
				_model.PropertyChanged -= OnModelPropertyChanged;

			var vm = DataContext as InstanceProfileOverviewViewModel;
			_model = vm?.Model ?? DataContext as InstanceProfileOverviewModel;
			_lastDescription = null;
			_renderPending = false;
			++_renderVersion;
			// Do not show the previous instance's page while the new one loads.
			SetStatus("Загрузка полного описания…");

			if (_model != null)
			{
				_model.PropertyChanged += OnModelPropertyChanged;
					OverviewDiagnostics.Info("Model attached: source=" + _model.DescriptionSource +
					", ready=" + _model.IsDescriptionReady +
					", length=" + (_model.OverviewDescription?.Length ?? 0));
			}
			else
			{
				OverviewDiagnostics.Warn("Overview Model missing. Check DataContext binding.");
			}
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(InstanceProfileOverviewModel.OverviewDescription) &&
				e.PropertyName != nameof(InstanceProfileOverviewModel.IsDescriptionReady))
				return;
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.BeginInvoke(new Action(() => { RequestRender(); }));
				return;
			}
			RequestRender();
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			OverviewDiagnostics.Info("View Loaded; size=" + ActualWidth + "x" + ActualHeight);
			AttachModel();
			UpdateScrollbarInset();
			SetStatus("Инициализация WebView2…");
			await EnsureBrowserAsync();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			OverviewDiagnostics.Info("View Unloaded");
			++_generation;
			++_renderVersion;
			_initializing = false;
			_renderPending = false;
			CloseInformationDrawer();
			LoadingOverlay.Visibility = Visibility.Collapsed;
			if (_model != null)
			{
				_model.PropertyChanged -= OnModelPropertyChanged;
				_model = null;
			}
			ClearBrowser();
		}
    }
}
