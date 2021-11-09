using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace GenshinToolbox;

public unsafe sealed class FastBitmap : IDisposable
{
    private readonly Bitmap _bmp;
    private readonly BitmapData _data;
    private readonly byte* _bufferPtr;
    private readonly int RawWidth;
    private readonly int RawHeigth;
    private Rectangle window;
    public int Width => window.Width;
    public int Height => window.Height;

    public FastBitmap(Bitmap bmp)
    {
        if (bmp.PixelFormat != ImageExt.SharedPixelFormat)
        {
            throw new ArgumentException($"pixel format should be {ImageExt.SharedPixelFormat}!", nameof(bmp));
        }

        _bmp = bmp;
        RawWidth = _bmp.Width;
        RawHeigth = _bmp.Height;
        window = new Rectangle(0, 0, RawWidth, RawHeigth);
        _data = bmp.LockBits(window, ImageLockMode.ReadWrite, ImageExt.SharedPixelFormat);
        _bufferPtr = (byte*)_data.Scan0.ToPointer();
    }

    public Span<Bgrx32> GetRow(int y)
    {
        if (y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException(nameof(y));
        var row = _bufferPtr + ((y + window.Y) * _data.Stride);
        return new Span<Bgrx32>(row, RawWidth).Slice(window.X, Width);
    }

    public Bgrx32* this[int x, int y]
    {
        get
        {
            var pixel = _bufferPtr + ((y + window.Y) * _data.Stride) + ((x + window.X) * ImageExt.SharedBPP);
            return (Bgrx32*)pixel;
        }
    }

    public void Dispose()
    {
        _bmp.UnlockBits(_data);
    }

    public void DisposeWithBithmap()
    {
        Dispose();
        _bmp.Dispose();
    }

    public void SetWindow(Rectangle window)
    {
        if (window.Left < 0 || window.Right > Width
            || window.Top < 0 || window.Bottom > Height)
        {
            throw new ArgumentOutOfRangeException(nameof(window));
        }

        this.window = window;
    }

    public void ResetWindow()
    {
        this.window = new Rectangle(0, 0, _bmp.Width, _bmp.Height);
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
