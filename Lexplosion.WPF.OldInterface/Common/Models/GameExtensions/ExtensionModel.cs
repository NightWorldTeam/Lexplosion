using Lexplosion.Logic.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Lexplosion.Common.Models.GameExtensions
{
    public abstract class ExtensionModel : VMBase, IExtensionModel
    {
        private static readonly IDictionary<GameExtension, ConcurrentDictionary<string, (bool, IEnumerable<string>)>> _extensionVersions;
        
        public event Action<bool> AvailiableChanged;

        private static readonly object _locker = new(); 

        #region Properties


        public string GameVersion { get; }
        public GameExtension GameExtension { get; set; }

        /// <summary>
        /// Включен ли модлоадер
        /// </summary>
        private bool _isEnable;
        public bool IsEnable
        {
            get => _isEnable; set
            {
                _isEnable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Доступно ли расшерение на эту версию игры.
        /// </summary>
        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable; private set
            {
                _isAvailable = value;
                AvailiableChanged?.Invoke(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Версия расширения;
        /// </summary>
        private string _version;
        public string Version
        {
            get => _version; set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Массив с версиями расширения
        /// </summary>
        private IEnumerable<string> _versions;
        public IEnumerable<string> Versions
        {
            get => _versions; private set
            {
                _versions = value;
                OnPropertyChanged();
            }
        }


        public bool IsForgeAvailable
        {
            get => _extensionVersions[GameExtension.Forge].ContainsKey(GameVersion) ? _extensionVersions[GameExtension.Forge][GameVersion].Item1 : false;
        }


        public bool IsFabricAvailable
        {
            get => _extensionVersions[GameExtension.Fabric].ContainsKey(GameVersion) ? _extensionVersions[GameExtension.Fabric][GameVersion].Item1 : false;
        }

        public bool IsQuiltAvailable
        {
            get => _extensionVersions[GameExtension.Quilt].ContainsKey(GameVersion) ? _extensionVersions[GameExtension.Quilt][GameVersion].Item1 : false;
        }


        #endregion Properties


        #region Constructors


        static ExtensionModel()
        {
            _extensionVersions = new Dictionary<GameExtension, ConcurrentDictionary<string, (bool, IEnumerable<string>)>>()
            {
                { GameExtension.Optifine, new ConcurrentDictionary<string, (bool,  IEnumerable<string>)>() },
                { GameExtension.Forge, new ConcurrentDictionary <string, (bool, IEnumerable< string>)>() },
                { GameExtension.Fabric, new ConcurrentDictionary <string, (bool, IEnumerable<string>)>() },
                { GameExtension.Quilt, new ConcurrentDictionary <string, (bool, IEnumerable<string>)>() },
            };
        }

        protected ExtensionModel(GameExtension extension, string gameVersion)
        {
            GameVersion = GetCorrectGameVersion(gameVersion);
            GameExtension = extension;

            ThreadPool.QueueUserWorkItem((state) =>
            {
                lock (_locker) 
                { 
                    Versions = LoadExtensionVersions(extension, GameVersion);
                    var versionList = Versions.ToList();
                    if (versionList.Count > 0)
                    {
                        Version = versionList[0];
                        IsAvailable = true;
                    }
                    else
                    {
                        IsAvailable = false;
                    }
                    OnPropertiesChanged();
                }
                OnPropertiesChanged();
            });
        }


        #endregion Constructors


        #region Private Methods


        public static string GetCorrectGameVersion(string gameVersion)
        {
            if (gameVersion.Length <= 8)
            {
                return gameVersion;
            }

            return gameVersion.Replace("snapshot ", "").Replace("release ", "");
        }


        #endregion Private Methods


        #region Public & Protected Methods


        /// <summary>
        /// Возвращает неизменяемый массив с версиями расширениями.
        /// Также сохраняет массив в словарь.
        /// </summary>
        /// <param name="GameExtension"></param>
        /// <param name="gameVersion"></param>
        /// <returns></returns>
        public static IEnumerable<string> LoadExtensionVersions(GameExtension extension, string gameVersion)
        {
            if (!_extensionVersions[extension].ContainsKey(gameVersion))
            {
                if (GameExtension.Optifine == extension)
                {
                    var list = ToServer.GetOptifineVersions(gameVersion);
                    _extensionVersions[extension].TryAdd(gameVersion, (list.Count > 0, list));
                }
                else
                {
                    var list = ToServer.GetModloadersList(gameVersion, (ClientType)extension);

                    _extensionVersions[extension].TryAdd(gameVersion, (list.Count > 0, list));
                }
                return _extensionVersions[extension][gameVersion].Item2;
            }

            return _extensionVersions[extension][gameVersion].Item2;
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void OnPropertiesChanged()
        {
            OnPropertyChanged(nameof(IsForgeAvailable));
            OnPropertyChanged(nameof(IsFabricAvailable));
            OnPropertyChanged(nameof(IsQuiltAvailable));
        }


        #endregion Private Methods
    }
}
