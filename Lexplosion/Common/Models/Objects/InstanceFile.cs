using System.Collections.Generic;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public sealed class InstanceFile : VMBase
    {
        public string Name { get; }
        private int _procents;
        public int Procents
        {
            get => _procents; set
            {
                _procents = value;
                OnPropertyChanged();
            }
        }

        public InstanceFile(string name, int procents)
        {
            Name = name;
            Procents = procents;
        }

        /// Выполняется O(n).
        public static InstanceFile GetInstanceFile(IEnumerable<InstanceFile> files, string name)
        {
            foreach (var file in files)
            {
                if (file.Name == name)
                    return file;
            }
            return null;
        }
    }
}
