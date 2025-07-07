// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Vectors.Svgs;

namespace Wangkanai.Graphics.Vectors.Tests.Svgs;

public class SvgViewBoxTests
{
	[Fact]
	public void Constructor_WithValues_ShouldInitializeCorrectly()
	{
		// Arrange & Act
		var viewBox = new SvgViewBox(10, 20, 100, 200);

		// Assert
		Assert.Equal(10, viewBox.X);
		Assert.Equal(20, viewBox.Y);
		Assert.Equal(100, viewBox.Width);
		Assert.Equal(200, viewBox.Height);
	}

	[Fact]
	public void Default_ShouldReturnDefaultViewBox()
	{
		// Act
		var viewBox = SvgViewBox.Default;

		// Assert
		Assert.Equal(0, viewBox.X);
		Assert.Equal(0, viewBox.Y);
		Assert.Equal(100, viewBox.Width);
		Assert.Equal(100, viewBox.Height);
	}

	[Theory]
	[InlineData(100, 100, 1.0)]
	[InlineData(200, 100, 2.0)]
	[InlineData(100, 200, 0.5)]
	[InlineData(0, 100, 0.0)]  // Height is zero
	public void AspectRatio_WithDimensions_ShouldCalculateCorrectly(double width, double height, double expected)
	{
		// Arrange
		var viewBox = new SvgViewBox(0, 0, width, height);

		// Act
		var aspectRatio = viewBox.AspectRatio;

		// Assert
		Assert.Equal(expected, aspectRatio, precision: 1);
	}

	[Fact]
	public void Equals_SameValues_ShouldReturnTrue()
	{
		// Arrange
		var viewBox1 = new SvgViewBox(10, 20, 100, 200);
		var viewBox2 = new SvgViewBox(10, 20, 100, 200);

		// Act & Assert
		Assert.True(viewBox1.Equals(viewBox2));
		Assert.True(viewBox1 == viewBox2);
		Assert.False(viewBox1 != viewBox2);
	}

	[Fact]
	public void Equals_DifferentValues_ShouldReturnFalse()
	{
		// Arrange
		var viewBox1 = new SvgViewBox(10, 20, 100, 200);
		var viewBox2 = new SvgViewBox(15, 25, 150, 250);

		// Act & Assert
		Assert.False(viewBox1.Equals(viewBox2));
		Assert.False(viewBox1 == viewBox2);
		Assert.True(viewBox1 != viewBox2);
	}

	[Fact]
	public void GetHashCode_SameValues_ShouldReturnSameHash()
	{
		// Arrange
		var viewBox1 = new SvgViewBox(10, 20, 100, 200);
		var viewBox2 = new SvgViewBox(10, 20, 100, 200);

		// Act & Assert
		Assert.Equal(viewBox1.GetHashCode(), viewBox2.GetHashCode());
	}

	[Fact]
	public void ToString_ShouldReturnSpaceSeparatedValues()
	{
		// Arrange
		var viewBox = new SvgViewBox(10, 20, 100, 200);

		// Act
		var result = viewBox.ToString();

		// Assert
		Assert.Equal("10 20 100 200", result);
	}

	[Theory]
	[InlineData("0 0 100 100", 0, 0, 100, 100)]
	[InlineData("10 20 150 250", 10, 20, 150, 250)]
	[InlineData("-5 -10 50 75", -5, -10, 50, 75)]
	[InlineData("0,0,100,100", 0, 0, 100, 100)]  // Comma separated
	[InlineData("10, 20, 150, 250", 10, 20, 150, 250)]  // Mixed separators
	public void Parse_ValidString_ShouldReturnCorrectViewBox(string input, double expectedX, double expectedY, double expectedWidth, double expectedHeight)
	{
		// Act
		var viewBox = SvgViewBox.Parse(input);

		// Assert
		Assert.Equal(expectedX, viewBox.X);
		Assert.Equal(expectedY, viewBox.Y);
		Assert.Equal(expectedWidth, viewBox.Width);
		Assert.Equal(expectedHeight, viewBox.Height);
	}

	[Theory]
	[InlineData("")]
	[InlineData("10 20 100")]  // Missing value
	[InlineData("10 20 100 200 300")]  // Too many values
	[InlineData("a b c d")]  // Invalid numbers
	[InlineData("10 20 abc 200")]  // Mixed valid/invalid
	public void Parse_InvalidString_ShouldThrowException(string input)
	{
		// Act & Assert
		Assert.Throws<FormatException>(() => SvgViewBox.Parse(input));
	}

