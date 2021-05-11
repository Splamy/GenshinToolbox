using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		public static void Validate()
		{
			Trace.Assert(Shift(new Note(C, 1), 2) == new Note(E, 1));
			Trace.Assert(Shift(new Note(A, 0), 2) == new Note(C, 1));
			Trace.Assert(Shift(new Note(C, 1), 8) == new Note(D, 2));
			Trace.Assert(Shift(new Note(C, 1), -1) == new Note(H, 0));
			Trace.Assert(Shift(new Note(C, 2), -8) == new Note(H, 0));
			Trace.Assert(Shift(new Note(C, 2), -7) == new Note(C, 1));
		}

		public static void RunMusic()
		{
			var list = new List<Song>
			{
				// https://musescore.com/user/9192181/scores/2032511
				MXMLParser.Parse(SongPath + "Sakkijarven_polkka.mxl", new MXMlConf
				{
					Speed = 240,
					HonorRepeats = true,
					Autoscale = new()
					{
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.Auto) }
					},
				}),

				// https://musescore.com/user/70981/scores/1086966
				MXMLParser.Parse(SongPath + "_Beethoven_Virus_.mxl", new MXMlConf
				{
					Speed = 150,
					HonorRepeats = false,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.Auto) },
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.FromLowerUp) }
					},
				}),

				// https://musescore.com/user/1417101/scores/960611
				MXMLParser.Parse(SongPath + "La_Valse_d'Amélie_(original_version)_Yann_Tiersen.mxl", new MXMlConf
				{
					Speed = 360,
					HonorRepeats = false,
					Autoscale = new()
					{
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.Auto) }
					},
				}),

				//list.Add(Warframe_LiftTogether.Get());

				// https://musescore.com/rhythmicrevival/never_gonna_give_you_up_for_two_melodicas
				MXMLParser.Parse(SongPath + "Never_Gonna_Give_You_Up_-_Melodica_Duet.mxl", new MXMlConf
				{
					Speed = 120,
					HonorRepeats = false,
					Transpose = Transpose.F2,
				}),

				// https://musescore.com/user/28837378/scores/5144713
				MXMLParser.Parse(SongPath + "Tetris_Theme_(Korobeniki)_Easy_to_play.mxl", new MXMlConf
				{
					Speed = 80,
					HonorRepeats = false,
				}),

				// https://musescore.com/radical_edward/scores/3660686
				MXMLParser.Parse(SongPath + "A_Cruel_Angel's_Thesis_-_Neon_Genesis_Evangelion.mxl", new MXMlConf
				{
					Speed = 120,
					HonorRepeats = false,
					Transpose = Transpose.F3,
					Autoscale = new()
					{
						{ "P1_2", new(ScaleFunction.None, ScaleDirection.Auto, 0) }
					},
				}),

				// https://musescore.com/user/18080646/scores/3993531
				MXMLParser.Parse(SongPath + "Song_of_Storms_Piano_and_Ocarina.mxl", new MXMlConf
				{
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
				}),

				// https://musescore.com/user/14429466/scores/3073586
				MXMLParser.Parse(SongPath + "Nothing_Else_Matters_(by_Metallica).mxl", new MXMlConf
				{
					Speed = 100,
					HonorRepeats = false,
					Transpose = Transpose.S1,
					MeasureStart = 8,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.Auto, 1, 1) },
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.Auto, 0, 1) },
					}
				}),

				// https://musescore.com/user/21965011/scores/4217351
				MXMLParser.Parse(SongPath + "Hallelujah.mxl", new MXMlConf
				{
					Speed = 50,
					HonorRepeats = false,
					Transpose = Transpose.None,
					MeasureEnd = 30,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.FromUpperDown) },
					}
				}),

				// https://musescore.com/user/252946/scores/235796
				MXMLParser.Parse(SongPath + "Gigi_d'Agostino_-_L'amour_toujours_(I'll_fly_with_you).mxl", new MXMlConf
				{
					Speed = 80,
					HonorRepeats = false,
					Transpose = Transpose.S2,
					Autoscale = new()
					{
						{ "P1_1", new(ScaleFunction.Clamp, ScaleDirection.Auto) },
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.Auto, 0) },
					}
				}),

				// https://musescore.com/user/27176639/scores/6612455
				MXMLParser.Parse(SongPath + "Soon_May_The_Wellerman_Come.mxl", new MXMlConf
				{
					Speed = 220,
					HonorRepeats = false,
					Transpose = Transpose.F3,
					MeasureEnd = 17,
					Autoscale = new()
					{
						{ "P1_5", new(ScaleFunction.Clamp, ScaleDirection.FromLowerUp, 0, 1) },
					}
				}),

				// https://musescore.com/sh_yy/unravel-animenz-toyko-ghoul
				MXMLParser.Parse(SongPath + "Unravel_[by_Animenz]_-_Tokyo_Ghoul.mxl", new MXMlConf
				{
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
				})
			};

			PlayDialogueLoop(list);
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
				shiftedList.Add(new Accord(p.Length, p.Notes.Select(n => Shift(n, dir)).ToList()));
			}
			return new Voice(shiftedList);
		}

		public static void PlayDialogueLoop(List<Song> list)
		{
			//Console.Clear();

			while (true)
			{
				for (int i = 0; i < list.Count; i++)
				{
					var song = list[i];
					var len = song.GetLength();
					Console.WriteLine("{0,4}: [{1:mm\\:ss}] {2}", i, len, song.Name);
				}

				var rl = Console.ReadLine();
				if (rl is null) break;

				if (int.TryParse(rl, out var num))
				{
					var song = list[num];
					Console.WriteLine("Playing: {0}", song.Name);
					Play(song);
				}

				Console.Clear();
			}
		}

		public static void Play(Song song)
		{
			var merged = Merge(song.Voices);
			var length = song.GetLength();
			var played = TimeSpan.Zero;
			var drawLine = Console.CursorTop;

			Util.Focus();
			foreach (var acc in merged.Accords)
			{
				if (!Util.GenshinHasFocus()) break;

				PlayNote(acc.Notes);
				var noteLength = song.LenToTime(acc.Length);
				Thread.Sleep(noteLength);
				played += noteLength;

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

		public static Voice Pause(int len) => new(new List<Accord> { new Accord(len) });
	}

	public record Song(float Bpm, string Name, List<Voice> Voices)
	{
		public Song(float bpm, string name, params Voice[] voices) : this(bpm, name, voices.ToList()) { }

		public TimeSpan LenToTime(int len)
		{
			return TimeSpan.FromSeconds((len / (float)LengthQuarter) / (Bpm / 60));
		}

		public TimeSpan GetLength()
		{
			return LenToTime(Voices.Max(v => v.Accords.Sum(a => a.Length)));
		}
	}

	public record Note
	{
		public Scale S { get; set; }
		public int Oct { get; set; }

		public Note(Scale s, int oct)
		{
			S = s;
			Oct = oct;
		}

		public override string ToString() => $"{S}{Oct}";
	}

	public record Accord
	{
		public int Length { get; set; }
		public List<Note> Notes { get; set; } = new();

		public Accord(int Length, params Note[] Notes) : this(Length, Notes.ToList()) { }
		public Accord(int length, List<Note> notes) { (Length, Notes) = (length, notes); }
		public override string ToString() => $"<{string.Join(":", Notes)}|{Length}>";
	}

	public record Voice(List<Accord> Accords)
	{
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
