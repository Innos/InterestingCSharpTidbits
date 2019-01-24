namespace ConsoleApplication1
{
    using System;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            var a = DoWorkEventArgs();

            Console.WriteLine("Test");
            Console.WriteLine(a.Status);
            Console.WriteLine(a.Result);
        }

        public static async Task<string> DoWorkEventArgs()
        {
            try
            {
                // Does not propagate Exceptions by itself
                var task = SomeAsyncMethod();

                // Will propagate exceptions caused during the execution of the task as an Aggregate exception
                //var someInt = task.Result;

                // Will propagate exceptions caused during the execution of the task as an Aggregate exception
                //task.Wait();

                // Will propagate the first exception caught during the execution of the task (without being boxed as an Aggregate exception)
                var someInt2 = await task;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex.ToString());
            }


            return "sha";
        }

        public static async Task<int> SomeAsyncMethod()
        {
            throw new InvalidOperationException("Some exception");
        }
    }
}
