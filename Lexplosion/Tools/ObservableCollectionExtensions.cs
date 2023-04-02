using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Tools
{
    public static class ObservableCollectionExtensions
    {
        public static void ObservableColletionSort<T>(this ObservableCollection<T> colletion)
        {
            List<T> list = new List<T>(colletion);

            list.Sort();

            colletion = new ObservableCollection<T>(list);
        }
    }
}
