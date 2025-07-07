// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Specifies the color depth supported by TIFF format.</summary>
public enum TiffColorDepth
{
	/// <summary>1-bit bilevel (black and white)</summary>
	Bilevel = 1,

	/// <summary>4-bit grayscale or palette</summary>
	FourBit = 4,

	/// <summary>8-bit grayscale or palette</summary>
	EightBit = 8,

	/// <summary>16-bit grayscale or RGB</summary>
	SixteenBit = 16,

	/// <summary>24-bit RGB color</summary>
	TwentyFourBit = 24,

	/// <summary>32-bit RGB with alpha or CMYK</summary>
	ThirtyTwoBit = 32,

	/// <summary>48-bit RGB color (16 bits per channel)</summary>
	FortyEightBit = 48,

	/// <summary>64-bit RGB with alpha (16 bits per channel)</summary>
	SixtyFourBit = 64
}
