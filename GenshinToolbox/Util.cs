using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;
using static GenshinToolbox.NativeMethods;

namespace GenshinToolbox
{
	public static class Util
	{
		public static readonly int Frame = (int)Math.Ceiling(1000 / 60f);

		private static readonly Point MinimizedLoc = new(-32000, -32000);

		private static Point? _WindowOffset = null;
		private static Process? _proc = null;

		public static readonly InputSimulator inp = new();
		public static Point WindowOffset => GetWindowRect();

		public static void Focus()
		{
			var proc = GetProcess();
			SetForegroundWindow(proc.MainWindowHandle);
			_WindowOffset = null;
			Thread.Sleep(20);
		}

		private static Process GetProcess()
		{
			return _proc ??= Process.GetProcessesByName("GenshinImpact")[0];
		}

		private static Point GetWindowRect()
		{
			if (_WindowOffset is { } off && off != MinimizedLoc)
				return _WindowOffset.Value;

			var proc = GetProcess();
			off = ClientToScreen(proc.MainWindowHandle);
			_WindowOffset = off;
			return off;
		}

		public static bool GenshinHasFocus()
			=> GetForegroundWindow() == GetProcess()?.MainWindowHandle;

		public static void WaitForFocus()
		{
			if (!GenshinHasFocus())
			{
				Console.WriteLine("Waiting for focus");

				while (!GenshinHasFocus())
				{
					Thread.Sleep(100);
				}

				Console.WriteLine("Resuming task in 500ms");
				Thread.Sleep(500);
				_WindowOffset = null;
			}
		}

		public static void ClickTimed(Point p)
		{
			SetRelativeCursorPos(p);
			PreciseSleep(5 * Frame);
			inp.Mouse.LeftButtonClick();
			PreciseSleep(5 * Frame);
		}

		public static void PressKey(VirtualKeyCode key)
		{
			WaitForFocus();
			inp.Keyboard.KeyPress(key);
		}

		public static void SetRelativeCursorPos(Point p)
		{
			var realPos = WindowOffset.Add(p);
			SetCursorPosPlus(realPos.X, realPos.Y);
		}

		public static void PreciseSleep(int ms)
		{
			var sw = Stopwatch.StartNew();
			if (ms > 20)
			{
				Thread.Sleep(ms - 20);
			}
			while (sw.ElapsedMilliseconds < ms)
			{
				Thread.SpinWait(10_000);
			}
			WaitForFocus();
		}

		public static void PressButton(this IXbox360Controller controller, Xbox360Button button)
		{
			WaitForFocus();

			controller.SetButtonState(button, true);
			PreciseSleep(2 * Frame);
			controller.SetButtonState(button, false);
		}

		public static void TapAxis(this IXbox360Controller controller, AxisDir dir)
		{
			WaitForFocus();

			switch (dir)
			{
			case AxisDir.Up:
				controller.SetAxisValue(Xbox360Axis.LeftThumbY, short.MaxValue);
				break;
			case AxisDir.Right:
				controller.SetAxisValue(Xbox360Axis.LeftThumbX, short.MaxValue);
				break;
			case AxisDir.Down:
				controller.SetAxisValue(Xbox360Axis.LeftThumbY, short.MinValue);
				break;
			case AxisDir.Left:
				controller.SetAxisValue(Xbox360Axis.LeftThumbX, short.MinValue);
				break;
			}
			PreciseSleep(2 * Frame);
			controller.SetAxisValue(Xbox360Axis.LeftThumbX, 0);
			controller.SetAxisValue(Xbox360Axis.LeftThumbY, 0);
		}

		public static Point Add(this Point operand1, Point operand2) => new(operand1.X + operand2.X, operand1.Y + operand2.Y);
		public static Point Sub(this Point operand1, Point operand2) => new(operand1.X - operand2.X, operand1.Y - operand2.Y);

		public static Rectangle AddTop(this Rectangle rect, int add) => new(new(rect.Left, rect.Top + add), rect.Size);

		public static Rectangle SetHeight(this Rectangle rect, int height) => new(rect.Location, new(rect.Width, height));

		public static Rectangle Inner(this Rectangle rect, Rectangle inner)
			=> new(rect.Location.Add(inner.Location), new Size(
					Math.Min(rect.Width - inner.Left, inner.Width),
					Math.Min(rect.Height - inner.Top, inner.Height)
				));
	}

	public enum AxisDir
	{
		Up,
		Right,
		Down,
		Left
	}
}
