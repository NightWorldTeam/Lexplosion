﻿using Lexplosion.Global;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Nightworld;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Lexplosion.Logic.Network
{
    public class NightWorldApi
    {
        private readonly ToServer _toServer;

        /// <summary>
        /// этот класс нужен для декодирования json в GetInstanceManifest
        /// </summary>
        private class ProtectedNightWorldManifest : NightWorldManifest, ProtectedManifest
        {
            public string code { get; set; }
            public string str { get; set; }
        }

        public class FullInstanceInfo : InstanceInfo
        {
            public long DownloadCounts;
            public string WebsiteUrl;
            public List<string> Images;
            public long LastUpdate;
            public ClientType Modloader;
        }

        public class InstanceInfo
        {
            public string Name;
            public string LogoUrl;
            public string Author;
            public long Version;
            public string Description;
            public string Summary;
            public List<NightWorldCategory> Categories;
            public string GameVersion;
        }

        public struct AuthData
        {
            public struct AccessData
            {
                public string type;
                public string data;
            }

            public AccessData accessData;
            public string login;
        }

        public class AuthManifest : ProtectedManifest
        {
            public string login;
            public string UUID;
            public string accesToken;
            public string sessionToken;
            public string accessID;
            public int baseStatus;
            public NwUserBanner banner;
            public long lastNewsId;
            public string code { get; set; }
            public string str { get; set; }
        }

        public NightWorldApi(ToServer toServer)
        {
            _toServer = toServer;
        }

        public NwAuthResult Authorization(AuthData authData)
        {
            string data = JsonConvert.SerializeObject(authData);
            var manifest = _toServer.ProtectedUserRequest<AuthManifest>(LaunсherSettings.URL.Account + "auth", data, out string notComplitedResult);

            if (notComplitedResult != null)
            {
                var response = new NwAuthResult();
                if (notComplitedResult == "ERROR:1")
                {
                    response.Status = AuthCode.DataError;
                    return response;
                }
                else if (notComplitedResult == "ERROR:2")
                {
                    response.Status = AuthCode.SessionExpired;
                    return response;
                }
                else
                {
                    response.Status = AuthCode.NoConnect;
                    return response;
                }
            }
            else if (manifest == null)
            {
                return new NwAuthResult()
                {
                    Status = AuthCode.NoConnect
                };
            }
            else
            {
                var response = new NwAuthResult()
                {
                    Status = AuthCode.Successfully,
                    Login = manifest.login,
                    UUID = manifest.UUID,
                    AccesToken = manifest.accesToken,
                    SessionToken = manifest.sessionToken,
                    AccessID = manifest.accessID,
                    BaseStatus = manifest.baseStatus,
                    LastNewsId = manifest.lastNewsId,
                    Banner = manifest.banner,
                };

                return response;
            }
        }

        public Dictionary<string, InstanceInfo> GetInstancesList()
        {
            try
            {
                string answer = _toServer.HttpPost(LaunсherSettings.URL.ModpacksData);
                Dictionary<string, InstanceInfo> list = JsonConvert.DeserializeObject<Dictionary<string, InstanceInfo>>(answer);

                return list;
            }
            catch
            {
                return new Dictionary<string, InstanceInfo>();
            }
        }

        public int GetInstanceVersion(string id)
        {
            try
            {
                string answer = _toServer.HttpPost(LaunсherSettings.URL.ModpacksData + WebUtility.UrlEncode(id) + "/version");
                Int32.TryParse(answer, out int idInt);

                return idInt;
            }
            catch
            {
                return 0;
            }
        }

        public FullInstanceInfo GetInstanceInfo(string id)
        {
            try
            {
                string answer = _toServer.HttpPost(LaunсherSettings.URL.ModpacksData + WebUtility.UrlEncode(id) + "/info");
                FullInstanceInfo info = JsonConvert.DeserializeObject<FullInstanceInfo>(answer);

                return info;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Получет манифест для NightWorld модпаков
        /// </summary>
        /// <param name="instanceId"></param>
        public NightWorldManifest GetInstanceManifest(string instanceId) // TODO: одинаковые блоки кода в этих двух функция вынести в другую функцию
        {
            var filesData = _toServer.ProtectedRequest<ProtectedNightWorldManifest>(LaunсherSettings.URL.ModpacksData + WebUtility.UrlEncode(instanceId) + "/manifest");

            if (filesData == null) return null;

            NightWorldManifest ret = new NightWorldManifest
            {
                data = filesData.data,
                version = filesData.version,
                CustomVersion = filesData.CustomVersion
            };

            return ret;
        }

        public VersionManifest GetVersionManifest(string modpackId)
        {
            var filesData = _toServer.ProtectedRequest<ProtectedVersionManifest>(LaunсherSettings.URL.ModpacksData + modpackId + "/versionManifest");

            if (filesData == null) return null;

            Dictionary<string, LibInfo> libraries = new Dictionary<string, LibInfo>();
            foreach (string lib in filesData.libraries.Keys)
            {
                if (filesData.libraries[lib].os == null || filesData.libraries[lib].os.Contains("windows"))
                {
                    libraries[lib] = new LibInfo
                    {
                        notArchived = filesData.libraries[lib].notArchived,
                        url = filesData.libraries[lib].url,
                        obtainingMethod = filesData.libraries[lib].obtainingMethod,
                        isNative = filesData.libraries[lib].isNative
                    };
                }
            }

            return new VersionManifest
            {
                version = filesData.version,
                libraries = libraries
            };
        }

        public PlayerData GetPlayerData(string uuid)
        {
            string data = _toServer.HttpPost(LaunсherSettings.URL.Account + "getPlayerData", new Dictionary<string, string>
            {
                ["playerUUID"] = uuid
            });

            if (data != null)
            {
                try
                {
                    return JsonConvert.DeserializeObject<PlayerData>(data);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Выполняет поиск пользователей.
        /// </summary>
        /// <param name="UUID">uuid. Его можно получить в GlobalData.User.UUID</param>
        /// <param name="sessionToken">Токен сессии. Его можно получить в GlobalData.User.SessionToken</param>
        /// <param name="page">Страница (начиная с нуля)</param>
        /// <param name="filter">Поиск конкретного логина. Если не нужен, то пустая строка</param>
        public UsersCatalogPage FindUsers(string UUID, string sessionToken, uint page, string filter)
        {
            string data = _toServer.HttpPost(LaunсherSettings.URL.UserApi + "findUsers?page=" + page + "&user_login=" + filter, new Dictionary<string, string>
            {
                ["UUID"] = UUID,
                ["sessionToken"] = sessionToken
            });

            UsersCatalogPage result = new UsersCatalogPage()
            {
                Data = null,
                NextPage = false
            };

            if (data != null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<UsersCatalogPage>(data);
                }
                catch { }
            }

            if (result.Data == null)
            {
                result.Data = new List<NwUser>();
                result.NextPage = false;
            }

            return result;
        }

        /// <summary>
        /// Получает спсиок друзей пользователя
        /// </summary>
        /// <param name="UUID">uuid. Его можно получить в GlobalData.User.UUID</param>
        /// <param name="sessionToken">Токен сессии. Его можно получить в GlobalData.User.SessionToken</param>
        /// <param name="userLogin">
        /// Ник пользователя, список друзей которого нужно получить. 
        /// Если нужно получить список своих друзей, то нужно передать свой ник.
        /// </param>
        /// <returns></returns>
        public List<NwUser> GetFriends(string UUID, string sessionToken, string userLogin)
        {
            string data = _toServer.HttpPost(LaunсherSettings.URL.UserApi + "getFreinds?login=" + userLogin, new Dictionary<string, string>
            {
                ["UUID"] = UUID,
                ["sessionToken"] = sessionToken
            });

            List<NwUser> result = null;

            if (data != null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<List<NwUser>>(data);
                }
                catch { }
            }

            return result ?? new List<NwUser>();
        }

        /// <summary>
        /// Получает списки входящих и исходящих заявок в друзья
        /// </summary>
        /// <param name="UUID">uuid. Его можно получить в GlobalData.User.UUID</param>
        /// <param name="sessionToken">Токен сессии. Его можно получить в GlobalData.User.SessionToken</param>
        /// <returns></returns>
        public FriendRequests GetFriendRequests(string UUID, string sessionToken)
        {
            string data = _toServer.HttpPost(LaunсherSettings.URL.UserApi + "getFriendRequests", new Dictionary<string, string>
            {
                ["UUID"] = UUID,
                ["sessionToken"] = sessionToken
            });

            FriendRequests result = new FriendRequests
            {
                Outgoing = null,
                Incoming = null
            };

            if (data != null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<FriendRequests>(data);
                }
                catch { }
            }

            if (result.Incoming == null) result.Incoming = new List<NwUser>();
            if (result.Outgoing == null) result.Outgoing = new List<NwUser>();

            return result;
        }

        /// <summary>
        /// Если пользователь с ником login отправлял заявку в друзья, то принимает её. 
        /// Иначе отправляет отправляет аему заявку.
        /// </summary>
        /// <param name="UUID">uuid. Его можно получить в GlobalData.User.UUID</param>
        /// <param name="sessionToken">Токен сессии. Его можно получить в GlobalData.User.SessionToken</param>
        /// <param name="login">Логин потльзователя, которому нужно отправить заявку в друзья или принять её.</param>
        public void AddFriend(string UUID, string sessionToken, string login)
        {
            _toServer.HttpPost(LaunсherSettings.URL.UserApi + "addFriend?user_login=" + login, new Dictionary<string, string>
            {
                ["UUID"] = UUID,
                ["sessionToken"] = sessionToken
            });
        }

        /// <summary>
        /// Если пользователь с ником login отправлял заявку в друзья, то отвергает её. 
        /// Если пользователю с ником login была отправлена заявка, то отменяет её.
        /// Если же этот пользователь уже в друзьях, то удаляет его.
        /// </summary>
        /// <param name="UUID">uuid. Его можно получить в GlobalData.User.UUID</param>
        /// <param name="sessionToken">Токен сессии. Его можно получить в GlobalData.User.SessionToken</param>
        /// <param name="login">Логин пользователя.</param>
        public void RemoveFriend(string UUID, string sessionToken, string login)
        {

            _toServer.HttpPost(LaunсherSettings.URL.UserApi + "removeFriend?user_login=" + login, new Dictionary<string, string>
            {
                ["UUID"] = UUID,
                ["sessionToken"] = sessionToken
            });
        }

        /// <summary>
        /// Возврщает версию лаунчера на сервере. -1 если произошла ошибка
        /// </summary>
        public int CheckLauncherUpdates(int timeout = 10000)
        {
            var result = _toServer.HttpGet(LaunсherSettings.URL.LauncherParts + "launcherVersion.html", timeout: timeout);
            if (result == null) return -1;

            if (Int32.TryParse(result, out int res)) return res;

            return -1;
        }

        public bool ServerIsOnline()
        {
            var isMirrorMode = _toServer.IsMirrorModeToNw;
            var result = _toServer.HttpPost(LaunсherSettings.URL.Base + "api/onlineStatus", timeout: 10000);

            if (result == null && !isMirrorMode)
            {
                _toServer.ChangeToMirrorMode();
                return ServerIsOnline();
            }

            return result == "online";
        }

        public List<NewsModel> GetUnseenNews(long lastViewedNewsId)
        {
            var data = _toServer.HttpGet(LaunсherSettings.URL.Base + "api/news/getUnseenNews");
            if (data == null) return new();

            try
            {
                return JsonConvert.DeserializeObject<List<NewsModel>>(data);
            }
            catch
            {
                return new();
            }
        }

    }
}
