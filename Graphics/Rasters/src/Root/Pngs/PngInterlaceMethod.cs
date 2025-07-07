// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Pngs;

/// <summary>Defines the interlace methods supported by PNG format.</summary>
public enum PngInterlaceMethod : byte
{
	/// <summary>No interlacing.</summary>
	None = 0,

	/// <summary>Adam7 interlacing for progressive rendering.</summary>
	Adam7 = 1
}
