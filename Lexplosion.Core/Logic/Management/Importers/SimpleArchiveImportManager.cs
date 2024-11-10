using System;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Importers
{
    internal class SimpleArchiveImportManager : IImportManager
    {
        public int CompletedStagesCount { set => throw new NotImplementedException(); }

        public ImportResult Import(ProgressHandlerCallback progressHandler, out IReadOnlyCollection<string> errors)
        {
            throw new NotImplementedException();
        }

        public ImportResult Prepeare(ProgressHandlerCallback progressHandler, out PrepeareResult result)
        {
            throw new NotImplementedException();
        }

        public void SetInstanceId(string id)
        {
            throw new NotImplementedException();
        }
    }
}
