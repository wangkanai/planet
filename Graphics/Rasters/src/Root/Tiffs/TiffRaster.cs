// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Runtime.InteropServices;

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Represents a TIFF raster image with format-specific properties.</summary>
public class TiffRaster : Raster, ITiffRaster
{
	/// <inheritdoc />
	public override int Width { get; set; }

	/// <inheritdoc />
	public override int Height { get; set; }

	/// <inheritdoc />
	public TiffColorDepth ColorDepth { get; set; }

	/// <inheritdoc />
	public TiffCompression Compression { get; set; }

	/// <inheritdoc />
	public TiffMetadata Metadata { get; set; } = new();

	/// <inheritdoc />
	public int SamplesPerPixel { get; set; }

	/// <summary>Inline storage for up to 4 samples (covers 95% of TIFF use cases).</summary>
#pragma warning disable CS0414 // Field assigned but never used - accessed via MemoryMarshal
	private int _sample1, _sample2, _sample3, _sample4;
#pragma warning restore CS0414

	/// <summary>Backing array for cases with more than 4 samples.</summary>
	private int[]? _bitsPerSampleArray;

	private static readonly int[] Int32Array = [8, 8, 8];

	/// <summary>Number of samples per pixel.</summary>
	private int _samplesCount;

	/// <inheritdoc />
	public PhotometricInterpretation PhotometricInterpretation { get; set; }

	/// <inheritdoc />
	public bool HasAlpha { get; set; }

	/// <inheritdoc />
	public int PlanarConfiguration { get; set; } = 1;

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
	public override bool HasLargeMetadata
		=> EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold;

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			var size = 0L;

			// Add string metadata sizes
			if (!string.IsNullOrEmpty(Metadata.ImageDescription))
				size += System.Text.Encoding.UTF8.GetByteCount(Metadata.ImageDescription);
			if (!string.IsNullOrEmpty(Metadata.Make))
				size += System.Text.Encoding.UTF8.GetByteCount(Metadata.Make);
			if (!string.IsNullOrEmpty(Metadata.Model))
				size += System.Text.Encoding.UTF8.GetByteCount(Metadata.Model);
			if (!string.IsNullOrEmpty(Metadata.Software))
				size += System.Text.Encoding.UTF8.GetByteCount(Metadata.Software);
			if (!string.IsNullOrEmpty(Metadata.Copyright))
				size += System.Text.Encoding.UTF8.GetByteCount(Metadata.Copyright);
			if (!string.IsNullOrEmpty(Metadata.Artist))
				size += System.Text.Encoding.UTF8.GetByteCount(Metadata.Artist);

			// Add TIFF-specific array data sizes
			if (Metadata.StripOffsets != null)
				size += Metadata.StripOffsets.Length * sizeof(int);
			if (Metadata.StripByteCounts != null)
				size += Metadata.StripByteCounts.Length * sizeof(int);
			if (Metadata.TileOffsets != null)
				size += Metadata.TileOffsets.Length * sizeof(int);
			if (Metadata.TileByteCounts != null)
				size += Metadata.TileByteCounts.Length * sizeof(int);

			// Add color data sizes
			if (Metadata.ColorMap != null)
				size += Metadata.ColorMap.Length * sizeof(ushort);
			if (Metadata.TransferFunction != null)
				size += Metadata.TransferFunction.Length * sizeof(ushort);

			// Add chromaticity and color space data
			if (Metadata.WhitePoint != null)
				size += Metadata.WhitePoint.Length * sizeof(double);
			if (Metadata.PrimaryChromaticities != null)
				size += Metadata.PrimaryChromaticities.Length * sizeof(double);
			if (Metadata.YCbCrCoefficients != null)
				size += Metadata.YCbCrCoefficients.Length * sizeof(double);
			if (Metadata.ReferenceBlackWhite != null)
				size += Metadata.ReferenceBlackWhite.Length * sizeof(double);

