﻿namespace Lexplosion.UI.WPF.Core
{
    public enum LogType
    {
        Info,
        InfoGame,
        Warning,
        Error,
    }

    public class ConsoleLog
    {
        public string Message { get; set; }
        public LogType Type { get; set; }
        public string Time { get; set; }
        public bool HasTime { get; set; }

        public ConsoleLog(string message)
        {
            Message = message;

            if (message.Contains("/INFO"))
            {
                Type = LogType.InfoGame;
            }
            else if (message.Contains("/WARN"))
            {
                Type = LogType.Warning;
            }
            else if (message.Contains("/ERROR") || message.Contains("Exception: "))
            {
                Type = LogType.Error;
            }
        }
    }
}
