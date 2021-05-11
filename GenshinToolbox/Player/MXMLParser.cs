using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace GenshinToolbox.Player
{
	public class MXMLParser
	{
		public static Song Parse(string file, MXMlConf conf)
		{
			var name = conf.Name ?? Path.GetFileNameWithoutExtension(file) ?? file ?? "Unknown";
			Console.WriteLine("Parsing {0}", name);

			// TODO FIX:
			// - repeats are not added when a new voice gets added in mid
			// - autoadjust xml measure length to norm

			using var fs = File.OpenRead(file);
			using var ds = new ZipArchive(fs, ZipArchiveMode.Read);
			using var scoreFile = ds.GetEntry("score.xml").Open();
			using var sr = new StreamReader(scoreFile);
			var scoreFileContent = sr.ReadToEnd();
			var xml = XDocument.Parse(scoreFileContent);

			var voices = new Dictionary<string, VoiceEdit>();

			var partList = xml.Root.Element("part-list");
			var pickInstrumentsIds = new HashSet<string>();
			if (conf.PickInstruments.Count > 0)
			{
				foreach (var scorePart in partList.Elements().Where(c => c.Name.LocalName == "score-part"))
				{
					var id = scorePart.Attribute("id").Value;
					if (conf.PickInstruments.Overlaps(new[] {
						scorePart.Element("part-name")?.Value,
						scorePart.Element("part-abbreviation")?.Value,
					}))
					{
						pickInstrumentsIds.Add(id);
					}
				}
			}

			foreach (var part in xml.Root.Elements().Where(c => c.Name.LocalName == "part"))
			{
				var partId = part.Attribute("id").Value;
				if (pickInstrumentsIds.Count > 0 && !pickInstrumentsIds.Contains(partId))
					continue;

				var voicesInCurrentPart = new List<VoiceEdit>();
				var ties = new HashSet<string>();
				foreach (var measure in part.Elements().Where(c => c.Name.LocalName == "measure"))
				{
					var measure_num = int.Parse(measure.Attribute("number").Value);
					if (measure_num < conf.MeasureStart || measure_num >= conf.MeasureEnd) continue;

					var currentLen = voicesInCurrentPart.Select(ve => ve.Length).FirstOrDefault();

					foreach (var playElem in measure.Elements())
					{
						if (playElem.Name.LocalName == "note")
						{
							var isPause = playElem.Element("rest") is not null;
							var duration = int.Parse(playElem.Element("duration")?.Value ?? "0") * NoteLength.Full / 16;
							var voice_key = partId + "_" + playElem.Element("voice")?.Value ?? "?";

							if (!voices.TryGetValue(voice_key, out var ve))
							{
								ve = new VoiceEdit();
								if (currentLen > 0)
									ve.Add(new Accord(currentLen));
								voices.Add(voice_key, ve);
								voicesInCurrentPart.Add(ve);
							}
							ve.Changed = true;

							var isTied = ties.Contains(voice_key);
							var tieStart = playElem.Element("tie")?.Attribute("type")?.Value == "start";
							if (tieStart) ties.Add(voice_key);
							var tieEnd = playElem.Element("tie")?.Attribute("type")?.Value == "stop";
							if (tieEnd) ties.Remove(voice_key);

							if (isPause)
							{
								ve.Add(new Accord(duration));
							}
							else
							{
								var isInPrevChord = playElem.Element("chord") is not null;

								var pitchData = playElem.Element("pitch");
								if (pitchData is null) continue;
								var scale = Enum.Parse<Scale>(pitchData.Element("step").Value, true);
								var oct = int.Parse(pitchData.Element("octave").Value);
								var alter = int.Parse(pitchData.Element("alter")?.Value ?? "0");

								var note = new Note(scale, oct);

								var remove = false;

								// TODO check if the alter matches our tonality

								switch (isTied, isInPrevChord, remove) // conf.RemoveShiftedNotes && alter != 0
								{
									case (_, true, true): break;
									case (_, true, false): ve.Voice.Accords.Last().Notes.Add(note); break;
									case (_, false, true): ve.Add(new Accord(duration)); break;
									case (true, false, false): { ve.Voice.Accords.Last().Length += duration; ve.Length += duration; break; }
									case (false, false, false): ve.Add(new Accord(duration, note)); break;
									default: throw new Exception("Unhandled case");
								}
							}
						}
						else if (playElem.Name.LocalName == "barline")
						{
							if (conf.HonorRepeats)
							{
								var jumpDir = playElem.Element("repeat")?.Attribute("direction")?.Value;
								if (jumpDir == "forward")
								{
									foreach (var ve in voices.Values)
										ve.Repeats.Add(new Jump { Start = ve.Voice.Accords.Count });
								}
								else if (jumpDir == "backward")
								{
									foreach (var ve in voices.Values)
										ve.Repeats.Last().End = ve.Voice.Accords.Count;
								}
							}
						}
					}

					// After each measure fill all voices up with silence to be in sync
					currentLen = voicesInCurrentPart.Select(ve => ve.Length).Max();
					foreach (var ve in voicesInCurrentPart)
					{
						if (ve.Length < currentLen)
						{
							ve.Add(new Accord(currentLen - ve.Length));
						}
					}
				}
			}

			var finalVoices = new List<Voice>();
			foreach (var (key, ve) in voices)
			{
				var scaleConf = conf.Autoscale.GetValueOrDefault(key, new());
				var voice = ve.Voice;
				if (!voice.Accords.SelectMany(a => a.Notes).Any()) continue;

				// Check if we want to filter voices
				//if (conf.PickVoice.Count > 0 && !conf.PickVoice.Contains(key)) continue;

				// Transpose
				if (conf.Transpose != Transpose.None)
				{
					voice = MuseEngine.Shift(voice, (int)conf.Transpose);
				}

				// Autoscale/trim
				var octMin = voice.Accords.SelectMany(a => a.Notes).Min(n => n.Oct);
				var octMax = voice.Accords.SelectMany(a => a.Notes).Max(n => n.Oct);
				Console.Write(" > {0}: [{1};{2}]", key, octMin, octMax);
				if (octMax - octMin > scaleConf.Range)
				{
				ask:
					if (scaleConf.sf == ScaleFunction.None)
					{
						Console.WriteLine();
						Console.WriteLine("Scale to big! Drop (d) or Clamp (c) ?");
						switch (Console.ReadKey().Key)
						{
							case ConsoleKey.D: scaleConf.sf = ScaleFunction.Drop; break;
							case ConsoleKey.C: scaleConf.sf = ScaleFunction.Clamp; break;
							default: goto ask;
						}
					}

					var iter = (octMax - octMin) - Math.Clamp(scaleConf.Range, 0, 2);

					for (int i = 0; i < iter; i++)
					{
						var numMin = voice.Accords.SelectMany(a => a.Notes).Count(n => n.Oct == octMin);
						var numMax = voice.Accords.SelectMany(a => a.Notes).Count(n => n.Oct == octMax);

						//int modOct = 0, num = 0, modDir = 0;
						var (modOct, num, modDir) = (scaleConf.sd) switch
						{
							ScaleDirection.Auto => numMin < numMax ? (octMin, numMin, 1) : (octMax, numMax, -1),
							ScaleDirection.FromLowerUp => (octMin, numMin, 1),
							ScaleDirection.FromUpperDown => (octMax, numMax, -1),
							_ => throw new InvalidOperationException(),
						};

						Console.Write(" <{0}{2} {1}♫>", modOct, num, modDir > 0 ? '⬆' : '⬇');
						switch (scaleConf.sf)
						{
							case ScaleFunction.Drop:
								voice.Accords.ForEach(a => a.Notes.RemoveAll(n => n.Oct == modOct));
								break;
							case ScaleFunction.Clamp:
								voice.Accords.ForEach(a => a.Notes.ForEach(n => { if (n.Oct == modOct) n.Oct += modDir; }));
								break;
							default:
								throw new InvalidOperationException("Voice must be scaled");
						}

						if (modDir == 1) octMin++; else octMax--;
					}
				}

				// Apply autoshift into correct octaves
				{
					int sub;
					if (scaleConf.LowerOctTarget.HasValue)
						sub = octMin - scaleConf.LowerOctTarget.Value;
					else if (octMax > 2)
						sub = octMax - 2;
					else
						sub = 0;

					Console.Write($" -> [{octMin - sub};{octMax - sub}]");
					if (sub != 0)
					{
						foreach (var note in voice.Accords.SelectMany(a => a.Notes))
						{
							note.Oct -= sub;
						}
					}
					finalVoices.Add(voice);
				}

				// Apply loops
				if (conf.HonorRepeats)
				{
					foreach (var rep in ve.Repeats.Reverse<Jump>())
					{
						voice.Accords.InsertRange(rep.End, voice.Accords.Skip(rep.Start).Take(rep.End - rep.Start).ToArray());
					}
				}

				Trace.Assert(voice.Accords.SelectMany(a => a.Notes).Max(n => n.Oct) - voice.Accords.SelectMany(a => a.Notes).Min(n => n.Oct) <= scaleConf.Range);

				Console.WriteLine();
			}

			return new Song(conf.Speed, name, finalVoices);
		}
	}

	public class MXMlConf
	{
		public string Name { get; set; }
		public float Speed { get; set; }
		public bool HonorRepeats { get; set; }
		public bool RemoveShiftedNotes { get; set; } = true;
		public Transpose Transpose { get; set; } = Transpose.None;
		public HashSet<string> PickInstruments { get; set; } = new();
		public Dictionary<string, ScaleConf> Autoscale { get; set; } = new();
		/// <summary>Inclusive</summary>
		public int? MeasureStart { get; set; } = null;
		/// <summary>Excludive</summary>
		public int? MeasureEnd { get; set; } = null;
	}

	public class ScaleConf
	{
		public ScaleFunction sf { get; set; } = ScaleFunction.None;
		public ScaleDirection sd { get; set; } = ScaleDirection.Auto;
		public int? LowerOctTarget { get; set; } = null;
		public int Range { get; set; } = 2;

		public ScaleConf(
			ScaleFunction sf = ScaleFunction.None,
			ScaleDirection sd = ScaleDirection.Auto,
			int? lowerOctTarget = null,
			int range = 2)
		{
			this.sf = sf;
			this.sd = sd;
			LowerOctTarget = lowerOctTarget;
			Range = range;
		}
	}

	public enum ScaleFunction
	{
		None,
		/// <summary>Remove out of range</summary>
		Drop,
		/// <summary>Wrap octave up/down to fit</summary>
		Clamp,
	}

	public enum ScaleDirection
	{
		Auto,
		FromLowerUp,
		FromUpperDown,
	}

	/// <summary>(F)lat or (S)harp written in</summary>
	public enum Transpose
	{
		None = 0,
		S1 = +3,
		S2 = +6,
		S3 = +2,
		S4 = +5,
		S5 = +1,
		S6 = +4,
		F1 = S6,
		F2 = S5,
		F3 = S4,
		F4 = S3,
		F5 = S2,
		F6 = S1,
	}

	public class VoiceEdit
	{
		public bool Changed { get; set; }
		public Voice Voice { get; set; }
		public int Length { get; set; } = 0;
		public List<Jump> Repeats { get; } = new();

		public VoiceEdit()
		{
			Changed = false;
			Voice = new Voice(new());
		}

		public void Add(Accord acc)
		{
			Length += acc.Length;
			Voice.Accords.Add(acc);
		}

		public override string ToString() => $"{(Changed ? "*" : "")} {Length} || {Voice}";
	}

	public record Jump
	{
		public int Start { get; set; }
		public int End { get; set; }

		public override string ToString() => $"{Start}<-{End}";
	}
}
