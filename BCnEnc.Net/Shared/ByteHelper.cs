namespace BCnComp.Net.Shared
{
	public static class ByteHelper
	{
		public static byte ClampToByte(int i) {
			if (i < 0) i = 0;
			if (i > 255) i = 255;
			return (byte) i;
		}

		public static byte ClampToByte(float f)
			=> ClampToByte((int) f);
	}
}
