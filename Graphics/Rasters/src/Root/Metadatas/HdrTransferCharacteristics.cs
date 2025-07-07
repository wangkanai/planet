// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Defines HDR transfer characteristics.
/// </summary>
public enum HdrTransferCharacteristics
{
	/// <summary>BT.709 transfer.</summary>
	Bt709 = 1,

	/// <summary>Gamma 2.2.</summary>
	Gamma22 = 4,

	/// <summary>Gamma 2.8.</summary>
	Gamma28 = 5,

	/// <summary>BT.601.</summary>
	Bt601 = 6,

	/// <summary>SMPTE 240M.</summary>
	Smpte240M = 7,

	/// <summary>Linear.</summary>
	Linear = 8,

	/// <summary>sRGB.</summary>
	Srgb = 13,

	/// <summary>BT.2020 10-bit.</summary>
	Bt2020_10bit = 14,

	/// <summary>BT.2020 12-bit.</summary>
	Bt2020_12bit = 15,

	/// <summary>SMPTE 2084 (PQ).</summary>
	Pq = 16,

	/// <summary>SMPTE 428.</summary>
	Smpte428 = 17,

	/// <summary>ARIB STD-B67 (HLG).</summary>
	Hlg = 18
}