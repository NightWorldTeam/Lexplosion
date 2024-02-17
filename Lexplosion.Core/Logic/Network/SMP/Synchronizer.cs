using System.Threading;

namespace Lexplosion.Logic.Network.SMP
{
    public class Synchronizer
    {
        private AutoResetEvent _lock;
        public bool IsSignal { get; private set; }

        public Synchronizer(bool isSignal)
        {
            _lock = new AutoResetEvent(isSignal);
            IsSignal = isSignal;
        }

        public bool ResetCalled { get; private set; }

        public bool WaitOne(int timeout)
        {
            //bool isSignal = IsSignal;
            //IsSignal = false;
            //return _lock.WaitOne(timeout) || IsSignal || isSignal;
            IsSignal = false;
            return _lock.WaitOne(timeout);
        }

        public bool Set()
        {
            ResetCalled = false;
            IsSignal = true;
            return _lock.Set();
        }

        public void Reset()
        {
            ResetCalled = true;
            IsSignal = false;
            _lock.Reset();
        }
    }
}
