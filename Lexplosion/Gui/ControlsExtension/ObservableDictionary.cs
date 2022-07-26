using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Lexplosion.Gui.Extension
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public  ObservableDictionary() { }

        public ObservableDictionary(Dictionary<TKey, TValue> dict) 
        {
            foreach (var key in dict.Keys) 
            {
                this.Add(key, dict[key]);
            }
        }

        public new void Add(TKey key, TValue value)
        {
            if (base.ContainsKey(key))
                return;

            base.Add(key, value);
            if (!TryGetValue(key, out _)) return;
            var index = Keys.Count;
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Values));
            OnCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
        }

        public new void Remove(TKey key)
        {
            if (!TryGetValue(key, out var value)) return;
            var index = IndexOf(Keys, key);
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Values));
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, value, index);
            base.Remove(key);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private int IndexOf(KeyCollection keys, TKey key)
        {
            var index = 0;
            foreach (var k in keys)
            {
                if (Equals(k, key))
                    return index;
                index++;
            }
            return -1;
        }
    }
}
