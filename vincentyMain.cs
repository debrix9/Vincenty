using System;
using System.Reflection.Metadata;




namespace vincenty
{

    public static class constants
    {
        public const double a = 6378137.0;
        public const double b = 6356752.314245;
    }
    class vincentyMain
    {
        

        static void Main(string[] args)
        {
       
            // Calculate flattening coefficient
            // Example of processing arguments
            if (args.Length > 0)
            {
                Console.WriteLine("Arguments passed to the program:");
                foreach (var arg in args)
                {
                    Console.WriteLine(arg);
                }
            }
            else
            {
                Console.WriteLine("No arguments were passed.");
            }

            // Keeping the console open
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void DisplayCurrentDate()
        {
            Console.WriteLine($"Today's date is: {DateTime.Now:MMMM dd, yyyy}");
        }
    }
}
