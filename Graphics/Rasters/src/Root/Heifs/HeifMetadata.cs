// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Heifs;

using Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Represents comprehensive metadata for HEIF images including EXIF, XMP, ICC profiles, and HDR information.
/// </summary>
public sealed class HeifMetadata : IDisposable, IAsyncDisposable
{
	private bool _disposed;

	/// <summary>
	/// Gets or sets the EXIF metadata.
	/// </summary>
	public byte[]? ExifData { get; set; }

	/// <summary>
	/// Gets or sets the XMP metadata.
	/// </summary>
	public byte[]? XmpData { get; set; }

	/// <summary>
	/// Gets or sets the ICC color profile.
	/// </summary>
	public byte[]? IccProfile { get; set; }

	/// <summary>
	/// Gets or sets the HDR metadata.
	/// </summary>
	public HdrMetadata? HdrMetadata { get; set; }

	/// <summary>
	/// Gets or sets the image creation timestamp.
	/// </summary>
	public DateTimeOffset? CreationTime { get; set; }

	/// <summary>
	/// Gets or sets the image modification timestamp.
	/// </summary>
	public DateTimeOffset? ModificationTime { get; set; }

	/// <summary>
	/// Gets or sets the software used to create the image.
	/// </summary>
	public string? Software { get; set; }

	/// <summary>
	/// Gets or sets the image description or caption.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the image copyright information.
	/// </summary>
	public string? Copyright { get; set; }

	/// <summary>
	/// Gets or sets the image author or artist.
	/// </summary>
	public string? Author { get; set; }

	/// <summary>
	/// Gets or sets the camera make.
	/// </summary>
	public string? CameraMake { get; set; }

	/// <summary>
	/// Gets or sets the camera model.
	/// </summary>
	public string? CameraModel { get; set; }

	/// <summary>
	/// Gets or sets the lens make.
	/// </summary>
	public string? LensMake { get; set; }

	/// <summary>
	/// Gets or sets the lens model.
	/// </summary>
	public string? LensModel { get; set; }

	/// <summary>
	/// Gets or sets the focal length in millimeters.
	/// </summary>
	public double? FocalLength { get; set; }

	/// <summary>
	/// Gets or sets the aperture (f-number).
	/// </summary>
	public double? Aperture { get; set; }

	/// <summary>
	/// Gets or sets the exposure time in seconds.
	/// </summary>
	public double? ExposureTime { get; set; }

	/// <summary>
	/// Gets or sets the ISO sensitivity.
	/// </summary>
	public int? IsoSensitivity { get; set; }

	/// <summary>
	/// Gets or sets the GPS coordinates.
	/// </summary>
	public GpsCoordinates? GpsCoordinates { get; set; }

	/// <summary>
	/// Gets or sets the orientation of the image.
	/// </summary>
	public ImageOrientation Orientation { get; set; } = ImageOrientation.Normal;

	/// <summary>
	/// Gets or sets the pixel density in pixels per inch.
	/// </summary>
	public double? PixelDensity { get; set; }

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
	/// Gets the total estimated size of all metadata in bytes.
	/// </summary>
	public long EstimatedSize
	{
		get
		{
			var size = 0L;
			
			if (ExifData != null) size += ExifData.Length;
			if (XmpData != null) size += XmpData.Length;
			if (IccProfile != null) size += IccProfile.Length;
			if (ThumbnailData != null) size += ThumbnailData.Length;
			if (PreviewData != null) size += PreviewData.Length;
			if (DepthMapData != null) size += DepthMapData.Length;
			
			if (AuxiliaryImages != null)
			{
				foreach (var aux in AuxiliaryImages.Values)
					size += aux.Length;
			}

			// Add estimated size for text properties and HDR metadata
			size += 2048;
			
			return size;
		}
	}

	/// <summary>
	/// Gets whether this metadata contains large data that might benefit from async disposal.
	/// </summary>
	public bool HasLargeData => EstimatedSize > HeifConstants.Memory.DefaultMetadataBufferSizeMB * 1024 * 1024;

