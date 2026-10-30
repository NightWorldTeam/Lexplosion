using Lexplosion.UI.WPF.Core.ViewModel;
#if DEBUG
using Lexplosion.QA;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Lexplosion.UI.WPF.Core
{
    public class FiltableObservableCollection : ObservableObject, IEnumerable, IReadOnlyCollection<object>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;


        private Action? _disposeObservableColletion = null;

        
        #region Properties


        private readonly BulkObservableCollection<object> _filteredCollection = new BulkObservableCollection<object>();
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

        public int Count => _filteredCollection.Count;


        #endregion Properties


        #region Public & Protected Methods


        /// <summary>
        /// For collection without INotifyCollectionChanged implementation.
        /// </summary>
        public void UpdateSourceData()
        {
            UpdateCollectionWithFilter();
        }


        protected virtual void UpdateCollectionWithFilter()
        {
            // TODO: Перевести на делегат, чтобы можно было использовать в любом MVVM проекте (пригодиться в avalonia)
            App.Current.Dispatcher.Invoke(() =>
            {
#if DEBUG
                using (UiDebugTrace.Measure("collection.filter.rebuild", 3))
#endif
                {
                    var matches = new List<object>();
                    if (Source != null)
                    {
                        foreach (var item in Source)
                        {
                            if (Filter == null || Filter(item))
                                matches.Add(item);
                        }
                    }

                    // WPF sees one Reset, rather than Clear + N incremental
                    // changes (and the parent collection's own Reset).
                    _filteredCollection.ReplaceAll(matches);
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
                    );

                    OnPropertyChanged(nameof(Count));
                }
            });
        }

        protected virtual void OnFilterChanged()
        {
            UpdateCollectionWithFilter();
        }

        protected virtual void OnSourceChanged()
        {
            _disposeObservableColletion?.Invoke();
            if (Source is INotifyCollectionChanged observCollection)
            {
                NotifyCollectionChangedEventHandler collectionChanged = (e, s) => UpdateSourceData();
                observCollection.CollectionChanged += collectionChanged;
                _disposeObservableColletion = () => observCollection.CollectionChanged -= collectionChanged;
            }

            UpdateCollectionWithFilter();
        }

        public IEnumerator GetEnumerator()
        {
            return Filtered.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return _filteredCollection.GetEnumerator();
        }


        #endregion Public & Protected Methods
    }
}
