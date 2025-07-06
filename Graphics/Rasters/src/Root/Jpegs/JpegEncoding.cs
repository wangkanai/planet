// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Represents JPEG encoding formats.</summary>
public enum JpegEncoding
{
	/// <summary>Baseline JPEG - standard format, most widely supported.</summary>
	Baseline = 0,

	/// <summary>Progressive JPEG - loads in multiple passes from low to high quality.</summary>
	Progressive = 1,

	/// <summary>JPEG 2000 - newer standard with better compression (less common).</summary>
	Jpeg2000 = 2
}
