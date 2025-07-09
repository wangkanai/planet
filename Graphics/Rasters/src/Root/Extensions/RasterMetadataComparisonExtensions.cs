// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Extensions;

/// <summary>
/// Extension methods for comparing raster metadata instances.
/// </summary>
public static class RasterMetadataComparisonExtensions
{
	/// <summary>
	/// Determines if two raster metadata instances have similar resolution within a tolerance.
	/// </summary>
	/// <param name="metadata">The first raster metadata instance.</param>
	/// <param name="other">The raster metadata instance to compare with.</param>
	/// <param name="tolerance">Tolerance as a percentage (default: 5% = 0.05).</param>
	/// <returns>True if resolutions are similar within tolerance.</returns>
	public static bool HasSimilarResolution(this IRasterMetadata metadata, IRasterMetadata other, double tolerance = 0.05)
	{
		if (metadata == other) return true;
		
		// If one has resolution and the other doesn't, they're not similar
		if (metadata.HasResolution() != other.HasResolution())
			return false;
		
		// If neither has resolution, consider them similar
		if (!metadata.HasResolution())
			return true;
		
		var res1 = metadata.GetResolution()!.Value;
		var res2 = other.GetResolution()!.Value;
		
		var xDiff = Math.Abs(res1.x - res2.x) / Math.Max(res1.x, res2.x);
		var yDiff = Math.Abs(res1.y - res2.y) / Math.Max(res1.y, res2.y);
		
		return xDiff <= tolerance && yDiff <= tolerance;
	}

	/// <summary>
	/// Performs a comprehensive comparison between two raster metadata instances.
	/// </summary>
	/// <param name="metadata">The first raster metadata instance.</param>
	/// <param name="other">The raster metadata instance to compare with.</param>
	/// <returns>Detailed raster comparison result.</returns>
	public static RasterComparisonResult CompareRaster(this IRasterMetadata metadata, IRasterMetadata other)
	{
		return new RasterComparisonResult
		{
			BitDepthMatch = metadata.BitDepth == other.BitDepth,
			ResolutionMatch = metadata.HasSimilarResolution(other),
			ColorSpaceMatch = metadata.ColorSpace == other.ColorSpace,
			HasExifData = (metadata.HasExifData(), other.HasExifData()),
			HasGpsData = (metadata.HasGpsCoordinates(), other.HasGpsCoordinates()),
			HasIccProfile = (metadata.HasIccProfile(), other.HasIccProfile())
		};
	}

	/// <summary>
	/// Determines if two raster metadata instances are functionally equivalent for printing.
	/// </summary>
	/// <param name="metadata">The first raster metadata instance.</param>
	/// <param name="other">The raster metadata instance to compare with.</param>
	/// <returns>True if functionally equivalent for printing.</returns>
	public static bool IsPrintEquivalent(this IRasterMetadata metadata, IRasterMetadata other)
	{
		return metadata.Width == other.Width &&
		       metadata.Height == other.Height &&
		       metadata.HasSimilarResolution(other, 0.01) && // 1% tolerance for print
		       metadata.BitDepth == other.BitDepth;
	}

	/// <summary>
	/// Determines if two raster metadata instances are functionally equivalent for processing.
	/// </summary>
	/// <param name="metadata">The first raster metadata instance.</param>
	/// <param name="other">The raster metadata instance to compare with.</param>
	/// <returns>True if functionally equivalent for processing.</returns>
	public static bool IsProcessingEquivalent(this IRasterMetadata metadata, IRasterMetadata other)
	{
		return metadata.Width == other.Width &&
		       metadata.Height == other.Height &&
		       metadata.BitDepth == other.BitDepth &&
		       metadata.ColorSpace == other.ColorSpace;
	}

