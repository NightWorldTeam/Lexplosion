using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using System;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel
{
    public sealed class LaunchModel : ViewModelBase
    {
        private readonly InstanceClient _instanceClient;
        private NotifyCallback _notify;

        /// <summary>
        /// Игра запускается
        /// </summary>
        public event Action LaunchStarted;
        /// <summary>
        /// Результат запуска игры.<br/>
        /// bool - Успешно или нет.
        /// </summary>
        public event Action<bool> LaunchCompleted;
        /// <summary>
        /// Игра закрывается
        /// </summary>
        public event Action Closed;





        #region Constructors


        public LaunchModel(InstanceClient instanceClient, NotifyCallback notify)
        {

        }


        #endregion Constructors


        #region Public Methods





        #endregion Public Methods


        #region Private Methods





        #endregion Private Methods
    }
}
