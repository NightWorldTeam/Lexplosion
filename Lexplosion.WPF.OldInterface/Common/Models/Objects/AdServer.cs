using System;
using System.Windows;

namespace Lexplosion.Common.Models.Objects
{
    public class AdServer : VMBase
    {
        #region Properties


        public int Id { get; }

        /// <summary>
        /// Base info
        /// </summary>
        public string Name { get; }
        public string Desctiption { get; }
        public string Version { get; }


        /// <summary>
        /// Online Properties
        /// </summary>
        public bool Online { get; }
        public uint OnlineCount { get; }

        /// <summary>
        /// Servers Connect Data
        /// </summary>
        public string Ip { get; }
        public string Port { get; }
        public string FullAdress { get => Ip + ":" + Port; }

        /// <summary>
        /// Logo or Country Flag
        /// </summary>
        public byte[] Logo { get; }

        /// <summary>
        /// TODO:
        /// Будем ли делить сервера по регионом? Например если человек захочет только пиарить сервер на свой регион? Например: СНГ, EU, Asia
        /// Для каждого региона свой топ???
        /// </summary>


        #endregion Properties


        #region Constructors


        public AdServer(string name, string version, string ip, string port, string description = "", bool online = true)
        {
            Name = name;
            Version = version;
            Ip = ip;
            Port = port;
            Desctiption = description;
            Online = online;
            OnlineCount = (uint)new Random().Next(0, 1000);
        }


        #endregion Constructors


        #region Public Methods


        public void ConnectTo()
        {

        }

        public void CopyAddressToClipboard()
        {
            Clipboard.SetText(FullAdress);
        }


        #endregion Public Methods
    }
}
