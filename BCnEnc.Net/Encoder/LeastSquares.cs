using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BCnEncoder.Encoder.Bptc;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	/// <summary>
	/// Least squares optimization from https://github.com/knarkowicz/GPURealTimeBC6H
	/// Code is public domain
	/// </summary>
	internal static class LeastSquares
	{

		private static int ComputeIndex4(float texelPos, float endPoint0Pos, float endPoint1Pos)
		{
			var r = (texelPos - endPoint0Pos) / (endPoint1Pos - endPoint0Pos);
			return (int)Math.Clamp(r * 15f /*14.93333f + 0.03333f + 0.5f*/, 0.0f, 15.0f);
		}

		private static int ComputeIndex3(float texelPos, float endPoint0Pos, float endPoint1Pos)
		{
			float r = (texelPos - endPoint0Pos) / (endPoint1Pos - endPoint0Pos);
			return (int)Math.Clamp(r * 6.98182f + 0.00909f + 0.5f, 0.0f, 7.0f);
		}

		private static float F32ToF16(float f32)
		{
			return Half.GetBits(new Half(f32));
		}

		private static Vector3 F32ToF16(Vector3 f32)
		{
			return new Vector3(
				F32ToF16(f32.X),
				F32ToF16(f32.Y),
				F32ToF16(f32.Z)
			);
		}

		private static float F16ToF32(float f16)
		{
			return Half.ToHalf((ushort)f16);
		}

		private static Vector3 F16ToF32(Vector3 f16)
		{
			return new Vector3(
				F16ToF32(f16.X),
				F16ToF32(f16.Y),
				F16ToF32(f16.Z)
			);
		}

		public static ((int, int, int), (int, int, int)) OptimizeEndpoints1SubInt(RawBlock4X4RgbHalfInt block, ref ColorRgbFloat ep0, ref ColorRgbFloat ep1, bool signed)
		{
			var ep0i = Bc6EncodingHelpers.PreQuantizeRawEndpoint(ep0, signed);
			var ep1i = Bc6EncodingHelpers.PreQuantizeRawEndpoint(ep1, signed);

			var ep0v = new Vector3(ep0i.Item1, ep0i.Item2, ep0i.Item3);
			var ep1v = new Vector3(ep1i.Item1, ep1i.Item2, ep1i.Item3);
			
			var pixels = block.AsSpan;

			var blockDir = ep1v - ep0v;
			blockDir = blockDir / (blockDir.X + blockDir.Y + blockDir.Z);

			var endPoint0Pos = Vector3.Dot(ep0v, blockDir);
			var endPoint1Pos = Vector3.Dot(ep1v, blockDir);

			var alphaTexelSum = new Vector3();
			var betaTexelSum = new Vector3();
			var alphaBetaSum = 0.0f;
			var alphaSqSum = 0.0f;
			var betaSqSum = 0.0f;

			for (var i = 0; i < 16; i++)
			{
				var pixel = new Vector3(
					pixels[i].Item1,
					pixels[i].Item2,
					pixels[i].Item3
					);
				var texelPos = Vector3.Dot(pixel, blockDir);
				var texelIndex = ComputeIndex4(texelPos, endPoint0Pos, endPoint1Pos);

				var beta = Math.Clamp(texelIndex / 15.0f, 0f, 1f);
				var alpha = 1.0f - beta;

				var texelF16 = pixel;
				alphaTexelSum += alpha * texelF16;
				betaTexelSum += beta * texelF16;

				alphaBetaSum += alpha * beta;

				alphaSqSum += alpha * alpha;
				betaSqSum += beta * beta;
			}

			var det = alphaSqSum * betaSqSum - alphaBetaSum * alphaBetaSum;

			if (MathF.Abs(det) > 0.00001f)
			{
				var detRcp = 1f / (det);
				var ep0o = Vector3.Clamp(
					detRcp * (alphaTexelSum * betaSqSum - betaTexelSum * alphaBetaSum), new Vector3(0.0f),
					new Vector3(Half.GetBits(Half.MaxValue)));
				var ep1o = Vector3.Clamp(
					detRcp * (betaTexelSum * alphaSqSum - alphaTexelSum * alphaBetaSum), new Vector3(0.0f),
					new Vector3(Half.GetBits(Half.MaxValue)));

				return ((
						(int)ep0o.X,
						(int)ep0o.Y,
						(int)ep0o.Z),
					(
						(int)ep1o.X,
						(int)ep1o.Y,
						(int)ep1o.Z));
			}

			return (ep0i, ep1i);
		}

		public static void OptimizeEndpoints1Sub(RawBlock4X4RgbFloat block, ref ColorRgbFloat ep0, ref ColorRgbFloat ep1)
		{
			var ep0v = ep0.ToVector3();
			var ep1v = ep1.ToVector3();

			var pixels = block.AsSpan;

			var blockDir = ep1v - ep0v;
			blockDir = Vector3.Normalize(blockDir);

			var endPoint0Pos = Vector3.Dot(ep0v, blockDir);
			var endPoint1Pos = Vector3.Dot(ep1v, blockDir);

			var alphaTexelSum = new Vector3();
			var betaTexelSum = new Vector3();
			var alphaBetaSum = 0.0f;
			var alphaSqSum = 0.0f;
			var betaSqSum = 0.0f;

			for (var i = 0; i < 16; i++)
			{
				var texelPos = Vector3.Dot(pixels[i].ToVector3(), blockDir);
				var texelIndex = ComputeIndex4(texelPos, endPoint0Pos, endPoint1Pos);

				var beta = Math.Clamp(texelIndex / 15.0f, 0f, 1f);
				var alpha = 1.0f - beta;

				var texelF16 = pixels[i].ToVector3();
				alphaTexelSum += alpha * texelF16;
				betaTexelSum += beta * texelF16;

				alphaBetaSum += alpha * beta;

				alphaSqSum += alpha * alpha;
				betaSqSum += beta * beta;
			}

			var det = alphaSqSum * betaSqSum - alphaBetaSum * alphaBetaSum;

			if (MathF.Abs(det) > 0.00001f)
			{
				var detRcp = 1f / (det);
				ep0 = new ColorRgbFloat(Vector3.Clamp(
					detRcp * (alphaTexelSum * betaSqSum - betaTexelSum * alphaBetaSum), new Vector3(0.0f),
					new Vector3(Half.MaxValue)));
				ep1 = new ColorRgbFloat(Vector3.Clamp(
					detRcp * (betaTexelSum * alphaSqSum - alphaTexelSum * alphaBetaSum), new Vector3(0.0f),
					new Vector3(Half.MaxValue)));
			}
		}

		public static void OptimizeEndpoints2Sub(RawBlock4X4RgbFloat block, ref ColorRgbFloat ep0, ref ColorRgbFloat ep1, int partitionSetId, int subsetIndex)
		{
			var ep0v = ep0.ToVector3();
			var ep1v = ep1.ToVector3();

			var pixels = block.AsSpan;

			var blockDir = ep1v - ep0v;
			blockDir = Vector3.Normalize(blockDir);

			var endPoint0Pos = Vector3.Dot(ep0v, blockDir);
			var endPoint1Pos = Vector3.Dot(ep1v, blockDir);

			var alphaTexelSum = new Vector3();
			var betaTexelSum = new Vector3();
			var alphaBetaSum = 0.0f;
			var alphaSqSum = 0.0f;
			var betaSqSum = 0.0f;

			for (var i = 0; i < 16; i++)
			{
				if (Bc6Block.Subsets2PartitionTable[partitionSetId][i] == subsetIndex)
				{
					var texelPos = Vector3.Dot(pixels[i].ToVector3(), blockDir);
					var texelIndex = ComputeIndex3(texelPos, endPoint0Pos, endPoint1Pos);

					var beta = Math.Clamp(texelIndex / 7.0f, 0f, 1f);
					var alpha = 1.0f - beta;

					var texelF16 = pixels[i].ToVector3();
					alphaTexelSum += alpha * texelF16;
					betaTexelSum += beta * texelF16;

					alphaBetaSum += alpha * beta;

					alphaSqSum += alpha * alpha;
					betaSqSum += beta * beta;
				}
			}

			var det = alphaSqSum * betaSqSum - alphaBetaSum * alphaBetaSum;

			if (MathF.Abs(det) > 0.00001f)
			{
				var detRcp = 1f / (det);
				ep0 = new ColorRgbFloat(Vector3.Clamp(
					detRcp * (alphaTexelSum * betaSqSum - betaTexelSum * alphaBetaSum), new Vector3(0.0f),
					new Vector3(Half.MaxValue)));
				ep1 = new ColorRgbFloat(Vector3.Clamp(
					detRcp * (betaTexelSum * alphaSqSum - alphaTexelSum * alphaBetaSum), new Vector3(0.0f),
					new Vector3(Half.MaxValue)));
			}
		}
	}
}
