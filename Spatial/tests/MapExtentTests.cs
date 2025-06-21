// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Spatial.Tests;

public class MapExtentTests
{
	[Fact]
	public void MapExtent_WhenCreatedWithDefaultConstructor_HasFullMercatorExtent()
	{
		// Arrange & Act
		var mapExtent = new MapExtent();

		// Assert
		// Default constructor sets the map extent to the full Spherical Mercator projection range
		Assert.Equal(Math.PI * MapExtent.Max, mapExtent.North);
		Assert.Equal(Math.PI * MapExtent.Max, mapExtent.East);
		Assert.Equal(Math.PI * -MapExtent.Max, mapExtent.South);
		Assert.Equal(Math.PI * -MapExtent.Max, mapExtent.West);
	}

	[Fact]
	public void MapExtent_WhenCreatedWithParameters_HasSpecifiedValues()
	{
		// Arrange
		double north = 10000;
		double east  = 20000;
		double south = -10000;
		double west  = -20000;

		// Act
		var mapExtent = new MapExtent(north, east, south, west);

		// Assert
		Assert.Equal(north, mapExtent.North);
		Assert.Equal(east, mapExtent.East);
		Assert.Equal(south, mapExtent.South);
		Assert.Equal(west, mapExtent.West);
	}

	[Fact]
	public void Width_CalculatesCorrectly()
	{
		// Arrange
		var mapExtent = new MapExtent(10000, 20000, -10000, -20000);

		// Act
		var width = mapExtent.Width;

		// Assert
		Assert.Equal(40000, width);
	}

	[Fact]
	public void Height_CalculatesCorrectly()
	{
		// Arrange
		var mapExtent = new MapExtent(10000, 20000, -10000, -20000);

		// Act
		var height = mapExtent.Height;

		// Assert
		Assert.Equal(20000, height);
	}

	[Fact]
	public void GetCenter_ReturnsCorrectCenter()
	{
		// Arrange
		var mapExtent = new MapExtent(10000, 20000, -10000, -20000);

		// Act
		var center = mapExtent.GetCenter();

		// Assert
		Assert.Equal(0, center.X);
		Assert.Equal(0, center.Y);
	}

	[Theory]
	[InlineData(0, 0, true)]         // Center point
	[InlineData(15000, 5000, true)]  // Point inside
	[InlineData(20000, 10000, true)] // Point on edge
	[InlineData(25000, 5000, false)] // Point outside (X too large)
	[InlineData(15000, 15000, false)]// Point outside (Y too large)
	[InlineData(-25000, 0, false)]   // Point outside (X too small)
	[InlineData(0, -15000, false)]   // Point outside (Y too small)
	public void Contains_ChecksPointCorrectly(double x, double y, bool expected)
	{
		// Arrange
		var mapExtent = new MapExtent(10000, 20000, -10000, -20000);
		var point     = new Coordinate(x, y);

		// Act
		var result = mapExtent.Contains(point);

		// Assert
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(10000, 20000, -10000, -20000, true)]// Valid extent
	[InlineData(MapExtent.Max + 1, 0, 0, 0, false)] // North too large
	[InlineData(0, MapExtent.Max + 1, 0, 0, false)] // East too large
	[InlineData(0, 0, -MapExtent.Max - 1, 0, false)]// South too small
	[InlineData(0, 0, 0, -MapExtent.Max - 1, false)]// West too small
	[InlineData(0, 0, 10, 0, false)]                // South > North
	[InlineData(0, 0, 0, 10, false)]                // West > East
	public void IsValid_ChecksExtentCorrectly(double north, double east, double south, double west, bool expected)
	{
		// Arrange
		var mapExtent = new MapExtent(north, east, south, west);

		// Act
		var result = mapExtent.IsValid();

		// Assert
		Assert.Equal(expected, result);
	}

	[Fact]
	public void ToString_FormatsCorrectly()
	{
		// Arrange
		var mapExtent = new MapExtent(10000, 20000, -10000, -20000);

		// Act
		var result = mapExtent.ToString();

		// Assert
		Assert.Equal("N:10000m, E:20000m, S:-10000m, W:-20000m", result);
	}
}
