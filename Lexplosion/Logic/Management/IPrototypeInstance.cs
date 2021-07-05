using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management
{
    interface IPrototypeInstance
    {
        string Check();
        InitData Update();
        string CheckOnlyBase();
        InitData UpdateOnlyBase();
    }
}
