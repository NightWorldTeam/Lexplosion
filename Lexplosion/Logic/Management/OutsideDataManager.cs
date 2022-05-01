using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

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
            Console.WriteLine("UploadInstances " + pageIndex);
            List<string> CategoriesListConverter(List<Category> categories)
            {
                List<string> znfvrdfga = new List<string>();
                foreach (var c in categories)
                {
                    znfvrdfga.Add(c.name);
                }

                return znfvrdfga;
            }

            List<OutsideInstance> Instances = new List<OutsideInstance>();

            List<AutoResetEvent> events = new List<AutoResetEvent>();

            if (type == InstanceSource.Nightworld)
            {
                Dictionary<string, NWInstanceInfo> nwInstances = NightWorldApi.GetInstancesList();

                int i = 0;
                foreach (string nwModpack in nwInstances.Keys)
                {
                    if (i < pageSize * (pageIndex + 1))
                    {
                        OutsideInstance instanceInfo = new OutsideInstance()
                        {
                            Name = nwInstances[nwModpack].name ?? "Uncnown name",
                            InstanceAssets = new InstanceAssets()
                            {
                                author = nwInstances[nwModpack].author ?? "",
                                description = nwInstances[nwModpack].description ?? "",
                            },
                            MainImage = null,
                            Categories = nwInstances[nwModpack].categories ?? new List<string>(),
                            DownloadCount = 0,
                            Type = InstanceSource.Nightworld,
                            Id = nwModpack
                        };

                        var e = new AutoResetEvent(false);
                        events.Add(e);
                        ThreadPool.QueueUserWorkItem(ImageDownload, new object[] { e, instanceInfo, nwInstances[nwModpack].mainImage });

                        instanceInfo.IsInstalled = UserData.Instances.ExternalIds.ContainsKey(nwModpack);

                        if (instanceInfo.IsInstalled)
                        {
                            instanceInfo.LocalId = UserData.Instances.ExternalIds[nwModpack];
                            instanceInfo.UpdateAvailable = ManageLogic.CheckIntanceUpdates(UserData.Instances.Record[UserData.Instances.ExternalIds[nwModpack]].Name, InstanceSource.Nightworld);
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
                    string author = "";
                    if (instance.authors != null && instance.authors.Count > 0 && instance.authors[0].name != null)
                    {
                        author = instance.authors[0].name;
                    }

                    OutsideInstance instanceInfo = new OutsideInstance()
                    {
                        Name = instance.name ?? "Uncnown name",
                        InstanceAssets = new InstanceAssets()
                        {
                            author = (instance.authors != null && instance.authors.Count > 0) ? instance.authors[0].name : "Unknown",
                            description = instance.summary,
                        },
                        MainImage = null, // TODO: если картинки не найдено тут нулл и останется
                        Categories = CategoriesListConverter(instance.categories),
                        DownloadCount = instance.downloadCount,
                        Type = InstanceSource.Curseforge,
                        Id = instance.id.ToString()
                    };

                    if (instance.attachments != null && instance.attachments.Count > 0)
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

                        var e = new AutoResetEvent(false);
                        events.Add(e);
                        ThreadPool.QueueUserWorkItem(ImageDownload, new object[] { e, instanceInfo, url });
                    }

                    instanceInfo.IsInstalled = UserData.Instances.ExternalIds.ContainsKey(instance.id.ToString());

                    if (instanceInfo.IsInstalled)
                    {
                        instanceInfo.LocalId = UserData.Instances.ExternalIds[instance.id.ToString()];
                        instanceInfo.UpdateAvailable = ManageLogic.CheckIntanceUpdates(UserData.Instances.Record[UserData.Instances.ExternalIds[instance.id.ToString()]].Name, InstanceSource.Curseforge);
                    }

                    Instances.Add(instanceInfo);
                }
            }

            foreach (var e in events)
            {
                e.WaitOne();
            }

            Console.WriteLine("UploadInstances End " + pageIndex);

            return Instances;
        }

        private static void ImageDownload(object state)
        {
            object[] array = state as object[];

            AutoResetEvent e = (AutoResetEvent)array[0];
            OutsideInstance instanceInfo = (OutsideInstance)array[1];
            string url = (string)array[2];

            try
            {
                using (var webClient = new WebClient())
                {
                    instanceInfo.MainImage = webClient.DownloadData(url);
                }
            }
            catch
            {
                instanceInfo.MainImage = null;
            }

            e.Set();
        }

        private static void ChangePages(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter = "")
        {
            //Console.WriteLine("filter " + searchFilter);
            if (pageIndex > PageIndex)
            {
                uploadedInstances[type].Back = uploadedInstances[type].This;
                uploadedInstances[type].This = uploadedInstances[type].Next;
                uploadedInstances[type].Next = null;
                uploadedInstances[type].Next = UploadInstances(type, pageSize, pageIndex + 1, categoriy, searchFilter);
                //Console.WriteLine("Next " + (uploadedInstances[InstanceSource.Curseforge].Next == null) + " " + pageIndex + " " + PageIndex);
            }
            else if (pageIndex < PageIndex)
            {
                uploadedInstances[type].Next = uploadedInstances[type].This;
                uploadedInstances[type].This = uploadedInstances[type].Back;
                uploadedInstances[type].Back = null;
                uploadedInstances[type].Back = pageIndex > 0 ? UploadInstances(type, pageSize, pageIndex - 1, categoriy, searchFilter) : null;
                //Console.WriteLine("Back " + (uploadedInstances[InstanceSource.Curseforge].Back == null).ToString() + " " + pageIndex + " " + PageIndex);
            }
            else
            {
                uploadedInstances[type].Next = null;
                uploadedInstances[type].Back = null;
                uploadedInstances[type].Next = UploadInstances(type, pageSize, pageIndex + 1, categoriy, searchFilter);
                uploadedInstances[type].Back = pageIndex > 0 ? UploadInstances(type, pageSize, pageIndex - 1, categoriy, searchFilter) : null;
            }
        }

        public static List<OutsideInstance> GetInstances(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter = "")
        {
            Console.WriteLine("CLICK");
            List<OutsideInstance> page;
            if (Math.Abs(pageIndex - PageIndex) > 1)
            {
                PageIndex = pageIndex;
            }

            if (pageIndex > PageIndex)
            {
                page = uploadedInstances[type].Next;
            }
            else if (pageIndex < PageIndex)
            {
                page = uploadedInstances[type].Back;
            }
            else
            {
                page = UploadInstances(type, pageSize, pageIndex, ModpacksCategories.All, searchFilter);
                WaitUpload.WaitOne();
                uploadedInstances[type].This = page;
            }

            if (page != null)
            {
                WaitUpload.Reset();
                //Console.WriteLine("A " + (page == null).ToString());
            }
            else
            {
                WaitUpload.WaitOne();
                page = pageIndex >= PageIndex ? uploadedInstances[type].Next : uploadedInstances[type].Back;
                //Console.WriteLine("B " + (page == null).ToString());
            }

            Lexplosion.Run.TaskRun(delegate ()
            {
                ChangePages(type, pageSize, pageIndex, categoriy, searchFilter);
                PageIndex = pageIndex;
                WaitUpload.Set();
            });

            return page;
        }
    }
}
