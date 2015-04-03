using FFMpegNet.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegNet.Filters
{
    [DebuggerDisplay("Start = {StartTicks}, End= {EndTicks}, output= {OutputProcessString}")]
    public class Overlay
    {
        public Guid Id { get; set; }
        public long StartTicks { get; set; }
        public long EndTicks { get; set; }
        public Size Size { get; set; }

        public Point Offset { get; set; }

        public WatermarkPosition Position { get; set; }

        public string Path { get; set; }

        public string Name
        {
            get
            {
                return String.Format("[{0}]", Id);
            }
        }
        public string VideoOutParameter
        {
            get
            {
                return String.Format("[{0}]", Id + "-out");
            }
        }

        public string InitializeOutputString
        {
            get
            {
                return String.Format("-i {0} ", Path.Replace("\\", "\\\\"), Id);
            }
        }

        public string OutputProcessString
        {
            get
            {
                return String.Format("overlay={0}:{1}:enable='between(t,{2},{3})'", new object[] { Offset.X, Offset.Y, TimeSpan.FromTicks(StartTicks).TotalSeconds, TimeSpan.FromTicks(EndTicks).TotalSeconds });
            }
        }



        public Overlay(long start, long end, Size size, WatermarkPosition position, Point offset)
        {
            Id = Guid.NewGuid();
            StartTicks = start;
            EndTicks = end;
            Size = size;
            Position = position;
            Offset = offset;
        }

    }
}
