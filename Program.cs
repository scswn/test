using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;

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

            string path = args[0];
            if (!File.Exists(path))
            {
                return;
            }

            Console.Write("{0}, ", Path.GetFileNameWithoutExtension(path));

            Process proc = new Process();
            proc.StartInfo.FileName = @"E:\ffmpeg\ffmpeg.exe";
            // proc.StartInfo.Arguments = "-i \"" + path + "\" -vn -ar 44100 -ac 1 -f wav \"" + path_temp + "\"";
            proc.StartInfo.Arguments = "-i \"" + path + "\" -vn -ar 44100 -ac 1 -f f32le -";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
//            proc.StartInfo.RedirectStandardError = true;

            proc.Start();

            MemoryStream ms = new MemoryStream();
            StreamCopy(proc.StandardOutput.BaseStream, ms);

//            string stderr = proc.StandardError.ReadToEnd();

            proc.WaitForExit(30000); // 30s

            if (!proc.HasExited)
            {
                proc.Kill();
                Environment.Exit(1);
            }

            ms.Position = 0;

            Process(ms);

#if DEBUG
            Console.ReadKey();
#endif
            Environment.Exit(0);
        }

        static void StreamCopy(Stream src, Stream dst)
        {
            // From System.IO.Stream.InternalCopyTo() in .NET Framework 4.0
            int num;
            byte[] buffer = new byte[4096];
            while ((num = src.Read(buffer, 0, buffer.Length)) != 0)
            {
                dst.Write(buffer, 0, num);
            }
        }

        static void Process(Stream stream)
        {
            Calculator calculator = new Calculator();

            calculator.Init(44100, 1);

            byte[] buffer = new byte[4096];
            int length = 0;

            while (stream.Position < stream.Length)
            {
                length = stream.Read(buffer, 0, buffer.Length);
                if (length == 0)
                {
                    break;
                }

                calculator.Apply(buffer, length);
            }

            calculator.Calc();
        }
    }
}
