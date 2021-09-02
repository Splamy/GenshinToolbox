using System;
using System.Drawing;
using WindowsInput.Native;
using static GenshinToolbox.Util;

namespace GenshinToolbox.Collector
{
	static class AutoCollector
	{
		public static readonly int Frame = (int)Math.Ceiling(1000 / 60f);
		public static int Leeway = 0;

		private static readonly Point[] Expeditions = {
			// Mondstad
			//        (0)
			//   (4)       (1)
			//      (5)      (2)
			//                (3)
			new(810, 250), // 0 - Flower/Egg 1
			new(1060,340), // 1 - Ores
			new(1120,450), // 2 - Steak
			new(1180,660), // 3 - Ores
			new(565,405), // 4 - Mora
			new(745,535), // 5 - Carrot
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

		private static readonly Point MondstadtButton = new(90, 165);
		private static readonly Point LiyueButton = new(90, 235);

		private static readonly Point ClaimRewardsArea = new(606, 757); // somewhere bot left | same pos as decline button
		private static readonly Point ClaimButton = new(1740, 1025); // Or 'Send' button
		private static readonly Point Pick20hLength = new(1800, 690);

		private static readonly Point CharactersListOffset = new(50, 120);
		private static readonly Point CharactersBoxSize = new(860, 125);

		// Notes:
		// Clicking Dialague away with 'Esc' takes about 6 frames
		const int TimingCancelDialogueEsc = 12;
		const int TimingClickPre = 2;
		const int TimingClickExpPost = 2;
		const int TimingClickClaimPost = 5;


		public static void AutoCollect(int? slowdown = null, int? warmup = null)
		{
			warmup ??= slowdown;

			Util.Focus();


			//PreciseSleep(1000);
			//Console.WriteLine("Go");

			//for (int i = 0; i < 50; i++)
			//{
			//	SetRelativeCursorPos(new POINT { Left = 50 + 10 * (i + i % 2), Top = 50 + 10 * (i + (i + 1) % 2) });
			//	PreciseSleep(2 * Frame);
			//}

			//return;

			//for (var i = 2; i >= 0; i -= 1)
			//{
			//	Console.WriteLine("Frames: {0}", i);
			//	for (int ex = 0; ex < 3; ex++)
			//	{
			//		SetRelativeCursorPos(ClaimRewardsArea);

			//		PreciseSleep(300);

			//		ClickExpedition((Expedition)ex);
			//		ClickClaimOrSend();
			//		CancelDialogueEsc();

			//		SetRelativeCursorPos(ClaimRewardsArea);
			//	}
			//}

			//ClickTimed(ClaimButton);
			//PreciseSleep(Frame);
			//PressKey(VirtualKeyCode.ESCAPE);
			//PreciseSleep(Frame);
			//SetRelativeCursorPos(LiyueButton);
			//while (true)
			//{
			//	PreciseSleep(Frame);
			//	Util.inp.Mouse.LeftButtonClick();
			//}

			PreciseSleep(300);
			ClickTimed(MondstadtButton);
			PreciseSleep(100);

			Leeway = warmup ?? 5;
			CollectExpedition(Expedition.M_Flower);
			Leeway = slowdown ?? 0;
			CollectExpedition(Expedition.M_Ores1);
			CollectExpedition(Expedition.M_Steak);
			CollectExpedition(Expedition.M_Ores2);

			DispatchExpedition(Expedition.M_Ores1, 0);
			DispatchExpedition(Expedition.M_Ores2, 1);
			DispatchExpedition(Expedition.M_Flower, 3);
			DispatchExpedition(Expedition.M_Steak, 4);

			PreciseSleep(300);
			ClickTimed(LiyueButton);
			PreciseSleep(100);

			CollectExpedition(Expedition.L_Ores);
			DispatchExpedition(Expedition.L_Ores, 0);
		}

		static void CollectExpedition(Expedition ex)
		{
			ClickExpedition(ex);
			ClickClaimOrSend();
			CancelDialogueEsc();
		}

		static void DispatchExpedition(Expedition ex, int character)
		{
			ClickExpedition(ex);
			ClickTimed(Pick20hLength); // TODO
			ClickClaimOrSend();
			ClickTimed(new( // TODO
				CharactersListOffset.X + CharactersBoxSize.X / 2,
				CharactersListOffset.Y + CharactersBoxSize.Y / 2 + CharactersBoxSize.Y * character
				));
			// Sanity click, when we try to dispatch a already running expedition
			// at this point the 'Are you sure to cancel' dialogue will be up
			// so we close it in case.
			CancelDialogueClick();
		}

		static void ClickExpedition(Expedition ex)
		{
			SetRelativeCursorPos(Expeditions[(int)ex]);
			PreciseSleep((TimingClickPre + Leeway) * Frame);
			Util.inp.Mouse.LeftButtonClick();
			PreciseSleep((TimingClickExpPost + Leeway) * Frame);
		}

		static void ClickClaimOrSend()
		{
			SetRelativeCursorPos(ClaimButton);
			PreciseSleep((TimingClickPre + Leeway) * Frame);
			Util.inp.Mouse.LeftButtonClick();
			PreciseSleep((TimingClickClaimPost + Leeway) * Frame);
		}

		static void CancelDialogueEsc()
		{
			PressKey(VirtualKeyCode.ESCAPE);
			PreciseSleep((TimingCancelDialogueEsc + Leeway) * Frame);
		}

		// Also doubles as click cancel so we have to wait at least dialogue time
		static void CancelDialogueClick()
		{
			SetRelativeCursorPos(ClaimRewardsArea);
			PreciseSleep((TimingClickPre + Leeway) * Frame);
			Util.inp.Mouse.LeftButtonClick();
			PreciseSleep((TimingCancelDialogueEsc + Leeway) * Frame);
		}


	}
}
