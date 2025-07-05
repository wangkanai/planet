// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Spatial;

/// <summary>Defines the schema for a tile.</summary>
public interface ITileSchema
{
	/// <summary>Gets the name of the tile schema.</summary>
	string Name { get; }

	/// <summary>Gets the spatial reference system (SRS) of the tile schema.</summary>
	string Srs { get; }

	/// <summary>Gets the extent of the tile schema.</summary>
	Extent Extent { get; }

	/// <summary>Gets the format of the tiles in the schema.</summary>
	string Format { get; }

	/// <summary>Gets the resolutions available in the tile schema.</summary>
	IDictionary<int, Resolution> Resolutions { get; }
}
