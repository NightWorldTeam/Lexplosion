using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public sealed class InstanceFormModel : VMBase
    {
        private string _overviewField;
        private List<Category> _categories = new List<Category>();

        #region Properties

        public InstanceClient InstanceClient { get; set; }
        public DownloadModel DownloadModel { get; set; }
        public LaunchModel LaunchModel { get; set; }

        // buttons
        public UpperButton UpperButton { get; set; }
        // сделать lock объект
        public ObservableCollection<LowerButton> LowerButtons { get; } = new ObservableCollection<LowerButton>();

        public string OverviewField
        {
            get => _overviewField; set
            {
                _overviewField = value;
                OnPropertyChanged();
            }
        }

        public List<Category> Categories
        {
            get => _categories; set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        private bool _isCanRun;
        public bool IsCanRun
        {
            get => _isCanRun; set
            {
                _isCanRun = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public InstanceFormModel(MainViewModel mainViewModel, InstanceClient instanceClient)
        {
            InstanceClient = instanceClient;

            instanceClient.StateChanged += UpdateLowerButton;

            UpperButton = new UpperButton
                (
                    ResourceGetter.GetIcon("Download"),
                    UpperButtonFunc.Download,
                    new Tip()
                    {
                        Text = ResourceGetter.GetString("installInstance"),
                        Offset = -160
                    }
                );

            // set categories to list
            // add game version like category
            Categories.Add(new Category { name = instanceClient.GameVersion });
            if (InstanceClient.Categories != null && InstanceClient.Categories.Count > 0) 
            { 
                foreach (var category in InstanceClient.Categories)
                {
                    Categories.Add(category);
                }
            }

            OverviewField = instanceClient.Summary;
            DownloadModel = new DownloadModel(mainViewModel, this)
            {
                DownloadProgress = 0,
                Stage = 0,
                StagesCount = 0
            };
            LaunchModel = new LaunchModel(mainViewModel, this);

            UpdateButtons();
        }

        public void OpenInstanceFolder()
        {
            Process.Start("explorer", InstanceClient.GetDirectoryPath());
        }

        public void UpdateButtons() 
        {
            UpdateLowerButton();

            if (InstanceClient.IsInstalled) 
            { 
                UpperButton.ChangeFuncPlay();
            }
            else if (!InstanceClient.IsInstalled || !InstanceClient.InLibrary) 
            {
                UpperButton.ChangeFuncDownload();
            }
        }

        public void UpdateLowerButton()
        {
            App.Current.Dispatcher.Invoke(() => { 

                LowerButtons.Clear();

                if (InstanceClient.UpdateAvailable)
                {
                    LowerButtons.Add(
                        new LowerButton(ResourceGetter.GetString("update"), ResourceGetter.GetIcon("UpdateInstance"), LowerButtonFunc.Update)
                    );
                }

                if (InstanceClient.WebsiteUrl != null)
                {
                    if (InstanceClient.Type == InstanceSource.Curseforge)
                    {
                        LowerButtons.Add(
                                new LowerButton(ResourceGetter.GetString("visitCurseforge"), ResourceGetter.GetIcon("CurseforgeLogo"), LowerButtonFunc.OpenWebsite)
                            );
                    }
                    else if (InstanceClient.Type == InstanceSource.Nightworld)
                    {
                        LowerButtons.Add(
                            new LowerButton(ResourceGetter.GetString("visitNightWorld"), ResourceGetter.GetIcon("CurseforgeLogo"), LowerButtonFunc.OpenWebsite)
                        );
                    }

                }

                if (InstanceClient.InLibrary && !InstanceClient.IsInstalled && !DownloadModel.IsDownloadInProgress)
                {
                    LowerButtons.Add(
                            new LowerButton(ResourceGetter.GetString("removeFromLibrary"), MultiButtonProperties.GeometryLibraryDelete, LowerButtonFunc.DeleteFromLibrary)
                        );
                }

                else if (!InstanceClient.InLibrary && !DownloadModel.IsDownloadInProgress)
                {
                    LowerButtons.Add(
                        new LowerButton(ResourceGetter.GetString("addToLibrary"), MultiButtonProperties.GeometryLibraryAdd, LowerButtonFunc.AddToLibrary)
                    );
                }

                if (DownloadModel.IsDownloadInProgress)
                {
                    LowerButtons.Add(
                        new LowerButton(ResourceGetter.GetString("cancelDownload"), MultiButtonProperties.GeometryCancelIcon, LowerButtonFunc.CancelDownload)
                    );
                }

                if (InstanceClient.IsInstalled)
                {
                    LowerButtons.Add(
                        new LowerButton(ResourceGetter.GetString("removeInstance"), ResourceGetter.GetIcon("RemoveInstance"), LowerButtonFunc.RemoveInstance)
                    );
                    LowerButtons.Add(
                        new LowerButton(ResourceGetter.GetString("export"), ResourceGetter.GetIcon("Export"), LowerButtonFunc.Export)
                    );
                }

                if (InstanceClient.IsInstalled || InstanceClient.InLibrary) 
                {
                    LowerButtons.Add(
                        new LowerButton(ResourceGetter.GetString("openFolder"), ResourceGetter.GetIcon("OpenFolder"), LowerButtonFunc.OpenFolder)
                    );
                    LowerButtons.Add(
                        new LowerButton(ResourceGetter.GetString("instanceDLC"), ResourceGetter.GetIcon("OpenFolder"), LowerButtonFunc.OpenDLCPage)
                    );
                }
            });
        }
    }
}