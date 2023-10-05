using Lexplosion.Common.Models.Objects;
using Lexplosion.Common.ViewModels;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lexplosion.Common.Models.InstanceForm
{
    public sealed class InstanceFormModel : VMBase
    {
        #region Properties


        private readonly MainViewModel _mainViewModel;
        public ObservableCollection<LowerButton> LowerButtons { get; } = new ObservableCollection<LowerButton>();
        public ObservableCollection<IProjectCategory> Categories { get; } = new ObservableCollection<IProjectCategory>();
        public InstanceClient InstanceClient { get; }
        public DownloadModel DownloadModel { get; }
        public LaunchModel LaunchModel { get; }
        public UpperButton UpperButton { get; set; }


        public readonly Action OnComplitedError;


        public InstanceDistribution InstanceDistribution { get; }

        private bool _isLaunch = false;
        public bool IsLaunch
        {
            get => _isLaunch; set
            {
                _isLaunch = value;
                OnPropertyChanged();
            }
        }

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


        public InstanceFormModel(MainViewModel mainViewModel, InstanceClient instanceClient, InstanceFormViewModel instanceFormViewModel, InstanceDistribution instanceDistribution)
        {
            _mainViewModel = mainViewModel;
            InstanceDistribution = instanceDistribution;
            InstanceClient = instanceClient;

            instanceClient.BuildFinished += UpdateFromInstanceClient;
            instanceClient.StateChanged += UpdateLowerButton;
            instanceClient.DownloadStarted += InstanceClient_DownloadStarted;
            LoadingCategories(InstanceClient.Categories);

            UpperButtonSetup();

            OverviewField = instanceClient.Summary;
            LaunchModel = new LaunchModel(instanceClient, _mainViewModel, this, instanceFormViewModel);
            OnComplitedError = () =>
            {
                LaunchModel.Shutdown();
            };
            DownloadModel = new DownloadModel(this, OnComplitedError, MainViewModel.ShowToastMessage);


            UpdateButtons();
        }

        private void InstanceClient_DownloadStarted()
        {
            UpdateLowerButton();
        }

        #endregion Constructors


        #region Public & Protected Methods


        public void OpenInstanceFolder()
        {
            Process.Start("explorer", InstanceClient.GetDirectoryPath());
        }

        public void UpdateButtons()
        {
            if (InstanceClient.IsInstalled && !MainModel.Instance.IsInstanceRunning)
            {
                UpperButton.ChangeFuncPlay();
            }
            else if (!InstanceClient.IsInstalled || !InstanceClient.InLibrary)
            {
                UpperButton.ChangeFuncDownload();
            }
            else
            {
                UpperButton.ChangeFuncClose();
            }

            UpdateLowerButton();
        }

        public void UpdateLowerButton()
        {
            App.Current.Dispatcher.Invoke(() =>
            {

                LowerButtons.Clear();

                if (InstanceClient.UpdateAvailable && !DownloadModel.IsPrepareOnly)
                {
                    LowerButtons.Add(
                        new LowerButton(ResourceGetter.GetString("update"), ResourceGetter.GetIcon("UpdateInstance"), LowerButtonFunc.Update)
                    );
                }
                if (InstanceClient.WebsiteUrl != null)
                {
                    switch (InstanceClient.Type)
                    {
                        case InstanceSource.Curseforge:
                            {
                                LowerButtons.Add(new LowerButton(ResourceGetter.GetString("visitCurseforge"), ResourceGetter.GetIcon("Planet"), LowerButtonFunc.OpenWebsite));
                                break;
                            }
                        case InstanceSource.Nightworld:
                            {
                                LowerButtons.Add(new LowerButton(ResourceGetter.GetString("visitNightWorld"), ResourceGetter.GetIcon("Planet"), LowerButtonFunc.OpenWebsite));
                                break;
                            }
                        case InstanceSource.Modrinth:
                            {
                                LowerButtons.Add(new LowerButton(ResourceGetter.GetString("visitModrinth"), ResourceGetter.GetIcon("Planet"), LowerButtonFunc.OpenWebsite));
                                break;
                            }
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

                if (DownloadModel.IsDownloadInProgress && !MainModel.Instance.IsInstanceRunning)
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

                //ObservableCollectionExtensions.ObservableColletionSort(LowerButtons);
            });
        }


        public void StopDownloadShareInstance()
        {
            InstanceDistribution.CancelDownload();
        }


        #endregion Public & Protected Methods


        #region Private Methods

        private void UpdateFromInstanceClient()
        {
            OverviewField = InstanceClient.Summary;
            LoadingCategories(InstanceClient.Categories);
        }


        /// <summary>
        /// Загружаем категории
        /// </summary>
        private void LoadingCategories(IEnumerable<CategoryBase> categories)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Categories.Clear();
                Categories.Add(new SimpleCategory { Name = InstanceClient.GameVersion.Id });

                if (categories != null)
                {
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                    }
                }
            });
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