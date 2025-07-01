// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Spatial;

/// <summary>Specifies the format of an MbTile.</summary>
public enum MbTileFormat
{
	/// <summary>Portable Network Graphics (PNG)</summary>
	Png,
	/// <summary>Joint Photographic Experts Group (JPEG)</summary>
	Jpg,
	/// <summary>Joint Photographic Experts Group (JPEG)</summary>
	Jpeg = Jpg,
	/// <summary>WebP format</summary>
	Webp,
	/// <summary>Protobuf vector format</summary>
	Pbf,
}
