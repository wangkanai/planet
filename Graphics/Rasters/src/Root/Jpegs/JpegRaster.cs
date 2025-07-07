// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Represents a JPEG raster image with format-specific properties.</summary>
public class JpegRaster : Raster, IJpegRaster
{
	/// <inheritdoc />
	public override int Width { get; set; }

	/// <inheritdoc />
	public override int Height { get; set; }

	/// <inheritdoc />
	public JpegColorMode ColorMode { get; set; }

	/// <inheritdoc />
	public int Quality { get; set; }

	/// <inheritdoc />
	public JpegEncoding Encoding { get; set; }

	/// <inheritdoc />
	public JpegMetadata Metadata { get; set; } = new();

	/// <inheritdoc />
	public int SamplesPerPixel { get; set; }

	/// <inheritdoc />
	public int BitsPerSample { get; set; }

	/// <inheritdoc />
	public JpegChromaSubsampling ChromaSubsampling { get; set; }

	/// <inheritdoc />
	public bool IsProgressive { get; set; }

	/// <inheritdoc />
	public bool IsOptimized { get; set; }

	/// <inheritdoc />
	public double CompressionRatio { get; set; }

	/// <inheritdoc />
	public override bool HasLargeMetadata => EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold;

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			var size = 0L;

			// Add ICC profile size
			if (Metadata.IccProfile != null)
				size += Metadata.IccProfile.Length;

			// Add custom EXIF tags size
			size += Metadata.CustomExifTags.Count * 16; // Estimate 16 bytes per tag

			// Add IPTC tags size
			foreach (var tag in Metadata.IptcTags.Values)
				size += System.Text.Encoding.UTF8.GetByteCount(tag);

			// Add XMP tags size
			foreach (var tag in Metadata.XmpTags.Values)
				size += System.Text.Encoding.UTF8.GetByteCount(tag);

			return size;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="JpegRaster"/> class.</summary>
	public JpegRaster()
	{
		ColorMode         = JpegColorMode.Rgb;
		Quality           = JpegConstants.DefaultQuality;
		Encoding          = JpegEncoding.Baseline;
		SamplesPerPixel   = 3;
		BitsPerSample     = JpegConstants.BitsPerSample;
		ChromaSubsampling = JpegChromaSubsampling.Both;
		IsProgressive     = false;
		IsOptimized       = false;
		CompressionRatio  = 10.0;
	}

	/// <summary>Initializes a new instance of the <see cref="JpegRaster"/> class with specified dimensions.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	public JpegRaster(int width, int height) : this()
	{
		Width  = width;
		Height = height;
	}

	/// <summary>Initializes a new instance of the <see cref="JpegRaster"/> class with specified dimensions and quality.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="quality">The JPEG quality (0-100).</param>
	public JpegRaster(int width, int height, int quality) : this(width, height)
	{
		Quality = Math.Clamp(quality, JpegConstants.MinQuality, JpegConstants.MaxQuality);
	}

	/// <summary>Sets the color mode and updates related properties.</summary>
	/// <param name="colorMode">The color mode to set.</param>
	public void SetColorMode(JpegColorMode colorMode)
	{
		ColorMode = colorMode;
		SamplesPerPixel = colorMode switch
		{
			JpegColorMode.Grayscale => 1,
			JpegColorMode.Rgb       => 3,
			JpegColorMode.Cmyk      => 4,
			JpegColorMode.YCbCr     => 3,
			_                       => 3
		};
	}

	/// <summary>Sets the quality level and updates compression ratio estimate.</summary>
	/// <param name="quality">The quality level (0-100).</param>
	public void SetQuality(int quality)
	{
		Quality = Math.Clamp(quality, JpegConstants.MinQuality, JpegConstants.MaxQuality);

		// Estimate compression ratio based on quality
		CompressionRatio = quality switch
		{
			>= 90 => 3.0,
			>= 80 => 5.0,
			>= 70 => 8.0,
			>= 60 => 12.0,
			>= 50 => 16.0,
			>= 40 => 20.0,
			>= 30 => 25.0,
			>= 20 => 30.0,
			>= 10 => 40.0,
			_     => 50.0
		};
	}

	/// <summary>Validates the JPEG image properties.</summary>
	/// <returns>True if the image properties are valid, false otherwise.</returns>
	public bool IsValid()
	{
		return Width > 0 && Width <= JpegConstants.MaxDimension &&
		       Height > 0 && Height <= JpegConstants.MaxDimension &&
		       Quality >= JpegConstants.MinQuality && Quality <= JpegConstants.MaxQuality &&
		       SamplesPerPixel > 0 && SamplesPerPixel <= 4 &&
		       BitsPerSample == JpegConstants.BitsPerSample;
	}

	/// <summary>Gets the estimated file size in bytes.</summary>
	/// <returns>The estimated file size.</returns>
	public long GetEstimatedFileSize()
	{
		if (!IsValid())
			return 0;

		var uncompressedSize = (long)Width * Height * SamplesPerPixel * (BitsPerSample / 8);
		return (long)(uncompressedSize / CompressionRatio);
	}

	/// <summary>Gets the color depth in bits per pixel.</summary>
	/// <returns>The color depth.</returns>
	public int GetColorDepth()
	{
		return SamplesPerPixel * BitsPerSample;
	}

	/// <inheritdoc />
	protected override async ValueTask DisposeAsyncCore()
	{
		if (HasLargeMetadata)
		{
			// For large metadata, clear in stages with yielding
			await Task.Yield();
			Metadata.IccProfile = [];

			await Task.Yield();
			Metadata.CustomExifTags.Clear();

			await Task.Yield();
			Metadata.IptcTags.Clear();

			await Task.Yield();
			Metadata.XmpTags.Clear();
		}
		else
		{
			// For small metadata, use synchronous disposal
			Dispose(true);
		}
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Clear JPEG-specific managed resources
			Metadata.IccProfile = null;
			Metadata.CustomExifTags.Clear();
			Metadata.IptcTags.Clear();
			Metadata.XmpTags.Clear();
		}

		// Call base class disposal
		base.Dispose(disposing);
	}
}
