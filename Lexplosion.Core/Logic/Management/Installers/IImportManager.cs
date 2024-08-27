using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Installers
{
    interface IImportManager
    {
        public InstanceInit Prepeare(ProgressHandlerCallback progressHandler, out PrepeareResult result);

        public InstanceInit Import(ProgressHandlerCallback progressHandler, string instanceId, out IReadOnlyCollection<string> errors);

        public void SetInstanceId(string id);
    }

    public struct PrepeareResult
    {
        public string Name;
    }
}
