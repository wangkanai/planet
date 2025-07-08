// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;

namespace Wangkanai.Graphics.Rasters.Avifs;

/// <summary>
/// Represents an AVIF (AV1 Image File Format) raster image with advanced compression and HDR capabilities.
/// </summary>
/// <remarks>
/// AVIF is a modern image format based on the AV1 video codec, offering superior compression efficiency
/// while maintaining high image quality. It supports HDR, wide color gamut, and alpha transparency.
/// </remarks>
public sealed class AvifRaster : Raster, IAvifRaster
{
	private byte[]? _encodedData;
	private bool _disposed;
	private AvifMetadata _metadata = new();

	/// <summary>Initializes a new instance of the AVIF raster with default settings.</summary>
	public AvifRaster()
	{
		InitializeDefaults();
	}

	/// <summary>Initializes a new instance of the AVIF raster with specified dimensions.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether the image has an alpha channel.</param>
	public AvifRaster(int width, int height, bool hasAlpha = false)
	{
		if (width <= 0 || width > AvifConstants.MaxDimension)
			throw new ArgumentException($"Width must be between 1 and {AvifConstants.MaxDimension}.", nameof(width));
		if (height <= 0 || height > AvifConstants.MaxDimension)
			throw new ArgumentException($"Height must be between 1 and {AvifConstants.MaxDimension}.", nameof(height));

		Width = width;
		Height = height;

		_metadata.Width = width;
		_metadata.Height = height;
		_metadata.HasAlpha = hasAlpha;

		InitializeDefaults();
	}

