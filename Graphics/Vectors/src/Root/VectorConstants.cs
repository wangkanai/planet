// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors;

/// <summary>Constants for vector graphics processing and optimization.</summary>
public static class VectorConstants
{
	/// <summary>Default vector image width in pixels.</summary>
	public const int DefaultWidth = 100;

	/// <summary>Default vector image height in pixels.</summary>
	public const int DefaultHeight = 100;

	/// <summary>Maximum recommended vector complexity for real-time rendering.</summary>
	public const int MaxComplexityThreshold = 10000;

	/// <summary>Memory estimation factor per vector element (in bytes).</summary>
	public const int MemoryPerVectorElement = 128;

	/// <summary>Default precision for vector coordinates (decimal places).</summary>
	public const int DefaultCoordinatePrecision = 4;

	/// <summary>Common vector graphics MIME types.</summary>
	public static class MimeTypes
	{
		/// <summary>SVG MIME type.</summary>
		public const string Svg = "image/svg+xml";

		/// <summary>Compressed SVG MIME type.</summary>
		public const string SvgCompressed = "image/svg+xml-compressed";

		/// <summary>PostScript MIME type.</summary>
		public const string PostScript = "application/postscript";

		/// <summary>PDF MIME type.</summary>
		public const string Pdf = "application/pdf";
	}

	/// <summary>Common vector graphics file extensions.</summary>
	public static class FileExtensions
	{
		/// <summary>SVG file extension.</summary>
		public const string Svg = ".svg";

		/// <summary>Compressed SVG file extension.</summary>
		public const string SvgCompressed = ".svgz";

		/// <summary>PostScript file extension.</summary>
		public const string PostScript = ".ps";

		/// <summary>Encapsulated PostScript file extension.</summary>
		public const string Eps = ".eps";

		/// <summary>PDF file extension.</summary>
		public const string Pdf = ".pdf";
	}
}
