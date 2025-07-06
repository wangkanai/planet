// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Represents JPEG color modes.</summary>
public enum JpegColorMode
{
	/// <summary>Grayscale (8-bit per pixel).</summary>
	Grayscale = 1,

	/// <summary>RGB true color (24-bit per pixel).</summary>
	Rgb = 3,

	/// <summary>CMYK for print (32-bit per pixel).</summary>
	Cmyk = 4,

	/// <summary>YCbCr color space (most common for JPEG).</summary>
	YCbCr = 6
}
