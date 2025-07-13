// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.

namespace Wangkanai.Graphics;

/// <summary>Raster-specific comparison results.</summary>
public class RasterComparisonResult
{
	public bool                      BitDepthMatch   { get; set; }
	public bool                      ResolutionMatch { get; set; }
	public bool                      ColorSpaceMatch { get; set; }
	public (bool first, bool second) HasExifData     { get; set; }
	public (bool first, bool second) HasGpsData      { get; set; }
	public (bool first, bool second) HasIccProfile   { get; set; }
}
