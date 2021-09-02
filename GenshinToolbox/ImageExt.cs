using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection.Metadata.Ecma335;

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

		public static void IterCols(this Bitmap img, Action<Color, int, int> pixelFunc)
		{
			for (int x = 0; x < img.Width; x++)
			{
				for (int y = 0; y < img.Height; y++)
				{
					var p = img.GetPixel(x, y);
					pixelFunc(p, x, y);
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

		public static void ApplyFilter(this Bitmap img, Action<Bitmap, Graphics> effect)
		{
			using var g = Graphics.FromImage(img);
			effect.Invoke(img, g);
		}

		public static Bitmap ResizeTo(this Image image, int width, int height)
		{
			var destRect = new Rectangle(0, 0, width, height);
			var destImage = new Bitmap(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighSpeed;
				graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
				graphics.SmoothingMode = SmoothingMode.None;
				graphics.PixelOffsetMode = PixelOffsetMode.None;
				graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
			}

			return destImage;
		}

		public static bool MatchesAll(this Bitmap img, Predicate<Color> matches)
		{
			for (var x = 0; x < img.Width; x++)
				for (var y = 0; y < img.Height; y++)
				{
					var px = img.GetPixel(x, y);
					if (!matches(px))
						return false;
				}
			return true;
		}

		/// <summary>Filter almost White to Black; Rest White</summary>
		public static void WhiteFilter(Bitmap i, Graphics g) => i.ForAll(px => px.R + px.G + px.B > 180 * 3 ? Color.Black : Color.White);
		/// <summary>Filter lighter than Gray to Black; Rest White</summary>
		public static void GrayFilter(Bitmap i, Graphics g) => i.ForAll(px => px.R + px.G + px.B > 128 * 3 ? Color.Black : Color.White);
		/// <summary>Filter darker than Gray to Black; Rest White</summary>
		public static void BlackFilter(Bitmap i, Graphics g) => i.ForAll(px => px.R + px.G + px.B < 128 * 3 ? Color.Black : Color.White);
		/// <summary>Green-ish to Black; Rest White</summary>
		public static void GreenFilter(Bitmap i, Graphics g) => i.ForAll(px => px.G > 128 && px.R + px.B < 128 * 2 ? Color.Black : Color.White);


		public static bool RgbEq(this Color color, Color other) =>
			color.R == other.R && color.G == other.G && color.B == other.B;

		public static bool RgbIn(this Color color, Color min, Color max) =>
			color.R >= min.R && color.G >= min.G && color.B >= min.B &&
			color.R <= max.R && color.G <= max.G && color.B <= max.B;
	}
}
