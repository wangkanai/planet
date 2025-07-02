// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Providers;

/// <summary>Provides functionality for interacting with Google map tiles.</summary>
public static class GoogleProvider
{
	/// <summary>Gets the URL of a Google map tile.</summary>
	/// <param name="x">The x-coordinate of the tile.</param>
	/// <param name="y">The y-coordinate of the tile.</param>
	/// <param name="z">The zoom level of the tile.</param>
	/// <returns>The URL of the Google map tile.</returns>
	public static string GetTileUrl(int x, int y, int z)
		=> $"https://mt1.google.com/vt/lyrs=s&x={x}&y={y}&z={z}";
}
