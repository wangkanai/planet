// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

public class Jpeg2000ProgressionTests
{
	[Theory]
	[InlineData(Jpeg2000Progression.LayerResolutionComponentPosition, 0)]
	[InlineData(Jpeg2000Progression.ResolutionLayerComponentPosition, 1)]
	[InlineData(Jpeg2000Progression.ResolutionPositionComponentLayer, 2)]
	[InlineData(Jpeg2000Progression.PositionComponentResolutionLayer, 3)]
	[InlineData(Jpeg2000Progression.ComponentPositionResolutionLayer, 4)]
	public void ProgressionValues_ShouldMatchConstants(Jpeg2000Progression progression, byte expectedValue)
	{
		// Act
		var actualValue = (byte)progression;

		// Assert
		Assert.Equal(expectedValue, actualValue);
	}

	[Theory]
	[InlineData(Jpeg2000Progression.LayerResolutionComponentPosition, "Quality progression - each layer adds quality to entire image")]
	[InlineData(Jpeg2000Progression.ResolutionLayerComponentPosition, "Resolution progression - each level adds resolution detail")]
	[InlineData(Jpeg2000Progression.ResolutionPositionComponentLayer, "Mixed resolution and spatial progression")]
	[InlineData(Jpeg2000Progression.PositionComponentResolutionLayer, "Spatial progression - regions transmitted sequentially")]
	[InlineData(Jpeg2000Progression.ComponentPositionResolutionLayer, "Component progression - color channels transmitted separately")]
	public void GetDescription_ShouldReturnCorrectDescription(Jpeg2000Progression progression, string expectedDescription)
	{
		// Act
		var description = progression.GetDescription();

		// Assert
		Assert.Equal(expectedDescription, description);
	}

	[Theory]
	[InlineData(Jpeg2000Progression.LayerResolutionComponentPosition, "Progressive quality enhancement, web streaming")]
	[InlineData(Jpeg2000Progression.ResolutionLayerComponentPosition, "Progressive resolution, thumbnail generation")]
	[InlineData(Jpeg2000Progression.ResolutionPositionComponentLayer, "Large images with spatial access patterns")]
	[InlineData(Jpeg2000Progression.PositionComponentResolutionLayer, "Region-of-interest streaming, tiled access")]
	[InlineData(Jpeg2000Progression.ComponentPositionResolutionLayer, "Multi-spectral imagery, scientific data")]
	public void GetRecommendedUseCase_ShouldReturnCorrectUseCase(Jpeg2000Progression progression, string expectedUseCase)
	{
		// Act
		var useCase = progression.GetRecommendedUseCase();

		// Assert
		Assert.Equal(expectedUseCase, useCase);
	}

	[Theory]
	[InlineData(Jpeg2000Progression.LayerResolutionComponentPosition, false)]
	[InlineData(Jpeg2000Progression.ResolutionLayerComponentPosition, false)]
	[InlineData(Jpeg2000Progression.ResolutionPositionComponentLayer, true)]
	[InlineData(Jpeg2000Progression.PositionComponentResolutionLayer, true)]
	[InlineData(Jpeg2000Progression.ComponentPositionResolutionLayer, false)]
	public void SupportsEfficientSpatialAccess_ShouldReturnCorrectValue(Jpeg2000Progression progression, bool expected)
	{
		// Act
		var result = progression.SupportsEfficientSpatialAccess();

		// Assert
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(Jpeg2000Progression.LayerResolutionComponentPosition, true)]
	[InlineData(Jpeg2000Progression.ResolutionLayerComponentPosition, true)]
	[InlineData(Jpeg2000Progression.ResolutionPositionComponentLayer, false)]
	[InlineData(Jpeg2000Progression.PositionComponentResolutionLayer, false)]
	[InlineData(Jpeg2000Progression.ComponentPositionResolutionLayer, false)]
	public void SupportsEfficientQualityScaling_ShouldReturnCorrectValue(Jpeg2000Progression progression, bool expected)
	{
		// Act
		var result = progression.SupportsEfficientQualityScaling();

		// Assert
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(Jpeg2000Progression.LayerResolutionComponentPosition, false)]
	[InlineData(Jpeg2000Progression.ResolutionLayerComponentPosition, true)]
	[InlineData(Jpeg2000Progression.ResolutionPositionComponentLayer, true)]
	[InlineData(Jpeg2000Progression.PositionComponentResolutionLayer, false)]
	[InlineData(Jpeg2000Progression.ComponentPositionResolutionLayer, false)]
	public void SupportsEfficientResolutionScaling_ShouldReturnCorrectValue(Jpeg2000Progression progression, bool expected)
	{
		// Act
		var result = progression.SupportsEfficientResolutionScaling();

		// Assert
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(Jpeg2000Progression.LayerResolutionComponentPosition, 0)]
	[InlineData(Jpeg2000Progression.ResolutionLayerComponentPosition, 1)]
	[InlineData(Jpeg2000Progression.ResolutionPositionComponentLayer, 2)]
	[InlineData(Jpeg2000Progression.PositionComponentResolutionLayer, 3)]
	[InlineData(Jpeg2000Progression.ComponentPositionResolutionLayer, 4)]
	public void ToByte_ShouldReturnCorrectByteValue(Jpeg2000Progression progression, byte expected)
	{
		// Act
		var result = progression.ToByte();

		// Assert
		Assert.Equal(expected, result);
	}