	/// <inheritdoc />
	public AvifColorSpace ColorSpace
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.ColorSpace;
		}
		set
		{
			ThrowIfDisposed();
			_metadata.ColorSpace = value;
		}
	}

	/// <inheritdoc />
	public int Quality
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.Quality;
		}
		set
		{
			ThrowIfDisposed();
			if (value < AvifConstants.MinQuality || value > AvifConstants.MaxQuality)
				throw new ArgumentException($"Quality must be between {AvifConstants.MinQuality} and {AvifConstants.MaxQuality}.");
			_metadata.Quality = value;
		}
	}

	/// <inheritdoc />
	public override IMetadata Metadata => _metadata;

	/// <inheritdoc />
	AvifMetadata IAvifRaster.Metadata
	{
		get => _metadata;
		set => _metadata = value;
	}

	/// <summary>Gets the AVIF-specific metadata.</summary>
	public AvifMetadata AvifMetadata => _metadata;

	/// <inheritdoc />
	public int BitDepth
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.BitDepth;
		}
		set
		{
			ThrowIfDisposed();
			if (value != 8 && value != 10 && value != 12)
				throw new ArgumentException("Bit depth must be 8, 10, or 12.");
			_metadata.BitDepth = value;
		}
	}

	/// <inheritdoc />
	public bool HasAlpha
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.HasAlpha;
		}
		set
		{
			ThrowIfDisposed();
			_metadata.HasAlpha = value;
		}
	}

	/// <inheritdoc />
	public AvifChromaSubsampling ChromaSubsampling
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.ChromaSubsampling;
		}
		set
		{
			ThrowIfDisposed();
			_metadata.ChromaSubsampling = value;
		}
	}

	/// <inheritdoc />
	public int Speed
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.Speed;
		}
		set
		{
			ThrowIfDisposed();
			if (value < AvifConstants.MinSpeed || value > AvifConstants.MaxSpeed)
				throw new ArgumentException($"Speed must be between {AvifConstants.MinSpeed} and {AvifConstants.MaxSpeed}.");
			_metadata.Speed = value;
		}
	}

	/// <inheritdoc />
	public bool IsLossless
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.IsLossless;
		}
		set
		{
			ThrowIfDisposed();
			_metadata.IsLossless = value;
			if (value)
			{
				_metadata.Quality = AvifConstants.QualityPresets.Lossless;
				_metadata.ChromaSubsampling = AvifChromaSubsampling.Yuv444;
			}
		}
	}

	/// <inheritdoc />
	public bool HasHdrMetadata
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.HdrInfo != null;
		}
	}

	/// <inheritdoc />
	public int ThreadCount { get; set; } = AvifConstants.DefaultThreadCount;

	/// <inheritdoc />
	public bool EnableFilmGrain
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.UsesFilmGrain;
		}
		set
		{
			ThrowIfDisposed();
			_metadata.UsesFilmGrain = value;
		}
	}


	/// <summary>Initializes default settings for the AVIF raster.</summary>
	private void InitializeDefaults()
	{
		// Set reasonable defaults based on image size
		if (Width > 0 && Height > 0)
		{
			var pixels = (long)Width * Height;

			// Adjust quality based on image size
			if (pixels > 4000000) // > 4MP
			{
				Quality = AvifConstants.QualityPresets.Standard;
				ChromaSubsampling = AvifChromaSubsampling.Yuv420;
			}
			else if (pixels > 1000000) // > 1MP
			{
				Quality = AvifConstants.QualityPresets.Professional;
				ChromaSubsampling = AvifChromaSubsampling.Yuv422;
			}
			else
			{
				Quality = AvifConstants.QualityPresets.Professional;
				ChromaSubsampling = AvifChromaSubsampling.Yuv444;
			}
		}
	}

	/// <inheritdoc />
	public async Task<byte[]> EncodeAsync(AvifEncodingOptions? options = null)
	{
		ThrowIfDisposed();

		options ??= new AvifEncodingOptions
		{
			Quality = Quality,
			Speed = Speed,
			IsLossless = IsLossless,
			ChromaSubsampling = ChromaSubsampling,
			ThreadCount = ThreadCount,
			EnableFilmGrain = EnableFilmGrain
		};

		// Validate options
		if (!options.Validate(out var error))
			throw new ArgumentException($"Invalid encoding options: {error}");

		// Apply options to metadata
		ApplyEncodingOptions(options);

		// Validate configuration
		if (!IsValid())
			throw new InvalidOperationException("Invalid AVIF configuration for encoding.");

		// For now, return placeholder implementation
		// In a real implementation, this would use libavif or similar
		await Task.Yield();

		_encodedData = CreatePlaceholderEncodedData();
		return _encodedData;
	}

	/// <inheritdoc />
	public async Task DecodeAsync(byte[] data)
	{
		ThrowIfDisposed();

		if (data == null || data.Length == 0)
			throw new ArgumentException("AVIF data cannot be null or empty.", nameof(data));

		// Validate AVIF signature
		if (!AvifValidator.IsValidAvifSignature(data))
			throw new ArgumentException("Invalid AVIF file signature.", nameof(data));

		// For now, simulate decoding
		await Task.Yield();

		_encodedData = data;
		ParsePlaceholderData(data);
	}

	/// <inheritdoc />
	public void SetHdrMetadata(HdrMetadata hdrMetadata)
	{
		ThrowIfDisposed();

		if (hdrMetadata == null)
			throw new ArgumentNullException(nameof(hdrMetadata));

		_metadata.HdrInfo = hdrMetadata;

		// Update color space for HDR
		if (hdrMetadata.Format == HdrFormat.Hdr10)
		{
			ColorSpace = AvifColorSpace.Bt2100Pq;
			BitDepth = 10;
		}
		else if (hdrMetadata.Format == HdrFormat.Hlg)
		{
			ColorSpace = AvifColorSpace.Bt2100Hlg;
			BitDepth = 10;
		}
	}

	/// <inheritdoc />
	public long GetEstimatedFileSize()
	{
		ThrowIfDisposed();

		var baseSize = (long)Width * Height * (HasAlpha ? 4 : 3);

		// Adjust for bit depth
		if (BitDepth > 8)
			baseSize = baseSize * BitDepth / 8;

		// Adjust for quality and compression
		if (IsLossless)
		{
			// Lossless typically achieves 2:1 compression
			return baseSize / 2;
		}
		else
		{
			// Lossy compression based on quality
			var compressionRatio = Quality switch
			{
				>= 95 => 4.0,
				>= 90 => 6.0,
				>= 85 => 8.0,
				>= 75 => 12.0,
				>= 60 => 20.0,
				_ => 30.0
			};

			// Adjust for chroma subsampling
			compressionRatio *= ChromaSubsampling switch
			{
				AvifChromaSubsampling.Yuv444 => 1.0,
				AvifChromaSubsampling.Yuv422 => 1.25,
				AvifChromaSubsampling.Yuv420 => 1.5,
				AvifChromaSubsampling.Yuv400 => 2.0,
				_ => 1.0
			};

			return (long)(baseSize / compressionRatio);
		}
	}

	/// <inheritdoc />
	public bool IsValid()
	{
		var validation = AvifValidator.Validate(this);
		return validation.IsValid;
	}

	/// <inheritdoc />
	public async Task<byte[]> CreateThumbnailAsync(int maxWidth, int maxHeight)
	{
		ThrowIfDisposed();

		if (maxWidth <= 0)
			throw new ArgumentException("Maximum width must be positive.", nameof(maxWidth));
		if (maxHeight <= 0)
			throw new ArgumentException("Maximum height must be positive.", nameof(maxHeight));

		// Calculate thumbnail dimensions maintaining aspect ratio
		var aspectRatio = (double)Width / Height;
		int thumbWidth, thumbHeight;

		if (Width > Height)
		{
			thumbWidth = Math.Min(Width, maxWidth);
			thumbHeight = (int)(thumbWidth / aspectRatio);
			if (thumbHeight > maxHeight)
			{
				thumbHeight = maxHeight;
				thumbWidth = (int)(thumbHeight * aspectRatio);
			}
		}
		else
		{
			thumbHeight = Math.Min(Height, maxHeight);
			thumbWidth = (int)(thumbHeight * aspectRatio);
			if (thumbWidth > maxWidth)
			{
				thumbWidth = maxWidth;
				thumbHeight = (int)(thumbWidth / aspectRatio);
			}
		}

		// Create thumbnail with optimized settings
		var thumbnailOptions = new AvifEncodingOptions
		{
			Quality = AvifConstants.QualityPresets.Thumbnail,
			Speed = AvifConstants.SpeedPresets.Fast,
			ChromaSubsampling = AvifChromaSubsampling.Yuv420,
			AddPreviewImage = false
		};

		// For now, return placeholder
		await Task.Yield();
		return CreatePlaceholderEncodedData();
	}

	/// <inheritdoc />
	public void ApplyColorProfile(byte[] iccProfile)
	{
		ThrowIfDisposed();

		if (iccProfile == null || iccProfile.Length == 0)
			throw new ArgumentException("ICC profile cannot be null or empty.", nameof(iccProfile));

		_metadata.IccProfile = iccProfile;
	}

	/// <inheritdoc />
	public AvifFeatures GetSupportedFeatures()
	{
		// In a real implementation, this would query libavif capabilities
		var features = AvifFeatures.BasicCodec |
					   AvifFeatures.TenBitDepth |
					   AvifFeatures.AlphaChannel |
					   AvifFeatures.MultiThreading |
					   AvifFeatures.ExifMetadata |
					   AvifFeatures.XmpMetadata |
					   AvifFeatures.IccProfile |
					   AvifFeatures.LosslessCompression;

		// Check platform-specific features
		if (Environment.Is64BitProcess)
		{
			features |= AvifFeatures.TwelveBitDepth;
			features |= AvifFeatures.FilmGrain;
		}

		// Check for HDR support
		if (OperatingSystem.IsWindows() && Environment.OSVersion.Version.Major >= 10)
		{
			features |= AvifFeatures.HdrMetadata;
		}
		else if (OperatingSystem.IsMacOS())
		{
			features |= AvifFeatures.HdrMetadata;
		}

		return features;
	}

	/// <summary>Applies encoding options to the metadata.</summary>
	private void ApplyEncodingOptions(AvifEncodingOptions options)
	{
		Quality = options.Quality;
		Speed = options.Speed;
		IsLossless = options.IsLossless;
		ChromaSubsampling = options.ChromaSubsampling;
		ThreadCount = options.ThreadCount;
		EnableFilmGrain = options.EnableFilmGrain;
	}

	/// <summary>Creates placeholder encoded data for demonstration.</summary>
	private byte[] CreatePlaceholderEncodedData()
	{
		using var stream = new MemoryStream();
		using var writer = new BinaryWriter(stream);

		// Write file type box
		writer.Write((uint)28); // Box size
		writer.Write(AvifConstants.FileTypeBoxType.AsSpan());
		writer.Write(AvifConstants.AvifBrand.AsSpan());
		writer.Write((uint)0); // Minor version
		writer.Write(AvifConstants.AvifBrand.AsSpan());
		writer.Write(AvifConstants.Mif1Brand.AsSpan());

		// Placeholder for remaining structure
		var estimatedSize = GetEstimatedFileSize();
		var remainingSize = Math.Max(100, (int)(estimatedSize - stream.Position));
		writer.Write(new byte[remainingSize]);

		return stream.ToArray();
	}

	/// <summary>Parses placeholder data for demonstration.</summary>
	private void ParsePlaceholderData(byte[] data)
	{
		if (data.Length < 28)
			throw new ArgumentException("Invalid AVIF data - too small.");

		// Update metadata based on file size estimation
		_metadata.ModificationTime = DateTime.UtcNow;

		// Simulate extraction of basic info
		if (Width == 0 || Height == 0)
		{
			// Estimate dimensions from file size
			var estimatedPixels = data.Length * 8; // Rough estimate
			var dimension = (int)Math.Sqrt(estimatedPixels);
			Width = dimension;
			Height = dimension;
			_metadata.Width = Width;
			_metadata.Height = Height;
		}
	}

	/// <summary>Throws if the object has been disposed.</summary>
	private void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(AvifRaster));
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_encodedData = null;
				_metadata?.Dispose();
			}

			_disposed = true;
		}

		base.Dispose(disposing);
	}

	/// <inheritdoc />
	protected override async ValueTask DisposeAsyncCore()
	{
		if (_metadata?.HasLargeMetadata == true)
		{
			await _metadata.DisposeAsync().ConfigureAwait(false);
		}

		await base.DisposeAsyncCore().ConfigureAwait(false);
	}
}
