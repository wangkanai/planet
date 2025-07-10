// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters;

/// <summary>
/// Base implementation of raster metadata with common properties and functionality.
/// </summary>
public abstract class RasterMetadata : Metadata, IRasterMetadata
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
	public virtual double? XResolution { get; set; }

	/// <inheritdoc />
	public virtual double? YResolution { get; set; }

	/// <inheritdoc />
	public virtual int? ResolutionUnit { get; set; }

	/// <inheritdoc />
	public virtual int? ColorSpace { get; set; }

	/// <inheritdoc />
	public virtual double? GpsLatitude { get; set; }

	/// <inheritdoc />
	public virtual double? GpsLongitude { get; set; }

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
			
			// Add missing raster properties
			size += sizeof(double); // XResolution
			size += sizeof(double); // YResolution
			size += sizeof(int);    // ResolutionUnit
			size += sizeof(int);    // ColorSpace (nullable int)
			size += sizeof(double); // GpsLatitude
			size += sizeof(double); // GpsLongitude

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
	IRasterMetadata IRasterMetadata.Clone() => CloneRaster();

	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();
		
		BitDepth       = 8;
		ExifData       = null;
		XmpData        = null;
		IccProfile     = null;
		XResolution    = null;
		YResolution    = null;
		ResolutionUnit = null;
		ColorSpace     = null;
		GpsLatitude    = null;
		GpsLongitude   = null;
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
	protected virtual void CopyRasterTo(RasterMetadata target)
	{
		// Copy base properties
		base.CopyBaseTo(target);
		
		// Copy raster-specific properties
		target.BitDepth       = BitDepth;
		target.ExifData       = ExifData?.ToArray();
		target.XmpData        = XmpData;
		target.IccProfile     = IccProfile?.ToArray();
		target.XResolution    = XResolution;
		target.YResolution    = YResolution;
		target.ResolutionUnit = ResolutionUnit;
		target.ColorSpace     = ColorSpace;
		target.GpsLatitude    = GpsLatitude;
		target.GpsLongitude   = GpsLongitude;
	}
}
