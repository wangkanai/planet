// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Runtime.InteropServices;

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Represents a TIFF raster image with format-specific properties.</summary>
public class TiffRaster : ITiffRaster
{
	/// <inheritdoc />
	public int Width { get; set; }

	/// <inheritdoc />
	public int Height { get; set; }

	/// <inheritdoc />
	public TiffColorDepth ColorDepth { get; set; }

	/// <inheritdoc />
	public TiffCompression Compression { get; set; }

	/// <inheritdoc />
	public TiffMetadata Metadata { get; set; } = new();

	/// <inheritdoc />
	public int SamplesPerPixel { get; set; }

	/// <summary>Inline storage for up to 4 samples (covers 95% of TIFF use cases).</summary>
#pragma warning disable CS0414// Field assigned but never used - accessed via MemoryMarshal
	private int _sample1, _sample2, _sample3, _sample4;
#pragma warning restore CS0414

	/// <summary>Backing array for cases with more than 4 samples.</summary>
	private int[]? _bitsPerSampleArray;

	/// <summary>Number of samples per pixel.</summary>
	private int _samplesCount;

	/// <inheritdoc />
	public PhotometricInterpretation PhotometricInterpretation { get; set; }

	/// <inheritdoc />
	public bool HasAlpha { get; set; }

	/// <inheritdoc />
	public int PlanarConfiguration { get; set; } = 1;

	/// <inheritdoc />
	public bool HasLargeMetadata => EstimatedMetadataSize > 1_000_000; // 1MB threshold

	/// <inheritdoc />
	public long EstimatedMetadataSize
	{
		get
		{
			var size = 0L;
			if (!Metadata.ExifData.IsEmpty)
				size += Metadata.ExifData.Length;
			if (!Metadata.IccProfile.IsEmpty)
				size += Metadata.IccProfile.Length;
			if (!Metadata.XmpData.IsEmpty)
				size += Metadata.XmpData.Length;
			if (!Metadata.GeoKeyDirectory.IsEmpty)
				size += Metadata.GeoKeyDirectory.Length;
			if (!Metadata.GeoDoubleParams.IsEmpty)
				size += Metadata.GeoDoubleParams.Length;
			if (!Metadata.GeoAsciiParams.IsEmpty)
				size += Metadata.GeoAsciiParams.Length;
			
			// Add estimated size of image strips/tiles data if stored in metadata
			foreach (var strip in Metadata.StripOffsets)
				size += Metadata.StripByteCounts.ElementAtOrDefault(strip.Key);
			
			return size;
		}
	}

	private static readonly int[] Int32Array = [8, 8, 8];

	/// <summary>Initializes a new instance of the <see cref="TiffRaster"/> class.</summary>
	public TiffRaster()
	{
		ColorDepth                = TiffColorDepth.TwentyFourBit;
		Compression               = TiffCompression.None;
		PhotometricInterpretation = PhotometricInterpretation.Rgb;
		SamplesPerPixel           = 3;
		SetBitsPerSample(Int32Array);
	}

	/// <summary>Initializes a new instance of the <see cref="TiffRaster"/> class with specified dimensions.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	public TiffRaster(int width, int height) : this()
	{
		Width  = width;
		Height = height;
	}

	/// <inheritdoc />
	public ReadOnlySpan<int> BitsPerSample
	{
		get
		{
			// Fast path for most common cases using unsafe operations for better performance
			if (_samplesCount <= 4)
				return _samplesCount switch
				{
					0 => ReadOnlySpan<int>.Empty,
					1 => MemoryMarshal.CreateReadOnlySpan(ref _sample1, 1),
					2 => MemoryMarshal.CreateReadOnlySpan(ref _sample1, 2),
					3 => MemoryMarshal.CreateReadOnlySpan(ref _sample1, 3),
					4 => MemoryMarshal.CreateReadOnlySpan(ref _sample1, 4),
					_ => ReadOnlySpan<int>.Empty // Should never hit this
				};

			// Fallback to array for larger cases
			return _bitsPerSampleArray.AsSpan();
		}
	}


	/// <summary>Sets the bits per sample values with optimal performance for common cases.</summary>
	/// <param name="bitsPerSample">The bits per sample array.</param>
	public void SetBitsPerSample(int[] bitsPerSample)
		=> SetBitsPerSample(bitsPerSample.AsSpan());

	/// <summary>Sets the bits per sample values with optimal performance for common cases.</summary>
	/// <param name="bitsPerSample">The bits per sample span.</param>
	public void SetBitsPerSample(ReadOnlySpan<int> bitsPerSample)
	{
		_samplesCount = bitsPerSample.Length;

		switch (bitsPerSample.Length)
		{
			case 0:
				_bitsPerSampleArray = null;
				break;
			case 1:
				_sample1            = bitsPerSample[0];
				_bitsPerSampleArray = null;
				break;
			case 2:
				_sample1            = bitsPerSample[0];
				_sample2            = bitsPerSample[1];
				_bitsPerSampleArray = null;
				break;
			case 3:
				_sample1            = bitsPerSample[0];
				_sample2            = bitsPerSample[1];
				_sample3            = bitsPerSample[2];
				_bitsPerSampleArray = null;
				break;
			case 4:
				_sample1            = bitsPerSample[0];
				_sample2            = bitsPerSample[1];
				_sample3            = bitsPerSample[2];
				_sample4            = bitsPerSample[3];
				_bitsPerSampleArray = null;
				break;
			default:
				// Fallback to array for more than 4 samples (rare cases)
				_bitsPerSampleArray = bitsPerSample.ToArray();
				break;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Implementation for resource cleanup
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (HasLargeMetadata)
		{
			// For large TIFF metadata (common with GeoTIFF), clear in stages
			await Task.Yield();
			Metadata.ExifData = ReadOnlyMemory<byte>.Empty;
			await Task.Yield();
			Metadata.IccProfile = ReadOnlyMemory<byte>.Empty;
			await Task.Yield();
			Metadata.XmpData = ReadOnlyMemory<byte>.Empty;
			await Task.Yield();
			Metadata.GeoKeyDirectory = ReadOnlyMemory<byte>.Empty;
			await Task.Yield();
			Metadata.GeoDoubleParams = ReadOnlyMemory<byte>.Empty;
			await Task.Yield();
			Metadata.GeoAsciiParams = ReadOnlyMemory<byte>.Empty;
			await Task.Yield();
			
			// Clear strip data in batches for large TIFF files
			Metadata.StripOffsets.Clear();
			Metadata.StripByteCounts.Clear();
			_bitsPerSampleArray = null;
			
			// Suggest GC for very large TIFF metadata
			if (EstimatedMetadataSize > 10_000_000) // 10MB
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
		}
		else
		{
			Dispose(true);
		}
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Free managed resources if any
			_bitsPerSampleArray = null;
			Metadata.ExifData = ReadOnlyMemory<byte>.Empty;
			Metadata.IccProfile = ReadOnlyMemory<byte>.Empty;
			Metadata.XmpData = ReadOnlyMemory<byte>.Empty;
			Metadata.GeoKeyDirectory = ReadOnlyMemory<byte>.Empty;
			Metadata.GeoDoubleParams = ReadOnlyMemory<byte>.Empty;
			Metadata.GeoAsciiParams = ReadOnlyMemory<byte>.Empty;
			Metadata.StripOffsets.Clear();
			Metadata.StripByteCounts.Clear();
		}

		// Free unmanaged resources if any
	}
}
