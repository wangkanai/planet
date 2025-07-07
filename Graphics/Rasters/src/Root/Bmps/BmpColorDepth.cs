// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

/// <summary>Defines the color depth (bits per pixel) for BMP images.</summary>
public enum BmpColorDepth : ushort
{
	/// <summary>1-bit monochrome (black and white).</summary>
	Monochrome = 1,

	/// <summary>4-bit color with 16-color palette.</summary>
	FourBit = 4,

	/// <summary>8-bit color with 256-color palette.</summary>
	EightBit = 8,

	/// <summary>16-bit high color (RGB555 or RGB565).</summary>
	SixteenBit = 16,

	/// <summary>24-bit true color (RGB888).</summary>
	TwentyFourBit = 24,

	/// <summary>32-bit true color with alpha channel (ARGB8888).</summary>
	ThirtyTwoBit = 32
}
