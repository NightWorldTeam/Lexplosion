using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.ServerProfile
{
    public class ServerFieldInfo<T> : LeftPanelFieldInfo
    {
        private readonly LeftPanelFieldInfo _info;

        public ServerFieldInfo(string name, T value, Func<T, string>? converter = null)
            : base(name, converter == null ? value.ToString() : converter(value))
        { }
    }

    public sealed class ServerProfileLeftPanelViewModel : LeftPanelViewModel
    {
        private readonly AppCore _appCore;

        public MinecraftServerInstance ServerModel { get; }

        private ObservableCollection<LeftPanelFieldInfo> _additionalInfo = [];
        public IEnumerable<LeftPanelFieldInfo> AdditionalInfo { get => _additionalInfo; }



        #region Commands


        public ICommand BackCommand { get; }


        #endregion Commands


        public ServerProfileLeftPanelViewModel(AppCore appCore, MinecraftServerInstance minecraftServerInstance, ICommand backCommand)
        {
            _appCore = appCore;
            BackCommand = backCommand;
            ServerModel = minecraftServerInstance;
            GenerateAdditionalInfo();
        }


        #region Private Methods


        private void GenerateAdditionalInfo()
        {
            _appCore.UIThread.Invoke(() =>
            {
                _additionalInfo.Clear();
                _additionalInfo.Add(new InstanceFieldInfo<int>("Online:", ServerModel.OnlineCount));
                _additionalInfo.Add(new InstanceFieldInfo<string>("Status:", ServerModel.IsOnline ? "online" : "offline"));
                _additionalInfo.Add(new InstanceFieldInfo<string>("Version:", ServerModel.GameVersion));
            });
        }


        #endregion Private Methods
    }
}