	[Fact]
	public void Equals_WithObject_ShouldWorkCorrectly()
	{
		// Arrange
		var viewBox = new SvgViewBox(10, 20, 100, 200);
		object sameViewBox = new SvgViewBox(10, 20, 100, 200);
		object differentViewBox = new SvgViewBox(15, 25, 150, 250);
		object notViewBox = "not a viewbox";

		// Act & Assert
		Assert.True(viewBox.Equals(sameViewBox));
		Assert.False(viewBox.Equals(differentViewBox));
		Assert.False(viewBox.Equals(notViewBox));
		Assert.False(viewBox.Equals(null));
	}
}

public class SvgColorSpaceTests
{
	[Fact]
	public void SvgColorSpace_ShouldHaveExpectedValues()
	{
		// Assert
		Assert.True(Enum.IsDefined(typeof(SvgColorSpace), SvgColorSpace.sRGB));
		Assert.True(Enum.IsDefined(typeof(SvgColorSpace), SvgColorSpace.LinearRGB));
		Assert.True(Enum.IsDefined(typeof(SvgColorSpace), SvgColorSpace.DisplayP3));
		Assert.True(Enum.IsDefined(typeof(SvgColorSpace), SvgColorSpace.Rec2020));
		Assert.True(Enum.IsDefined(typeof(SvgColorSpace), SvgColorSpace.Custom));
	}

	[Fact]
	public void SvgColorSpace_ShouldStartWithsRGB()
	{
		// Assert
		Assert.Equal(0, (int)SvgColorSpace.sRGB);
	}
}

public class SvgVersionTests
{
	[Fact]
	public void SvgVersion_ShouldHaveExpectedValues()
	{
		// Assert
		Assert.True(Enum.IsDefined(typeof(SvgVersion), SvgVersion.V1_0));
		Assert.True(Enum.IsDefined(typeof(SvgVersion), SvgVersion.V1_1));
		Assert.True(Enum.IsDefined(typeof(SvgVersion), SvgVersion.V2_0));
	}

	[Fact]
	public void SvgVersion_ShouldStartWithV1_0()
	{
		// Assert
		Assert.Equal(0, (int)SvgVersion.V1_0);
	}
}

public class SvgElementTypeTests
{
	[Fact]
	public void SvgElementType_ShouldHaveExpectedValues()
	{
		// Assert common element types
		Assert.True(Enum.IsDefined(typeof(SvgElementType), SvgElementType.Svg));
		Assert.True(Enum.IsDefined(typeof(SvgElementType), SvgElementType.Group));
		Assert.True(Enum.IsDefined(typeof(SvgElementType), SvgElementType.Path));
		Assert.True(Enum.IsDefined(typeof(SvgElementType), SvgElementType.Rectangle));
		Assert.True(Enum.IsDefined(typeof(SvgElementType), SvgElementType.Circle));
		Assert.True(Enum.IsDefined(typeof(SvgElementType), SvgElementType.Ellipse));
		Assert.True(Enum.IsDefined(typeof(SvgElementType), SvgElementType.Line));
		Assert.True(Enum.IsDefined(typeof(SvgElementType), SvgElementType.Text));
		Assert.True(Enum.IsDefined(typeof(SvgElementType), SvgElementType.Other));
	}

	[Fact]
	public void SvgElementType_ShouldStartWithSvg()
	{
		// Assert
		Assert.Equal(0, (int)SvgElementType.Svg);
	}
}

public class SvgCoordinateSystemTests
{
	[Fact]
	public void SvgCoordinateSystem_ShouldHaveExpectedValues()
	{
		// Assert
		Assert.True(Enum.IsDefined(typeof(SvgCoordinateSystem), SvgCoordinateSystem.Standard));
		Assert.True(Enum.IsDefined(typeof(SvgCoordinateSystem), SvgCoordinateSystem.Cartesian));
		Assert.True(Enum.IsDefined(typeof(SvgCoordinateSystem), SvgCoordinateSystem.Geographic));
		Assert.True(Enum.IsDefined(typeof(SvgCoordinateSystem), SvgCoordinateSystem.WebMercator));
		Assert.True(Enum.IsDefined(typeof(SvgCoordinateSystem), SvgCoordinateSystem.UTM));
		Assert.True(Enum.IsDefined(typeof(SvgCoordinateSystem), SvgCoordinateSystem.Custom));
	}

	[Fact]
	public void SvgCoordinateSystem_ShouldStartWithStandard()
	{
		// Assert
		Assert.Equal(0, (int)SvgCoordinateSystem.Standard);
	}
}