using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management.Notifications;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.Network.Web;

namespace Lexplosion.Logic.Management
{
    public class AllServicesContainer : IAllFileServicesContainer, IAuthServicesContainer
    {
        public ToServer WebService { get; }

        public MinecraftInfoService MinecraftService { get; }

        public WithDirectory DirectoryService { get; }

        public DataFilesManager DataFilesService { get; }

        public CurseforgeApi CfApi { get; }

        public ModrinthApi MdApi { get; }

        public NightWorldApi NwApi { get; }

        public MojangApi MjApi { get; }

        public CategoriesManager CategoriesService { get; }
        public NotificationsManager NotificationsService { get;}

        public AllServicesContainer(ToServer webService, MinecraftInfoService minecraftService, WithDirectory directoryService, DataFilesManager dataFilesService, CurseforgeApi cfApi, ModrinthApi mdApi, NightWorldApi nwApi, MojangApi mjApi, CategoriesManager categoriesService, NotificationsManager notificationsService)
        {
            WebService = webService;
            MinecraftService = minecraftService;
            DirectoryService = directoryService;
            DataFilesService = dataFilesService;
            CfApi = cfApi;
            MdApi = mdApi;
            NwApi = nwApi;
            MjApi = mjApi;
            CategoriesService = categoriesService;
            NotificationsService = notificationsService;
        }
    }
}
