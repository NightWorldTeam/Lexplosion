using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace Lexplosion.Logic
{

    class StreamList
    {
        public class ThreadInfo
        {
            public bool isImportant;
            public Thread thread;
        }

        private Dictionary<int, ThreadInfo> data;
        private Semaphore sem;

        public StreamList()
        {
            data = new Dictionary<int, ThreadInfo>();
            sem = new Semaphore(1, 1);
        }

        public void StopThreads()
        {
            sem.WaitOne();


            int[] keys = new int[data.Count];
            data.Keys.CopyTo(keys, 0);

            foreach (var key in keys)
            {
                if (!data[key].isImportant)
                {
                    data[key].thread.Abort();
                }

                data.Remove(key);

            }

            sem.Release();

        }

        public void Wait()
        {
            sem.WaitOne();
        }

        public bool ContainsKey(int key)
        {
            return data.ContainsKey(key);
        }

        public int Count()
        {
            return data.Count;
        }

        public void Release()
        {
            sem.Release();
        }

        public ThreadInfo this[int index]
        {
            get
            {
                return data[index];
            }

            set
            {
                data[index] = value;
            }
        }

        public int Add(ThreadInfo obj)
        {

            Random rnd = new Random();

            int key;
            do
            {
                key = rnd.Next();
            } while (data.ContainsKey(key));

            data[key] = obj;

            return key;

        }

        public void RemoveAt(int index)
        {
            sem.WaitOne();
            try
            {
                data.Remove(index);
            }
            catch { }
            sem.Release();
        }
    }
}