			// Add embedded metadata sizes
			if (Metadata.ExifIfd != null)
				size += Metadata.ExifIfd.Length;
			if (Metadata.GpsIfd != null)
				size += Metadata.GpsIfd.Length;
			if (Metadata.IccProfile != null)
				size += Metadata.IccProfile.Length;
			if (Metadata.XmpData != null)
				size += Metadata.XmpData.Length;
			if (Metadata.IptcData != null)
				size += Metadata.IptcData.Length;

			// Add custom tags size (more accurate estimation)
			foreach (var tag in Metadata.CustomTags.Values)
			{
				size += tag switch
				{
					string str => System.Text.Encoding.UTF8.GetByteCount(str),
					byte[] bytes => bytes.Length,
					int[] ints => ints.Length * sizeof(int),
					ushort[] ushorts => ushorts.Length * sizeof(ushort),
					double[] doubles => doubles.Length * sizeof(double),
					float[] floats => floats.Length * sizeof(float),
					_ => 16 // Default estimate for other types
				};
			}

			// Add bits per sample array size
			if (_bitsPerSampleArray != null)
				size += _bitsPerSampleArray.Length * sizeof(int);

			// Add TIFF directory entry overhead (12 bytes per standard tag + IFD overhead)
			var estimatedTagCount = 20; // Standard TIFF tags
			if (Metadata.CustomTags.Count > 0)
				estimatedTagCount += Metadata.CustomTags.Count;
			
			size += estimatedTagCount * 12; // 12 bytes per directory entry
			size += 6; // IFD header (2 bytes count + 4 bytes next IFD pointer)

			return size;
		}
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
	protected override async ValueTask DisposeAsyncCore()
	{
		if (HasLargeMetadata)
		{
			// For large TIFF metadata, clear in stages with yielding
			await Task.Yield();
			Metadata.ImageDescription = null;

			await Task.Yield();
			Metadata.Make = null;

			await Task.Yield();
			Metadata.Model = null;

			await Task.Yield();
			Metadata.Software = null;

			await Task.Yield();
			Metadata.Copyright = null;

			await Task.Yield();
			Metadata.Artist = null;

			await Task.Yield();
			Metadata.CustomTags.Clear();

			await Task.Yield();
			_bitsPerSampleArray = null;

			// Clear TIFF-specific arrays with yielding
			await Task.Yield();
			Metadata.StripOffsets = null;
			Metadata.StripByteCounts = null;

			await Task.Yield();
			Metadata.TileOffsets = null;
			Metadata.TileByteCounts = null;

			await Task.Yield();
			Metadata.ColorMap = null;
			Metadata.TransferFunction = null;

			await Task.Yield();
			Metadata.WhitePoint = null;
			Metadata.PrimaryChromaticities = null;

			await Task.Yield();
			Metadata.YCbCrCoefficients = null;
			Metadata.ReferenceBlackWhite = null;

			await Task.Yield();
			Metadata.ExifIfd = null;
			Metadata.GpsIfd = null;

			await Task.Yield();
			Metadata.IccProfile = null;
			Metadata.XmpData = null;
			Metadata.IptcData = null;

			// Let the runtime handle garbage collection automatically
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
			// Clear TIFF-specific managed resources
			_bitsPerSampleArray = null;
			Metadata.ImageDescription = null;
			Metadata.Make = null;
			Metadata.Model = null;
			Metadata.Software = null;
			Metadata.Copyright = null;
			Metadata.Artist = null;
			Metadata.CustomTags.Clear();

			// Clear TIFF-specific arrays
			Metadata.StripOffsets = null;
			Metadata.StripByteCounts = null;
			Metadata.TileOffsets = null;
			Metadata.TileByteCounts = null;
			Metadata.ColorMap = null;
			Metadata.TransferFunction = null;
			Metadata.WhitePoint = null;
			Metadata.PrimaryChromaticities = null;
			Metadata.YCbCrCoefficients = null;
			Metadata.ReferenceBlackWhite = null;

			// Clear embedded metadata
			Metadata.ExifIfd = null;
			Metadata.GpsIfd = null;
			Metadata.IccProfile = null;
			Metadata.XmpData = null;
			Metadata.IptcData = null;
		}

		// Call base class disposal
		base.Dispose(disposing);
	}
}
