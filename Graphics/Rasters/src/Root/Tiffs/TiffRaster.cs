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

	private readonly TiffMetadata _metadata = new();

	/// <summary>Gets the TIFF metadata.</summary>
	public TiffMetadata TiffMetadata => _metadata;

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
	public override IMetadata Metadata => _metadata;

	/// <inheritdoc />
	TiffMetadata ITiffRaster.Metadata => _metadata;

	/// <inheritdoc />
	public ReadOnlySpan<int> BitsPerSample
	{
		get
		{
			// Fast path for most common cases using unsafe operations for better performance
			if (_samplesCount <= 4)
			{
				return _samplesCount switch
				{
					0 => ReadOnlySpan<int>.Empty,
					1 => MemoryMarshal.CreateReadOnlySpan(ref _sample1, 1),
					2 => MemoryMarshal.CreateReadOnlySpan(ref _sample1, 2),
					3 => MemoryMarshal.CreateReadOnlySpan(ref _sample1, 3),
					4 => MemoryMarshal.CreateReadOnlySpan(ref _sample1, 4),
					_ => ReadOnlySpan<int>.Empty // Should never hit this
				};
			}

			// Fallback to array for larger cases
			return _bitsPerSampleArray.AsSpan();
		}
	}


	/// <summary>Sets the bits per sample values with optimal performance for common cases.</summary>
	/// <param name="bitsPerSample">The bits per sample array.</param>
	public void SetBitsPerSample(int[] bitsPerSample)
	{
		SetBitsPerSample(bitsPerSample.AsSpan());
	}

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
		if (_metadata.HasLargeMetadata)
		{
			// For large TIFF metadata, clear in stages with yielding
			await Task.Yield();
			_metadata.ImageDescription = null;

			await Task.Yield();
			_metadata.Make = null;

			await Task.Yield();
			_metadata.Model = null;

			await Task.Yield();
			_metadata.Software = null;

			await Task.Yield();
			_metadata.Copyright = null;

			await Task.Yield();
			_metadata.Artist = null;

			await Task.Yield();
			_metadata.CustomTags.Clear();

			await Task.Yield();
			_bitsPerSampleArray = null;

			// Clear TIFF-specific arrays with yielding
			await Task.Yield();
			_metadata.StripOffsets    = null;
			_metadata.StripByteCounts = null;

			await Task.Yield();
			_metadata.TileOffsets    = null;
			_metadata.TileByteCounts = null;

			await Task.Yield();
			_metadata.ColorMap         = null;
			_metadata.TransferFunction = null;

			await Task.Yield();
			_metadata.WhitePoint            = null;
			_metadata.PrimaryChromaticities = null;

			await Task.Yield();
			_metadata.YCbCrCoefficients   = null;
			_metadata.ReferenceBlackWhite = null;

			await Task.Yield();
			_metadata.ExifIfd = null;
			_metadata.GpsIfd  = null;

			await Task.Yield();
			_metadata.IccProfile = null;
			_metadata.XmpData    = null;
			_metadata.IptcData   = null;

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
			_bitsPerSampleArray       = null;
			_metadata.ImageDescription = null;
			_metadata.Make             = null;
			_metadata.Model            = null;
			_metadata.Software         = null;
			_metadata.Copyright        = null;
			_metadata.Artist           = null;
			_metadata.CustomTags.Clear();

			// Clear TIFF-specific arrays
			_metadata.StripOffsets          = null;
			_metadata.StripByteCounts       = null;
			_metadata.TileOffsets           = null;
			_metadata.TileByteCounts        = null;
			_metadata.ColorMap              = null;
			_metadata.TransferFunction      = null;
			_metadata.WhitePoint            = null;
			_metadata.PrimaryChromaticities = null;
			_metadata.YCbCrCoefficients     = null;
			_metadata.ReferenceBlackWhite   = null;

			// Clear embedded metadata
			_metadata.ExifIfd    = null;
			_metadata.GpsIfd     = null;
			_metadata.IccProfile = null;
			_metadata.XmpData    = null;
			_metadata.IptcData   = null;
		}

		// Call base class disposal
		base.Dispose(disposing);
	}
}
