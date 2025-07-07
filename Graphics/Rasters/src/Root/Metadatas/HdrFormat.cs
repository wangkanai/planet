// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Defines HDR format types.
/// </summary>
public enum HdrFormat
{
	/// <summary>Standard dynamic range.</summary>
	Sdr,

	/// <summary>HDR10 format.</summary>
	Hdr10,

	/// <summary>HDR10+ format.</summary>
	Hdr10Plus,

	/// <summary>Dolby Vision format.</summary>
	DolbyVision,

	/// <summary>Hybrid Log-Gamma format.</summary>
	Hlg
}