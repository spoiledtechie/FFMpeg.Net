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

namespace FFMpegNet
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

    public class VideoFile
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

        public VideoFile(string filePath)
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
            GetVideoInfo();
        }

        protected static Image LoadImageFromFile(string filePath)
        {

            Image loadedImage = null;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] img;
                img = new byte[fileStream.Length];
                fileStream.Read(img, 0, img.Length);
                fileStream.Close();
                loadedImage = Image.FromStream(new MemoryStream(img));
                img = null;
            }

            GC.Collect();

            return loadedImage;
        }

        // I would have made "dimensions" be an optional parameter, but
        // unfortunately C# requires that optional parameters be a "compile
        // time constant" so I cannot use either "Size.Empty" or "new Size()"
        // as the default parameter value. Hence, we have to fall back to a
        // method overload.
        public Image ExtractSingleFrame(long ticksToExtract, ImageFormat type)
        {
            return ExtractSingleFrame(ticksToExtract, type, Size.Empty);
        }

        public Image ExtractSingleFrame(long ticksToExtract, ImageFormat type, Size dimensions)
        {
            string tempFile = Path.ChangeExtension(Path.GetTempFileName(), type.ToString());
            var span = TimeSpan.FromTicks(ticksToExtract);

            if (span > Duration)
                throw new Exception("Time is larger than actual video");

            FFMPEGParameters parameters = new FFMPEGParameters()
            {
                InputFilePath = FilePath,
                DisableAudio = true,
                OutputOptions = String.Format("-f image2 -ss {0} -vframes 1", span.Hours.ToString("D2") + ":" + span.Minutes.ToString("D2") + ":" + span.Seconds.ToString("D2") + "." + span.Milliseconds.ToString("D3")),
                Size = dimensions,
                OutputFilePath = tempFile,
            };

            string output = FFMpegService.Execute(parameters);

            if (!File.Exists(tempFile))
            {
                throw new Exception("Could not create single frame image from video clip");
            }

            Image previewImage = LoadImageFromFile(tempFile);

            try
            {
                File.Delete(tempFile);
            }

            catch (Exception ex)
            {
                throw new Exception("Failed to delete temporary file used for thumbnail " + ex.Message);
            }

            return previewImage;
        }

        public string ExtractVideoSegment(long ticksToExtract, long ticksTimeLapse, VideoFormat type)
        {
            return ExtractVideoSegment(ticksToExtract, ticksTimeLapse, type, Size.Empty);
        }

        public string ExtractVideoSegment(long ticksToExtract, long ticksTimeLapse, VideoFormat type, Size dimensions)
        {
            string tempFile = Path.ChangeExtension(Path.GetTempFileName(), type.ToString());
            var span = TimeSpan.FromTicks(ticksToExtract);
            var spanTo = TimeSpan.FromTicks(ticksTimeLapse - ticksToExtract);

            if (span > Duration)
                throw new Exception("Time is larger than actual video");

            FFMPEGParameters parameters = new FFMPEGParameters()
            {
                InputFilePath = FilePath,
                DisableAudio = false,
                OutputOptions = String.Format("-ss {0} -t {1}", span.Hours.ToString("D2") + ":" + span.Minutes.ToString("D2") + ":" + span.Seconds.ToString("D2") + "." + span.Milliseconds.ToString("D3"), spanTo.Hours.ToString("D2") + ":" + spanTo.Minutes.ToString("D2") + ":" + spanTo.Seconds.ToString("D2") + "." + spanTo.Milliseconds.ToString("D3")),
                Size = dimensions,
                OutputFilePath = tempFile,
            };

            string output = FFMpegService.Execute(parameters);

            if (!File.Exists(tempFile))
            {
                throw new Exception("Could not create single frame image from video clip");
            }

            return tempFile;
        }

        public string MergeAudioSegment(string audioFile, VideoFormat type)
        {
            //ffmpeg -i tmpVideo.mpg -i tmpAudioRB.wav -vcodec copy finalVideow_6.mpg

            string tempFile = Path.ChangeExtension(Path.GetTempFileName(), type.ToString());

            List<string> files = new List<string>();
            files.Add(audioFile);

            FFMPEGParameters parameters = new FFMPEGParameters()
            {
                InputFilePath = FilePath,
                DisableAudio = false,
                AdditionalFileInputs = files,
                OutputOptions = String.Format("-map 0:0 -map 1:0 -vcodec copy -acodec copy"),
                OutputFilePath = tempFile,
            };

            string output = FFMpegService.Execute(parameters);

            if (!File.Exists(tempFile))
            {
                throw new Exception("Could not create single frame image from video clip");
            }

            return tempFile;
        }

        public string MergeAudioSegments(List<string> audioFiles, VideoFormat type)
        {

            string tempFile = Path.ChangeExtension(Path.GetTempFileName(), type.ToString());

            List<string> files = audioFiles;

            string outputOptions = string.Empty;

            string copyAll = " -c:v copy -c:a copy";

            for (int i = 0; i < files.Count; i++)
            {
                outputOptions += "-map " + (i + 1) + ":0";
                if (i + files.Count < files.Count)
                    outputOptions += " ";
            }

            FFMPEGParameters parameters = new FFMPEGParameters()
            {
                InputFilePath = FilePath,
                DisableAudio = false,
                AdditionalFileInputs = files,
                OutputOptions = outputOptions + copyAll,
                OutputFilePath = tempFile,
            };

            string output = FFMpegService.Execute(parameters);

            if (!File.Exists(tempFile))
            {
                throw new Exception("Could not create single frame image from video clip");
            }

            return tempFile;
        }

        public string ExtractAudioSegment(AudioFormat type)
        {
            string tempFile = Path.ChangeExtension(Path.GetTempFileName(), type.ToString());

            FFMPEGParameters parameters = new FFMPEGParameters()
            {
                InputFilePath = FilePath,
                DisableAudio = false,
                OutputOptions = String.Format("-vn"),
                OutputFilePath = tempFile,

            };

            string output = FFMpegService.Execute(parameters);

            if (!File.Exists(tempFile))
            {
                throw new Exception("Could not extract Audio From Video Clip");
            }

            return tempFile;
        }


        public string ExtractAudioSegment(long ticksToExtract, long ticksTimeLapse, AudioFormat type)
        {
            string tempFile = Path.ChangeExtension(Path.GetTempFileName(), type.ToString());
            var span = TimeSpan.FromTicks(ticksToExtract);
            var spanTo = TimeSpan.FromTicks(ticksTimeLapse - ticksToExtract);

            if (span > Duration)
                throw new Exception("Time is larger than actual video");

            FFMPEGParameters parameters = new FFMPEGParameters()
            {
                InputFilePath = FilePath,
                DisableAudio = false,
                OutputOptions = String.Format("-vn -ss {0} -t {1}", span.Hours.ToString("D2") + ":" + span.Minutes.ToString("D2") + ":" + span.Seconds.ToString("D2") + "." + span.Milliseconds.ToString("D3"), spanTo.Hours.ToString("D2") + ":" + spanTo.Minutes.ToString("D2") + ":" + spanTo.Seconds.ToString("D2") + "." + spanTo.Milliseconds.ToString("D3")),
                OutputFilePath = tempFile,

            };

            string output = FFMpegService.Execute(parameters);

            if (!File.Exists(tempFile))
            {
                throw new Exception("Could not extract Audio From Video Clip");
            }

            return tempFile;
        }



        protected void GetVideoInfo()
        {
            string output = FFMpegService.Execute(FilePath);

            Duration = InfoProcessor.GetDuration(output);
            AudioBitRate = InfoProcessor.GetAudioBitRate(output);
            AudioFormat = InfoProcessor.GetAudioFormat(output);
            VideoFormat = InfoProcessor.GetVideoFormat(output);
            Fps = InfoProcessor.GetVideoFps(VideoFormat);
            Dimensions = InfoProcessor.GetVideoDimensions(output);
            Created = InfoProcessor.GetCreationTime(output);
        }

        public string WatermarkVideo(string watermarkImageFilePath, bool overwrite, WatermarkPosition position, Point offset)
        {
            string extension = Path.GetExtension(FilePath);
            string tempOutputFile = Path.ChangeExtension(Path.GetTempFileName(), extension);

            string overlayFormat;
            switch (position)
            {
                case WatermarkPosition.TopLeft:
                    overlayFormat = "{0}:{1}";
                    break;
                case WatermarkPosition.TopRight:
                    overlayFormat = "main_w-overlay_w-{0}:{1}";
                    break;
                case WatermarkPosition.BottomLeft:
                    overlayFormat = "{0}:main_h-overlay_h-{1}";
                    break;
                case WatermarkPosition.BottomRight:
                    overlayFormat = "main_w-overlay_w-{0}:main_h-overlay_h-{1}";
                    break;
                case WatermarkPosition.Center:
                    overlayFormat = "(main_w-overlay_w)/2-{0}:(main_h-overlay_h)/2-{1}";
                    break;
                case WatermarkPosition.MiddleLeft:
                    overlayFormat = "{0}:(main_h-overlay_h)/2-{1}";
                    break;
                case WatermarkPosition.MiddleRight:
                    overlayFormat = "main_w-overlay_w-{0}:(main_h-overlay_h)/2-{1}";
                    break;
                case WatermarkPosition.CenterTop:
                    overlayFormat = "(main_w-overlay_w)/2-{0}:{1}";
                    break;
                case WatermarkPosition.CenterBottom:
                    overlayFormat = "(main_w-overlay_w)/2-{0}:main_h-overlay_h-{1}";
                    break;

                default:
                    throw new ArgumentException("Invalid position specified");

            }

            string overlayPostion = String.Format(overlayFormat, offset.X, offset.Y);

            FFMPEGParameters parameters = new FFMPEGParameters
            {
                InputFilePath = FilePath,
                OutputFilePath = tempOutputFile,
                QScale = false,
                Overwrite = true,
                VideoFilter = String.Format("\"movie=\\'{0}\\' [logo]; [in][logo] overlay={1} [out]\"", watermarkImageFilePath.Replace("\\", "\\\\"), overlayPostion)
            };


            string output = FFMpegService.Execute(parameters);
            if (File.Exists(tempOutputFile) == false)
            {
                throw new ApplicationException(String.Format("Failed to watermark video {0}{1}{2}", FilePath, Environment.NewLine, output));
            }

            FileInfo watermarkedVideoFileInfo = new FileInfo(tempOutputFile);
            if (watermarkedVideoFileInfo.Length == 0)
            {
                throw new ApplicationException(String.Format("Failed to watermark video {0}{1}{2}", FilePath, Environment.NewLine, output));
            }

            if (overwrite)
            {
                File.Delete(FilePath);
                File.Move(tempOutputFile, FilePath);

                return FilePath;
            }

            return tempOutputFile;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="overwrite"></param>
        /// <param name="overlays"></param>
        /// <returns>the file name the video was saved to.</returns>
        public string OverlayVideo(bool overwrite, List<Overlay> overlays)
        {
            string overLays = string.Empty;
            string tempOutputFile = string.Empty;
            string testParamsLength = string.Empty;
            long lastTimeImageExtracted = -10000000;
            Image imageFromFrame = null;
            Bitmap bitmapFromImage = null;
            int MillisecondsToCutNewFrameWithBlur = Convert.ToInt32(ConfigurationManager.AppSettings["MillisecondsToCutNewFrameWithBlur"]);
            int PixelateSize = Convert.ToInt32(ConfigurationManager.AppSettings["PixelateSize"]);

            for (int i = 0; i < overlays.Count; i++)
            {
                if (overlays[i].Size.Width > 0 && overlays[i].Size.Height > 0)
                {
                    overlays[i].Path = Path.ChangeExtension(Path.GetTempFileName(), "png");

                    if (overlays[i].OverlayType == OverlayType.Ellipse || overlays[i].OverlayType == OverlayType.Rectangle)
                    {
                        Bitmap bmp = new Bitmap(overlays[i].Size.Width, overlays[i].Size.Height);
                        Graphics g = Graphics.FromImage(bmp);

                        Brush brush = new SolidBrush(Color.Black);
                        if (overlays[i].OverlayType == OverlayType.Rectangle)
                            g.FillRectangle(brush, 0, 0, overlays[i].Size.Width, overlays[i].Size.Height);
                        else if (overlays[i].OverlayType == OverlayType.Ellipse)
                            g.FillEllipse(brush, 0, 0, overlays[i].Size.Width, overlays[i].Size.Height);

                        g.Flush();
                        bmp.Save(overlays[i].Path, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else if (overlays[i].OverlayType == OverlayType.Rectangle_Blur)
                    {
                        //if the frame was created within the last second, then don't extract the same second of video since we are blurring it hard core.
                        //no one will notice.
                        if (TimeSpan.FromTicks(lastTimeImageExtracted).Add(TimeSpan.FromMilliseconds(MillisecondsToCutNewFrameWithBlur)) < TimeSpan.FromTicks(overlays[i].StartTicks))
                        {
                            imageFromFrame = ExtractSingleFrame(overlays[i].StartTicks, ImageFormat.Png);
                            bitmapFromImage = new Bitmap(imageFromFrame);
                            lastTimeImageExtracted = overlays[i].StartTicks;
                        }

                        Rectangle srcRect = new Rectangle(overlays[i].Offset.X, overlays[i].Offset.Y, overlays[i].Size.Width, overlays[i].Size.Height);
                        Bitmap card = (Bitmap)bitmapFromImage.Clone(srcRect, bitmapFromImage.PixelFormat);
                        //cutting rect from image and getting ready to save it as a ellipse or rect.
                        var pixellated = ImageHelper.Pixelate(card, PixelateSize);
                        pixellated.Save(overlays[i].Path, System.Drawing.Imaging.ImageFormat.Png);

                    }
                    testParamsLength += overlays[i].InitializeOutputString;
                }

            }

            for (int i = 0; i < overlays.Count; i++)
            {
                if (overlays[i].Size.Width > 0 && overlays[i].Size.Height > 0)
                {
                    testParamsLength += overlays[i].OutputProcessString;

                    if (i != (overlays.Count - 1))
                        testParamsLength += ",";
                }
            }

            int maxCharCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxCharacterCountForCommandParams"]);
            //commandprompt only allows 32,000 character params.  
            //if the test params are over that, we need to split up the overlay params, so we will make multiple passes on the file 
            //until we satisfy all the overlays.
            if (testParamsLength.Length > maxCharCount)
            {
                //split up command and output.
                int passesToMake = (int)Math.Ceiling(testParamsLength.Length / (double)maxCharCount);
                string tempFilePath = FilePath;
                int totalOverlays = overlays.Count;

                int overlayCountForBatches = totalOverlays / (int)passesToMake;

                List<List<Overlay>> overlayBatches = new List<List<Overlay>>();
                for (int i = 1; i <= passesToMake; i++)
                {
                    Debug.WriteLine(i);
                    List<Overlay> batch = new List<Overlay>();
                    for (int j = 0; j <= overlayCountForBatches; j++)
                    {
                        Debug.WriteLine(j);
                        if (i * j < overlays.Count)
                        {
                            batch.Add(overlays[i * j]);
                        }
                    }
                    overlayBatches.Add(batch);
                }

                foreach (var batch in overlayBatches)
                {
                    tempFilePath = SingleOverlayPass(batch, tempFilePath);
                }
                tempOutputFile = tempFilePath;
            }
            else
            {
                //single output needed
                tempOutputFile = SingleOverlayPass(overlays, FilePath);
            }

            if (overwrite)
            {
                File.Delete(FilePath);
                File.Move(tempOutputFile, FilePath);

                return FilePath;
            }
            for (int i = 0; i < overlays.Count; i++)
            {
                if (!string.IsNullOrEmpty(overlays[i].Path) && File.Exists(overlays[i].Path))
                    File.Delete(overlays[i].Path);
            }

            return tempOutputFile;
        }



        private string SingleOverlayPass(List<Overlay> overlays, string inputFile)
        {
            string VideoFilterInputs = string.Empty;
            string VideoFilterCommands = string.Empty;

            for (int i = 0; i < overlays.Count; i++)
            {
                if (overlays[i].Size.Width > 0 && overlays[i].Size.Height > 0)
                {
                    VideoFilterInputs += overlays[i].InitializeOutputString;
                }
            }

            for (int i = 0; i < overlays.Count; i++)
            {
                if (overlays[i].Size.Width > 0 && overlays[i].Size.Height > 0)
                {
                    VideoFilterCommands += overlays[i].OutputProcessString;

                    if (i != (overlays.Count - 1))
                        VideoFilterCommands += ",";
                }
            }

            string extension = Path.GetExtension(inputFile);
            string tempOutputFile = Path.ChangeExtension(Path.GetTempFileName(), extension);

            FFMPEGParameters parameters = new FFMPEGParameters
            {
                InputFilePath = inputFile,
                OutputFilePath = tempOutputFile,
                ComplexVideoFilterInputs = VideoFilterInputs,
                ComplexVideoFilterCommands = VideoFilterCommands
            };

            string output = FFMpegService.Execute(parameters);
            if (File.Exists(tempOutputFile) == false)
            {
                throw new ApplicationException(String.Format("Failed to overlay video {0}{1}{2}", inputFile, Environment.NewLine, output));
            }

            FileInfo watermarkedVideoFileInfo = new FileInfo(tempOutputFile);
            if (watermarkedVideoFileInfo.Length == 0)
            {
                throw new ApplicationException(String.Format("Failed to overlay video {0}{1}{2}", inputFile, Environment.NewLine, output));
            }
            return tempOutputFile;
        }

        private static string BuildOverlayPosition(WatermarkPosition position, Point offset)
        {
            string overlayFormat;
            switch (position)
            {
                case WatermarkPosition.TopLeft:
                    overlayFormat = "{0}:{1}";
                    break;
                case WatermarkPosition.TopRight:
                    overlayFormat = "main_w-overlay_w-{0}:{1}";
                    break;
                case WatermarkPosition.BottomLeft:
                    overlayFormat = "{0}:main_h-overlay_h-{1}";
                    break;
                case WatermarkPosition.BottomRight:
                    overlayFormat = "main_w-overlay_w-{0}:main_h-overlay_h-{1}";
                    break;
                case WatermarkPosition.Center:
                    overlayFormat = "(main_w-overlay_w)/2-{0}:(main_h-overlay_h)/2-{1}";
                    break;
                case WatermarkPosition.MiddleLeft:
                    overlayFormat = "{0}:(main_h-overlay_h)/2-{1}";
                    break;
                case WatermarkPosition.MiddleRight:
                    overlayFormat = "main_w-overlay_w-{0}:(main_h-overlay_h)/2-{1}";
                    break;
                case WatermarkPosition.CenterTop:
                    overlayFormat = "(main_w-overlay_w)/2-{0}:{1}";
                    break;
                case WatermarkPosition.CenterBottom:
                    overlayFormat = "(main_w-overlay_w)/2-{0}:main_h-overlay_h-{1}";
                    break;

                default:
                    throw new ArgumentException("Invalid position specified");

            }

            return String.Format(overlayFormat, offset.X, offset.Y);
        }


    }
}
