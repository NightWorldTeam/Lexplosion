using Lexplosion.UI.WPF.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.NewsHub
{
    public sealed class NewsHubViewModel : ViewModelBase
    {
        public List<int> Items { get; } = new List<int>()
            {
                1,2,3,4,5,6
            };
    }
}
