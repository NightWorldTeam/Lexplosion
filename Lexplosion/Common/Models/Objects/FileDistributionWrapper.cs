using Lexplosion.Logic.FileSystem;

namespace Lexplosion.Common.Models.Objects
{
    public sealed class FileDistributionWrapper
    {
        public readonly FileDistributor FileDistribution;

        public string Name { get; }

        public FileDistributionWrapper(string name, FileDistributor fileDistributor)
        {
            Name = name;
            FileDistribution = fileDistributor;
        }
    }
}
