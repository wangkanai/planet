// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors.Extensions;

/// <summary>
/// Extension methods for comparing vector metadata instances.
/// </summary>
public static class VectorMetadataComparisonExtensions
{
	/// <summary>
	/// Performs a comprehensive comparison between two vector metadata instances.
	/// </summary>
	/// <param name="metadata">The first vector metadata instance.</param>
	/// <param name="other">The vector metadata instance to compare with.</param>
	/// <returns>Detailed vector comparison result.</returns>
	public static VectorComparisonResult CompareVector(this IVectorMetadata metadata, IVectorMetadata other)
	{
		return new VectorComparisonResult
		{
			ElementCountSimilarity = CalculateElementCountSimilarity(metadata, other),
			CoordinateSystemMatch = string.Equals(metadata.CoordinateReferenceSystem, 
				other.CoordinateReferenceSystem, StringComparison.OrdinalIgnoreCase),
			ColorSpaceMatch = string.Equals(metadata.ColorSpace, 
				other.ColorSpace, StringComparison.OrdinalIgnoreCase),
			ComplexityLevelMatch = metadata.GetComplexityLevel() == other.GetComplexityLevel()
		};
	}

	/// <summary>
	/// Calculates the similarity score between two vector metadata instances.
	/// </summary>
	/// <param name="metadata">The first vector metadata instance.</param>
	/// <param name="other">The vector metadata instance to compare with.</param>
	/// <returns>Similarity score from 0.0 to 1.0.</returns>
	public static double CalculateVectorSimilarity(this IVectorMetadata metadata, IVectorMetadata other)
	{
		var scores = new List<double>();

		// Dimension similarity
		var pixelCount1 = (long)metadata.Width * metadata.Height;
		var pixelCount2 = (long)other.Width * other.Height;
		if (pixelCount1 > 0 && pixelCount2 > 0)
		{
			scores.Add((double)Math.Min(pixelCount1, pixelCount2) / Math.Max(pixelCount1, pixelCount2));
		}

		// Element count similarity
		scores.Add(CalculateElementCountSimilarity(metadata, other));

		// Coordinate system similarity
		var crsMatch = string.Equals(metadata.CoordinateReferenceSystem, 
			other.CoordinateReferenceSystem, StringComparison.OrdinalIgnoreCase);
		scores.Add(crsMatch ? 1.0 : 0.0);

		// Color space similarity
		var colorSpaceMatch = string.Equals(metadata.ColorSpace, 
			other.ColorSpace, StringComparison.OrdinalIgnoreCase);
		scores.Add(colorSpaceMatch ? 1.0 : 0.0);

		// Complexity level similarity
		var complexityMatch = metadata.GetComplexityLevel() == other.GetComplexityLevel();
		scores.Add(complexityMatch ? 1.0 : 0.5); // Partial credit if complexity levels differ

		return scores.Average();
	}

	/// <summary>
	/// Determines if two vector metadata instances are functionally equivalent for a specific use case.
	/// </summary>
	/// <param name="metadata">The first vector metadata instance.</param>
	/// <param name="other">The vector metadata instance to compare with.</param>
	/// <param name="useCase">The use case to evaluate for.</param>
	/// <returns>True if functionally equivalent for the use case.</returns>
	public static bool IsVectorEquivalent(this IVectorMetadata metadata, IVectorMetadata other, VectorUseCase useCase)
	{
		return useCase switch
		{
			VectorUseCase.WebDisplay => IsWebEquivalent(metadata, other),
			VectorUseCase.Printing => IsPrintEquivalent(metadata, other),
			VectorUseCase.Animation => IsAnimationEquivalent(metadata, other),
			VectorUseCase.Mapping => IsMappingEquivalent(metadata, other),
			_ => metadata.CalculateVectorSimilarity(other) > 0.9
		};
	}

	/// <summary>
	/// Finds the most similar vector metadata from a collection.
	/// </summary>
	/// <param name="metadata">The vector metadata to compare against.</param>
	/// <param name="candidates">Collection of candidate vector metadata instances.</param>
	/// <returns>The most similar vector metadata and its similarity score, or null if none found.</returns>
	public static (IVectorMetadata metadata, double similarity)? FindMostSimilarVector(
		this IVectorMetadata metadata, IEnumerable<IVectorMetadata> candidates)
	{
		var bestMatch = candidates
			.Where(candidate => candidate != metadata) // Exclude self
			.Select(candidate => new { Metadata = candidate, Similarity = metadata.CalculateVectorSimilarity(candidate) })
			.OrderByDescending(x => x.Similarity)
			.FirstOrDefault();
		
		return bestMatch != null ? (bestMatch.Metadata, bestMatch.Similarity) : null;
	}

