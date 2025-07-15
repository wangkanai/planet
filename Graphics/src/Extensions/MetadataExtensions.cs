// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.

namespace Wangkanai.Graphics.Extensions;

/// <summary>
/// Extension methods for IMetadata interface providing common utility functions.
/// </summary>
public static class MetadataExtensions
{
	/// <summary>
	/// Determines if the metadata has valid dimensions (width and height > 0).
	/// </summary>
	/// <param name="metadata">The metadata to check.</param>
	/// <returns>True if both width and height are greater than 0.</returns>
	public static bool HasDimensions(this IMetadata metadata)
		=> metadata.Width > 0 && metadata.Height > 0;

	/// <summary>
	/// Determines if the metadata has a title.
	/// </summary>
	/// <param name="metadata">The metadata to check.</param>
	/// <returns>True if the title is not null or empty.</returns>
	public static bool HasTitle(this IMetadata metadata)
		=> !string.IsNullOrWhiteSpace(metadata.Title);

	/// <summary>
	/// Determines if the metadata has orientation information.
	/// </summary>
	/// <param name="metadata">The metadata to check.</param>
	/// <returns>True if orientation is set.</returns>
	public static bool HasOrientation(this IMetadata metadata)
		=> metadata.Orientation.HasValue;

	/// <summary>
	/// Sets the dimensions of the metadata.
	/// </summary>
	/// <param name="metadata">The metadata to modify.</param>
	/// <param name="width">The width in pixels.</param>
	/// <param name="height">The height in pixels.</param>
	public static void SetDimensions(this IMetadata metadata, int width, int height)
	{
		metadata.Width  = width;
		metadata.Height = height;
	}

	/// <summary>
	/// Gets the dimensions as a tuple.
	/// </summary>
	/// <param name="metadata">The metadata to read from.</param>
	/// <returns>A tuple containing width and height.</returns>
	public static (int width, int height) GetDimensions(this IMetadata metadata)
		=> (metadata.Width, metadata.Height);

	/// <summary>
	/// Calculates the aspect ratio of the image.
	/// </summary>
	/// <param name="metadata">The metadata to calculate from.</param>
	/// <returns>The aspect ratio (width/height), or 0 if height is 0.</returns>
	public static float GetAspectRatio(this IMetadata metadata)
		=> metadata.Height > 0 ? (float)metadata.Width / metadata.Height : 0f;

	/// <summary>
	/// Determines if the metadata has valid dimensions within specified bounds.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <param name="minDimension">Minimum allowed dimension.</param>
	/// <param name="maxDimension">Maximum allowed dimension.</param>
	/// <returns>True if dimensions are within bounds.</returns>
	public static bool IsValidDimensions(this IMetadata metadata, int minDimension = 1, int maxDimension = int.MaxValue)
		=> metadata.Width >= minDimension &&
		   metadata.Width <= maxDimension &&
		   metadata.Height >= minDimension &&
		   metadata.Height <= maxDimension;

	/// <summary>
	/// Determines if the orientation value is valid, according to EXIF specification.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>True if orientation is null or between 1-8.</returns>
	public static bool IsValidOrientation(this IMetadata metadata)
		=> !metadata.Orientation.HasValue ||
		   metadata.Orientation >= 1 &&
		   metadata.Orientation <= 8;

	/// <summary>
	/// Determines if the metadata requires async disposal based on size.
	/// </summary>
	/// <param name="metadata">The metadata to check.</param>
	/// <returns>True if async disposal is recommended.</returns>
	public static bool RequiresAsyncDisposal(this IMetadata metadata)
		=> metadata.HasLargeMetadata;

	/// <summary>
	/// Gets the estimated metadata size in kilobytes.
	/// </summary>
	/// <param name="metadata">The metadata to measure.</param>
	/// <returns>Size in KB.</returns>
	public static double GetEstimatedSizeInKB(this IMetadata metadata)
		=> metadata.EstimatedMetadataSize / 1024.0;

	/// <summary>
	/// Gets the estimated metadata size in megabytes.
	/// </summary>
	/// <param name="metadata">The metadata to measure.</param>
	/// <returns>Size in MB.</returns>
	public static double GetEstimatedSizeInMB(this IMetadata metadata)
		=> metadata.EstimatedMetadataSize / (1024.0 * 1024.0);

	/// <summary>
	/// Determines if the metadata is considered large (over 1MB).
	/// </summary>
	/// <param name="metadata">The metadata to check.</param>
	/// <returns>True if metadata is large.</returns>
	public static bool IsConsideredLarge(this IMetadata metadata)
		=> metadata.GetEstimatedSizeInMB() > 1.0;

	/// <summary>
	/// Performs basic validation of common metadata properties.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>True if basic validation passes.</returns>
	public static bool HasValidBasicProperties(this IMetadata metadata)
		=> metadata.IsValidDimensions() && metadata.IsValidOrientation();

	/// <summary>
	/// Gets the total pixel count of the image.
	/// </summary>
	/// <param name="metadata">The metadata to calculate from.</param>
	/// <returns>Total number of pixels.</returns>
	public static long GetPixelCount(this IMetadata metadata)
		=> (long)metadata.Width * metadata.Height;

	/// <summary>
	/// Determines if the image is in landscape orientation.
	/// </summary>
	/// <param name="metadata">The metadata to check.</param>
	/// <returns>True if width is greater than height.</returns>
	public static bool IsLandscape(this IMetadata metadata)
		=> metadata.Width > metadata.Height;

	/// <summary>
	/// Determines if the image is in portrait orientation.
	/// </summary>
	/// <param name="metadata">The metadata to check.</param>
	/// <returns>True if height is greater than width.</returns>
	public static bool IsPortrait(this IMetadata metadata)
		=> metadata.Height > metadata.Width;

	/// <summary>
	/// Determines if the image is square.
	/// </summary>
	/// <param name="metadata">The metadata to check.</param>
	/// <returns>True if width equals height.</returns>
	public static bool IsSquare(this IMetadata metadata)
		=> metadata.Width == metadata.Height;

	/// <summary>
	/// Creates a copy of the metadata with specified dimensions.
	/// </summary>
	/// <param name="metadata">The source metadata.</param>
	/// <param name="width">New width.</param>
	/// <param name="height">New height.</param>
	/// <returns>A cloned metadata with new dimensions.</returns>
	public static IMetadata WithDimensions(this IMetadata metadata, int width, int height)
	{
		var clone = metadata.Clone();
		clone.SetDimensions(width, height);
		return clone;
	}

	/// <summary>
	/// Creates a copy of the metadata with a specified title.
	/// </summary>
	/// <param name="metadata">The source metadata.</param>
	/// <param name="title">New title.</param>
	/// <returns>A cloned metadata with new title.</returns>
	public static IMetadata WithTitle(this IMetadata metadata, string? title)
	{
		var clone = metadata.Clone();
		clone.Title = title;
		return clone;
	}
}
