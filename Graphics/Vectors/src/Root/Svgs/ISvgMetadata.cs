// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors.Svgs;

/// <summary>
/// Defines the contract for SVG vector graphics metadata.
/// Provides SVG-specific metadata including viewBox, coordinate systems, and styling information.
/// </summary>
/// <remarks>
/// SVG metadata differs from raster metadata as it includes vector-specific properties
/// such as viewBox dimensions, coordinate reference systems for geospatial mapping,
/// and scalable vector graphics properties that are resolution-independent.
/// </remarks>
public interface ISvgMetadata : IVectorMetadata
{
	/// <summary>Gets or sets the SVG version (e.g., "1.1", "2.0").</summary>
	string Version { get; set; }

	/// <summary>Gets or sets the SVG viewBox dimensions (x, y, width, height).</summary>
	SvgViewBox ViewBox { get; set; }

	/// <summary>Gets or sets the SVG viewport width.</summary>
	double ViewportWidth { get; set; }

	/// <summary>Gets or sets the SVG viewport height.</summary>
	double ViewportHeight { get; set; }

	/// <summary>Gets or sets the coordinate reference system (CRS) for geospatial SVG.</summary>
	string? CoordinateReferenceSystem { get; set; }

	/// <summary>Gets the XML namespace declarations.</summary>
	Dictionary<string, string> Namespaces { get; }

	/// <summary>Gets or sets the title of the SVG document.</summary>
	string? Title { get; set; }

	/// <summary>Gets or sets the description of the SVG document.</summary>
	string? Description { get; set; }

	/// <summary>Gets or sets the creation date of the SVG document.</summary>
	DateTime CreationDate { get; set; }

	/// <summary>Gets or sets the last modification date of the SVG document.</summary>
	DateTime ModificationDate { get; set; }

	/// <summary>Gets or sets the creator/author of the SVG document.</summary>
	string? Creator { get; set; }

	/// <summary>Gets or sets whether the SVG uses compression (SVGZ format).</summary>
	bool IsCompressed { get; set; }

	/// <summary>Gets or sets the compression level for SVGZ format (1-9).</summary>
	int CompressionLevel { get; set; }

	/// <summary>Gets or sets the number of SVG elements in the document.</summary>
	int ElementCount { get; set; }

	/// <summary>Gets or sets the total path length for optimization calculations.</summary>
	double TotalPathLength { get; set; }

	/// <summary>Gets or sets the color space used in the SVG.</summary>
	SvgColorSpace ColorSpace { get; set; }

	/// <summary>Gets the custom metadata properties.</summary>
	Dictionary<string, object> CustomProperties { get; }

	/// <summary>Calculates the estimated memory usage based on SVG complexity.</summary>
	long CalculateEstimatedMemoryUsage();

	/// <summary>Validates the SVG metadata for compliance with specified version.</summary>
	bool ValidateCompliance();

	/// <summary>Clears all metadata values to their defaults.</summary>
	void Clear();
}
