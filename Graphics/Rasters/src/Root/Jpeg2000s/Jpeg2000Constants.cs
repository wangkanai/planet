// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Collections.Immutable;

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

/// <summary>Defines constants for JPEG2000 format specifications.</summary>
public static class Jpeg2000Constants
{
	/// <summary>The JP2 signature box type ("jP  ").</summary>
	public static readonly ImmutableArray<byte> SignatureBoxType = "jP  "u8.ToImmutableArray();

	/// <summary>The JP2 signature data ([0x0D, 0x0A, 0x87, 0x0A]).</summary>
	public static readonly ImmutableArray<byte> SignatureData = new byte[] { 0x0D, 0x0A, 0x87, 0x0A }.ToImmutableArray();

	/// <summary>The File Type box type ("ftyp").</summary>
	public static readonly ImmutableArray<byte> FileTypeBoxType = "ftyp"u8.ToImmutableArray();

	/// <summary>The JP2 brand identifier ("jp2 ").</summary>
	public static readonly ImmutableArray<byte> Jp2Brand = "jp2 "u8.ToImmutableArray();

	/// <summary>The JP2 Header box type ("jp2h").</summary>
	public static readonly ImmutableArray<byte> HeaderBoxType = "jp2h"u8.ToImmutableArray();

	/// <summary>The Image Header box type ("ihdr").</summary>
	public static readonly ImmutableArray<byte> ImageHeaderBoxType = "ihdr"u8.ToImmutableArray();

	/// <summary>The Color Specification box type ("colr").</summary>
	public static readonly ImmutableArray<byte> ColorSpecBoxType = "colr"u8.ToImmutableArray();

	/// <summary>The Palette box type ("pclr").</summary>
	public static readonly ImmutableArray<byte> PaletteBoxType = "pclr"u8.ToImmutableArray();

	/// <summary>The Component Mapping box type ("cmap").</summary>
	public static readonly ImmutableArray<byte> ComponentMappingBoxType = "cmap"u8.ToImmutableArray();

	/// <summary>The Channel Definition box type ("cdef").</summary>
	public static readonly ImmutableArray<byte> ChannelDefinitionBoxType = "cdef"u8.ToImmutableArray();

	/// <summary>The Resolution box type ("res ").</summary>
	public static readonly ImmutableArray<byte> ResolutionBoxType = "res "u8.ToImmutableArray();

	/// <summary>The Capture Resolution box type ("resc").</summary>
	public static readonly ImmutableArray<byte> CaptureResolutionBoxType = "resc"u8.ToImmutableArray();

	/// <summary>The Display Resolution box type ("resd").</summary>
	public static readonly ImmutableArray<byte> DisplayResolutionBoxType = "resd"u8.ToImmutableArray();

	/// <summary>The Contiguous Codestream box type ("jp2c").</summary>
	public static readonly ImmutableArray<byte> CodestreamBoxType = "jp2c"u8.ToImmutableArray();

	/// <summary>The UUID box type ("uuid").</summary>
	public static readonly ImmutableArray<byte> UuidBoxType = "uuid"u8.ToImmutableArray();

	/// <summary>The XML box type ("xml ").</summary>
	public static readonly ImmutableArray<byte> XmlBoxType = "xml "u8.ToImmutableArray();

	/// <summary>The Intellectual Property box type ("uinf").</summary>
	public static readonly ImmutableArray<byte> IntellectualPropertyBoxType = "uinf"u8.ToImmutableArray();

	/// <summary>Box size field length in bytes.</summary>
	public const int BoxSizeLength = 4;

	/// <summary>Box type field length in bytes.</summary>
	public const int BoxTypeLength = 4;

	/// <summary>Standard box header length in bytes (size + type).</summary>
	public const int BoxHeaderLength = BoxSizeLength + BoxTypeLength;

	/// <summary>Extended box size field length in bytes (for large boxes).</summary>
	public const int ExtendedBoxSizeLength = 8;

	/// <summary>Extended box header length in bytes (size + type + extended size).</summary>
	public const int ExtendedBoxHeaderLength = BoxHeaderLength + ExtendedBoxSizeLength;

	/// <summary>Image Header box data length in bytes.</summary>
	public const int ImageHeaderDataLength = 14;

	/// <summary>Minimum width for JPEG2000 images.</summary>
	public const int MinWidth = 1;

	/// <summary>Maximum width for JPEG2000 images.</summary>
	public const int MaxWidth = int.MaxValue;

	/// <summary>Minimum height for JPEG2000 images.</summary>
	public const int MinHeight = 1;

