// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Planet.Providers.Bing.Extensions;

namespace Wangkanai.Planet.Extensions.Bing;

public class BingProvider
{
	public static string GetTileUrl(int x, int y, int z)
	{
		var q = TileExtensions.QuadKey(x, y, z);
		return $"http://ecn.t3.tiles.virtualearth.net/tiles/a{q}.jpeg?g=1";
	}
}
