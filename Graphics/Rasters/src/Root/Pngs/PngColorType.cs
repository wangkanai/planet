// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Pngs;

/// <summary>Defines the color types supported by PNG format.</summary>
public enum PngColorType : byte
{
	/// <summary>Each pixel is a grayscale sample.</summary>
	Grayscale = 0,

	/// <summary>RGB triple per pixel.</summary>
	Truecolor = 2,

	/// <summary>Each pixel is an index into a palette.</summary>
	IndexedColor = 3,

	/// <summary>Grayscale sample with transparency.</summary>
	GrayscaleWithAlpha = 4,

	/// <summary>RGB triple with transparency.</summary>
	TruecolorWithAlpha = 6
}
