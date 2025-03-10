using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.TextureFormats
{
	public interface ITextureFileFormat
	{
		bool SupportsLdr { get; }
		bool SupportsHdr { get; }
		bool SupportsCubeMap { get; }
		bool SupportsMipMaps { get; }
		bool SupportsArrays { get; }

		bool IsSupportedFormat(CompressionFormat format);
		void FromTextureData(BCnTextureData textureData);
		BCnTextureData ToTextureData();
		void ReadFromStream(Stream inputStream);
		void WriteToStream(Stream outputStream);
	}
	public interface ITextureFileFormat<T> : ITextureFileFormat where T : class, ITextureFileFormat<T>, new()
	{
		public static T Read(Stream inputStream)
		{
			var tex = new T();
			tex.ReadFromStream(inputStream);
			return tex;
		}
	}

	public static class TextureFileFormatExtensions
	{

	}
}
