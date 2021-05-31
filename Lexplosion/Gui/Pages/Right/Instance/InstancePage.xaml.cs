using Lexplosion.Gui.Pages.Left;
using Lexplosion.Logic.Management;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Lexplosion.Gui.Pages.Right.Instance
{
    /// <summary>
    /// Interaction logic for InstancePage.xaml
    /// </summary>
    public partial class InstancePage : Page
    {
        private ToggleButton selectedToggleButton;
        public static InstancePage instance = null;

        private static string titleInstance = "";
        private static string descriptionInstance = "";
        private static string authorInstance = "";
        private static List<string> tagsInstance = new List<string>();


        public InstancePage(string title, string description, string author, List<string> tags)
        {
            InitializeComponent();
            instance = this;

            titleInstance = title;
            descriptionInstance = description;
            authorInstance = author;
            tagsInstance = tags;

            BottomSideFrame.Navigate(new OverviewPage(titleInstance, descriptionInstance));
            selectedToggleButton = OverviewToggleButton;
        }

        public static string GetTitleInstance() => titleInstance;
        public static string GetDescriptionInstance() => descriptionInstance;
        public static string GetAuthorInstance() => authorInstance;
        public static List<string> GetTagsInstance() => tagsInstance;


        private void ReselectionToggleButton(object sender)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            if (toggleButton.Name != selectedToggleButton.Name)
            {
                toggleButton.IsChecked = true;
                selectedToggleButton.IsChecked = false;
                selectedToggleButton = toggleButton;
            }
            else toggleButton.IsChecked = true;
        }

        private void ClickedOverview(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
            BottomSideFrame.Navigate(new OverviewPage(titleInstance, descriptionInstance));
        }

        private void ClickedModsList(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
            BottomSideFrame.Navigate(new ModsListPage());
        }

        private void ClickedVersion(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
            BottomSideFrame.Navigate(new VersionPage());
        }

        private void СlientManager(object sender, RoutedEventArgs e)
        {
            if(LeftSideMenuPage.instance.selectedInstance != "")
                ManageLogic.СlientManager(LeftSideMenuPage.instance.selectedInstance);

        }
    }
}
