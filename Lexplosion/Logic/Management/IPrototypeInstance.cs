using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management
{
    interface IPrototypeInstance
    {
        InstanceInit Check();
        InitData Update(); // TODO: сделать так, чтобы при неудачном скачивании некоторых файлов он сохранял результат работы, чтобы не перекачивать всё снова
    }
}
