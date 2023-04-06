using Lexplosion.Common.Models.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Common.Models.Controllers
{
    public sealed class ShareController
    {
        #region Singleton


        private static readonly ShareController _instance = new ShareController();
        public static ShareController Instance => _instance;


        #endregion Singleton


        private ObservableCollection<FileDistributionWrapper> _activeShareProcess = new ObservableCollection<FileDistributionWrapper>();
        public IEnumerable<FileDistributionWrapper> ActiveShareProcess => _activeShareProcess;


        private ObservableCollection<InstanceDistribution> _fileReceivers = new ObservableCollection<InstanceDistribution>();
        private Dictionary<string, InstanceDistribution> _receiverIdByInstanceDistribution = new Dictionary<string, InstanceDistribution>(); 
        public IEnumerable<InstanceDistribution> FileReceivers => _fileReceivers;


        #region Constructors


        private ShareController() 
        {
            
        }


        #endregion Constructors

        
        // TODO: придумать более быстрый способ хранения и обратки данных.

        public void AddActiveShareProcess(FileDistributionWrapper fileDistributionWrapper) 
        {
            _activeShareProcess.Add(fileDistributionWrapper);
        }

        public void RemoveActiveShareProcess(FileDistributionWrapper fileDistributionWrapper) 
        {
            _activeShareProcess.Remove(fileDistributionWrapper);
        }

        public void AddFileReceiver(InstanceDistribution fileReceiver) 
        {
            _receiverIdByInstanceDistribution.Add(fileReceiver.Id, fileReceiver);
            _fileReceivers.Add(fileReceiver);
        }

        public void RemoveFileReceiver(InstanceDistribution fileReceiver)
        {
            _receiverIdByInstanceDistribution.Remove(fileReceiver.Id);
            _fileReceivers.Remove(fileReceiver);
        }

        public void RemoveFileReceiver(string id)
        {
            var fileReceiver = _receiverIdByInstanceDistribution[id];
            _receiverIdByInstanceDistribution.Remove(id);
            _fileReceivers.Remove(fileReceiver);
        }

        public bool IsReceiverContains(string id) 
        {
            return _receiverIdByInstanceDistribution.ContainsKey(id);
        }
    }
}
