// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;
using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Heifs;

/// <summary>
/// Represents a HEIF (High Efficiency Image File Format) raster image implementation.
/// </summary>
public sealed class HeifRaster : Raster, IHeifRaster
{
	private byte[]? _encodedData;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="HeifRaster"/> class.
	/// </summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="hasAlpha">Whether the image has an alpha channel.</param>
	public HeifRaster(int width, int height, bool hasAlpha = false)
	{
		Width = width;
		Height = height;
		HasAlpha = hasAlpha;
		ColorSpace = HeifColorSpace.Srgb;
		Quality = HeifConstants.DefaultQuality;
		BitDepth = HeifConstants.MinBitDepth;
		ChromaSubsampling = HeifChromaSubsampling.Yuv420;
		Speed = HeifConstants.DefaultSpeed;
		Compression = HeifCompression.Hevc;
		IsLossless = false;
		ThreadCount = HeifConstants.DefaultThreadCount;
		Profile = HeifProfile.Main;
		EnableProgressiveDecoding = false;
		GenerateThumbnails = true;
		HeifMetadata = new HeifMetadata();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HeifRaster"/> class from encoded data.
	/// </summary>
	/// <param name="encodedData">The encoded HEIF data.</param>
	public HeifRaster(byte[] encodedData)
	{
		_encodedData = encodedData ?? throw new ArgumentNullException(nameof(encodedData));
		ColorSpace = HeifColorSpace.Srgb;
		Quality = HeifConstants.DefaultQuality;
		BitDepth = HeifConstants.MinBitDepth;
		ChromaSubsampling = HeifChromaSubsampling.Yuv420;
		Speed = HeifConstants.DefaultSpeed;
		Compression = HeifCompression.Hevc;
		IsLossless = false;
		ThreadCount = HeifConstants.DefaultThreadCount;
		Profile = HeifProfile.Main;
		EnableProgressiveDecoding = false;
		GenerateThumbnails = true;
		HeifMetadata = new HeifMetadata();
	}

	/// <inheritdoc />
	public HeifColorSpace ColorSpace { get; set; }

	/// <inheritdoc />
	public int Quality { get; set; }

	/// <inheritdoc />
	public override IMetadata Metadata => HeifMetadata;

	/// <inheritdoc />
	HeifMetadata IHeifRaster.Metadata
	{
		get => HeifMetadata;
		set => HeifMetadata = value;
	}

	/// <summary>Gets or sets the HEIF-specific metadata.</summary>
	public HeifMetadata HeifMetadata { get; set; }

	/// <inheritdoc />
	public int BitDepth { get; set; }

	/// <inheritdoc />
	public bool HasAlpha { get; set; }

	/// <inheritdoc />
	public HeifChromaSubsampling ChromaSubsampling { get; set; }

	/// <inheritdoc />
	public int Speed { get; set; }

	/// <inheritdoc />
	public HeifCompression Compression { get; set; }

	/// <inheritdoc />
	public bool IsLossless { get; set; }

	/// <inheritdoc />
	public bool HasHdrMetadata => HeifMetadata.HdrMetadata != null;

	/// <inheritdoc />
	public int ThreadCount { get; set; }

	/// <inheritdoc />
	public HeifProfile Profile { get; set; }

	/// <inheritdoc />
	public bool EnableProgressiveDecoding { get; set; }

	/// <inheritdoc />
	public bool GenerateThumbnails { get; set; }


	/// <inheritdoc />
	public async Task<byte[]> EncodeAsync(HeifEncodingOptions? options = null)
	{
		ThrowIfDisposed();

		options ??= HeifEncodingOptions.CreateDefault();

		// Validate encoding options
		if (!options.Validate(out var error))
			throw new InvalidOperationException($"Invalid encoding options: {error}");

		// Apply options to current instance
		ApplyEncodingOptions(options);

		// Simulate encoding process
		await Task.Delay(10); // Simulate encoding work

		// Generate mock encoded data
		var estimatedSize = GetEstimatedFileSize();
		var data = new byte[Math.Min(estimatedSize, 1024)];
		Random.Shared.NextBytes(data);

		// Add HEIF signature
		if (data.Length >= 12)
		{
			// ftyp box header (size + type)
			data[0] = 0x00; data[1] = 0x00; data[2] = 0x00; data[3] = 0x18; // box size
			data[4] = (byte)'f'; data[5] = (byte)'t'; data[6] = (byte)'y'; data[7] = (byte)'p'; // box type
			// major brand
			data[8] = (byte)'h'; data[9] = (byte)'e'; data[10] = (byte)'i'; data[11] = (byte)'c'; // heic brand
		}

		_encodedData = data;
		return data;
	}

	/// <inheritdoc />
	public async Task DecodeAsync(byte[] data)
	{
		ThrowIfDisposed();

		if (data == null)
			throw new ArgumentNullException(nameof(data));

		if (data.Length < HeifConstants.BoxHeaderSize)
			throw new ArgumentException("Invalid HEIF data: too small", nameof(data));

		// Simulate decoding process
		await Task.Delay(5);

		// Extract basic information from mock data
		_encodedData = data;

		// Set default dimensions if not already set
		if (Width == 0) Width = 1920;
		if (Height == 0) Height = 1080;
	}

	/// <inheritdoc />
	public void SetHdrMetadata(HdrMetadata hdrMetadata)
	{
		ThrowIfDisposed();
		HeifMetadata.HdrMetadata = hdrMetadata ?? throw new ArgumentNullException(nameof(hdrMetadata));
	}

	/// <inheritdoc />
	public long GetEstimatedFileSize()
	{
		ThrowIfDisposed();

		var pixelCount = (long)Width * Height;
		var channelCount = HasAlpha ? 4 : 3;
		var bitsPerPixel = BitDepth * channelCount;

		// Base size calculation
		var baseSize = pixelCount * bitsPerPixel / 8;

		// Apply compression ratio based on quality and settings
		var compressionRatio = IsLossless ? 0.7 : (100 - Quality) / 100.0 * 0.8 + 0.1;
		var compressedSize = (long)(baseSize * compressionRatio);

		// Add overhead for container and metadata
		var overhead = 8192 + HeifMetadata.EstimatedMetadataSize;

		return compressedSize + overhead;
	}

	/// <inheritdoc />
	public bool IsValid()
	{
		ThrowIfDisposed();
		return Width > 0 
		       && Height > 0 
		       && Width <= HeifConstants.MaxDimension 
		       && Height <= HeifConstants.MaxDimension
		       && Quality is >= HeifConstants.MinQuality and <= HeifConstants.MaxQuality
		       && Speed is >= HeifConstants.MinSpeed and <= HeifConstants.MaxSpeed
		       && BitDepth is >= HeifConstants.MinBitDepth and <= HeifConstants.MaxBitDepth;
	}

	/// <inheritdoc />
	public async Task<byte[]> CreateThumbnailAsync(int maxWidth, int maxHeight)
	{
		ThrowIfDisposed();

		if (maxWidth <= 0) throw new ArgumentOutOfRangeException(nameof(maxWidth));
		if (maxHeight <= 0) throw new ArgumentOutOfRangeException(nameof(maxHeight));

		// Calculate thumbnail dimensions
		var aspectRatio = (double)Width / Height;
		int thumbWidth, thumbHeight;

		if (aspectRatio > (double)maxWidth / maxHeight)
		{
			thumbWidth = maxWidth;
			thumbHeight = (int)(maxWidth / aspectRatio);
		}
		else
		{
			thumbHeight = maxHeight;
			thumbWidth = (int)(maxHeight * aspectRatio);
		}

		// Create thumbnail with thumbnail quality
		var thumbnailOptions = HeifEncodingOptions.CreateThumbnail();
		var thumbnail = new HeifRaster(thumbWidth, thumbHeight, HasAlpha)
		{
			Quality = HeifConstants.QualityPresets.Thumbnail,
			Speed = HeifConstants.SpeedPresets.Fast,
			ColorSpace = ColorSpace,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420
		};

		return await thumbnail.EncodeAsync(thumbnailOptions);
	}

	/// <inheritdoc />
	public void ApplyColorProfile(byte[] iccProfile)
	{
		ThrowIfDisposed();
		HeifMetadata.IccProfile = iccProfile ?? throw new ArgumentNullException(nameof(iccProfile));
	}

	/// <inheritdoc />
	public HeifFeatures GetSupportedFeatures()
	{
		// Return a comprehensive set of supported features
		return HeifFeatures.BasicCodec
		       | HeifFeatures.TenBitDepth
		       | HeifFeatures.TwelveBitDepth
		       | HeifFeatures.HdrMetadata
		       | HeifFeatures.AlphaChannel
		       | HeifFeatures.MultiThreading
		       | HeifFeatures.ExifMetadata
		       | HeifFeatures.XmpMetadata
		       | HeifFeatures.IccProfile
		       | HeifFeatures.LosslessCompression
		       | HeifFeatures.ThumbnailGeneration
		       | HeifFeatures.ProgressiveDecoding;
	}

	/// <inheritdoc />
	public void SetCodecParameters(Dictionary<string, object> codecParameters)
	{
		ThrowIfDisposed();
		
		if (codecParameters == null)
			throw new ArgumentNullException(nameof(codecParameters));

		// Store codec parameters in metadata
		HeifMetadata.CodecParameters = new Dictionary<string, object>(codecParameters);
	}

	/// <inheritdoc />
	public HeifContainerInfo GetContainerInfo()
	{
		ThrowIfDisposed();

		return new HeifContainerInfo
		{
			MajorBrand = Compression switch
			{
				HeifCompression.Hevc => "heic",
				HeifCompression.Avc => "avci",
				HeifCompression.Av1 => "avif",
				_ => "heic"
			},
			MinorVersion = 0,
			CompatibleBrands = new[] { "mif1", "miaf" },
			HasThumbnails = GenerateThumbnails,
			ItemCount = 1,
			BoxCount = EstimateBoxCount()
		};
	}

	private void ApplyEncodingOptions(HeifEncodingOptions options)
	{
		Quality = options.Quality;
		Speed = options.Speed;
		IsLossless = options.IsLossless;
		ChromaSubsampling = options.ChromaSubsampling;
		if (options.ThreadCount.HasValue)
			ThreadCount = options.ThreadCount.Value;
		if (options.Compression.HasValue)
			Compression = options.Compression.Value;
		if (options.Profile.HasValue)
			Profile = options.Profile.Value;
	}

	private int EstimateBoxCount()
	{
		var count = 5; // ftyp, meta, hdlr, pitm, iloc
		if (HeifMetadata.ExifData?.Length > 0) count++;
		if (HeifMetadata.XmpData?.Length > 0) count++;
		if (HeifMetadata.IccProfile?.Length > 0) count++;
		if (HasHdrMetadata) count++;
		if (GenerateThumbnails) count += 2;
		return count;
	}

	private void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(HeifRaster));
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			_encodedData = null;
			Metadata?.Dispose();
		}

		_disposed = true;
		base.Dispose(disposing);
	}

	/// <inheritdoc />
	protected override async ValueTask DisposeAsyncCore()
	{
		if (_disposed)
			return;

		_encodedData = null;
		if (HeifMetadata?.HasLargeMetadata == true)
			await HeifMetadata.DisposeAsync();

		_disposed = true;
	}
}