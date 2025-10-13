using Lexplosion.Global;
using System;
using System.Net;

namespace Lexplosion.Logic.Network
{
    using WebSockets;

    public static class CommandReceiver
    {
        #region Events

        private static Action<string> _openModpackPage;
        public static event Action<string> OpenModpackPage
        {
            add => _openModpackPage += value;
            remove => _openModpackPage -= value;
        }

        private static Action<string, MicrosoftAuthRes> _microsoftAuthPassed;
        public static event Action<string, MicrosoftAuthRes> MicrosoftAuthPassed
        {
            add => _microsoftAuthPassed += value;
            remove => _microsoftAuthPassed -= value;
        }

        private static Action _lexplosionOpened;
        public static event Action OnLexplosionOpened
        {
            add => _lexplosionOpened += value;
            remove => _lexplosionOpened -= value;
        }

        #endregion

        private static WebSocketServer _server;

        private static string CommandHandler(string text)
        {
            if (text.StartsWith("$openModpackPage:"))
            {
                if (_openModpackPage != null)
                {
                    try
                    {
                        string modpackId = text.Replace("$openModpackPage:", "");
                        _openModpackPage(modpackId);
                        return "OK";
                    }
                    catch
                    {
                        return "NO_AUTH";
                    }
                }
                else
                {
                    return "NO_AUTH";
                }
            }
            else if (text.StartsWith("$microsoftAuth:"))
            {
                text = text.Replace("$microsoftAuth:", "");
                if (text.StartsWith("$result:ERROR-0"))
                {
                    _microsoftAuthPassed?.Invoke("", MicrosoftAuthRes.UnknownError);
                }
                else if (text.StartsWith("$result:ERROR-1"))
                {
                    _microsoftAuthPassed?.Invoke("", MicrosoftAuthRes.UserDenied);
                }
                else if (text.StartsWith("$result:ERROR-2"))
                {
                    _microsoftAuthPassed?.Invoke("", MicrosoftAuthRes.Minor);
                }
                else if (text.StartsWith("$result:ERROR-3"))
                {
                    _microsoftAuthPassed?.Invoke("", MicrosoftAuthRes.NoXbox);
                }
                else if (text.StartsWith("$result:OK,"))
                {
                    string data = text.Replace("$result:OK,", "");
                    _microsoftAuthPassed?.Invoke(data, MicrosoftAuthRes.Successful);
                }
            }
            else if (text.StartsWith("$lexplosionOpened:"))
            {
                _lexplosionOpened?.Invoke();
                return "OK";
            }

            return null;
        }

        public static bool StartCommandServer()
        {
            if (HttpListener.IsSupported)
            {
                _server = new WebSocketServer();
                _server.ReceivedData += CommandHandler;
                return _server.Run(LaunсherSettings.CommandServerPort);
            }

            return false;
        }

        public static void StopCommandServer()
        {
            _server?.Stop();
        }
    }
}
