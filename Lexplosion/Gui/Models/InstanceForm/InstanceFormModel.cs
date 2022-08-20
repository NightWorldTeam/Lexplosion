using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class InstanceFormModel : VMBase
    {
        private string _overviewField;
        private List<Category> _categories = new List<Category>();

        #region props

        public InstanceClient InstanceClient { get; set; }
        public DownloadModel DownloadModel { get; set; }
        public LaunchModel LaunchModel { get; set; }

        // buttons
        public UpperButton UpperButton { get; set; }
        // сделать lock объект
        public ObservableCollection<LowerButton> LowerButtons { get; } = new ObservableCollection<LowerButton>();

        private object _locker = false;

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
                    MultiButtonProperties.GeometryDownloadIcon,
                    UpperButtonFunc.Download,
                    new Tip()
                    {
                        Text = "Установить сборку",
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
            DownloadModel = new DownloadModel(this)
            {
                DownloadProgress = 0,
                Stage = 0,
                StagesCount = 0
            };
            LaunchModel = new LaunchModel(mainViewModel, this);

            if (InstanceClient.IsInstalled && InstanceClient.InLibrary)
            {
                UpperButton.ChangeFuncPlay();
            }
            else UpperButton.ChangeFuncDownload(InstanceClient.IsInstalled);
            UpdateLowerButton();
        }

        public void OpenInstanceFolder()
        {
            Process.Start("explorer", InstanceClient.GetDirectoryPath());
        }

        public void UpdateLowerButton()
        {
            App.Current.Dispatcher.Invoke(() => { 
                LowerButtons.Clear();
                if (InstanceClient.UpdateAvailable)
                {
                    LowerButtons.Add(
                        new LowerButton("Обновить", MultiButtonProperties.UpdateInstance, LowerButtonFunc.Update)
                    );
                }

                if (InstanceClient.WebsiteUrl != null)
                {
                    if (InstanceClient.Type == InstanceSource.Curseforge)
                    {
                        LowerButtons.Add(
                                new LowerButton("Перейти на Curseforge", MultiButtonProperties.ToCurseforge, LowerButtonFunc.OpenWebsite)
                            );
                    }
                    else if (InstanceClient.Type == InstanceSource.Nightworld)
                    {
                        LowerButtons.Add(
                            new LowerButton("Перейти на NightWorld", MultiButtonProperties.ToCurseforge, LowerButtonFunc.OpenWebsite)
                        );
                    }

                }

                if (InstanceClient.InLibrary || DownloadModel.IsDownloadInProgress)
                {
                    LowerButtons.Add(
                            new LowerButton("Удалить из библиотеки", MultiButtonProperties.GeometryLibraryDelete, LowerButtonFunc.DeleteFromLibrary)
                        );
                }
                else
                {
                    LowerButtons.Add(
                        new LowerButton("Добавить в библиотеку", MultiButtonProperties.GeometryLibraryAdd, LowerButtonFunc.AddToLibrary)
                    );
                }

                if (DownloadModel.IsDownloadInProgress)
                {
                    LowerButtons.Add(
                        new LowerButton("Отменить скачивание", MultiButtonProperties.GeometryCancelIcon, LowerButtonFunc.CancelDownload)
                    );
                }

                if (InstanceClient.IsInstalled)
                {
                    LowerButtons.Add(
                        new LowerButton("Открыть папку", MultiButtonProperties.GeometryOpenFolder, LowerButtonFunc.OpenFolder)
                    );
                    LowerButtons.Add(
                        new LowerButton("Удалить сборку", MultiButtonProperties.RemoveInstance, LowerButtonFunc.RemoveInstance)
                    );
                    LowerButtons.Add(
                        new LowerButton("Экспорт", MultiButtonProperties.Export, LowerButtonFunc.Export)
                    );
                }
            });
        }
    }
}