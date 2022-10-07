using System;
using System.Net.Http;
using System.Threading;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;

namespace Lexplosion.Logic.Management
{
    public partial class Player : VMBase
    {
        public string UUID { get; }
        private Action<Player> _unkickedAction = null;

        private string _nickname = "Player";
        /// <summary>
        /// Содержит ник пользователя.
        /// <para>Стаднартное значение: Player</para>
        /// </summary>
        public string Nickname
        {
            get => _nickname;
            private set
            {
                _nickname = value;
                OnPropertyChanged();
            }
        }

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
        private Action _unkickMethod;

        public Player(string uuid, Action kickMethod, Action unkickMethod)
        {
            UUID = uuid;
            _kickMethod = kickMethod;
            _unkickMethod = unkickMethod;

            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                PlayerData data = NightWorldApi.GetPlayerData(uuid);
                if (data != null)
                {
                    Nickname = data.Nickname;
                    ProfileUrl = data.ProfileUrl;
                }

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        Skin = httpClient.GetByteArrayAsync(data.AvatarUrl).Result;
                    }
                }
                catch { }
            });
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
            _unkickMethod();
            _unkickedAction?.Invoke(this);
            IsKicked = false;
        }

        /// <summary>
        /// Присваивает значение делегату. Поменять значение можно только единажды.
        /// </summary>
        /// <param name="action"></param>
        public void SetUnkickedAction(Action<Player> action)
        {
            if (_unkickedAction != null)
                return;

            _unkickedAction = action;
        }
    }
}
