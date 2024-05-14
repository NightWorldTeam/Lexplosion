using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu;
using Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects;
using Lexplosion.Logic.Objects;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class LibraryViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;
        private readonly ICommand _toMainMenuLayoutCommand;
        private readonly ModalNavigationStore _modalNavigationStore;
        private readonly Func<IEnumerable<InstanceModelBase>> _getInstances;

        public LibraryModel Model { get; }

        private bool _isCategoriesListOpen;
        public bool IsCategoriesListOpen 
        {
            get => _isCategoriesListOpen; set 
            {
                _isCategoriesListOpen = value;
                OnPropertyChanged();
            }
        }


        #region Commands


        private RelayCommand _openInstanceProfileMenu;
        public ICommand OpenInstanceProfileMenu
        {
            get => RelayCommand.GetCommand(ref _openInstanceProfileMenu, (obj) =>
            {
                var ins = (InstanceModelBase)obj;

                _navigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(_navigationStore, _toMainMenuLayoutCommand, ins);
            });
        }


        private RelayCommand _openInstanceFactory;
        public ICommand OpenInstanceFactory
        {
            get => RelayCommand.GetCommand(ref _openInstanceFactory, () => 
            {
                _modalNavigationStore.OpenModalPageByType(typeof(InstanceFactoryViewModel));
            });
        }


        private RelayCommand _testCommand;
        public ICommand TestCommand 
        {
            get => RelayCommand.GetCommand<ITranslatableObject<InstanceSource>>(ref _testCommand, (o) => 
            {
                if (Model.FilterPanel.SelectedSource.Value == o.Value)
                    return;

                Model.FilterPanel.SelectedSource = o;
            });
        }


        private RelayCommand _selectCategoryCommand;
        public ICommand SelectCategoryCommand 
        {
            get => RelayCommand.GetCommand<CategoryBase>(ref _selectCategoryCommand, (category) => 
            {
                Model.FilterPanel.SelectedCategories.Add(category);

                Model.FilterPanel.AvailableCategories.Remove(category);
                Model.FilterPanel.FilterChangedExecuteEvent();
                IsCategoriesListOpen = false;
            });
        }

        private RelayCommand _unselectCategoryCommand;
        public ICommand UnselectCategoryCommand
        {
            get => RelayCommand.GetCommand<CategoryBase>(ref _unselectCategoryCommand, (category) =>
            {
                Model.FilterPanel.SelectedCategories.Remove(category);
                Model.FilterPanel.AvailableCategories.Add(category);
                Model.FilterPanel.FilterChangedExecuteEvent();
            });
        }
                

        #endregion Commands


        // TODO: думаю делегат с инстансами это костыль ченить другое надо придумать
        public LibraryViewModel(INavigationStore navigationStore, ICommand toMainMenuLayoutCommand, ModalNavigationStore modalNavigationStore, IInstanceController instanceController)
        {
            Model = new LibraryModel(instanceController);
            _navigationStore = navigationStore;
            _toMainMenuLayoutCommand = toMainMenuLayoutCommand;
            _modalNavigationStore = modalNavigationStore;
        }
    }
}
