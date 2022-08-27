using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Lexplosion.Gui;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;

namespace Lexplosion.Logic.Management
{
    public class Player : VMBase
    {
        public string Nickname { get; private set; } = "Player";

        private byte[] _skin = null;
        public byte[] Skin
        {
            get => _skin;
            private set
            {
                _skin = value;
                OnPropertyChanged();
            }
        }

        public string ProfileUrl { get; private set; } = null;

        private bool _isKicked;
        public bool IsKicked 
        { 
            get => _isKicked; private set 
            {
                _isKicked = value;
                OnPropertyChanged();
            } 
        }

        private Action _kickMethod;
        public RelayCommand AccessChangeAction 
        { 
            get => new RelayCommand(obj => 
            {
                AccessChange();
            });
        }

        public Player(string uuid, Action kickMethod)
        {
            _kickMethod = kickMethod;

            PlayerData data = NightWorldApi.GetPlayerData(uuid);
            if (data != null)
            {
                Nickname = data.Nickname;
                ProfileUrl = data.ProfileUrl;

                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    try
                    {
                        using (var webClient = new WebClient())
                        {
                            Skin = webClient.DownloadData(data.AvatarUrl);
                        }
                    }
                    catch { }
                });
            }
        }

        private void AccessChange() 
        {
            if (IsKicked) 
                Unkick();
            else 
                Kick();
        }

        private void Kick()
        {
            IsKicked = true;
            _kickMethod();
        }

        private void Unkick()
        {
            IsKicked = false;
        }
    }
}
