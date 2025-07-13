// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0


namespace Wangkanai.Graphics.Extensions;

/// <summary>
/// Extension methods for comparing metadata between different instances.
/// </summary>
public static class MetadataComparisonExtensions
{
	/// <summary>
	/// Determines if two metadata instances have similar dimensions within a tolerance.
	/// </summary>
	/// <param name="metadata">The first metadata instance.</param>
	/// <param name="other">The metadata instance to compare with.</param>
	/// <param name="tolerance">Tolerance as a percentage (default: 1% = 0.01).</param>
	/// <returns>True if dimensions are similar within tolerance.</returns>
	public static bool HasSimilarDimensions(this IMetadata metadata, IMetadata other, double tolerance = 0.01)
	{
		if (metadata == other) return true;

		var widthDiff  = Math.Abs(metadata.Width - other.Width) / (double)Math.Max(metadata.Width, other.Width);
		var heightDiff = Math.Abs(metadata.Height - other.Height) / (double)Math.Max(metadata.Height, other.Height);

		return widthDiff <= tolerance && heightDiff <= tolerance;
	}


	/// <summary>
	/// Performs a comprehensive comparison between two metadata instances.
	/// </summary>
	/// <param name="metadata">The first metadata instance.</param>
	/// <param name="other">The metadata instance to compare with.</param>
	/// <returns>Detailed comparison result.</returns>
	public static MetadataComparisonResult Compare(this IMetadata metadata, IMetadata other)
	{
		var result = new MetadataComparisonResult();

		// Basic property comparisons
		result.DimensionsMatch  = metadata.Width == other.Width && metadata.Height == other.Height;
		result.TitleMatch       = string.Equals(metadata.Title, other.Title, StringComparison.Ordinal);
		result.OrientationMatch = metadata.Orientation == other.Orientation;
		result.TypeMatch        = metadata.GetType() == other.GetType();

		// Calculate similarity scores
		result.DimensionSimilarity   = CalculateDimensionSimilarity(metadata, other);
		result.AspectRatioSimilarity = CalculateAspectRatioSimilarity(metadata, other);
		result.SizeSimilarity        = CalculateSizeSimilarity(metadata, other);

		// Type-specific comparisons would be handled by format-specific extension methods

		// Calculate overall similarity
		result.OverallSimilarity = CalculateOverallSimilarity(result);

		return result;
	}

	/// <summary>
	/// Finds the most similar metadata from a collection.
	/// </summary>
	/// <param name="metadata">The metadata to compare against.</param>
	/// <param name="candidates">Collection of candidate metadata instances.</param>
	/// <returns>The most similar metadata and its comparison result, or null if none found.</returns>
	public static (IMetadata metadata, MetadataComparisonResult comparison)? FindMostSimilar(
		this IMetadata metadata, IEnumerable<IMetadata> candidates)
	{
		var bestMatch = candidates
		                .Select(candidate => new { Metadata = candidate, Comparison = metadata.Compare(candidate) })
		                .Where(x => x.Metadata != metadata) // Exclude self
		                .OrderByDescending(x => x.Comparison.OverallSimilarity)
		                .FirstOrDefault();

		return bestMatch != null ? (bestMatch.Metadata, bestMatch.Comparison) : null;
	}

	/// <summary>
	/// Groups a collection of metadata by similarity.
	/// </summary>
	/// <param name="metadataCollection">Collection of metadata to group.</param>
	/// <param name="similarityThreshold">Minimum similarity to group together (default: 0.8).</param>
	/// <returns>Groups of similar metadata.</returns>
	public static IEnumerable<IGrouping<int, IMetadata>> GroupBySimilarity(
		this IEnumerable<IMetadata> metadataCollection, double similarityThreshold = 0.8)
	{
		var metadataList = metadataCollection.ToList();
		var groups       = new Dictionary<int, List<IMetadata>>();
		var groupId      = 0;

		foreach (var metadata in metadataList)
		{
			var assignedToGroup = false;

			// Check if it belongs to an existing group
			foreach (var (id, group) in groups)
			{
				var representative = group.First();
				var similarity     = metadata.Compare(representative).OverallSimilarity;

				if (similarity >= similarityThreshold)
				{
					group.Add(metadata);
					assignedToGroup = true;
					break;
				}
			}

			// Create a new group if not assigned
			if (!assignedToGroup)
				groups[groupId++] = new List<IMetadata> { metadata };
		}

		return groups.Select(kvp => new MetadataGrouping(kvp.Key, kvp.Value));
	}

	/// <summary>
	/// Calculates the difference in file size requirements between two metadata instances.
	/// </summary>
	/// <param name="metadata">The first metadata instance.</param>
	/// <param name="other">The metadata instance to compare with.</param>
	/// <returns>Size difference information.</returns>
	public static SizeDifference CalculateSizeDifference(this IMetadata metadata, IMetadata other)
	{
		var size1                = metadata.EstimatedMetadataSize;
		var size2                = other.EstimatedMetadataSize;
		var difference           = size2 - size1;
		var percentageDifference = size1 > 0 ? (difference / (double)size1) * 100 : 0;

		return new SizeDifference
		       {
			       AbsoluteDifference   = difference,
			       PercentageDifference = percentageDifference,
			       IsLarger             = difference > 0,
			       IsSignificant        = Math.Abs(percentageDifference) > 10 // More than 10% difference
		       };
	}

