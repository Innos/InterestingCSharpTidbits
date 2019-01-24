namespace CapturingOfFreeVariablesInClosures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    //Examples from https://stackoverflow.com/questions/271440/captured-variable-in-a-loop-in-c-sharp and
    // https://stackoverflow.com/questions/8170699/c-sharp-action-closure-and-garbage-collection
    // also see Fast Delegates from Jon Skeet's book "C Sharp in depth"
    class Program
    {
        static void Main(string[] args)
        {
            DemonstrateCaptureLogic();
            Console.WriteLine("-----------------");
            DemonstrateCaptureLifetime();
        }


        // This example demonstrates that variables captured in a closure are captured by reference not by value.
        // That is to say that the anonymous method doesn't store a copy of the variable's value at the moment of creation of the
        // anonymous method, but instead stores a reference to the variable
        public static void DemonstrateCaptureLogic()
        {
            List<Func<int>> actions = new List<Func<int>>();

            int variable = 0;
            while (variable < 5)
            {
                actions.Add(() => variable * 2);
                ++variable;
            }

            foreach (var act in actions)
            {
                Console.WriteLine(act.Invoke());
            }
        }

        // This example demonstrates the effects closure variable capturing can have on
        // captured variables' lifetimes
        public static void DemonstrateCaptureLifetime()
        {
            C.M();
            Type cType = typeof(C);
            // When a closure is made the referenced variables are captured in an anonymous class instance, so that the
            // delegate could still have access to them (which it does by actually referencing the anonymous instance)
            var anonymousClass = C.longLived.Target.GetType();
            var nestedClassFields = anonymousClass.GetFields();

            // The anonymous class holding the captured variables is the same for multiple delegates declared in the same
            // scope, so all captured variables now have lifetimes equal to longest living delegate's lifetime 
            foreach (var nestedClassField in nestedClassFields)
            {
                Console.WriteLine("Field Type: " + nestedClassField.FieldType);
                if (nestedClassField.FieldType == typeof(Expensive))
                {
                    Console.WriteLine("The length of the byte[] object captured: " + ((Expensive)nestedClassField.GetValue(C.longLived.Target)).huge.Length);
                }

            }
        }
    }

    class Expensive
    {
        public byte[] huge = new byte[1000000];
    }

    class Cheap
    {
        public int tiny;
    }

    class C
    {
        public static Func<Cheap> longLived;
        public static void M()
        {
            Expensive expensiveLocal = new Expensive();
            Cheap cheapLocal = new Cheap();

            //The closure context that is captured is the same for both delegates,
            // which means it keeps both "expensiveLocal" and "cheapLocal" alive, regardless of
            // whether "shortLived" delegate has already been garbage collected or not
            Func<Expensive> shortLived = () => expensiveLocal;
            C.longLived = () => cheapLocal;
        }
    }
}
