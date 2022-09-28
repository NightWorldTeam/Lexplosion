using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Tools
{

    /// <summary>
    /// Аргументы для как-йто долгой задачи.
    /// </summary>
    struct TaskArgs
    {
        /// <summary>
        /// Токен отмены
        /// </summary>
        public CancellationToken CancelToken;
        /// <summary>
        /// обработчки процентов.
        /// </summary>
        public Action<int> PercentHandler;
    }
}