	/// <summary>
	/// Determines if two metadata instances are functionally equivalent for a specific use case.
	/// </summary>
	/// <param name="metadata">The first metadata instance.</param>
	/// <param name="other">The metadata instance to compare with.</param>
	/// <param name="useCase">The use case to evaluate for.</param>
	/// <returns>True if functionally equivalent for the use case.</returns>
	public static bool IsFunctionallyEquivalent(this IMetadata metadata, IMetadata other, UseCase useCase)
		=> useCase switch
		{
			UseCase.WebDisplay => IsWebEquivalent(metadata, other),
			UseCase.Printing   => IsPrintEquivalent(metadata, other),
			UseCase.Archival   => IsArchivalEquivalent(metadata, other),
			UseCase.Processing => IsProcessingEquivalent(metadata, other),
			_                  => metadata.Compare(other).OverallSimilarity > 0.9
		};

	private static double CalculateDimensionSimilarity(IMetadata metadata1, IMetadata metadata2)
	{
		var pixelCount1 = metadata1.GetPixelCount();
		var pixelCount2 = metadata2.GetPixelCount();

		if (pixelCount1 == 0 && pixelCount2 == 0) return 1.0;
		if (pixelCount1 == 0 || pixelCount2 == 0) return 0.0;

		var ratio = (double)Math.Min(pixelCount1, pixelCount2) / Math.Max(pixelCount1, pixelCount2);
		return ratio;
	}

	private static double CalculateAspectRatioSimilarity(IMetadata metadata1, IMetadata metadata2)
	{
		var ratio1 = metadata1.GetAspectRatio();
		var ratio2 = metadata2.GetAspectRatio();

		if (ratio1 == 0 && ratio2 == 0) return 1.0;
		if (ratio1 == 0 || ratio2 == 0) return 0.0;

		var similarity = Math.Min(ratio1, ratio2) / Math.Max(ratio1, ratio2);
		return similarity;
	}

	private static double CalculateSizeSimilarity(IMetadata metadata1, IMetadata metadata2)
	{
		var size1 = metadata1.EstimatedMetadataSize;
		var size2 = metadata2.EstimatedMetadataSize;

		if (size1 == 0 && size2 == 0) return 1.0;
		if (size1 == 0 || size2 == 0) return 0.0;

		var ratio = (double)Math.Min(size1, size2) / Math.Max(size1, size2);
		return ratio;
	}


	private static double CalculateOverallSimilarity(MetadataComparisonResult result)
	{
		var scores = new List<double>
		             {
			             result.DimensionSimilarity,
			             result.AspectRatioSimilarity,
			             result.SizeSimilarity
		             };

		// Type-specific scores would be added by format-specific extension methods

		return scores.Average();
	}

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
		// For processing, dimensions matter - format-specific extensions can add bit depth checks
		return metadata1.Width == metadata2.Width && metadata1.Height == metadata2.Height;
	}

	private class MetadataGrouping : IGrouping<int, IMetadata>
	{
		public           int             Key { get; }
		private readonly List<IMetadata> _metadata;

		public MetadataGrouping(int key, List<IMetadata> metadata)
		{
			Key       = key;
			_metadata = metadata;
		}

		public IEnumerator<IMetadata>                                 GetEnumerator() => _metadata.GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

/// <summary>
/// Result of comparing two metadata instances.
/// </summary>
public class MetadataComparisonResult
{
	public bool DimensionsMatch  { get; set; }
	public bool TitleMatch       { get; set; }
	public bool OrientationMatch { get; set; }
	public bool TypeMatch        { get; set; }

	public double DimensionSimilarity   { get; set; }
	public double AspectRatioSimilarity { get; set; }
	public double SizeSimilarity        { get; set; }
	public double OverallSimilarity     { get; set; }

	public RasterComparisonResult? RasterComparison { get; set; }
	public VectorComparisonResult? VectorComparison { get; set; }
}

/// <summary>
/// Raster-specific comparison results.
/// </summary>
public class RasterComparisonResult
{
	public bool                      BitDepthMatch   { get; set; }
	public bool                      ResolutionMatch { get; set; }
	public bool                      ColorSpaceMatch { get; set; }
	public (bool first, bool second) HasExifData     { get; set; }
	public (bool first, bool second) HasGpsData      { get; set; }
	public (bool first, bool second) HasIccProfile   { get; set; }
}

/// <summary>
/// Vector-specific comparison results.
/// </summary>
public class VectorComparisonResult
{
	public double ElementCountSimilarity { get; set; }
	public bool   CoordinateSystemMatch  { get; set; }
	public bool   ColorSpaceMatch        { get; set; }
	public bool   ComplexityLevelMatch   { get; set; }
}

/// <summary>
/// Size difference information between two metadata instances.
/// </summary>
public record SizeDifference
{
	public long   AbsoluteDifference   { get; init; }
	public double PercentageDifference { get; init; }
	public bool   IsLarger             { get; init; }
	public bool   IsSignificant        { get; init; }
}

/// <summary>
/// Use cases for functional equivalence testing.
/// </summary>
public enum UseCase
{
	WebDisplay,
	Printing,
	Archival,
	Processing
}
