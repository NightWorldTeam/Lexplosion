using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lexplosion
{
    static partial class Runtime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TaskRun(ThreadStart threadFunc) => new Thread(threadFunc).Start();

        [Conditional("DEBUG")]
        public static void DebugWrite<T>(T line)
        {
#if DEBUG
            System.Console.WriteLine(line);
#endif
        }

        [Conditional("DEBUG")]
        public static void DebugWrite()
        {
#if DEBUG
            System.Console.WriteLine();
#endif
        }
    }
}
