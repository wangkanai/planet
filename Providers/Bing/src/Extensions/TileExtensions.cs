// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Text;

namespace Wangkanai.Planet.Providers.Bing.Extensions;

/// <summary>Provides extension methods for working with tile coordinates.</summary>
public static class TileExtensions
{
	/// <summary>Converts tile XYZ coordinates into a QuadKey at a specified level of detail.</summary>
	/// <param name="x">Tile X coordinate</param>
	/// <param name="y">Tile Y coordinate</param>
	/// <param name="zoom">zoom, from 1 (lowest detail) to 23 (highest detail)</param>
	/// <returns>A string containing the QuadKey</returns>
	public static string QuadKey(int x, int y, int zoom)
	{
		var quadKey = new StringBuilder();

		for (var i = zoom; i > 0; i--)
		{
			var digit = '0';
			var mask  = 1 << (i - 1);

			if ((x & mask) != 0)
				digit++;

			if ((y & mask) != 0)
			{
				digit++;
				digit++;
			}

			quadKey.Append(digit);
		}

		return quadKey.ToString();
	}
}