	[Fact]
	public void GetDescription_WithInvalidProgression_ShouldReturnUnknown()
	{
		// Arrange
		var invalidProgression = (Jpeg2000Progression)255;

		// Act
		var description = invalidProgression.GetDescription();

		// Assert
		Assert.Equal("Unknown progression order", description);
	}

	[Fact]
	public void GetRecommendedUseCase_WithInvalidProgression_ShouldReturnGeneralPurpose()
	{
		// Arrange
		var invalidProgression = (Jpeg2000Progression)255;

		// Act
		var useCase = invalidProgression.GetRecommendedUseCase();

		// Assert
		Assert.Equal("General purpose", useCase);
	}

	[Fact]
	public void AllProgressionValues_ShouldBeValid()
	{
		// Arrange
		var allProgressions = Enum.GetValues<Jpeg2000Progression>();

		// Act & Assert
		foreach (var progression in allProgressions)
		{
			// Verify each progression has a valid byte value
			var byteValue = progression.ToByte();
			Assert.InRange(byteValue, 0, 4);

			// Verify each progression has a description
			var description = progression.GetDescription();
			Assert.False(string.IsNullOrEmpty(description));
			Assert.NotEqual("Unknown progression order", description);

			// Verify each progression has a use case
			var useCase = progression.GetRecommendedUseCase();
			Assert.False(string.IsNullOrEmpty(useCase));
			Assert.NotEqual("General purpose", useCase);
		}
	}

	[Fact]
	public void ProgressionEnumeration_ShouldHaveCorrectCount()
	{
		// Arrange & Act
		var allProgressions = Enum.GetValues<Jpeg2000Progression>();

		// Assert
		Assert.Equal(5, allProgressions.Length);
	}

	[Fact]
	public void SpatialAccessSupport_ShouldBeConsistent()
	{
		// Arrange
		var spatialProgressions = new[]
		{
			Jpeg2000Progression.ResolutionPositionComponentLayer,
			Jpeg2000Progression.PositionComponentResolutionLayer
		};

		var nonSpatialProgressions = new[]
		{
			Jpeg2000Progression.LayerResolutionComponentPosition,
			Jpeg2000Progression.ResolutionLayerComponentPosition,
			Jpeg2000Progression.ComponentPositionResolutionLayer
		};

		// Act & Assert
		foreach (var progression in spatialProgressions)
		{
			Assert.True(progression.SupportsEfficientSpatialAccess(),
				$"{progression} should support efficient spatial access");
		}

		foreach (var progression in nonSpatialProgressions)
		{
			Assert.False(progression.SupportsEfficientSpatialAccess(),
				$"{progression} should not support efficient spatial access");
		}
	}

	[Fact]
	public void QualityScalingSupport_ShouldBeConsistent()
	{
		// Arrange
		var qualityProgressions = new[]
		{
			Jpeg2000Progression.LayerResolutionComponentPosition,
			Jpeg2000Progression.ResolutionLayerComponentPosition
		};

		var nonQualityProgressions = new[]
		{
			Jpeg2000Progression.ResolutionPositionComponentLayer,
			Jpeg2000Progression.PositionComponentResolutionLayer,
			Jpeg2000Progression.ComponentPositionResolutionLayer
		};

		// Act & Assert
		foreach (var progression in qualityProgressions)
		{
			Assert.True(progression.SupportsEfficientQualityScaling(),
				$"{progression} should support efficient quality scaling");
		}

		foreach (var progression in nonQualityProgressions)
		{
			Assert.False(progression.SupportsEfficientQualityScaling(),
				$"{progression} should not support efficient quality scaling");
		}
	}

	[Fact]
	public void ResolutionScalingSupport_ShouldBeConsistent()
	{
		// Arrange
		var resolutionProgressions = new[]
		{
			Jpeg2000Progression.ResolutionLayerComponentPosition,
			Jpeg2000Progression.ResolutionPositionComponentLayer
		};

		var nonResolutionProgressions = new[]
		{
			Jpeg2000Progression.LayerResolutionComponentPosition,
			Jpeg2000Progression.PositionComponentResolutionLayer,
			Jpeg2000Progression.ComponentPositionResolutionLayer
		};

		// Act & Assert
		foreach (var progression in resolutionProgressions)
		{
			Assert.True(progression.SupportsEfficientResolutionScaling(),
				$"{progression} should support efficient resolution scaling");
		}

		foreach (var progression in nonResolutionProgressions)
		{
			Assert.False(progression.SupportsEfficientResolutionScaling(),
				$"{progression} should not support efficient resolution scaling");
		}
	}
}
