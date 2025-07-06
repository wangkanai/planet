// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Webps;

/// <summary>Represents a WebP raster image implementation with high-performance optimizations.</summary>
public class WebPRaster : IWebPRaster
{
	private WebPFormat _format = WebPFormat.Simple;
	private WebPCompression _compression = WebPCompression.VP8;
	private WebPColorMode _colorMode = WebPColorMode.Rgb;
	private WebPPreset _preset = WebPPreset.Default;
	private int _quality = WebPConstants.DefaultQuality;
	private int _compressionLevel = WebPConstants.DefaultCompressionLevel;
	private double _compressionRatio = 4.0;

	/// <summary>Initializes a new instance of the <see cref="WebPRaster"/> class.</summary>
	public WebPRaster()
	{
		Width = 1;
		Height = 1;
		Format = WebPFormat.Simple;
		Compression = WebPCompression.VP8;
		ColorMode = WebPColorMode.Rgb;
		Quality = WebPConstants.DefaultQuality;
		CompressionLevel = WebPConstants.DefaultCompressionLevel;
		Preset = WebPPreset.Default;
		Metadata = new WebPMetadata();
	}

	/// <summary>Initializes a new instance of the <see cref="WebPRaster"/> class with specified dimensions.</summary>
	/// <param name="width">The width of the image in pixels.</param>
	/// <param name="height">The height of the image in pixels.</param>
	public WebPRaster(int width, int height) : this()
	{
		Width = Math.Clamp(width, (int)WebPConstants.MinWidth, (int)WebPConstants.MaxWidth);
		Height = Math.Clamp(height, (int)WebPConstants.MinHeight, (int)WebPConstants.MaxHeight);
	}

	/// <summary>Initializes a new instance of the <see cref="WebPRaster"/> class with specified dimensions and quality.</summary>
	/// <param name="width">The width of the image in pixels.</param>
	/// <param name="height">The height of the image in pixels.</param>
	/// <param name="quality">The quality level (0-100) for lossy compression.</param>
	public WebPRaster(int width, int height, int quality) : this(width, height)
	{
		Quality = Math.Clamp(quality, WebPConstants.MinQuality, WebPConstants.MaxQuality);
	}

	/// <inheritdoc />
	public int Width { get; set; } = 1;

	/// <inheritdoc />
	public int Height { get; set; } = 1;

	/// <inheritdoc />
	public WebPFormat Format
	{
		get => _format;
		set
		{
			_format = value;
			UpdateDependentProperties();
		}
	}

	/// <inheritdoc />
	public WebPCompression Compression
	{
		get => _compression;
		set
		{
			_compression = value;
			UpdateDependentProperties();
		}
	}

	/// <inheritdoc />
	public WebPColorMode ColorMode
	{
		get => _colorMode;
		set
		{
			_colorMode = value;
			UpdateDependentProperties();
		}
	}

	/// <inheritdoc />
	public int Quality
	{
		get => _quality;
		set => _quality = Math.Clamp(value, WebPConstants.MinQuality, WebPConstants.MaxQuality);
	}

	/// <inheritdoc />
	public int CompressionLevel
	{
		get => _compressionLevel;
		set => _compressionLevel = Math.Clamp(value, WebPConstants.MinCompressionLevel, WebPConstants.MaxCompressionLevel);
	}

	/// <inheritdoc />
	public WebPPreset Preset
	{
		get => _preset;
		set
		{
			_preset = value;
			ApplyPresetOptimizations();
		}
	}

	/// <inheritdoc />
	public WebPMetadata Metadata { get; set; }

	/// <inheritdoc />
	public int Channels => ColorMode == WebPColorMode.Rgba ? WebPConstants.RgbaChannels : WebPConstants.RgbChannels;

	/// <inheritdoc />
	public bool HasAlpha => ColorMode == WebPColorMode.Rgba || Metadata.HasAlpha;

	/// <inheritdoc />
	public bool IsLossless => Compression == WebPCompression.VP8L;

	/// <inheritdoc />
	public bool IsAnimated => Metadata.HasAnimation && Metadata.AnimationFrames.Count > 0;

	/// <inheritdoc />
	public double CompressionRatio
	{
		get => _compressionRatio;
		set => _compressionRatio = Math.Max(1.0, value);
	}

	/// <summary>Sets the color mode and updates related properties for performance.</summary>
	/// <param name="colorMode">The color mode to set.</param>
	public void SetColorMode(WebPColorMode colorMode)
	{
		_colorMode = colorMode;
		Metadata.HasAlpha = colorMode == WebPColorMode.Rgba;
		UpdateDependentProperties();
	}

	/// <summary>Configures the WebP for lossless compression with optimal settings.</summary>
	public void ConfigureLossless()
	{
		Compression = WebPCompression.VP8L;
		Format = WebPFormat.Lossless;
		CompressionLevel = WebPConstants.DefaultCompressionLevel;
		_compressionRatio = 2.5; // Typical lossless ratio
	}

