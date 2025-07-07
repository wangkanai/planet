// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Contains TIFF format constants and specifications.</summary>
public static class TiffConstants
{
	/// <summary>TIFF file signature for little-endian byte order.</summary>
	public const uint LittleEndianSignature = 0x002A4949;

	/// <summary>TIFF file signature for big-endian byte order.</summary>
	public const uint BigEndianSignature = 0x4D4D002A;

	/// <summary>TIFF magic number.</summary>
	public const ushort MagicNumber = 42;

	/// <summary>Little-endian byte order identifier.</summary>
	public const ushort LittleEndian = 0x4949;

	/// <summary>Big-endian byte order identifier.</summary>
	public const ushort BigEndian = 0x4D4D;

	/// <summary>Maximum number of directory entries in a single IFD.</summary>
	public const int MaxDirectoryEntries = 65535;

	/// <summary>Size of IFD entry in bytes.</summary>
	public const int IfdEntrySize = 12;

	/// <summary>Default tile size for tiled TIFF images.</summary>
	public const int DefaultTileSize = 256;

	/// <summary>Default strip size for stripped TIFF images.</summary>
	public const int DefaultStripSize = 8192;
}

/// <summary>Contains standard TIFF tag identifiers.</summary>
public static class TiffTags
{
	/// <summary>NewSubFileType tag (254).</summary>
	public const int NewSubFileType = 254;

	/// <summary>ImageWidth tag (256).</summary>
	public const int ImageWidth = 256;

	/// <summary>ImageLength tag (257).</summary>
	public const int ImageLength = 257;

	/// <summary>BitsPerSample tag (258).</summary>
	public const int BitsPerSample = 258;

	/// <summary>Compression tag (259).</summary>
	public const int Compression = 259;

	/// <summary>PhotometricInterpretation tag (262).</summary>
	public const int PhotometricInterpretation = 262;

	/// <summary>ImageDescription tag (270).</summary>
	public const int ImageDescription = 270;

	/// <summary>Make tag (271).</summary>
	public const int Make = 271;

	/// <summary>Model tag (272).</summary>
	public const int Model = 272;

	/// <summary>StripOffsets tag (273).</summary>
	public const int StripOffsets = 273;

	/// <summary>SamplesPerPixel tag (277).</summary>
	public const int SamplesPerPixel = 277;

	/// <summary>RowsPerStrip tag (278).</summary>
	public const int RowsPerStrip = 278;

	/// <summary>StripByteCounts tag (279).</summary>
	public const int StripByteCounts = 279;

	/// <summary>XResolution tag (282).</summary>
	public const int XResolution = 282;

	/// <summary>YResolution tag (283).</summary>
	public const int YResolution = 283;

	/// <summary>PlanarConfiguration tag (284).</summary>
	public const int PlanarConfiguration = 284;

	/// <summary>ResolutionUnit tag (296).</summary>
	public const int ResolutionUnit = 296;

	/// <summary>Software tag (305).</summary>
	public const int Software = 305;

	/// <summary>DateTime tag (306).</summary>
	public const int DateTime = 306;

	/// <summary>Artist tag (315).</summary>
	public const int Artist = 315;

	/// <summary>Copyright tag (33432).</summary>
	public const int Copyright = 33432;
}
