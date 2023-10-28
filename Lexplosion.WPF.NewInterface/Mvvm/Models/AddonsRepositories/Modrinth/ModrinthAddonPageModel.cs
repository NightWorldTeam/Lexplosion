using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Tools;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories
{
    public sealed class ModrinthAddonPageModel : ViewModelBase
    {
        private readonly InstanceAddon _instanceAddon;


        #region Properties


        public byte[] ImageBytes { get => _instanceAddon.Logo; }

        public string Name { get => _instanceAddon.Name; }
        public string Summary { get => _instanceAddon.Description; }
        public IEnumerable<CategoryBase> Categories { get => new List<CategoryBase>(); }

        public string DownloadsKey { get; }
        public string FollowersKey { get; }


        #endregion Proeprties


        #region Constructors


        public ModrinthAddonPageModel(InstanceAddon instanceAddon)
        {
            _instanceAddon = instanceAddon;

            var keysArrDownloads = new string[3] { "Downloads1", "Downloads2", "Downloads3" };
            var keysArrFollowers = new string[3] { "Followers1", "Followers2", "Followers3" };

            var v1 = (string)App.Current.Resources[keysArrDownloads[0]];

            DownloadsKey = "";
            FollowersKey = "";

            // если ключи совпадают, запускаем алгоритм подбора слова
            if (!(v1 == (string)App.Current.Resources[keysArrDownloads[1]] && v1 == (string)App.Current.Resources[keysArrDownloads[2]]))
            {
                DownloadsKey = WordHelper.GetWordKeyWithRightEndingForNumber(7130000, keysArrDownloads);
            }

            var _v1 = (string)App.Current.Resources[keysArrFollowers[0]];

            // если ключи совпадают, запускаем алгоритм подбора слова
            if (!(_v1 == (string)App.Current.Resources[keysArrFollowers[1]] && _v1 == (string)App.Current.Resources[keysArrFollowers[2]]))
            {
                FollowersKey = WordHelper.GetWordKeyWithRightEndingForNumber(6177, keysArrFollowers);
            }

            OnPropertyChanged(nameof(DownloadsKey));
            OnPropertyChanged(nameof(FollowersKey));
        }


        #endregion Constructors
    }

}