	/// <summary>
	/// Creates a copy of this metadata instance.
	/// </summary>
	/// <returns>A new metadata instance with copied values.</returns>
	public HeifMetadata Clone()
	{
		ThrowIfDisposed();

		return new HeifMetadata
		{
			ExifData = ExifData?.ToArray(),
			XmpData = XmpData?.ToArray(),
			IccProfile = IccProfile?.ToArray(),
			HdrMetadata = HdrMetadata?.Clone(),
			CreationTime = CreationTime,
			ModificationTime = ModificationTime,
			Software = Software,
			Description = Description,
			Copyright = Copyright,
			Author = Author,
			CameraMake = CameraMake,
			CameraModel = CameraModel,
			LensMake = LensMake,
			LensModel = LensModel,
			FocalLength = FocalLength,
			Aperture = Aperture,
			ExposureTime = ExposureTime,
			IsoSensitivity = IsoSensitivity,
			GpsCoordinates = GpsCoordinates?.Clone(),
			Orientation = Orientation,
			PixelDensity = PixelDensity,
			ColorSpaceInfo = ColorSpaceInfo,
			WhiteBalance = WhiteBalance,
			CodecParameters = CodecParameters != null ? new Dictionary<string, object>(CodecParameters) : null,
			CustomMetadata = CustomMetadata != null ? new Dictionary<string, object>(CustomMetadata) : null,
			ThumbnailData = ThumbnailData?.ToArray(),
			PreviewData = PreviewData?.ToArray(),
			DepthMapData = DepthMapData?.ToArray(),
			AuxiliaryImages = AuxiliaryImages?.ToDictionary(
				kvp => kvp.Key, 
				kvp => kvp.Value.ToArray())
		};
	}

	/// <summary>
	/// Clears all metadata.
	/// </summary>
	public void Clear()
	{
		ThrowIfDisposed();

		ExifData = null;
		XmpData = null;
		IccProfile = null;
		HdrMetadata = null;
		CreationTime = null;
		ModificationTime = null;
		Software = null;
		Description = null;
		Copyright = null;
		Author = null;
		CameraMake = null;
		CameraModel = null;
		LensMake = null;
		LensModel = null;
		FocalLength = null;
		Aperture = null;
		ExposureTime = null;
		IsoSensitivity = null;
		GpsCoordinates = null;
		Orientation = ImageOrientation.Normal;
		PixelDensity = null;
		ColorSpaceInfo = null;
		WhiteBalance = null;
		CodecParameters?.Clear();
		CustomMetadata?.Clear();
		ThumbnailData = null;
		PreviewData = null;
		DepthMapData = null;
		AuxiliaryImages?.Clear();
	}

	private void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(HeifMetadata));
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (_disposed)
			return;

		ExifData = null;
		XmpData = null;
		IccProfile = null;
		HdrMetadata = null;
		ThumbnailData = null;
		PreviewData = null;
		DepthMapData = null;
		CodecParameters?.Clear();
		CustomMetadata?.Clear();
		AuxiliaryImages?.Clear();

		_disposed = true;
	}

	/// <inheritdoc />
	public ValueTask DisposeAsync()
	{
		if (_disposed)
			return ValueTask.CompletedTask;

		// For large metadata, we might want to do async cleanup
		if (HasLargeData)
		{
			return DisposeAsyncCore();
		}

		Dispose();
		return ValueTask.CompletedTask;
	}

	private async ValueTask DisposeAsyncCore()
	{
		// Simulate async cleanup for large metadata
		await Task.Yield();
		
		ExifData = null;
		XmpData = null;
		IccProfile = null;
		HdrMetadata = null;
		ThumbnailData = null;
		PreviewData = null;
		DepthMapData = null;
		CodecParameters?.Clear();
		CustomMetadata?.Clear();
		AuxiliaryImages?.Clear();

		_disposed = true;
	}
}