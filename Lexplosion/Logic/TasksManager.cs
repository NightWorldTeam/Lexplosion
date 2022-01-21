using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Logic
{
    static class TasksManager
    {
        private static List<ThreadStart> NewTasks = new List<ThreadStart>();
        private static Semaphore NewTasksBlock = new Semaphore(1, 1);
        private static Semaphore priorityChangedBlock = new Semaphore(1, 1);
        private static AutoResetEvent WaitNewTasks = new AutoResetEvent(false);
        private static Thread MainThread;

        private static bool work = false;

        private static List<(int, int)> priorityChanged = new List<(int, int)>();

        public static delegate void TaskStatus(int id);
        public static event TaskStatus TaskBegin;
        public static event TaskStatus TaskEnd;

        public static int AddTask(ThreadStart ThreadFunc)
        {
            NewTasksBlock.WaitOne();
            int key = NewTasks.Count;
            NewTasks.Add(ThreadFunc);
            WaitNewTasks.Set();
            NewTasksBlock.Release();

            return key;
        }

        public static void ChangePriority(int id_1, int id_2)
        {
            priorityChangedBlock.WaitOne();
            priorityChanged.Add((id_1, id_2));
            priorityChangedBlock.Release();
        }

        public static void Init()
        {
            work = true;
            MainThread = new Thread(delegate ()
            {
                List<ThreadStart> tasks;
                while (work)
                {
                    WaitNewTasks.WaitOne();
                    NewTasksBlock.WaitOne();

                    tasks = new List<ThreadStart>(NewTasks);
                    NewTasks = new List<ThreadStart>();

                    NewTasksBlock.Release();

                    for (int i = 0; i < tasks.Count; i++)
                    {
                        TaskBegin?.Invoke(i);
                        tasks[i]();
                        TaskEnd?.Invoke(i);

                        priorityChangedBlock.WaitOne();
                        if (priorityChanged.Count != 0)
                        {
                            foreach (var parent in priorityChanged)
                            {
                                if (parent.Item1 > i || parent.Item2 > i)
                                {
                                    var temp = tasks[parent.Item1];
                                    tasks[parent.Item1] = tasks[parent.Item2];
                                    tasks[parent.Item2] = temp;
                                }
                            }
                            priorityChanged = new List<(int, int)>();
                        }
                        priorityChangedBlock.Release();
                    }

                }

            });

            MainThread.Start();
        }

        public static void Stop()
        {
            work = false;
            MainThread.Join();
        }
    }
}
