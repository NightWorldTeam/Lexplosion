using System.Collections.Generic;

namespace Lexplosion.Logic.Management
{
    public delegate void ProgressHandlerCallback(DownloadStageTypes stageType, ProgressHandlerArguments data);
    public delegate void ComplitedDownloadCallback(InstanceInit result, List<string> downloadErrors, bool launchGame);
    public delegate void ComplitedLaunchCallback(string instanceId, bool successful);
    public delegate void GameExitedCallback(string instanceId);

    public struct ProgressHandlerArguments
    {
        public int StagesCount;
        public int Stage;
        public int Procents;
        public int TotalFilesCount;
        public int FilesCount;
    }
}
