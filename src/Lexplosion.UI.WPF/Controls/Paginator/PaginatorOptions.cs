using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.UI.WPF.Controls
{
    public class PaginatorOptions
    {
        public int Current { get; set; }
        public int PageSize { get; set; }
        public int Count { get; set; }
        public bool IsDisabled { get; set; }
    }
}
