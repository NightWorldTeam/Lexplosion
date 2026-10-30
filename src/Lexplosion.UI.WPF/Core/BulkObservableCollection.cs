using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Lexplosion.UI.WPF.Core
{
    /// <summary>
    /// Replaces a bound collection with one Reset notification instead of one
    /// notification per item. Call from the owning UI thread.
    /// </summary>
    public sealed class BulkObservableCollection<T> : ObservableCollection<T>
    {
        public void ReplaceAll(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            // Materialize before clearing so callers may pass the current items.
            var snapshot = new List<T>(items);
            CheckReentrancy();
            Items.Clear();
            foreach (var item in snapshot)
                Items.Add(item);

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
