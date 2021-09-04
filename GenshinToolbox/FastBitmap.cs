using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace GenshinToolbox
{
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

		// ********

		public void ApplyFilter(PxFunc transform, Rectangle? slice = null)
		{
			var box = slice ?? new(0, 0, Width, Height);
			var sY = box.Y;
			var sX = box.X;
			var eY = Math.Min(box.Bottom, Height);
			var eX = Math.Min(box.Right, Width);

			for (int y = sY; y < eY; y++)
			{
				var row = GetRow(y);
				if (eX > row.Length) throw new Exception();
				for (int x = sX; x < eX; x++)
				{
					transform(ref row[x]);
				}
			}
		}
	}
}
