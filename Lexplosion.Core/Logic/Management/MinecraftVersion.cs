using System;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Management
{
	//TODO: перенести в пространсво имён Lexplosion.Logic.Objects
	public sealed class MinecraftVersion : IComparable, IComparable<MinecraftVersion>, IEquatable<MinecraftVersion>
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
		[JsonIgnore]
		public int Weight { get; }


		#region Constructors


		public MinecraftVersion()
		{
			Id = null;
			Type = VersionType.Release;
		}

		public MinecraftVersion(string id, VersionType versionType, int weight = -4)
		{
			Id = id;
			Type = versionType;

			if (weight != -4)
			{
				Weight = weight;
				return;
			}

			if (!string.IsNullOrWhiteSpace(id))
			{
				Weight = CalcWeight(id);
			}
		}

		public MinecraftVersion(string id)
		{
			Type = VersionType.Release;
			if (!string.IsNullOrWhiteSpace(id))
			{
				if (!id.Contains("."))
					Type = VersionType.Snapshot;

				Weight = CalcWeight(id);
			}

			Id = id;
		}


		#endregion Constructors


		#region Public Methods


		public override string ToString()
		{
			return Type == VersionType.Snapshot ? Type + " " + Id : Id;
		}

		public string ToFullString()
		{
			return $"{Type} {Id}";
		}

		public int CompareTo(object obj)
		{
			if (obj == null || !(obj is MinecraftVersion))
				return 1;

			return CompareTo(obj as MinecraftVersion);
		}

		public int CompareTo(MinecraftVersion other)
		{
			if (other == null)
				return -1;

			if (Type == other.Type)
			{
				return Weight.CompareTo(other.Weight);
			}

			if (Type == VersionType.Release)
			{
				return 1;
			}

			return -1;
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

			return Equals(obj as MinecraftVersion);
		}

		public bool Equals(MinecraftVersion other)
		{
			if (other == null)
			{
				return false;
			}

			return CompareTo(other) == 0;
		}

		public static bool IsValidRelease(string gameVersion)
		{
			if (string.IsNullOrWhiteSpace(gameVersion)) return false;

			string[] parts = gameVersion.Split('.');
			if (parts.Length >= 1)
			{
				foreach (string part in parts)
				{
					if (!Int32.TryParse(part, out _))
					{
						return false;
					}
				}
			}
			else
			{
				return false;
			}

			return true;
		}


		#endregion Public Methods


		#region Private Methods


		private int CalcWeight(string id)
		{
			if (id == "All")
				return -1;

			if (Type == VersionType.Release)
			{
				return CalcWeight(id.Split('.'));
			}
			else
			{
				return -2;// CalcWeight(id.Replace("a", "").Split('w'));
			}
		}

		private int CalcWeight(string[] parts)
		{
			var weight = 0;
			var i = 100000000;
			foreach (var part in parts)
			{
				weight += int.Parse(part) * i;
				i /= 10000;
			}
			return weight;
		}


		#endregion Private Methods


		#region Math Operators


		public static bool operator <(MinecraftVersion mv1, MinecraftVersion mv2)
		{
			return mv1.CompareTo(mv2) < 0;
		}

		public static bool operator >(MinecraftVersion mv1, MinecraftVersion mv2)
		{
			return mv1.CompareTo(mv2) > 0;
		}

		public static bool operator <=(MinecraftVersion mv1, MinecraftVersion mv2)
		{
			return !(mv1 > mv2);
		}

		public static bool operator >=(MinecraftVersion mv1, MinecraftVersion mv2)
		{
			return !(mv1 < mv2);
		}

		public static bool operator ==(MinecraftVersion mv1, MinecraftVersion mv2)
		{
			return mv1?.Id == mv2?.Id;
		}

		public static bool operator !=(MinecraftVersion mv1, MinecraftVersion mv2)
		{
			return mv1?.Id != mv2?.Id;
		}

		#endregion Math Operators
	}
}
