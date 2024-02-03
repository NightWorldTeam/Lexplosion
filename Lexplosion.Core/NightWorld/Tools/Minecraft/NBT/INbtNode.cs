using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightWorld.Tools.Minecraft.NBT
{
    public interface INbtNode
    {
        NbtTagType Type { get; }
        string Name { get; }
    }
}

