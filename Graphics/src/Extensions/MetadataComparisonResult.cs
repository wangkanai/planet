// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.

namespace Wangkanai.Graphics.Extensions;

/// <summary>
/// Result of comparing two metadata instances.
/// </summary>
public class MetadataComparisonResult
{
	public bool DimensionsMatch  { get; set; }
	public bool TitleMatch       { get; set; }
	public bool OrientationMatch { get; set; }
	public bool TypeMatch        { get; set; }

	public double DimensionSimilarity   { get; set; }
	public double AspectRatioSimilarity { get; set; }
	public double SizeSimilarity        { get; set; }
	public double OverallSimilarity     { get; set; }

	public RasterComparisonResult? RasterComparison { get; set; }
	public VectorComparisonResult? VectorComparison { get; set; }
}
