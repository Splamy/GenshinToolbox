using IronOcr;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace GenshinToolbox.ArtScraper
{
	static class Scraper
	{
		private static readonly Dictionary<Stat, string> StatNames = new()
		{
			{ Stat.Atk, "ATK" },
			{ Stat.AtkPerc, "ATK" },
			{ Stat.Hp, "HP" },
			{ Stat.HpPerc, "HP" },
			{ Stat.Def, "DEF" },
			{ Stat.DefPerc, "DEF" },
			{ Stat.EnergyRecharge, "Energy Recharge" },
			{ Stat.ElementalMastery, "Elemental Mastery" },
			{ Stat.CritDmg, "CRIT DMG" },
			{ Stat.CritRate, "CRIT Rate" },
			{ Stat.PyroDmgBonus, "Pyro DMG Bonus" },
			{ Stat.HydroDmgBonus, "Hydro DMG Bonus" },
			{ Stat.ElectroDmgBonus, "Electro DMG Bonus" },
			{ Stat.AnemoDmgBonus, "Anemo DMG Bonus" },
			{ Stat.CryoDmgBonus, "Cryo DMG Bonus" },
			{ Stat.GeoDmgBonus, "Geo DMG Bonus" },
			{ Stat.PhysicalDmgBonus, "Physical DMG Bonus" },
			{ Stat.HealingBonusPerc, "Healing Bonus" },
		};

		private static readonly Dictionary<Slot, string> SlotNames = new()
		{
			{ Slot.Flower, "Flower of Life" },
			{ Slot.Plume, "Plume of Death" },
			{ Slot.Sands, "Sands of Eon" },
			{ Slot.Goblet, "Goblet of Eonothem" },
			{ Slot.Circlet, "Circlet of Logos" },
		};

		private static readonly Dictionary<ArtSet, string> SetNames = new()
		{
			{ ArtSet.GladiatorsFinale, "Gladiator's Finale" },
			{ ArtSet.WanderersTroupe, "Wanderer's Troupe" },
			{ ArtSet.ViridescentVenerer, "Viridescent Venerer" },
			{ ArtSet.ThunderingFury, "Thundering Fury" },
			{ ArtSet.Thundersoother, "Thundersoother" },
			{ ArtSet.CrimsonWitchOfFlames, "Crimson Witch of Flames" },
			{ ArtSet.Lavawalker, "Lavawalker" },
			{ ArtSet.ArchaicPetra, "Archaic Petra" },
			{ ArtSet.RetracingBolide, "Retracing Bolide" },
			{ ArtSet.MaidenBeloved, "Maiden Beloved" },
			{ ArtSet.NoblesseOblige, "Noblesse Oblige" },
			{ ArtSet.BloodstainedChivalry, "Bloodstained Chivalry" },
			{ ArtSet.BlizzardStrayer, "Blizzard Strayer" },
			{ ArtSet.HeartOfDepth, "Heart of Depth" },
			{ ArtSet.TenacityOfTheMillelith, "Tenacity of the Millelith" },
			{ ArtSet.PaleFlame, "Pale Flame" },
			{ ArtSet.EmblemOfSeveredFate, "Emblem of Severed Fate" },
			{ ArtSet.ShimenawasReminiscence, "Shimenawa's Reminiscence" },
			{ ArtSet.Instructor, "Instructor" },
			{ ArtSet.TheExile, "The Exile" },
			{ ArtSet.ResolutionOfSojourner, "Resolution of Sojourner" },
			{ ArtSet.MartialArtist, "Martial Artist" },
			{ ArtSet.DefendersWill, "Defender's Will" },
			{ ArtSet.TinyMiracle, "Tiny Miracle" },
			{ ArtSet.BraveHeart, "Brave Heart" },
			{ ArtSet.Gambler, "Gambler" },
			{ ArtSet.Scholar, "Scholar" },
			{ ArtSet.PrayersForWisdom, "Prayers for Wisdom" },
			{ ArtSet.PrayersToSpringtime, "Prayers to Springtime" },
			{ ArtSet.PrayersForIllumination, "Prayers for Illumination" },
			{ ArtSet.PrayersForDestiny, "Prayers for Destiny" },
			{ ArtSet.Berserker, "Berserker" },
			{ ArtSet.LuckyDog, "Lucky Dog" },
			{ ArtSet.TravelingDoctor, "Traveling Doctor" },
			{ ArtSet.Adventurer, "Adventurer" },
			{ ArtSet.Initiate, "Initiate" },
		};

		private static readonly Stat[] SubStats = new[] {
			Stat.Atk,
			Stat.AtkPerc,
			Stat.Hp,
			Stat.HpPerc,
			Stat.Def,
			Stat.DefPerc,
			Stat.EnergyRecharge,
			Stat.ElementalMastery,
			Stat.CritDmg,
			Stat.CritRate,
		};

		private static readonly Dictionary<Slot, (Stat[] main, Stat[] sub)> StatCategory = new()
		{
			{ Slot.Flower, (new[] { Stat.Hp }, SubStats) },
			{ Slot.Plume, (new[] { Stat.Atk }, SubStats) },
			{ Slot.Sands, (new[] { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.EnergyRecharge }, SubStats) },
			{ Slot.Goblet, (new[] { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.PyroDmgBonus, Stat.HydroDmgBonus, Stat.ElectroDmgBonus, Stat.AnemoDmgBonus, Stat.CryoDmgBonus, Stat.GeoDmgBonus, Stat.PhysicalDmgBonus, }, SubStats) },
			{ Slot.Circlet, (new[] { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.CritRate, Stat.CritDmg, Stat.HealingBonusPerc }, SubStats) },
		};

		const string Numbers = "0123456789";
		const string AdditionalCharacter = ".,+%";

		const string CharsNumbers = Numbers + AdditionalCharacter;
		const string CharsSet = ":";

		const string DbgFolder = "DbgImgs";
		const string ArtsFolder = "ArtImgs";

		public static void Run(ArtifactsOptions opts)
		{
			using var c = new ViGEmClient();
			var x360 = c.CreateXbox360Controller();
			x360.Connect();

			while (true)
			{
				Console.Clear();
				Console.WriteLine("1. Scan all artifacts (make sure game is in controller mode)");
				Console.WriteLine("2. Analyze all artifacts");
				Console.WriteLine("3. Switch game to controller input");
				Console.WriteLine("4. Switch game to kdb+mouse input");
				Console.WriteLine("5. Navigate to artifacts (make sure game is in controller mode)");

				switch (Console.ReadKey().KeyChar)
				{
				case '1':
					CaptureArts(opts, x360);
					break;
				case '2':
					Console.Write("Min level (Default: >=0):");
					if (!int.TryParse(Console.ReadLine()!.Trim(), out var minLevel))
						minLevel = 0;
					Console.Write("Min stars (Default: >=5):");
					if (!int.TryParse(Console.ReadLine()!.Trim(), out var minStars))
						minStars = 5;

					opts.MinLevel = minLevel;
					opts.MinStars = minStars;
					AnalyzeAll(opts);
					break;
				case '3':
					SwitchToController();
					break;
				case '4':
					SwitchToKbdMouse(x360);
					break;
				case '5':
					Console.WriteLine("Not implemented");
					Thread.Sleep(2000);
					break;
				}

			}
		}

		// Kbd / Controller switching utils

		const int Timing = 1000;

		private static void SwitchToController()
		{
			Util.Focus();

			for (int i = 0; i < 5; i++)
			{
				Util.PressKey(VirtualKeyCode.ESCAPE);
				Thread.Sleep(Timing);
			}

			if (!IsMenuOpen())
			{
				Console.WriteLine("Opening menu");
				Util.PressKey(VirtualKeyCode.ESCAPE);
				Thread.Sleep(Timing);
			}
			else
			{
				Console.WriteLine("Menu is open");
			}

			Util.ClickTimed(new Point(50, 820));
			Thread.Sleep(Timing);

			Util.ClickTimed(new Point(1630, 215));
			Thread.Sleep(Timing);

			Util.ClickTimed(new Point(1630, 315));
			Thread.Sleep(Timing);
		}

		private static void EnterMenuWithController(IXbox360Controller x360)
		{
			Util.Focus();

			for (int i = 0; i < 5; i++)
			{
				x360.PressButton(Xbox360Button.A);
				Thread.Sleep(Timing);
			}

			//TODO 
		}

		private static void SwitchToKbdMouse(IXbox360Controller x360)
		{
			Util.Focus();

			for (int i = 0; i < 5; i++)
			{
				x360.PressButton(Xbox360Button.A);
				Thread.Sleep(Timing);
			}

			x360.PressButton(Xbox360Button.Start);
			Thread.Sleep(Timing);

			x360.TapAxis(AxisDir.Left);
			Thread.Sleep(Timing);

			for (int i = 0; i < 4; i++)
			{
				x360.TapAxis(AxisDir.Down);
				Thread.Sleep(Timing);
			}

			x360.PressButton(Xbox360Button.B);
			Thread.Sleep(Timing);

			x360.TapAxis(AxisDir.Right);
			Thread.Sleep(Timing);

			x360.PressButton(Xbox360Button.B);
			Thread.Sleep(Timing);

			x360.TapAxis(AxisDir.Up);
			Thread.Sleep(Timing);

			x360.PressButton(Xbox360Button.B);
			Thread.Sleep(Timing);
		}

		private static bool IsMenuOpen()
		{
			var cmin = Color.FromArgb(73, 83, 102);
			var cmax = Color.FromArgb(80, 89, 107);

			var open = Capture.Game(MenuOpenScanRect);
			return open.MatchesAll(px => px.RgbIn(cmin, cmax));
		}

		// *****************************

		private static void CaptureArts(ArtifactsOptions opts, IXbox360Controller x360)
		{
			Util.Focus();

			//EnterMenuWithController(x360);

			int index = 0;
			bool hasMoved = true;

			var alignZero = new string('0', (int)Math.Floor(Math.Log10(Math.Min(opts.Max, 1000))) + 1);

			while (true)
			{
				if (index >= opts.Max)
					break;

				Util.WaitForFocus();

				if (hasMoved)
				{
					Thread.Sleep(100);
					if (!Util.GenshinHasFocus()) continue;

					using var cap = Capture.Game(ArtifactScanRect);
					hasMoved = false;
					Directory.CreateDirectory(ArtsFolder);
					var saveFile = Path.Combine(ArtsFolder, $"Art{index.ToString(alignZero)}.png");
					File.Delete(saveFile);
					cap.Save(saveFile, ImageFormat.Png);
				}

				if (!hasMoved)
				{
					if (!Util.GenshinHasFocus()) continue;

					x360.SetAxisValue(Xbox360Axis.LeftThumbX, short.MaxValue);
					hasMoved = true;
					index++;

					Thread.Sleep(100);
					x360.SetAxisValue(Xbox360Axis.LeftThumbX, 0);

					if (!Util.GenshinHasFocus()) continue;
				}
			}
		}

		static readonly Rectangle MenuOpenScanRect = new(140, 380, 5, 20);
		static readonly Rectangle ArtifactScanRect = new(1330, 133, 410, 815);
		private static readonly Rectangle[] Areas = new Rectangle[] {
			new(20, 5, 365, 35), // Name
			new(20, 56, 217, 25), // SubName
			new(20, 123, 196, 25), // MainStatType
			new(20, 149, 224, 36), // MainStatValue
			new(26, 258, 43, 19), // Level
			new(38, 299, 350, 118), // Substats (W:350 H:32)
			new(40, 294, 300, 32), // Substat-1
			new(40, 326, 300, 32), // Substat-2
			new(40, 358, 300, 32), // Substat-3
			new(40, 390, 300, 32), // Substat-4
			new(20, 299, 368, 151), // Set
			new(23, 198, 28 * 5, 22), // Stars (Star: 22x22 | Spaced: 28x22)
		};

		private static void AnalyzeAll(ArtifactsOptions opts)
		{
			if (opts.Debug)
				Directory.CreateDirectory(DbgFolder);

			//Analyze(opts, GetOcrInstance(), Path.Combine(ArtsFolder, $"Art{13}.png"));
			//Analyze(opts, GetOcrInstance(), Path.Combine(ArtsFolder, $"Art{0}.png"));

			var artList = new ConcurrentBag<ArtData>();

			//foreach (var file in Directory.EnumerateFiles(ArtsFolder).Take(opts.Max))
			var files = Directory.EnumerateFiles(ArtsFolder).Take(opts.Max).ToArray();
			Parallel.ForEach(
				files,
				() => GetOcrInstance(),
				(file, state, ocr) =>
				{
					var artData = Analyze(opts, ocr, file);
					if (artData != null)
						artList.Add(artData);
					return ocr;
				},
				(ocr) => { }
			);

			var collisionList = new Dictionary<string, int>();
			var genshOptDict = artList.Select(x =>
			{
				var id = GetHashString($"{x.Main}{string.Join(",", x.SubStats)}{x.Slot}{x.Stars}{x.Level}");
				var cnt = collisionList.GetValueOrDefault(id, 0);
				collisionList[id] = cnt + 1;

				x.Id = $"artifact_{id}_{cnt}";
				return x;
			}).ToDictionary(x => x.Id);
			var text = JsonSerializer.Serialize(genshOptDict, new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				Converters = {
					new JsonStringEnumConverter(),
					new GenshinOptimizerConverter(),
				}
			});
			File.WriteAllText("data.json", text);

			Console.WriteLine("Done. Press any key to close");
			Console.ReadKey();
		}

		private static IronTesseract GetOcrInstance()
		{
			var ocr = new IronTesseract();
			ocr.Configuration.ReadBarCodes = false;
			ocr.Configuration.RenderSearchablePdfsAndHocr = false;
			ocr.Language = OcrLanguage.English;
			return ocr;
		}

		private static ArtData? Analyze(ArtifactsOptions opts, IronTesseract Ocr, string file)
		{
			using var img = new Bitmap(file);

			T ProcessStat<T>(string dbgName, Rectangle area, Action<Bitmap, Graphics>? postprocess, Func<string, T> filter)
			{
				using var crop = img.CropOut(area, postprocess);
				if (opts.Debug) crop.Save(Path.Combine(DbgFolder, $"dbg_{dbgName}.png"), ImageFormat.Png);
				using var Input = new OcrInput(crop);
				var Result = Ocr.Read(Input);
				var text = Result.Text;
				var guess = filter(text);
				Console.WriteLine("For {0}: got '{1}', reading {2}", dbgName, text, guess);
				return guess;
			}

			using var starCrop = img.CropOut(Areas[11]);
			var stars = 0;
			for (int i = 0; i < 5; i++)
			{
				var checkPx = starCrop.GetPixel(28 * i + 22 / 2, 22 / 2);
				if (checkPx.R > 180 && checkPx.G > 180 && checkPx.B < 100)
				{
					stars = i + 1;
				}
				else
				{
					break;
				}
			}

			if (stars < opts.MinStars)
				return null;

			Ocr.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.SingleLine;

			Ocr.Configuration.WhiteListCharacters = (Numbers + "+").Allow();
			var levelNum = ProcessStat(
				"level",
				Areas[4],
				ImageExt.WhiteFilter,
				text => int.TryParse(text, out var num) ? num : -1
			);

			if (levelNum < opts.MinLevel)
				return null;

			Ocr.Configuration.WhiteListCharacters = SlotNames.Values.Allow();
			Slot slot = ProcessStat(
				"subName",
				Areas[1],
				ImageExt.WhiteFilter,
				text => SlotNames.FindClosest(text)
			);
			var slotData = StatCategory[slot];

			Ocr.Configuration.WhiteListCharacters = slotData.main.Select(s => StatNames[s]).Allow();
			Stat mainStat = ProcessStat(
				"mainStat",
				Areas[2],
				ImageExt.GrayFilter,
				text => slotData.main.Select(s => (s, StatNames[s])).FindClosest(text)
			);

			Ocr.Configuration.WhiteListCharacters = CharsNumbers.Allow();
			string mainStatValueText = ProcessStat(
				"mainStatValue",
				Areas[3],
				ImageExt.WhiteFilter,
				text => text
			);
			mainStat = ModStatWithPercent(mainStat, mainStatValueText, slotData.main);
			float mainStatValue = ParseNumber(mainStatValueText, mainStat);

			var subStats = new List<StatGroup>();
			var allowedSubstats = new List<Stat>(slotData.sub);
			allowedSubstats.Remove(mainStat);
			for (int i = 0; i < 4; i++)
			{
				// Check the enumeration dot if there is a substat at this pos
				var area = Areas[6 + i];
				area.X = 20;
				area.Width = 20;
				int pxlCount = 0;
				using var crop = img.CropOut(area, (i, g) => i.ForAll(px =>
				{
					if (px.R < 128 && px.G < 128 && px.B < 128)
						pxlCount++;
					return px;
				}));

				if (pxlCount < 2)
					break;

				Ocr.Configuration.WhiteListCharacters = allowedSubstats.Select(s => StatNames[s]).Concat(new[] { CharsNumbers }).Allow();
				var substat = ProcessStat(
					$"subStat{i}",
					Areas[6 + i],
					ImageExt.BlackFilter,
					text =>
					{
						if (text.Contains("+"))
						{
							var subParts = text.Split('+', 2);
							var statText = subParts[0];
							var statValueText = subParts[1];

							var stat = allowedSubstats.Select(s => (s, StatNames[s])).FindClosest(statText);
							stat = ModStatWithPercent(stat, statValueText, allowedSubstats);
							var statValue = ParseNumber(statValueText, stat);

							allowedSubstats.Remove(stat);
							return new StatGroup(stat, statValue, text);
						}
						else
						{
							Console.WriteLine("WARN: Unrecognized substat: {0}", text);
							return null;
						}
					}
				);

				if (substat != null)
					subStats.Add(substat);
			}

			Ocr.Configuration.WhiteListCharacters = SetNames.Values.Concat(new[] { CharsSet }).Allow();
			//var pos = Areas[10];
			//pos.Y -= (4 - subStats.Count) * 32;
			var artSet = ProcessStat(
				"artSet",
				Areas[10],
				ImageExt.GreenFilter,
				text => SetNames.FindClosest(text.TrimEnd(':'))
			);

			//string name = ProcessStat(
			//	"name",
			//	Areas[0],
			//	WhiteFilter,
			//	text => text
			//);

			return new ArtData()
			{
				FileName = file,
				Slot = slot,
				Stars = stars,
				Level = levelNum,
				ArtSet = artSet,
				Main = new StatGroup(mainStat, mainStatValue, mainStatValueText),
				SubStats = subStats,
			};
		}

		public static T FindClosest<T>(this IEnumerable<(T, string)> kvs, string result)
			=> kvs.Select(s => new KeyValuePair<T, string>(s.Item1, s.Item2)).FindClosest(result);
		public static T FindClosest<T>(this IEnumerable<KeyValuePair<T, string>> kvs, string result)
		{
			var bestDist = int.MaxValue;
			T best = default!;
			foreach (var kvp in kvs)
			{
				var dist = Lehvenshtein(kvp.Value, result);
				if (dist < bestDist)
				{
					bestDist = dist;
					best = kvp.Key;
					if (dist == 0)
						return best;
				}
			}
			return best;
		}

		static int Lehvenshtein(string s, string t)
		{
			if (s == t)
			{
				return 0;
			}

			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Verify arguments.
			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			// Initialize arrays.
			for (int i = 0; i <= n; d[i, 0] = i++) { }

			for (int j = 0; j <= m; d[0, j] = j++) { }

			// Begin looping.
			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					// Compute cost.
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
					d[i, j] = Math.Min(
						Math.Min(
							d[i - 1, j] + 1,
							d[i, j - 1] + 1
						),
						d[i - 1, j - 1] + cost
					);
				}
			}
			// Return cost.
			return d[n, m];
		}

		private static string Allow(this IEnumerable<string> enu) => enu.SelectMany(x => x).Allow();
		private static string Allow(this IEnumerable<char> enu) => string.Join("", enu.Distinct().OrderBy(x => x));

		public static Stat ModStatWithPercent(Stat orig, string value, IList<Stat> valid)
		{
			bool hasPerc = value.Contains('%');
			return orig switch
			{
				Stat.Atk => hasPerc && valid.Contains(Stat.AtkPerc) ? Stat.AtkPerc : Stat.Atk,
				Stat.Hp => hasPerc && valid.Contains(Stat.HpPerc) ? Stat.HpPerc : Stat.Hp,
				Stat.Def => hasPerc && valid.Contains(Stat.DefPerc) ? Stat.DefPerc : Stat.Def,
				var other => other,
			};
		}

		public static float ParseNumber(string value, Stat stat)
		{
			value = value.Replace("%", "").Replace("+", "").Trim('.', ' ');

			switch (stat)
			{
			// trim ','
			case Stat.Atk:
			case Stat.Hp:
			case Stat.Def:
			case Stat.ElementalMastery:
				value = value.Replace(",", "").Replace(".", "");
				if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
					num = -1;
				return num;

			case Stat.AtkPerc: // 40.0%
			case Stat.HpPerc: // 7.0%
			case Stat.DefPerc: // 7.8%
			case Stat.EnergyRecharge:
			case Stat.CritDmg:
			case Stat.CritRate:
			case Stat.PyroDmgBonus:
			case Stat.HydroDmgBonus:
			case Stat.ElectroDmgBonus:
			case Stat.AnemoDmgBonus:
			case Stat.CryoDmgBonus:
			case Stat.GeoDmgBonus:
			case Stat.PhysicalDmgBonus:
			case Stat.HealingBonusPerc:
				value = value.Replace(",", ".");
				if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fnum))
					fnum = -1;
				return fnum;

			default:
				throw new ArgumentOutOfRangeException(nameof(stat));
			}
		}

		public static byte[] GetHash(string inputString)
		{
			using var algorithm = MD5.Create();
			return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		public static string GetHashString(string inputString)
		{
			var sb = new StringBuilder();
			foreach (byte b in GetHash(inputString))
				sb.Append(b.ToString("X2"));

			return sb.ToString();
		}
	}

	class GenshinOptimizerConverter : JsonConverter<ArtData>
	{
		public override ArtData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, ArtData val, JsonSerializerOptions options)
		{
			static string StatToGenshOpt(Stat stat) => stat switch
			{
				Stat.Atk => "atk",
				Stat.AtkPerc => "atk_",
				Stat.Hp => "hp",
				Stat.HpPerc => "hp_",
				Stat.Def => "def",
				Stat.DefPerc => "def_",
				Stat.EnergyRecharge => "enerRech_",
				Stat.ElementalMastery => "eleMas",
				Stat.CritDmg => "critDMG_",
				Stat.CritRate => "critRate_",
				Stat.PyroDmgBonus => "pyro_dmg_",
				Stat.HydroDmgBonus => "hydro_dmg_",
				Stat.ElectroDmgBonus => "electro_dmg_",
				Stat.AnemoDmgBonus => "anemo_dmg_",
				Stat.CryoDmgBonus => "cryo_dmg_",
				Stat.GeoDmgBonus => "geo_dmg_",
				Stat.PhysicalDmgBonus => "physical_dmg_",
				Stat.HealingBonusPerc => "heal_",
				_ => throw new ArgumentOutOfRangeException(nameof(stat))
			};

			writer.WriteStartObject();
			writer.WriteString("id", val.Id);
			writer.WriteString("_file", val.FileName);
			writer.WriteString("setKey", val.ArtSet.ToString());
			writer.WriteNumber("numStars", val.Stars);
			writer.WriteString("slotKey", val.Slot.ToString().ToLowerInvariant());
			writer.WriteNumber("level", val.Level);
			writer.WriteString("mainStatKey", StatToGenshOpt(val.Main.Type));

			writer.WritePropertyName("substats");
			writer.WriteStartArray();

			foreach (var sub in val.SubStats)
			{
				writer.WriteStartObject();
				writer.WriteString("key", StatToGenshOpt(sub.Type));
				writer.WriteNumber("value", sub.Value);
				writer.WriteEndObject();
			}

			writer.WriteEndArray();
			writer.WriteEndObject();
		}
	}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	class ArtData
	{
		public string FileName { get; set; }
		public string Id { get; set; }

		public Slot Slot { get; set; }

		public int Stars { get; set; }
		public int Level { get; set; }

		public ArtSet ArtSet { get; set; }

		public StatGroup Main { get; set; }
		public List<StatGroup> SubStats { get; set; }
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	record StatGroup(Stat Type, float Value, string Raw)
	{
		public override string ToString() => $"{Type}:{Value}";
	}

	enum Slot
	{
		Flower,
		Plume,
		Sands,
		Goblet,
		Circlet
	}

	enum Stat
	{
		Atk,
		AtkPerc,
		Hp,
		HpPerc,
		Def,
		DefPerc,
		EnergyRecharge,
		ElementalMastery,
		CritDmg,
		CritRate,

		PyroDmgBonus,
		HydroDmgBonus,
		ElectroDmgBonus,
		AnemoDmgBonus,
		CryoDmgBonus,
		GeoDmgBonus,
		PhysicalDmgBonus,
		HealingBonusPerc,
	}

	enum ArtSet
	{
		GladiatorsFinale,
		WanderersTroupe,
		ViridescentVenerer,
		ThunderingFury,
		Thundersoother,
		CrimsonWitchOfFlames,
		Lavawalker,
		ArchaicPetra,
		RetracingBolide,
		MaidenBeloved,
		NoblesseOblige,
		BloodstainedChivalry,
		BlizzardStrayer,
		HeartOfDepth,
		TenacityOfTheMillelith,
		PaleFlame,
		EmblemOfSeveredFate,
		ShimenawasReminiscence,
		Instructor,
		TheExile,
		ResolutionOfSojourner,
		MartialArtist,
		DefendersWill,
		TinyMiracle,
		BraveHeart,
		Gambler,
		Scholar,
		PrayersForWisdom,
		PrayersToSpringtime,
		PrayersForIllumination,
		PrayersForDestiny,
		Berserker,
		LuckyDog,
		TravelingDoctor,
		Adventurer,
		Initiate,
	}
}
