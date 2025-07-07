// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Collections.Immutable;

namespace Wangkanai.Graphics.Rasters.WebPs;

/// <summary>Defines constants for WebP format specifications.</summary>
public static class WebPConstants
{
	/// <summary>The WebP signature bytes (RIFF header).</summary>
	public static readonly ImmutableArray<byte> Signature = "RIFF"u8.ToImmutableArray();

	/// <summary>The WebP format identifier.</summary>
	public static readonly ImmutableArray<byte> FormatId = "WEBP"u8.ToImmutableArray();

	/// <summary>The VP8 chunk identifier for lossy compression.</summary>
	public static readonly ImmutableArray<byte> VP8ChunkId = "VP8 "u8.ToImmutableArray();

	/// <summary>The VP8L chunk identifier for lossless compression.</summary>
	public static readonly ImmutableArray<byte> VP8LChunkId = "VP8L"u8.ToImmutableArray();

	/// <summary>The VP8X chunk identifier for extended features.</summary>
	public static readonly ImmutableArray<byte> VP8XChunkId = "VP8X"u8.ToImmutableArray();

	/// <summary>The ALPH chunk identifier for alpha channel.</summary>
	public static readonly ImmutableArray<byte> AlphaChunkId = "ALPH"u8.ToImmutableArray();

	/// <summary>The ANIM chunk identifier for animation.</summary>
	public static readonly ImmutableArray<byte> AnimChunkId = "ANIM"u8.ToImmutableArray();

	/// <summary>The ANMF chunk identifier for animation frames.</summary>
	public static readonly ImmutableArray<byte> AnimFrameChunkId = "ANMF"u8.ToImmutableArray();

	/// <summary>The ICCP chunk identifier for ICC profile.</summary>
	public static readonly ImmutableArray<byte> IccProfileChunkId = "ICCP"u8.ToImmutableArray();

	/// <summary>The EXIF chunk identifier for EXIF metadata.</summary>
	public static readonly ImmutableArray<byte> ExifChunkId = "EXIF"u8.ToImmutableArray();

	/// <summary>The XMP chunk identifier for XMP metadata.</summary>
	public static readonly ImmutableArray<byte> XmpChunkId = "XMP "u8.ToImmutableArray();

	/// <summary>The minimum width for WebP images.</summary>
	public const uint MinWidth = 1;

	/// <summary>The maximum width for WebP images.</summary>
	public const uint MaxWidth = 16383;

	/// <summary>The minimum height for WebP images.</summary>
	public const uint MinHeight = 1;

	/// <summary>The maximum height for WebP images.</summary>
	public const uint MaxHeight = 16383;

	/// <summary>The minimum quality level for lossy compression.</summary>
	public const int MinQuality = 0;

	/// <summary>The maximum quality level for lossy compression.</summary>
	public const int MaxQuality = 100;

	/// <summary>The default quality level for lossy compression.</summary>
	public const int DefaultQuality = 75;

	/// <summary>The minimum compression level for lossless compression.</summary>
	public const int MinCompressionLevel = 0;

	/// <summary>The maximum compression level for lossless compression.</summary>
	public const int MaxCompressionLevel = 9;

	/// <summary>The default compression level for lossless compression.</summary>
	public const int DefaultCompressionLevel = 6;

	/// <summary>The size of the RIFF header in bytes.</summary>
	public const int RiffHeaderSize = 12;

	/// <summary>The size of a chunk header in bytes.</summary>
	public const int ChunkHeaderSize = 8;

	/// <summary>The size of the VP8X chunk data in bytes.</summary>
	public const int VP8XChunkSize = 10;

	/// <summary>The size of the ANIM chunk data in bytes.</summary>
	public const int AnimChunkSize = 6;

	/// <summary>The size of the alpha preprocessing flag in bytes.</summary>
	public const int AlphaPreprocessingSize = 1;

	/// <summary>The maximum number of animation loops (0 = infinite).</summary>
	public const ushort MaxAnimationLoops = ushort.MaxValue;

	/// <summary>The default background color for animations (transparent).</summary>
	public const uint DefaultBackgroundColor = 0x00000000;

	/// <summary>Estimated overhead for WebP container and chunks.</summary>
	public const int ContainerOverhead = 50;

	/// <summary>The bit depth for WebP images (always 8 bits per channel).</summary>
	public const byte BitDepth = 8;

	/// <summary>The number of color channels in RGB format.</summary>
	public const byte RgbChannels = 3;

	/// <summary>The number of color channels in RGBA format.</summary>
	public const byte RgbaChannels = 4;
}
