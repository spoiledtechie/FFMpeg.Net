using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;

namespace FFMpegNet
{
    public static class FFMpegService
    {
        public static string FFMPEGExecutableFilePath;

        private const int MaximumBuffers = 25;

        public static Queue<string> PreviousBuffers = new Queue<string>();

        static FFMpegService()
        {
            FFMPEGExecutableFilePath = Environment.CurrentDirectory + ConfigurationManager.AppSettings["FFMPEGExecutableFilePath"];
        }


        public static string Execute(string inputFilePath)
        {
            if (String.IsNullOrWhiteSpace(inputFilePath))
            {
                throw new ArgumentNullException("Input file path cannot be null");
            }

            FFMPEGParameters parameters = new FFMPEGParameters()
            {
                InputFilePath = inputFilePath
            };

            return Execute(parameters);
        }

        public static string Execute(string inputFilePath, string outputOptions, string outputFilePath)
        {
            if (String.IsNullOrWhiteSpace(inputFilePath))
            {
                throw new ArgumentNullException("Input file path cannot be null");
            }

            if (String.IsNullOrWhiteSpace(inputFilePath))
            {
                throw new ArgumentNullException("Output file path cannot be null");
            }

            FFMPEGParameters parameters = new FFMPEGParameters()
            {
                InputFilePath = inputFilePath,
                OutputOptions = outputOptions,
                OutputFilePath = outputFilePath,
            };

            return Execute(parameters);

        }

        public static string Execute(string inputFilePath, string outputOptions)
        {
            if (String.IsNullOrWhiteSpace(inputFilePath))
            {
                throw new ArgumentNullException("Input file path cannot be null");
            }

            FFMPEGParameters parameters = new FFMPEGParameters()
            {
                InputFilePath = inputFilePath,
                OutputOptions = outputOptions
            };

            return Execute(parameters);
        }

        public static string Execute(FFMPEGParameters parameters)
        {
            if (String.IsNullOrWhiteSpace(FFMPEGExecutableFilePath))
            {
                throw new ArgumentNullException("Path to FFMPEG executable cannot be null");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("FFMPEG parameters cannot be completely null");
            }

            using (Process ffmpegProcess = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo(FFMPEGExecutableFilePath)
                {
                    Arguments = parameters.ToString(),
                    WorkingDirectory = Path.GetDirectoryName(FFMPEGExecutableFilePath),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                ffmpegProcess.StartInfo = info;
                ffmpegProcess.Start();
                string processOutput = ffmpegProcess.StandardError.ReadToEnd();
                ffmpegProcess.WaitForExit();
                PreviousBuffers.Enqueue(processOutput);
                lock (PreviousBuffers)
                {
                    while (PreviousBuffers.Count > MaximumBuffers)
                    {
                        PreviousBuffers.Dequeue();
                    }

                }

                return processOutput;
            }

        }

    }

}
