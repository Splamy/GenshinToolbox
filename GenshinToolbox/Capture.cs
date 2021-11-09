using System.Drawing;

namespace GenshinToolbox;

public static class Capture
{
    public static readonly Size R1080p = new(1920, 1080);

    public static Bitmap Screen(Rectangle rect) => Screen(rect.Location, rect.Size);
    public static Bitmap Screen(Point pos, Size size)
    {
        var result = new Bitmap(size.Width, size.Height, ImageExt.SharedPixelFormat);
        using var g = Graphics.FromImage(result);
        g.CopyFromScreen(pos, Point.Empty, size);
        return result;
    }

    public static Bitmap Game(Rectangle rect, Size? rescaleTo = null) => Game(rect.Location, rect.Size, rescaleTo);
    public static Bitmap Game(Point pos, Size size, Size? rescaleTo = null)
    {
        if (rescaleTo is { } rescale && Util.WindowSize != rescale)
        {
            var window = Util.WindowSize;
            var rpos = new Point(pos.X * window.Width / rescale.Width, pos.Y * window.Height / rescale.Height);
            var rsize = new Size(size.Width * window.Width / rescale.Width, size.Height * window.Height / rescale.Height);
            var offed = Util.WindowOffset.Add(rpos);

            var snap = Screen(offed, rsize);
            return snap.ResizeTo(size.Width, size.Height, true);
        }
        else
        {
            return Screen(Util.WindowOffset.Add(pos), size);
        }
    }
}
