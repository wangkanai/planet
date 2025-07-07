// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Defines HDR color primaries.
/// </summary>
public enum HdrColorPrimaries
{
	/// <summary>BT.709 primaries.</summary>
	Bt709 = 1,

	/// <summary>BT.470M primaries.</summary>
	Bt470M = 4,

	/// <summary>BT.470BG primaries.</summary>
	Bt470Bg = 5,

	/// <summary>BT.601/SMPTE 170M primaries.</summary>
	Bt601 = 6,

	/// <summary>SMPTE 240M primaries.</summary>
	Smpte240M = 7,

	/// <summary>Generic film primaries.</summary>
	GenericFilm = 8,

	/// <summary>BT.2020 primaries.</summary>
	Bt2020 = 9,

	/// <summary>XYZ primaries.</summary>
	Xyz = 10,

	/// <summary>DCI-P3 primaries.</summary>
	DciP3 = 11,

	/// <summary>Display P3 primaries.</summary>
	DisplayP3 = 12
}