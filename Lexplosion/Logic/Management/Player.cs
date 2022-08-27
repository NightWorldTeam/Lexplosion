using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Lexplosion.Gui;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;

namespace Lexplosion.Logic.Management
{
    public class Player : VMBase
    {
        /// <summary>
        /// Содержит ник пользователя.
        /// <para>Стаднартное значение: Player</para>
        /// </summary>
        public string Nickname { get; private set; } = "Player";

        private byte[] _skin = null;
        /// <summary>
        /// Содержит аватар пользователя в виде массива байт.
        /// </summary>
        public byte[] Skin
        {
            get => _skin;
            private set
            {
                _skin = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Ссылка на профиль 
        /// </summary>
        public string ProfileUrl { get; private set; } = null;

        private bool _isKicked;
        /// <summary>
        /// Отвечает на вопрос был ли кикнут пользователь.
        /// </summary>
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
                        using (var httpClient = new HttpClient())
                        {
                            Skin = httpClient.GetByteArrayAsync(data.AvatarUrl).Result;
                        }
                    }
                    catch { }
                });

                //ThreadPool.QueueUserWorkItem(delegate (object state)
                //{
                //    try
                //    {
                //        using (var webClient = new WebClient())
                //        {
                //            Skin = webClient.DownloadData(data.AvatarUrl);
                //        }
                //    }
                //    catch { }
                //});
            }
        }

        /// <summary>
        /// Вызывает метод Kick или Unkick взависимости от статуса пользователя.
        /// </summary>
        private void AccessChange() 
        {
            if (IsKicked) 
                Unkick();
            else 
                Kick();
        }

        /// <summary>
        /// Кикает пользователя
        /// </summary>
        private void Kick()
        {
            IsKicked = true;
            _kickMethod();
        }

        /// <summary>
        /// Снимает кик.
        /// </summary>
        private void Unkick()
        {
            IsKicked = false;
        }
    }
}
