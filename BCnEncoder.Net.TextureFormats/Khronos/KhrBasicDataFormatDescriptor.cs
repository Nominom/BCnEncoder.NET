using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BCnEncoder.TextureFormats
{
	// From https://www.khronos.org/registry/DataFormat/specs/1.3/dataformat.1.3.html
	public class KhrDataFormatDescriptor
	{
		internal const int KHR_DF_WORD_SAMPLESTART = 6;
		internal const int KHR_DF_WORD_SAMPLEWORDS = 4;
		public uint[] data;

		public KhrDfVendorId VendorId
		{
			get => (KhrDfVendorId)AccessGet(KhrDataFormatDescriptorAccessor.VendorId);
			set => AccessSet(KhrDataFormatDescriptorAccessor.VendorId, (uint)value);
		}
		public KhrDfKhrDescriptorType DescriptorType
		{
			get => (KhrDfKhrDescriptorType)AccessGet(KhrDataFormatDescriptorAccessor.DescriptorType);
			set => AccessSet(KhrDataFormatDescriptorAccessor.DescriptorType, (uint)value);
		}
		public uint VersionNumber
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.VersionNumber);
			set => AccessSet(KhrDataFormatDescriptorAccessor.VersionNumber, value);
		}
		public uint DescriptorBlockSize
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.DescriptorBlockSize);
			set => AccessSet(KhrDataFormatDescriptorAccessor.DescriptorBlockSize, value);
		}

		internal uint AccessGet(in KhrDataFormatDescriptorAccessor accessor)
		{
			return (data[accessor.offset] >> accessor.shift) & accessor.mask;
		}
		internal void AccessSet(in KhrDataFormatDescriptorAccessor accessor, uint value)
		{
			var u = data[accessor.offset];
			u &= ~(accessor.mask << accessor.shift);
			u |= (value & accessor.mask) << accessor.shift;
			data[accessor.offset] = u;
		}

		public void Write(BinaryWriter writer)
		{
			Debug.Assert(data.Length * 4 == DescriptorBlockSize, "Data length did not match DescriptorBlockSize");
			foreach (var u in data)
			{
				writer.Write(u);
			}
		}

		public static KhrDataFormatDescriptor Read(BinaryReader reader)
		{
			var descriptor = new KhrDataFormatDescriptor();
			// Read first two bytes
			descriptor.data = new uint[2];
			descriptor.data[0] = reader.ReadUInt32();
			descriptor.data[1] = reader.ReadUInt32();

			var oldData = descriptor.data;

			// Basic descriptor
			if (descriptor.VendorId == KhrDfVendorId.KhrDfVendoridKhronos &&
			    descriptor.DescriptorType == KhrDfKhrDescriptorType.KhrDfKhrDescriptortypeBasicformat)
			{
				var numSamples = (int)(((descriptor.DescriptorBlockSize >> 2) - KHR_DF_WORD_SAMPLESTART) / KHR_DF_WORD_SAMPLEWORDS);

				descriptor = new KhrBasicDataFormatDescriptor(numSamples);
				oldData.CopyTo(descriptor.data.AsSpan());
			}
			// Unknown descriptor
			else
			{
				var dataLength = (int)(descriptor.DescriptorBlockSize / 4);
				var newData = new uint[dataLength];
				oldData.CopyTo(newData.AsSpan());
				descriptor.data = newData;
			}

			// Read rest of descriptor
			for (var i = 2; i < descriptor.data.Length; i++)
			{
				descriptor.data[i] = reader.ReadUInt32();
			}

			return descriptor;
		}
	}

	public class KhrBasicDataFormatDescriptor : KhrDataFormatDescriptor
	{
		public KhrBasicDataFormatDescriptor(int numSamples)
		{
			data = new uint[KHR_DF_WORD_SAMPLESTART + numSamples * KHR_DF_WORD_SAMPLEWORDS];
			DescriptorBlockSize = 4U * (uint)data.Length;
			VersionNumber = 2;
			DescriptorType = KhrDfKhrDescriptorType.KhrDfKhrDescriptortypeBasicformat;
			VendorId = KhrDfVendorId.KhrDfVendoridKhronos;
		}

		public class SampleAccessor
		{
			internal KhrBasicDataFormatDescriptor descriptor;
			internal int sampleIdx;
			internal SampleAccessor(KhrBasicDataFormatDescriptor descriptor, int sampleIdx)
			{
				this.descriptor = descriptor;
				this.sampleIdx = sampleIdx;
			}

			public uint BitOffset
			{
				get => descriptor.AccessSampleGet(sampleIdx, KhrDataFormatDescriptorAccessor.SampleBitOffset);
				set => descriptor.AccessSampleSet(sampleIdx, KhrDataFormatDescriptorAccessor.SampleBitOffset, value);
			}
			/// <summary>
			/// This is actual BitLength - 1 (eg. 7 here is actually 8)
			/// </summary>
			public uint BitLength
			{
				get => descriptor.AccessSampleGet(sampleIdx, KhrDataFormatDescriptorAccessor.SampleBitLength);
				set => descriptor.AccessSampleSet(sampleIdx, KhrDataFormatDescriptorAccessor.SampleBitLength, value);
			}

			/// <summary>
			/// Bottom 4 bits are <see cref="KhrDfModelChannels"/>,
			/// upper 4 bits are <see cref="KhrDfSampleDatatypeQualifiers"/>
			/// </summary>
			public uint ChannelType
			{
				get => descriptor.AccessSampleGet(sampleIdx, KhrDataFormatDescriptorAccessor.SampleChannelType);
				set => descriptor.AccessSampleSet(sampleIdx, KhrDataFormatDescriptorAccessor.SampleChannelType, value);
			}

			public KhrDfModelChannels ChannelTypeChannel => (KhrDfModelChannels)(ChannelType & 0xFU);
			public KhrDfSampleDatatypeQualifiers ChannelTypeDataType => (KhrDfSampleDatatypeQualifiers)(ChannelType & 0xF0U);

			public uint Position0
			{
				get => descriptor.AccessSampleGet(sampleIdx, KhrDataFormatDescriptorAccessor.SamplePosition0);
				set => descriptor.AccessSampleSet(sampleIdx, KhrDataFormatDescriptorAccessor.SamplePosition0, value);
			}
			public uint Position1
			{
				get => descriptor.AccessSampleGet(sampleIdx, KhrDataFormatDescriptorAccessor.SamplePosition1);
				set => descriptor.AccessSampleSet(sampleIdx, KhrDataFormatDescriptorAccessor.SamplePosition1, value);
			}
			public uint Position2
			{
				get => descriptor.AccessSampleGet(sampleIdx, KhrDataFormatDescriptorAccessor.SamplePosition2);
				set => descriptor.AccessSampleSet(sampleIdx, KhrDataFormatDescriptorAccessor.SamplePosition2, value);
			}
			public uint Position3
			{
				get => descriptor.AccessSampleGet(sampleIdx, KhrDataFormatDescriptorAccessor.SamplePosition3);
				set => descriptor.AccessSampleSet(sampleIdx, KhrDataFormatDescriptorAccessor.SamplePosition3, value);
			}
			public uint SampleLower
			{
				get => descriptor.AccessSampleGet(sampleIdx, KhrDataFormatDescriptorAccessor.SampleLower);
				set => descriptor.AccessSampleSet(sampleIdx, KhrDataFormatDescriptorAccessor.SampleLower, value);
			}
			public uint SampleUpper
			{
				get => descriptor.AccessSampleGet(sampleIdx, KhrDataFormatDescriptorAccessor.SampleUpper);
				set => descriptor.AccessSampleSet(sampleIdx, KhrDataFormatDescriptorAccessor.SampleUpper, value);
			}
		}

		public struct SampleArrayAccessor
		{
			internal KhrBasicDataFormatDescriptor descriptor;

			internal SampleArrayAccessor(KhrBasicDataFormatDescriptor descriptor)
			{
				this.descriptor = descriptor;
			}

			public SampleAccessor this[int index] => new SampleAccessor(descriptor, index);
		}

		

		public SampleArrayAccessor Samples => new SampleArrayAccessor(this);
		public uint NumSamples => (((DescriptorBlockSize >> 2) - KHR_DF_WORD_SAMPLESTART) / KHR_DF_WORD_SAMPLEWORDS);

		public KhrDfModel ColorModel
		{
			get => (KhrDfModel)AccessGet(KhrDataFormatDescriptorAccessor.ColorModel);
			set => AccessSet(KhrDataFormatDescriptorAccessor.ColorModel, (uint)value);
		}
		public KhrDfPrimaries ColorPrimaries
		{
			get => (KhrDfPrimaries)AccessGet(KhrDataFormatDescriptorAccessor.ColorPrimaries);
			set => AccessSet(KhrDataFormatDescriptorAccessor.ColorPrimaries, (uint)value);
		}
		public KhrDfTransfer TransferFunction
		{
			get => (KhrDfTransfer)AccessGet(KhrDataFormatDescriptorAccessor.TransferFunction);
			set => AccessSet(KhrDataFormatDescriptorAccessor.TransferFunction, (uint)value);
		}
		public KhrDfFlags Flags
		{
			get => (KhrDfFlags)AccessGet(KhrDataFormatDescriptorAccessor.Flags);
			set => AccessSet(KhrDataFormatDescriptorAccessor.Flags, (uint)value);
		}
		public uint TexelBlockDimension0
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.TexelBlockDimension0);
			set => AccessSet(KhrDataFormatDescriptorAccessor.TexelBlockDimension0, value);
		}
		public uint TexelBlockDimension1
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.TexelBlockDimension1);
			set => AccessSet(KhrDataFormatDescriptorAccessor.TexelBlockDimension1, value);
		}
		public uint TexelBlockDimension2
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.TexelBlockDimension2);
			set => AccessSet(KhrDataFormatDescriptorAccessor.TexelBlockDimension2, value);
		}
		public uint TexelBlockDimension3
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.TexelBlockDimension3);
			set => AccessSet(KhrDataFormatDescriptorAccessor.TexelBlockDimension3, value);
		}
		public uint BytesPlane0
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.BytesPlane0);
			set => AccessSet(KhrDataFormatDescriptorAccessor.BytesPlane0, value);
		}
		public uint BytesPlane1
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.BytesPlane1);
			set => AccessSet(KhrDataFormatDescriptorAccessor.BytesPlane1, value);
		}
		public uint BytesPlane2
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.BytesPlane2);
			set => AccessSet(KhrDataFormatDescriptorAccessor.BytesPlane2, value);
		}
		public uint BytesPlane3
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.BytesPlane3);
			set => AccessSet(KhrDataFormatDescriptorAccessor.BytesPlane3, value);
		}
		public uint BytesPlane4
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.BytesPlane4);
			set => AccessSet(KhrDataFormatDescriptorAccessor.BytesPlane4, value);
		}
		public uint BytesPlane5
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.BytesPlane5);
			set => AccessSet(KhrDataFormatDescriptorAccessor.BytesPlane5, value);
		}
		public uint BytesPlane6
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.BytesPlane6);
			set => AccessSet(KhrDataFormatDescriptorAccessor.BytesPlane6, value);
		}
		public uint BytesPlane7
		{
			get => AccessGet(KhrDataFormatDescriptorAccessor.BytesPlane7);
			set => AccessSet(KhrDataFormatDescriptorAccessor.BytesPlane7, value);
		}

		private uint AccessSampleGet(int sampleIdx, in KhrDataFormatDescriptorAccessor accessor)
		{
			var offset = KHR_DF_WORD_SAMPLESTART + sampleIdx * KHR_DF_WORD_SAMPLEWORDS + accessor.offset;
			return (data[offset] >> accessor.shift) & accessor.mask;
		}
		private void AccessSampleSet(int sampleIdx, in KhrDataFormatDescriptorAccessor accessor, uint value)
		{
			var offset = KHR_DF_WORD_SAMPLESTART + sampleIdx * KHR_DF_WORD_SAMPLEWORDS + accessor.offset;
			var u = data[offset];
			u &= ~(accessor.mask << accessor.shift);
			u |= (value & accessor.mask) << accessor.shift;
			data[offset] = u;
		}
	}

	internal readonly struct KhrDataFormatDescriptorAccessor
	{
		public readonly uint offset;
		public readonly int shift;
		public readonly uint mask;

		public KhrDataFormatDescriptorAccessor(uint offset, int shift, uint mask)
		{
			this.offset = offset;
			this.shift = shift;
			this.mask = mask;
		}

		public static readonly KhrDataFormatDescriptorAccessor VendorId = new KhrDataFormatDescriptorAccessor
		(
			offset: 0,
			shift: 0,
			mask: 0x1FFFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor DescriptorType = new KhrDataFormatDescriptorAccessor
		(
			offset: 0,
			shift: 17,
			mask: 0x7FFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor VersionNumber = new KhrDataFormatDescriptorAccessor
		(
			offset: 1,
			shift: 0,
			mask: 0xFFFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor DescriptorBlockSize = new KhrDataFormatDescriptorAccessor
		(
			offset: 1,
			shift: 16,
			mask: 0xFFFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor ColorModel = new KhrDataFormatDescriptorAccessor
		(
			offset: 2,
			shift: 0,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor ColorPrimaries = new KhrDataFormatDescriptorAccessor
		(
			offset: 2,
			shift: 8,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor TransferFunction = new KhrDataFormatDescriptorAccessor
		(
			offset: 2,
			shift: 16,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor Flags = new KhrDataFormatDescriptorAccessor
		(
			offset: 2,
			shift: 24,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor TexelBlockDimension0 = new KhrDataFormatDescriptorAccessor
		(
			offset: 3,
			shift: 0,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor TexelBlockDimension1 = new KhrDataFormatDescriptorAccessor
		(
			offset: 3,
			shift: 8,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor TexelBlockDimension2 = new KhrDataFormatDescriptorAccessor
		(
			offset: 3,
			shift: 16,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor TexelBlockDimension3 = new KhrDataFormatDescriptorAccessor
		(
			offset: 3,
			shift: 24,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor BytesPlane0 = new KhrDataFormatDescriptorAccessor
		(
			offset: 4,
			shift: 0,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor BytesPlane1 = new KhrDataFormatDescriptorAccessor
		(
			offset: 4,
			shift: 8,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor BytesPlane2 = new KhrDataFormatDescriptorAccessor
		(
			offset: 4,
			shift: 16,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor BytesPlane3 = new KhrDataFormatDescriptorAccessor
		(
			offset: 4,
			shift: 24,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor BytesPlane4 = new KhrDataFormatDescriptorAccessor
		(
			offset: 5,
			shift: 0,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor BytesPlane5 = new KhrDataFormatDescriptorAccessor
		(
			offset: 5,
			shift: 8,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor BytesPlane6 = new KhrDataFormatDescriptorAccessor
		(
			offset: 5,
			shift: 16,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor BytesPlane7 = new KhrDataFormatDescriptorAccessor
		(
			offset: 5,
			shift: 24,
			mask: 0xFFU
		);

		// Samples
		public static readonly KhrDataFormatDescriptorAccessor SampleBitOffset = new KhrDataFormatDescriptorAccessor
		(
			offset: 0,
			shift: 0,
			mask: 0xFFFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor SampleBitLength = new KhrDataFormatDescriptorAccessor
		(
			offset: 0,
			shift: 16,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor SampleChannelType = new KhrDataFormatDescriptorAccessor
		(
			offset: 0,
			shift: 24,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor SamplePosition0 = new KhrDataFormatDescriptorAccessor
		(
			offset: 1,
			shift: 0,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor SamplePosition1 = new KhrDataFormatDescriptorAccessor
		(
			offset: 1,
			shift: 8,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor SamplePosition2 = new KhrDataFormatDescriptorAccessor
		(
			offset: 1,
			shift: 16,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor SamplePosition3 = new KhrDataFormatDescriptorAccessor
		(
			offset: 1,
			shift: 24,
			mask: 0xFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor SampleLower = new KhrDataFormatDescriptorAccessor
		(
			offset: 2,
			shift: 0,
			mask: 0xFFFFFFFFU
		);
		public static readonly KhrDataFormatDescriptorAccessor SampleUpper = new KhrDataFormatDescriptorAccessor
		(
			offset: 3,
			shift: 0,
			mask: 0xFFFFFFFFU
		);
	}

	public static class DataFormatDescriptorLibrary
	{
		internal const uint FloatNegativeOne = 0xBF800000U; // IEEE 754 floating-point representation for -1.0f
		internal const uint FloatPositiveOne = 0x3F800000U; // IEEE 754 floating-point representation for 1.0f
		//internal const uint FloatMaxNonInf = 0x7F7FFFFFU; // 3.40282e+38
		//internal const uint FloatMinNegNonInf = 0xFF7FFFFFU; // -3.40282e+38
		//internal const uint FloatZero = 0;

		public static KhrBasicDataFormatDescriptor R8
		{
			get
			{
				var d = new KhrBasicDataFormatDescriptor(1)
				{
					BytesPlane0 = 1,
					ColorModel = KhrDfModel.KhrDfModelRgbsda,
					ColorPrimaries = KhrDfPrimaries.KhrDfPrimariesSrgb,
					TransferFunction = KhrDfTransfer.KhrDfTransferLinear
				};
				d.Samples[0].BitOffset = 0;
				d.Samples[0].BitLength = 7;
				d.Samples[0].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaR;
				d.Samples[0].SampleLower = 0;
				d.Samples[0].SampleUpper = 255;

				return d;
			}
		}
		public static KhrBasicDataFormatDescriptor R8G8
		{
			get
			{
				var d = new KhrBasicDataFormatDescriptor(2)
				{
					BytesPlane0 = 2,
					ColorModel = KhrDfModel.KhrDfModelRgbsda,
					ColorPrimaries = KhrDfPrimaries.KhrDfPrimariesSrgb,
					TransferFunction = KhrDfTransfer.KhrDfTransferLinear
				};
				d.Samples[0].BitOffset = 0;
				d.Samples[0].BitLength = 7;
				d.Samples[0].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaR;
				d.Samples[0].SampleLower = 0;
				d.Samples[0].SampleUpper = 255;

				d.Samples[1].BitOffset = 8;
				d.Samples[1].BitLength = 7;
				d.Samples[1].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaG;
				d.Samples[1].SampleLower = 0;
				d.Samples[1].SampleUpper = 255;

				return d;
			}
		}
		public static KhrBasicDataFormatDescriptor Rgb24
		{
			get
			{
				var d = new KhrBasicDataFormatDescriptor(3)
				{
					BytesPlane0 = 3,
					ColorModel = KhrDfModel.KhrDfModelRgbsda,
					ColorPrimaries = KhrDfPrimaries.KhrDfPrimariesSrgb,
					TransferFunction = KhrDfTransfer.KhrDfTransferLinear
				};
				d.Samples[0].BitOffset = 0;
				d.Samples[0].BitLength = 7;
				d.Samples[0].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaR;
				d.Samples[0].SampleLower = 0;
				d.Samples[0].SampleUpper = 255;

				d.Samples[1].BitOffset = 8;
				d.Samples[1].BitLength = 7;
				d.Samples[1].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaG;
				d.Samples[1].SampleLower = 0;
				d.Samples[1].SampleUpper = 255;

				d.Samples[2].BitOffset = 16;
				d.Samples[2].BitLength = 7;
				d.Samples[2].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaB;
				d.Samples[2].SampleLower = 0;
				d.Samples[2].SampleUpper = 255;

				return d;
			}
		}
		public static KhrBasicDataFormatDescriptor Rgba32
		{
			get
			{
				var d = new KhrBasicDataFormatDescriptor(4)
				{
					BytesPlane0 = 4,
					ColorModel = KhrDfModel.KhrDfModelRgbsda,
					ColorPrimaries = KhrDfPrimaries.KhrDfPrimariesSrgb,
					TransferFunction = KhrDfTransfer.KhrDfTransferLinear
				};
				d.Samples[0].BitOffset = 0;
				d.Samples[0].BitLength = 7;
				d.Samples[0].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaR;
				d.Samples[0].SampleLower = 0;
				d.Samples[0].SampleUpper = 255;

				d.Samples[1].BitOffset = 8;
				d.Samples[1].BitLength = 7;
				d.Samples[1].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaG;
				d.Samples[1].SampleLower = 0;
				d.Samples[1].SampleUpper = 255;

				d.Samples[2].BitOffset = 16;
				d.Samples[2].BitLength = 7;
				d.Samples[2].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaB;
				d.Samples[2].SampleLower = 0;
				d.Samples[2].SampleUpper = 255;

				d.Samples[3].BitOffset = 24;
				d.Samples[3].BitLength = 7;
				d.Samples[3].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaA;
				d.Samples[3].SampleLower = 0;
				d.Samples[3].SampleUpper = 255;

				return d;
			}
		}
		public static KhrBasicDataFormatDescriptor Bgra32
		{
			get
			{
				var d = Rgba32;
				// Swap channels
				d.Samples[0].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaB;
				d.Samples[2].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaR;
				return d;
			}
		}
		public static KhrBasicDataFormatDescriptor RgbaFloat
		{
			get
			{
				var d = new KhrBasicDataFormatDescriptor(4)
				{
					BytesPlane0 = 16,
					ColorModel = KhrDfModel.KhrDfModelRgbsda,
					ColorPrimaries = KhrDfPrimaries.KhrDfPrimariesSrgb,
					TransferFunction = KhrDfTransfer.KhrDfTransferLinear
				};
				d.Samples[0].BitOffset = 0;
				d.Samples[0].BitLength = 31;
				d.Samples[0].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaR
				                           | (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypeFloat
				                           | (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypeSigned;
				d.Samples[0].SampleLower = FloatNegativeOne;
				d.Samples[0].SampleUpper = FloatPositiveOne;

				d.Samples[1].BitOffset = 32;
				d.Samples[1].BitLength = 31;
				d.Samples[1].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaG
				                           | (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypeFloat
				                           | (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypeSigned;
				d.Samples[1].SampleLower = FloatNegativeOne;
				d.Samples[1].SampleUpper = FloatPositiveOne;

				d.Samples[2].BitOffset = 64;
				d.Samples[2].BitLength = 31;
				d.Samples[2].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaB
				                           | (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypeFloat
				                           | (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypeSigned;
				d.Samples[2].SampleLower = FloatNegativeOne;
				d.Samples[2].SampleUpper = FloatPositiveOne;

				d.Samples[3].BitOffset = 96;
				d.Samples[3].BitLength = 31;
				d.Samples[3].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaA
				                           | (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypeFloat
				                           | (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypeSigned;
				d.Samples[3].SampleLower = FloatNegativeOne;
				d.Samples[3].SampleUpper = FloatPositiveOne; 

				return d;
			}
		}

		public static KhrBasicDataFormatDescriptor R8G8B8E8 // TODO: ??
		{
			get
			{
				var d = new KhrBasicDataFormatDescriptor(6)
				{
					BytesPlane0 = 4,
					ColorModel = KhrDfModel.KhrDfModelRgbsda,
					ColorPrimaries = KhrDfPrimaries.KhrDfPrimariesSrgb,
					TransferFunction = KhrDfTransfer.KhrDfTransferLinear
				};

				d.Samples[0].BitOffset = 0;
				d.Samples[0].BitLength = 7;
				d.Samples[0].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaR;
				d.Samples[0].SampleLower = 0;
				d.Samples[0].SampleUpper = 255;

				d.Samples[1].BitOffset = 24;
				d.Samples[1].BitLength = 7;
				d.Samples[1].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaR |
				                           (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypexponent;
				d.Samples[1].SampleLower = 128;
				d.Samples[1].SampleUpper = 255;

				d.Samples[2].BitOffset = 8;
				d.Samples[2].BitLength = 7;
				d.Samples[2].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaG;
				d.Samples[2].SampleLower = 0;
				d.Samples[2].SampleUpper = 255;

				d.Samples[3].BitOffset = 24;
				d.Samples[3].BitLength = 7;
				d.Samples[3].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaG |
				                           (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypexponent;
				d.Samples[3].SampleLower = 128;
				d.Samples[3].SampleUpper = 255;

				d.Samples[4].BitOffset = 16;
				d.Samples[4].BitLength = 7;
				d.Samples[4].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaB;
				d.Samples[4].SampleLower = 0;
				d.Samples[4].SampleUpper = 255;

				d.Samples[5].BitOffset = 24;
				d.Samples[5].BitLength = 7;
				d.Samples[5].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaB |
				                           (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypexponent;
				d.Samples[5].SampleLower = 128;
				d.Samples[5].SampleUpper = 255;

				return d;
			}
		}

		public static KhrBasicDataFormatDescriptor R9G9B9E5
		{
			get
			{
				var d = new KhrBasicDataFormatDescriptor(6)
				{
					BytesPlane0 = 4,
					ColorModel = KhrDfModel.KhrDfModelRgbsda,
					ColorPrimaries = KhrDfPrimaries.KhrDfPrimariesSrgb,
					TransferFunction = KhrDfTransfer.KhrDfTransferLinear
				};

				d.Samples[0].BitOffset = 0;
				d.Samples[0].BitLength = 8;
				d.Samples[0].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaR;
				d.Samples[0].SampleLower = 0;
				d.Samples[0].SampleUpper = 256;

				d.Samples[1].BitOffset = 27;
				d.Samples[1].BitLength = 4;
				d.Samples[1].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaR |
				                           (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypexponent;
				d.Samples[1].SampleLower = 15;
				d.Samples[1].SampleUpper = 31;

				d.Samples[2].BitOffset = 9;
				d.Samples[2].BitLength = 8;
				d.Samples[2].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaG;
				d.Samples[2].SampleLower = 0;
				d.Samples[2].SampleUpper = 256;

				d.Samples[3].BitOffset = 27;
				d.Samples[3].BitLength = 4;
				d.Samples[3].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaG |
				                           (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypexponent;
				d.Samples[3].SampleLower = 15;
				d.Samples[3].SampleUpper = 31;

				d.Samples[4].BitOffset = 18;
				d.Samples[4].BitLength = 8;
				d.Samples[4].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaB;
				d.Samples[4].SampleLower = 0;
				d.Samples[4].SampleUpper = 256;

				d.Samples[5].BitOffset = 27;
				d.Samples[5].BitLength = 4;
				d.Samples[5].ChannelType = (uint)KhrDfModelChannels.KhrDfChannelRgbsdaB |
				                           (uint)KhrDfSampleDatatypeQualifiers.KhrDfSampleDatatypexponent;
				d.Samples[5].SampleLower = 15;
				d.Samples[5].SampleUpper = 31;

				return d;
			}
		}

	}
}
