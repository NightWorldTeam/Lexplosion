using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Management.Installers
{
    interface IInstallManager
    {
        InstanceInit Check(out string gameVersion);
        InitData Update(string javaPath, ProgressHandlerCallback progressHandler); // TODO: сделать так, чтобы при неудачном скачивании некоторых файлов он сохранял результат работы, чтобы не перекачивать всё снова

        event Action<string, int> FileDownloadEvent;
    }
}
