using System;
using System.Drawing;
using System.Reflection;

namespace GenshinToolbox
{
	class ImgMatch
	{
		public readonly FastBitmap fastImg;
		public readonly Rectangle matchRect;
		public readonly Bgrx32 chromaKey;

		public static ImgMatch YouGotABite = new("YouGotABite.png", new Rectangle(851, 216 - 11, 216, 23), Bgrx32.Black);

		public ImgMatch(string file, Rectangle matchRect, Bgrx32 chromaKey)
		{
			var assembly = Assembly.GetExecutingAssembly();
			using var stream = assembly.GetManifestResourceStream("GenshinToolbox.Matcher." + file);
			fastImg = new FastBitmap(new Bitmap(stream!).ToSharedPixelFormat());
			this.matchRect = matchRect;
			this.chromaKey = chromaKey;
		}

		public float Match(FastBitmap img, Point slice)
		{
			int sum = 0;
			int ok = 0;
			for (int y = 0; y < fastImg.Height; y++)
			{
				var row = fastImg.GetRow(y);
				var rowImg = img.GetRow(y + slice.Y);
				for (int x = 0; x < row.Length; x++)
				{
					if (row[x] == chromaKey)
						continue;
					sum++;
					if (rowImg[x + slice.X] == chromaKey) ok++;
				}
			}
			return (float)ok / sum;
		}

		// Rectangle: Source Image
		// Rectangle: Scan Rect

		public static Rectangle Analyze(FastBitmap img, Rectangle? slice, Predicate<Bgrx32> matches)
		{
			var box = slice ?? new(0, 0, img.Width, img.Height);
			var sY = box.Y;
			var sX = box.X;
			var eY = Math.Min(box.Bottom, img.Height);
			var eX = Math.Min(box.Right, img.Width);

			var foundMinX = eX;
			var foundMaxX = sX;
			var foundMinY = eY;
			var foundMaxY = sY;

			for (int y = sY; y < eY; y++)
			{
				var row = img.GetRow(y);
				if (eX > row.Length) throw new Exception();
				for (int x = sX; x < eX; x++)
				{
					if (matches(row[x]))
					{
						foundMinX = Math.Min(foundMinX, x);
						foundMaxX = Math.Max(foundMaxX, x);
						foundMinY = Math.Min(foundMinY, y);
						foundMaxY = Math.Max(foundMaxY, y);
					}
				}
			}

			return Rectangle.FromLTRB(foundMinX, foundMinY, foundMaxX, foundMaxY);
		}

		public static void DoStuff()
		{
			using var png = new Bitmap(Image.FromFile(@"D:\MEGA\Pictures\Puush\2021-09\GenshinImpact_2021-09-02_03-07-40.png")).ToSharedPixelFormat();
			var fast = new FastBitmap(png);

			var y = YouGotABite.Match(fast, YouGotABite.matchRect.Location);

			return;

			var minrect = Analyze(fast, new Rectangle(800, 200, 330, 40), px => px == Bgrx32.White);
			fast.Dispose();

			var crop = png.CropOut(minrect);
			crop.ApplyFilter(ImageExt.ExactWhiteFilter);
			crop.Save("fish_caught.png");
		}
	}
}
;