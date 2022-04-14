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
        private readonly Dictionary<string, double> tagSizes = new Dictionary<string, double>()
        {
            { "Tech", 36.5333333333333 },
            { "Magic", 46.9233333333333},
            { "Sci-Fi", 49.88},
            { "Adventure and RPG", 132.433333333333},
            { "Exploration", 80.4466666666667},
            { "Mini Game", 77.18},
            { "Quests", 51.3233333333333},
            { "Hardcore", 66.5366666666667},
            { "Map Based", 78.2066666666667},
            { "Small / Light", 88.28},
            { "Extra Large", 78.74},
            { "Combat / PvP", 95.0533333333333},
            { "Multiplayer", 80.5133333333333},
            { "FTB Official Pack", 113.16},
            { "Skyblock", 64.4766666666667},
            { "Vanilla+", 49.71}
        };

        private readonly InstanceProperties _instanceProperties;
        private readonly InstanceSource source;
        private CurseforgeInstanceInfo _instanceInfo;

        public OverviewPage(InstanceProperties instanceProperties)
        {
            InitializeComponent();

            source = instanceProperties.Type;
            _instanceProperties = instanceProperties;

            if (source == InstanceSource.Curseforge || source == InstanceSource.Nightworld) 
            {
                GetInstance();
                LoadingOutsideInstance();
            }
        }

        public void LoadingOutsideInstance()
        {
            Lexplosion.Run.TaskRun(delegate ()
            {
                this.Dispatcher.Invoke(delegate ()
                {
                    Gallery.LoadImages(GetUrls());
                    Description.Text = _instanceInfo.summary;
                    ShortDescription.Text = _instanceInfo.summary;

                    SetRightPanelInfo();

                    var childWidth = 0.0;

                    foreach (var item in _instanceInfo.categories) 
                    { 
                        CategoryPanel.Children.Add(GetCategery(item.name));
                        childWidth += tagSizes[item.name];
                    }

                    if (childWidth < 326.5) CategoryPanelBorder.Height += 20;
                    else CategoryPanelBorder.Height += 50;

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
            switch (_instanceInfo.Modloader) {
                case ModloaderType.None:
                    Core.Text = "Vanilla";
                    break;
                case ModloaderType.Forge:
                    Core.Text = "Forge";
                    break;
                case ModloaderType.Fabric:
                    Core.Text = "Fabric";
                    break;
            }
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