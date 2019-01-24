namespace SynchronizationContextExample
{
    using System.Threading;

    internal class StaThread
    {
        private readonly Thread mStaThread;
        private readonly IQueueReader<SendOrPostCallbackItem> mQueueConsumer;
        private readonly ManualResetEvent mStopEvent = new ManualResetEvent(false);


        internal StaThread(IQueueReader<SendOrPostCallbackItem> reader)
        {
            this.mQueueConsumer = reader;
            this.mStaThread = new Thread(this.Run) { Name = "STA Worker Thread" };
            this.mStaThread.SetApartmentState(ApartmentState.STA);
        }

        internal void Start()
        {
            this.mStaThread.Start();
        }


        internal void Join()
        {
            this.mStaThread.Join();
        }

        private void Run()
        {

            while (true)
            {
                bool stop = this.mStopEvent.WaitOne(0);
                if (stop)
                {
                    break;
                }

                SendOrPostCallbackItem workItem = this.mQueueConsumer.Dequeue();
                workItem?.Execute();
            }
        }

        internal void Stop()
        {
            this.mStopEvent.Set();
            this.mQueueConsumer.ReleaseReader();
            this.mStaThread.Join();
            this.mQueueConsumer.Dispose();

        }
    }
}
