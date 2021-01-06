using System;
using System.Collections.Generic;
using System.Text;

namespace BCnEncoder.Shared
{
	public struct Rgba32
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public Rgba32(byte r, byte g, byte b, byte a)
		{
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = a;
		}

		public Rgba32(byte r, byte g, byte b)
		{
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = 255;
		}

		public bool Equals(Rgba32 other)
		{
			return R == other.R && G == other.G && B == other.B && A == other.A;
		}

		public override bool Equals(object obj)
		{
			return obj is Rgba32 other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = R.GetHashCode();
				hashCode = (hashCode * 397) ^ G.GetHashCode();
				hashCode = (hashCode * 397) ^ B.GetHashCode();
				hashCode = (hashCode * 397) ^ A.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(Rgba32 left, Rgba32 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Rgba32 left, Rgba32 right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return $"{nameof(R)}: {R}, {nameof(G)}: {G}, {nameof(B)}: {B}, {nameof(A)}: {A}";
		}
	}
}
