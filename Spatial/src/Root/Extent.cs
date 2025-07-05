// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Spatial;

/// <summary>Represents a rectangular extent with minimum and maximum X and Y coordinates.</summary>
public readonly struct Extent
{
	public double MinX { get; }
	public double MinY { get; }
	public double MaxX { get; }
	public double MaxY { get; }

	public double CenterX => (MinX + MaxX) / 2;
	public double CenterY => (MinY + MaxY) / 2;
	public double Width   => MaxX - MinX;
	public double Height  => MaxY - MinY;
	public double Area    => Width * Height;

	/// <summary>Represents a rectangular extent with minimum and maximum X and Y coordinates.</summary>
	public Extent(double minX, double minY, double maxX, double maxY)
	{
		MinX = minX;
		MinY = minY;
		MaxX = maxX;
		MaxY = maxY;

		if (minX > maxX)
			throw new ArgumentException("X Min cannot be greater than Max");
		if (minY > maxY)
			throw new ArgumentException("Y Min cannot be greater than Max");
	}
}
