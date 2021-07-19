using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WindowsInput;
using static GenshinToolbox.NativeMethods;

namespace GenshinToolbox
{
	public static class Util
	{
		private static POINT? _WindowOffset = null;
		private static Process _proc = null;

		public static readonly InputSimulator inp = new();
		public static POINT WindowOffset => GetWindowRect();

		public static void Focus()
		{
			var proc = GetProcess();
			SetForegroundWindow(proc.MainWindowHandle);
			_WindowOffset = null;
			Thread.Sleep(20);
		}

		private static Process GetProcess()
		{
			if (_proc is null)
			{
				_proc = Process.GetProcessesByName("GenshinImpact").First();
			}
			return _proc;
		}

		private static POINT GetWindowRect()
		{
			if (!_WindowOffset.HasValue)
			{
				var proc = GetProcess();
				ClientToScreen(proc.MainWindowHandle, out var rect);
				_WindowOffset = rect;
			}
			return _WindowOffset.Value;
		}

		public static bool GenshinHasFocus()
			=> GetForegroundWindow() == GetProcess()?.MainWindowHandle;

		public static void WaitForFocus()
		{
			if (!Util.GenshinHasFocus())
			{
				Console.WriteLine("Waiting for focus");

				while (!Util.GenshinHasFocus())
				{
					Thread.Sleep(100);
				}

				Console.WriteLine("Resuming task in 500ms");
				Thread.Sleep(500);
			}
		}
	}
}
