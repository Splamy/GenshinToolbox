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
	class Program
	{
		static void Main(string[] args)
		{
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

			MuseEngine.Validate();
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			Parser.Default.ParseArguments<PlayerOptions, CollectOptions, ArtifactsOptions, FisherOptions>(args).MapResult(
				(PlayerOptions o) =>
				{
					var museEngine = new MuseEngine();
					museEngine.InteractiveConsole();
					return 0;
				},
				(CollectOptions o) =>
				{
					AutoCollector.AutoCollect(o.CollectSlowdown, o.CollectWarmup);
					return 0;
				},
				(ArtifactsOptions o) =>
				{
					Scraper.Run(o);
					return 0;
				},
				(FisherOptions o) =>
				{
					AutoFisher.Run(o);
					return 0;
				},
				errs => 1);
		}
	}


	[Verb("player", HelpText = "")]
	class PlayerOptions
	{
	}

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

		[Option('m', "max", Required = false, Default = int.MaxValue)]
		public int Max { get; set; }
		[Option('c', "capture", Required = false, Default = false)]
		public bool Capture { get; set; }

		[Option('a', "analyze", Required = false, Default = false)]
		public bool Analyze { get; set; }

		[Option('l', "minlevel", Required = false, Default = 0)]
		public int MinLevel { get; set; }
		[Option('s', "minstars", Required = false, Default = 0)]
		public int MinStars { get; set; }
	}

	[Verb("fisher", HelpText = "")]
	class FisherOptions
	{
	}
}
