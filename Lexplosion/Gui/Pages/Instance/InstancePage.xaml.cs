using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.Pages.Instance
{
    /// <summary>
    /// Interaction logic for InstancePage.xaml
    /// </summary>
    public partial class InstancePage : Page
    {
        public static InstancePage obj = null;
        private MainWindow _mainWindow;
        private List<ToggleButton> toggleButtons = new List<ToggleButton>();
        public InstanceProperties _instanceProperties;


        public InstancePage(MainWindow mainWindow, InstanceProperties instanceProperties)
        {
            InitializeComponent();
            obj = this;
            Console.WriteLine(obj.ToString());
            _mainWindow = mainWindow;
            _instanceProperties = instanceProperties;
            ActivateButtons();


            mainWindow.PagesController("OverviewPage", this.BottomSideFrame, delegate ()
            {
                return new OverviewPage(instanceProperties);
            });
        }

        public static InstanceProperties GetInstanceProperties() 
        {
            return obj._instanceProperties;   
        }

        private void ActivateButtons() 
        {
            toggleButtons.Add(OverviewToggleButton);
            toggleButtons.Add(ModsToggleButton);
            toggleButtons.Add(VersionToggleButton);
        }

        private void ClickedOverview(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(typeof(OverviewPage).ToString());
            _mainWindow.PagesController("OverviewPage", this.BottomSideFrame, delegate ()
            {
                return new OverviewPage(_instanceProperties);
            });
            ReselectionButton(OverviewToggleButton);
        }

        private void ClickedModsList(object sender, RoutedEventArgs e)
        {
            ReselectionButton(ModsToggleButton);
            //BottomSideFrame.Navigate(new ModsListPage(this));
        }

        private void ClickedVersion(object sender, RoutedEventArgs e)
        {
            ReselectionButton(VersionToggleButton);
            //BottomSideFrame.Navigate(new VersionPage(this));
        }

        private void СlientManager(object sender, RoutedEventArgs e)
        {
            //if (LeftSideMenuPage.instance.selectedInstance != "")
            //    ManageLogic.СlientManager(LeftSideMenuPage.instance.selectedInstance);
        }

        private void ReselectionButton(ToggleButton selectedButton)
        {
            foreach (ToggleButton toggleButton in toggleButtons)
            {
                if (toggleButton != selectedButton)
                {
                    toggleButton.IsEnabled = true;
                    toggleButton.IsChecked = false;
                }
                else
                {
                    selectedButton.IsChecked = true;
                    selectedButton.IsEnabled = false;
                }
            }
        }
    }
}