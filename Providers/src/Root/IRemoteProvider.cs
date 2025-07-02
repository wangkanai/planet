// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Providers;

public interface IRemoteProvider
{
	/// <summary>Generates a URL for a tile based on its coordinates and zoom level.</summary>
	/// <param name="x">The x-coordinate of the tile.</param>
	/// <param name="y">The y-coordinate of the tile.</param>
	/// <param name="z">The zoom level of the tile.</param>
	/// <returns>A string containing the URL of the tile.</returns>
	string GetTileUrl(int x, int y, int z);
}
