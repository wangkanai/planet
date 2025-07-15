// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.

namespace Wangkanai.Graphics;

/// <summary>Vector-specific comparison results.</summary>
public class VectorComparisonResult : IImageComparisonResult
{
	public double ElementCountSimilarity { get; set; }
	public bool   CoordinateSystemMatch  { get; set; }
	public bool   ColorSpaceMatch        { get; set; }
	public bool   ComplexityLevelMatch   { get; set; }
}
