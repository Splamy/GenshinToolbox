using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GenshinToolbox
{
	public static class ImageExt
	{
		public static void ForAll(this Bitmap img, Func<Color, Color> transform)
		{
			for (int x = 0; x < img.Width; x++)
			{
				for (int y = 0; y < img.Height; y++)
				{
					var p = img.GetPixel(x, y);
					img.SetPixel(x, y, transform(p));
				}
			}
		}

		public static Bitmap CropOut(this Bitmap img, Rectangle rect, Action<Bitmap, Graphics>? postprocess = null)
		{
			var crop = new Bitmap(rect.Width, rect.Height);
			using var g = Graphics.FromImage(crop);
			g.SmoothingMode = SmoothingMode.None;
			g.PixelOffsetMode = PixelOffsetMode.Default;
			g.CompositingQuality = CompositingQuality.Default;
			g.InterpolationMode = InterpolationMode.Default;
			g.CompositingMode = CompositingMode.SourceCopy;
			g.DrawImage(img, 0, 0, rect, GraphicsUnit.Pixel);
			postprocess?.Invoke(crop, g);
			return crop;
		}

		/// <summary>Filter almost White to Black; Rest White</summary>
		public static void WhiteFilter(Bitmap i, Graphics g) => i.ForAll(px => px.R + px.G + px.B > 180 * 3 ? Color.Black : Color.White);
		/// <summary>Filter lighter than Gray to Black; Rest White</summary>
		public static void GrayFilter(Bitmap i, Graphics g) => i.ForAll(px => px.R + px.G + px.B > 128 * 3 ? Color.Black : Color.White);
		/// <summary>Filter darker than Gray to Black; Rest White</summary>
		public static void BlackFilter(Bitmap i, Graphics g) => i.ForAll(px => px.R + px.G + px.B < 128 * 3 ? Color.Black : Color.White);
		/// <summary>Green-ish to Black; Rest White</summary>
		public static void GreenFilter(Bitmap i, Graphics g) => i.ForAll(px => px.G > 128 && px.R + px.B < 128 * 2 ? Color.Black : Color.White);
	}
}
