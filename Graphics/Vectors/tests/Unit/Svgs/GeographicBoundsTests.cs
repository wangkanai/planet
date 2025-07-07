// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Vectors.Svgs;

namespace Wangkanai.Graphics.Vectors.Tests.Svgs;

public class GeographicBoundsTests
{
	[Fact]
	public void Constructor_Default_ShouldInitializeToZero()
	{
		// Arrange & Act
		var bounds = new GeographicBounds();

		// Assert
		Assert.Equal(0, bounds.MinLatitude);
		Assert.Equal(0, bounds.MaxLatitude);
		Assert.Equal(0, bounds.MinLongitude);
		Assert.Equal(0, bounds.MaxLongitude);
	}

	[Fact]
	public void Constructor_WithValues_ShouldInitializeCorrectly()
	{
		// Arrange & Act
		var bounds = new GeographicBounds(10, 20, 30, 40);

		// Assert
		Assert.Equal(10, bounds.MinLatitude);
		Assert.Equal(20, bounds.MaxLatitude);
		Assert.Equal(30, bounds.MinLongitude);
		Assert.Equal(40, bounds.MaxLongitude);
	}

	[Theory]
	[InlineData(15, 35, true)]  // Inside bounds
	[InlineData(10, 30, true)]  // On boundary
	[InlineData(20, 40, true)]  // On boundary
	[InlineData(5, 35, false)]  // Outside lat
	[InlineData(15, 25, false)] // Outside lon
	public void Contains_WithCoordinates_ShouldReturnExpectedResult(double lat, double lon, bool expected)
	{
		// Arrange
		var bounds = new GeographicBounds(10, 20, 30, 40);

		// Act
		var result = bounds.Contains(lat, lon);

		// Assert
		Assert.Equal(expected, result);
	}

	[Fact]
	public void Width_ShouldReturnCorrectValue()
	{
		// Arrange
		var bounds = new GeographicBounds(10, 20, 30, 50);

		// Act
		var width = bounds.Width;

		// Assert
		Assert.Equal(20, width);
	}

	[Fact]
	public void Height_ShouldReturnCorrectValue()
	{
		// Arrange
		var bounds = new GeographicBounds(10, 30, 40, 50);

		// Act
		var height = bounds.Height;

		// Assert
		Assert.Equal(20, height);
	}

	[Fact]
	public void Center_ShouldReturnCorrectValue()
	{
		// Arrange
		var bounds = new GeographicBounds(10, 30, 40, 60);

		// Act
		var center = bounds.Center;

		// Assert
		Assert.Equal(20, center.Latitude);
		Assert.Equal(50, center.Longitude);
	}

	[Fact]
	public void Expand_WithPoint_ShouldExpandBounds()
	{
		// Arrange
		var bounds = new GeographicBounds(10, 20, 30, 40);

		// Act
		bounds.Expand(5, 50);

		// Assert
		Assert.Equal(5, bounds.MinLatitude);
		Assert.Equal(20, bounds.MaxLatitude);
		Assert.Equal(30, bounds.MinLongitude);
		Assert.Equal(50, bounds.MaxLongitude);
	}

	[Theory]
	[InlineData(10, 20, 30, 40, true)]     // Valid bounds
	[InlineData(-90, 90, -180, 180, true)] // World bounds
	[InlineData(20, 10, 30, 40, false)]    // Invalid lat order
	[InlineData(10, 20, 40, 30, false)]    // Invalid lon order
	[InlineData(-100, 20, 30, 40, false)]  // Invalid lat range
	[InlineData(10, 100, 30, 40, false)]   // Invalid lat range
	[InlineData(10, 20, -200, 40, false)]  // Invalid lon range
	[InlineData(10, 20, 30, 200, false)]   // Invalid lon range
	public void IsValid_WithBounds_ShouldReturnExpectedResult(double minLat, double maxLat, double minLon, double maxLon, bool expected)
	{
		// Arrange
		var bounds = new GeographicBounds(minLat, maxLat, minLon, maxLon);

		// Act
		var result = bounds.IsValid();

		// Assert
		Assert.Equal(expected, result);
	}

	[Fact]
	public void ToString_ShouldReturnCorrectFormat()
	{
		// Arrange
		var bounds = new GeographicBounds(10.12345, 20.67890, 30.11111, 40.22222);

		// Act
		var result = bounds.ToString();

		// Assert
		Assert.Equal("10.12345,30.11111,20.67890,40.22222", result);
	}

	[Theory]
	[InlineData("10,30,20,40", 10, 20, 30, 40)]
	[InlineData("-90,-180,90,180", -90, 90, -180, 180)]
	[InlineData("10.5,30.5,20.5,40.5", 10.5, 20.5, 30.5, 40.5)]
	public void Parse_ValidString_ShouldReturnCorrectBounds(string input, double expectedMinLat, double expectedMaxLat, double expectedMinLon, double expectedMaxLon)
	{
		// Act
		var bounds = GeographicBounds.Parse(input);

		// Assert
		Assert.Equal(expectedMinLat, bounds.MinLatitude);
		Assert.Equal(expectedMaxLat, bounds.MaxLatitude);
		Assert.Equal(expectedMinLon, bounds.MinLongitude);
		Assert.Equal(expectedMaxLon, bounds.MaxLongitude);
	}

	[Theory]
	[InlineData("")]
	[InlineData("10,20,30")]
	[InlineData("10,20,30,40,50")]
	[InlineData("a,b,c,d")]
	[InlineData("10,20,30,abc")]
	public void Parse_InvalidString_ShouldThrowException(string input)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => GeographicBounds.Parse(input));
	}

	[Fact]
	public void World_ShouldReturnWorldBounds()
	{
		// Act
		var world = GeographicBounds.World;

		// Assert
		Assert.Equal(-90, world.MinLatitude);
		Assert.Equal(90, world.MaxLatitude);
		Assert.Equal(-180, world.MinLongitude);
		Assert.Equal(180, world.MaxLongitude);
	}

	[Fact]
	public void WebMercator_ShouldReturnWebMercatorBounds()
	{
		// Act
		var webMercator = GeographicBounds.WebMercator;

		// Assert
		Assert.Equal(-85.0511, webMercator.MinLatitude);
		Assert.Equal(85.0511, webMercator.MaxLatitude);
		Assert.Equal(-180, webMercator.MinLongitude);
		Assert.Equal(180, webMercator.MaxLongitude);
	}
}
