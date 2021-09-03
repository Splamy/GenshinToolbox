using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GenshinToolbox
{
	internal static class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool GetCursorPos(out POINT lpPoint);

		[DllImport("user32.dll")]
		private static extern bool SetCursorPos(int x, int y);

		// Gets the Absolute (X/Y/X+W/Y+H) Coordinated of a window (With Border)
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		// Gets relative the size (X=0/Y=0/X+W/Y+H) of the windows content
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		// Gets the Absolute (X/Y) Position of the content of a window (Without Border)
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool ClientToScreen(IntPtr hWnd, out POINT lpPoint);

		public static Point GetCursorPos()
		{
			GetCursorPos(out var pos);
			return pos.ToPoint();
		}

		public static void SetCursorPosPlus(int x, int y)
		{
			for (int i = 0; i < 3; i++)
			{
				SetCursorPos(x, y);
				GetCursorPos(out var actual);
				if (actual.Left == x && actual.Top == y)
					break;
			}
		}

		public static Rectangle GetWindowRect(IntPtr hWnd)
		{
			GetWindowRect(hWnd, out var rect);
			return rect.ToRectangle();
		}

		public static Rectangle GetClientRect(IntPtr hWnd)
		{
			GetClientRect(hWnd, out var rect);
			return rect.ToRectangle();
		}

		public static Point ClientToScreen(IntPtr hWnd)
		{
			ClientToScreen(hWnd, out var pos);
			return pos.ToPoint();
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

			public Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int Left;
			public int Top;

			public POINT(int left, int top)
			{
				Left = left;
				Top = top;
			}

			public Point ToPoint() => new(Left, Top);
		}
	}
}
