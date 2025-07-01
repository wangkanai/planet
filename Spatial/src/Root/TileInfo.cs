// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Spatial;

/// <summary>Represents information about a tile.</summary>
public class TileInfo
{
	/// <summary>Represents a spatial extent with bounds.</summary>
	public Extent Extent { get; set; }

	/// <summary>Gets or sets the tile source associated with this tile.</summary>
	public TileIndex Index { get; set; }
}
