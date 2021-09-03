using System.Drawing;

namespace GenshinToolbox
{
	public static class Capture
	{
		public static Bitmap Screen(Rectangle rect) => Screen(rect.Location, rect.Size);
		public static Bitmap Screen(Point pos, Size size)
		{
			var result = new Bitmap(size.Width, size.Height, ImageExt.SharedPixelFormat);
			using var g = Graphics.FromImage(result);
			g.CopyFromScreen(pos, Point.Empty, size);
			return result;
		}

		public static Bitmap Game(Rectangle rect) => Game(rect.Location, rect.Size);
		public static Bitmap Game(Point pos, Size size) => Screen(Util.WindowOffset.Add(pos), size);
	}
}
