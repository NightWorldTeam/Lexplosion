﻿using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Lexplosion.Logic.Management.Addons
{
    public class InstanceAddon : VMBase
    {
        public enum InstallAddonState
        {
            StartDownload,
            EndDownload
        }


        private const string UNKNOWN_NAME = "UnknownName";
        private const string DISABLE_FILE_EXTENSION = ".disable";


        public event Action LoadLoaded;


        private readonly BaseInstanceData _modpackInfo;
        private readonly IPrototypeAddon _addonPrototype;
        private readonly string _projectId;
        private readonly string _gameVersion;

        private readonly InstaceAddonsSynchronizer _synchronizer;
        private readonly IFileServicesContainer _services;
        private CancellationTokenSource _cancelTokenSource = null;


        #region Properties


        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; internal set; } = string.Empty;
        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; internal set; } = string.Empty;
        public string Version { get; internal set; } = string.Empty;
        /// <summary>
        /// Количество скачиваний
        /// </summary>
        public int DownloadCount { get; internal set; } = 0;
        /// <summary>
        /// Дата последнего обновления
        /// </summary>
        public string LastUpdated { get; internal set; } = string.Empty;
        /// <summary>
        /// Устанавливает значение для свойства IsEnable без вызова OnPropertyChanged
        /// </summary>
        internal bool SetIsEnable { set => _isEnable = value; }
        /// <summary>
        /// Тип (Mod, Resourcepack, etc)
        /// </summary>
        public AddonType Type { get; }

        public IEnumerable<string> DisplayModloaders { get; }
        public string CreatedTime { get; }
        public string GameVersion { get; }

        private string _author = string.Empty;
        public string Author
        {
            get => _addonPrototype?.AuthorName ?? _author; set;
        }

        private bool _updateAvailable;
        public bool UpdateAvailable
        {
            get => _updateAvailable; internal set
            {
                _updateAvailable = value;
                OnPropertyChanged();
            }
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName; internal set
            {
                _fileName = value;
                OnPropertyChanged();
            }
        }

        private bool _isEnable = true;
        public bool IsEnable
        {
            get => _isEnable; set
            {
                _isEnable = value;
                Disable();
                OnPropertyChanged();
            }
        }

        private bool _isInstalled = false;
        public bool IsInstalled
        {
            get => _isInstalled; internal set
            {
                _isInstalled = value;
                OnPropertyChanged();
            }
        }

        public byte[] _logo = null;
        public byte[] Logo
        {
            get => _logo;
            set
            {
                _logo = value;
                OnPropertyChanged();
                LoadLoaded?.Invoke();
            }
        }

        private bool _isInstalling = false;
        public bool IsInstalling
        {
            get => _isInstalling; set
            {
                _isInstalling = value;
                OnPropertyChanged();
            }
        }

        private int _downloadPercentages = 0;
        public int DownloadPercentages
        {
            get => _downloadPercentages; set
            {
                _downloadPercentages = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading; set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private string _websiteUrl = null;
        public string WebsiteUrl
        {
            get => _websiteUrl; set
            {
                _websiteUrl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUrlExist));
            }
        }

        public bool IsUrlExist
        {
            get => !string.IsNullOrWhiteSpace(_websiteUrl);
        }

        private string _logoUrl = null;
        public string LogoUrl
        {
            get => _logoUrl; set
            {
                _logoUrl = value;
                OnPropertyChanged();
            }
        }

        public ProjectSource Source
        {
            get => (_addonPrototype is CurseforgeAddon) ? ProjectSource.Curseforge : ((_addonPrototype is ModrinthAddon) ? ProjectSource.Modrinth : ProjectSource.None);
        }

        private IEnumerable<IProjectCategory> _categories = new List<CategoryBase>();
        public IEnumerable<IProjectCategory> Categories
        {
            get => _categories; private set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties

        /// <summary>
        /// Создает экземпляр аддона с курсфорджа.
        /// </summary>
        internal InstanceAddon(AddonType type, InstaceAddonsSynchronizer synchronizer, IFileServicesContainer services, IPrototypeAddon addonPrototype, BaseInstanceData modpackInfo)
        {
            Type = type;
            _synchronizer = synchronizer;
            _services = services;
            _addonPrototype = addonPrototype;
            _projectId = addonPrototype.ProjectId;
            _modpackInfo = modpackInfo;
            _gameVersion = modpackInfo.GameVersion.Id;

            Author = addonPrototype.AuthorName;
            Description = addonPrototype.Description;
            Name = addonPrototype.Name;

            LogoUrl = addonPrototype.LogoUrl;

            LoadAdditionalData(addonPrototype.LogoUrl);

            addonPrototype.OnInfoUpdated += delegate ()
            {
                OnPropertyChanged(nameof(Author));
            };
        }

        /// <summary>
        /// Создает экземпляр аддона не с курсфорджа.
        /// </summary>    
        internal InstanceAddon(AddonType type, InstaceAddonsSynchronizer synchronizer, IFileServicesContainer services, string projectId, BaseInstanceData modpackInfo)
        {
            Type = type;
            _addonPrototype = null;
            _synchronizer = synchronizer;
            _services = services;
            _projectId = projectId;
            _modpackInfo = modpackInfo;
            _gameVersion = modpackInfo.GameVersion.Id;
        }

        public string GetFullDescription()
        {
            return _addonPrototype?.GetFullDescription() ?? string.Empty;
        }

        public void Delete()
        {
            string instanceId = _modpackInfo.LocalId;
            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId, _services.DataFilesService))
            {
                InstalledAddonInfo addon = installedAddons[_projectId];
                installedAddons.TryRemove(_projectId);
                addon?.RemoveFromDir(_services.DirectoryService.GetInstancePath(instanceId));
                installedAddons.Save();
            }

            IsInstalled = false;
            _synchronizer.RemoveInstalledAddon(this);
        }

        public DownloadAddonRes Update()
        {
            var stateData = new DynamicStateData<SetValues<InstanceAddon, DownloadAddonRes>, InstallAddonState>();

            var result = DownloadAddonRes.unknownError;
            stateData.StateChanged += (arg, state) =>
            {
                if (state == InstallAddonState.EndDownload)
                {
                    result = arg.Value2;
                }
            };

            InstallLatestVersion(stateData.GetHandler, false);

            if (result == DownloadAddonRes.Successful)
                UpdateAvailable = false;

            return result;
        }

        public void InstallLatestVersion(DynamicStateHandler<SetValues<InstanceAddon, DownloadAddonRes>, InstallAddonState> stateHandler, bool downloadDependencies = true, bool isDependencie = false, IEnumerable<Modloader> acceptableModloaders = null)
        {
            if (_addonPrototype == null)
            {
                stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>
                {
                    Value1 = this,
                    Value2 = DownloadAddonRes.ProjectDataError
                }, InstallAddonState.EndDownload);
                return;
            }

            IsInstalling = true;
            _addonPrototype.SetAcceptableModloaders(acceptableModloaders);
            _addonPrototype.DefineLatestVersion();
            InstallAddon(downloadDependencies, stateHandler, isDependencie, acceptableModloaders);
            IsInstalling = false;
        }

        public void InstallSpecificVersion(DynamicStateHandler<SetValues<InstanceAddon, DownloadAddonRes>, InstallAddonState> stateHandler, object versionInfo, bool downloadDependencies = true, bool isDependencie = false, IEnumerable<Modloader> acceptableModloaders = null)
        {
            IsInstalling = true;
            _addonPrototype.SetAcceptableModloaders(acceptableModloaders);
            _addonPrototype.DefineSpecificVersion(versionInfo);
            InstallAddon(downloadDependencies, stateHandler, isDependencie, acceptableModloaders);
            IsInstalling = false;
        }

        public IDictionary<string, object> GetAllVersion(IEnumerable<Modloader> acceptableModloaders = null)
        {
            _addonPrototype.SetAcceptableModloaders(acceptableModloaders);
            return _addonPrototype.GetAllVersions();
        }

        internal void LoadAdditionalData(string logoUrl)
        {
            Categories = _addonPrototype.LoadCategories();
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                WebsiteUrl = _addonPrototype.LoadWebsiteUrl();
                DownloadLogo(logoUrl);
            });
        }

        /// <summary>
        /// Отменяет скачивание аддона.
        /// </summary>
        public void CancellDownload()
        {
            _cancelTokenSource?.Cancel();
        }

        private void InstallAddon(bool downloadDependencies, DynamicStateHandler<SetValues<InstanceAddon, DownloadAddonRes>, InstallAddonState> stateHandler, bool isDependencie, IEnumerable<Modloader> acceptableModloaders)
        {
            Runtime.DebugWrite($"Download {Name}");
            // если такой аддон уже скачивается - выходим нахуй
            if (!isDependencie && _synchronizer.CheckAddonInstalling(_addonPrototype.ProjectId)) return;

            IsLoading = true;

            _cancelTokenSource = new CancellationTokenSource();
            stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>
            {
                Value1 = this,
                Value2 = DownloadAddonRes.Successful
            }, InstallAddonState.StartDownload);

            _synchronizer.AddonInstallingStarted();

            string instanceId = _modpackInfo.LocalId;
            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId, _services.DataFilesService))
            {
                string addonKey = _addonPrototype.ProjectId;

                _synchronizer.InstallingSemaphore.WaitOne(addonKey);
                _synchronizer.InstallingAddons[addonKey] = new Pointer<InstanceAddon>
                {
                    Point = this
                };
                _synchronizer.InstallingSemaphore.Release(addonKey);

                var taskArgs = new TaskArgs
                {
                    PercentHandler = delegate (int percentages)
                    {
                        DownloadPercentages = percentages;
                    },
                    CancelToken = _cancelTokenSource.Token
                };

                if (_cancelTokenSource.Token.IsCancellationRequested)
                {
                    stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>
                    {
                        Value1 = this,
                        Value2 = DownloadAddonRes.IsCanselled
                    }, InstallAddonState.EndDownload);

                    _synchronizer.AddonInstallingFinished();
                    IsLoading = false;
                    return;
                }

                //так же скачиваем зависимости
                List<AddonDependencie> dependencies = _addonPrototype.Dependecies;
                if (dependencies.Count > 0 && downloadDependencies)
                {
                    foreach (var dependencie in dependencies)
                    {
                        if (installedAddons.ContainsKey(dependencie.AddonId)) continue;

                        Lexplosion.Runtime.TaskRun(delegate ()
                        {
                            string modId = dependencie.AddonId;

                            // если такой аддон уже скачивается - выходим нахуй
                            if (_synchronizer.CheckAddonInstalling(modId)) return;

                            var addonPointer = new Pointer<InstanceAddon>();
                            addonPointer.Point = null;
                            _synchronizer.ChacheSemaphore.WaitOne();
                            if (_synchronizer.AddonsCatalogChacheContains(modId))
                            {
                                addonPointer.Point = _synchronizer.AddonsCatalogChache[modId];
                                addonPointer.Point.IsInstalling = true;
                            }
                            _synchronizer.ChacheSemaphore.Release();

                            _synchronizer.InstallingSemaphore.WaitOne(modId);
                            _synchronizer.InstallingAddons[modId] = addonPointer;
                            _synchronizer.InstallingSemaphore.Release(modId);

                            InstanceAddon addonInstance;
                            if (addonPointer.Point == null)
                            {
                                IPrototypeAddon addon = dependencie.AddonPrototype;
                                addon.DefineDefaultVersion();

                                _synchronizer.InstallingSemaphore.WaitOne(modId);
                                addonInstance = new InstanceAddon(Type, _synchronizer, _services, addon, _modpackInfo);
                                _synchronizer.InstallingSemaphore.Release(modId);
                            }
                            else
                            {
                                addonInstance = addonPointer.Point;
                            }

                            addonInstance.InstallLatestVersion(stateHandler, true, true, acceptableModloaders);
                        });
                    }
                }

                var ressult = _addonPrototype.Install(taskArgs);
                if (ressult.Value2 != DownloadAddonRes.Successful)
                {
                    stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>
                    {
                        Value1 = this,
                        Value2 = ressult.Value2
                    }, InstallAddonState.EndDownload);

                    _synchronizer.AddonInstallingFinished();

                    IsLoading = false;
                    return;
                }

                FileName = Path.GetFileName(ressult.Value1.ActualPath);
                IsInstalling = false;

                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    WebsiteUrl = _addonPrototype.LoadWebsiteUrl();
                });

                _synchronizer.InstallingSemaphore.WaitOne(addonKey);
                _synchronizer.InstallingAddons.TryRemove(addonKey, out _);
                _synchronizer.InstallingSemaphore.Release(addonKey);

                if (ressult.Value2 == DownloadAddonRes.Successful)
                {
                    IsInstalled = true;

                    if (_cancelTokenSource.Token.IsCancellationRequested)
                    {
                        stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>
                        {
                            Value1 = this,
                            Value2 = DownloadAddonRes.Successful
                        }, InstallAddonState.EndDownload);

                        _synchronizer.AddonInstallingFinished();

                        IsLoading = false;
                        return;
                    }

                    // удаляем старый файл
                    if (installedAddons[_addonPrototype.ProjectId] != null)
                    {
                        if (installedAddons[_addonPrototype.ProjectId].ActualPath != ressult.Value1.ActualPath)
                        {
                            try
                            {
                                string path = _services.DirectoryService.InstancesPath + instanceId + "/";
                                InstalledAddonInfo installedAddon = installedAddons[_addonPrototype.ProjectId];
                                if (installedAddon.IsExists(path))
                                {
                                    installedAddon.RemoveFromDir(path);
                                }
                            }
                            catch { }
                        }

                        if (installedAddons[_addonPrototype.ProjectId].IsDisable)
                        {
                            try
                            {
                                string dir = _services.DirectoryService.GetInstancePath(instanceId);
                                File.Move(dir + ressult.Value1.ActualPath, dir + ressult.Value1.Path + DISABLE_FILE_EXTENSION);
                                ressult.Value1.IsDisable = true;

                                FileName = Path.GetFileName(ressult.Value1.ActualPath);
                            }
                            catch { }
                        }
                    }

                    installedAddons[_addonPrototype.ProjectId] = ressult.Value1;
                    _synchronizer.AddInstalledAddon(this);
                }
                else
                {
                    if (_cancelTokenSource.Token.IsCancellationRequested)
                    {
                        stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>()
                        {
                            Value1 = this,
                            Value2 = DownloadAddonRes.IsCanselled
                        }, InstallAddonState.EndDownload);
                    }
                    else
                    {
                        stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>()
                        {
                            Value1 = this,
                            Value2 = ressult.Value2
                        }, InstallAddonState.EndDownload);
                    }

                    _synchronizer.AddonInstallingFinished();

                    IsLoading = false;
                    return;
                }

                installedAddons.Save();
            }

            _cancelTokenSource = null;
            stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>()
            {
                Value1 = this,
                Value2 = DownloadAddonRes.Successful
            }, InstallAddonState.EndDownload);

            _synchronizer.AddonInstallingFinished();
            IsLoading = false;
        }

        /// <summary>
        /// Ну бля, качает заглавную картинку (лого) по ссылке и записывает в переменную Logo. Делает это всё в пуле потоков.
        /// </summary>
        private void DownloadLogo(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Proxy = null;
                    var image = webClient.DownloadData(url);
                    if (image != null)
                    {
                        try
                        {
                            Logo = ImageTools.ResizeImage(image, 80, 80);
                        }
                        catch
                        {
                            Logo = image;
                        }
                    }
                }
            }
            catch { }
        }

        private void Disable()
        {
            string projectID = _projectId;
            string instanceId = _modpackInfo.LocalId;

            using (InstalledAddons addons = InstalledAddons.Get(instanceId, _services.DataFilesService))
            {
                addons.DisableAddon(projectID, !_isEnable, delegate (InstalledAddonInfo data)
                {
                    try
                    {
                        string dir = _services.DirectoryService.InstancesPath + instanceId + "/";
                        if (data.IsExists(dir))
                        {
                            File.Move(dir + data.Path + DISABLE_FILE_EXTENSION, dir + data.Path);
                        }
                    }
                    catch { }
                },
                delegate (InstalledAddonInfo data)
                {
                    try
                    {
                        string dir = _services.DirectoryService.InstancesPath + instanceId + "/";
                        if (data.IsExists(dir))
                        {
                            File.Move(dir + data.Path, dir + data.Path + DISABLE_FILE_EXTENSION);
                        }
                    }
                    catch { }
                });

                FileName = Path.GetFileName(addons[projectID]?.ActualPath);

                addons.Save();
            }
        }
    }
}
