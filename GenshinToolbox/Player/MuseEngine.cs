using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using WindowsInput.Native;
using static GenshinToolbox.Player.NoteLength;
using static GenshinToolbox.Player.Scale;

namespace GenshinToolbox.Player
{
	public class MuseEngine
	{
		private static readonly VirtualKeyCode[][] Keys = {
			new [] { VirtualKeyCode.VK_Z, VirtualKeyCode.VK_X, VirtualKeyCode.VK_C, VirtualKeyCode.VK_V, VirtualKeyCode.VK_B, VirtualKeyCode.VK_N, VirtualKeyCode.VK_M, },
			new [] { VirtualKeyCode.VK_A, VirtualKeyCode.VK_S, VirtualKeyCode.VK_D, VirtualKeyCode.VK_F, VirtualKeyCode.VK_G, VirtualKeyCode.VK_H, VirtualKeyCode.VK_J, },
			new [] { VirtualKeyCode.VK_Q, VirtualKeyCode.VK_W, VirtualKeyCode.VK_E, VirtualKeyCode.VK_R, VirtualKeyCode.VK_T, VirtualKeyCode.VK_Y, VirtualKeyCode.VK_U, },
		};

		private const string SongPath = "../../../Songs/";

		private List<MXMlConf> Confs { get; }

		public static void Validate()
		{
			Trace.Assert(Shift(new Note(C, 1), 2) == new Note(E, 1));
			Trace.Assert(Shift(new Note(A, 0), 2) == new Note(C, 1));
			Trace.Assert(Shift(new Note(C, 1), 8) == new Note(D, 2));
			Trace.Assert(Shift(new Note(C, 1), -1) == new Note(H, 0));
			Trace.Assert(Shift(new Note(C, 2), -8) == new Note(H, 0));
			Trace.Assert(Shift(new Note(C, 2), -7) == new Note(C, 1));
		}

		public MuseEngine()
		{
			Confs = InitConfs();
		}

