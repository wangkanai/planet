// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Defines HDR matrix coefficients.
/// </summary>
public enum HdrMatrixCoefficients
{
	/// <summary>RGB or GBR (identity).</summary>
	Identity = 0,

	/// <summary>BT.709.</summary>
	Bt709 = 1,

	/// <summary>BT.470M.</summary>
	Bt470M = 4,

	/// <summary>BT.470BG.</summary>
	Bt470Bg = 5,

	/// <summary>BT.601.</summary>
	Bt601 = 6,

	/// <summary>SMPTE 240M.</summary>
	Smpte240M = 7,

	/// <summary>YCoCg.</summary>
	YCoCg = 8,

	/// <summary>BT.2020 non-constant luminance.</summary>
	Bt2020Ncl = 9,

	/// <summary>BT.2020 constant luminance.</summary>
	Bt2020Cl = 10,

	/// <summary>SMPTE 2085.</summary>
	Smpte2085 = 11,

	/// <summary>Chromaticity-derived non-constant luminance.</summary>
	ChromaDerivedNcl = 12,

	/// <summary>Chromaticity-derived constant luminance.</summary>
	ChromaDerivedCl = 13,

	/// <summary>ICtCp.</summary>
	ICtCp = 14
}