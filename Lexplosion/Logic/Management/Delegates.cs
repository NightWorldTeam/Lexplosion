using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management
{
    public delegate void ProgressHandlerCallback(DownloadStageTypes stageType, int stagesCount, int stage, int procents);
    public delegate void ComplitedDownloadCallback(InstanceInit result, List<string> downloadErrors, bool launchGame);
    public delegate void ComplitedLaunchCallback(string instanceId, bool successful);
    public delegate void GameExitedCallback(string instanceId);
}