		public static List<MXMlConf> InitConfs()
		{
			return new List<MXMlConf>
			{
				new MXMlConf {
					Path = SongPath + "Sakkijarven_polkka.mxl" ,
					Link = "https://musescore.com/user/9192181/scores/2032511",
					Speed = 240,
					HonorRepeats = true,
					Autoscale = new()
					{
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.Auto) }
					},
				},
				new MXMlConf {
					Path = SongPath + "_Beethoven_Virus_.mxl" ,
					Link = "https://musescore.com/user/70981/scores/1086966",
					Speed = 150,
					HonorRepeats = false,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.Auto) },
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.FromLowerUp) }
					},
				},
				new MXMlConf {
					Path = SongPath + "La_Valse_d'Amélie_(original_version)_Yann_Tiersen.mxl" ,
					Link = "https://musescore.com/user/1417101/scores/960611",
					Speed = 360,
					HonorRepeats = false,
					Autoscale = new()
					{
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.Auto) }
					},
				},
				new MXMlConf {
					Path = SongPath + "Never_Gonna_Give_You_Up_-_Melodica_Duet.mxl" ,
					Link = "https://musescore.com/rhythmicrevival/never_gonna_give_you_up_for_two_melodicas",
					Speed = 120,
					HonorRepeats = false,
					Transpose = Transpose.F2,
				},
				new MXMlConf {
					Path = SongPath + "Tetris_Theme_(Korobeniki)_Easy_to_play.mxl" ,
					Link = "https://musescore.com/user/28837378/scores/5144713",
					Speed = 80,
					HonorRepeats = false,
				},
				new MXMlConf {
					Path = SongPath + "A_Cruel_Angel's_Thesis_-_Neon_Genesis_Evangelion.mxl" ,
					Link = "https://musescore.com/radical_edward/scores/3660686",
					Speed = 120,
					HonorRepeats = false,
					Transpose = Transpose.F3,
					Autoscale = new()
					{
						{ "P1_2", new(ScaleFunction.None, ScaleDirection.Auto, 0) }
					},
				},
				new MXMlConf {
					Path = SongPath + "Song_of_Storms_Piano_and_Ocarina.mxl" ,
					Link = "https://musescore.com/user/18080646/scores/3993531",
					Speed = 100,
					HonorRepeats = false,
					Transpose = Transpose.F1,
					PickInstruments = new()
					{
						"Oc.",
					},
					Autoscale = new()
					{
						// { "P1_1", new(ScaleFunction.Clamp, ScaleDirection.FromLowerUp, 1, 1) },
						// { "P2_1", new(ScaleFunction.Clamp, ScaleDirection.FromLowerUp, 0, 1) },
						// { "P2_5", new(ScaleFunction.Clamp, ScaleDirection.FromLowerUp, 0, 1) },
					}
				},
				new MXMlConf {
					Path = SongPath + "Nothing_Else_Matters_(by_Metallica).mxl" ,
					Link = "https://musescore.com/user/14429466/scores/3073586",
					Speed = 100,
					HonorRepeats = false,
					Transpose = Transpose.S1,
					MeasureStart = 8,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.Auto, 1, 1) },
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.Auto, 0, 1) },
					}
				},
				new MXMlConf {
					Path = SongPath + "Hallelujah.mxl" ,
					Link = "https://musescore.com/user/21965011/scores/4217351",
					Speed = 50,
					HonorRepeats = false,
					Transpose = Transpose.None,
					MeasureEnd = 30,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.FromUpperDown) },
					}
				},
				new MXMlConf {
					Path = SongPath + "Gigi_d'Agostino_-_L'amour_toujours_(I'll_fly_with_you).mxl" ,
					Link = "https://musescore.com/user/252946/scores/235796",
					Speed = 80,
					HonorRepeats = false,
					Transpose = Transpose.S2,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.Auto) },
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.Auto, 0) },
					}
				},
				new MXMlConf {
					Path = SongPath + "Soon_May_The_Wellerman_Come.mxl" ,
					Link = "https://musescore.com/user/27176639/scores/6612455",
					Speed = 220,
					HonorRepeats = false,
					Transpose = Transpose.F3,
					MeasureEnd = 17,
					Autoscale = new()
					{
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.FromLowerUp, 0, 1) },
					}
				},
				new MXMlConf {
					Path = SongPath + "Unravel_[by_Animenz]_-_Tokyo_Ghoul.mxl" ,
					Link = "https://musescore.com/sh_yy/unravel-animenz-toyko-ghoul",
					Name = "[Meh] Tokyo Ghoul - Unravel",
					Speed = 400,
					Transpose = Transpose.F2,
					MeasureStart = 0,
					MeasureEnd = 35,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.FromLowerUp, 1, 1) },
						{ "P1_2", new(ScaleFunction.Clamp, ScaleDirection.Auto, 1, 1) },
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.FromUpperDown, 0, 0) },
						{ "P1_6", new(ScaleFunction.Clamp, ScaleDirection.Auto, 0, 0) },
					}
				},
				new MXMlConf {
					Path = SongPath + "Caramelldansen.mxl" ,
					Link = "https://musescore.com/user/12700621/scores/5212860",
					Speed = 160,
					Transpose = Transpose.S1,
					MeasureEnd = 52,
					ForwardAsRepeat = true,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.Auto, 1) },
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.Auto, 0, 0) },
					}
				},
				new MXMlConf {
					Path = SongPath + "Pirates_of_the_Caribbean_-_He's_a_Pirate-Piano.mxl" ,
					Link = "https://musescore.com/user/2830596/scores/1421196",
					TargetLength = new TimeSpan(0, 1, 23),
					Transpose = Transpose.F1,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.Auto, 0) },
						//{ "P1_2", ScaleConf.Disabled },
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.Auto, 0, 0) },
					}
				},
				new MXMlConf {
					Path = SongPath + "Frog's_Theme_from_Chrono_Trigger.mxl" ,
					Link = "https://musescore.com/namvet/scores/50457",
					TargetLength = new TimeSpan(0, 0, 45),
					MeasureEnd = 25,
					Transpose = Transpose.F7,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Drop, ScaleDirection.Auto, 1) },
						{ "P1_5", new(ScaleFunction.Drop, ScaleDirection.Auto, 0, 0) },
					}
				}
			};
		}

		public static Voice Merge(IEnumerable<Voice> voices)
		{
			var evList = new Dictionary<int, HashSet<Note>>();

			foreach (var voice in voices)
			{
				int curTime = 0;
				foreach (var acc in voice.Accords)
				{
					if (acc.Notes.Count > 0)
					{
						if (evList.TryGetValue(curTime, out var builder))
						{
							builder.UnionWith(acc.Notes);
						}
						else
						{
							evList.Add(curTime, new HashSet<Note>(acc.Notes));
						}
					}
					curTime += acc.Length;
				}
			}

			var merged = new List<Accord>();
			KeyValuePair<int, HashSet<Note>>? previous = null;
			foreach (var accKvp in evList.OrderBy(x => x.Key))
			{
				if (previous != null)
				{
					var wait = accKvp.Key - previous.Value.Key;
					merged.Add(new Accord(wait, previous.Value.Value.ToArray()));
				}

				previous = accKvp;
			}
			if (previous != null)
			{
				merged.Add(new Accord(0, previous.Value.Value.ToArray()));
			}
			return new Voice(merged);
		}

		public static Note Shift(Note note, int dir)
		{
			var newOct = note.Oct;
			var newS = note.S;

			newOct += dir / 7;
			dir %= 7;

			newS += dir;
			if (newS >= 0)
				newOct += (int)(newS) / 7;
			else
				newOct += (int)(newS - 6) / 7;
			newS = (Scale)((int)(newS + 7) % 7);

			return new Note(newS, newOct);
		}

		public static Voice Shift(Voice part, int dir)
		{
			var shiftedList = new List<Accord>();
			foreach (var p in part.Accords)
			{
				shiftedList.Add(new Accord(p.Length, p.Notes.ConvertAll(n => Shift(n, dir))));
			}
			return new Voice(shiftedList);
		}

		public static Voice Pause(int len) => new(new List<Accord> { new Accord(len) });

		public void InteractiveConsole()
		{
			var list = Confs.ConvertAll(MXMLParser.Parse);
			InteractiveConsoleLoop(list);
		}

		public void InteractiveConsoleLoop(List<Song> list)
		{
			bool clear = false;

			while (true)
			{
				if (clear)
					Console.Clear();
				else
					clear = true;

				for (int i = 0; i < list.Count; i++)
				{
					var song = list[i];
					var len = song.RealLength;
					Console.WriteLine("{0,4}: [{1:mm\\:ss}] {2}", i, len, song.Name);
				}

				var rl = Console.ReadLine();
				if (rl is null) break;
				if (string.IsNullOrWhiteSpace(rl)) continue;

				// Shortcut for playing a song by just entering the number
				if (int.TryParse(rl, out var num))
				{
					if (num < 0 || num >= list.Count) continue;
					var song = list[num];
					Console.WriteLine("Playing: {0}", song.Name);
					Play(song);
				}
				else
				{
					var parts = rl.Split(' ', StringSplitOptions.TrimEntries);
					try
					{
						const string DefaultDumpPath = "dump/";
						switch (parts[0].ToLowerInvariant())
						{
							case "dump":
								{
									var prefix = parts.Length > 1 ? parts[1] : DefaultDumpPath;
									foreach (var song in list)
									{
										var merged = song.Merge();
										var strDump = string.Join("\n", merged.Accords);

										var dumpFile = $"./{prefix}{song.Name}.txt";
										Directory.CreateDirectory(Path.GetDirectoryName(dumpFile)!);
										File.WriteAllText(dumpFile, strDump);
									}
									break;
								}

							case "validate":
								{
									var prefix = parts.Length > 1 ? parts[1] : DefaultDumpPath;
									foreach (var song in list)
									{
										var dumpFile = $"./{prefix}{song.Name}.txt";
										if (!File.Exists(dumpFile)) { Console.WriteLine("Skipping (No compare file) {0}", song.Name); continue; }

										var merged = song.Merge();
										var strDump = string.Join("\n", merged.Accords);

										var strCmp = File.ReadAllText(dumpFile);

										if (strDump == strCmp)
											Console.WriteLine("Ok: {0}", song.Name);
										else
											Console.WriteLine("!!! Song differs: {0}", song.Name);
									}

									clear = false;
									break;
								}

							case "pick":
								{
									var song = list[int.Parse(parts[1])];

									for (int i = 0; i < song.Voices.Count; i++)
									{
										var voice = song.Voices[i];
										Console.WriteLine("{0,4}: {1}", i, voice.Name);
									}

									var voiceI = Console.ReadLine();
									if (string.IsNullOrWhiteSpace(voiceI)) break;
									var voicePick = song.Voices[int.Parse(voiceI)];

									Play(new Song(song.Conf, song.Name, voicePick));

									break;
								}

							case "parse":
								{
									var conf = Confs[int.Parse(parts[1])];
									MXMLParser.Parse(conf);
									break;
								}

							default:
								Console.WriteLine("Unknown command");
								clear = false;
								break;
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Don't type stupid stuff man: {0}", ex.Message);
						clear = false;
					}
				}
			}
		}

		public static void Play(Song song)
		{
			var merged = song.Merge();
			var length = song.RealLength;
			var played = TimeSpan.Zero;
			var drawLine = Console.CursorTop;

			Util.Focus();
			var timer = Stopwatch.StartNew();
			foreach (var acc in merged.Accords)
			{
				while (timer.Elapsed < played)
					Thread.SpinWait(10_000);

				if (!Util.GenshinHasFocus()) break;
				PlayNote(acc.Notes);
				var noteLength = song.TickLength * acc.Length;
				played += noteLength;
				//Thread.Sleep(noteLength);

				Console.SetCursorPosition(0, drawLine);
				var playFill = (int)(Console.BufferWidth * (played / length));
				Console.Write("{0}{1}", new string('|', playFill), new string('.', Console.BufferWidth - playFill));
			}
		}

		public static void PlayNote(IEnumerable<Note> note)
		{
			var vks = note.Select(n => Keys[n.Oct][(int)n.S]).ToArray();
			if (vks.Length == 0) return;
			Util.inp.Keyboard.KeyPress(vks);
		}
	}

	public class Song
	{
		public string Name { get; }
		public List<Voice> Voices { get; }
		public MXMlConf Conf { get; }

		public int TickCount { get; private set; }
		public TimeSpan RealLength { get; private set; }
		public TimeSpan TickLength { get; private set; }

		public Song(MXMlConf conf, string name, List<Voice> voices)
		{
			Conf = conf;
			Name = name;
			Voices = voices;

			RecalcTimes();
		}

		public Song(MXMlConf conf, string name, params Voice[] voices) : this(conf, name, voices.ToList()) { }

		private void RecalcTimes()
		{
			TickCount = Voices.Max(v => v.Accords.Sum(a => a.Length));
			if (Conf.Speed.HasValue)
			{
				TickLength = TimeSpan.FromMinutes(1 / (LengthQuarter * Conf.Speed.Value));
				RealLength = TickLength * TickCount;
			}
			else if (Conf.TargetLength.HasValue)
			{
				RealLength = Conf.TargetLength.Value;
				TickLength = RealLength / TickCount;
			}
			else
			{
				throw new InvalidOperationException("Missing speed");
			}
		}

		public Voice Merge()
		{
			return MuseEngine.Merge(Voices);
		}
	}

	public record Note(Scale S, int Oct)
	{
		public override string ToString() => $"{S}{Oct}";
	}

	public record Accord(int Length, List<Note> Notes)
	{
		public Accord(int Length, params Note[] Notes) : this(Length, Notes.ToList()) { }
		//public Accord(int length, List<Note> notes) { (Length, Notes) = (length, notes); }
		public override string ToString() => $"<{string.Join(":", Notes)}|{Length}>";
	}

	public record Voice(List<Accord> Accords)
	{
		public string Name { get; set; } = "<?>";

		public static Voice operator +(Voice a, Voice b)
		{
			return new Voice(a.Accords.Concat(b.Accords).ToList());
		}
		public override string ToString() => $"{string.Join(", ", Accords)}";
	}

	public enum Scale
	{
		C,
		D,
		E,
		F,
		G,
		A,
		H,
		B = H,
	}

	public static class NoteLength
	{
		public const int LengthQuarter = 128 / 4;

		public const int Full = 128;
		public const int HalfN = Full / 2;
		public const int HalfDot = HalfN + HalfN / 2;
		public const int Quart = Full / 4;
		public const int QuartDot = Quart + Quart / 2;
		public const int Eigth = Full / 8;
		public const int EigthDot = Eigth + Eigth / 2;
	}
}
