using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NightWorld.Tools.Minecraft.NBT
{
    public abstract class NbtTagBase<TContent> : INbtNode
    {
        public abstract NbtTagType Type { get; }
        public TContent Content { get; set; }
        public string Name { get; }

        public NbtTagBase(string name, TContent content)
        {
            Content = content;
            Name = name ?? string.Empty;
        }
    }

    public class NbtByte : NbtTagBase<byte>
    {
        public override NbtTagType Type { get => NbtTagType.Byte; }
        public NbtByte(string name, byte content) : base(name, content) { }

    }

    public class NbtShort : NbtTagBase<short>
    {
        public override NbtTagType Type { get => NbtTagType.Short; }
        public NbtShort(string name, short content) : base(name, content) { }
    }

    public class NbtInt : NbtTagBase<int>
    {
        public override NbtTagType Type { get => NbtTagType.Int; }
        public NbtInt(string name, int content) : base(name, content) { }
    }

    public class NbtLong : NbtTagBase<long>
    {
        public override NbtTagType Type { get => NbtTagType.Long; }
        public NbtLong(string name, long content) : base(name, content) { }
    }

    public class NbtFloat : NbtTagBase<float>
    {
        public override NbtTagType Type { get => NbtTagType.Float; }
        public NbtFloat(string name, float content) : base(name, content) { }
    }

    public class NbtDouble : NbtTagBase<double>
    {
        public override NbtTagType Type { get => NbtTagType.Double; }
        public NbtDouble(string name, double content) : base(name, content) { }
    }

    public class NbtString : NbtTagBase<string>
    {
        public override NbtTagType Type { get => NbtTagType.String; }
        public NbtString(string name, string content) : base(name, content ?? string.Empty) { }
    }

    public class NbtByteArray : NbtTagBase<byte[]>
    {
        public override NbtTagType Type { get => NbtTagType.ByteArray; }
        public NbtByteArray(string name, byte[] content) : base(name, content ?? new byte[0]) { }
    }

    public class NbtIntArray : NbtTagBase<int[]>
    {
        public override NbtTagType Type { get => NbtTagType.IntArray; }
        public NbtIntArray(string name, int[] content) : base(name, content ?? new int[0]) { }
    }

    public class NbtLongArray : NbtTagBase<long[]>
    {
        public override NbtTagType Type { get => NbtTagType.LongArray; }
        public NbtLongArray(string name, long[] content) : base(name, content ?? new long[0]) { }
    }

    public class NbtCompound : NbtTagBase<Dictionary<string, INbtNode>>, IDictionary<string, INbtNode>
    {
        public override NbtTagType Type { get => NbtTagType.Compound; }

        public NbtCompound() : base(string.Empty, new Dictionary<string, INbtNode>()) { }
        public NbtCompound(string name) : base(name, new Dictionary<string, INbtNode>()) { }
        public NbtCompound(string name, Dictionary<string, INbtNode> content) : base(name, content ?? new Dictionary<string, INbtNode>()) { }

        public INbtNode this[string key]
        {
            get => Content[key];
            set => Content[key] = value;
        }

        public ICollection<string> Keys => Content.Keys;
        public ICollection<INbtNode> Values => Content.Values;
        public int Count => Content.Count;
        public bool IsReadOnly => ((IDictionary<string, INbtNode>)Content).IsReadOnly;
        public bool ContainsKey(string key) => Content.ContainsKey(key);
        public void Add(string key, INbtNode value) => Content.Add(key, value);
        public void Add(INbtNode value) => Content.Add(value.Name, value);
        public bool Remove(string key) => Content.Remove(key);
        public bool TryGetValue(string key, out INbtNode value) => Content.TryGetValue(key, out value);
        public void Add(KeyValuePair<string, INbtNode> item) => Content.Add(item.Key, item.Value);
        public void Clear() => Content.Clear();
        public bool Remove(KeyValuePair<string, INbtNode> item) => Content.Remove(item.Key);
        public IEnumerator<KeyValuePair<string, INbtNode>> GetEnumerator() => Content.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Content).GetEnumerator();
        public bool Contains(KeyValuePair<string, INbtNode> item) => Content.Contains(item);
        public void CopyTo(KeyValuePair<string, INbtNode>[] array, int arrayIndex) => ((IDictionary<string, INbtNode>)Content).CopyTo(array, arrayIndex);
    }

    public class NbtList : NbtTagBase<List<INbtNode>>, IList<INbtNode>
    {
        public override NbtTagType Type { get => NbtTagType.List; }
        public NbtTagType ListContentType { get; }

        public NbtList(string name, NbtTagType listContentType) : this(name, new List<INbtNode>(), listContentType)
        {
            ListContentType = listContentType;
        }

        public NbtList(string name, List<INbtNode> content, NbtTagType listContentType) : base(name, content ?? new List<INbtNode>())
        {
            ListContentType = listContentType;
        }

        public INbtNode this[int index]
        {
            get => Content[index];
            set => Content[index] = value;
        }

        public int Count => Content.Count;
        public bool IsReadOnly => false;
        public IEnumerator<INbtNode> GetEnumerator() => Content.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Content).GetEnumerator();
        public int IndexOf(INbtNode item) => Content.IndexOf(item);
        public void Insert(int index, INbtNode item) => Content.Insert(index, item);
        public void RemoveAt(int index) => Content.RemoveAt(index);
        public void Add(INbtNode item) => Content.Add(item);
        public void Clear() => Content.Clear();
        public bool Contains(INbtNode item) => Content.Contains(item);
        public void CopyTo(INbtNode[] array, int arrayIndex) => Content.CopyTo(array, arrayIndex);
        public bool Remove(INbtNode item) => Content.Remove(item);
    }
}
