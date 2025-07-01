// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Xunit;

namespace Wangkanai.Planet.Spatial.Tests;

public class ExtentTests
{
	[Fact]
	public void Constructor_WithValidCoordinates_SetsPropertiesCorrectly()
	{
		// Arrange
		var minX = 10.0;
		var minY = 20.0;
		var maxX = 30.0;
		var maxY = 40.0;

		// Act
		var extent = new Extent(minX, minY, maxX, maxY);

		// Assert
		Assert.Equal(minX, extent.MinX);
		Assert.Equal(minY, extent.MinY);
		Assert.Equal(maxX, extent.MaxX);
		Assert.Equal(maxY, extent.MaxY);
	}

	[Fact]
	public void Constructor_WithEqualMinAndMaxCoordinates_SetsPropertiesCorrectly()
	{
		// Arrange & Act
		var extent = new Extent(10.0, 20.0, 10.0, 20.0);

		// Assert
		Assert.Equal(10.0, extent.MinX);
		Assert.Equal(20.0, extent.MinY);
		Assert.Equal(10.0, extent.MaxX);
		Assert.Equal(20.0, extent.MaxY);
	}

	[Fact]
	public void Constructor_WithMinXGreaterThanMaxX_ThrowsArgumentException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new Extent(30.0, 20.0, 10.0, 40.0));
		Assert.Equal("X Min cannot be greater than Max", exception.Message);
	}

	[Fact]
	public void Constructor_WithMinYGreaterThanMaxY_ThrowsArgumentException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new Extent(10.0, 40.0, 30.0, 20.0));
		Assert.Equal("Y Min cannot be greater than Max", exception.Message);
	}

	[Fact]
	public void Constructor_WithBothMinGreaterThanMax_ThrowsArgumentException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new Extent(30.0, 40.0, 10.0, 20.0));
		Assert.Equal("X Min cannot be greater than Max", exception.Message);
	}

	[Fact]
	public void CenterX_CalculatesCorrectly()
	{
		// Arrange
		var extent = new Extent(10.0, 20.0, 30.0, 40.0);

		// Act & Assert
		Assert.Equal(20.0, extent.CenterX);
	}

	[Fact]
	public void CenterY_CalculatesCorrectly()
	{
		// Arrange
		var extent = new Extent(10.0, 20.0, 30.0, 40.0);

		// Act & Assert
		Assert.Equal(30.0, extent.CenterY);
	}

	[Fact]
	public void Width_CalculatesCorrectly()
	{
		// Arrange
		var extent = new Extent(10.0, 20.0, 30.0, 40.0);

		// Act & Assert
		Assert.Equal(20.0, extent.Width);
	}

	[Fact]
	public void Height_CalculatesCorrectly()
	{
		// Arrange
		var extent = new Extent(10.0, 20.0, 30.0, 40.0);

		// Act & Assert
		Assert.Equal(20.0, extent.Height);
	}

	[Fact]
	public void Area_CalculatesCorrectly()
	{
		// Arrange
		var extent = new Extent(10.0, 20.0, 30.0, 40.0);

		// Act & Assert
		Assert.Equal(400.0, extent.Area);
	}

	[Fact]
	public void Area_WithZeroWidth_ReturnsZero()
	{
		// Arrange
		var extent = new Extent(10.0, 20.0, 10.0, 40.0);

		// Act & Assert
		Assert.Equal(0.0, extent.Area);
	}

	[Fact]
	public void Area_WithZeroHeight_ReturnsZero()
	{
		// Arrange
		var extent = new Extent(10.0, 20.0, 30.0, 20.0);

		// Act & Assert
		Assert.Equal(0.0, extent.Area);
	}

	[Fact]
	public void Properties_WithNegativeCoordinates_CalculatesCorrectly()
	{
		// Arrange
		var extent = new Extent(-30.0, -40.0, -10.0, -20.0);

		// Act & Assert
		Assert.Equal(-20.0, extent.CenterX);
		Assert.Equal(-30.0, extent.CenterY);
		Assert.Equal(20.0, extent.Width);
		Assert.Equal(20.0, extent.Height);
		Assert.Equal(400.0, extent.Area);
	}

	[Fact]
	public void Properties_WithMixedSignCoordinates_CalculatesCorrectly()
	{
		// Arrange
		var extent = new Extent(-10.0, -20.0, 10.0, 20.0);

		// Act & Assert
		Assert.Equal(0.0, extent.CenterX);
		Assert.Equal(0.0, extent.CenterY);
		Assert.Equal(20.0, extent.Width);
		Assert.Equal(40.0, extent.Height);
		Assert.Equal(800.0, extent.Area);
	}

	[Fact]
	public void Properties_WithDecimalCoordinates_CalculatesCorrectly()
	{
		// Arrange
		var extent = new Extent(1.5, 2.5, 3.5, 4.5);

		// Act & Assert
		Assert.Equal(2.5, extent.CenterX);
		Assert.Equal(3.5, extent.CenterY);
		Assert.Equal(2.0, extent.Width);
		Assert.Equal(2.0, extent.Height);
		Assert.Equal(4.0, extent.Area);
	}
}
