// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Represents JPEG chroma subsampling formats.</summary>
public enum JpegChromaSubsampling
{
	/// <summary>4:4:4 - No subsampling, full color resolution.</summary>
	None = 0,

	/// <summary>4:2:2 - Horizontal subsampling by factor of 2.</summary>
	Horizontal = 1,

	/// <summary>4:2:0 - Horizontal and vertical subsampling by factor of 2.</summary>
	Both = 2
}
