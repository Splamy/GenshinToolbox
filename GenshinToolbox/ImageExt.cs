using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace GenshinToolbox;

public delegate void PxFunc(ref Bgrx32 px);

public static class ImageExt
{
    public const PixelFormat OcrPixelFormat = PixelFormat.Format32bppArgb;
    public const PixelFormat SharedPixelFormat = PixelFormat.Format32bppRgb;
    public const int SharedBPP = 4;

    public static unsafe void ApplyFilter(this Bitmap img, PxFunc transform, Rectangle? slice = null)
    {
        using var fastbmp = new FastBitmap(img);
        fastbmp.ApplyFilter(transform, slice);
    }

    public static Bitmap CropOut(this Bitmap img, Rectangle rect)
    {
        var crop = new Bitmap(rect.Width, rect.Height, img.PixelFormat);
        using var g = Graphics.FromImage(crop);
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.Default;
        g.CompositingQuality = CompositingQuality.Default;
        g.InterpolationMode = InterpolationMode.Default;
        g.CompositingMode = CompositingMode.SourceCopy;
        g.DrawImage(img, 0, 0, rect, GraphicsUnit.Pixel);
        return crop;
    }

    public static Bitmap ResizeTo(this Image image, int width, int height, bool hq)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height, image.PixelFormat);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using var graphics = Graphics.FromImage(destImage);
        graphics.CompositingMode = CompositingMode.SourceCopy;
        if (hq)
        {
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.High;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }
        else
        {
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.SmoothingMode = SmoothingMode.None;
            graphics.PixelOffsetMode = PixelOffsetMode.None;
        }
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

    public static Bitmap ToOcrPixelFormat(this Bitmap img)
    {
        if (img.PixelFormat == OcrPixelFormat)
            return img;

        using var _img = img;
        var clone = new Bitmap(img.Width, img.Height, OcrPixelFormat);
        using var g = Graphics.FromImage(clone);
        g.DrawImage(img, new Rectangle(0, 0, clone.Width, clone.Height));
        return clone;
    }

    /// <summary>Filter almost White to Black; Rest White</summary>
    public static void WhiteFilter(ref Bgrx32 px) => px = px.R + px.G + px.B > 180 * 3 ? Bgrx32.Black : Bgrx32.White;
    /// <summary>Filter lighter than Gray to Black; Rest White</summary>
    public static void GrayFilter(ref Bgrx32 px) => px = px.R + px.G + px.B > 128 * 3 ? Bgrx32.Black : Bgrx32.White;
    /// <summary>Filter darker than Gray to Black; Rest White</summary>
    public static void BlackFilter(ref Bgrx32 px) => px = px.R + px.G + px.B < 128 * 3 ? Bgrx32.Black : Bgrx32.White;
    /// <summary>Green-ish to Black; Rest White</summary>
    public static void GreenFilter(ref Bgrx32 px) => px = px.G > 128 && px.R + px.B < 128 * 2 ? Bgrx32.Black : Bgrx32.White;

    public static void BlackScaleFilter(ref Bgrx32 px)
    {
        var black = px.Lightness;
        if (black < 128)
        {
            px = new(black, black, black);
        }
        else
        {
            px = Bgrx32.White;
        }
    }

    public static void GrayScaleFilter(ref Bgrx32 px)
    {
        var black = px.Lightness;
        px = new(black, black, black);
    }

    public static void WhiteScaleFilter(ref Bgrx32 px)
    {
        var white = px.Lightness;
        if (white > 128)
        {
            // About [229-255]
            // About [0-26]
            var black = 255 - white;
            //var scaled = (byte)(black * 9);
            var scaled = (byte)black;
            px = new(scaled, scaled, scaled);
        }
        else
        {
            px = Bgrx32.White;
        }
    }

    public static void ExactWhiteFilter(ref Bgrx32 px) => px = px == Bgrx32.White ? Bgrx32.White : Bgrx32.Black;

    public static bool RgbIn(this Color color, Color min, Color max) =>
        color.R >= min.R && color.G >= min.G && color.B >= min.B &&
        color.R <= max.R && color.G <= max.G && color.B <= max.B;
}
