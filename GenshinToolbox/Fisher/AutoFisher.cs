
using IronOcr;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace GenshinToolbox.Fisher
{
	internal static class AutoFisher
	{
		const string FishingCaughtText = "You've got a bite!";
		static readonly string[] AllTexts = new[] { FishingCaughtText, "Switched to walking", "Switched to running" };

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
			var ocr = Ocr.GetInstance();
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
					bool found = false;
					foreach (var off in TrackRangeTopOff)
					{
						if (BarAssist(catchImg, new Rectangle(0, off, TrackRangeWidth, TrackRangeHeigth)))
						{
							LastRangeTopOff = off;
							found = true;
							break;
						}
					}

					if (!found && opt.AutoCatch)
					{
						CatchAssist(ocr, catchImg);
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
		const int OcrTopOffset = 100;
		static readonly Rectangle OcrOffset = new(0, OcrTopOffset, TrackRangeWidth, TrackRangeRectAll.Height - OcrTopOffset);

		public static bool CatchAssist(IronTesseract ocr, Bitmap catchImg)
		{
			catchImg.ApplyFilter(ImageExt.WhiteScaleFilter, OcrOffset);

			ocr.Configuration.WhiteListCharacters = FishingCaughtText.Allow();
			ocr.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.SingleLine;
			ocr.Configuration.TesseractVersion = TesseractVersion.Tesseract5;
			ocr.Configuration.EngineMode = TesseractEngineMode.LstmOnly;
			ocr.Language = OcrLanguage.EnglishFast;

			using var Input = new OcrInput(catchImg, OcrOffset);
			var Result = ocr.Read(Input);
			var text = Result.Text;

			var acc = Ocr.Lehvenshtein(text, FishingCaughtText);

			var now = DateTime.Now;
			if (acc < 5 && lastClick + TimeSpan.FromSeconds(1) < now)
			{
				Util.inp.Mouse.LeftButtonClick();
				lastClick = now;
				return true;
			}

			//Console.SetCursorPosition(0, 10);
			//Console.WriteLine("DETECTED: {0}               ", text);
			//Console.WriteLine("DIST WALK: {0}              ", Ocr.Lehvenshtein(text, "Switched to walking"));
			//Console.WriteLine("DIST FISH: {0}              ", acc);

			return false;
		}
	}
}
