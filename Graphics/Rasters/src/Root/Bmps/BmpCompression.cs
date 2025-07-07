// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

/// <summary>Defines the compression methods used in BMP images.</summary>
public enum BmpCompression : uint
{
	/// <summary>Uncompressed RGB format (most common).</summary>
	Rgb = 0,

	/// <summary>8-bit run-length encoding compression.</summary>
	Rle8 = 1,

	/// <summary>4-bit run-length encoding compression.</summary>
	Rle4 = 2,

	/// <summary>Uncompressed format with custom bit field masks.</summary>
	BitFields = 3,

	/// <summary>JPEG compression (for BMP files containing embedded JPEG data).</summary>
	Jpeg = 4,

	/// <summary>PNG compression (for BMP files containing embedded PNG data).</summary>
	Png = 5
}
