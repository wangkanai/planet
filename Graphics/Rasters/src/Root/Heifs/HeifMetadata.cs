// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Heifs;

using System.Threading.Tasks;
using Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Represents comprehensive metadata for HEIF images including EXIF, XMP, ICC profiles, and HDR information.
/// </summary>
public sealed class HeifMetadata : RasterMetadata
{
	/// <summary>
	/// Gets or sets the HDR metadata.
	/// </summary>
	public HdrMetadata? HdrMetadata { get; set; }

	/// <summary>
	/// Gets or sets the camera metadata.
	/// </summary>
	public CameraMetadata? CameraMetadata { get; set; }

	/// <summary>
	/// Gets or sets the GPS coordinates.
	/// </summary>
	public GpsCoordinates? GpsCoordinates { get; set; }

	/// <summary>
	/// Gets or sets the orientation of the image.
	/// </summary>
	public ImageOrientation Orientation { get; set; } = ImageOrientation.Normal;

	/// <summary>
	/// Gets or sets the color space information.
	/// </summary>
	public string? ColorSpaceInfo { get; set; }

	/// <summary>
	/// Gets or sets the white balance setting.
	/// </summary>
	public string? WhiteBalance { get; set; }

	/// <summary>
	/// Gets or sets codec-specific parameters.
	/// </summary>
	public Dictionary<string, object>? CodecParameters { get; set; }

	/// <summary>
	/// Gets or sets custom application-specific metadata.
	/// </summary>
	public Dictionary<string, object>? CustomMetadata { get; set; }

	/// <summary>
	/// Gets or sets the thumbnail data.
	/// </summary>
	public byte[]? ThumbnailData { get; set; }

	/// <summary>
	/// Gets or sets the preview data.
	/// </summary>
	public byte[]? PreviewData { get; set; }

	/// <summary>
	/// Gets or sets the depth map data.
	/// </summary>
	public byte[]? DepthMapData { get; set; }

	/// <summary>
	/// Gets or sets auxiliary image data.
	/// </summary>
	public Dictionary<string, byte[]>? AuxiliaryImages { get; set; }

	/// <summary>
	/// Gets the total estimated memory usage of all metadata in bytes.
	/// </summary>
	public override long EstimatedMetadataSize
	{
		get
		{
			var size = base.EstimatedMetadataSize;
			
			if (ThumbnailData != null) size += ThumbnailData.Length;
			if (PreviewData != null) size += PreviewData.Length;
			if (DepthMapData != null) size += DepthMapData.Length;
			
			if (AuxiliaryImages != null)
			{
				foreach (var aux in AuxiliaryImages.Values)
					size += aux.Length;
			}

			// Add estimated size for HEIF-specific properties
			size += 1024;
			
			return size;
		}
	}

	/// <summary>
	/// Gets whether this metadata contains large data that might benefit from async disposal.
	/// </summary>
	public override bool HasLargeMetadata => EstimatedMetadataSize > HeifConstants.Memory.DefaultMetadataBufferSizeMB * 1024 * 1024;

	/// <summary>
	/// Creates a copy of this metadata instance.
	/// </summary>
	/// <returns>A new metadata instance with copied values.</returns>
	public override IMetadata Clone() => CloneRaster();
	
	/// <inheritdoc />
	public override IRasterMetadata CloneRaster()
	{
		ThrowIfDisposed();
		
		var clone = new HeifMetadata();
		CopyRasterTo(clone);
		
		clone.HdrMetadata = HdrMetadata?.Clone();
		clone.CameraMetadata = CameraMetadata?.Clone();
		clone.GpsCoordinates = GpsCoordinates?.Clone();
		clone.Orientation = Orientation;
		clone.ColorSpaceInfo = ColorSpaceInfo;
		clone.WhiteBalance = WhiteBalance;
		clone.CodecParameters = CodecParameters != null ? new Dictionary<string, object>(CodecParameters) : null;
		clone.CustomMetadata = CustomMetadata != null ? new Dictionary<string, object>(CustomMetadata) : null;
		clone.ThumbnailData = ThumbnailData?.ToArray();
		clone.PreviewData = PreviewData?.ToArray();
		clone.DepthMapData = DepthMapData?.ToArray();
		clone.AuxiliaryImages = AuxiliaryImages?.ToDictionary(
			kvp => kvp.Key, 
			kvp => kvp.Value.ToArray());
			
		return clone;
	}

	/// <summary>
	/// Clears all metadata.
	/// </summary>
	public override void Clear()
	{
		base.Clear();
		
		HdrMetadata = null;
		CameraMetadata = null;
		GpsCoordinates = null;
		Orientation = ImageOrientation.Normal;
		ColorSpaceInfo = null;
		WhiteBalance = null;
		CodecParameters?.Clear();
		CustomMetadata?.Clear();
		ThumbnailData = null;
		PreviewData = null;
		DepthMapData = null;
		AuxiliaryImages?.Clear();
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			HdrMetadata = null;
			CameraMetadata = null;
			ThumbnailData = null;
			PreviewData = null;
			DepthMapData = null;
			CodecParameters?.Clear();
			CustomMetadata?.Clear();
			AuxiliaryImages?.Clear();
		}
		
		base.Dispose(disposing);
	}

	/// <inheritdoc />
	public override async ValueTask DisposeAsync()
	{
		// For large metadata, we might want to do async cleanup
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
}