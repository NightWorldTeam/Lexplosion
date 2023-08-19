using Lexplosion.Logic.FileSystem;
using System;
using System.Windows.Threading;

namespace Lexplosion.Common.Models.Objects
{
    public sealed class FileDistributionWrapper : VMBase
    {
        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        public readonly FileDistributor FileDistribution;


        #region Properties


        public string Name { get; }

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


        #region Constructors


        public FileDistributionWrapper(string name, FileDistributor fileDistributor)
        {
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Interval += new TimeSpan(0, 1, 0);
            _dispatcherTimer.Start();

            Name = name;
            FileDistribution = fileDistributor;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void StopTimer()
        {
            _dispatcherTimer.Stop();
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ActivityTime += 1;
        }

        #endregion Private Methods
    }
}
