using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GenshinToolbox
{
	public static class ImageExt
	{
		public const PixelFormat SharedPixelFormat = PixelFormat.Format32bppRgb;
		public const int SharedBPP = 4;

		public delegate void PxFunc(ref Bgrx32 px);

		public static unsafe void ForAll(this Bitmap img, PxFunc transform)
		{
			using var fastbmp = new FastBitmap(img);
			//var span = new Span<Rgbx32>(ptr, 1);
			//var rgb = MemoryMarshal.GetReference(span);

			for (int y = 0; y < img.Height; y++)
			{
				var row = fastbmp.GetRow(y);
				for (int x = 0; x < row.Length; x++)
				{
					transform(ref row[x]);
				}
			}
		}

		internal static unsafe void IterDemo(Bitmap bmp)
		{
			int w = bmp.Width;
			int h = bmp.Height;

			if (bmp.PixelFormat != SharedPixelFormat)
			{
				throw new ArgumentException($"pixel format should be {SharedPixelFormat}!", nameof(bmp));
			}

			BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bmp.PixelFormat);

			try
			{
				byte* sourcePtrBase = (byte*)data.Scan0;
				for (int y = 0; y < h; y++)
				{
					byte* sourcePtr = sourcePtrBase + (data.Stride * y);
					var rbgArr = new Span<byte>(sourcePtr, w * SharedBPP);

				}
			}
			finally
			{
				bmp.UnlockBits(data);
			}
		}

		public static Bitmap CropOut(this Bitmap img, Rectangle rect)
		{
			var crop = new Bitmap(rect.Width, rect.Height, SharedPixelFormat);
			using var g = Graphics.FromImage(crop);
			g.SmoothingMode = SmoothingMode.None;
			g.PixelOffsetMode = PixelOffsetMode.Default;
			g.CompositingQuality = CompositingQuality.Default;
			g.InterpolationMode = InterpolationMode.Default;
			g.CompositingMode = CompositingMode.SourceCopy;
			g.DrawImage(img, 0, 0, rect, GraphicsUnit.Pixel);
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
			var destImage = new Bitmap(width, height, SharedPixelFormat);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using var graphics = Graphics.FromImage(destImage);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighSpeed;
			graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			graphics.SmoothingMode = SmoothingMode.None;
			graphics.PixelOffsetMode = PixelOffsetMode.None;
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

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

		public static Bitmap ToSharedPixelFormat(this Bitmap img)
		{
			if (img.PixelFormat == SharedPixelFormat)
				return img;

			using var _img = img;
			var clone = new Bitmap(img.Width, img.Height, SharedPixelFormat);
			using var g = Graphics.FromImage(clone);
			g.DrawImage(img, new Rectangle(0, 0, clone.Width, clone.Height));
			return clone;
		}

		/// <summary>Filter almost White to Black; Rest White</summary>
		public static void WhiteFilter(Bitmap i, Graphics _) => i.ForAll((ref Bgrx32 px) => px = px.R + px.G + px.B > 180 * 3 ? Bgrx32.Black : Bgrx32.White);
		/// <summary>Filter lighter than Gray to Black; Rest White</summary>
		public static void GrayFilter(Bitmap i, Graphics _) => i.ForAll((ref Bgrx32 px) => px = px.R + px.G + px.B > 128 * 3 ? Bgrx32.Black : Bgrx32.White);
		/// <summary>Filter darker than Gray to Black; Rest White</summary>
		public static void BlackFilter(Bitmap i, Graphics _) => i.ForAll((ref Bgrx32 px) => px = px.R + px.G + px.B < 128 * 3 ? Bgrx32.Black : Bgrx32.White);
		/// <summary>Green-ish to Black; Rest White</summary>
		public static void GreenFilter(Bitmap i, Graphics _) => i.ForAll((ref Bgrx32 px) => px = px.G > 128 && px.R + px.B < 128 * 2 ? Bgrx32.Black : Bgrx32.White);

		public static bool RgbEq(this Color color, Color other) =>
			color.R == other.R && color.G == other.G && color.B == other.B;

		public static bool RgbIn(this Color color, Color min, Color max) =>
			color.R >= min.R && color.G >= min.G && color.B >= min.B &&
			color.R <= max.R && color.G <= max.G && color.B <= max.B;
	}

	public unsafe readonly ref struct FastBitmap
	{
		private readonly Bitmap _bmp;
		private readonly BitmapData _data;
		private readonly byte* _bufferPtr;

		public int Width => _bmp.Width;
		public int Height => _bmp.Height;

		public FastBitmap(Bitmap bmp)
		{
			if (bmp.PixelFormat != ImageExt.SharedPixelFormat)
			{
				throw new ArgumentException($"pixel format should be {ImageExt.SharedPixelFormat}!", nameof(bmp));
			}

			_bmp = bmp;
			_data = bmp.LockBits(new Rectangle(0, 0, _bmp.Width, _bmp.Height), ImageLockMode.ReadWrite, ImageExt.SharedPixelFormat);
			_bufferPtr = (byte*)_data.Scan0.ToPointer();
		}

		public Span<Bgrx32> GetRow(int y)
		{
			var row = _bufferPtr + (y * _data.Stride);
			return new Span<Bgrx32>(row, Width);
		}

		public Bgrx32* this[int x, int y]
		{
			get
			{
				var pixel = _bufferPtr + (y * _data.Stride) + (x * ImageExt.SharedBPP);
				return (Bgrx32*)pixel;
			}
		}

		public void Dispose()
		{
			_bmp.UnlockBits(_data);
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Bgrx32 : IEquatable<Bgrx32>
	{
		[FieldOffset(0)]
		public uint All;
		[FieldOffset(0)]
		public byte B;
		[FieldOffset(1)]
		public byte G;
		[FieldOffset(2)]
		public byte R;

		[FieldOffset(3)]
		private byte _X;

		public static readonly Bgrx32 White = FromColor(Color.White);
		public static readonly Bgrx32 Black = FromColor(Color.Black);

		public Bgrx32(byte r, byte g, byte b) : this()
		{
			R = r;
			G = g;
			B = b;
			_X = 0;
		}

		public override bool Equals(object? obj)
		{
			return obj is Bgrx32 rgbx && Equals(rgbx);
		}

		public bool Equals(Bgrx32 other) => All == other.All;
		public static bool operator ==(Bgrx32 left, Bgrx32 right) => (left.All & 0x00FFFFFF) == (right.All & 0x00FFFFFF);
		public static bool operator !=(Bgrx32 left, Bgrx32 right) => !(left == right);
		public override int GetHashCode() => unchecked((int)All);

		public static Bgrx32 FromColor(Color c) => new(c.R, c.G, c.B);

		public override string? ToString() => $"{R:X2}{G:X2}{B:X2}";
	}
}
