using System;
using System.Net.Http;
using System.Threading;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;

namespace Lexplosion.Logic.Management
{
    public partial class Player : VMBase
    {
        public enum UserAction
        {
            Kick,
            Unkick
        }

        public readonly Action _kickMethod;
        public readonly Action _unkickMethod;


        public string UUID { get; }

        /// <summary>
        /// Содержит ник пользователя.
        /// <para>Стаднартное значение: Player</para>
        /// </summary>
        public string Nickname { get; set; } = "Player";

        /// <summary>
        /// Содержит аватар пользователя в виде массива байт.
        /// </summary>
        public byte[] Skin { get; set; } = null;

        /// <summary>
        /// Ссылка на профиль 
        /// </summary>
        public string ProfileUrl { get; private set; } = null;

        /// <summary>
        /// Отвечает на вопрос был ли кикнут пользователь.
        /// </summary>
        public bool IsKicked { get; set; } = false;

        public Player(string uuid)
        {
            UUID = uuid;
        }

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
                    OnPropertyChanged(nameof(Nickname));
                }

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        Skin = httpClient.GetByteArrayAsync(data.AvatarUrl).Result;
                        OnPropertyChanged(nameof(Skin));
                    }
                }
                catch { }
            });
        }
    }
}
