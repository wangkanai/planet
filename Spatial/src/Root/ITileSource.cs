// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Spatial;

/// <summary>Defines an interface for a tile source.</summary>
public interface ITileSource
{
	/// <summary>Gets the name of the tile source.</summary>
	string Name { get; }

	/// <summary>Gets the schema for the tile source.</summary>
	ITileSchema Schema { get; }

	/// <summary>Gets the attribution information for the tile source.</summary>
	Attribution Attribution { get; }
}
