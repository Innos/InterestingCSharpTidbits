namespace SynchronizationContextExample
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    public class Params
    {
        public string Output { get; set; }
        public int CallCounter { get; set; }
        public int OriginalThread { get; set; }
    }

    // This project is a slightly modified version of https://www.codeproject.com/Articles/32113/Understanding-SynchronizationContext-Part-II
    public class Program
    {
        private static int mCount = 0;
        private static StaSynchronizationContext mStaSyncContext = null;
        public static void Main(string[] args)
        {

            mStaSyncContext = new StaSynchronizationContext();
            for (int i = 0; i < 100; i++)
            {
                ThreadPool.QueueUserWorkItem(NonStaThread);

            }
            Console.WriteLine("Processing");
            Console.WriteLine("Press any key to dispose SyncContext");
            Console.ReadLine();
            mStaSyncContext.Dispose();
        }


        private static void NonStaThread(object state)
        {
            int id = Thread.CurrentThread.ManagedThreadId;

            for (int i = 0; i < 10; i++)
            {
                var param = new Params { OriginalThread = id, CallCounter = i };
                try
                {
                    mStaSyncContext.Send(RunOnStaThread, param);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Debug.Assert(param.Output == "Processed", "Unexpected behavior by STA thread");
            }
        }

        private static void RunOnStaThread(object state)
        {
            //throw new InvalidOperationException("Something went wrong");
            mCount++;
            Console.WriteLine(mCount);
            int id = Thread.CurrentThread.ManagedThreadId;
            var args = (Params)state;
            Console.WriteLine("STA id " + id + " original thread " + args.OriginalThread + " call count " + args.CallCounter);
            //Trace.WriteLine("STA id " + id + " original thread " + args.OriginalThread + " call count " + args.CallCounter);
            args.Output = "Processed";

        }
    }
}
