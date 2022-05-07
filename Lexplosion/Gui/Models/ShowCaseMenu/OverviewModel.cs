using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.Models.ShowCaseMenu
{
    public class OverviewModel : VMBase
    {
        private readonly string _id;
        private readonly InstanceSource _source;
        private CurseforgeInstanceInfo _instanceInfo;

        public InstanceSource Source
        {
            get => _source;
        }

        public CurseforgeInstanceInfo Info
        {
            get => _instanceInfo; set
            {
                _instanceInfo = value;
                OnPropertyChanged("Info");
            }
        }

        public string GameVersion
        {
            get => Info.gameVersionLatestFiles[0].gameVersion;
        }

        public string LastUpdate
        {
            get => DateTime.Parse(Info.dateModified).ToString("dd MMM yyyy");
        }

        public string TotalDownloads
        {
            get => ((Int32)Info.downloadCount).ToString("##,#");
        }

        public string Description
        {
            get 
            {
                var x = Info.summary;
                Console.WriteLine(x);
                return x;
            }
        }

        public string ShortDescription
        {
            get => Info.summary;
        }

        public List<Category> Categories
        {
            get => Info.categories;
        }

        public List<string> ImagesLinks
        {
            get
            {
                var urls = new List<string>();
                foreach (var item in _instanceInfo.attachments)
                {
                    if (!item.isDefault && !item.url.Contains("avatars"))
                        urls.Add(item.url);
                }
                return urls;
            }
        }

        public string Modloader
        {
            get
            {
                switch (_instanceInfo.Modloader)
                {
                    case ModloaderType.None:
                        return "Vanilla";
                    case ModloaderType.Forge:
                        return "Forge";
                    case ModloaderType.Fabric:
                        return "Fabric";
                    default:
                        return "Не определенно";
                }
            }
        }

        public OverviewModel(string id, InstanceSource source)
        {
            _source = source;
            _id = id;

            if (_source == InstanceSource.Curseforge || _source == InstanceSource.Nightworld)
            {
                GetInstance();
            }
        }

        private void GetInstance()
        {
            switch (_source)
            {
                case InstanceSource.Curseforge:
                    Info = CurseforgeApi.GetInstance(_id);
                    break;
                case InstanceSource.Nightworld:
                    break;
                case InstanceSource.Local:
                    break;
            }
        }
    }
}

