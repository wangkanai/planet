// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Spatial.Coordinates;

public class MercatorTests
{
	[Fact]
	public void Constructor_WithDefaultSize_InitializesCorrectly()
	{
		// Arrange & Act
		var mercator = new Mercator();

		// Assert
		Assert.Equal(512, mercator.TileSize);
		Assert.NotEqual(0, mercator.InitialResolution);
		Assert.NotEqual(0, mercator.OriginShift);
	}

	[Fact]
	public void Constructor_WithCustomSize_InitializesCorrectly()
	{
		// Arrange & Act
		var mercator = new Mercator(1024);

		// Assert
		Assert.Equal(1024, mercator.TileSize);
		Assert.NotEqual(0, mercator.InitialResolution);
		Assert.NotEqual(0, mercator.OriginShift);
	}

	[Theory]
	[InlineData(0, 0, 0, 0)] // Origin point
	[InlineData(20037508.34, 0, 180, 0)] // Right edge of the world
	[InlineData(-20037508.34, 0, -180, 0)] // Left edge of the world
	[InlineData(0, 20037508.34, 0, 85.05112878)] // Top edge (approximately)
	[InlineData(0, -20037508.34, 0, -85.05112878)] // Bottom edge (approximately)
	public void MetersToLatLon_WithValidInput_ReturnsCorrectCoordinates(double mx, double my, double expectedLon, double expectedLat)
	{
		// Arrange
		var mercator = new Mercator();

		// Act
		var result = mercator.MetersToLatLon(mx, my);

		// Assert
		Assert.InRange(result.X, expectedLon - 0.01, expectedLon + 0.01); // Longitude with small delta
		Assert.InRange(result.Y, expectedLat - 0.01, expectedLat + 0.01); // Latitude with small delta
	}

	[Theory]
	[InlineData(0, 0, 0, 0)] // Origin point
	[InlineData(180, 0, 20037508.34, 0)] // Right edge of the world
	[InlineData(-180, 0, -20037508.34, 0)] // Left edge of the world
	[InlineData(0, 85.05112878, 0, 20037508.34)] // Top edge (approximately)
	[InlineData(0, -85.05112878, 0, -20037508.34)] // Bottom edge (approximately)
	public void LatLonToMeters_WithValidInput_ReturnsCorrectCoordinates(double lon, double lat, double expectedX, double expectedY)
	{
		// Arrange
		var mercator = new Mercator();

		// Act
		var result = mercator.LatLonToMeters(lon, lat);

		// Assert
		Assert.InRange(result.X, expectedX - 0.01, expectedX + 0.01); // X with small delta
		Assert.InRange(result.Y, expectedY - 0.01, expectedY + 0.01); // Y with small delta
	}

	[Fact]
	public void LatLonToMeters_ClampsPolarLatitudes()
	{
		// Arrange
		var mercator = new Mercator();

		// Act - Using values beyond the valid range
		var result1 = mercator.LatLonToMeters(0, 90); // North pole
		var result2 = mercator.LatLonToMeters(0, -90); // South pole

		// Assert - Should be clamped to the valid range (-85.05112878, 85.05112878)
		Assert.InRange(result1.Y, 20037508.33, 20037508.35); // Close to max value
		Assert.InRange(result2.Y, -20037508.35, -20037508.33); // Close to min value
	}

	[Fact]
	public void ConversionRoundTrip_ShouldBeApproximatelyEqual()
	{
		// Arrange
		var mercator = new Mercator();
		var originalLon = 45.0;
		var originalLat = 30.0;

		// Act - Convert from LatLon to meters and back
		var meters = mercator.LatLonToMeters(originalLon, originalLat);
		var result = mercator.MetersToLatLon(meters.X, meters.Y);

		// Assert - Round trip should result in approximately the same values
		Assert.InRange(result.X, originalLon - 0.0001, originalLon + 0.0001);
		Assert.InRange(result.Y, originalLat - 0.0001, originalLat + 0.0001);
	}
}
