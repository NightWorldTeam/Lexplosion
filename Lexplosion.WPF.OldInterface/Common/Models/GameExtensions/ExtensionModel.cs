using Lexplosion.Logic.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.Common.Models.GameExtensions
{
    public abstract class ExtensionModel : VMBase, IExtensionModel
    {
        private static readonly IDictionary<GameExtension, ConcurrentDictionary<string, (bool, IEnumerable<string>)>> _extensionVersions;

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
        private bool _isAvaliable;
        public bool IsAvaliable
        {
            get => _isAvaliable; private set
            {
                _isAvaliable = value;
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


        public bool IsForgeAvaliable
        {
            get => _extensionVersions[GameExtension.Forge].ContainsKey(GameVersion) ? _extensionVersions[GameExtension.Forge][GameVersion].Item1 : false;
        }


        public bool IsFabricAvaliable
        {
            get => _extensionVersions[GameExtension.Fabric].ContainsKey(GameVersion) ? _extensionVersions[GameExtension.Fabric][GameVersion].Item1 : false;
        }

        public bool IsQuiltAvaliable
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

            IsAvaliable = CheckExistsOnVersion(GameVersion, extension);

            Lexplosion.Runtime.TaskRun(() =>
            {
                Versions = LoadExtensionVersions(extension, GameVersion);
                var versionList = Versions.ToList();
                if (versionList.Count > 0)
                {
                    Version = versionList[0];
                    IsAvaliable = true;
                    OnPropertiesChanged();
                }
                else
                {
                    IsAvaliable = false;
                    OnPropertiesChanged();
                }
            });

            OnPropertiesChanged();
        }


        #endregion Constructors


        #region Private Methods


        public static bool CheckExistsOnVersion(string gameVersion, GameExtension extension)
        {
            ushort[] version = gameVersion.Split('.').Select(ushort.Parse).ToArray<ushort>();

            // TODO: Сделать unit тестирование
            try
            {
                switch (extension)
                {
                    case GameExtension.Forge: return version[0] >= 1 && version[1] >= 1;
                    case GameExtension.Fabric: return version[0] >= 1 && version[1] >= 13;
                    case GameExtension.Quilt:
                        {
                            if (version.Length > 2)
                                return version[0] >= 1 && version[1] >= 14 && version[2] >= 4;
                            else if (version.Length > 1) return version[0] >= 1 && version[1] >= 15;
                            throw new Exception("Wrong Version Length");
                        }
                    default: return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

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
            OnPropertyChanged(nameof(IsForgeAvaliable));
            OnPropertyChanged(nameof(IsFabricAvaliable));
            OnPropertyChanged(nameof(IsQuiltAvaliable));
        }


        #endregion Private Methods
    }
}
