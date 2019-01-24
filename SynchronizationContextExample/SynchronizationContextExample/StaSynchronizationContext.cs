namespace SynchronizationContextExample
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading;

    public class StaSynchronizationContext : SynchronizationContext, IDisposable
    {
        private readonly BlockingQueue<SendOrPostCallbackItem> mQueue;
        private readonly StaThread mStaThread;
        public StaSynchronizationContext()
        {
            this.mQueue = new BlockingQueue<SendOrPostCallbackItem>();
            this.mStaThread = new StaThread(this.mQueue);
            this.mStaThread.Start();
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            // create an item for execution
            var item = new SendOrPostCallbackItem(d, state, ExecutionType.Send);
            // queue the item
            this.mQueue.Enqueue(item);
            // wait for the item execution to end
            item.ExecutionCompleteWaitHandle.WaitOne();

            // If we captured an exception, throw it in the caller thread, not the Sta thread
            // Using ExceptionDispatcherInfo to capture and rethrow the exception in order to preserve the stack trace
            if (item.ExecutedWithException)
            {
                ExceptionDispatchInfo.Capture(item.Exception).Throw();
            }  
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            // queue the item and don't wait for its execution. This is risky because
            // an unhandled exception will terminate the STA thread. Use with caution.
            SendOrPostCallbackItem item = new SendOrPostCallbackItem(d, state, ExecutionType.Post);
            this.mQueue.Enqueue(item);
        }

        public void Dispose()
        {
            this.mStaThread.Stop();
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}
