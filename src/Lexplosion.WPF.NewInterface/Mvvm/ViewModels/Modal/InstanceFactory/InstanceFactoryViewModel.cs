using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core.Modal;
using System;
using System.Windows.Input;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Modal;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
	public sealed class InstanceFactoryViewModel : ActionModalViewModelBase
	{
		#region Properties

		public InstanceFactoryModel Model { get; }


		private bool _isVanilla;
		public bool IsVanilla
		{
			get => _isVanilla; set
			{
				_isVanilla = value;
				OnPropertyChanged();
			}
		}

		private bool _isForge;
		public bool IsForge
		{
			get => _isForge; set
			{
				_isForge = value;
				OnPropertyChanged();
			}
		}

		private bool _isFabric;
		public bool IsFabric
		{
			get => _isFabric; set
			{
				_isFabric = value;
				OnPropertyChanged();
			}
		}

		private bool _isQuilt;
		public bool IsQuilt
		{
			get => _isQuilt; set
			{
				_isQuilt = value;
				OnPropertyChanged();
			}
		}

		private bool _isNeoForged;
		public bool IsNeoForged
		{
			get => _isNeoForged; set
			{
				_isNeoForged = value;
				OnPropertyChanged();
			}
		}


        private RelayCommand _changeInstanceClientTypeCommand;
        public ICommand ChangeInstanceClientTypeCommand
        {
            get => RelayCommand.GetCommand(ref _changeInstanceClientTypeCommand, Model.ChangeGameType);
        }


        #endregion Properties


        public InstanceFactoryViewModel(Action<InstanceClient> addToLibrary, ICommand closeModalMenu, IReadOnlyCollection<InstancesGroup> groups, InstancesGroup defaultGroup) : base()
		{
			IsCloseAfterCommandExecuted = true;
			Model = new InstanceFactoryModel(groups, defaultGroup);

			ActionCommandExecutedEvent += (obj) =>
			{
				Model.CreateInstance();
                //addToLibrary();
                closeModalMenu.Execute(obj);
			};

			Model.GameTypeChanged += UpdateSelectedGameType;
			UpdateSelectedGameType(ClientType.Vanilla);
		}


        private void UpdateSelectedGameType(ClientType clientType)
        {
            switch (clientType)
            {
                case ClientType.Vanilla: IsVanilla = true; break;
                case ClientType.Forge: IsForge = true; break;
                case ClientType.Fabric: IsFabric = true; break;
                case ClientType.Quilt: IsQuilt = true; break;
                case ClientType.NeoForge: IsNeoForged = true; break;
            }
        }
    }
}
