using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lexplosion.Gui.Pages.Instance
{
    /// <summary>
    /// Interaction logic for OverviewPage.xaml
    /// </summary>
    public partial class OverviewPage : Page
    {
        private InstanceProperties _instanceProperties;
        private CurseforgeInstanceInfo _instanceInfo;
        private InstanceSource source = InstanceSource.Curseforge;
        public OverviewPage(InstanceProperties instanceProperties)
        {
            InitializeComponent();
            _instanceProperties = instanceProperties;
            GetInstance();

            Gallery.LoadImages(GetUrls());

            description.Text = _instanceInfo.summary;
            shortDescription.Text = _instanceInfo.summary;

            SetRightPanelInfo();

            foreach (var item in _instanceInfo.categories) 
                CategoryPanel.Children.Add(GetCategery(item.name));
        }

        private void SetRightPanelInfo() 
        {
            Verison.Text = "1.16.5";
            LastUpdate.Text = _instanceInfo.dateModified; 
            TotalDownloads.Text = ((Int32)_instanceInfo.downloadCount).ToString("##,#"); 
            Core.Text = "Forge"; 
        }

        private TextBlock GetCategery(string categery) => new TextBlock()
        {
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Colors.LightGray),
            Padding = new Thickness(5, 0, 3, 0),
            Text = categery
        };

        private List<string> GetUrls()
        {
            var urls = new List<string>();
            foreach (var item in _instanceInfo.attachments) 
            {
                if (!item.isDefault && !item.url.Contains("avatars"))
                    urls.Add(item.url);
            }
            return urls;
        }

        private void GetInstance() 
        {
            switch (source) {
                case InstanceSource.Curseforge:
                    _instanceInfo = CurseforgeApi.GetInstance(_instanceProperties.Id);
                    break;
                case InstanceSource.Nightworld:
                    break;
                case InstanceSource.Local:
                    break;
            }
        }

        private void CurseforgeUrl_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(_instanceInfo.websiteUrl);
        }
    }
}