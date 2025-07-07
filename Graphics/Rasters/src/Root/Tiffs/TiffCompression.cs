// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Specifies the compression algorithms supported by TIFF format.</summary>
public enum TiffCompression
{
	/// <summary>No compression</summary>
	None = 1,

	/// <summary>CCITT Group 3 1-Dimensional Modified Huffman run length encoding</summary>
	CcittGroup3 = 2,

	/// <summary>CCITT Group 3 fax encoding</summary>
	CcittGroup3Fax = 3,

	/// <summary>CCITT Group 4 fax encoding</summary>
	CcittGroup4 = 4,

	/// <summary>LZW compression</summary>
	Lzw = 5,

	/// <summary>JPEG compression (old-style)</summary>
	JpegOld = 6,

	/// <summary>JPEG compression (new-style)</summary>
	Jpeg = 7,

	/// <summary>Adobe Deflate compression</summary>
	AdobeDeflate = 8,

	/// <summary>T.85 JBIG compression</summary>
	Jbig = 9,

	/// <summary>T.43 colour by layered JBIG compression</summary>
	JbigColor = 10,

	/// <summary>PackBits compression</summary>
	PackBits = 32773,

	/// <summary>ThunderScan 4-bit compression</summary>
	Thunderscan = 32809,

	/// <summary>Deflate compression</summary>
	Deflate = 32946,

	/// <summary>JPEG 2000 compression</summary>
	Jpeg2000 = 34712
}
