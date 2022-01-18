using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Logic
{
    class TasksManager
    {
        private List<ThreadStart> NewTasks = new List<ThreadStart>();
        private Semaphore NewTasksBlock = new Semaphore(1, 1);
        private Semaphore priorityChangedBlock = new Semaphore(1, 1);
        private AutoResetEvent WaitNewTasks = new AutoResetEvent(false);
        private Thread MainThread;

        private bool work = false;

        private List<(int, int)> priorityChanged = new List<(int, int)>();

        public delegate void TaskStatus(int id);
        public event TaskStatus TaskBegin;
        public event TaskStatus TaskEnd;

        public int AddTask(ThreadStart ThreadFunc)
        {
            NewTasksBlock.WaitOne();
            int key = NewTasks.Count;
            NewTasks.Add(ThreadFunc);
            WaitNewTasks.Set();
            NewTasksBlock.Release();

            return key;
        }

        public void ChangePriority(int id_1, int id_2)
        {
            priorityChangedBlock.WaitOne();
            priorityChanged.Add((id_1, id_2));
            priorityChangedBlock.Release();
        }

        public void Init()
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

        public void Stop()
        {
            work = false;
            MainThread.Join();
        }
    }
}
