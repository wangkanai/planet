// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors.Svgs;

/// <summary>Constants for SVG vector graphics processing and optimization.</summary>
public static class SvgConstants
{
	/// <summary>Default SVG version for new documents.</summary>
	public const string DefaultVersion = "1.1";

	/// <summary>SVG MIME type.</summary>
	public const string MimeType = "image/svg+xml";

	/// <summary>SVGZ (compressed SVG) MIME type.</summary>
	public const string CompressedMimeType = "image/svg+xml-compressed";

	/// <summary>Standard SVG file extension.</summary>
	public const string FileExtension = ".svg";

	/// <summary>Compressed SVG file extension.</summary>
	public const string CompressedFileExtension = ".svgz";

	/// <summary>Default compression level for SVGZ format (1-9).</summary>
	public const int DefaultCompressionLevel = 6;

	/// <summary>Maximum recommended compression level for SVGZ.</summary>
	public const int MaxCompressionLevel = 9;

	/// <summary>Threshold for determining large SVG files (in bytes) that benefit from compression.</summary>
	public const long LargeSvgThreshold = 50_000; // 50KB

	/// <summary>Threshold for very large SVG files requiring streaming processing.</summary>
	public const long VeryLargeSvgThreshold = 1_000_000; // 1MB

	/// <summary>Maximum number of elements before performance optimization is recommended.</summary>
	public const int PerformanceOptimizationThreshold = 1000;

	/// <summary>Default viewBox size for new SVG documents.</summary>
	public const string DefaultViewBox = "0 0 100 100";

	/// <summary>Standard SVG namespace URI.</summary>
	public const string SvgNamespace = "http://www.w3.org/2000/svg";

	/// <summary>XLink namespace URI (for SVG 1.1 compatibility).</summary>
	public const string XLinkNamespace = "http://www.w3.org/1999/xlink";

	/// <summary>XML namespace URI.</summary>
	public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";

	/// <summary>Default coordinate reference system for geospatial SVG.</summary>
	public const string DefaultCrs = "EPSG:4326"; // WGS84

	/// <summary>Web Mercator coordinate reference system.</summary>
	public const string WebMercatorCrs = "EPSG:3857";

	/// <summary>Units per inch for SVG measurements.</summary>
	public const double UnitsPerInch = 96.0;

	/// <summary>Units per centimeter for SVG measurements.</summary>
	public const double UnitsPerCentimeter = UnitsPerInch / 2.54;

	/// <summary>Units per millimeter for SVG measurements.</summary>
	public const double UnitsPerMillimeter = UnitsPerCentimeter / 10.0;

	/// <summary>Default DPI for SVG rasterization.</summary>
	public const double DefaultDpi = 96.0;

	/// <summary>Precision for floating-point SVG coordinates (decimal places).</summary>
	public const int CoordinatePrecision = 6;

	/// <summary>Memory estimation factor per SVG element (in bytes).</summary>
	public const int MemoryPerElement = 256;

	/// <summary>Memory estimation factor per path segment (in bytes).</summary>
	public const int MemoryPerPathSegment = 32;

	/// <summary>Buffer size for streaming SVG processing.</summary>
	public const int StreamingBufferSize = 8192;

	/// <summary>Maximum recursion depth for nested SVG elements.</summary>
	public const int MaxRecursionDepth = 100;

	/// <summary>Supported SVG versions.</summary>
	public static readonly string[] SupportedVersions = { "1.0", "1.1", "2.0" };

	/// <summary>Standard SVG namespace declarations.</summary>
	public static readonly Dictionary<string, string> StandardNamespaces = new()
	                                                                       {
		                                                                       { "svg", SvgNamespace },
		                                                                       { "xlink", XLinkNamespace },
		                                                                       { "xml", XmlNamespace }
	                                                                       };

	/// <summary>Common geospatial CRS definitions.</summary>
	public static readonly Dictionary<string, string> CommonCrs = new()
	                                                              {
		                                                              { "WGS84", "EPSG:4326" },
		                                                              { "WebMercator", "EPSG:3857" },
		                                                              { "UTM", "EPSG:32633" }, // Example UTM zone
		                                                              { "PlateCarree", "EPSG:4326" }
	                                                              };

	/// <summary>Performance thresholds for different SVG complexities.</summary>
	public static class PerformanceThresholds
	{
		/// <summary>Simple SVG with minimal elements.</summary>
		public const int Simple = 50;

		/// <summary>Moderate complexity SVG.</summary>
		public const int Moderate = 200;

		/// <summary>Complex SVG requiring optimization.</summary>
		public const int Complex = 1000;

		/// <summary>Very complex SVG requiring streaming.</summary>
		public const int VeryComplex = 5000;
	}

	/// <summary>Default style properties for SVG elements.</summary>
	public static class DefaultStyles
	{
		/// <summary>Default fill color.</summary>
		public const string Fill = "black";

		/// <summary>Default stroke color.</summary>
		public const string Stroke = "none";

		/// <summary>Default stroke width.</summary>
		public const string StrokeWidth = "1";

		/// <summary>Default opacity.</summary>
		public const string Opacity = "1";

		/// <summary>Default font family.</summary>
		public const string FontFamily = "sans-serif";

		/// <summary>Default font size.</summary>
		public const string FontSize = "12";
	}
}
