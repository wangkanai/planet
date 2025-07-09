// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters;

/// <summary>
/// Base implementation of raster metadata with common properties and functionality.
/// </summary>
public abstract class RasterMetadataBase : MetadataBase, IRasterMetadata
{
	/// <inheritdoc />
	public virtual int BitDepth { get; set; } = 8;

	/// <inheritdoc />
	public virtual byte[]? ExifData { get; set; }

	/// <inheritdoc />
	public virtual string? XmpData { get; set; }

	/// <inheritdoc />
	public virtual byte[]? IccProfile { get; set; }

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			// Get base size which includes common properties
			long size = GetBaseMemorySize();

			// Add raster-specific properties
			size += EstimateByteArraySize(ExifData);
			size += EstimateStringSize(XmpData);
			size += EstimateByteArraySize(IccProfile);

			return size;
		}
	}

	/// <inheritdoc />
	public abstract override IMetadata Clone();
	
	/// <summary>
	/// Creates a deep copy of the raster metadata.
	/// </summary>
	/// <returns>A new instance with the same values.</returns>
	public abstract IRasterMetadata CloneRaster();

	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();
		
		BitDepth   = 8;
		ExifData   = null;
		XmpData    = null;
		IccProfile = null;
	}

	/// <inheritdoc />
	protected override void DisposeManagedResources()
	{
		// Clear large arrays
		ExifData   = null;
		IccProfile = null;
		XmpData    = null;
	}


	/// <summary>
	/// Copies raster metadata properties from this instance to another.
	/// </summary>
	/// <param name="target">The target raster metadata instance.</param>
	protected virtual void CopyRasterTo(RasterMetadataBase target)
	{
		// Copy base properties
		base.CopyBaseTo(target);
		
		// Copy raster-specific properties
		target.BitDepth   = BitDepth;
		target.ExifData   = ExifData?.ToArray();
		target.XmpData    = XmpData;
		target.IccProfile = IccProfile?.ToArray();
	}
}
