// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.

namespace Wangkanai.Graphics.Extensions;

public static class MetadataComparisionExtensions
{
	/// <summary>Determines if two metadata instances are functionally equivalent for a specific use case.</summary>
	/// <param name="metadata">The first metadata instance.</param>
	/// <param name="other">The metadata instance to compare with.</param>
	/// <param name="useCase">The use case to evaluate for.</param>
	/// <returns>True if functionally equivalent for the use case.</returns>
	public static bool IsFunctionallyEquivalent(this IMetadata metadata, IMetadata other, ImageUseCase useCase)
		=> useCase switch
		{
			ImageUseCase.WebDisplay => IsWebEquivalent(metadata, other),
			ImageUseCase.Printing   => IsPrintEquivalent(metadata, other),
			ImageUseCase.Archival   => IsArchivalEquivalent(metadata, other),
			ImageUseCase.Processing => IsProcessingEquivalent(metadata, other),
			_                  => metadata.Compare(other).OverallSimilarity > 0.9
		};

	private static bool IsWebEquivalent(IMetadata metadata1, IMetadata metadata2)
	{
		// For web use, dimensions and aspect ratio are most important
		return metadata1.HasSimilarDimensions(metadata2, 0.05) && // 5% tolerance
		       Math.Abs(metadata1.GetAspectRatio() - metadata2.GetAspectRatio()) < 0.1;
	}

	private static bool IsPrintEquivalent(IMetadata metadata1, IMetadata metadata2)
	{
		// For printing, exact dimensions matter - format-specific extensions can add resolution checks
		return metadata1.Width == metadata2.Width && metadata1.Height == metadata2.Height;
	}

	private static bool IsArchivalEquivalent(IMetadata metadata1, IMetadata metadata2)
	{
		// For archival, type and basic properties should match
		return metadata1.GetType() == metadata2.GetType() &&
		       metadata1.Width == metadata2.Width &&
		       metadata1.Height == metadata2.Height;
	}

	private static bool IsProcessingEquivalent(IMetadata metadata1, IMetadata metadata2)
	{
		// For processing, dimension matter - format-specific extensions can add bit depth checks
		return metadata1.Width == metadata2.Width && metadata1.Height == metadata2.Height;
	}
}
