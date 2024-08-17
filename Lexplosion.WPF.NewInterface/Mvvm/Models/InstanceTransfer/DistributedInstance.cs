using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer
{
    public class DistributedInstance : ObservableObject
    {
        private readonly DispatcherTimer _timer = new();
        private readonly FileDistributor _fileDistributor;
        private readonly ObservableCollection<Player> _тянучи;


        #region Properties


        public string Id { get; }
        public string Name { get; }

        public IEnumerable<Player> Тянучи { get => _тянучи; }


        private int _activityTime;
        public int ActivityTime
        {
            get => _activityTime; private set
            {
                _activityTime = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        public DistributedInstance(string instanceId, string name, FileDistributor fileDistributor)
        {
            _fileDistributor = fileDistributor;

            Id = instanceId;
            Name = name;

            _fileDistributor.UserConnected += (user) =>
            {
                _тянучи.Add(user);
            };

            _fileDistributor.UserDisconnected += (user) =>
            {
                _тянучи.Remove(user);
            };

            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            ActivityTime += 1;
        }

        public void Stop() 
        {
            _timer.Stop();
            _fileDistributor?.Stop();
        }
    }
}
