// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Example of how format-specific metadata classes can extend RasterMetadataBase.
/// This demonstrates the pattern for implementing format-specific metadata.
/// </summary>
/// <remarks>
/// Actual JPEG metadata implementation should be updated in Jpegs/JpegMetadata.cs
/// to inherit from RasterMetadataBase and implement IRasterMetadata.
/// </remarks>
internal class ExampleJpegMetadata : RasterMetadata
{
	/// <summary>Gets or sets the camera metadata.</summary>
	public CameraMetadata? Camera { get; set; }

	/// <summary>Gets or sets the chroma subsampling mode.</summary>
	public ChromaSubsampling ChromaSubsampling { get; set; } = ChromaSubsampling.Yuv420;

	/// <summary>Gets or sets the image orientation.</summary>
	public ImageOrientation Orientation { get; set; } = ImageOrientation.Normal;

	/// <summary>Gets or sets the color space information.</summary>
	public int? ColorSpace { get; set; }

	/// <summary>Gets or sets additional custom EXIF tags.</summary>
	public Dictionary<int, object> CustomExifTags { get; set; } = new();

	/// <summary>Gets or sets IPTC metadata for image descriptions and keywords.</summary>
	public Dictionary<string, string> IptcTags { get; set; } = new();

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			long size = base.EstimatedMetadataSize;

			// Add format-specific memory usage
			size += CustomExifTags.Count * 64;
			size += IptcTags.Count * 128;

			if (Camera != null)
			{
				// Estimate camera metadata size
				size += 512;
			}

			return size;
		}
	}

	/// <inheritdoc />
	public override IMetadata Clone() => CloneRaster();
	
	/// <inheritdoc />
	public override IRasterMetadata CloneRaster()
	{
		var clone = new ExampleJpegMetadata
		{
			Camera = Camera?.Clone(),
			ChromaSubsampling = ChromaSubsampling,
			Orientation = Orientation,
			ColorSpace = ColorSpace,
			CustomExifTags = new Dictionary<int, object>(CustomExifTags),
			IptcTags = new Dictionary<string, string>(IptcTags)
		};

		// Copy base properties
		CopyRasterTo(clone);

		return clone;
	}

	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();
		Camera?.Clear();
		Camera = null;
		ChromaSubsampling = ChromaSubsampling.Yuv420;
		Orientation = ImageOrientation.Normal;
		ColorSpace = null;
		CustomExifTags.Clear();
		IptcTags.Clear();
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			CustomExifTags.Clear();
			IptcTags.Clear();
			Camera = null;
		}

		base.Dispose(disposing);
	}
}