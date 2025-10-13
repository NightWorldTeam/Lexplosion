namespace Lexplosion.Logic.Network.Services
{
    public interface INightWorldWebServicesContainer : IWebServicesContainer
    {
        public NightWorldApi NwApi { get; }
    }
}
