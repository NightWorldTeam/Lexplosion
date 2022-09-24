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
        #region Properties


        public ObservableCollection<LowerButton> LowerButtons { get; } = new ObservableCollection<LowerButton>();
        public List<Category> Categories { get; } = new List<Category>();
        public InstanceClient InstanceClient { get; }
        public DownloadModel DownloadModel { get; }
        public LaunchModel LaunchModel { get; }
        // buttons
        public UpperButton UpperButton { get; set; }


        private string _overviewField;
        public string OverviewField
        {
            get => _overviewField; set
            {
                _overviewField = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public InstanceFormModel(MainViewModel mainViewModel, InstanceClient instanceClient)
        {
            InstanceClient = instanceClient;

            instanceClient.StateChanged += UpdateLowerButton;

            UpperButtonSetup();

            LoadingCategories();

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


        #endregion Constructors


        #region Public & Protected Methods


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

                if (InstanceClient.UpdateAvailable && !DownloadModel.IsPrepareOnly)
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
                                new LowerButton(ResourceGetter.GetString("visitCurseforge"), ResourceGetter.GetIcon("Planet"), LowerButtonFunc.OpenWebsite)
                            );
                    }
                    else if (InstanceClient.Type == InstanceSource.Nightworld)
                    {
                        LowerButtons.Add(
                            new LowerButton(ResourceGetter.GetString("visitNightWorld"), ResourceGetter.GetIcon("Planet"), LowerButtonFunc.OpenWebsite)
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

                if (InstanceClient.IsInstalled && !DownloadModel.IsDownloadInProgress)
                {
                    LowerButtons.Add(
                        new LowerButton(ResourceGetter.GetString("removeInstance"), ResourceGetter.GetIcon("Delete"), LowerButtonFunc.RemoveInstance, int.MaxValue)
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
                        new LowerButton(ResourceGetter.GetString("instanceDLC"), ResourceGetter.GetIcon("Extension"), LowerButtonFunc.OpenDLCPage)
                    );
                }

                ObservableCollectionExtensions.ObservableColletionSort(LowerButtons);
            });
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void LoadingCategories() 
        {
            // set categories to list
            // add game version like category
            Categories.Add(new Category { name = InstanceClient.GameVersion });
            if (InstanceClient.Categories != null && InstanceClient.Categories.Count > 0)
            {
                foreach (var category in InstanceClient.Categories)
                {
                    Categories.Add(category);
                }
            }
        }


        private void UpperButtonSetup()
        {
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
        }

        #endregion Private Methods
    }
}