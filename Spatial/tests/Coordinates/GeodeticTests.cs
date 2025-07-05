// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Xunit;

namespace Wangkanai.Spatial.Coordinates;

public class GeodeticTests
{
	[Fact]
	public void DefaultConstructor_ShouldInitializeToZero()
	{
		// Act
		var geodetic = new Geodetic();

		// Assert
		Assert.Equal(0, geodetic.Latitude);
		Assert.Equal(0, geodetic.Longitude);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(45.5, -122.6)]
	[InlineData(-33.9, 151.2)]
	[InlineData(90, 180)]
	[InlineData(-90, -180)]
	public void Constructor_WithParameters_ShouldSetProperties(double latitude, double longitude)
	{
		// Act
		var geodetic = new Geodetic(latitude, longitude);

		// Assert
		Assert.Equal(latitude, geodetic.Latitude);
		Assert.Equal(longitude, geodetic.Longitude);
	}

	[Fact]
	public void ToMeters_ShouldConvertToMercatorProjection()
	{
		// Arrange
		var geodetic = new Geodetic(0, 0);

		// Act
		var meters = geodetic.ToMeters();

		// Assert
		Assert.Equal(0, meters.X, precision: 5);
		Assert.Equal(0, meters.Y, precision: 5);
	}

	[Fact]
	public void ToMeters_WithNonZeroValues_ShouldConvertCorrectly()
	{
		// Arrange
		var geodetic = new Geodetic(45, 45);
		var expectedX = 5009377.085697312;
		var expectedY = 5621521.486192066;

		// Act
		var meters = geodetic.ToMeters();

		// Assert
		Assert.InRange(meters.X, expectedX - 0.1, expectedX + 0.1);
		Assert.InRange(meters.Y, expectedY - 0.1, expectedY + 0.1);
	}

	[Fact]
	public void FromMeters_ShouldConvertFromMercatorProjection()
	{
		// Arrange
		var meters = new Coordinate(0, 0);

		// Act
		var geodetic = Geodetic.FromMeters(meters);

		// Assert
		Assert.Equal(0, geodetic.Latitude, precision: 5);
		Assert.Equal(0, geodetic.Longitude, precision: 5);
	}

	[Fact]
	public void FromMeters_WithNonZeroValues_ShouldConvertCorrectly()
	{
		// Arrange
		var meters = new Coordinate(5009377.085697312, 5621521.486192066);
		var expectedLat = 45.0;
		var expectedLon = 45.0;

		// Act
		var geodetic = Geodetic.FromMeters(meters);

		// Assert
		Assert.InRange(geodetic.Latitude, expectedLat - 0.001, expectedLat + 0.001);
		Assert.InRange(geodetic.Longitude, expectedLon - 0.001, expectedLon + 0.001);
	}

	[Fact]
	public void ConversionRoundTrip_ShouldPreserveCoordinates()
	{
		// Arrange
		var original = new Geodetic(37.7749, -122.4194); // San Francisco coordinates

		// Act
		var meters = original.ToMeters();
		var result = Geodetic.FromMeters(meters);

		// Assert
		Assert.InRange(result.Latitude, original.Latitude - 0.0001, original.Latitude + 0.0001);
		Assert.InRange(result.Longitude, original.Longitude - 0.0001, original.Longitude + 0.0001);
	}

	[Theory]
	[InlineData(0, 0, "0.00000°, 0.00000°")]
	[InlineData(37.7749, -122.4194, "37.77490°, -122.41940°")]
	[InlineData(90, 180, "90.00000°, 180.00000°")]
	public void ToString_ShouldReturnFormattedString(double latitude, double longitude, string expected)
	{
		// Arrange
		var geodetic = new Geodetic(latitude, longitude);

		// Act
		var result = geodetic.ToString();

		// Assert
		Assert.Equal(expected, result);
	}

	[Fact]
	public void Properties_ShouldAllowModification()
	{
		// Arrange
		var geodetic = new Geodetic();

		// Act
		geodetic.Latitude = 45;
		geodetic.Longitude = 90;

		// Assert
		Assert.Equal(45, geodetic.Latitude);
		Assert.Equal(90, geodetic.Longitude);
	}
}
