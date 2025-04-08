using System;
using BCnEncoder.Shared;
using Xunit;
using Vector4 = System.Numerics.Vector4;

namespace BCnEncTests.Helpers
{
	public class PcaTests
	{
		[Fact]
		public void PrincipalAxisTest()
		{
			var testData = new[] {
				new Vector4(0.4f, 0.3f, 0.5f, 0.1f),
				new Vector4(0.3f, 0.3f, 0.5f, 0.3f),
				new Vector4(0.5f, 0.3f, 0.6f, 0.2f),
				new Vector4(0.2f, 0.3f, 0.6f, 0.2f),
				new Vector4(0.456f, 0.34f, 0.23f, 0.45f)
			};

			var refMean = new[] { 0.3712, 0.308, 0.48600000000000004, 0.25 };
			var refCovar = new[] {
				new[] {0.014747200000000002, 0.00084800000000000088, -0.0067839999999999992, 0.0028000000000000004},
				new[] {0.00084800000000000088, 0.00032000000000000062, -0.0025600000000000024, 0.0020000000000000018},
				new[] {-0.0067839999999999992, -0.0025600000000000024, 0.022979999999999993, -0.015999999999999997},
				new[] {0.0028000000000000004, 0.0020000000000000018, -0.015999999999999997, 0.0174999}
			};

			var covarianceMatrix = PcaVectors.CalculateCovariance(testData, out var mean);
			var pa = PcaVectors.CalculatePrincipalAxis(covarianceMatrix);

			var refPa = new[] { 0.282, 0.087, -0.744, 0.603 };

			Assert.Equal(refMean[0], mean.X, 2);
			Assert.Equal(refMean[1], mean.Y, 2);
			Assert.Equal(refMean[2], mean.Z, 2);
			Assert.Equal(refMean[3], mean.W, 2);

			Assert.Equal(refCovar[0][0], covarianceMatrix.M11, 4);
			Assert.Equal(refCovar[0][1], covarianceMatrix.M12, 4);
			Assert.Equal(refCovar[0][2], covarianceMatrix.M13, 4);
			Assert.Equal(refCovar[0][3], covarianceMatrix.M14, 4);

			Assert.Equal(refCovar[1][0], covarianceMatrix.M21, 4);
			Assert.Equal(refCovar[1][1], covarianceMatrix.M22, 4);
			Assert.Equal(refCovar[1][2], covarianceMatrix.M23, 4);
			Assert.Equal(refCovar[1][3], covarianceMatrix.M24, 4);

			Assert.Equal(refCovar[2][0], covarianceMatrix.M31, 4);
			Assert.Equal(refCovar[2][1], covarianceMatrix.M32, 4);
			Assert.Equal(refCovar[2][2], covarianceMatrix.M33, 4);
			Assert.Equal(refCovar[2][3], covarianceMatrix.M34, 4);

			Assert.Equal(refCovar[3][0], covarianceMatrix.M41, 4);
			Assert.Equal(refCovar[3][1], covarianceMatrix.M42, 4);
			Assert.Equal(refCovar[3][2], covarianceMatrix.M43, 4);
			Assert.Equal(refCovar[3][3], covarianceMatrix.M44, 4);

			Assert.True(Math.Abs(pa.X - refPa[0]) < 0.1f, $"actual: {pa.X} expected: {refPa[0]}");
			Assert.True(Math.Abs(pa.Y - refPa[1]) < 0.1f, $"actual: {pa.Y} expected: {refPa[1]}");
			Assert.True(Math.Abs(pa.Z - refPa[2]) < 0.1f, $"actual: {pa.Z} expected: {refPa[2]}");
			Assert.True(Math.Abs(pa.W - refPa[3]) < 0.1f, $"actual: {pa.W} expected: {refPa[3]}");
		}
	}
}
