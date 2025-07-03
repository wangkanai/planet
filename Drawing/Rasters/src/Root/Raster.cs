// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Drawing.Rasters;

/// <summary>Represents a raster image</summary>
public class Raster : IRaster
{
	public int Width  { get; set; }
	public int Height { get; set; }

	public void Dispose()
	{
		// Implementation for resource cleanup
		GC.SuppressFinalize(this);
	}
}