	/// <summary>Maximum height for JPEG2000 images.</summary>
	public const int MaxHeight = int.MaxValue;

	/// <summary>Maximum number of components supported.</summary>
	public const int MaxComponents = 16384;

	/// <summary>Maximum bit depth per component.</summary>
	public const int MaxBitDepth = 38;

	/// <summary>Default compression ratio for lossy encoding.</summary>
	public const float DefaultCompressionRatio = 20.0f;

	/// <summary>Default number of decomposition levels.</summary>
	public const int DefaultDecompositionLevels = 5;

	/// <summary>Maximum number of decomposition levels.</summary>
	public const int MaxDecompositionLevels = 32;

	/// <summary>Default tile size in pixels.</summary>
	public const int DefaultTileSize = 1024;

	/// <summary>JPEG2000 codestream markers.</summary>
	public static class Markers
	{
		/// <summary>Start of Codestream (SOC) marker.</summary>
		public const ushort StartOfCodestream = 0xFF4F;

		/// <summary>Start of Image (SOI) marker.</summary>
		public const ushort StartOfImage = 0xFF51;

		/// <summary>End of Codestream (EOC) marker.</summary>
		public const ushort EndOfCodestream = 0xFFD9;

		/// <summary>Codestream Size (SIZ) marker.</summary>
		public const ushort CodestreamSize = 0xFF51;

		/// <summary>Coding Style Default (COD) marker.</summary>
		public const ushort CodingStyleDefault = 0xFF52;

		/// <summary>Coding Style Component (COC) marker.</summary>
		public const ushort CodingStyleComponent = 0xFF53;

		/// <summary>Region of Interest (RGN) marker.</summary>
		public const ushort RegionOfInterest = 0xFF5E;

		/// <summary>Quantization Default (QCD) marker.</summary>
		public const ushort QuantizationDefault = 0xFF5C;

		/// <summary>Quantization Component (QCC) marker.</summary>
		public const ushort QuantizationComponent = 0xFF5D;

		/// <summary>Progression Order Change (POC) marker.</summary>
		public const ushort ProgressionOrderChange = 0xFF5F;

		/// <summary>Tile-part Length (TLM) marker.</summary>
		public const ushort TilePartLength = 0xFF55;

		/// <summary>Packet Length Main Header (PLM) marker.</summary>
		public const ushort PacketLengthMainHeader = 0xFF57;

		/// <summary>Packet Length Tile-part Header (PLT) marker.</summary>
		public const ushort PacketLengthTilePartHeader = 0xFF58;

		/// <summary>Packed Packet Headers Main Header (PPM) marker.</summary>
		public const ushort PackedPacketHeadersMainHeader = 0xFF60;

		/// <summary>Packed Packet Headers Tile-part Header (PPT) marker.</summary>
		public const ushort PackedPacketHeadersTilePartHeader = 0xFF61;

		/// <summary>Start of Tile-part (SOT) marker.</summary>
		public const ushort StartOfTilePart = 0xFF90;

		/// <summary>Start of Packet (SOP) marker.</summary>
		public const ushort StartOfPacket = 0xFF91;

		/// <summary>End of Packet Header (EPH) marker.</summary>
		public const ushort EndOfPacketHeader = 0xFF92;

		/// <summary>Start of Data (SOD) marker.</summary>
		public const ushort StartOfData = 0xFF93;

		/// <summary>Comment (COM) marker.</summary>
		public const ushort Comment = 0xFF64;
	}

	/// <summary>Color space enumeration values.</summary>
	public static class ColorSpaces
	{
		/// <summary>Grayscale color space.</summary>
		public const ushort Grayscale = 17;

		/// <summary>sRGB color space.</summary>
		public const ushort sRGB = 16;

		/// <summary>YCC color space.</summary>
		public const ushort YCC = 18;

		/// <summary>Restricted ICC profile.</summary>
		public const ushort RestrictedICC = 2;

		/// <summary>Any ICC profile.</summary>
		public const ushort AnyICC = 3;

		/// <summary>Vendor color space.</summary>
		public const ushort VendorColor = 4;
	}

	/// <summary>Progression order values.</summary>
	public static class ProgressionOrders
	{
		/// <summary>Layer-Resolution-Component-Position.</summary>
		public const byte LRCP = 0;

		/// <summary>Resolution-Layer-Component-Position.</summary>
		public const byte RLCP = 1;

		/// <summary>Resolution-Position-Component-Layer.</summary>
		public const byte RPCL = 2;

