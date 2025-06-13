using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.ModalFactory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Stores
{
    public sealed class ModalNavigationStore
    {
        public event CurrentViewModelChangedEventHandler CurrentViewModelChanged;
        public event Action Opened;
        public event Action Closed;

        private static readonly Dictionary<Type, Func<IModalViewModel>> _modalAbstractFactoriesByType = new();

        public string LatestModal;

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
            if (viewModel != null) 
            {
                LatestModal = viewModel.ToString().Split('.').LastOrDefault();
            }
            CurrentViewModel.CloseCommandExecutedEvent += CloseInternal;
            Opened?.Invoke();
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
            var tmpVM = CurrentViewModel;
            CurrentViewModel = null;
            tmpVM.ExecuteClosedEvent();
            Closed?.Invoke();
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
