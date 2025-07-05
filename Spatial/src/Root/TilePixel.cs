// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Spatial;

/// <summary>Represents a pixel within a tile, defined by its X, Y, and Z coordinates.</summary>
public class TilePixel
{
	/// <summary>Gets or sets the X-coordinate of the pixel.</summary>
	public int X { get; set; }

	/// <summary>Gets or sets the Y-coordinate of the pixel.</summary>
	public int Y { get; set; }

	/// <summary>Gets or sets the Z-coordinate of the pixel, typically representing the zoom level.</summary>
	public int Z { get; set; }
}
