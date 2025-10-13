namespace Lexplosion.Logic.Network.Services
{
    public interface IWebServicesContainer
    {
        public ToServer WebService { get; }
        public MinecraftInfoService MinecraftService { get; }
    }
}
