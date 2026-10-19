using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Mvvm.Models.Modal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal
{
    public sealed class JvmArgsEditorViewModel : ActionModalViewModelBase
    {
        public JvmArgsEditorModel Model { get; }
        private readonly Action<string> _onSave;

        public event Action<JvmArgEntry> FocusNewEntryRequested;

        public ObservableCollection<JvmArgEntry> Entries { get; }

        public bool HasEntries => Entries.Count > 0;
        public bool HasNoEntries => Entries.Count == 0;

        private ICollectionView _filteredEntries;
        public ICollectionView FilteredEntries
        {
            get => _filteredEntries;
            set
            {
                _filteredEntries = value;
                OnPropertyChanged();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (RaiseAndSetIfChanged(ref _searchText, value))
                    FilterEntries();
            }
        }

        private string _bulkPasteText;
        public string BulkPasteText
        {
            get => _bulkPasteText;
            set => RaiseAndSetIfChanged(ref _bulkPasteText, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (RaiseAndSetIfChanged(ref _statusMessage, value))
                    OnPropertyChanged(nameof(HasStatusMessage));
            }
        }

        public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

        #region Commands

        private RelayCommand _addEntryCommand;
        public ICommand AddEntryCommand
        {
            get => RelayCommand.GetCommand(ref _addEntryCommand, obj =>
            {
                var entry = new JvmArgEntry("-Dnew", string.Empty);
                Entries.Insert(0, entry);
                FilteredEntries.Refresh();
                UpdateHasEntries();
                FocusNewEntryRequested?.Invoke(entry);
            });
        }

        private RelayCommand _removeEntryCommand;
        public ICommand RemoveEntryCommand
        {
            get => RelayCommand.GetCommand(ref _removeEntryCommand, obj =>
            {
                if (obj is JvmArgEntry entry && Entries.Contains(entry))
                {
                    Entries.Remove(entry);
                    FilteredEntries.Refresh();
                    UpdateHasEntries();
                }
            });
        }

        private RelayCommand _clearAllCommand;
        public ICommand ClearAllCommand
        {
            get => RelayCommand.GetCommand(ref _clearAllCommand, obj =>
            {
                Entries.Clear();
                FilteredEntries.Refresh();
                UpdateHasEntries();
            });
        }

        private RelayCommand _bulkPasteCommand;
        public ICommand BulkPasteCommand
        {
            get => RelayCommand.GetCommand(ref _bulkPasteCommand, obj => ExecuteBulkPaste());
        }

        private RelayCommand _clearStatusCommand;
        public ICommand ClearStatusCommand
        {
            get => RelayCommand.GetCommand(ref _clearStatusCommand, obj =>
            {
                StatusMessage = string.Empty;
            });
        }

        #endregion

        public JvmArgsEditorViewModel(string initialArgs, Action<string> onSave)
        {
            Model = new JvmArgsEditorModel();
            _onSave = onSave;

            var parsed = Model.Parse(initialArgs);
            Entries = new ObservableCollection<JvmArgEntry>(parsed);

            FilteredEntries = CollectionViewSource.GetDefaultView(Entries);
            FilteredEntries.Filter = FilterPredicate;

            ActionCommandExecutedEvent += OnSave;

            UpdateHasEntries();
        }

        private void UpdateHasEntries()
        {
            OnPropertyChanged(nameof(HasEntries));
            OnPropertyChanged(nameof(HasNoEntries));
        }

        private void ExecuteBulkPaste()
        {
            if (string.IsNullOrWhiteSpace(BulkPasteText))
            {
                StatusMessage = App.Current.Resources["JVMArgsEditorNoInput"] as string ?? "No input to paste.";
                return;
            }

            var newEntries = Model.MergeAndDeduplicate(
                new List<JvmArgEntry>(Entries),
                BulkPasteText);

            Entries.Clear();
            foreach (var entry in newEntries)
                Entries.Add(entry);

            FilteredEntries.Refresh();
            UpdateHasEntries();
            BulkPasteText = string.Empty;
            var format = App.Current.Resources["JVMArgsEditorMergeSuccess"] as string ?? "Merged successfully. Total: {0} entries.";
            StatusMessage = string.Format(format, Entries.Count);
        }

        private void OnSave(object obj)
        {
            var entries = new List<JvmArgEntry>(Entries);
            var result = Model.Rebuild(entries);
            _onSave?.Invoke(result);
        }

        private bool FilterPredicate(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (obj is JvmArgEntry entry)
            {
                return entry.Key.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
        }

        private void FilterEntries()
        {
            FilteredEntries.Refresh();
        }
    }
}
