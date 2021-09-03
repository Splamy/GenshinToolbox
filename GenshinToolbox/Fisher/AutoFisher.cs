
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace GenshinToolbox.Fisher
{
	internal static class AutoFisher
	{
		public static void Run(FisherOptions _)
		{
			RunInternal();
			//Iterate();
			//Record();

			//while (true)
			//{
			//	Console.WriteLine("catch: {0}", CatchAssist());
			//	Thread.Sleep(500);
			//}
		}

		static void RunInternal()
		{
			var sw = new Stopwatch();
			while (true)
			{
				Console.SetCursorPosition(0, 0);
				//Console.Clear();

				sw.Restart();

				using var catchImg = Capture.Game(TrackRangeRectAll);
				if (LastRangeTopOff.HasValue)
				{
					if (!BarAssist(catchImg, new Rectangle(0, LastRangeTopOff.Value, TrackRangeWidth, TrackRangeHeigth)))
						LastRangeTopOff = null;
				}
				else
				{
					foreach (var off in TrackRangeTopOff)
					{
						if (BarAssist(catchImg, new Rectangle(0, off, TrackRangeWidth, TrackRangeHeigth)))
						{
							LastRangeTopOff = off;
							break;
						}
					}
				}

				var workTime = sw.ElapsedMilliseconds;
				Thread.Sleep(Math.Max(1, Util.Frame - (int)sw.ElapsedMilliseconds));
				Console.WriteLine("Work: {0:000}ms  Frame: {1:000}ms               ", workTime, sw.ElapsedMilliseconds);
			}
		}

		static void Iterate()
		{
			cnt = 109;
			while (true)
			{
				Console.Clear();

				Console.WriteLine("Fish: {0}", cnt);

				using var catchImg = new Bitmap(Image.FromFile(@"D:\MEGA\Pictures\Puush\2021-09\GenshinImpact_2021-09-02_17-28-58.png")).ToSharedPixelFormat();
				var slice = TrackRangeRectAll.AddTop(TrackRangeTopOff[1]).SetHeight(TrackRangeHeigth);
				//catchImg.Save($"./DbgImgs/fish_current.png");

				//using var catchImg = new Bitmap(Image.FromFile($"./DbgImgs/fish_match/fish_{cnt:0000}.png")).ToSharedPixelFormat();
				// var slice = new Rectangle(0, TrackRangeTopOff[0], TrackRangeWidth, TrackRangeHeigth);

				BarAssist(catchImg, slice);
				cnt++;
				Console.ReadKey();
			}
		}

		static void Record()
		{
			while (true)
			{
				using var catchImg = Capture.Game(TrackRangeRectAll);
				catchImg.Save($"./DbgImgs/fish_{cnt++:0000}.png");

				try
				{
					BarAssist(catchImg, new Rectangle(0, TrackRangeTopOff[0], TrackRangeWidth, TrackRangeHeigth));
				}
				catch { }

				Thread.Sleep(350);
			}
		}

		static int cnt;


		static readonly Rectangle TrackRangeRectAll = new(719, 100, TrackRangeWidth, 80);
		const int TrackRangeWidth = 489;
		const int TrackRangeHeigth = 30;
		static readonly int[] TrackRangeTopOff = new[] { 0, 47 };
		static int? LastRangeTopOff = null;

		static readonly Color PissColor = Color.FromArgb(255, 255, 192);
		static readonly int[] MeasureBuffer = new int[TrackRangeWidth];

		static bool found;
		static int RangeVelo;
		static int BarVelo;
		static int LastRangePos; // Using center here
		static int LastBarPos;

		public static bool BarAssist(Bitmap catchImg, Rectangle slice)
		{
			//Console.SetCursorPosition(0, 0);

			//using var img = new Bitmap(Image.FromFile(@"D:\MEGA\Pictures\Puush\2021-09\GenshinImpact_2021-09-02_03-07-50.png"));
			//using var catchImg = img.CropOut(TrackBarRect);
			//using var catchImg = Capture.Game(TrackBarRect);
			//catchImg.Save($"./DbgImgs/fish_current.png");

			#region FindBar
			MeasureBuffer.AsSpan().Clear();

			using var fastbmp = new FastBitmap(catchImg);

			for (int y = slice.Top; y < slice.Bottom; y++)
			{
				var row = fastbmp.GetRow(y);
				for (int x = slice.Left; x < Math.Min(slice.Right, row.Length); x++)
				{
					int i = x - slice.Left;
					var p = row[x];
					if (p.R > 240 && p.G > 210 && (p.B > 30 && p.B < 220))
						MeasureBuffer[i]++;
				}
			}

			int bestBarPosRating = 0;
			int bestBarPos = 0;
			for (int x = 0; x < MeasureBuffer.Length; x++)
			{
				if (MeasureBuffer[x] > bestBarPosRating)
				{
					bestBarPosRating = MeasureBuffer[x];
					bestBarPos = x;
				}
			}
			#endregion

			#region FindTargetRange
			var avg = MeasureBuffer.Average();
			bool tracking = false;
			int largestBumpStart = 0;
			int largestBumpLength = 0;
			int currentBumpStart = 0;
			int currentBumpLength = 0;

			void StopTracking()
			{
				if (tracking)
				{
					if (currentBumpLength > largestBumpLength)
					{
						largestBumpLength = currentBumpLength;
						largestBumpStart = currentBumpStart;
					}
					tracking = false;
				}
			}

			for (int i = 0; i < MeasureBuffer.Length; i++)
			{
				if (MeasureBuffer[i] < avg)
				{
					StopTracking();
				}
				else
				{
					if (!tracking)
					{
						currentBumpLength = 0;
						currentBumpStart = i;
					}
					tracking = true;
					currentBumpLength++;
				}
			}

			StopTracking();
			#endregion

			var wasFound = found;
			if (bestBarPosRating == 0 || largestBumpLength < 30)
			{
				Console.WriteLine("No bar found                          ");
				found = false;
				return false;
			}
			found = true;
			//Console.WriteLine("Found                         ");
			//return;

			var barPos = bestBarPos;
			var range = (start: largestBumpStart, width: largestBumpLength);
			var rangeCenter = range.start + (range.width / 2);

			static int PxToDraw(int px) => 100 * px / TrackRangeWidth;

			Console.Write('[');
			Console.Write(new string(' ', PxToDraw(range.start)));
			Console.Write(new string('#', PxToDraw(range.width)));
			Console.Write(new string(' ', PxToDraw(TrackRangeWidth - (range.start + range.width))));
			Console.Write(']');
			Console.WriteLine();

			Console.Write('[');
			Console.Write(new string(' ', PxToDraw(barPos)));
			Console.Write('I');
			Console.Write(new string(' ', PxToDraw(TrackRangeWidth - barPos)));
			Console.Write(']');
			Console.WriteLine();

			if (!wasFound)
			{
				BarVelo = 0;
				RangeVelo = 0;
			}
			else
			{
				BarVelo = barPos - LastBarPos;
				RangeVelo = rangeCenter - LastRangePos;
			}
			LastBarPos = barPos;
			LastRangePos = rangeCenter;

			Console.WriteLine("Velo Bar:{0:000} Range:{1:000}     ", BarVelo, RangeVelo);

			int compareTarget;

			//switch (BarVelo, RangeVelo)
			//{
			//case ( <= 0, <= 0):
			//case ( > 0, > 0):
			//	compareTarget = rangeCenter;
			//	break;
			//case ( <= 0, > 0):
			//	compareTarget = rangeCenter + range.width / 4;
			//	break;
			//case ( > 0, <= 0):
			//	compareTarget = rangeCenter - range.width / 4;
			//	break;
			//}

			//compareTarget = rangeCenter + Math.Clamp(BarVelo, -range.width / 2, range.width / 2);

			compareTarget = rangeCenter;

			// Simple logic test
			if (barPos < compareTarget)
			{
				Util.inp.Mouse.LeftButtonDown();
				Console.WriteLine("Down            ");
			}
			else
			{
				Util.inp.Mouse.LeftButtonUp();
				Console.WriteLine("Up              ");
			}

			return true;
		}

		static readonly Rectangle ActionButtonRect = new(1594, 956, 75, 75);

		static readonly bool? T = true;
		static readonly bool? F = false;
		static readonly bool? _ = null;

		//.....
		//...##
		//..#..
		//.#...
		//.....
		public static readonly bool?[,] HookReady = {
			{ _, _, _, _, _ },
			{ _, _, _, T, T },
			{ _, F, T, F, _ },
			{ _, T, F, F, _ },
			{ _, _, _, _, _ },
		};

		//.....
		//.....
		//.###.
		//...#.
		//.....
		public static readonly bool?[,] HookWaiting = {
			{ _, _, _, _, _ },
			{ _, _, _, F, F },
			{ _, T, T, T, _ },
			{ _, F, F, T, _ },
			{ _, _, _, _, _ },
		};

		public static bool CatchAssist()
		{
			//using var img = new Bitmap(Image.FromFile(@"D:\MEGA\Pictures\Puush\2021-09\GenshinImpact_2021-09-02_06-30-04.png"));
			//using var catchImg = img.CropOut(ActionButtonRect);

			using var catchImg = Capture.Game(ActionButtonRect);
			catchImg.ApplyFilter(ImageExt.WhiteFilter);
			var mini = catchImg.ResizeTo(5, 5);

			int rateReady = 0;
			int rateWaiting = 0;
			int rateBit = 0;

			static void HitMatch(ref int rate, bool? map, bool hit)
			{
				if (map is { } m)
				{
					rate += m == hit ? 1 : -1;
				}
			}

			for (int x = 0; x < 5; x++)
			{
				Console.SetCursorPosition(0, 10 + x);
				for (int y = 0; y < 5; y++)
				{
					bool hit = mini.GetPixel(x, y).RgbEq(Color.Black);
					HitMatch(ref rateReady, HookReady[y, x], hit);
					HitMatch(ref rateWaiting, HookWaiting[y, x], hit);
					rateBit += hit ? 1 : 0;
					Console.Write("{0}", hit ? '#' : '.');
				}
			}
			Console.WriteLine();
			if (rateBit == 0)
			{
				Console.WriteLine("Status: HIT");
				Util.inp.Mouse.LeftButtonClick();
			}
			else
			{
				Console.WriteLine("Status: {0}", rateReady > rateWaiting ? "Ready" : "Waiting");
			}

			return false;
		}
	}
}
