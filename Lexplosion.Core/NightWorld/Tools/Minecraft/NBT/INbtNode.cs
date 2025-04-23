namespace NightWorld.Tools.Minecraft.NBT
{
	public interface INbtNode
	{
		NbtTagType Type { get; }
		string Name { get; }
	}
}

