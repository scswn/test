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
        private static string path_temp = @"Z:\volfluc_temp\temp.wav";

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

            if (Path.GetExtension(path).ToLowerInvariant() != ".wav")
            {
                if (File.Exists(path_temp))
                {
                    File.Delete(path_temp);
                }

                Process proc = new Process();
//                proc.StartInfo.FileName = @"C:\Program Files\Media Utilities\NeroAac\neroAacDec.exe";
//                proc.StartInfo.Arguments = "-if \"" + path + "\" -of \"" + path_temp + "\"";
                proc.StartInfo.FileName = @"E:\ffmpeg\ffmpeg.exe";
//                proc.StartInfo.Arguments = "-i \"" + path + "\" -vn -ar 44100 -ac 1 -f wav \"" + path_temp + "\"";
                proc.StartInfo.Arguments = "-i \"" + path + "\" -vn -f wav \"" + path_temp + "\"";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();

                proc.WaitForExit(30000); // 30s

                if (!proc.HasExited)
                {
                    proc.Kill();
                    Environment.Exit(1);
                }

                Process(path_temp);
            }
            else
            {
                Process(path);
            }

#if DEBUG
            Console.ReadKey();
#endif
            Environment.Exit(0);
        }

        static void Process(string path)
        {
            Calculator calculator = new Calculator();

            WaveStream readerStream = new WaveFileReader(path);

            //Console.WriteLine("{0}, {1}, {2}, {3}", readerStream.WaveFormat.Encoding,
            //    readerStream.WaveFormat.BitsPerSample, readerStream.WaveFormat.Channels,
            //    readerStream.WaveFormat.SampleRate);

            WaveChannel32 sourceStream = new WaveChannel32(readerStream);

            //Console.WriteLine("{0}, {1}, {2}, {3}", sourceStream.WaveFormat.Encoding,
            //    sourceStream.WaveFormat.BitsPerSample, sourceStream.WaveFormat.Channels,
            //    sourceStream.WaveFormat.SampleRate);

            if (sourceStream.WaveFormat.BitsPerSample != 32)
                throw new ArgumentException("Metering Stream expects 32 bit floating point audio", "sourceStream");

            calculator.Init(sourceStream.WaveFormat.SampleRate, sourceStream.WaveFormat.Channels);

            byte[] buffer = new byte[4096];
            int length = 0;

            while (sourceStream.Position < sourceStream.Length)
            {
                length = sourceStream.Read(buffer, 0, buffer.Length);
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
