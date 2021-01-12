using System;
using SixLabors.ImageSharp.PixelFormats;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace BCnEncoder.Shared
{
	internal static class PcaVectors
	{
		private const int C5655Mask_ = 0xF8;
		private const int C5656Mask_ = 0xFC;

		private static void ConvertToVector4(ReadOnlySpan<Rgba32> colors, Span<Vector4> vectors)
		{
			for (var i = 0; i < colors.Length; i++)
			{
				vectors[i].X += colors[i].R / 255f;
				vectors[i].Y += colors[i].G / 255f;
				vectors[i].Z += colors[i].B / 255f;
				vectors[i].W += colors[i].A / 255f;
			}
		}

		private static Vector4 CalculateMean(Span<Vector4> colors)
		{

			float r = 0;
			float g = 0;
			float b = 0;
			float a = 0;

			for (var i = 0; i < colors.Length; i++)
			{
				r += colors[i].X;
				g += colors[i].Y;
				b += colors[i].Z;
				a += colors[i].W;
			}

			return new Vector4(
				r / colors.Length,
				g / colors.Length,
				b / colors.Length,
				a / colors.Length
				);
		}

		internal static Matrix4x4 CalculateCovariance(Span<Vector4> values, out Vector4 mean) {
			mean = CalculateMean(values);
			for (var i = 0; i < values.Length; i++)
			{
				values[i] -= mean;
			}

			//4x4 matrix
			var mat = new Matrix4x4();

			for (var i = 0; i < values.Length; i++)
			{
				mat.M11 += values[i].X * values[i].X;
				mat.M12 += values[i].X * values[i].Y;
				mat.M13 += values[i].X * values[i].Z;
				mat.M14 += values[i].X * values[i].W;
				
				mat.M22 += values[i].Y * values[i].Y;
				mat.M23 += values[i].Y * values[i].Z;
				mat.M24 += values[i].Y * values[i].W;
				
				mat.M33 += values[i].Z * values[i].Z;
				mat.M34 += values[i].Z * values[i].W;
				
				mat.M44 += values[i].W * values[i].W;
			}

			mat = Matrix4x4.Multiply(mat, 1f / (values.Length - 1));
			
			mat.M21 = mat.M12;
			mat.M31 = mat.M13;
			mat.M32 = mat.M23;
			mat.M41 = mat.M14;
			mat.M42 = mat.M24;
			mat.M43 = mat.M34;

			return mat;
		}

		/// <summary>
		/// Calculate principal axis with the power-method
		/// </summary>
		/// <param name="covarianceMatrix"></param>
		/// <returns></returns>
		internal static Vector4 CalculatePrincipalAxis(Matrix4x4 covarianceMatrix) {
			var lastDa = Vector4.UnitY;

			for (var i = 0; i < 30; i++) {
				var dA = Vector4.Transform(lastDa, covarianceMatrix);

				if(dA.LengthSquared() == 0) {
					break;
				}

				dA = Vector4.Normalize(dA);
				if (Vector4.Dot(lastDa, dA) > 0.999999) {
					lastDa = dA;
					break;
				}
				else {
					lastDa = dA;
				}
			}
			return lastDa;
		}

		public static void Create(Span<Rgba32> colors, out Vector3 mean, out Vector3 principalAxis)
		{
			Span<Vector4> vectors = stackalloc Vector4[colors.Length];
			ConvertToVector4(colors, vectors);
			

			var cov = CalculateCovariance(vectors, out var v4Mean);
			mean = new Vector3(v4Mean.X, v4Mean.Y, v4Mean.Z);

			var pa = CalculatePrincipalAxis(cov);
			principalAxis = new Vector3(pa.X, pa.Y, pa.Z);
			if (principalAxis.LengthSquared() == 0) {
				principalAxis = Vector3.UnitY;
			}
			else {
				principalAxis = Vector3.Normalize(principalAxis);
			}

		}

		public static void CreateWithAlpha(Span<Rgba32> colors, out Vector4 mean, out Vector4 principalAxis)
		{
			Span<Vector4> vectors = stackalloc Vector4[colors.Length];
			ConvertToVector4(colors, vectors);
			
			var cov = CalculateCovariance(vectors, out mean);
			principalAxis = CalculatePrincipalAxis(cov);
		}


		public static void GetExtremePoints(Span<Rgba32> colors, Vector3 mean, Vector3 principalAxis, out ColorRgb24 min,
			out ColorRgb24 max)
		{

			float minD = 0;
			float maxD = 0;

			for (var i = 0; i < colors.Length; i++)
			{
				var colorVec = new Vector3(colors[i].R / 255f, colors[i].G / 255f, colors[i].B / 255f);

				var v = colorVec - mean;
				var d = Vector3.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			var minVec = mean + principalAxis * minD;
			var maxVec = mean + principalAxis * maxD;

			var minR = (int) (minVec.X * 255);
			var minG = (int) (minVec.Y * 255);
			var minB = (int) (minVec.Z * 255);

			var maxR = (int) (maxVec.X * 255);
			var maxG = (int) (maxVec.Y * 255);
			var maxB = (int) (maxVec.Z * 255);

			minR = minR >= 0 ? minR : 0;
			minG = minG >= 0 ? minG : 0;
			minB = minB >= 0 ? minB : 0;

			maxR = maxR <= 255 ? maxR : 255;
			maxG = maxG <= 255 ? maxG : 255;
			maxB = maxB <= 255 ? maxB : 255;

			min = new ColorRgb24((byte)minR, (byte)minG, (byte)minB);
			max = new ColorRgb24((byte)maxR, (byte)maxG, (byte)maxB);
		}

		public static void GetMinMaxColor565(Span<Rgba32> colors, Vector3 mean, Vector3 principalAxis, 
			out ColorRgb565 min, out ColorRgb565 max)
		{

			float minD = 0;
			float maxD = 0;

			for (var i = 0; i < colors.Length; i++)
			{
				var colorVec = new Vector3(colors[i].R / 255f, colors[i].G / 255f, colors[i].B / 255f);

				var v = colorVec - mean;
				var d = Vector3.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			//Inset
			minD *= 15 / 16f;
			maxD *= 15 / 16f;

			var minVec = mean + principalAxis * minD;
			var maxVec = mean + principalAxis * maxD;

			var minR = (int) (minVec.X * 255);
			var minG = (int) (minVec.Y * 255);
			var minB = (int) (minVec.Z * 255);

			var maxR = (int) (maxVec.X * 255);
			var maxG = (int) (maxVec.Y * 255);
			var maxB = (int) (maxVec.Z * 255);

			minR = minR >= 0 ? minR : 0;
			minG = minG >= 0 ? minG : 0;
			minB = minB >= 0 ? minB : 0;

			maxR = maxR <= 255 ? maxR : 255;
			maxG = maxG <= 255 ? maxG : 255;
			maxB = maxB <= 255 ? maxB : 255;

			// Optimal round
			minR = (minR & C5655Mask_) | (minR >> 5);
			minG = (minG & C5656Mask_) | (minG >> 6);
			minB = (minB & C5655Mask_) | (minB >> 5);

			maxR = (maxR & C5655Mask_) | (maxR >> 5);
			maxG = (maxG & C5656Mask_) | (maxG >> 6);
			maxB = (maxB & C5655Mask_) | (maxB >> 5);

			min = new ColorRgb565((byte)minR, (byte)minG, (byte)minB);
			max = new ColorRgb565((byte)maxR, (byte)maxG, (byte)maxB);

		}

		public static void GetExtremePointsWithAlpha(Span<Rgba32> colors, Vector4 mean, Vector4 principalAxis, out Vector4 min,
			out Vector4 max)
		{

			float minD = 0;
			float maxD = 0;

			for (var i = 0; i < colors.Length; i++)
			{
				var colorVec = new Vector4(colors[i].R / 255f, colors[i].G / 255f, colors[i].B / 255f, colors[i].A / 255f);

				var v = colorVec - mean;
				var d = Vector4.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			min = mean + principalAxis * minD;
			max = mean + principalAxis * maxD;
		}

		public static void GetOptimizedEndpoints565(Span<Rgba32> colors, Vector3 mean, Vector3 principalAxis, out ColorRgb565 min, out ColorRgb565 max,
			float rWeight = 0.3f, float gWeight = 0.6f, float bWeight = 0.1f)
		{
			var length = colors.Length;
			var vectorColors = new Vector3[length];
			for (var i = 0; i < colors.Length; i++)
			{
				vectorColors[i] = new Vector3(colors[i].R / 255f, colors[i].G / 255f, colors[i].B / 255f);
			}

			float minD = 0;
			float maxD = 0;

			Vector3 Clamp565(Vector3 vec)
			{
				if (vec.X < 0) vec.X = 0;
				if (vec.X > 31) vec.X = 31;
				if (vec.Y < 0) vec.Y = 0;
				if (vec.Y > 63) vec.Y = 63;
				if (vec.Z < 0) vec.Z = 0;
				if (vec.Z > 31) vec.Z = 31;
				return new Vector3(MathF.Round(vec.X), MathF.Round(vec.Y), MathF.Round(vec.Z));
			}

			float Distance(Vector3 v, Vector3 p)
			{
				return (v.X - p.X) * (v.X - p.X) * rWeight
				       + (v.Y - p.Y) * (v.Y - p.Y) * gWeight
				       + (v.Z - p.Z) * (v.Z - p.Z) * bWeight;
				;
			}

			float SelectClosestDistance(Vector3 selector, Vector3 f0, Vector3 f1, Vector3 f2, Vector3 f3)
			{
				var d0 = Distance(selector, f0);
				var d1 = Distance(selector, f1);
				var d2 = Distance(selector, f2);
				var d3 = Distance(selector, f3);

				if (d0 < d1 && d0 < d2 && d0 < d3) return d0;
				if (d1 < d0 && d1 < d2 && d1 < d3) return d1;
				if (d2 < d0 && d2 < d1 && d2 < d3) return d2;
				else return d3;
			}

			Vector3 endPoint0;
			Vector3 endPoint1;

			double CalculateError()
			{
				double cumulativeError = 0;
				var ep0 = new Vector3(endPoint0.X / 31, endPoint0.Y / 63, endPoint0.Z / 31);
				var ep1 = new Vector3(endPoint1.X / 31, endPoint1.Y / 63, endPoint1.Z / 31);
				var ep2 = ep0 + (ep1 - ep0) * 1 / 3f;
				var ep3 = ep0 + (ep1 - ep0) * 2 / 3f;

				for (var i = 0; i < length; i++)
				{
					double distance = SelectClosestDistance(vectorColors[i], ep0, ep1, ep2, ep3);
					cumulativeError += distance;
				}
				return cumulativeError;
			}


			for (var i = 0; i < vectorColors.Length; i++)
			{
				var d = ProjectPointOnLine(vectorColors[i], mean, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			endPoint0 = mean + principalAxis * minD;
			endPoint1 = mean + principalAxis * maxD;

			endPoint0 = new Vector3(MathF.Round(endPoint0.X * 31), MathF.Round(endPoint0.Y * 63), MathF.Round(endPoint0.Z * 31));
			endPoint1 = new Vector3(MathF.Round(endPoint1.X * 31), MathF.Round(endPoint1.Y * 63), MathF.Round(endPoint1.Z * 31));
			endPoint0 = Clamp565(endPoint0);
			endPoint1 = Clamp565(endPoint1);

			var best = CalculateError();
			var increment = 5;
			var foundBetter = true;
			var rounds = 0;
			// Variate color and look for better endpoints
			while (increment > 1 || foundBetter)
			{
				rounds++;
				foundBetter = false;
				{ // decrement ep0
					var prev = endPoint0;
					endPoint0 -= principalAxis * increment * 2;
					endPoint0 = Clamp565(endPoint0);
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0 = prev;
					}
				}

				{ // decrement ep1
					var prev = endPoint1;
					endPoint1 -= principalAxis * increment * 2;
					endPoint1 = Clamp565(endPoint1);
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1 = prev;
					}
				}

				{ // increment ep0
					var prev = endPoint0;
					endPoint0 += principalAxis * increment * 2;
					endPoint0 = Clamp565(endPoint0);
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0 = prev;
					}
				}

				{ // increment ep1
					var prev = endPoint1;
					endPoint1 += principalAxis * increment * 2;
					endPoint1 = Clamp565(endPoint1);
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1 = prev;
					}
				}

				{ // scaleUp 
					var prev0 = endPoint0;
					var prev1 = endPoint1;
					endPoint0 -= principalAxis * increment * 2;
					endPoint1 += principalAxis * increment * 2;
					endPoint0 = Clamp565(endPoint0);
					endPoint1 = Clamp565(endPoint1);
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0 = prev0;
						endPoint1 = prev1;
					}
				}

				{ // scaleDown
					var prev0 = endPoint0;
					var prev1 = endPoint1;
					endPoint0 += principalAxis * increment * 2;
					endPoint1 -= principalAxis * increment * 2;
					endPoint0 = Clamp565(endPoint0);
					endPoint1 = Clamp565(endPoint1);
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0 = prev0;
						endPoint1 = prev1;
					}
				}

				#region G
				if (endPoint0.Y - increment >= 0)
				{ // decrement ep0 G
					var prevY = endPoint0.Y;
					endPoint0.Y -= increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.Y = prevY;
					}
				}

				if (endPoint1.Y - increment >= 0)
				{ // decrement ep1 G
					var prevY = endPoint1.Y;
					endPoint1.Y -= increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.Y = prevY;
					}
				}

				if (foundBetter && increment > 1)
				{
					increment--;
				}

				if (endPoint1.Y + increment <= 63)
				{ // increment ep1 G
					var prevY = endPoint1.Y;
					endPoint1.Y += increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.Y = prevY;
					}
				}

				if (endPoint0.Y + increment <= 63)
				{ // increment ep0 G
					var prevY = endPoint0.Y;
					endPoint0.Y += increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.Y = prevY;
					}
				}

				#endregion

				#region R
				if (endPoint0.X - increment >= 0)
				{ // decrement ep0 R
					var prevX = endPoint0.X;
					endPoint0.X -= increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.X = prevX;
					}
				}

				if (endPoint1.X - increment >= 0)
				{ // decrement ep1 R
					var prevX = endPoint1.X;
					endPoint1.X -= increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.X = prevX;
					}
				}

				if (foundBetter && increment > 1)
				{
					increment--;
				}

				if (endPoint1.X + increment <= 31)
				{ // increment ep1 R
					var prevX = endPoint1.X;
					endPoint1.X += increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.X = prevX;
					}
				}

				if (endPoint0.X + increment <= 31)
				{ // increment ep0 R
					var prevX = endPoint0.X;
					endPoint0.X += increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.X = prevX;
					}
				}
				#endregion

				#region B

				if (endPoint0.Z - increment >= 0)
				{ // decrement ep0 B
					var prevZ = endPoint0.Z;
					endPoint0.Z -= increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.Z = prevZ;
					}
				}

				if (endPoint1.Z - increment >= 0)
				{ // decrement ep1 B
					var prevZ = endPoint1.Z;
					endPoint1.Z -= increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.Z = prevZ;
					}
				}

				if (foundBetter && increment > 1)
				{
					increment--;
				}

				if (endPoint1.Z + increment <= 31)
				{ // increment ep1 B
					var prevZ = endPoint1.Z;
					endPoint1.Z += increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.Z = prevZ;
					}
				}

				if (endPoint0.Z + increment <= 31)
				{ // increment ep0 B
					var prevZ = endPoint0.Z;
					endPoint0.Z += increment;
					var error = CalculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.Z = prevZ;
					}
				}

				#endregion

				endPoint0 = Clamp565(endPoint0);
				endPoint1 = Clamp565(endPoint1);

				if (!foundBetter && increment > 1)
				{
					increment--;
				}
			}

			min = new ColorRgb565();
			min.RawR = (int)endPoint0.X;
			min.RawG = (int)endPoint0.Y;
			min.RawB = (int)endPoint0.Z;

			max = new ColorRgb565();
			max.RawR = (int)endPoint1.X;
			max.RawG = (int)endPoint1.Y;
			max.RawB = (int)endPoint1.Z;
		}

		private static float ProjectPointOnLine(Vector3 point, Vector3 linePoint, Vector3 lineDir)
		{
			var v = point - linePoint;
			var d = Vector3.Dot(v, lineDir);
			return d;
		}
	}
}
