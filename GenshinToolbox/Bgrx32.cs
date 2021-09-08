using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GenshinToolbox
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Bgrx32 : IEquatable<Bgrx32>
	{
		[FieldOffset(0)]
		public uint All;
		[FieldOffset(0)]
		public byte B;
		[FieldOffset(1)]
		public byte G;
		[FieldOffset(2)]
		public byte R;

		[FieldOffset(3)]
		private byte _X;

		public static readonly Bgrx32 White = FromColor(Color.White);
		public static readonly Bgrx32 Black = FromColor(Color.Black);

		public Bgrx32(byte r, byte g, byte b) : this()
		{
			R = r;
			G = g;
			B = b;
			_X = 255;
		}

		public override bool Equals(object? obj)
		{
			return obj is Bgrx32 rgbx && Equals(rgbx);
		}

		public bool Equals(Bgrx32 other) => All == other.All;
		public static bool operator ==(Bgrx32 left, Bgrx32 right) => (left.All & 0x00FFFFFF) == (right.All & 0x00FFFFFF);
		public static bool operator !=(Bgrx32 left, Bgrx32 right) => !(left == right);
		public override int GetHashCode() => unchecked((int)All);

		public byte Lightness => unchecked((byte)((0.299f * R) + (0.587f * G) + (0.114f * B)));

		public static Bgrx32 FromColor(Color c) => new(c.R, c.G, c.B);

		public override string? ToString() => $"{R:X2}{G:X2}{B:X2}";
	}
}
