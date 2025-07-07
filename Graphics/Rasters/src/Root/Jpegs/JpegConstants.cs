// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Contains JPEG format constants and specifications.</summary>
public static class JpegConstants
{
	/// <summary>JPEG file signature (SOI marker).</summary>
	public const ushort StartOfImage = 0xFFD8;

	/// <summary>JPEG end of image marker (EOI).</summary>
	public const ushort EndOfImage = 0xFFD9;

	/// <summary>JPEG APP0 marker for JFIF.</summary>
	public const ushort App0Marker = 0xFFE0;

	/// <summary>JPEG APP1 marker for EXIF.</summary>
	public const ushort App1Marker = 0xFFE1;

	/// <summary>JPEG APP2 marker for ICC profile.</summary>
	public const ushort App2Marker = 0xFFE2;

	/// <summary>JPEG Start of Frame (Baseline DCT).</summary>
	public const ushort StartOfFrameBaseline = 0xFFC0;

	/// <summary>JPEG Start of Frame (Progressive DCT).</summary>
	public const ushort StartOfFrameProgressive = 0xFFC2;

	/// <summary>JPEG Define Huffman Table marker.</summary>
	public const ushort DefineHuffmanTable = 0xFFC4;

	/// <summary>JPEG Define Quantization Table marker.</summary>
	public const ushort DefineQuantizationTable = 0xFFDB;

	/// <summary>JPEG Start of Scan marker.</summary>
	public const ushort StartOfScan = 0xFFDA;

	/// <summary>JPEG restart marker range start.</summary>
	public const ushort RestartMarkerStart = 0xFFD0;

	/// <summary>JPEG restart marker range end.</summary>
	public const ushort RestartMarkerEnd = 0xFFD7;

	/// <summary>Maximum JPEG image dimensions (65,535 × 65,535).</summary>
	public const int MaxDimension = 65535;

	/// <summary>Standard JPEG quality range minimum.</summary>
	public const int MinQuality = 0;

	/// <summary>Standard JPEG quality range maximum.</summary>
	public const int MaxQuality = 100;

	/// <summary>Default JPEG quality setting.</summary>
	public const int DefaultQuality = 75;

	/// <summary>JPEG bits per sample (8 bits per channel).</summary>
	public const int BitsPerSample = 8;

	/// <summary>JFIF identifier string.</summary>
	public const string JfifIdentifier = "JFIF";

	/// <summary>EXIF identifier string.</summary>
	public const string ExifIdentifier = "Exif";
}

/// <summary>Contains standard JPEG file extensions.</summary>
public static class JpegFileExtensions
{
	/// <summary>Primary JPEG file extension.</summary>
	public const string Jpg = ".jpg";

	/// <summary>Alternative JPEG file extension.</summary>
	public const string Jpeg = ".jpeg";

	/// <summary>JPEG file extension variant.</summary>
	public const string Jpe = ".jpe";

	/// <summary>JPEG File Interchange Format extension.</summary>
	public const string Jfif = ".jfif";

	/// <summary>All supported JPEG extensions.</summary>
	public static readonly string[] All = [Jpg, Jpeg, Jpe, Jfif];
}

/// <summary>Contains JPEG MIME type information.</summary>
public static class JpegMimeTypes
{
	/// <summary>Standard JPEG MIME type.</summary>
	public const string ImageJpeg = "image/jpeg";

	/// <summary>Alternative JPEG MIME type.</summary>
	public const string ImageJpg = "image/jpg";

	/// <summary>JFIF MIME type.</summary>
	public const string ImageJfif = "image/jfif";
}
