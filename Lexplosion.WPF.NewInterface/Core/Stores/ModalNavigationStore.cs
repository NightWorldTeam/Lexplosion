using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels;
using System;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Stores
{
    public sealed class ModalNavigationStore
    {
        private static readonly Dictionary<ModalAbstractFactory.ModalPage, ModalAbstractFactory> _modalAbstractFactoriesByType = new Dictionary<ModalAbstractFactory.ModalPage, ModalAbstractFactory>();


        public event CurrentViewModelChangedEventHandler CurrentViewModelChanged;


        private IModalViewModel _currentViewModel;
        public IModalViewModel CurrentViewModel
        {
            get => _currentViewModel; private set
            {
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        public void Open(IModalViewModel viewModel)
        {
            CurrentViewModel = viewModel;
            CurrentViewModel.CloseCommandExecutedEvent += Close;
        }

        public void RegisterAbstractFactory(ModalAbstractFactory.ModalPage type, ModalAbstractFactory factory) 
        {
            if (_modalAbstractFactoriesByType.ContainsKey(type))
                throw new ArgumentException($"{type.ToString()} уже существует в словаре абстрактных фабрик модального окна");

            _modalAbstractFactoriesByType.Add(type, factory);
        }

        public void OpenModalPageByType(ModalAbstractFactory.ModalPage type) 
        {
            if (!_modalAbstractFactoriesByType.ContainsKey(type))
                throw new ArgumentException($"{type.ToString()} не существует в словаре абстрактных фабрик модального окна");

            Open(_modalAbstractFactoriesByType[type].Create());
        }

        public void Close(object obj)
        {
            CurrentViewModel.CloseCommandExecutedEvent -= Close;
            CurrentViewModel = null;
        }

        private void OnCurrentViewModelChanged()
        {
            Runtime.DebugWrite("TESLTLKTSEKLTETKS");
            CurrentViewModelChanged?.Invoke();
        }
    }
}
