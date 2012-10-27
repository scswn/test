using System;
using System.Collections.Generic;
using System.Text;

namespace VolumeFluctuation
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Volume Fluctuation Index Calculator (v{0}).",
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                Console.WriteLine("");
                Console.WriteLine("Usage: volfluc [filename]");
                Console.WriteLine("");
                Console.WriteLine("Example: volfluc test.mp4");
                Console.WriteLine("Output:  0.500");
#if DEBUG
                Console.ReadKey();
#endif
                Environment.Exit(0);
            }

#if DEBUG
            Console.ReadKey();
#endif
            Environment.Exit(0);
        }
    }
}
