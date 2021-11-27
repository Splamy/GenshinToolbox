using IronOcr;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System.Collections.Concurrent;
using System.Diagnostics;
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

namespace GenshinToolbox.ArtScraper;

static partial class Scraper
{
	const string Numbers = "0123456789";
	const string AdditionalCharacter = ".,+%";

	const string CharsNumbers = Numbers + AdditionalCharacter;
	const string CharsSet = ":";

	const string DbgFolder = "DbgImgs";
	static readonly string ArtsFolder = Path.Join("ArtImgs");
	const int MaxArtifacts = 1500;

	public static void Run(ArtifactsOptions opts)
	{
		StaticInit();

		using var c = new ViGEmClient();
		var x360 = c.CreateXbox360Controller();
		bool connected = false;
		void ConnectController()
		{
			if (!connected)
			{
				x360.Connect();
				connected = true;
			}
		}

		while (true)
		{
			Console.Clear();
			Console.WriteLine("1. Scan all artifacts (make sure game is in controller mode)");
			Console.WriteLine("2. Analyze all artifacts");
			Console.WriteLine("3. Switch game to controller input");
			Console.WriteLine("4. Switch game to kdb+mouse input");
			Console.WriteLine("5. Navigate to artifacts (make sure game is in controller mode)");
			Console.WriteLine("6. Only connect controller");

			switch (Console.ReadKey().KeyChar)
			{
			case '1':
				ConnectController();
				CaptureArts(x360);
				break;
			case '2':
				Console.Write("Min level (Default: >=1):");
				if (!int.TryParse(Console.ReadLine()!.Trim(), out var minLevel))
					minLevel = 1;
				Console.Write("Min stars (Default: >=4):");
				if (!int.TryParse(Console.ReadLine()!.Trim(), out var minStars))
					minStars = 4;
				Console.Write("Count (Default: All):");
				if (!int.TryParse(Console.ReadLine()!.Trim(), out var amount))
					amount = Directory.EnumerateFiles(ArtsFolder).Count();
				Console.Write("Keep at least 1 unique (Default: false) [Y/_]:");
				var keepUnique = Console.ReadKey().Key == ConsoleKey.Y;
				Console.WriteLine("");
				Console.WriteLine("Starting...");

				opts.MinLevel = minLevel;
				opts.MinStars = minStars;
				opts.Max = amount;
				opts.KeepUnique = keepUnique;
				AnalyzeAll(opts);
				break;
			case '3':
				ConnectController();
				SwitchToController();
				break;
			case '4':
				ConnectController();
				SwitchToKbdMouse(x360);
				break;
			case '5':
				ConnectController();
				NavToArtifacts(x360);
				break;
			case '6':
				ConnectController();
				break;
			}
		}
	}

	private static void StaticInit() {
		Trace.Assert(SetNames.Count > 0);
		Trace.Assert(SlotNames.Count > 0);
		Trace.Assert(StatCategory.Count > 0);
		Trace.Assert(StatNames.Count > 0);
		using var bmp = new Bitmap(4,4);
		using var ocri = new OcrInput(bmp);
	}

	// Kbd / Controller switching utils

	const int Timing = 1000;
	const int FastTiming = 200;

