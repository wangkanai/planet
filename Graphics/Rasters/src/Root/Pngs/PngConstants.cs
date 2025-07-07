// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Pngs;

/// <summary>Contains constants and specifications for PNG format.</summary>
public static class PngConstants
{
	/// <summary>PNG file signature bytes (magic number).</summary>
	/// <remarks>Hex: 89 50 4E 47 0D 0A 1A 0A, ASCII: ‰PNG\r\n\x1A\n</remarks>
	public static readonly byte[] Signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

	/// <summary>PNG MIME type.</summary>
	public const string MimeType = "image/png";

	/// <summary>Primary PNG file extension.</summary>
	public const string FileExtension = ".png";

	/// <summary>Maximum image width in pixels.</summary>
	public const uint MaxWidth = int.MaxValue;

	/// <summary>Maximum image height in pixels.</summary>
	public const uint MaxHeight = int.MaxValue;

	/// <summary>Minimum image width in pixels.</summary>
	public const uint MinWidth = 1;

	/// <summary>Minimum image height in pixels.</summary>
	public const uint MinHeight = 1;

	/// <summary>Length of PNG signature in bytes.</summary>
	public const int SignatureLength = 8;

	/// <summary>Length of chunk length field in bytes.</summary>
	public const int ChunkLengthSize = 4;

	/// <summary>Length of chunk type field in bytes.</summary>
	public const int ChunkTypeSize = 4;

	/// <summary>Length of chunk CRC field in bytes.</summary>
	public const int ChunkCrcSize = 4;

	/// <summary>Minimum chunk size (without data).</summary>
	public const int MinChunkSize = ChunkLengthSize + ChunkTypeSize + ChunkCrcSize;

	/// <summary>Estimated overhead for critical PNG chunks (IHDR, IEND, etc.).</summary>
	public const int CriticalChunksOverhead = 100;

	/// <summary>Critical chunk types.</summary>
	public static class ChunkTypes
	{
		/// <summary>Image header chunk (must be first).</summary>
		public const string IHDR = "IHDR";

		/// <summary>Palette table chunk (required for indexed images).</summary>
		public const string PLTE = "PLTE";

		/// <summary>Image data chunk (can appear multiple times).</summary>
		public const string IDAT = "IDAT";

		/// <summary>Image trailer chunk (marks end of PNG).</summary>
		public const string IEND = "IEND";

		/// <summary>Transparency chunk.</summary>
		public const string tRNS = "tRNS";

		/// <summary>Image gamma chunk.</summary>
		public const string gAMA = "gAMA";

		/// <summary>Chromaticity settings chunk.</summary>
		public const string cHRM = "cHRM";

		/// <summary>Standard RGB color space chunk.</summary>
		public const string sRGB = "sRGB";

		/// <summary>Physical pixel dimensions chunk.</summary>
		public const string pHYs = "pHYs";

		/// <summary>Textual information chunk.</summary>
		public const string tEXt = "tEXt";

		/// <summary>Compressed textual information chunk.</summary>
		public const string zTXt = "zTXt";

		/// <summary>International textual information chunk.</summary>
		public const string iTXt = "iTXt";

		/// <summary>Background color chunk.</summary>
		public const string bKGD = "bKGD";

		/// <summary>Last modification time chunk.</summary>
		public const string tIME = "tIME";
	}

	/// <summary>Supported bit depths for different color types.</summary>
	public static class BitDepths
	{
		/// <summary>Bit depths allowed for grayscale images.</summary>
		public static readonly byte[] Grayscale = [1, 2, 4, 8, 16];

		/// <summary>Bit depths allowed for truecolor images.</summary>
		public static readonly byte[] Truecolor = [8, 16];

		/// <summary>Bit depths allowed for indexed-color images.</summary>
		public static readonly byte[] IndexedColor = [1, 2, 4, 8];

		/// <summary>Bit depths allowed for grayscale with alpha images.</summary>
		public static readonly byte[] GrayscaleWithAlpha = [8, 16];

		/// <summary>Bit depths allowed for truecolor with alpha images.</summary>
		public static readonly byte[] TruecolorWithAlpha = [8, 16];
	}
}
