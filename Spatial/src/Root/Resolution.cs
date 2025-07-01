// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Spatial;

public readonly struct Resolution(
	int    level,
	double unitsPerPixel,
	int    tileWidth  = 512,
	int    tileHeight = 512)
{
	public int    Level         { get; } = level;
	public double UnitsPerPixel { get; } = unitsPerPixel;
	public int    TileWidth     { get; } = tileWidth;
	public int    TileHeight    { get; } = tileHeight;
}
