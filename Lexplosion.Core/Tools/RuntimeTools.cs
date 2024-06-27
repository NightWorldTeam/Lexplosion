using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Lexplosion
{
    public static partial class Runtime
    {
        private static int _importantThreads = 0;
        private static object _locker = new object();
        private static ManualResetEvent _waitingClosing = new ManualResetEvent(true);

        /// <summary>
        /// Добавляет приоритетную задачу. При выключении лаунчер будет ждать завершения всех приоритетных задач.
        /// </summary>
        public static void AddImportantTask()
        {
            lock (_locker)
            {
                _importantThreads++;
                _waitingClosing.Reset();
            }
        }

        /// <summary>
        /// Сообщает что приоритетная задача выполнена.
        /// </summary>
        public static void RemoveImportantTask()
        {
            lock (_locker)
            {
                _importantThreads--;
                if (_importantThreads == 0)
                {
                    _waitingClosing.Set();
                }
            }
        }

        private static bool _isDebugMode = false;
        private static FileStream _logFileStream = null;

        public static bool IsDebugMode
        {
            get
            {
                return _isDebugMode;
            }
            set
            {
                _isDebugMode = value;
                try
                {
                    if (value)
                    {
                        if (_logFileStream != null)
                        {
                            _logFileStream.Close();
                        }

                        string fileName = Global.LaunсherSettings.LauncherDataPath + "/debug-log_" + DateTime.Now.ToString("dd.MM.yyyy-h.mm.ss") + ".log";
                        _logFileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                    }
                    else
                    {
                        _logFileStream.Close();
                        _logFileStream = null;
                    }
                }
                catch { }
            }
        }

        private static bool _exitIsCanceled = false;
        private static bool _inExited = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TaskRun(ThreadStart threadFunc) => new Thread(threadFunc).Start();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormDebugString(object line, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            string prefix = $"[{Path.GetFileName(sourceFilePath)}:{memberName}:{sourceLineNumber}]";

            var nowTime = DateTimeOffset.Now;
            string time = $"[{nowTime.Hour}:{nowTime.Minute}:{nowTime.Second}.{nowTime.Millisecond}]";

            return $"{time} {prefix} {line}";
        }

        [Conditional("DEBUG")]
        public static void DebugConsoleWrite<T>(T line, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0, ConsoleColor color = ConsoleColor.White)
        {
#if DEBUG
            string str = FormDebugString(line, memberName, sourceFilePath, sourceLineNumber);
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
#endif
        }

        private static object _debugWriteLocker = new object();

        public static void DebugWrite<T>(T line, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0, ConsoleColor color = ConsoleColor.White)
        {
#if DEBUG
            string str = FormDebugString(line, memberName, sourceFilePath, sourceLineNumber);
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
#endif

            if (IsDebugMode && _logFileStream != null)
            {
#if !DEBUG
                string str = FormDebugString(line, memberName, sourceFilePath, sourceLineNumber);
#endif
                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    lock (_debugWriteLocker)
                    {
                        FileStream stream = _logFileStream;
                        if (IsDebugMode && stream != null)
                        {
                            try
                            {
                                var bytes = Encoding.UTF8.GetBytes(str + "\n");
                                stream.Write(bytes, 0, bytes.Length);
                            }
                            catch { }
                        }
                    }
                });
            }
        }

        [Conditional("DEBUG")]
        public static void DebugWrite()
        {
#if DEBUG
            Console.WriteLine();
#endif
        }
    }
}
