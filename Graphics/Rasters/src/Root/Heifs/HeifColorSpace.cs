// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Heifs;

/// <summary>
/// Defines the color space options for HEIF images.
/// </summary>
public enum HeifColorSpace
{
	/// <summary>Unknown or unspecified color space.</summary>
	Unknown = 0,

	/// <summary>sRGB color space (standard for web and general use).</summary>
	Srgb = 1,

	/// <summary>Linear RGB color space.</summary>
	LinearRgb = 2,

	/// <summary>BT.709 color space (standard for HD video).</summary>
	Bt709 = 3,

	/// <summary>BT.470M color space.</summary>
	Bt470M = 4,

	/// <summary>BT.470BG color space.</summary>
	Bt470Bg = 5,

	/// <summary>BT.601 color space (standard for SD video).</summary>
	Bt601 = 6,

	/// <summary>SMPTE 240M color space.</summary>
	Smpte240 = 7,

	/// <summary>Generic film color space.</summary>
	GenericFilm = 8,

	/// <summary>BT.2020 non-constant luminance color space (UHD).</summary>
	Bt2020Ncl = 9,

	/// <summary>BT.2020 constant luminance color space.</summary>
	Bt2020Cl = 10,

	/// <summary>Display P3 color space (wide gamut).</summary>
	DisplayP3 = 11,

	/// <summary>XYZ color space.</summary>
	Xyz = 12,

	/// <summary>sRGB color space with IEC 61966-2-1 standard.</summary>
	SrgbIec61966 = 13,

	/// <summary>BT.2100 PQ color space (HDR10).</summary>
	Bt2100Pq = 16,

	/// <summary>SMPTE 428 color space.</summary>
	Smpte428 = 17,

	/// <summary>BT.2100 HLG color space (Hybrid Log-Gamma).</summary>
	Bt2100Hlg = 18,

	/// <summary>SMPTE 431 color space (DCI-P3).</summary>
	Smpte431 = 19,

	/// <summary>SMPTE 432 color space (Display P3).</summary>
	Smpte432 = 20,

	/// <summary>EBU Tech 3213-E color space.</summary>
	EbuTech3213E = 22
}

/// <summary>
/// Defines the chroma subsampling options for HEIF images.
/// </summary>
public enum HeifChromaSubsampling
{
	/// <summary>YUV 4:4:4 - No chroma subsampling (highest quality).</summary>
	Yuv444 = 0,

	/// <summary>YUV 4:2:2 - Horizontal chroma subsampling.</summary>
	Yuv422 = 1,

	/// <summary>YUV 4:2:0 - Both horizontal and vertical chroma subsampling (most common).</summary>
	Yuv420 = 2,

	/// <summary>YUV 4:0:0 - Monochrome (no chroma).</summary>
	Yuv400 = 3
}

/// <summary>
/// Defines the compression methods for HEIF images.
/// </summary>
public enum HeifCompression
{
	/// <summary>HEVC (H.265) compression - most common for HEIF.</summary>
	Hevc = 0,

	/// <summary>AVC (H.264) compression - for compatibility.</summary>
	Avc = 1,

	/// <summary>AV1 compression - newer, more efficient.</summary>
	Av1 = 2,

	/// <summary>VVC (H.266) compression - next generation.</summary>
	Vvc = 3,

	/// <summary>EVC (Essential Video Coding) compression.</summary>
	Evc = 4,

	/// <summary>JPEG compression for thumbnails and previews.</summary>
	Jpeg = 5
}

/// <summary>
/// Defines the supported features for HEIF on the current platform.
/// </summary>
[Flags]
public enum HeifFeatures
{
	/// <summary>No features supported.</summary>
	None = 0,

	/// <summary>Basic encoding and decoding with HEVC.</summary>
	BasicCodec = 1 << 0,

	/// <summary>10-bit color depth support.</summary>
	TenBitDepth = 1 << 1,

	/// <summary>12-bit color depth support.</summary>
	TwelveBitDepth = 1 << 2,

	/// <summary>HDR metadata support.</summary>
	HdrMetadata = 1 << 3,

	/// <summary>Alpha channel support.</summary>
	AlphaChannel = 1 << 4,

	/// <summary>Multi-threaded encoding/decoding.</summary>
	MultiThreading = 1 << 5,

	/// <summary>Hardware acceleration support.</summary>
	HardwareAcceleration = 1 << 6,

	/// <summary>EXIF metadata support.</summary>
	ExifMetadata = 1 << 7,

	/// <summary>XMP metadata support.</summary>
	XmpMetadata = 1 << 8,

	/// <summary>ICC color profile support.</summary>
	IccProfile = 1 << 9,

	/// <summary>Lossless compression support.</summary>
	LosslessCompression = 1 << 10,

	/// <summary>Image sequences support.</summary>
	ImageSequences = 1 << 11,

	/// <summary>Image collections support.</summary>
	ImageCollections = 1 << 12,

	/// <summary>Thumbnail generation support.</summary>
	ThumbnailGeneration = 1 << 13,

	/// <summary>Progressive decoding support.</summary>
	ProgressiveDecoding = 1 << 14,

	/// <summary>AV1 codec support.</summary>
	Av1Codec = 1 << 15,

	/// <summary>VVC (H.266) codec support.</summary>
	VvcCodec = 1 << 16,

	/// <summary>Depth maps support.</summary>
	DepthMaps = 1 << 17,

	/// <summary>Auxiliary images support.</summary>
	AuxiliaryImages = 1 << 18
}

/// <summary>
/// Defines the use cases for HEIF encoding presets.
/// </summary>
public enum HeifUseCase
{
	/// <summary>Web-optimized images with good compression.</summary>
	WebOptimized,

	/// <summary>High-quality photography with minimal compression.</summary>
	Photography,

	/// <summary>Archival quality with lossless or near-lossless compression.</summary>
	Archival,

	/// <summary>Thumbnail generation with high compression.</summary>
	Thumbnail,

	/// <summary>Real-time encoding with fast speed.</summary>
	RealTime,

	/// <summary>Mobile device optimization.</summary>
	Mobile,

	/// <summary>HDR content preservation.</summary>
	Hdr,

	/// <summary>Professional video workflow.</summary>
	Professional
}

/// <summary>
/// Defines the profile levels for HEIF encoding.
/// </summary>
public enum HeifProfile
{
	/// <summary>Main profile for general use.</summary>
	Main = 0,

	/// <summary>Main 10 profile for 10-bit content.</summary>
	Main10 = 1,

	/// <summary>Main Still Picture profile for single images.</summary>
	MainStillPicture = 2,

	/// <summary>High throughput profile for performance.</summary>
	HighThroughput = 3,

	/// <summary>Multiview Main profile for stereoscopic content.</summary>
	MultiviewMain = 4,

	/// <summary>Scalable Main profile for adaptive streaming.</summary>
	ScalableMain = 5
}