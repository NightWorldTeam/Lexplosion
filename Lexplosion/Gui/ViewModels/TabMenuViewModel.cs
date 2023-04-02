using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class TabMenuViewModel : SubmenuViewModel
    {
        private bool _isInstance;
        public bool IsInstance
        {
            get => _isInstance; private set
            {
                _isInstance = value;
                OnPropertyChanged();
            }
        }

        private string _header;
        /// <summary>
        /// Заголовок страницы.
        /// </summary>
        public string Header
        {
            get => _header; private set
            {
                _header = value;
                OnPropertyChanged();
            }
        }

        /* Button */////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool _isEnableButton;
        public bool IsEnableButton
        {
            get => _isEnableButton; set
            {
                _isEnableButton = value;
                OnPropertyChanged();
            }
        }

        private string _buttonContent = "";
        public string ButtonContent
        {
            get => _buttonContent; set
            {
                _buttonContent = value;
                OnPropertyChanged();
            }
        }

        private Action _buttonAction;
        private RelayCommand _buttonActionCommand;
        public RelayCommand ButtonActionCommand
        {
            get => _buttonActionCommand ?? (_buttonActionCommand = new RelayCommand(obj =>
            {
                _buttonAction();
            }));
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public InstanceFormViewModel InstanceFormVM { get; }

        public InstanceClient InstanceClient { get; private set; }

        public void ShowButton(string content, Action action)
        {
            _buttonAction = action;
            ButtonContent = content;
            IsEnableButton = true;
        }

        public void HideButton()
        {
            IsEnableButton = false;
        }

        public TabMenuViewModel(IList<Tab<VMBase>> tabs, string header, int selectedTabIndex = 0, InstanceFormViewModel instanceFormViewModel = null)
        {
            if (instanceFormViewModel != null)
            {
                InstanceFormVM = instanceFormViewModel;
                InstanceClient = instanceFormViewModel.Client;
                IsInstance = true;
            }
            else IsInstance = false;

            Header = header;
            Tabs = new ObservableCollection<Tab<VMBase>>(tabs);
            if (tabs.Count > 0)
                SelectedTab = Tabs[selectedTabIndex];
        }
    }
}
