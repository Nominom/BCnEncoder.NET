using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.TextureFormats;
using Microsoft.Toolkit.HighPerformance;

namespace BCnEncoder.Shared
{
	public enum CubeMapFaceDirection
	{
		/// <summary>
		/// Right face
		/// </summary>
		XPositive = 0,
		/// <summary>
		/// Left face
		/// </summary>
		XNegative = 1,
		/// <summary>
		/// Top face
		/// </summary>
		YPositive = 2,
		/// <summary>
		/// Bottom face
		/// </summary>
		YNegative = 3,
		/// <summary>
		/// Back face
		/// </summary>
		ZPositive = 4,
		/// <summary>
		/// Front face
		/// </summary>
		ZNegative = 5
	}
	
	public class BCnTextureData
	{
		public BCnTextureData(CompressionFormat format, int width, int height, int numMips, bool isCubeMap = false, bool allocateBuffers = false)
		{
			Format = format;
			Width = width;
			Height = height;
			NumFaces = isCubeMap ? 6 : 1;
			NumMips = numMips;
			Faces = new CubeMapFace[NumFaces];
			
			for (var f = 0; f < NumFaces; f++)
			{
				Faces[f] = new CubeMapFace
				{
					Direction = (CubeMapFaceDirection)f,
					Mips = new MipMapLevel[numMips]
				};
				for (var m = 0; m < numMips; m++)
				{
					MipMapper.CalculateMipLevelSize(width, height, m, out var mipWidth, out var mipHeight);

					Faces[f].Mips[m] = new MipMapLevel()
					{
						SizeInBytes = format.CalculateMipByteSize(mipWidth, mipHeight),
						Data = allocateBuffers ? new byte[format.CalculateMipByteSize(mipWidth, mipHeight)] : null,
						Width = mipWidth,
						Height = mipHeight
					};
				}
			}
		}

		/// <summary>
		/// Create a texture data with a single face and a single mipmap.
		/// </summary>
		public BCnTextureData(CompressionFormat format, int width, int height, byte[] singleMipData)
		{
			Format = format;
			Width = width;
			Height = height;
			NumFaces = 1;
			NumMips = 1;

			Faces = new CubeMapFace[]
			{
				new CubeMapFace()
				{
					Direction = CubeMapFaceDirection.XPositive,
					Mips = new MipMapLevel[]
					{
						new MipMapLevel()
						{
							SizeInBytes = singleMipData.LongLength,
							Data = singleMipData,
							Width = width,
							Height = height
						}
					}
				}
			};
		}

		public class MipMapLevel
		{
			public int Width { get; set; }
			public int Height { get; set; }
			public long SizeInBytes { get; set; }
			public byte[] Data { get; set; }

			/// <summary>
			/// Potentially dangerous!
			/// Please use only when you know for sure the underlying data is of the provided type.
			/// I.e. Check that CompressionFormat == <see cref="CompressionFormat.Rgba32"/>
			/// before trying to get a <see cref="ColorRgba32"/> memory.
			/// </summary>
			public Memory2D<T> AsMemory2D<T>() where T : unmanaged
			{
				if (Unsafe.SizeOf<T>() * Width * Height != Data.Length)
				{
					throw new InvalidCastException($"The size of {typeof(T).Name} does not match data format!");
				}
				return Data.AsMemory().Cast<byte, T>()
					.AsMemory2D(Height, Width);
			}

			/// <summary>
			/// Potentially dangerous!
			/// Please use only when you know for sure the underlying data is of the provided type.
			/// I.e. Check that CompressionFormat == <see cref="CompressionFormat.Rgba32"/>
			/// before trying to get a <see cref="ColorRgba32"/> memory.
			/// </summary>
			public Memory<T> AsMemory<T>() where T : unmanaged
			{
				if (Unsafe.SizeOf<T>() * Width * Height != Data.Length)
				{
					throw new InvalidCastException($"The size of {typeof(T).Name} does not match data format!");
				}

				return Data.AsMemory().Cast<byte, T>();
			}
		}

		public class CubeMapFace
		{
			public CubeMapFaceDirection Direction { get; set; }
			public MipMapLevel[] Mips { get; set; }
		}

		/// <summary>
		/// Width of mip0
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Height of mip0
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Number of mip maps
		/// </summary>
		public int NumMips { get; set; }

		/// <summary>
		/// Number of faces. 1 if normal texture, 6 if cube map
		/// </summary>
		public int NumFaces { get; set; }

		/// <summary>
		/// Cubemap faces. Only 1 face is present if the texture is not a cubemap.
		/// </summary>
		public CubeMapFace[] Faces { get; set; }

		/// <summary>
		/// The CompressionFormat of this texture data.
		/// </summary>
		public CompressionFormat Format { get; set; }

