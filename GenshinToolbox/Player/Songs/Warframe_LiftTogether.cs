using System.Collections.Generic;
using static GenshinToolbox.Player.NoteLength;
using static GenshinToolbox.Player.Scale;

namespace GenshinToolbox.Player.Songs
{
	class Warframe_LiftTogether
	{
		public static Song Get()
		{
			var m1_8 = new Voice(new List<Accord> {
				// 1
				new Accord(QuartDot, new Note(A, 1)),
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart, new Note(G, 1)),
				new Accord(Quart, new Note(A, 1)),
				// 2
				new Accord(Quart, new Note(C, 2)),
				new Accord(Quart, new Note(H, 1)),
				new Accord(Eigth, new Note(A, 1)),
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart),
				// 3
				new Accord(QuartDot, new Note(A, 1)),
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart, new Note(G, 1)),
				new Accord(Quart, new Note(A, 1)),
				// 4
				new Accord(Eigth, new Note(C, 2)),
				new Accord(QuartDot, new Note(D, 2)),
				new Accord(Eigth, new Note(A, 1)),
				new Accord(QuartDot),
				// 5
				new Accord(QuartDot, new Note(A, 1)),
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart, new Note(G, 1)),
				new Accord(Quart, new Note(A, 1)),
				// 6
				new Accord(Quart, new Note(E, 2)),
				new Accord(Quart, new Note(D, 2)),
				new Accord(Eigth, new Note(C, 2)),
				new Accord(Eigth, new Note(A, 1)),
				new Accord(Quart),
				// 7
				new Accord(Eigth, new Note(G, 2)),
				new Accord(QuartDot, new Note(F, 2)),
				new Accord(Eigth, new Note(E, 2)),
				new Accord(QuartDot, new Note(D, 2)),
				// 8
				new Accord(Eigth, new Note(C, 2)),
				new Accord(QuartDot, new Note(H, 1)),
				new Accord(Quart, new Note(A, 1)),
				new Accord(Quart, new Note(G, 1)),
			});

			var m9_16 = new Voice(new List<Accord> {
				// 9
				new Accord(QuartDot, new Note(A, 1)),
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart, new Note(G, 1)),
				new Accord(Quart, new Note(A, 1)),
				// 10
				new Accord(Eigth, new Note(C, 2)),
				new Accord(QuartDot, new Note(H, 1)),
				new Accord(Eigth, new Note(A, 1)),
				new Accord(QuartDot, new Note(E, 1)),
				// 11
				new Accord(QuartDot, new Note(A, 1)),
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart, new Note(G, 1)),
				new Accord(Quart, new Note(A, 1)),
				// 12
				new Accord(Eigth, new Note(C, 2)),
				new Accord(QuartDot, new Note(D, 2)),
				new Accord(Eigth, new Note(A, 1)),
				new Accord(QuartDot),
				// 13
				new Accord(QuartDot, new Note(A, 1)),
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart, new Note(G, 1)),
				new Accord(Quart, new Note(A, 1)),
				// 14
				new Accord(Quart, new Note(C, 2)),
				new Accord(Quart, new Note(H, 1)),
				new Accord(Eigth, new Note(A, 1)),
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart),
				// 15
				new Accord(QuartDot, new Note(E, 2)),
				new Accord(Eigth, new Note(D, 2)),
				new Accord(Quart, new Note(C, 2)),
				new Accord(Quart, new Note(A, 1)),
				// 16
				new Accord(Eigth, new Note(C, 2)),
				new Accord(QuartDot, new Note(D, 2)),
				new Accord(Quart, new Note(A, 1)),
				new Accord(Quart),
			});

			var m17_s1 = new Voice(new List<Accord> { new Accord(QuartDot, new Note(A, 0), new Note(F, 0)), });

			var m17_24 = new Voice(new List<Accord> {
				// 17
				// ..
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart, new Note(G, 1)),
				new Accord(Quart, new Note(A, 1)),
				// 18
				new Accord(Eigth, new Note(C, 2)),
				new Accord(QuartDot, new Note(H, 1)),
				new Accord(Eigth, new Note(A, 1)),
				new Accord(QuartDot, new Note(E, 1)),
				// 19
				new Accord(QuartDot, new Note(A, 1)),
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart, new Note(G, 1)),
				new Accord(Quart, new Note(A, 1)),
				// 20
				new Accord(Eigth, new Note(C, 2)),
				new Accord(QuartDot, new Note(D, 2)),
				new Accord(Eigth, new Note(A, 1)),
				new Accord(QuartDot),
				// 21
				new Accord(Eigth, new Note(A, 1)),
				new Accord(QuartDot, new Note(E, 1)),
				new Accord(Quart, new Note(G, 1)),
				new Accord(Quart, new Note(A, 1)),
				// 22
				new Accord(Eigth, new Note(C, 2)),
				new Accord(QuartDot, new Note(H, 1)),
				new Accord(Eigth, new Note(A, 1)),
				new Accord(Eigth, new Note(E, 1)),
				new Accord(Quart),
				// 23
				new Accord(Quart, new Note(G, 2)),
				new Accord(Quart, new Note(F, 2)),
				new Accord(Eigth, new Note(E, 2)),
				new Accord(QuartDot, new Note(D, 2)),
				// 24
				new Accord(Eigth, new Note(C, 2)),
				new Accord(QuartDot, new Note(H, 1)),
				new Accord(Eigth, new Note(A, 1)),
				new Accord(QuartDot, new Note(G, 1)),
			});

			var guit_16_24 = MuseEngine.Shift(new Voice(new List<Accord>{
				// 16
				new Accord(Eigth, new Note(A, 0), new Note(E, 1)),
				new Accord(QuartDot, new Note(C, 1), new Note(F, 1)),
				new Accord(HalfN, new Note(C, 1), new Note(G, 0)),
				// 17
				new Accord(HalfN, new Note(C, 1), new Note(G, 0)),
				new Accord(HalfN, new Note(C, 1), new Note(G, 0)),
				//18
				new Accord(QuartDot, new Note(C, 1), new Note(G, 0)),
				new Accord(Eigth, new Note(F, 0)),
				new Accord(Eigth, new Note(D, 1), new Note(H, 0)),
				new Accord(Eigth, new Note(E, 1), new Note(C, 1)),
				new Accord(Quart),
				// 19
				new Accord(HalfN, new Note(C, 1), new Note(G, 0)),
				new Accord(HalfN, new Note(C, 1), new Note(G, 0)),
				// 20
				new Accord(QuartDot, new Note(C, 1), new Note(G, 0)),
				new Accord(Eigth, new Note(F, 0)),
				new Accord(Eigth, new Note(D, 1), new Note(H, 0)),
				new Accord(Eigth, new Note(E, 1), new Note(C, 1)),
				new Accord(Quart),
				// 21
				new Accord(HalfN, new Note(C, 1), new Note(G, 0)),
				new Accord(HalfN, new Note(C, 1), new Note(G, 0)),
				// 22
				new Accord(QuartDot, new Note(C, 1), new Note(G, 0)),
				new Accord(Eigth, new Note(F, 0)),
				new Accord(Eigth, new Note(D, 1), new Note(H, 0)),
				new Accord(Eigth, new Note(E, 1), new Note(C, 1)),
				new Accord(Quart),
				// 23
				new Accord(HalfN, new Note(C, 1), new Note(G, 0)),
				new Accord(QuartDot, new Note(C, 1), new Note(G, 0)),
				new Accord(Eigth, new Note(F, 0)),
				// 24
				new Accord(Eigth, new Note(A, 0), new Note(E, 1)),
				new Accord(QuartDot, new Note(C, 1), new Note(F, 1)),
				new Accord(HalfN, new Note(C, 1), new Note(G, 0)),
			}), -2);

			var baseMelo = m1_8 + m9_16 + m17_s1 + m17_24;
			var subMelo = MuseEngine.Pause(Full * 8) + MuseEngine.Shift(m9_16 + MuseEngine.Pause(QuartDot) + m17_24, -7);
			var guitar = MuseEngine.Pause(Full * 15) + guit_16_24;

			return new Song(new MXMlConf { Speed = 132 }, "Warframe (handwritten)", baseMelo, subMelo, guitar);
		}
	}
}
