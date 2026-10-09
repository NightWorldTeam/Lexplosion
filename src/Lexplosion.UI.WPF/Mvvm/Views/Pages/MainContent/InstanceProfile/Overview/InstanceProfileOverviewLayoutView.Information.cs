using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.InstanceProfile
{
	public partial class InstanceProfileOverviewLayoutView
	{
		// Same viewport breakpoint as InstanceDescriptionHtmlBuilder's metadata-sidebar CSS.
		// The browser reserves 10 px for the scrollbar when the launcher is maximized.
		private const double InlineInformationMinWidth = 1120;

		private bool NeedsInformationButton()
		{
			var window = Window.GetWindow(this);
			double inset = window != null && window.WindowState == WindowState.Maximized ? 10 : 0;
			return Tabs.ActualWidth <= 0 || Tabs.ActualWidth - inset < InlineInformationMinWidth;
		}

		private void OnOverviewInformationClick(object sender, RoutedEventArgs e)
		{
			if (!NeedsInformationButton() || Tabs.SelectedIndex != 0) return;

			var view = FindOverviewView(Tabs) ?? FindOverviewView(this);
			view?.ToggleInformation(sender as FrameworkElement);
			// The header control remains at the exact same coordinates when
			// switching between open (×) and closed (i) states.
			UpdateOverviewInfoButton();
		}

		private void OnOverviewTabsLoadedForInfo(object sender, RoutedEventArgs e)
			=> UpdateOverviewInfoButton();

		private void OnOverviewTabsSizeChangedForInfo(object sender, SizeChangedEventArgs e)
			=> UpdateOverviewInfoButton();

		private void OnOverviewTabsSelectionChangedForInfo(object sender, SelectionChangedEventArgs e)
		{
			if (ReferenceEquals(e.OriginalSource, Tabs))
				UpdateOverviewInfoButton();
		}

		private void UpdateOverviewInfoButton()
		{
			bool compactOverview = Tabs.SelectedIndex == 0 && NeedsInformationButton();
			OverviewInformationButton.Visibility = compactOverview
				? Visibility.Visible : Visibility.Collapsed;

			var view = FindOverviewView(Tabs) ?? FindOverviewView(this);
			if (!compactOverview)
			{
				// No drawer in wide layouts, and none while other tabs are active.
				view?.CloseInformationIfOpen();
			}

			bool isOpen = compactOverview && view?.IsInformationDrawerOpen == true;
			OverviewInformationGlyph.Text = isOpen ? "×" : "i";
			OverviewInformationGlyph.FontSize = isOpen ? 20 : 16;
			OverviewInformationButton.ToolTip = isOpen
				? "Закрыть информацию о сборке" : "Показать информацию о сборке";
		}

		private static InstanceProfileOverviewView FindOverviewView(DependencyObject parent)
		{
			if (parent == null) return null;
			if (parent is InstanceProfileOverviewView view) return view;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var found = FindOverviewView(VisualTreeHelper.GetChild(parent, i));
				if (found != null) return found;
			}
			return null;
		}
	}
}
