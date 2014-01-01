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
            proc.StartInfo.Arguments = "-i \"" + path + "\" -vn -ar 44100 -ac 1 -f f32le -";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            proc.ErrorDataReceived += new DataReceivedEventHandler(proc_ErrorDataReceived);

            proc.Start();

            proc.BeginErrorReadLine();

            Process(proc.StandardOutput.BaseStream);

            proc.WaitForExit(10000); // 10s

            if (!proc.HasExited)
            {
                proc.Kill();
                Environment.Exit(1);
            }

#if DEBUG
            Console.ReadKey();
#endif
            Environment.Exit(0);
        }

        static void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                // Console.WriteLine(e.Data);
                // do nothing
            }
        }

        static void Process(Stream stream)
        {
            Calculator calculator = new Calculator();

            calculator.Init(44100, 1);

            int didread;
            int offset = 0;
            byte[] buffer = new byte[sizeof(Single) * (1024 + 1)];

            int length, residual_length;

            while ((didread = stream.Read(buffer, offset, sizeof(Single) * 1024)) != 0)
            {
                length = offset + didread;

                residual_length = length % sizeof(Single);

                if (residual_length == 0) {
                    calculator.Apply(buffer, length);

                    offset = 0;
                } else {
                    length -= residual_length;
                    calculator.Apply(buffer, length);

                    Array.Copy(buffer, length, buffer, 0, residual_length);
                    offset = residual_length;
                }
            }

            calculator.Calc();
        }
    }
}
