// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics;
using Wangkanai.Graphics.Vectors;

namespace Wangkanai.Graphics.UnitTests.Vectors;

/// <summary>
/// Tests for Vector class async disposal implementation (GitHub Issue #80).
/// </summary>
public class VectorAsyncDisposalTests
{
	[Fact]
	public void Vector_ImplementsIImageWithAsyncDisposable()
	{
		// Arrange & Act
		var vector = new Vector();
		
		// Assert
		Assert.IsAssignableFrom<IImage>(vector);
		Assert.IsAssignableFrom<IAsyncDisposable>(vector);
		Assert.IsAssignableFrom<IDisposable>(vector);
	}

	[Fact]
	public void Vector_HasCorrectDefaultMetadataProperties()
	{
		// Arrange
		var vector = new Vector();
		
		// Act & Assert
		Assert.False(vector.HasLargeMetadata);
		Assert.Equal(0, vector.EstimatedMetadataSize);
	}

	[Fact]
	public async Task Vector_AsyncDisposal_ShouldCompleteSuccessfully()
	{
		// Arrange
		var vector = new Vector { Width = 1920, Height = 1080 };
		
		// Act & Assert - Should complete without throwing
		await vector.DisposeAsync();
	}

	[Fact]
	public async Task Vector_MultipleAsyncDisposals_ShouldNotThrow()
	{
		// Arrange
		var vector = new Vector();
		
		// Act & Assert - Multiple calls should be safe
		await vector.DisposeAsync();
		await vector.DisposeAsync();
		await vector.DisposeAsync();
	}

	[Fact]
	public void Vector_SynchronousDisposal_ShouldWork()
	{
		// Arrange
		var vector = new Vector();
		
		// Act & Assert - Should not throw
		vector.Dispose();
		vector.Dispose(); // Second call should be safe
	}

	[Fact]
	public async Task Vector_MixedDisposalPattern_ShouldWork()
	{
		// Arrange
		var vector1 = new Vector();
		var vector2 = new Vector();
		
		// Act & Assert - Mixed disposal patterns should work
		vector1.Dispose(); // Sync first
		await vector2.DisposeAsync(); // Async second
	}

	[Fact]
	public void Vector_AsIImage_HasCorrectMetadataThreshold()
	{
		// Arrange
		IImage vector = new Vector();
		
		// Act & Assert
		Assert.False(vector.HasLargeMetadata);
		Assert.True(vector.EstimatedMetadataSize < 1_000_000); // Under 1MB threshold
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(1920, 1080)]
	[InlineData(4096, 4096)]
	public async Task Vector_WithDifferentDimensions_AsyncDisposalWorks(int width, int height)
	{
		// Arrange
		var vector = new Vector { Width = width, Height = height };
		
		// Act & Assert
		Assert.Equal(width, vector.Width);
		Assert.Equal(height, vector.Height);
		
		// Should dispose successfully regardless of dimensions
		await vector.DisposeAsync();
	}
}