namespace SynchronizationContextExample
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using System.Threading;

    internal interface IQueueReader<out T> : IDisposable
    {
        T Dequeue();
        void ReleaseReader();
    }

    internal interface IQueueWriter<in T> : IDisposable
    {
        void Enqueue(T data);
    }


    internal class BlockingQueue<T> : IQueueReader<T>, IQueueWriter<T>
    {
        // use a .NET queue to store the data
        private readonly Queue<T> mQueue = new Queue<T>();
        // create a semaphore that contains the items in the queue as resources.
        // initialize the semaphore to zero available resources (empty queue).
        private Semaphore mSemaphore = new Semaphore(0, int.MaxValue);
        // a event that gets triggered when the reader thread is exiting
        private readonly ManualResetEvent mKillThread = new ManualResetEvent(false);
        // wait handles that are used to unblock a Dequeue operation.
        // Either when there is an item in the queue
        // or when the reader thread is exiting.
        private readonly WaitHandle[] mWaitHandles;

        public BlockingQueue()
        {
            this.mWaitHandles = new WaitHandle[2] { this.mSemaphore, this.mKillThread };
        }
        public void Enqueue(T data)
        {
            lock (this.mQueue) this.mQueue.Enqueue(data);
            // add an available resource to the semaphore,
            // because we just put an item
            // into the queue.
            this.mSemaphore.Release();
        }

        public T Dequeue()
        {
            // wait until there is an item in the queue
            WaitHandle.WaitAny(this.mWaitHandles);
            lock (this.mQueue)
            {
                if (this.mQueue.Count > 0)
                    return this.mQueue.Dequeue();
            }
            return default(T);
        }

        public void ReleaseReader()
        {
            this.mKillThread.Set();
        }


        void IDisposable.Dispose()
        {
            if (this.mSemaphore != null)
            {
                this.mSemaphore.Close();
                this.mQueue.Clear();
                this.mSemaphore = null;
            }
        }
    }
}