	/// <summary>Configures the WebP for lossy compression with specified quality.</summary>
	/// <param name="quality">The quality level (0-100).</param>
	public void ConfigureLossy(int quality = WebPConstants.DefaultQuality)
	{
		Compression = WebPCompression.VP8;
		Format = WebPFormat.Simple;
		Quality = quality;
		_compressionRatio = CalculateLossyCompressionRatio(quality);
	}

	/// <summary>Enables extended features and sets the format to Extended.</summary>
	public void EnableExtendedFeatures()
	{
		Format = WebPFormat.Extended;
		Metadata.IsExtended = true;
	}

	/// <inheritdoc />
	public bool IsValid()
		=> Width > 0 && Height > 0 
		   && Width <= WebPConstants.MaxWidth && Height <= WebPConstants.MaxHeight
		   && Quality is >= WebPConstants.MinQuality and <= WebPConstants.MaxQuality
		   && CompressionLevel is >= WebPConstants.MinCompressionLevel and <= WebPConstants.MaxCompressionLevel;

	/// <inheritdoc />
	public long GetEstimatedFileSize()
	{
		if (!IsValid())
			return 0;

		// Calculate base size
		var pixelCount = (long)Width * Height;
		var bytesPerPixel = Channels;
		var uncompressedSize = pixelCount * bytesPerPixel;

		// Apply compression ratio
		var compressedDataSize = (long)(uncompressedSize / CompressionRatio);

		// Add WebP container overhead
		var overhead = WebPConstants.ContainerOverhead;

		// Add metadata overhead
		if (!Metadata.IccProfile.IsEmpty)
			overhead += Metadata.IccProfile.Length + WebPConstants.ChunkHeaderSize;
		if (!Metadata.ExifData.IsEmpty)
			overhead += Metadata.ExifData.Length + WebPConstants.ChunkHeaderSize;
		if (!Metadata.XmpData.IsEmpty)
			overhead += Metadata.XmpData.Length + WebPConstants.ChunkHeaderSize;

		// Add animation overhead if applicable
		if (IsAnimated)
		{
			overhead += WebPConstants.AnimChunkSize + WebPConstants.ChunkHeaderSize;
			overhead += Metadata.AnimationFrames.Count * (WebPConstants.ChunkHeaderSize + 16); // ANMF headers
		}

		return compressedDataSize + overhead;
	}

	/// <summary>Updates dependent properties when format or compression changes.</summary>
	private void UpdateDependentProperties()
	{
		// Sync format and compression
		switch (Compression)
		{
			case WebPCompression.VP8:
				if (Format == WebPFormat.Lossless)
					Format = WebPFormat.Simple;
				break;
			case WebPCompression.VP8L:
				Format = WebPFormat.Lossless;
				break;
		}

		// Update metadata flags
		Metadata.HasAlpha = HasAlpha;
		Metadata.IsExtended = Format == WebPFormat.Extended;

		// Update compression ratio based on settings
		_compressionRatio = IsLossless 
			? CalculateLosslessCompressionRatio() 
			: CalculateLossyCompressionRatio(Quality);
	}

	/// <summary>Applies optimizations based on the selected preset.</summary>
	private void ApplyPresetOptimizations()
	{
		switch (Preset)
		{
			case WebPPreset.Picture:
				Quality = Math.Max(Quality, 80);
				break;
			case WebPPreset.Photo:
				Quality = Math.Max(Quality, 85);
				break;
			case WebPPreset.Drawing:
				if (!IsLossless)
					Quality = Math.Max(Quality, 90);
				break;
			case WebPPreset.Icon:
				ConfigureLossless();
				break;
			case WebPPreset.Text:
				ConfigureLossless();
				CompressionLevel = 9; // Maximum compression for text
				break;
		}
	}

	/// <summary>Calculates the compression ratio for lossy compression based on quality.</summary>
	/// <param name="quality">The quality level (0-100).</param>
	/// <returns>The estimated compression ratio.</returns>
	private static double CalculateLossyCompressionRatio(int quality)
	{
		// WebP typically achieves better compression than JPEG
		// Quality 0: ~20:1 ratio, Quality 100: ~2:1 ratio
		var normalizedQuality = quality / 100.0;
		return 2.0 + (18.0 * (1.0 - normalizedQuality));
	}

	/// <summary>Calculates the compression ratio for lossless compression.</summary>
	/// <returns>The estimated compression ratio.</returns>
	private double CalculateLosslessCompressionRatio()
	{
		// Lossless compression ratio depends on compression level
		// Level 0: ~2:1, Level 9: ~3:1
		var levelFactor = CompressionLevel / 9.0;
		return 2.0 + levelFactor;
	}

	/// <summary>Disposes of the WebP raster resources.</summary>
	public void Dispose()
	{
		Metadata.IccProfile = ReadOnlyMemory<byte>.Empty;
		Metadata.ExifData = ReadOnlyMemory<byte>.Empty;
		Metadata.XmpData = ReadOnlyMemory<byte>.Empty;
		Metadata.CustomChunks.Clear();
		Metadata.AnimationFrames.Clear();
		GC.SuppressFinalize(this);
	}
}