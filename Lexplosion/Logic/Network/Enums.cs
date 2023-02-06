using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network
{

    /// <summary>
    /// Статус онлайн игры
    /// </summary>
    public enum OnlineGameStatus
    {
        None,
        OpenWorld,
        ConnectedToUser
    }

    /// <summary>
    /// Состояние системы онлайны игры
    /// </summary>
    public enum SystemState
    {
        Normal,
        ServerNotAvailable
    }
}