		/// <summary>
		/// Get a cubemap face by its direction.
		/// Only <see cref="CubeMapFaceDirection.XPositive"/> is present if the texture is not a cubemap.
		/// </summary>
		public CubeMapFace this[CubeMapFaceDirection direction] => Faces[(int)direction];

		/// <summary>
		/// Access mip levels directly if not a cube map
		/// </summary>
		public MipMapLevel[] MipLevels => Faces[0].Mips;

		/// <summary>
		/// True if texture is cube map
		/// </summary>
		public bool IsCubeMap => NumFaces > 1;

		/// <summary>
		/// True if texture has mip levels
		/// </summary>
		public bool HasMipLevels => NumMips > 1;

		/// <summary>
		/// Total byte size of all the faces and MipMaps summed together.
		/// </summary>
		public long TotalSize => Faces.Sum(f => f.Mips.Sum(m => m.SizeInBytes));

		/// <summary>
		/// True if the texture format is one of the block-compressed formats
		/// </summary>
		public bool IsBlockCompressed => Format.IsBlockCompressedFormat();

		public TTexture AsTexture<TTexture>()
			where TTexture : class, ITextureFileFormat, new()
		{
			var tex = new TTexture();
			tex.FromTextureData(this);
			return tex;
		}

		public static BCnTextureData CombineCubeMap(
			BCnTextureData right,
			BCnTextureData left,
			BCnTextureData top,
			BCnTextureData down,
			BCnTextureData back,
			BCnTextureData front
		)
		{
			if (
				right.Width != left.Width || right.Height != left.Height ||
				right.Width != top.Width || right.Height != top.Height ||
				right.Width != down.Width || right.Height != down.Height ||
				right.Width != back.Width || right.Height != back.Height ||
				right.Width != front.Width || right.Height != front.Height
			)
			{
				throw new ArgumentException("All faces of a cubeMap must be of equal width and height!");
			}
			if (
				right.Format != left.Format ||
				left.Format != top.Format ||
				top.Format != down.Format ||
				down.Format != back.Format ||
				back.Format != front.Format)
			{
				throw new ArgumentException("All faces of a cubeMap must have the same format!");
			}
			if (
				right.NumMips != left.NumMips ||
				left.NumMips != top.NumMips ||
				top.NumMips != down.NumMips ||
				down.NumMips != back.NumMips ||
				back.NumMips != front.NumMips)
			{
				throw new ArgumentException("All faces of a cubeMap must have the same amount of mipmaps!");
			}

			var outData = new BCnTextureData(right.Format, right.Width, right.Height, right.NumMips, true, false);
			for (var m = 0; m < right.NumMips; m++)
			{
				outData[CubeMapFaceDirection.XPositive].Mips[m].Data = right.MipLevels[m].Data;
				outData[CubeMapFaceDirection.XNegative].Mips[m].Data = left.MipLevels[m].Data;
				outData[CubeMapFaceDirection.YPositive].Mips[m].Data = top.MipLevels[m].Data;
				outData[CubeMapFaceDirection.YNegative].Mips[m].Data = down.MipLevels[m].Data;
				outData[CubeMapFaceDirection.ZPositive].Mips[m].Data = back.MipLevels[m].Data;
				outData[CubeMapFaceDirection.ZNegative].Mips[m].Data = front.MipLevels[m].Data;
			}
			return outData;
		}
	}

	public static class BCnTextureDataExtensions
	{
		/// <summary>
		/// Convert a <see cref="BCnTextureData"/> to another uncompressed pixel format.
		/// If the texture data is block-compressed, it is decoded first using <see cref="BcDecoder"/>.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="format"></param>
		/// <returns>Returns self if already desired format. New data is created otherwise.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static BCnTextureData ConvertTo(this BCnTextureData data, CompressionFormat format)
		{
			if (data.Format == format)
				return data;

			if (format.IsBlockCompressedFormat())
			{
				throw new ArgumentException(
					$"New format should be a non-block-compressed format. Please use {nameof(BcEncoder)} for encoding to compressed formats!");
			}

			var decoded = data;

			if(data.Format.IsBlockCompressedFormat())
			{
				var decoder = new BcDecoder
				{
					Options =
					{
						IsParallel = false
					}
				};

				decoded = decoder.Decode(data);
			}

			if (decoded.Format == format)
			{
				return decoded;
			}

			return ConvertPixelFormat(decoded, format);
		}

		private static BCnTextureData ConvertPixelFormat(BCnTextureData data, CompressionFormat newFormat)
		{
			var newData = new BCnTextureData(newFormat, data.Width, data.Height, data.NumMips, data.IsCubeMap, false);

			for (var f = 0; f < data.NumFaces; f++)
			{
				for (var m = 0; m < data.NumMips; m++)
				{
					var converted =
						ColorExtensions.InternalConvertToAsBytesFromBytes(
							data.Faces[f].Mips[m].Data,
							data.Format,
							newFormat);
					newData.Faces[f].Mips[m].Data = converted;
				}
			}

			return newData;
		}
	}
}
