using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using FFMpegNet.Images;
using System.Drawing.Imaging;
using FFMpegNet.Videos;
using FFMpegNet.Filters;
using FFMpegNet.Audio;

namespace FFMpegNet.Audio
{
    public enum WatermarkPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center,
        MiddleLeft,
        MiddleRight,
        CenterTop,
        CenterBottom,
    }

    public class AudioFile
    {

        public TimeSpan Duration
        {
            get;
            private set;
        }

        public double AudioBitRate
        {
            get;
            private set;
        }

        public string AudioFormat
        {
            get;
            private set;
        }

        public string VideoFormat
        {
            get;
            private set;
        }

        public double Fps
        {
            get;
            private set;
        }

        public Size Dimensions
        {
            get;
            private set;
        }

        public DateTime Created
        {
            get;
            private set;
        }

        public string FilePath
        {
            get;
            private set;
        }

        public AudioFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("Could not find the location of the video file");
            }

            if (!File.Exists(filePath))
            {
                throw new Exception(String.Format("The video file {0} does not exist.", FilePath));
            }



            FilePath = filePath;
            //GetVideoInfo();
        }

        public string ConvertAudioFormat(AudioFormat type)
        {
            //ffmpeg -i tmpVideo.mpg -i tmpAudioRB.wav -vcodec copy finalVideow_6.mpg

            string tempFile = Path.ChangeExtension(Path.GetTempFileName(), type.ToString());


            FFMPEGParameters parameters = new FFMPEGParameters()
            {
                InputFilePath = FilePath,
                DisableAudio = false,
                //OutputOptions = String.Format("-map 0:0 -map 1:0 -vcodec copy -acodec copy"),
                OutputFilePath = tempFile,
            };

            string output = FFMpegService.Execute(parameters);

            if (!File.Exists(tempFile))
            {
                throw new Exception("Could not convert audio file");
            }

            return tempFile;
        }


        //protected void GetVideoInfo()
        //{
        //    string output = FFMpegService.Execute(FilePath);

        //    Duration = InfoProcessor.GetDuration(output);
        //    AudioBitRate = InfoProcessor.GetAudioBitRate(output);
        //    AudioFormat = InfoProcessor.GetAudioFormat(output);
        //    VideoFormat = InfoProcessor.GetVideoFormat(output);
        //    Fps = InfoProcessor.GetVideoFps(VideoFormat);
        //    Dimensions = InfoProcessor.GetVideoDimensions(output);
        //    Created = InfoProcessor.GetCreationTime(output);
        //}




    }
}
