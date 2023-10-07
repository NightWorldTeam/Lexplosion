using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Management
{
    public sealed class MinecraftVersion : IComparable<MinecraftVersion>, IEquatable<MinecraftVersion>
    {
        public enum VersionType
        {
            Release,
            Snapshot
        }

        public string Id { get; }
        public VersionType Type { get; }

        [JsonIgnore]
        public bool IsNan { get => string.IsNullOrWhiteSpace(Id); }


        #region Constructors


        public MinecraftVersion()
        {
            Id = null;
            Type = VersionType.Release;
        }

        public MinecraftVersion(string id, VersionType versionType)
        {
            Id = id;
            Type = versionType;
        }

        public MinecraftVersion(string id)
        {
            Type = VersionType.Release;
            if (!string.IsNullOrWhiteSpace(id))
            {
                string[] parts = id.Split('.');
                if (parts.Length < 1)
                {
                    Type = VersionType.Snapshot;
                }
                else
                {
                    foreach (string part in parts)
                    {
                        if (!Int32.TryParse(part, out _))
                        {
                            Type = VersionType.Snapshot;
                            break;
                        }
                    }
                }
            }

            Id = id;
        }


        #endregion Constructors


        #region Public Methods


        public override string ToString()
        {
            return Type.ToString() + " " + Id;
        }

        public int CompareTo(MinecraftVersion other)
        {
            return (Id, Type).CompareTo((other.Id, other.Type));
        }

        public override int GetHashCode()
        {
            return CombineHashCodes(Id.GetHashCode(), Type.GetHashCode());
        }

        private static int CombineHashCodes(int hash1, int hash2)
        {
            return ((hash1 << 5) + hash1) ^ hash2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is MinecraftVersion))
                return false;

            return Equals((MinecraftVersion)obj);
        }

        public bool Equals(MinecraftVersion other)
        {
            if (other == null) 
            {
                return false;
            }

            return this.Id == other.Id && this.Type == other.Type;
        }


        #endregion Public Methods
    }
}
