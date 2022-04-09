using Lexplosion.Global;
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
            if (instanceProperties.LocalId != null && UserData.Instances.Record.ContainsKey(instanceProperties.LocalId))
                source = UserData.Instances.Record[instanceProperties.LocalId].Type;
            _instanceProperties = instanceProperties;
            if (source == InstanceSource.Curseforge || source == InstanceSource.Nightworld)
                LoadingOutsideInstance();
            // TODO: сборка созданая в лаунчере описываться тут не будет
        }

        public void LoadingOutsideInstance()
        {
            Lexplosion.Run.TaskRun(delegate ()
            {
                GetInstance();
                this.Dispatcher.Invoke(delegate ()
                {
                    var uris = GetUrls();

                    foreach (var ur in uris)
                    {
                        Console.WriteLine(ur);
                    }
                    Gallery.LoadImages(uris);
                    Description.Text = _instanceInfo.summary;
                    ShortDescription.Text = _instanceInfo.summary;

                    SetRightPanelInfo();

                    foreach (var item in _instanceInfo.categories)
                        CategoryPanel.Children.Add(GetCategery(item.name));

                    LoadedPage();
                });
            });
        }

        public void ClearGallery()
        {
            Gallery.Clear();
        }

        private void SetRightPanelInfo()
        {
            Verison.Text = _instanceInfo.gameVersionLatestFiles[0].gameVersion;
            LastUpdate.Text = DateTime.Parse(_instanceInfo.dateModified).ToString("dd MMM yyyy");
            TotalDownloads.Text = ((Int32)_instanceInfo.downloadCount).ToString("##,#");
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
            switch (source)
            {
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

        private void LoadingPage()
        {
            var skeletonElems = new Border[]
            {
                LoadingGallery, LoadingVersion1, LoadingVersion2, LoadingLastUpdate1, LoadingLastUpdate2,
                LoadingTotalDownloads1, LoadingTotalDownloads2, LoadingCore1, LoadingCore2, LoadingShortDescription,
                LoadingCategoryPanel, LoadingDescriptionTitle, LoadingDescription, LoadingWebsiteButton
            };
        }

        private void LoadedPage()
        {
            var skeletonElems = new Border[]
            {
                LoadingGallery, LoadingVersion1, LoadingVersion2, LoadingLastUpdate1, LoadingLastUpdate2,
                LoadingTotalDownloads1, LoadingTotalDownloads2, LoadingCore1, LoadingCore2, LoadingShortDescription,
                LoadingCategoryPanel, LoadingDescriptionTitle, LoadingDescription, LoadingWebsiteButton
            };

            var otherElems = new UIElement[]
            {
                Description, Gallery, LastUpdate, TotalDownloads, ShortDescription, CategoryPanelBorder, CategoryPanel
            };

            foreach (var elem in skeletonElems)
            {
                elem.Visibility = Visibility.Hidden;
            }

            foreach (var elem in otherElems)
            {
                elem.Visibility = Visibility.Visible;
            }
        }
    }
}