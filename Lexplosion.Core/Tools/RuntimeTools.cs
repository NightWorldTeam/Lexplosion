using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lexplosion
{
    public static partial class Runtime
    {
        private static int _importantThreads = 0;
        private static ManualResetEvent _waitingClosing = new ManualResetEvent(true);
        private static object _locker = new object();

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

        private static bool _exitIsCanceled = false;
        private static bool _inExited = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TaskRun(ThreadStart threadFunc) => new Thread(threadFunc).Start();

        [Conditional("DEBUG")]
        public static void DebugWrite<T>(T line, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
#if DEBUG
            string prefix = "[" + Path.GetFileName(sourceFilePath) + ":" + memberName + ":" + sourceLineNumber + "]";
            var nowTime = DateTimeOffset.Now;
            string time = " [" + nowTime.Hour + ":" + nowTime.Minute + ":" + nowTime.Second + "." + nowTime.Millisecond + "] ";
            Console.WriteLine(prefix + time + line);
#endif
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
