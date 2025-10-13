using Lexplosion.Logic.Management.Instances;
using System;
using System.Threading;

namespace Lexplosion.Logic.Management.Import
{
    public class ImportInterruption
    {
        private readonly ManualResetEvent _baseDataEvent = new(false);

        public enum InterruptionType
        {
            BasicDataRequired
        }

        public ImportInterruption(Guid importId)
        {
            ImportId = importId;
        }

        private BaseInstanceData _baseInstanceData;
        public BaseInstanceData BaseData
        {
            get
            {
                _baseDataEvent.WaitOne();
                return _baseInstanceData;
            }
            set
            {
                _baseInstanceData = value;
                _baseDataEvent.Set();
            }
        }

        public Guid ImportId { get; }
    }
}
