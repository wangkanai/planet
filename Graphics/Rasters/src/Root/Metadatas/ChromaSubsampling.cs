// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Represents common chroma subsampling formats used across various image formats.
/// </summary>
public enum ChromaSubsampling
{
	/// <summary>
	/// 4:4:4 - No chroma subsampling, full color resolution.
	/// Each pixel has its own color information.
	/// </summary>
	Yuv444 = 0,

	/// <summary>
	/// 4:2:2 - Horizontal chroma subsampling by factor of 2.
	/// Color resolution is halved horizontally but maintained vertically.
	/// </summary>
	Yuv422 = 1,

	/// <summary>
	/// 4:2:0 - Both horizontal and vertical chroma subsampling by factor of 2.
	/// Color resolution is halved in both dimensions (most common for compressed formats).
	/// </summary>
	Yuv420 = 2,

	/// <summary>
	/// 4:1:1 - Horizontal chroma subsampling by factor of 4.
	/// Color resolution is quartered horizontally but maintained vertically.
	/// </summary>
	Yuv411 = 3,

	/// <summary>
	/// 4:0:0 - Monochrome, no chroma information.
	/// Only luminance (brightness) information is stored.
	/// </summary>
	Yuv400 = 4,

	/// <summary>
	/// 4:4:0 - Vertical chroma subsampling by factor of 2.
	/// Color resolution is halved vertically but maintained horizontally (rare).
	/// </summary>
	Yuv440 = 5
}