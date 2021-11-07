using CommandLine;
using GenshinToolbox.ArtScraper;
using GenshinToolbox.Collector;
using GenshinToolbox.Fisher;
using GenshinToolbox.Player;
using System;
using System.Globalization;
using System.Threading;

namespace GenshinToolbox
{
	static class Program
	{
		static void Main(string[] args)
		{
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

			MuseEngine.Validate();
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			Parser.Default.ParseArguments<PlayerOptions, CollectOptions, ArtifactsOptions, FisherOptions, UtilOptions>(args).MapResult(
				(Func<PlayerOptions, int>)RunPlayer,
				(Func<CollectOptions, int>)RunCollect,
				(Func<ArtifactsOptions, int>)RunArtifacts,
				(Func<FisherOptions, int>)RunFisher,
				(UtilOptions _) => {
					ImgMatch.DoStuff();
					return 0;
				},
				errs =>
				{
					ConsoleSelector();
					return 0;
				});
		}

		static void ConsoleSelector()
		{
			while (true)
			{
				Console.Clear();
				Console.WriteLine("1. Music Player");
				Console.WriteLine("2. Expedition Collector");
				Console.WriteLine("3. Artifacts Analyzer");
				Console.WriteLine("4. Fishing Assistant");

				switch (Console.ReadKey().Key)
				{
				case ConsoleKey.D1:
					RunPlayer(new());
					break;
				case ConsoleKey.D2:
					RunCollect(new());
					break;
				case ConsoleKey.D3:
					RunArtifacts(new());
					break;
				case ConsoleKey.D4:
					RunFisher(new());
					break;
				default:
					continue;
				}
				break;
			}
		}

		static int RunPlayer(PlayerOptions _)
		{
			var museEngine = new MuseEngine();
			museEngine.InteractiveConsole();
			return 0;
		}

		static int RunCollect(CollectOptions o)
		{
			AutoCollector.AutoCollect(o.CollectSlowdown, o.CollectWarmup);
			return 0;
		}

		static int RunArtifacts(ArtifactsOptions o)
		{
			Scraper.Run(o);
			return 0;
		}

		static int RunFisher(FisherOptions o)
		{
			AutoFisher.Run(o);
			return 0;
		}
	}

	[Verb("player", HelpText = "")]
	class PlayerOptions { }

	[Verb("collect", HelpText = "")]
	class CollectOptions
	{
		[Option('w', "warmup", Required = false)]
		public int? CollectWarmup { get; set; }
		[Option('s', "slowdown", Required = false)]
		public int? CollectSlowdown { get; set; }
	}

	[Verb("artifacts", HelpText = "")]
	class ArtifactsOptions
	{
		[Option('d', "debug", Required = false, Default = false)]
		public bool Debug { get; set; }

		[Option('m', "max", Required = false, Default = null)]
		public int Max { get; set; }
		[Option('c', "capture", Required = false, Default = false)]
		public bool Capture { get; set; }

		[Option('a', "analyze", Required = false, Default = false)]
		public bool Analyze { get; set; }

		[Option('l', "minlevel", Required = false, Default = null)]
		public int MinLevel { get; set; }
		[Option('s', "minstars", Required = false, Default = null)]
		public int MinStars { get; set; }
		[Option('u', "keep-unique", Required = false, Default = false)]
		public bool KeepUnique { get; set; }
	}

	[Verb("fisher", HelpText = "")]
	class FisherOptions
	{
		[Option('c', "no-auto-catch", Required = false, Default = false)]
		public bool NoAutoCatch { get; set; }
		public bool AutoCatch => !NoAutoCatch;
	}

	[Verb("util", HelpText = "")]
	class UtilOptions { }
}
