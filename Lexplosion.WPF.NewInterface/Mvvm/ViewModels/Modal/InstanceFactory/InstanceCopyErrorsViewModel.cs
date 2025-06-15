using Lexplosion.Logic.Management.Addons;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core.Modal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{

    public sealed class InstanceCopyErrorsViewModel : ModalViewModelBase
    {
        public InstanceCopyErrorsModel Model { get; }


        private RelayCommand _openExternalResourceCommand;
        public ICommand OpenExternalResourceCommand
        {
            get => RelayCommand.GetCommand<InstanceAddon>(ref _openExternalResourceCommand, (addon) => 
            {
                try
                {
                    Process.Start(addon.WebsiteUrl);
                }
                catch (Exception ex)
                {

                }
            });
        }


        public InstanceCopyErrorsViewModel(IEnumerable<InstanceAddon> uncopiedAddons)
        {
            Model = new(uncopiedAddons);
        }
    }
}
