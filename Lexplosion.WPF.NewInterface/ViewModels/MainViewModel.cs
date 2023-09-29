using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.Tools;
using Lexplosion.WPF.NewInterface.Models.InstanceCatalogControllers;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile;
using Lexplosion.WPF.NewInterface.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.ViewModels
{
    public sealed class UserData : VMBase
    {
        public static UserData Instance { get; } = new UserData();


        private bool _isAuthrized;
        public bool IsAuthrized
        {
            get => _isAuthrized; set
            {
                _isAuthrized = value;
                OnPropertyChanged();
            }
        }

        private AccountType _currentAccountType;
        public AccountType CurrentAccountType
        {
            get => _currentAccountType; set
            {
                _currentAccountType = value;
                OnPropertyChanged();
            }
        }


        private string _nickname;
        public string Nickname
        {
            get => _nickname; set
            {
                _nickname = value;
                OnPropertyChanged();
            }
        }



        #region Constructors


        private UserData()
        {

        }


        #endregion Constructors
    }


    public readonly struct MinecraftVersion : IComparable<MinecraftVersion>, IEquatable<MinecraftVersion>
    {
        public enum VersionType
        {
            Release,
            Snapshot
        }

        public string Id { get; }
        public VersionType Type { get; }


        #region Constructors


        public MinecraftVersion(string id, VersionType versionType)
        {
            Id = id;
            Type = versionType;
        }


        #endregion Constructors


        #region Public Methods


        public static MinecraftVersion Parse(string str) 
        {


            return new MinecraftVersion();
        }

        public override string ToString()
        {
            return Type.ToString() + " " + Id;
        }

        public int CompareTo(MinecraftVersion other)
        {
            return (Id, Type).CompareTo((other.Id, other.Type));
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CombineHashCodes(Id.GetHashCode(), Type.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is MinecraftVersion)) 
                return false;

            return Equals((MinecraftVersion)obj);
        }

        public bool Equals(MinecraftVersion other)
        {
            return this.Id == other.Id && this.Type == other.Type;
        }


        #endregion Public Methods
    }


    public sealed class MainViewModel : VMBase
    {
        #region Properties


        /// <summary>
        /// Навигационное хранилище, хранит активный viewmodel для окна.
        /// </summary>
        internal INavigationStore NavigationStore { get; } = new NavigationStore();

        /// <summary>
        /// Выбранный в данный момент viewmodel для окна. 
        /// </summary>
        public ViewModelBase CurrentViewModel => NavigationStore.CurrentViewModel;

        /// <summary>
        /// Выбранный в данный момент viewmodel для модального окна.
        /// </summary>
        public IModalViewModel CurrentModalViewModel => ModalNavigationStore.Instance.CurrentViewModel;

        /// <summary>
        /// Открыто ли модальное окно.
        /// </summary>
        public bool IsModalOpen { get => ModalNavigationStore.Instance.CurrentViewModel != null; }



        // Данное свойство содержит в себе версии игры.
        // Является static, т.к эксемпляр MainViewModel создаётся в единственном эксемляре, в начале запуска лаунчер, до появляния начального окна.

        /// <summary>
        /// Все версии без снапшотов.
        /// </summary>
        public static string[] ReleaseGameVersions { get; private set; }
        public static MinecraftVersion[] ReleaseGameVersions1 { get; private set; }

        /// <summary>
        /// Все версии включая снапшоты.
        /// </summary>
        public static string[] AllGameVersions { get; private set; }
        public static MinecraftVersion[] AllGameVersions1 { get; private set; }


        #endregion Properties

        public MainViewModel()
        {
            PreLoadGameVersions();
            PreLoadGameVersionsStructs();

            ModalNavigationStore.Instance.CurrentViewModelChanged += Instance_CurrentViewModelChanged;
            ModalNavigationStore.Instance.Open(new LeftMenuControl(
                new ModalLeftMenuTabItem[3]
                {
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "AddCircle",
                        TitleKey = "Create",
                        IsEnable = true,
                        IsSelected = true
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "PlaceItem",
                        TitleKey = "Import",
                        IsEnable = true,
                        IsSelected = true
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "DownloadCloud",
                        TitleKey = "Distributions",
                        IsEnable = true,
                        IsSelected = true
                    }
                }
                ));

            ModalNavigationStore.Instance.Close();
            ModalNavigationStore.Instance.Open(new DialogBoxViewModel("Library", "Protection", (obj) => { }, (obj) => { }));
            ModalNavigationStore.Instance.Close();

            NavigationStore.CurrentViewModelChanged += NavigationStore_CurrentViewModelChanged;
            //NavigationStore.CurrentViewModel = new MainMenuLayoutViewModel(); 
            NavigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(LibraryController.Instance.Instances.Last());
                //new InstanceModelBase(InstanceClient.GetOutsideInstances( InstanceSource.Modrinth, 2, 0, new IProjectCategory[] { new SimpleCategory() { Name = "All", Id = "-1", ClassId = "", ParentCategoryId = "" }}, "", CfSortField.Featured, "1.19.4")[1])); //new MainMenuLayoutViewModel(); //new ModrinthRepositoryViewModel(AddonType.Mods, ClientType.Fabric, "1.19.4");
            //NavigationStore.Content = new AuthorizationMenuViewModel(NavigationStore);
        }

        private void Instance_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalViewModel));
            OnPropertyChanged(nameof(IsModalOpen));
        }

        private void NavigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        public static void ChangeColor(Color color)
        {
        }


        #region Private Methods


        /// <summary>
        /// Метод в отдельном потоке загружает данные о версии игры.
        /// После данные заносятся в статичный неизменяемый лист [ImmutableList] - _releaseGameVersions. 
        /// </summary>
        private static void PreLoadGameVersions()
        {
            var releaseOnlyVersions = new List<string>();
            var allVersions = new List<string>();

            Lexplosion.Runtime.TaskRun(() =>
            {
                foreach (var v in ToServer.GetVersionsList())
                {
                    if (v.type == "release")
                    {
                        releaseOnlyVersions.Add(v.id);
                        allVersions.Add("release " + v.id);
                    }
                    else
                    {
                        allVersions.Add("snapshot " + v.id);
                    }

                }
                ReleaseGameVersions = releaseOnlyVersions.ToArray();
                AllGameVersions = allVersions.ToArray();
                releaseOnlyVersions.Clear();
                allVersions.Clear();
            });
        }

        private static void PreLoadGameVersionsStructs()
        {
            var releaseOnlyVersions = new List<MinecraftVersion>();
            var allVersions = new List<MinecraftVersion>();

            Lexplosion.Runtime.TaskRun(() =>
            {
                foreach (var v in ToServer.GetVersionsList())
                {
                    if (v.type == "release")
                    {
                        releaseOnlyVersions.Add(new MinecraftVersion(v.id, MinecraftVersion.VersionType.Release));
                        allVersions.Add(new MinecraftVersion(v.id, MinecraftVersion.VersionType.Release));
                    }
                    else
                    {
                        allVersions.Add(new MinecraftVersion(v.id, MinecraftVersion.VersionType.Snapshot));
                    }

                }
                ReleaseGameVersions1 = releaseOnlyVersions.ToArray();
                AllGameVersions1 = allVersions.ToArray();
                releaseOnlyVersions.Clear();
                allVersions.Clear();
            });
        }


        #endregion Private Methods
    }
}
