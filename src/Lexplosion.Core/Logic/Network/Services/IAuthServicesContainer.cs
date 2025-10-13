using Lexplosion.Logic.Network.Web;

namespace Lexplosion.Logic.Network.Services
{
    public interface IAuthServicesContainer
    {
        public ToServer WebService { get; }
        public NightWorldApi NwApi { get; }
        public MojangApi MjApi { get; }
    }
}
