using System.Threading;
using WindowsInput.Native;
using static GenshinToolbox.NativeMethods;

namespace GenshinToolbox.Collector
{
	class AutoCollector
	{
		public const int MinFrames = 5 * 16;

		private static readonly POINT[] Expeditions = {
			// Mondstad
			//        (0)
			//   (4)       (1)
			//      (5)      (2)
			//                (3)
			new(810, 250), // 0 - Flower/Egg 1
			new(1060,340), // 1 - Ores
			new(1120,450), // 2 - Steak
			new(1180,660), // 3 - Ores
			new(-1,-1), // 4 - Mora
			new(-1,-1), // 5 - Carrot
			// Liyue
			//     (0)
			//          (1)
			//  (2)   (3)     (4)
			//      (5)
			new(-1,-1), // 0 - Mora
			new(960,450), // 1 - Ores
			new(-1,-1), // 2 - Mora
			new(-1,-1), // 3 - Lotus
			new(-1,-1), // 4 - Carrot
			new(-1,-1), // 5 - Lotus
		};

		enum Expedition
		{
			M_Flower = 0,
			M_Ores1 = 1,
			M_Steak = 2,
			M_Ores2 = 3,
			M_Mora = 4,
			M_Carrot = 5,
			L_Mora1 = 6,
			L_Ores = 7,
			L_Mora2 = 8,
			L_Lotus1 = 9,
			L_Carrot = 10,
			L_Lotus2 = 11,
		}

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

			Thread.Sleep(300);
			ClickTimed(MondstadtButton);

			CollectExpedition(Expedition.M_Flower);
			CollectExpedition(Expedition.M_Ores1);
			CollectExpedition(Expedition.M_Steak);
			CollectExpedition(Expedition.M_Ores2);

			DispatchExpedition(Expedition.M_Ores1, 0);
			DispatchExpedition(Expedition.M_Ores2, 1);
			DispatchExpedition(Expedition.M_Flower, 3);
			DispatchExpedition(Expedition.M_Steak, 4);

			Thread.Sleep(300);
			ClickTimed(LiyueButton);

			CollectExpedition(Expedition.L_Ores);
			DispatchExpedition(Expedition.L_Ores, 0);
		}

		static void CollectExpedition(Expedition ex)
		{
			ClickTimed(Expeditions[(int)ex]);
			ClickTimed(ClaimButton);
			PressTimed(VirtualKeyCode.ESCAPE);
			Thread.Sleep(100);
		}

		static void DispatchExpedition(Expedition ex, int character)
		{
			ClickTimed(Expeditions[(int)ex]);
			ClickTimed(Pick20hLength);
			ClickTimed(ClaimButton);
			ClickTimed(new(
				CharactersListOffset.Left + CharactersBoxSize.Left / 2,
				CharactersListOffset.Top + CharactersBoxSize.Top / 2 + CharactersBoxSize.Top * character
				));
			// Sanity click, when we try to dispatch a already running expedition
			// at this point the 'Are you sure to cancel' dialogue will be up
			// so we close it in case.
			ClickTimed(ClaimRewardsArea);
		}

		public static void ClickTimed(POINT p)
		{
			SetRelativeCursorPos(p);
			Thread.Sleep(MinFrames);
			//Util.inp.Mouse.LeftButtonDown();
			//Thread.Sleep(MinFrames);
			//Util.inp.Mouse.LeftButtonUp();
			//Thread.Sleep(MinFrames);
			Util.inp.Mouse.LeftButtonClick();
			Thread.Sleep(MinFrames);
		}

		public static void PressTimed(VirtualKeyCode key)
		{
			Util.inp.Keyboard.KeyPress(key);
		}

		public static void SetRelativeCursorPos(POINT p)
		{
			var realPos = Util.WindowOffset + p;
			SetCursorPosPlus(realPos.Left, realPos.Top);
		}
	}
}
