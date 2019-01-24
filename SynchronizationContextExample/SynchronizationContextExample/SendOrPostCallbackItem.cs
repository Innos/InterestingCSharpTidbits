namespace SynchronizationContextExample
{
    using System;
    using System.Threading;

    internal enum ExecutionType
    {
        Post,
        Send
    }

    internal class SendOrPostCallbackItem
    {
        private readonly object mState;
        private readonly ExecutionType mExeType;
        private readonly SendOrPostCallback mMethod;
        private readonly ManualResetEvent mAsyncWaitHandle = new ManualResetEvent(false);
        private Exception mException = null;

        internal SendOrPostCallbackItem(SendOrPostCallback callback,
           object state, ExecutionType type)
        {
            this.mMethod = callback;
            this.mState = state;
            this.mExeType = type;
        }

        internal Exception Exception => this.mException;

        internal bool ExecutedWithException => this.mException != null;

        // this code must run ont the STA thread
        internal void Execute()
        {
            if (this.mExeType == ExecutionType.Send)
                this.Send();
            else
                this.Post();
        }

        // calling thread will block until mAsyncWaitHandle is set
        internal void Send()
        {
            try
            {
                // call the thread
                this.mMethod(this.mState);
            }
            catch (Exception e)
            {
                this.mException = e;
            }
            finally
            {
                this.mAsyncWaitHandle.Set();
            }
        }

        /// <summary>
        /// Unhandled exceptions will terminate the STA thread
        /// </summary>
        internal void Post()
        {
            this.mMethod(this.mState);
        }

        internal WaitHandle ExecutionCompleteWaitHandle => this.mAsyncWaitHandle;
    }
}
