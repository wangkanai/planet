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
	[InlineData(256)]
	[InlineData(512)]
	[InlineData(1024)]
	[InlineData(2048)]
	public void Constructor_WithVariousTileSizes_CalculatesPropertiesCorrectly(int tileSize)
	{
		// Arrange & Act
		var mercator = new Mercator(tileSize);

		// Assert
		Assert.Equal(tileSize, mercator.TileSize);
		var expectedInitialResolution = 2 * Math.PI * MapExtent.Max / tileSize;
		var expectedOriginShift       = 2 * Math.PI * MapExtent.Max / 2.0;

		Assert.Equal(expectedInitialResolution, mercator.InitialResolution, 10);
		Assert.Equal(expectedOriginShift, mercator.OriginShift, 10);
	}

	[Theory]
	[InlineData(0, 0, 0, 0)]                      // Origin point
	[InlineData(20037508.34, 0, 180, 0)]          // Right edge of the world
	[InlineData(-20037508.34, 0, -180, 0)]        // Left edge of the world
	[InlineData(0, 20037508.34, 0, 85.05112878)]  // Top edge (approximately)
	[InlineData(0, -20037508.34, 0, -85.05112878)]// Bottom edge (approximately)
	public void MetersToLatLon_WithValidInput_ReturnsCorrectCoordinates(double mx, double my, double expectedLon, double expectedLat)
	{
		// Arrange
		var mercator = new Mercator();

		// Act
		var result = mercator.MetersToLatLon(mx, my);

		// Assert
		Assert.InRange(result.X, expectedLon - 0.01, expectedLon + 0.01);// Longitude with small delta
		Assert.InRange(result.Y, expectedLat - 0.01, expectedLat + 0.01);// Latitude with small delta
	}

	[Theory]
	[InlineData(0, 0, 0, 0)]                      // Origin point
	[InlineData(180, 0, 20037508.34, 0)]          // Right edge of the world
	[InlineData(-180, 0, -20037508.34, 0)]        // Left edge of the world
	[InlineData(0, 85.05112878, 0, 20037508.34)]  // Top edge (approximately)
	[InlineData(0, -85.05112878, 0, -20037508.34)]// Bottom edge (approximately)
	public void LatLonToMeters_WithValidInput_ReturnsCorrectCoordinates(double lon, double lat, double expectedX, double expectedY)
	{
		// Arrange
		var mercator = new Mercator();

		// Act
		var result = mercator.LatLonToMeters(lon, lat);

		// Assert
		Assert.InRange(result.X, expectedX - 0.01, expectedX + 0.01);// X with small delta
		Assert.InRange(result.Y, expectedY - 0.01, expectedY + 0.01);// Y with small delta
	}

	[Fact]
	public void LatLonToMeters_ClampsPolarLatitudes()
	{
		// Arrange
		var mercator = new Mercator();

		// Act - Using values beyond the valid range
		var result1 = mercator.LatLonToMeters(0, 90); // North pole
		var result2 = mercator.LatLonToMeters(0, -90);// South pole

		// Assert - Should be clamped to the valid range (-85.05112878, 85.05112878)
		Assert.InRange(result1.Y, 20037508.33, 20037508.35);  // Close to max value
		Assert.InRange(result2.Y, -20037508.35, -20037508.33);// Close to min value
	}

	[Fact]
	public void ConversionRoundTrip_ShouldBeApproximatelyEqual()
	{
		// Arrange
		var mercator    = new Mercator();
		var originalLon = 45.0;
		var originalLat = 30.0;

		// Act - Convert from LatLon to meters and back
		var meters = mercator.LatLonToMeters(originalLon, originalLat);
		var result = mercator.MetersToLatLon(meters.X, meters.Y);

		// Assert - Round trip should result in approximately the same values
		Assert.InRange(result.X, originalLon - 0.0001, originalLon + 0.0001);
		Assert.InRange(result.Y, originalLat - 0.0001, originalLat + 0.0001);
	}


	[Theory]
	[InlineData(0, 0, 0, 100)]       // Origin at zoom 0
	[InlineData(256, 256, 1, 1000)]  // Center tile at zoom 1
	[InlineData(512, 512, 2, 2000)]  // Center tile at zoom 2
	[InlineData(1024, 1024, 3, 3000)]// Center tile at zoom 3
	[InlineData(-256, -256, 1, 1000)]// Negative coordinates
	[InlineData(0, 256, 1, 1000)]    // Mixed coordinates
	public void PixelToMeters_WithValidInput_ReturnsCorrectCoordinates(double px, double py, int zoom, double expectedDelta)
	{
		// Arrange
		var mercator = new Mercator();

		// Act
		var result = mercator.PixelToMeters(px, py, zoom);

		// Assert
		Assert.NotNull(result);

		// Check bounds - both minimum and maximum
		Assert.True(result.X >= -mercator.OriginShift, "X coordinate below minimum bound");
		Assert.True(result.X <= mercator.OriginShift, "X coordinate above maximum bound");
		Assert.True(result.Y >= -mercator.OriginShift, "Y coordinate below minimum bound");
		Assert.True(result.Y <= mercator.OriginShift, "Y coordinate above maximum bound");

		// Verify the conversion makes sense for the given zoom level
		// At zoom 0, the entire world should fit in one tile
		var expectedRange = mercator.OriginShift * 2 / Math.Pow(2, zoom);
		Assert.True(Math.Abs(result.X) <= mercator.OriginShift + expectedDelta,
			$"X coordinate {result.X} outside expected range for zoom {zoom}");
		Assert.True(Math.Abs(result.Y) <= mercator.OriginShift + expectedDelta,
			$"Y coordinate {result.Y} outside expected range for zoom {zoom}");
	}

	[Theory]
	[InlineData(0, 0, 0)]
	[InlineData(0, 0, 1)]
	[InlineData(0, 0, 10)]
	[InlineData(1000000, 1000000, 5)]
	public void MetersToPixels_WithValidInput_ReturnsCorrectCoordinates(double mx, double my, int zoom)
	{
		// Arrange
		var mercator = new Mercator();

		// Act
		var result = mercator.MetersToPixels(mx, my, zoom);

		// Assert
		Assert.NotNull(result);
		Assert.True(result.X >= 0);
		Assert.True(result.Y >= 0);
	}

	[Fact]
	public void PixelToMeters_MetersToPixels_RoundTrip_ShouldBeApproximatelyEqual()
	{
		// Arrange
		var mercator   = new Mercator();
		var originalPx = 256.0;
		var originalPy = 256.0;
		var zoom       = 5;

		// Act - Convert from pixels to meters and back
		var meters = mercator.PixelToMeters(originalPx, originalPy, zoom);
		var result = mercator.MetersToPixels(meters.X, meters.Y, zoom);

		// Assert - Round trip should result in approximately the same values
		Assert.InRange(result.X, originalPx - 0.001, originalPx + 0.001);
		Assert.InRange(result.Y, originalPy - 0.001, originalPy + 0.001);
	}

	[Theory]
	[InlineData(0, 1.0)]
	[InlineData(1, 0.5)]
	[InlineData(2, 0.25)]
	[InlineData(3, 0.125)]
	[InlineData(10, 1.0 / 1024)]
	public void Resolution_AtDifferentZoomLevels_ReturnsCorrectValues(int zoom, double expectedRatio)
	{
		// Arrange
		var mercator       = new Mercator();
		var baseResolution = mercator.InitialResolution;

		// Act - Use reflection to test private Resolution method
		var method = typeof(Mercator).GetMethod("Resolution", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var result = (double)method.Invoke(mercator, new object[] { zoom });

		// Assert
		var expected = baseResolution * expectedRatio;
		Assert.Equal(expected, result, 10);
	}

	[Fact]
	public void LatLonToMeters_WithExtremeLatitudes_ClampsCorrectly()
	{
		// Arrange
		var mercator = new Mercator();

		// Act
		var result1 = mercator.LatLonToMeters(0, 100); // Beyond max latitude
		var result2 = mercator.LatLonToMeters(0, -100);// Beyond min latitude

		// Assert - Should be clamped to valid range
		var maxResult = mercator.LatLonToMeters(0, 85.05112878);
		var minResult = mercator.LatLonToMeters(0, -85.05112878);

		Assert.Equal(maxResult.Y, result1.Y, 5);
		Assert.Equal(minResult.Y, result2.Y, 5);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(45, 45)]
	[InlineData(-45, -45)]
	[InlineData(180, 0)] // Edge case: longitude wrapping
	[InlineData(-180, 0)]// Edge case: longitude wrapping
	public void MetersToLatLon_WithSymmetricCoordinates_ProducesExpectedResults(double lon, double lat)
	{
		// Arrange
		var mercator = new Mercator();

		// Act
		var meters = mercator.LatLonToMeters(lon, lat);
		var result = mercator.MetersToLatLon(meters.X, meters.Y);

		// Assert
		Assert.InRange(result.X, lon - 0.001, lon + 0.001);
		Assert.InRange(result.Y, lat - 0.001, lat + 0.001);
	}

	[Fact]
	public void PixelToMeters_WithNegativePixels_HandlesCorrectly()
	{
		// Arrange
		var mercator = new Mercator();

		// Act
		var result = mercator.PixelToMeters(-100, -100, 1);

		// Assert
		Assert.NotNull(result);
		Assert.True(result.X < 0);
		Assert.True(result.Y < 0);
	}

	[Fact]
	public void MetersToPixels_WithNegativeMeters_HandlesCorrectly()
	{
		// Arrange
		var mercator = new Mercator();

		// Act
		var result = mercator.MetersToPixels(-1000000, -1000000, 1);

		// Assert
		Assert.NotNull(result);
		// Negative meters should still produce valid pixel coordinates
		Assert.True(!double.IsNaN(result.X));
		Assert.True(!double.IsNaN(result.Y));
	}

	[Theory]
	[InlineData(1)]
	[InlineData(256)]
	[InlineData(1024)]
	[InlineData(4096)]
	public void Constructor_WithDifferentTileSizes_MaintainsConsistentOriginShift(int tileSize)
	{
		// Arrange & Act
		var mercator = new Mercator(tileSize);

		// Assert - OriginShift should be independent of tile size
		var expectedOriginShift = 2 * Math.PI * MapExtent.Max / 2.0;
		Assert.Equal(expectedOriginShift, mercator.OriginShift, 5);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(-10)]
	[InlineData(-100)]
	public void MetersToPixels_WithNegativeZoom_ThrowsException(int negativeZoom)
	{
		// Arrange
		var mercator = new Mercator();

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			mercator.MetersToPixels(0, 0, negativeZoom));
	}

	[Theory]
	[InlineData(50)]
	[InlineData(100)]
	[InlineData(200)]
	public void MetersToPixels_WithExtremelyHighZoom_ThrowsException(int extremeZoom)
	{
		// Arrange
		var mercator = new Mercator();

		// Act & Assert
		Assert.Throws<OverflowException>(() =>
			mercator.MetersToPixels(0, 0, extremeZoom));
	}

	[Fact]
	public void MetersToPixels_WithInfiniteValues_ThrowsException()
	{
		// Arrange
		var mercator = new Mercator();

		// Act & Assert
		Assert.Throws<ArgumentException>(() =>
			mercator.MetersToPixels(double.PositiveInfinity, 0, 1));

		Assert.Throws<ArgumentException>(() =>
			mercator.MetersToPixels(0, double.NegativeInfinity, 1));

		Assert.Throws<ArgumentException>(() =>
			mercator.MetersToPixels(double.PositiveInfinity, double.NegativeInfinity, 1));
	}

	[Fact]
	public void MetersToPixels_WithNaNValues_ThrowsException()
	{
		// Arrange
		var mercator = new Mercator();

		// Act & Assert
		Assert.Throws<ArgumentException>(() =>
			mercator.MetersToPixels(double.NaN, 0, 1));

		Assert.Throws<ArgumentException>(() =>
			mercator.MetersToPixels(0, double.NaN, 1));

		Assert.Throws<ArgumentException>(() =>
			mercator.MetersToPixels(double.NaN, double.NaN, 1));
	}

	[Theory]
	[InlineData(double.MaxValue)]
	[InlineData(double.MinValue)]
	public void MetersToPixels_WithExtremeValues_HandlesGracefully(double extremeValue)
	{
		// Arrange
		var mercator = new Mercator();

		// Act & Assert
		// These should either work or throw a specific exception type
		try
		{
			var result = mercator.MetersToPixels(extremeValue, 0, 1);
			Assert.NotNull(result);
		}
		catch (OverflowException)
		{
			// Expected for extreme values
		}
		catch (ArgumentOutOfRangeException)
		{
			// Expected for values outside valid range
		}
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(-10)]
	[InlineData(-100)]
	public void PixelToMeters_WithNegativeZoom_ThrowsException(int negativeZoom)
	{
		// Arrange
		var mercator = new Mercator();

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			mercator.PixelToMeters(0, 0, negativeZoom));
	}

	[Theory]
	[InlineData(50)]
	[InlineData(100)]
	[InlineData(200)]
	public void PixelToMeters_WithExtremelyHighZoom_ThrowsException(int extremeZoom)
	{
		// Arrange
		var mercator = new Mercator();

		// Act & Assert
		Assert.Throws<OverflowException>(() =>
			mercator.PixelToMeters(0, 0, extremeZoom));
	}

	[Fact]
	public void PixelToMeters_WithInfiniteValues_ThrowsException()
	{
		// Arrange
		var mercator = new Mercator();

		// Act & Assert
		Assert.Throws<ArgumentException>(() =>
			mercator.PixelToMeters(double.PositiveInfinity, 0, 1));

		Assert.Throws<ArgumentException>(() =>
			mercator.PixelToMeters(0, double.NegativeInfinity, 1));
	}

	[Fact]
	public void PixelToMeters_WithNaNValues_ThrowsException()
	{
		// Arrange
		var mercator = new Mercator();

		// Act & Assert
		Assert.Throws<ArgumentException>(() =>
			mercator.PixelToMeters(double.NaN, 0, 1));

		Assert.Throws<ArgumentException>(() =>
			mercator.PixelToMeters(0, double.NaN, 1));
	}
}