	/// <summary>
	/// Groups a collection of vector metadata by similarity.
	/// </summary>
	/// <param name="vectorCollection">Collection of vector metadata to group.</param>
	/// <param name="similarityThreshold">Minimum similarity to group together (default: 0.8).</param>
	/// <returns>Groups of similar vector metadata.</returns>
	public static IEnumerable<IGrouping<int, IVectorMetadata>> GroupVectorsBySimilarity(
		this IEnumerable<IVectorMetadata> vectorCollection, double similarityThreshold = 0.8)
	{
		var vectorList = vectorCollection.ToList();
		var groups = new Dictionary<int, List<IVectorMetadata>>();
		var groupId = 0;
		
		foreach (var vector in vectorList)
		{
			var assignedToGroup = false;
			
			// Check if it belongs to an existing group
			foreach (var (id, group) in groups)
			{
				var representative = group.First();
				var similarity = vector.CalculateVectorSimilarity(representative);
				
				if (similarity >= similarityThreshold)
				{
					group.Add(vector);
					assignedToGroup = true;
					break;
				}
			}
			
			// Create new group if not assigned
			if (!assignedToGroup)
			{
				groups[groupId++] = new List<IVectorMetadata> { vector };
			}
		}
		
		return groups.Select(kvp => new VectorGrouping(kvp.Key, kvp.Value));
	}

	/// <summary>
	/// Determines if two vector metadata instances have similar complexity.
	/// </summary>
	/// <param name="metadata">The first vector metadata instance.</param>
	/// <param name="other">The vector metadata instance to compare with.</param>
	/// <param name="tolerance">Element count tolerance as a percentage (default: 10% = 0.1).</param>
	/// <returns>True if complexity is similar within tolerance.</returns>
	public static bool HasSimilarComplexity(this IVectorMetadata metadata, IVectorMetadata other, double tolerance = 0.1)
	{
		var elementSimilarity = CalculateElementCountSimilarity(metadata, other);
		return elementSimilarity >= (1.0 - tolerance);
	}

	private static double CalculateElementCountSimilarity(IVectorMetadata vector1, IVectorMetadata vector2)
	{
		var count1 = vector1.ElementCount;
		var count2 = vector2.ElementCount;
		
		if (count1 == 0 && count2 == 0) return 1.0;
		if (count1 == 0 || count2 == 0) return 0.0;
		
		return (double)Math.Min(count1, count2) / Math.Max(count1, count2);
	}

	private static bool IsWebEquivalent(IVectorMetadata metadata1, IVectorMetadata metadata2)
	{
		// For web use, dimensions and reasonable complexity matter
		return metadata1.Width == metadata2.Width &&
		       metadata1.Height == metadata2.Height &&
		       metadata1.HasSimilarComplexity(metadata2, 0.2); // 20% tolerance for web
	}

	private static bool IsPrintEquivalent(IVectorMetadata metadata1, IVectorMetadata metadata2)
	{
		// For printing, exact dimensions and coordinate system matter
		return metadata1.Width == metadata2.Width &&
		       metadata1.Height == metadata2.Height &&
		       string.Equals(metadata1.CoordinateReferenceSystem, 
		                    metadata2.CoordinateReferenceSystem, StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsAnimationEquivalent(IVectorMetadata metadata1, IVectorMetadata metadata2)
	{
		// For animation, complexity and performance characteristics matter
		return metadata1.Width == metadata2.Width &&
		       metadata1.Height == metadata2.Height &&
		       metadata1.GetComplexityLevel() == metadata2.GetComplexityLevel() &&
		       metadata1.IsSuitableForRealTimeRendering() == metadata2.IsSuitableForRealTimeRendering();
	}

	private static bool IsMappingEquivalent(IVectorMetadata metadata1, IVectorMetadata metadata2)
	{
		// For mapping, coordinate system is critical
		return string.Equals(metadata1.CoordinateReferenceSystem, 
		                    metadata2.CoordinateReferenceSystem, StringComparison.OrdinalIgnoreCase) &&
		       metadata1.HasSimilarComplexity(metadata2, 0.1); // 10% tolerance for mapping
	}

	private class VectorGrouping : IGrouping<int, IVectorMetadata>
	{
		public int Key { get; }
		private readonly List<IVectorMetadata> _vectors;

		public VectorGrouping(int key, List<IVectorMetadata> vectors)
		{
			Key = key;
			_vectors = vectors;
		}

		public IEnumerator<IVectorMetadata> GetEnumerator() => _vectors.GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

/// <summary>
/// Vector-specific comparison results.
/// </summary>
public class VectorComparisonResult
{
	public double ElementCountSimilarity { get; set; }
	public bool CoordinateSystemMatch { get; set; }
	public bool ColorSpaceMatch { get; set; }
	public bool ComplexityLevelMatch { get; set; }
}

/// <summary>
/// Use cases for vector functional equivalence testing.
/// </summary>
public enum VectorUseCase
{
	WebDisplay,
	Printing,
	Animation,
	Mapping
}