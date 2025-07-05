// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Spatial;

public class CoordinateTests
{
	[Fact]
	public void CoordinatePair_WhenCreatedWithDefaultConstructor_HasZeroValues()
	{
		var coordinate = new Coordinate();
		Assert.Equal(0, coordinate.X);
		Assert.Equal(0, coordinate.Y);
	}

	[Fact]
	public void CoordinatePair_WhenCreatedWithValues_HasCorrectValues()
	{
		var coordinate = new Coordinate(1, 2);
		Assert.Equal(1, coordinate.X);
		Assert.Equal(2, coordinate.Y);
	}
}
