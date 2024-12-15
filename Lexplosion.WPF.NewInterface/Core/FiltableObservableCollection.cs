using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Core
{
    public class FiltableObservableCollection : ObservableObject, IEnumerable
    {
        #region Properties


        private readonly ObservableCollection<object> _filteredCollection = new ObservableCollection<object>();
        public ICollection Filtered { get => _filteredCollection; }


        private IEnumerable _source;
        public IEnumerable Source
        {
            get => _source; set
            {
                _source = value;
                OnSourceChanged();
            }
        }

        private Predicate<object> _filter;
        public Predicate<object> Filter
        {
            get => _filter; set
            {
                _filter = value;
                OnFilterChanged();
            }
        }


        #endregion Properties


        #region Public & Protected Methods


        protected virtual void UpdateCollectionWithFilter()
        {
            _filteredCollection.Clear();

            foreach (var newItem in Source)
            {
                if (Filter == null)
                {
                    _filteredCollection.Add(newItem);
                    continue;
                }

                if (Filter(newItem))
                {
                    _filteredCollection.Add(newItem);
                }
            }
        }

        protected virtual void OnFilterChanged()
        {
            UpdateCollectionWithFilter();
        }

        protected virtual void OnSourceChanged()
        {
            UpdateCollectionWithFilter();
        }

        public IEnumerator GetEnumerator()
        {
            return Filtered.GetEnumerator();
        }


        #endregion Public & Protected Methods
    }
}
