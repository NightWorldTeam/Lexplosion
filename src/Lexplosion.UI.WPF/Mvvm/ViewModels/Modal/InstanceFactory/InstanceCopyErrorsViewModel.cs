using Lexplosion.Logic.Management.Addons;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Modal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal
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


        public InstanceCopyErrorsViewModel(AppCore appCore, string instanceName, IEnumerable<InstanceAddon> uncopiedAddons)
        {
            Model = new(appCore, instanceName, uncopiedAddons);
        }
    }
}
