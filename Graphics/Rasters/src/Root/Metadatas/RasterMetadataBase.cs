// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Base implementation of raster metadata with common properties and functionality.
/// </summary>
public abstract class RasterMetadataBase : IRasterMetadata
{
	private bool _disposed;

	/// <inheritdoc />
	public virtual int Width { get; set; }

	/// <inheritdoc />
	public virtual int Height { get; set; }

	/// <inheritdoc />
	public virtual int BitDepth { get; set; } = 8;

	/// <inheritdoc />
	public virtual byte[]? ExifData { get; set; }

	/// <inheritdoc />
	public virtual string? XmpData { get; set; }

	/// <inheritdoc />
	public virtual byte[]? IccProfile { get; set; }

	/// <inheritdoc />
	public virtual DateTime? CreationTime { get; set; }

	/// <inheritdoc />
	public virtual DateTime? ModificationTime { get; set; }

	/// <inheritdoc />
	public virtual string? Software { get; set; }

	/// <inheritdoc />
	public virtual string? Description { get; set; }

	/// <inheritdoc />
	public virtual string? Copyright { get; set; }

	/// <inheritdoc />
	public virtual string? Author { get; set; }

	/// <inheritdoc />
	public virtual long EstimatedMemoryUsage
	{
		get
		{
			long size = GetBaseMemorySize();

			if (ExifData != null)
				size += ExifData.Length;

			if (XmpData != null)
				size += XmpData.Length * 2; // Unicode string

			if (IccProfile != null)
				size += IccProfile.Length;

			// Estimate for string properties
			size += EstimateStringSize(Software);
			size += EstimateStringSize(Description);
			size += EstimateStringSize(Copyright);
			size += EstimateStringSize(Author);

			return size;
		}
	}

	/// <inheritdoc />
	public virtual bool HasLargeMetadata => EstimatedMemoryUsage > ImageConstants.LargeMetadataThreshold;

	/// <inheritdoc />
	public abstract IRasterMetadata Clone();

	/// <inheritdoc />
	public virtual void Clear()
	{
		Width = 0;
		Height = 0;
		BitDepth = 8;
		ExifData = null;
		XmpData = null;
		IccProfile = null;
		CreationTime = null;
		ModificationTime = null;
		Software = null;
		Description = null;
		Copyright = null;
		Author = null;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public virtual async ValueTask DisposeAsync()
	{
		if (HasLargeMetadata)
		{
			await Task.Run(() => Dispose(true)).ConfigureAwait(false);
		}
		else
		{
			Dispose(true);
		}
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Releases resources.
	/// </summary>
	/// <param name="disposing">True to release managed resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Clear large arrays
			ExifData = null;
			IccProfile = null;
			
			// Clear strings
			XmpData = null;
			Software = null;
			Description = null;
			Copyright = null;
			Author = null;
		}

		_disposed = true;
	}

	/// <summary>
	/// Gets the base memory size for the metadata object.
	/// </summary>
	/// <returns>Base size in bytes.</returns>
	protected virtual long GetBaseMemorySize()
	{
		// Base object size estimate
		return 256;
	}

	/// <summary>
	/// Estimates the memory size of a string.
	/// </summary>
	/// <param name="str">The string to estimate.</param>
	/// <returns>Estimated size in bytes.</returns>
	protected static long EstimateStringSize(string? str)
	{
		if (string.IsNullOrEmpty(str))
			return 0;
		
		// Unicode string: 2 bytes per character plus overhead
		return (str.Length * 2) + 24;
	}

	/// <summary>
	/// Copies base metadata properties from this instance to another.
	/// </summary>
	/// <param name="target">The target metadata instance.</param>
	protected virtual void CopyBaseTo(RasterMetadataBase target)
	{
		target.Width = Width;
		target.Height = Height;
		target.BitDepth = BitDepth;
		target.ExifData = ExifData?.ToArray();
		target.XmpData = XmpData;
		target.IccProfile = IccProfile?.ToArray();
		target.CreationTime = CreationTime;
		target.ModificationTime = ModificationTime;
		target.Software = Software;
		target.Description = Description;
		target.Copyright = Copyright;
		target.Author = Author;
	}
}