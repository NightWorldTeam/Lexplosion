using System;
using System.Net;

namespace Lexplosion.Logic.Network
{
    static class CommandReceiver
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

        #endregion

        private static string CommandHandler(string text)
        {
            if (text.StartsWith("$openModpackPage:"))
            {
                if (_openModpackPage != null)
                {
                    string modpackId = text.Replace("$openModpackPage:", "");
                    _openModpackPage(modpackId);
                    return "OK";
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

            return null;
        }

        public static void StartCommandServer()
        {
            if (HttpListener.IsSupported)
            {
                Lexplosion.Runtime.TaskRun(delegate ()
                {
                    var ws = new WebSocketServer();
                    ws.ReceivedData += CommandHandler;
                    ws.Run();
                });
            }
        }
    }
}
