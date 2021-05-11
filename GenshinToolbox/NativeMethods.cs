using System;
using System.Runtime.InteropServices;

namespace GenshinToolbox
{
	internal static class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out POINT lpPoint);

		[DllImport("user32.dll")]
		private static extern bool SetCursorPos(int x, int y);

		// Gets the Absolute (X/Y/X+W/Y+H) Coordinated of a window (With Border)
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		// Gets relative the size (X=0/Y=0/X+W/Y+H) of the windows content
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		// Gets the Absolute (X/Y) Position of the content of a window (Without Border)
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool ClientToScreen(IntPtr hWnd, out POINT lpPoint);

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
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}


	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int Left;
		public int Top;

		public POINT(int left, int top)
		{
			Left = left;
			Top = top;
		}

		public static POINT operator +(POINT a, POINT b) => new(a.Left + b.Left, a.Top + b.Top);
	}
}
