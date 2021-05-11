using GenshinToolbox.Collector;
using GenshinToolbox.Player;
using System;

namespace GenshinToolbox
{
	class Program
	{
		static void Main(string[] args)
		{
			MuseEngine.Validate();
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			string option = "";

			if (args.Length == 0)
			{
				// do selection
			}
			else
			{
				option = args[0].TrimStart('-');
			}

			switch (option)
			{
				case "p":
				case "player":
					MuseEngine.RunMusic();
					break;

				case "c":
				case "collect":
					AutoCollector.AutoCollect();
					break;

				default:
					Console.WriteLine("Unknown option");
					break;
			}
		}
	}
}
