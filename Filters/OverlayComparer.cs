using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegNet.Filters
{
  public  class OverlayComparer : IEqualityComparer<Overlay>
    {
        public bool Equals(Overlay left, Overlay right)
        {
            if ((object)left == null && (object)right == null)
            {
                return true;
            }
            if ((object)left == null || (object)right == null)
            {
                return false;
            }
            return left.EndTicks == right.EndTicks && left.Offset.Y == right.Offset.Y && left.Offset.X == right.Offset.X && left.Size.Height == right.Size.Height && left.Size.Width == right.Size.Width && left.StartTicks == right.StartTicks;
        }

        public int GetHashCode(Overlay author)
        {
            return (author.EndTicks + author.Offset.X + author.Offset.Y + author.Size.Height + author.Size.Width + author.StartTicks).GetHashCode();
        }
    }


}
