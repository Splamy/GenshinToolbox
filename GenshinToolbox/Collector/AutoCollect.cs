using System.Threading;
using static GenshinToolbox.NativeMethods;

namespace GenshinToolbox.Collector
{
	class AutoCollector
	{
		public const int MinFrames = 5 * 16;

		private static readonly POINT[] Expeditions = {
			// Mondstad
			//        (1)
			//   (5)       (2)
			//      (6)      (3)
			//                (4)
			new(810, 250), // 1 - Flower/Egg 1
			new(1060,340), // 2 - Ores
			new(1120,450), // 3 - Steak
			new(1180,660), // 4 - Ores
			new(-1,-1), // 5 - Mora
			new(-1,-1), // 6 - Carrot
			// Liyue
			//     (1)
			//          (2)
			//  (3)   (4)     (5)
			//      (6)
			new(-1,-1), // 1 - Mora
			new(960,450), // 2 - Ores
			new(-1,-1), // 3 - Mora
			new(-1,-1), // 4 - Lotus
			new(-1,-1), // 5 - Carrot
			new(-1,-1), // 6 - Lotus
		};

		private static readonly POINT MondstadtButton = new(90, 165);
		private static readonly POINT LiyueButton = new(90, 235);

		//private static readonly POINT ClaimRewardsArea = new(1400, 400); // somewhere top right
		private static readonly POINT ClaimRewardsArea = new(606, 757); // somewhere bot left | same pos as decline button

		private static readonly POINT ClaimButton = new(1740, 1025);
		private static readonly POINT Pick20hLength = new(1800, 690);

		private static readonly POINT CharactersListOffset = new(50, 120);
		private static readonly POINT CharactersBoxSize = new(860, 125);

		public static void AutoCollect()
		{
			Util.Focus();

			Thread.Sleep(200);

			ClickTimed(MondstadtButton);
			Thread.Sleep(300);

			CollectExpedition(0);
			CollectExpedition(1);
			CollectExpedition(2);
			CollectExpedition(3);

			DispatchExpedition(0, 3);
			DispatchExpedition(1, 0);
			DispatchExpedition(2, 1);
			DispatchExpedition(3, 4);

			Thread.Sleep(500);
			ClickTimed(LiyueButton);

			CollectExpedition(7);
			DispatchExpedition(7, 0);
		}

		static void CollectExpedition(int ex)
		{
			ClickTimed(Expeditions[ex]);
			ClickTimed(ClaimButton);
			Thread.Sleep(300);
			ClickTimed(ClaimRewardsArea);
			Thread.Sleep(100);
		}

		static void DispatchExpedition(int ex, int character)
		{
			ClickTimed(Expeditions[ex]);
			ClickTimed(Pick20hLength);
			ClickTimed(ClaimButton);
			ClickTimed(new(
				CharactersListOffset.Left + CharactersBoxSize.Left / 2,
				CharactersListOffset.Top + CharactersBoxSize.Top / 2 + CharactersBoxSize.Top * character
				));
			ClickTimed(ClaimRewardsArea);
		}

		public static void ClickTimed(POINT p)
		{
			SetRelativeCursorPos(p);
			Thread.Sleep(MinFrames);
			Util.inp.Mouse.LeftButtonDown();
			Thread.Sleep(MinFrames);
			Util.inp.Mouse.LeftButtonUp();
			Thread.Sleep(MinFrames);
		}

		public static void SetRelativeCursorPos(POINT p)
		{
			var realPos = Util.WindowOffset + p;
			SetCursorPosPlus(realPos.Left, realPos.Top);
		}
	}
}