	/// <summary>
	/// Calculates the similarity score between two raster metadata instances.
	/// </summary>
	/// <param name="metadata">The first raster metadata instance.</param>
	/// <param name="other">The raster metadata instance to compare with.</param>
	/// <returns>Similarity score from 0.0 to 1.0.</returns>
	public static double CalculateRasterSimilarity(this IRasterMetadata metadata, IRasterMetadata other)
	{
		var scores = new List<double>();

		// Dimension similarity
		var pixelCount1 = (long)metadata.Width * metadata.Height;
		var pixelCount2 = (long)other.Width * other.Height;
		if (pixelCount1 > 0 && pixelCount2 > 0)
		{
			scores.Add((double)Math.Min(pixelCount1, pixelCount2) / Math.Max(pixelCount1, pixelCount2));
		}

		// Bit depth similarity
		scores.Add(metadata.BitDepth == other.BitDepth ? 1.0 : 0.0);

		// Resolution similarity
		if (metadata.HasResolution() && other.HasResolution())
		{
			scores.Add(metadata.HasSimilarResolution(other) ? 1.0 : 0.5);
		}
		else if (metadata.HasResolution() == other.HasResolution())
		{
			scores.Add(1.0); // Both have or don't have resolution
		}
		else
		{
			scores.Add(0.5); // One has resolution, other doesn't
		}

		// Color space similarity
		scores.Add(metadata.ColorSpace == other.ColorSpace ? 1.0 : 0.0);

		// GPS data similarity
		var gpsScore = (metadata.HasGpsCoordinates(), other.HasGpsCoordinates()) switch
		{
			(true, true) => 1.0,
			(false, false) => 1.0,
			_ => 0.5
		};
		scores.Add(gpsScore);

		// Metadata presence similarity
		var metadataScore = 0.0;
		if (metadata.HasExifData() == other.HasExifData()) metadataScore += 0.33;
		if (metadata.HasXmpData() == other.HasXmpData()) metadataScore += 0.33;
		if (metadata.HasIccProfile() == other.HasIccProfile()) metadataScore += 0.34;
		scores.Add(metadataScore);

		return scores.Average();
	}

	/// <summary>
	/// Finds the most similar raster metadata from a collection.
	/// </summary>
	/// <param name="metadata">The raster metadata to compare against.</param>
	/// <param name="candidates">Collection of candidate raster metadata instances.</param>
	/// <returns>The most similar raster metadata and its similarity score, or null if none found.</returns>
	public static (IRasterMetadata metadata, double similarity)? FindMostSimilarRaster(
		this IRasterMetadata metadata, IEnumerable<IRasterMetadata> candidates)
	{
		var bestMatch = candidates
			.Where(candidate => candidate != metadata) // Exclude self
			.Select(candidate => new { Metadata = candidate, Similarity = metadata.CalculateRasterSimilarity(candidate) })
			.OrderByDescending(x => x.Similarity)
			.FirstOrDefault();
		
		return bestMatch != null ? (bestMatch.Metadata, bestMatch.Similarity) : null;
	}

	/// <summary>
	/// Groups a collection of raster metadata by similarity.
	/// </summary>
	/// <param name="rasterCollection">Collection of raster metadata to group.</param>
	/// <param name="similarityThreshold">Minimum similarity to group together (default: 0.8).</param>
	/// <returns>Groups of similar raster metadata.</returns>
	public static IEnumerable<IGrouping<int, IRasterMetadata>> GroupRastersBySimilarity(
		this IEnumerable<IRasterMetadata> rasterCollection, double similarityThreshold = 0.8)
	{
		var rasterList = rasterCollection.ToList();
		var groups = new Dictionary<int, List<IRasterMetadata>>();
		var groupId = 0;
		
		foreach (var raster in rasterList)
		{
			var assignedToGroup = false;
			
			// Check if it belongs to an existing group
			foreach (var (id, group) in groups)
			{
				var representative = group.First();
				var similarity = raster.CalculateRasterSimilarity(representative);
				
				if (similarity >= similarityThreshold)
				{
					group.Add(raster);
					assignedToGroup = true;
					break;
				}
			}
			
			// Create new group if not assigned
			if (!assignedToGroup)
			{
				groups[groupId++] = new List<IRasterMetadata> { raster };
			}
		}
		
		return groups.Select(kvp => new RasterGrouping(kvp.Key, kvp.Value));
	}

	private class RasterGrouping : IGrouping<int, IRasterMetadata>
	{
		public int Key { get; }
		private readonly List<IRasterMetadata> _rasters;

		public RasterGrouping(int key, List<IRasterMetadata> rasters)
		{
			Key = key;
			_rasters = rasters;
		}

		public IEnumerator<IRasterMetadata> GetEnumerator() => _rasters.GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

/// <summary>
/// Raster-specific comparison results.
/// </summary>
public class RasterComparisonResult
{
	public bool BitDepthMatch { get; set; }
	public bool ResolutionMatch { get; set; }
	public bool ColorSpaceMatch { get; set; }
	public (bool first, bool second) HasExifData { get; set; }
	public (bool first, bool second) HasGpsData { get; set; }
	public (bool first, bool second) HasIccProfile { get; set; }
}