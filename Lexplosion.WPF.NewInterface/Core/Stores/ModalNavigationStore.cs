using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.ModalFactory;
using System;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Stores
{
    public sealed class ModalNavigationStore
    {
        public event CurrentViewModelChangedEventHandler CurrentViewModelChanged;

        private static readonly Dictionary<Type, Func<IModalViewModel>> _modalAbstractFactoriesByType = new();


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
            CurrentViewModel.CloseCommandExecutedEvent += CloseInternal;
        }

        public void RegisterAbstractFactory(Type type, ModalFactoryBase factory) 
        {
            if (_modalAbstractFactoriesByType.ContainsKey(type))
                throw new ArgumentException($"{type.ToString()} уже существует в словаре абстрактных фабрик модального окна");

            _modalAbstractFactoriesByType.Add(type, factory.Create);
        }

        public void RegisterAbstractFactory(Type type, Func<IModalViewModel> factory)
        {
            if (_modalAbstractFactoriesByType.ContainsKey(type))
                throw new ArgumentException($"{type.ToString()} уже существует в словаре абстрактных фабрик модального окна");

            _modalAbstractFactoriesByType.Add(type, factory);
        }

        public void OpenModalPageByType(Type type) 
        {
            if (!_modalAbstractFactoriesByType.ContainsKey(type))
                throw new ArgumentException($"{type.ToString()} не существует в словаре абстрактных фабрик модального окна");

            Open(_modalAbstractFactoriesByType[type]?.Invoke());
        }

        public void Close()
        {
            if (CurrentViewModel == null) 
            {
                return;
            }

            CurrentViewModel.CloseCommandExecutedEvent -= CloseInternal;
            CurrentViewModel = null;
        }

        private void CloseInternal(object obj) 
        {
            Close();
        }

        private void OnCurrentViewModelChanged()
        {
            Runtime.DebugWrite("TESLTLKTSEKLTETKS");
            CurrentViewModelChanged?.Invoke();
        }
    }
}
