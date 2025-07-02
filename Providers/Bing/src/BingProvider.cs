// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Planet.Providers.Bing.Extensions;

namespace Wangkanai.Planet.Extensions.Bing;

/// <summary>Provides functionality for generating Bing tile URLs.</summary>
public class BingProvider
{
	/// <summary>Generates a URL for a Bing tile.</summary>
	/// <param name="x">The x-coordinate of the tile.</param>
	/// <param name="y">The y-coordinate of the tile.</param>
	/// <param name="z">The zoom level of the tile.</param>
	/// <returns>A string containing the URL of the tile.</returns>
	public static string GetTileUrl(int x, int y, int z)
	{
		var q = TileExtensions.QuadKey(x, y, z);
		return $"https://ecn.t3.tiles.virtualearth.net/tiles/a{q}.jpeg?g=1";
	}
}
