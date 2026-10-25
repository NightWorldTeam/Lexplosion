using Lexplosion.UI.WPF.Services.Descriptions;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.InstanceProfile
{
    public partial class InstanceProfileOverviewView
    {
		private void OnOverviewSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateInformationLayout();
			UpdateScrollbarInset();
		}

		// WPF's maximized non-client border can clip a Chromium scrollbar.
		// Move only the WebView surface inwards, never the entire content pane.
		private void UpdateScrollbarInset()
		{
			var window = Window.GetWindow(this);
			double inset = window != null && window.WindowState == WindowState.Maximized ? 10 : 0;
			BrowserHost.Margin = new Thickness(0, 0, inset, 0);
		}

		// Matches the HTML media query for its permanently visible sidebar.
		// In maximized windows, account for the WebView scrollbar safety inset.
		private bool HasInlineInformation()
		{
			var window = Window.GetWindow(this);
			double inset = window != null && window.WindowState == WindowState.Maximized ? 10 : 0;
			return OverviewRoot.ActualWidth > 0 && OverviewRoot.ActualWidth - inset >= 1120;
		}

		private void UpdateInformationLayout()
		{
			if (HasInlineInformation())
			{
				CloseInformationIfOpen();
				return;
			}
			if (InformationDrawer.Visibility != Visibility.Visible) return;
			// Keep the compact drawer inside the page and outside WebView2 airspace.
			double available = Math.Max(0, OverviewRoot.ActualWidth);
			double width = Math.Min(400, Math.Max(265, available * 0.36));
			if (available > 0) width = Math.Min(width, Math.Max(0, available - 240));
			InformationDrawer.Width = width;
		}

		public bool IsInformationDrawerOpen => InformationDrawer.Visibility == Visibility.Visible;

		public void ToggleInformation(FrameworkElement headerButton)
		{
			if (HasInlineInformation())
			{
				CloseInformationIfOpen();
				return;
			}
			if (InformationDrawer.Visibility == Visibility.Visible)
			{
				CloseInformationDrawer();
				return;
			}
			InformationDrawer.Visibility = Visibility.Visible;
			UpdateInformationLayout();
			OverviewDiagnostics.Info("Information drawer opened");
		}

		public void CloseInformationIfOpen()
		{
			if (InformationDrawer.Visibility == Visibility.Visible)
				CloseInformationDrawer();
		}

		private void CloseInformationDrawer()
		{
			InformationDrawer.Visibility = Visibility.Collapsed;
			OverviewDiagnostics.Info("Information drawer closed");
		}
    }
}