	private static void SwitchToController()
	{
		Util.Focus();

		for (int i = 0; i < 4; i++)
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

	private static void SwitchToKbdMouse(IXbox360Controller x360)
	{
		Util.Focus();

		ControllerExitOut(x360);

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

	private static void ControllerExitOut(IXbox360Controller x360)
	{
		for (int i = 0; i < 4; i++)
		{
			x360.PressButton(Xbox360Button.A);
			Thread.Sleep(Timing);
		}
	}

	private static bool IsMenuOpen()
	{
		var cmin = Color.FromArgb(73, 83, 102);
		var cmax = Color.FromArgb(80, 89, 107);

		var open = Capture.Game(MenuOpenScanRect);
		return open.MatchesAll(px => px.RgbIn(cmin, cmax));
	}

	private static void NavToArtifacts(IXbox360Controller x360)
	{
		Util.Focus();

		ControllerExitOut(x360);

		x360.SetButtonState(Xbox360Button.LeftShoulder, true);
		Thread.Sleep(FastTiming);
		x360.SetAxisValue(Xbox360Axis.RightThumbX, short.MaxValue);
		Thread.Sleep(FastTiming);
		x360.SetButtonState(Xbox360Button.LeftShoulder, false);
		x360.SetAxisValue(Xbox360Axis.RightThumbX, 0);
		Thread.Sleep(FastTiming);

		Console.WriteLine("Press [Left] or [Right] {n} times to move though {n} tabs. [Enter] to run.");

		int move = 0; // -1 = left | +1 = right
		bool done = false;
		var (_, top) = Console.GetCursorPosition();
		while (!done)
		{
			switch (Console.ReadKey().Key)
			{
			case ConsoleKey.LeftArrow:
				move--;
				break;

			case ConsoleKey.RightArrow:
				move++;
				break;

			case ConsoleKey.Enter:
				done = true;
				break;
			}

			Console.SetCursorPosition(0, top);
			Console.WriteLine("Moving {0}{1}", move, move switch { 0 => "           ", > 0 => " to right", < 0 => " to left" });
		}

		Util.Focus();
		Thread.Sleep(FastTiming);

		for (int i = 0; i < Math.Abs(move); i++)
		{
			var btn = move > 0 ? Xbox360Button.RightShoulder : Xbox360Button.LeftShoulder;
			x360.SetButtonState(btn, true);
			Thread.Sleep(FastTiming);
			x360.SetButtonState(btn, false);
			Thread.Sleep(FastTiming);
		}
	}

	// *****************************

	private static void CaptureArts(IXbox360Controller x360)
	{
		try { Directory.Delete(ArtsFolder, true); } catch { }
		Directory.CreateDirectory(ArtsFolder);

		Util.Focus();

		//EnterMenuWithController(x360);

		int index = 0;
		bool hasMoved = true;
		FastBitmap? lastScan = null;

		while (index <= MaxArtifacts)
		{
			Util.WaitForFocus();

			if (hasMoved)
			{
				Thread.Sleep(100);
				if (!Util.GenshinHasFocus()) continue;

				var cap = Capture.Game(ArtifactScanRect, Capture.R1080p);
				var saveFile = Path.Combine(ArtsFolder, $"Art{index:0000}.png");
				try { File.Delete(saveFile); } catch { }
				cap.Save(saveFile, ImageFormat.Png);
				hasMoved = false;

				var currentScan = new FastBitmap(cap);
				if (lastScan != null)
				{
					bool equals = true;
					for (int i = 0; i < currentScan.Height; i++)
					{
						if (!currentScan.GetRow(i).SequenceEqual(lastScan.GetRow(i)))
						{
							equals = false;
							break;
						}
					}

					if (equals)
					{
						lastScan.DisposeWithBithmap();
						currentScan.DisposeWithBithmap();
						break;
					}
				}
				lastScan?.DisposeWithBithmap();
				lastScan = currentScan;
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

		Console.WriteLine("Done");
		Thread.Sleep(1000);
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

		//Analyze(opts, Ocr.NewInstance(), Path.Combine(ArtsFolder, $"Art{93:000}.png"));
		//Analyze(opts, GetOcrInstance(), Path.Combine(ArtsFolder, $"Art{0}.png"));

		var artList = new ConcurrentBag<ArtData>();

		//foreach (var file in Directory.EnumerateFiles(ArtsFolder).Take(opts.Max))
		var files = Directory.EnumerateFiles(ArtsFolder).Take(opts.Max).ToArray();
		Parallel.ForEach(
			files,
			new ParallelOptions() { MaxDegreeOfParallelism = opts.Debug ? 1 : -1 },
			() => Ocr.NewInstance(),
			(file, _, ocr) =>
			{
				var artData = Analyze(opts, ocr, file);
				if (artData != null)
					artList.Add(artData);
				return ocr;
			},
			(_) => { }
		);

		var uniqueList = new HashSet<(Slot slot, ArtSet set, Stat main)>();
		var filteredArts = new List<ArtData>();
		var collisionList = new Dictionary<string, int>();
		foreach (var art in artList.OrderByDescending(x => x.Level).ThenByDescending(x => x.Stars))
		{
			// Add Artifact to result if either it fits the filter, or is unique.
			var artGroup = (art.Slot, art.ArtSet, art.Main.Type);
			if ((art.Stars < opts.MinStars || art.Level < opts.MinLevel) && uniqueList.Contains(artGroup))
				continue;
			uniqueList.Add(artGroup);

			// Add a unique string to each artifact (Probably not necessary anymore but good for finding arts simpler)
			var id = GetHashString($"{art.Main}{string.Join(",", art.SubStats)}{art.Slot}{art.Stars}{art.Level}");
			var cnt = collisionList.GetValueOrDefault(id, 0);
			collisionList[id] = cnt + 1;
			art.Id = $"artifact_{id}_{cnt}";

			filteredArts.Add(art);
		}

		var finalJsonArr = filteredArts.OrderBy(art => art.FileName).ToArray();
		var goodData = new GoodData()
		{
			format = "GOOD",
			dbVersion = 13,
			source = "GenshinToolbox",
			version = 1,
			artifacts = finalJsonArr,
		};
		var text = JsonSerializer.Serialize(goodData, new JsonSerializerOptions
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

	private static ArtData? Analyze(ArtifactsOptions opts, IronTesseract ocr, string file)
	{
		using var img = new Bitmap(file).ToSharedPixelFormat();

		T ProcessStat<T>(string dbgName, Rectangle area, PxFunc? postprocess, Func<string, T> filter)
		{
			using var crop = img.CropOut(area);
			if (postprocess != null) crop.ApplyFilter(postprocess);
			if (opts.Debug) crop.Save(Path.Combine(DbgFolder, $"dbg_{dbgName}.png"), ImageFormat.Png);
			using var ocrCrop = crop.ToOcrPixelFormat();
			using var Input = Ocr.CreateSafe(ocrCrop);
			ocr.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.SingleLine;
			var Result = ocr.Read(Input);
			//ocr.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.SingleBlock;
			//var Result2 = ocr.Read(Input);
			//ocr.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.RawLine; // clunky
			//var Result3 = ocr.Read(Input);
			//ocr.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.SparseText;
			//var Result4 = ocr.Read(Input);
			var text = Result.Text;
			var guess = filter(text);
			Console.WriteLine("For {0}: got '{1}', reading {2}", dbgName, text, guess);
			return guess;
		}

		using var starCrop = img.CropOut(Areas[11]);
		var stars = 0;
		for (int i = 0; i < 5; i++)
		{
			var checkPx = starCrop.GetPixel((28 * i) + (22 / 2), 22 / 2);
			if (checkPx.R > 180 && checkPx.G > 180 && checkPx.B < 100)
			{
				stars = i + 1;
			}
			else
			{
				break;
			}
		}

		if (stars < opts.MinStars && !opts.KeepUnique)
			return null;

		ocr.Configuration.WhiteListCharacters = (Numbers + "+").Allow();
		var levelNum = ProcessStat(
			"level",
			Areas[4],
			ImageExt.WhiteScaleFilter,
			text => int.TryParse(text, out var num) ? num : -1
		);

		if (levelNum < opts.MinLevel && !opts.KeepUnique)
			return null;

		ocr.Configuration.WhiteListCharacters = SlotNames.Values.Allow();
		Slot slot = ProcessStat(
			"subName",
			Areas[1],
			ImageExt.WhiteFilter,
			text => SlotNames.FindClosest(text)
		);
		var slotData = StatCategory[slot];

		ocr.Configuration.WhiteListCharacters = slotData.Main.Select(s => StatNames[s]).Allow();
		Stat mainStat = ProcessStat(
			"mainStat",
			Areas[2],
			ImageExt.GrayFilter,
			text => slotData.Main.Select(s => (s, StatNames[s])).FindClosest(text)
		);

		ocr.Configuration.WhiteListCharacters = CharsNumbers.Allow();
		string mainStatValueText = ProcessStat(
			"mainStatValue",
			Areas[3],
			ImageExt.WhiteFilter,
			text => text
		);
		mainStat = ModStatWithPercent(mainStat, mainStatValueText, slotData.Main);
		float mainStatValue = ParseNumber(mainStatValueText, mainStat);

		var subStats = new List<StatGroup>();
		var allowedSubstats = new List<Stat>(slotData.Sub);
		allowedSubstats.Remove(mainStat);
		for (int i = 0; i < 4; i++)
		{
			// Check the enumeration dot if there is a substat at this pos
			var area = Areas[6 + i];
			area.X = 20;
			area.Width = 20;
			int pxlCount = 0;
			img.ApplyFilter((ref Bgrx32 px) =>
			{
				if (px.R < 128 && px.G < 128 && px.B < 128)
					pxlCount++;
			}, area);

			if (pxlCount < 2)
				break;

			ocr.Configuration.WhiteListCharacters = allowedSubstats.Select(s => StatNames[s]).Concat(new[] { CharsNumbers }).Allow();
			var substat = ProcessStat(
				$"subStat{i}",
				Areas[6 + i],
				ImageExt.BlackScaleFilter,
				text =>
				{
					if (text.Contains('+'))
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

		ocr.Configuration.WhiteListCharacters = SetNames.Values.Concat(new[] { CharsSet }).Allow();
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

	public static Stat ModStatWithPercent(Stat orig, string value, IReadOnlyList<Stat> valid)
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
		writer.WriteNumber("rarity", val.Stars);
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

#pragma warning disable IDE1006 // Naming Styles
class GoodData
{
	public string format { get; set; }
	public int dbVersion { get; set; }
	public string source { get; set; }
	public int version { get; set; }

	public ArtData[]? artifacts { get; set; }
}
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

record StatGroup(Stat Type, float Value, string Raw)
{
	public override string ToString() => $"{Type}:{Value}";
}
