using DiscordRPC.Events;
using Lexplosion.Common.Models.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Common.Models.Controllers
{
    public sealed class ShareController : VMBase
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

        private int _receiversCount;
        public int ReceiversCount 
        {
            get => _receiversCount; set 
            {
                _receiversCount = value;
                OnPropertyChanged();
            }
        }

        private int _activeShareProcessCount;
        public int ActiveShareProcessCount
        {
            get => _activeShareProcessCount; set
            {
                _activeShareProcessCount = value;
                OnPropertyChanged();
            }
        }


        #region Constructors


        private ShareController() 
        {
            
        }


        #endregion Constructors

        
        // TODO: придумать более быстрый способ хранения и обратки данных.

        public void AddActiveShareProcess(FileDistributionWrapper fileDistributionWrapper) 
        {
            _activeShareProcess.Add(fileDistributionWrapper);
            ActiveShareProcessCount = _activeShareProcess.Count;
        }

        public void RemoveActiveShareProcess(FileDistributionWrapper fileDistributionWrapper) 
        {
            _activeShareProcess.Remove(fileDistributionWrapper);
            ActiveShareProcessCount = _activeShareProcess.Count;
        }

        public void AddFileReceiver(InstanceDistribution fileReceiver) 
        {
            _receiverIdByInstanceDistribution.Add(fileReceiver.Id, fileReceiver);
            _fileReceivers.Add(fileReceiver);
            ReceiversCount = _fileReceivers.Count;
        }

        public void RemoveFileReceiver(InstanceDistribution fileReceiver)
        {
            _receiverIdByInstanceDistribution.Remove(fileReceiver.Id);
            _fileReceivers.Remove(fileReceiver);
            ReceiversCount = _fileReceivers.Count;
        }

        public void RemoveFileReceiver(string id)
        {
            var fileReceiver = _receiverIdByInstanceDistribution[id];
            _receiverIdByInstanceDistribution.Remove(id);
            _fileReceivers.Remove(fileReceiver);
            ReceiversCount = _fileReceivers.Count;
        }

        public bool IsReceiverContains(string id) 
        {
            return _receiverIdByInstanceDistribution.ContainsKey(id);
        }

        public void RemoveAllActiveShareProcess() 
        {
            _activeShareProcess.Clear();
            ActiveShareProcessCount = 0;
        }
    }
}
