using System;
using System.Text.RegularExpressions;
using System.Drawing;

namespace FFMpegNet
{
    public class InfoProcessor
    {
        public static TimeSpan GetDuration(string outputCapture)
        {
            Match m = Regex.Match(outputCapture, @"[D|d]uration:.((\d|:|\.)*)");
            if (m.Success == false)
            {
                return TimeSpan.Zero;
            }

            string duration = m.Groups[1].Value;
            string[] timepieces = duration.Split(new char[] { ':', '.' });
            if (timepieces.Length == 4)
            {
                return new TimeSpan(0, Convert.ToInt16(timepieces[0]), Convert.ToInt16(timepieces[1]), Convert.ToInt16(timepieces[2]), Convert.ToInt16(timepieces[3]));
            }

            return TimeSpan.Zero;
        }

        public static double GetAudioBitRate(string outputCapture)
        {
            Match m = Regex.Match(outputCapture, @"[B|b]itrate:.((\d|:)*)");
            if (m.Success == false)
            {
                return 0.0;
            }

            double kb = 0.0;
            Double.TryParse(m.Groups[1].Value, out kb);

            return kb;
        }

        public static string GetAudioFormat(string outputCapture)
        {
            Match m = Regex.Match(outputCapture, @"[A|a]udio:(.*)");
            if (m.Success == false)
            {
                return String.Empty;
            }

            return m.Captures[0].Value;
        }

        public static string GetVideoFormat(string outputCapture)
        {
            Match m = Regex.Match(outputCapture, @"[V|v]ideo:(.*)");
            if (m.Success == false)
            {
                return string.Empty;
            }

            return m.Captures[0].Value;
        }

        public static Size GetVideoDimensions(string outputCapture)
        {
            Match m = Regex.Match(outputCapture, @"(\d{2,4})x(\d{2,4})");
            if (m.Success == false)
            {
                return Size.Empty;
            }

            int w;
            int h;

            int.TryParse(m.Groups[1].Value, out w);
            int.TryParse(m.Groups[2].Value, out h);

            return new Size(w, h);
        }

    }

}
