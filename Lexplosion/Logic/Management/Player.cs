using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management
{
    class Player : VMBase
    {
        public string Name { get; } = "Player";
        public byte[] Avatart { get; } = null;
        public string ProfileUrl { get; } = null;

        public void Kick()
        {

        }

        public void Unkick()
        {

        }
    }
}
