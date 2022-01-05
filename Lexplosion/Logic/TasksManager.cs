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
        private AutoResetEvent WaitNewTasks = new AutoResetEvent(false);
        private Thread MainThread;

        private bool work = false;

        public void AddTask(ThreadStart ThreadFunc)
        {
            NewTasksBlock.WaitOne();
            NewTasks.Add(ThreadFunc);
            WaitNewTasks.Set();
            NewTasksBlock.Release();
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
                    tasks = new List<ThreadStart>();
                    NewTasksBlock.WaitOne();

                    foreach(ThreadStart task in NewTasks)
                    {
                        tasks.Add(task);
                    }

                    NewTasksBlock.Release();

                    foreach (ThreadStart task in tasks)
                    {
                        task();
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
