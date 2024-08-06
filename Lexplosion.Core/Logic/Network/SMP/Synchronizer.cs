using System;
using System.Threading;

namespace Lexplosion.Logic.Network.SMP
{
    public class Synchronizer
    {
        private AutoResetEvent _lock;
        private bool _isSignal = false;

        public string IsSignalSetInfo { get; private set; }

        public string PositiveSignalSetInfo { get; private set; }

        public bool IsSignal
        {
            get => _isSignal; set
            {
                var nowTime = DateTimeOffset.Now;
                string time = $"{nowTime.Hour}:{nowTime.Minute}:{nowTime.Second}.{nowTime.Millisecond}";

                _isSignal = value;
                if (_isSignal) PositiveSignalSetInfo = $"Value: {value}, time : {time}, stacktrace: {new System.Diagnostics.StackTrace()}";
                IsSignalSetInfo = $"Value: {value}, time : {time}, stacktrace: {new System.Diagnostics.StackTrace()}";
            }
        }

        private object _syncLock = new object();

        public Synchronizer(bool isSignal)
        {
            _lock = new AutoResetEvent(isSignal);
            IsSignal = isSignal;
        }

        public bool ResetCalled { get; private set; }

        public bool WaitOne(int timeout)
        {
            bool isSignal;
            lock (_syncLock)
            {
                isSignal = IsSignal;
                IsSignal = false;
            }

            return _lock.WaitOne(timeout) || IsSignal || isSignal;
            //IsSignal = false;
            //return _lock.WaitOne(timeout);
        }

        public bool Set()
        {
            lock (_syncLock)
            {
                ResetCalled = false;
                IsSignal = true;
                return _lock.Set();
            }
        }

        public void Reset()
        {
            lock (_syncLock)
            {
                ResetCalled = true;
                IsSignal = false;
                _lock.Reset();
            }
        }
    }
}
