using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Lexplosion.Logic.Management
{
    static class OutsideDataManager
    {
        private static Dictionary<InstanceSource, List<OutsideInstance>> uploadedInstances = new Dictionary<InstanceSource, List<OutsideInstance>>();
        private static AutoResetEvent WaitUpload = new AutoResetEvent(false); //нужен для ожидания загрузки модпаков
        private static string SearchFilter = "";

        public static void DefineInstances()
        {
            UploadInstances(InstanceSource.Curseforge, 10, 0, ModpacksCategories.All);
            UploadInstances(InstanceSource.Nightworld, 10, 0, ModpacksCategories.All);
        }

        private static void UploadInstances(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter = "")
        {
            List<string> CategoriesListConverter(List<CurseforgeInstanceInfo.Category> categories)
            {
                List<string> znfvrdfga = new List<string>();
                foreach (var c in categories)
                {
                    znfvrdfga.Add(c.name);
                }

                return znfvrdfga;
            }

            List<OutsideInstance> Instances = new List<OutsideInstance>();

            if (type == InstanceSource.Nightworld)
            {
                Dictionary<string, NWInstanceInfo> nwInstances = NightWorldApi.GetInstancesList();

                int i = 0;
                foreach (string nwModpack in nwInstances.Keys)
                {
                    if (i < pageSize * (pageIndex + 1))
                    {
                        byte[] imageBytes;
                        using (var webClient = new WebClient())
                        {
                            imageBytes = webClient.DownloadData(nwInstances[nwModpack].mainImage);
                        }

                        OutsideInstance instanceInfo = new OutsideInstance()
                        {
                            Name = nwInstances[nwModpack].name ?? "Uncnown name",
                            Author = nwInstances[nwModpack].author ?? "",
                            MainImage = imageBytes, // TODO: url до картинке может быть битым и не только тут
                            Categories = nwInstances[nwModpack].categories ?? new List<string>(),
                            Description = nwInstances[nwModpack].description ?? "",
                            DownloadCount = 0,
                            Type = InstanceSource.Nightworld,
                            Id = nwModpack
                        };

                        instanceInfo.IsInstalled = UserData.Instances.ExternalIds.ContainsKey(nwModpack);

                        if (instanceInfo.IsInstalled)
                        {
                            instanceInfo.UpdateAvailable = ManageLogic.CheckIntanceUpdates(UserData.Instances.List[UserData.Instances.ExternalIds[nwModpack]].Name, InstanceSource.Nightworld);
                        }

                        Instances.Add(instanceInfo);
                    }

                    i++;
                }
            }
            else if (type == InstanceSource.Curseforge)
            {
                List<CurseforgeInstanceInfo> curseforgeInstances = CurseforgeApi.GetInstances(pageSize, pageIndex * pageSize, ModpacksCategories.All, searchFilter);
                foreach (var instance in curseforgeInstances)
                {
                    byte[] imageBytes;
                    using (var webClient = new WebClient())
                    {
                        string url = instance.attachments[0].thumbnailUrl;
                        foreach(var attachment in instance.attachments)
                        {
                            if (attachment.isDefault)
                            {
                                url = attachment.thumbnailUrl;
                                break;
                            }
                        }

                        imageBytes = webClient.DownloadData(url);
                    }

                    OutsideInstance instanceInfo = new OutsideInstance()
                    {
                        Name = instance.name,
                        Author = instance.authors[0].name, // TODO: тут может быть null
                        MainImage = imageBytes, // TODO: тут тоже может быть null
                        Categories = CategoriesListConverter(instance.categories),
                        Description = instance.summary,
                        DownloadCount = instance.downloadCount,
                        Type = InstanceSource.Curseforge,
                        Id = instance.id.ToString()
                    };

                    instanceInfo.IsInstalled = UserData.Instances.ExternalIds.ContainsKey(instance.id.ToString());

                    if (instanceInfo.IsInstalled)
                    {
                        instanceInfo.UpdateAvailable = ManageLogic.CheckIntanceUpdates(UserData.Instances.List[UserData.Instances.ExternalIds[instance.id.ToString()]].Name, InstanceSource.Curseforge);
                    }

                    Instances.Add(instanceInfo);
                }
            }

            uploadedInstances[type] = Instances;
            WaitUpload.Set();
        }

        public static List<OutsideInstance> GetInstances(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter = "")
        {
            if(SearchFilter != searchFilter)
            {
                SearchFilter = searchFilter;

                UploadInstances(type, pageSize, pageIndex, categoriy, searchFilter);
                var UploadedOutsideInstances_ = uploadedInstances[type];
                uploadedInstances[type] = null;

                Lexplosion.Run.ThreadRun(delegate ()
                {
                    UploadInstances(type, pageSize, pageIndex + 1, categoriy, searchFilter);
                });

                return UploadedOutsideInstances_;
            }

            if (uploadedInstances[type] != null)
            {
                WaitUpload.Reset();
                var UploadedOutsideInstances_ = uploadedInstances[type];
                uploadedInstances[type] = null;

                Lexplosion.Run.ThreadRun(delegate ()
                {
                    UploadInstances(type, pageSize, pageIndex + 1, categoriy, searchFilter);
                });

                return UploadedOutsideInstances_;
            }
            else
            {
                WaitUpload.WaitOne();
                Lexplosion.Run.ThreadRun(delegate ()
                {
                    UploadInstances(type, pageSize, pageIndex + 1, categoriy, searchFilter);
                });

                return uploadedInstances[type];
            }
        }
    }
}
