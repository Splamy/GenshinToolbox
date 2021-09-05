using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace GenshinToolbox.Fisher
{
	internal static class AutoFisher
	{
		public static void Run(FisherOptions opt)
		{
			RunInternal(opt);
			//Iterate();
			//Record();

			//while (true)
			//{
			//	Console.WriteLine("catch: {0}", CatchAssist());
			//	Thread.Sleep(500);
			//}
		}

		static void RunInternal(FisherOptions opt)
		{
			var sw = new Stopwatch();
			while (true)
			{
				var workTime = sw.ElapsedMilliseconds;
				Thread.Sleep(Math.Max(1, Util.Frame - (int)sw.ElapsedMilliseconds));
				Console.SetCursorPosition(0, Console.WindowHeight - 1);
				Console.Write("Work: {0:000}ms  Frame: {1:000}ms               ", workTime, sw.ElapsedMilliseconds);
				sw.Restart();
				Console.SetCursorPosition(0, 0);
				//Console.Clear();

				if (!ValidateFishing())
					continue;

				using var catchImg = Capture.Game(TrackRangeRectAll);
				using var fastbmp = new FastBitmap(catchImg);
				if (LastRangeTopOff.HasValue)
				{
					if (!BarAssist(fastbmp, new Rectangle(0, LastRangeTopOff.Value, TrackRangeWidth, TrackRangeHeigth)))
						LastRangeTopOff = null;
				}
				else
				{
					bool found = false;
					foreach (var off in TrackRangeTopOff)
					{
						if (BarAssist(fastbmp, new Rectangle(0, off, TrackRangeWidth, TrackRangeHeigth)))
						{
							LastRangeTopOff = off;
							found = true;
							break;
						}
					}

					if (!found && opt.AutoCatch)
					{
						CatchAssist(fastbmp);
					}
				}
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

				BarAssist(new FastBitmap(catchImg), slice);
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
					BarAssist(new FastBitmap(catchImg), new Rectangle(0, TrackRangeTopOff[0], TrackRangeWidth, TrackRangeHeigth));
				}
				catch { }

				Thread.Sleep(350);
			}
		}

		static int cnt;

		static readonly Rectangle TrackRangeRectAll = new(719, 100, TrackRangeWidth, 140);
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

		public static bool BarAssist(FastBitmap fastbmp, Rectangle slice)
		{
			//Console.SetCursorPosition(0, 0);

			//using var img = new Bitmap(Image.FromFile(@"D:\MEGA\Pictures\Puush\2021-09\GenshinImpact_2021-09-02_03-07-50.png"));
			//using var catchImg = img.CropOut(TrackBarRect);
			//using var catchImg = Capture.Game(TrackBarRect);
			//catchImg.Save($"./DbgImgs/fish_current.png");

			#region FindBar
			MeasureBuffer.AsSpan().Clear();

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

			const int MinClickBoost = 25; // about 25 px are added after a 1 frame click

			var absRangeVelo = Math.Abs(RangeVelo);
			var absClampRangeVelo = Math.Min(absRangeVelo, range.width / 4);

			int compareTarget = RangeVelo switch
			{
				0 => rangeCenter - MinClickBoost,
				< 0 => compareTarget = rangeCenter - ((range.width / 4) + Math.Min(absRangeVelo, range.width / 4)),
				> 0 => compareTarget = rangeCenter + Math.Min(absRangeVelo, range.width / 4) - MinClickBoost,
			};

			//int compareTarget = rangeCenter + Math.Clamp(RangeVelo, -range.width / 2, range.width / 4);

			//int compareTarget = rangeCenter;

			compareTarget = Math.Clamp(compareTarget, range.start, range.start + range.width);
			if (!Util.GenshinHasFocus())
			{
				Console.WriteLine("Waiting for focus            ");
				return false;
			}

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

			var consoleWidth = Console.WindowWidth;
			int PxToDraw(int px) => Math.Clamp(consoleWidth * px / TrackRangeWidth, 0, consoleWidth - 1);
			var clearLine = new string(' ', consoleWidth);
			var (cursorLeft, cursorTop) = Console.GetCursorPosition();
			Console.SetCursorPosition(0, cursorTop);
			Console.Write(clearLine);

			Console.SetCursorPosition(PxToDraw(range.start), cursorTop);
			Console.Write('<');
			Console.SetCursorPosition(PxToDraw(range.start + range.width), cursorTop);
			Console.Write('>');
			Console.SetCursorPosition(PxToDraw(compareTarget), cursorTop);
			Console.Write('_');
			Console.SetCursorPosition(PxToDraw(barPos), cursorTop);
			Console.Write('I');

			Console.SetCursorPosition(0, cursorTop + 1);

			return true;
		}

		static DateTime lastClick = DateTime.MinValue;

		public static bool CatchAssist(FastBitmap fastbmp)
		{
			var now = DateTime.Now;
			var onCooldown = lastClick + TimeSpan.FromSeconds(1) > now;

			if (onCooldown)
				return true;

			var conf = ImgMatch.YouGotABite.Match(fastbmp, ImgMatch.YouGotABite.matchRect.Location.Sub(TrackRangeRectAll.Location));

			Console.WriteLine("Capture Match: {0:0.00}", conf);
			if (conf > 0.95)
			{
				Util.inp.Mouse.LeftButtonClick();
				lastClick = now;
				return true;
			}

			return false;
		}

		static readonly Rectangle IsFishingRect = new(1600, 950, 70, 100);
		static readonly Rectangle MouseRect = new(23, 77, 8, 8);
		static readonly Rectangle AbsMouseRect = IsFishingRect.Inner(MouseRect);
		static readonly Bgrx32 MouseColor = new(255, 233, 44);

		public static bool ValidateFishing()
		{
			using var img = Capture.Game(AbsMouseRect);
			//img.Save("./DbgImgs/fish_check.png");
			//Thread.Sleep(1000);
			using var fastImg = new FastBitmap(img);
			int findYellow = 0;
			fastImg.ApplyFilter((ref Bgrx32 px) => findYellow += (px == MouseColor) ? 1 : 0);
			if (findYellow == 34)
			{
				return true;
			}
			else
			{
				Console.WriteLine("Not fishing...                              ");
				return false;
			}
		}
	}
}
