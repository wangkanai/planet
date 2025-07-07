// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Benchmark;

/// <summary>Baseline WebP constants implementation using byte[] for performance comparison.</summary>
public static class WebPConstantsBaseline
{
	/// <summary>The WebP signature bytes (RIFF header).</summary>
	public static readonly byte[] Signature = "RIFF"u8.ToArray();

	/// <summary>The WebP format identifier.</summary>
	public static readonly byte[] FormatId = "WEBP"u8.ToArray();

	/// <summary>The VP8 chunk identifier for lossy compression.</summary>
	public static readonly byte[] VP8ChunkId = "VP8 "u8.ToArray();

	/// <summary>The VP8L chunk identifier for lossless compression.</summary>
	public static readonly byte[] VP8LChunkId = "VP8L"u8.ToArray();

	/// <summary>The VP8X chunk identifier for extended features.</summary>
	public static readonly byte[] VP8XChunkId = "VP8X"u8.ToArray();

	/// <summary>The ALPH chunk identifier for alpha channel.</summary>
	public static readonly byte[] AlphaChunkId = "ALPH"u8.ToArray();

	/// <summary>The ANIM chunk identifier for animation.</summary>
	public static readonly byte[] AnimChunkId = "ANIM"u8.ToArray();

	/// <summary>The ANMF chunk identifier for animation frames.</summary>
	public static readonly byte[] AnimFrameChunkId = "ANMF"u8.ToArray();

	/// <summary>The ICCP chunk identifier for ICC profile.</summary>
	public static readonly byte[] IccProfileChunkId = "ICCP"u8.ToArray();

	/// <summary>The EXIF chunk identifier for EXIF metadata.</summary>
	public static readonly byte[] ExifChunkId = "EXIF"u8.ToArray();

	/// <summary>The XMP chunk identifier for XMP metadata.</summary>
	public static readonly byte[] XmpChunkId = "XMP "u8.ToArray();
}