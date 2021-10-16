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
        class InstacesList
        {
            public List<OutsideInstance> Next = null;
            public List<OutsideInstance> This = null;
            public List<OutsideInstance> Back = null;
        }

        private static Dictionary<InstanceSource, InstacesList> uploadedInstances = new Dictionary<InstanceSource, InstacesList>();
        private static AutoResetEvent WaitUpload = new AutoResetEvent(false); //нужен для ожидания загрузки модпаков

        private static string SearchFilter = "";
        private static int PageIndex = -1;

        public static void DefineInstances()
        {
            uploadedInstances[InstanceSource.Curseforge] = new InstacesList();
            uploadedInstances[InstanceSource.Nightworld] = new InstacesList();

            uploadedInstances[InstanceSource.Curseforge].Next = UploadInstances(InstanceSource.Curseforge, 10, 0, ModpacksCategories.All);
            uploadedInstances[InstanceSource.Nightworld].Next = UploadInstances(InstanceSource.Nightworld, 10, 0, ModpacksCategories.All);
        }

        private static List<OutsideInstance> UploadInstances(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter = "")
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
                    byte[] imageBytes = null;

                    if (instance.attachments != null && instance.attachments.Count > 0)
                    {
                        using (var webClient = new WebClient())
                        {
                            string url = instance.attachments[0].thumbnailUrl;
                            foreach (var attachment in instance.attachments)
                            {
                                if (attachment.isDefault)
                                {
                                    url = attachment.thumbnailUrl;
                                    break;
                                }
                            }
                            imageBytes = webClient.DownloadData(url);
                        }
                    }

                    string author = "";
                    if (instance.authors != null && instance.authors.Count > 0 && instance.authors[0].name != null)
                    {
                        author = instance.authors[0].name;
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

            return Instances;
        }

        private static void ChangePages(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter = "")
        {
            if (pageIndex > PageIndex)
            {
                uploadedInstances[InstanceSource.Curseforge].Back = uploadedInstances[type].This;
                uploadedInstances[InstanceSource.Curseforge].This = uploadedInstances[type].Next;
                uploadedInstances[InstanceSource.Curseforge].Next = null;
                uploadedInstances[InstanceSource.Curseforge].Next = UploadInstances(type, pageSize, pageIndex + 1, categoriy, searchFilter);
                Console.WriteLine("Next " + (uploadedInstances[InstanceSource.Curseforge].Next == null).ToString() + " " + pageIndex + " " + PageIndex);
            }
            else
            {
                uploadedInstances[InstanceSource.Curseforge].Next = uploadedInstances[type].This;
                uploadedInstances[InstanceSource.Curseforge].This = uploadedInstances[type].Back;
                uploadedInstances[InstanceSource.Curseforge].Back = null;
                uploadedInstances[InstanceSource.Curseforge].Back = pageIndex > 0 ? UploadInstances(type, pageSize, pageIndex - 1, categoriy, searchFilter) : null;
                Console.WriteLine("Back " + (uploadedInstances[InstanceSource.Curseforge].Back == null).ToString() + " " + pageIndex + " " + PageIndex);
            }
        }

        public static List<OutsideInstance> GetInstances(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter = "")
        {
            List<OutsideInstance> page;
            if (pageIndex > PageIndex)
            {
                if(SearchFilter == searchFilter)
                {
                    page = uploadedInstances[type].Next;
                }
                else
                {
                    page = UploadInstances(type, pageSize, pageIndex, ModpacksCategories.All, searchFilter);
                    SearchFilter = searchFilter;
                }
            }
            else
            {
                if (SearchFilter == searchFilter)
                {
                    page = uploadedInstances[type].Back;
                }
                else
                {
                    page = UploadInstances(type, pageSize, pageIndex, ModpacksCategories.All, searchFilter);
                    SearchFilter = searchFilter;
                }
            }

            if (page != null)
            {
                WaitUpload.Reset();
                Console.WriteLine("A " + (page == null).ToString());
            }
            else
            {
                WaitUpload.WaitOne();
                page = pageIndex > PageIndex ? uploadedInstances[type].Next : uploadedInstances[type].Back;
                Console.WriteLine("B " + (page == null).ToString());
            }

            Lexplosion.Run.ThreadRun(delegate ()
            {
                ChangePages(type, pageSize, pageIndex, categoriy, searchFilter);
                SearchFilter = searchFilter;
                PageIndex = pageIndex;
                WaitUpload.Set();
            });

            return page;
        }
    }
}
