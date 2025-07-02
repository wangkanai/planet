// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0


using Wangkanai.Planet.Providers.Extensions;

namespace Wangkanai.Planet.Providers.Bing.Tests;

public class TileExtensionsTests
{
	[Fact]
	public void QuadKey_WhenCalledWithValidCoordinates_ReturnsCorrectQuadKey()
	{
		// Arrange
		var x               = 3;
		var y               = 5;
		var zoom            = 3;
		var expectedQuadKey = "213";

		// Act
		var result = TileExtensions.QuadKey(x, y, zoom);

		// Assert
		Assert.Equal(expectedQuadKey, result);
	}

	[Fact]
	public void QuadKey_WhenCalledWithZeroCoordinates_ReturnsQuadKeyWithZeros()
	{
		// Arrange
		var x               = 0;
		var y               = 0;
		var zoom            = 3;
		var expectedQuadKey = "000";

		// Act
		var result = TileExtensions.QuadKey(x, y, zoom);

		// Assert
		Assert.Equal(expectedQuadKey, result);
	}

	[Fact]
	public void QuadKey_WhenCalledWithMaxCoordinates_ReturnsQuadKeyWithThrees()
	{
		// Arrange
		var zoom            = 3;
		var x               = (1 << zoom) - 1; // 7 for zoom=3
		var y               = (1 << zoom) - 1; // 7 for zoom=3
		var expectedQuadKey = "333";

		// Act
		var result = TileExtensions.QuadKey(x, y, zoom);

		// Assert
		Assert.Equal(expectedQuadKey, result);
	}

	[Fact]
	public void QuadKey_WhenCalledWithZoom1_ReturnsSingleDigit()
	{
		// Arrange
		var x    = 0;
		var y    = 0;
		var zoom = 1;

		// Act
		var result = TileExtensions.QuadKey(x, y, zoom);

		// Assert
		Assert.Equal("0", result);
		Assert.Equal(zoom, result.Length);
	}

	[Fact]
	public void QuadKey_WhenCalledWithZoom2_ReturnsTwoDigits()
	{
		// Arrange
		var x    = 2;
		var y    = 1;
		var zoom = 2;

		// Act
		var result = TileExtensions.QuadKey(x, y, zoom);

		// Assert
		Assert.Equal("12", result);
		Assert.Equal(zoom, result.Length);
	}

	[Theory]
	[InlineData(0, 0, 4, "0000")]
	[InlineData(15, 15, 4, "3333")]
	[InlineData(2, 3, 3, "032")]
	[InlineData(5, 2, 4, "0121")]
	[InlineData(7, 3, 4, "0133")]
	[InlineData(8, 9, 5, "03002")]
	[InlineData(16, 16, 5, "30000")]
	public void QuadKey_WithVariousInputs_ReturnsExpectedResults(int x, int y, int zoom, string expected)
	{
		// Act
		var result = TileExtensions.QuadKey(x, y, zoom);

		// Assert
		Assert.Equal(expected, result);
	}

	[Fact]
	public void QuadKey_DifferentCoordinatesAtSameZoom_ProduceDifferentQuadKeys()
	{
		// Arrange
		var zoom = 4;
		var coordinates = new[]
		                  {
			                  (x: 0, y: 0),
			                  (x: 1, y: 0),
			                  (x: 0, y: 1),
			                  (x: 1, y: 1),
			                  (x: 8, y: 8),
			                  (x: 15, y: 15)
		                  };

		// Act
		var results = coordinates.Select(c => TileExtensions.QuadKey(c.x, c.y, zoom)).ToList();

		// Assert
		Assert.Equal(coordinates.Length, results.Distinct().Count());
	}

	[Fact]
	public void QuadKey_BitPatternTest_EnsuresCorrectQuadKeyGeneration()
	{
		// This test verifies the bit pattern handling for each quadrant at zoom level 1

		// Quadrant 0: x=0, y=0
		Assert.Equal("0", TileExtensions.QuadKey(0, 0, 1));

		// Quadrant 1: x=1, y=0
		Assert.Equal("1", TileExtensions.QuadKey(1, 0, 1));

		// Quadrant 2: x=0, y=1
		Assert.Equal("2", TileExtensions.QuadKey(0, 1, 1));

		// Quadrant 3: x=1, y=1
		Assert.Equal("3", TileExtensions.QuadKey(1, 1, 1));
	}

	[Fact]
	public void QuadKey_ZoomLevel23_GeneratesCorrectLength()
	{
		// Arrange
		var zoom = 23; // Maximum zoom level typically used
		var x    = 1234567;
		var y    = 7654321;

		// Act
		var result = TileExtensions.QuadKey(x, y, zoom);

		// Assert
		Assert.Equal(zoom, result.Length);
	}

	[Fact]
	public void QuadKey_LengthEqualsZoomLevel_ForAllZoomLevels()
	{
		// Test that the quadkey length always equals the zoom level
		for (var zoom = 1; zoom <= 10; zoom++)
		{
			var x = zoom * 2;
			var y = zoom * 3;

			var result = TileExtensions.QuadKey(x, y, zoom);

			Assert.Equal(zoom, result.Length);
		}
	}

	[Fact]
	public void QuadKey_FirstDigitEncodesMostSignificantBit()
	{
		// This test verifies that the first digit of the quadkey
		// encodes the most significant bit position

		// For zoom=3, the first digit should encode bit position 2
		Assert.Equal('0', TileExtensions.QuadKey(0, 0, 3)[0]);
		Assert.Equal('1', TileExtensions.QuadKey(4, 0, 3)[0]);
		Assert.Equal('2', TileExtensions.QuadKey(0, 4, 3)[0]);
		Assert.Equal('3', TileExtensions.QuadKey(4, 4, 3)[0]);
	}
}
