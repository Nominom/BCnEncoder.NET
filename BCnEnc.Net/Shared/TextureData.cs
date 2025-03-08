using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared.Colors;
using BCnEncoder.TextureFormats;
using CommunityToolkit.HighPerformance;

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
		public BCnTextureData(CompressionFormat inputFormat, int width, int height, int depth = 1, int numMips = 0, int numArrayElements = 0, bool isCubeMap = false, bool allocateBuffers = false)
		{
			if (width <= 0)
				throw new ArgumentException($"Invalid texture size width: {width}", nameof(width));
			if (height <= 0)
				throw new ArgumentException($"Invalid texture size height: {height}", nameof(height));

			Format = inputFormat;
			Width = width;
			Height = height;
			Depth = Math.Max(depth, 1);

			NumFaces = isCubeMap ? 6 : 1;
			NumMips = Math.Max(numMips, 1);
			NumArrayElements = Math.Max(numArrayElements, 1);

			Mips = new MipMapLevel[NumMips];
			for (int i = 0; i < NumMips; i++)
			{
				Mips[i] = new MipMapLevel(this, i, NumFaces, NumArrayElements, allocateBuffers);
			}
		}

		public class MipMapLevel
		{
			private readonly BCnTextureData texture;
			public int Width { get; set; }
			public int Height { get; set; }
			public long SizeInBytes { get; set; }

			private TexData[] textureData;

			public TexData this[CubeMapFaceDirection face, int arrayIndex = 0]
			{
				get => textureData[(int)face * texture.NumArrayElements + arrayIndex];
				set => textureData[(int)face * texture.NumArrayElements + arrayIndex] = value;
			}

			public TexData this [int arrayIndex] => textureData[arrayIndex];

			public TexData First => textureData[0];

			internal MipMapLevel(BCnTextureData texture, int level, int numFaces, int numArrayElements, bool allocateBuffers)
			{
				if (level >= texture.NumMips)
					throw new ArgumentException("Invalid mip level");

				this.texture = texture;
				MipMapper.CalculateMipLevelSize(texture.Width, texture.Height, level, out var mipWidth, out var mipHeight);

				if (mipWidth == 0 || mipHeight == 0)
					throw new ArgumentException("Invalid mip level size");
				if (numFaces != 1 && numFaces != 6)
					throw new ArgumentException("Invalid number of faces");
				if (numArrayElements < 1)
					throw new ArgumentException("Invalid number of array items");

				this.Width = mipWidth;
				this.Height = mipHeight;
				this.SizeInBytes = texture.Format.CalculateMipByteSize(mipWidth, mipHeight);
				this.textureData = new TexData[numFaces * numArrayElements];

				for (var f = 0; f < numFaces; f++)
				{
					for (var a = 0; a < numArrayElements; a++)
					{
						byte[] data = null;
						if (allocateBuffers)
						{
							data = new byte[SizeInBytes];
						}
						textureData[f * numArrayElements + a] = new TexData((CubeMapFaceDirection)f, a, this, data);
					}
				}
			}
		}

		public class TexData
		{
			public CubeMapFaceDirection Direction { get; private set; }
			public int ArrayIndex { get; private set; }
			private MipMapLevel Mip { get; }

			private byte[] data;

			public byte[] Data
			{
				get => data;
				set
				{
					if (value != null && value.LongLength != Mip.SizeInBytes)
						throw new ArgumentException("Data size does not match expected mip size! Please provide a buffer of the correct size.");

					data = value;
				}
			}

			public int Width => Mip.Width;
			public int Height => Mip.Height;

			public TexData(CubeMapFaceDirection direction, int arrayIndex, MipMapLevel mip, byte[] data)
			{
				Direction = direction;
				ArrayIndex = arrayIndex;
				Mip = mip;
				Data = data;
			}


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

		/// <summary>
		/// Width of mip0
		/// </summary>
		public int Width { get; init; }

		/// <summary>
		/// Height of mip0
		/// </summary>
		public int Height { get; init; }

		/// <summary>
		/// Depth of mip0 (For volume textures only. Otherwise, 1)
		/// </summary>
		public int Depth { get; init; }

		/// <summary>
		/// Number of mip maps
		/// </summary>
		public int NumMips { get; set; }

		/// <summary>
		/// Number of faces. 1 if normal texture, 6 if cube map
		/// </summary>
		public int NumFaces { get; set; }

		/// <summary>
		/// Number of array elements. For texture arrays.
		/// </summary>
		public int NumArrayElements { get; set; }

		/// <summary>
		/// The CompressionFormat of this texture data.
		/// </summary>
		public CompressionFormat Format { get; set; }

		/// <summary>
		/// Access mip levels. Only mip level 0 is present if the texture has no mip levels.
		/// </summary>
		public MipMapLevel[] Mips { get; set; }

		/// <summary>
		/// Access the first texture of the first mip level.
		/// This is the default texture for the texture data if there are no mips, no cubemap faces, and no array elements.
		/// </summary>
		public TexData First => Mips[0].First;

		/// <summary>
		/// True if texture is cube map
		/// </summary>
		public bool IsCubeMap => NumFaces > 1;

		/// <summary>
		/// True if texture has mip levels
		/// </summary>
		public bool HasMipLevels => NumMips > 1;

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
				right.Depth != 1 ||
				left.Depth != 1 ||
				top.Depth != 1 ||
				down.Depth != 1 ||
				back.Depth != 1 ||
				front.Depth != 1)
			{
				throw new ArgumentException("All faces of a cubeMap must have a depth of 1!");
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
			if (
				right.NumArrayElements != left.NumArrayElements ||
				left.NumArrayElements != top.NumArrayElements ||
				top.NumArrayElements != down.NumArrayElements ||
				down.NumArrayElements != back.NumArrayElements ||
				back.NumArrayElements != front.NumArrayElements)
			{
				throw new ArgumentException("All faces of a cubeMap must have the same amount of array elements!");
			}

			var outData = new BCnTextureData(right.Format, right.Width, right.Height, right.Depth,
				right.NumMips,
				right.NumArrayElements,
				isCubeMap: true,
				allocateBuffers: false);

			for (var m = 0; m < right.NumMips; m++)
			{
				for (var a = 0; a < right.NumArrayElements; a++)
				{
					outData.Mips[m][CubeMapFaceDirection.XPositive, a].Data = right.Mips[m][CubeMapFaceDirection.XPositive, a].Data;
					outData.Mips[m][CubeMapFaceDirection.XNegative, a].Data = left.Mips[m][CubeMapFaceDirection.XNegative, a].Data;
					outData.Mips[m][CubeMapFaceDirection.YPositive, a].Data = top.Mips[m][CubeMapFaceDirection.YPositive, a].Data;
					outData.Mips[m][CubeMapFaceDirection.YNegative, a].Data = down.Mips[m][CubeMapFaceDirection.YNegative, a].Data;
					outData.Mips[m][CubeMapFaceDirection.ZPositive, a].Data = back.Mips[m][CubeMapFaceDirection.ZPositive, a].Data;
					outData.Mips[m][CubeMapFaceDirection.ZNegative, a].Data = front.Mips[m][CubeMapFaceDirection.ZNegative, a].Data;
				}
			}
			return outData;
		}

		/// <summary>
		/// Checks if the data is valid.
		/// </summary>
		/// <returns>True if the data is valid, false otherwise.</returns>
		public bool IsValid()
		{
			if (Width <= 0 || Height <= 0)
				return false;
			if (NumMips <= 0)
				return false;
			if (NumFaces != 1 || NumFaces != 6)
				return false;
			if (NumArrayElements < 1)
				return false;
			if (Mips.Length != NumMips)
				return false;

			foreach (var t in Mips)
			{
				if (t.Width <= 0 || t.Height <= 0)
					return false;
				if (t.SizeInBytes <= 0)
					return false;

				for (var f = 0; f < NumFaces; f++)
				{
					for (var a = 0; a < NumArrayElements; a++)
					{
						if (t[(CubeMapFaceDirection)f, a].Data == null)
							return false;
						if (t[(CubeMapFaceDirection)f, a].Data.Length != t.SizeInBytes)
							return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Validates the texture data. Throws <see cref="InvalidTextureException"/> if the data is invalid.
		/// </summary>
		public void Validate()
		{

			if (Width <= 0 || Height <= 0)
			{
				throw new InvalidTextureException($"Texture has invalid dimensions: Width={Width}, Height={Height}");
			}

			if (NumMips <= 0)
			{
				throw new InvalidTextureException("Texture has no mip levels");
			}

			if (NumFaces != 1 && NumFaces != 6)
			{
				throw new InvalidTextureException("Texture has invalid number of faces");
			}

			if (NumArrayElements < 1)
			{
				throw new InvalidTextureException("Texture has invalid number of array elements");
			}

			if (Mips.Length != NumMips)
			{
				throw new InvalidTextureException("Texture has incorrect number of mip levels");
			}

			for (var i = 0; i < Mips.Length; i++)
			{
				var mip = Mips[i];
				if (mip.Width <= 0 || mip.Height <= 0)
				{
					throw new InvalidTextureException(
						$"Mip level {i} has invalid dimensions: Width={mip.Width}, Height={mip.Height}");
				}
				if (mip.SizeInBytes <= 0)
					throw new InvalidTextureException($"Mip level {i} has invalid size: {mip.SizeInBytes}");

				for (var f = 0; f < NumFaces; f++)
				{
					for (var a = 0; a < NumArrayElements; a++)
					{
						var texData = mip[(CubeMapFaceDirection)f, a];
						if (texData.Data == null)
						{
							throw new InvalidTextureException(
								$"Mip level {i}, face {(CubeMapFaceDirection)f}, array element {a} has null data");
						}

						if (texData.Data.Length != mip.SizeInBytes)
						{
							throw new InvalidTextureException(
								$"Mip level {i}, face {(CubeMapFaceDirection)f}, array element {a} has incorrect data size");
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates a simple BCnTextureData instance from a single byte array with no mips, no array elements, and not a cubemap.
		/// </summary>
		/// <param name="format">The compression format of the input texture data.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <param name="data">The texture data as a byte array.</param>
		/// <returns>A new BCnTextureData instance.</returns>
		public static BCnTextureData FromSingle(CompressionFormat format, int width, int height, byte[] data)
		{
		    var texture = new BCnTextureData(format, width, height, 1, 1, 1, false, false);
		    texture.Mips[0].First.Data = data.ToArray();
		    return texture;
		}

		/// <summary>
		/// Creates a cubemap BCnTextureData instance from six byte[] faces.
		/// </summary>
		/// <param name="format">The compression format of the texture data.</param>
		/// <param name="size">The size of each cubemap face (assuming square faces).</param>
		/// <param name="faces">An IEnumerable of six byte arrays, one for each cubemap face.</param>
		/// <returns>A new BCnTextureData instance representing a cubemap.</returns>
		/// <exception cref="ArgumentException">Thrown when the number of faces is not exactly 6.</exception>
		public static BCnTextureData FromCubemap(CompressionFormat format, int size, IEnumerable<byte[]> faces)
		{
			var facesArray = faces.ToArray();
		    if (facesArray.Length != 6)
		        throw new ArgumentException("Cubemap requires exactly 6 faces", nameof(faces));

		    var texture = new BCnTextureData(format, size, size, depth: 1, numMips: 1, numArrayElements: 1, true, false);
		    for (int f = 0; f < facesArray.Length; f++)
		    {
		        texture.Mips[0][(CubeMapFaceDirection)f, 0].Data = facesArray[f];
		    }
		    return texture;
		}

		/// <summary>
		/// Creates a texture array BCnTextureData instance from multiple byte[] slices.
		/// </summary>
		/// <param name="format">The compression format of the texture data.</param>
		/// <param name="width">The width of each texture in the array.</param>
		/// <param name="height">The height of each texture in the array.</param>
		/// <param name="slices">An IEnumerable of byte arrays, one for each texture in the array.</param>
		/// <returns>A new BCnTextureData instance representing a texture array.</returns>
		public static BCnTextureData FromTextureArray(CompressionFormat format, int width, int height, IEnumerable<byte[]> slices)
		{
			var slicesArray = slices.ToArray();
		    var texture = new BCnTextureData(format, width, height, depth: 1, numMips: 1, slicesArray.Length, false, false);
		    for (int i = 0; i < slicesArray.Length; i++)
		    {
		        texture.Mips[0][i].Data = slicesArray[i];
		    }
		    return texture;
		}

		/// <summary>
		/// Creates a BCnTextureData instance with multiple mip levels from byte arrays.
		/// </summary>
		/// <param name="format">The compression format of the texture data.</param>
		/// <param name="width">The width of the largest (first) mip level.</param>
		/// <param name="height">The height of the largest (first) mip level.</param>
		/// <param name="mipLevels">An IEnumerable of byte arrays, one for each mip level.</param>
		/// <returns>A new BCnTextureData instance with the specified mip levels.</returns>
		public static BCnTextureData FromMipLevels(CompressionFormat format, int width, int height, IEnumerable<byte[]> mipLevels)
		{
			var mipsArray = mipLevels.ToArray();
		    var texture = new BCnTextureData(format, width, height, depth: 1, mipsArray.Length, 1, false, false);
		    for (int i = 0; i < mipsArray.Length; i++)
		    {
		        texture.Mips[i].First.Data = mipsArray[i];
		    }
		    return texture;
		}
	}

	/// <summary>
	/// Thrown when the provided texture data is invalid.
	/// </summary>
	public class InvalidTextureException : Exception
	{
		public InvalidTextureException(string message) : base(message)
		{
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
			var newData = new BCnTextureData(newFormat, data.Width, data.Height, data.Depth, data.NumMips, data.NumArrayElements, data.IsCubeMap, false);
			for (var m = 0; m < data.NumMips; m++)
			{
				for (var f = 0; f < data.NumFaces; f++)
				{
					for (var a = 0; a < data.NumArrayElements; a++)
					{
						var converted =
							ColorExtensions.InternalConvertToAsBytesFromBytes(
								data.Mips[m][(CubeMapFaceDirection)f, a].Data,
								data.Format,
								newFormat);

						newData.Mips[m][(CubeMapFaceDirection)f, a].Data = converted;
					}
				}
			}

			return newData;
		}
	}
}
