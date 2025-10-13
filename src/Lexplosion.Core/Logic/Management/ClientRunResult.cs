using System.Collections.Generic;

namespace Lexplosion.Logic.Management
{
    public struct ClientRunResult
    {
        public ClientInitResult InitResult;
        public bool RunResult;

        public ClientRunResult(InstanceInit state, List<string> downloadErrors, bool runResult)
        {
            InitResult = new ClientInitResult(state, downloadErrors);
            RunResult = runResult;
        }
    }
}
