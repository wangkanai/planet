// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Tiffs;

namespace Wangkanai.Graphics.Rasters.Benchmark;

/// <summary>Baseline TiffRaster implementation using int[] array for performance comparison.</summary>
public class TiffRasterBaseline : ITiffRaster
{
	/// <inheritdoc />
	public int Width { get; set; }

	/// <inheritdoc />
	public int Height { get; set; }

	IMetadata IImage.Metadata { get; }

	/// <inheritdoc />
	public TiffColorDepth ColorDepth { get; set; }

	/// <inheritdoc />
	public TiffCompression Compression { get; set; }

	/// <inheritdoc />
	public TiffMetadata Metadata { get; set; } = new();

	/// <inheritdoc />
	public int SamplesPerPixel { get; set; }

	/// <summary>Backing array for bits per sample values (baseline approach).</summary>
	private int[] _bitsPerSample = Array.Empty<int>();

	/// <inheritdoc />
	public ReadOnlySpan<int> BitsPerSample => _bitsPerSample.AsSpan();

	/// <summary>Sets the bits per sample values using the baseline int[] approach.</summary>
	/// <param name="bitsPerSample">The bits per sample array.</param>
	public void SetBitsPerSample(int[] bitsPerSample)
	{
		_bitsPerSample = bitsPerSample;
	}

	/// <summary>Sets the bits per sample values using the baseline int[] approach.</summary>
	/// <param name="bitsPerSample">The bits per sample span.</param>
	public void SetBitsPerSample(ReadOnlySpan<int> bitsPerSample)
	{
		_bitsPerSample = bitsPerSample.ToArray();
	}

	/// <inheritdoc />
	public PhotometricInterpretation PhotometricInterpretation { get; set; }

	/// <inheritdoc />
	public bool HasAlpha { get; set; }

	/// <inheritdoc />
	public int PlanarConfiguration { get; set; } = 1;

	/// <inheritdoc />
	public bool HasLargeMetadata => EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold;

	/// <inheritdoc />
	public long EstimatedMetadataSize
	{
		get
		{
			// Baseline implementation - simple metadata size estimation
			var size = 0L;

			// Add string metadata sizes (baseline approach)
			if (!string.IsNullOrEmpty(Metadata.ImageDescription))
				size += Metadata.ImageDescription.Length * 2; // Rough Unicode estimate
			if (!string.IsNullOrEmpty(Metadata.Make))
				size += Metadata.Make.Length * 2;
			if (!string.IsNullOrEmpty(Metadata.Model))
				size += Metadata.Model.Length * 2;
			if (!string.IsNullOrEmpty(Metadata.Software))
				size += Metadata.Software.Length * 2;
			if (!string.IsNullOrEmpty(Metadata.Copyright))
				size += Metadata.Copyright.Length * 2;
			if (!string.IsNullOrEmpty(Metadata.Artist))
				size += Metadata.Artist.Length * 2;

			// Add custom tags size (baseline approach)
			size += Metadata.CustomTags.Count * 20; // Rough estimate per tag

			return size;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="TiffRasterBaseline"/> class.</summary>
	public TiffRasterBaseline()
	{
		ColorDepth                = TiffColorDepth.TwentyFourBit;
		Compression               = TiffCompression.None;
		PhotometricInterpretation = PhotometricInterpretation.Rgb;
		SamplesPerPixel           = 3;
		SetBitsPerSample(new[] { 8, 8, 8 });
	}

	/// <summary>Initializes a new instance of the <see cref="TiffRasterBaseline"/> class with specified dimensions.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	public TiffRasterBaseline(int width, int height) : this()
	{
		Width = width;
		Height = height;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		// Baseline implementation - simple async disposal
		if (HasLargeMetadata)
		{
			// For baseline, just yield once for large metadata
			await Task.Yield();
		}

		Dispose();
		GC.SuppressFinalize(this);
	}
}