		/// <summary>Position-Component-Resolution-Layer.</summary>
		public const byte PCRL = 3;

		/// <summary>Component-Position-Resolution-Layer.</summary>
		public const byte CPRL = 4;
	}

	/// <summary>Wavelet transform values.</summary>
	public static class WaveletTransforms
	{
		/// <summary>9/7 irreversible wavelet transform.</summary>
		public const byte Irreversible97 = 0;

		/// <summary>5/3 reversible wavelet transform.</summary>
		public const byte Reversible53 = 1;
	}

	/// <summary>Compression mode values.</summary>
	public static class CompressionModes
	{
		/// <summary>Lossless compression.</summary>
		public const byte Lossless = 0;

		/// <summary>Lossy compression.</summary>
		public const byte Lossy = 1;
	}

	/// <summary>Channel type values.</summary>
	public static class ChannelTypes
	{
		/// <summary>Color channel.</summary>
		public const ushort Color = 0;

		/// <summary>Opacity channel.</summary>
		public const ushort Opacity = 1;

		/// <summary>Premultiplied opacity channel.</summary>
		public const ushort PremultipliedOpacity = 2;

		/// <summary>Unspecified channel.</summary>
		public const ushort Unspecified = 65535;
	}

	/// <summary>GeoJP2 constants for geospatial metadata.</summary>
	public static class GeoJp2
	{
		/// <summary>GeoTIFF UUID for GeoJP2.</summary>
		public static readonly ImmutableArray<byte> GeoTiffUuid = new byte[]
		                                                          {
			                                                          0xB1, 0x4B, 0xF8, 0xBD, 0x08, 0x3D, 0x4B, 0x43,
			                                                          0xA5, 0xAE, 0x8C, 0xD7, 0xD5, 0xA6, 0xCE, 0x03
		                                                          }.ToImmutableArray();

		/// <summary>GML UUID for GeoJP2.</summary>
		public static readonly ImmutableArray<byte> GmlUuid = new byte[]
		                                                      {
			                                                      0x96, 0xA9, 0xF1, 0xF1, 0xDC, 0x98, 0x40, 0x2D,
			                                                      0xA7, 0xAE, 0xD6, 0x8E, 0x34, 0x45, 0x18, 0x09
		                                                      }.ToImmutableArray();
	}

	/// <summary>Quality layer constants.</summary>
	public static class QualityLayers
	{
		/// <summary>Minimum number of quality layers.</summary>
		public const int MinLayers = 1;

		/// <summary>Maximum number of quality layers.</summary>
		public const int MaxLayers = 65535;

		/// <summary>Default number of quality layers.</summary>
		public const int DefaultLayers = 5;
	}

	/// <summary>Resolution level constants.</summary>
	public static class ResolutionLevels
	{
		/// <summary>Full resolution level.</summary>
		public const int FullResolution = 0;

		/// <summary>Half resolution level.</summary>
		public const int HalfResolution = 1;

		/// <summary>Quarter resolution level.</summary>
		public const int QuarterResolution = 2;

		/// <summary>Eighth resolution level.</summary>
		public const int EighthResolution = 3;
	}

	/// <summary>Error resilience constants.</summary>
	public static class ErrorResilience
	{
		/// <summary>No error resilience features.</summary>
		public const byte None = 0x00;

		/// <summary>Segmentation markers enabled.</summary>
		public const byte SegmentationMarkers = 0x01;

		/// <summary>Restart markers enabled.</summary>
		public const byte RestartMarkers = 0x02;

		/// <summary>Termination on each coding pass.</summary>
		public const byte TerminateOnEachPass = 0x04;

		/// <summary>Vertically causal context.</summary>
		public const byte VerticallyStreaming = 0x08;

		/// <summary>Predictable termination.</summary>
		public const byte PredictableTermination = 0x10;

		/// <summary>Error resilience in Tier-1.</summary>
		public const byte ErrorResilienceTier1 = 0x20;
	}

	/// <summary>Memory optimization constants.</summary>
	public static class Memory
	{
		/// <summary>Default tile cache size in MB.</summary>
		public const int DefaultTileCacheSizeMB = 256;

		/// <summary>Default decoded tile cache capacity.</summary>
		public const int DefaultDecodedTileCacheCapacity = 64;

		/// <summary>Minimum tile size for efficient processing.</summary>
		public const int MinEfficientTileSize = 256;

		/// <summary>Maximum tile size for memory efficiency.</summary>
		public const int MaxEfficientTileSize = 4096;
	}
}
