// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Pngs;

/// <summary>Represents a PNG raster image implementation.</summary>
public class PngRaster : IPngRaster
{
	private PngColorType _colorType        = PngColorType.Truecolor;
	private byte         _bitDepth         = 8;
	private int          _compressionLevel = 6;

	/// <summary>Initializes a new instance of the <see cref="PngRaster"/> class.</summary>
	public PngRaster()
	{
		Width            = 1;
		Height           = 1;
		ColorType        = PngColorType.Truecolor;
		BitDepth         = 8;
		Compression      = PngCompression.Deflate;
		FilterMethod     = PngFilterMethod.Standard;
		InterlaceMethod  = PngInterlaceMethod.None;
		CompressionLevel = 6;
	}

	/// <summary>Initializes a new instance of the <see cref="PngRaster"/> class.</summary>
	/// <param name="width">The width of the image in pixels.</param>
	/// <param name="height">The height of the image in pixels.</param>
	public PngRaster(int width, int height) : this()
	{
		Width  = Math.Max(1, width);
		Height = Math.Max(1, height);
	}

	/// <summary>Gets or sets the width of the image in pixels.</summary>
	public int Width
	{
		get;
		set;
	}

	/// <summary>Gets or sets the height of the image in pixels.</summary>
	public int Height
	{
		get;
		set;
	}

	/// <summary>Gets or sets the PNG color type.</summary>
	public PngColorType ColorType
	{
		get => _colorType;
		set
		{
			_colorType = value;
			UpdateDependentProperties();
		}
	}

	/// <summary>Gets or sets the bit depth per color component.</summary>
	public byte BitDepth
	{
		get => _bitDepth;
		set => _bitDepth = Math.Clamp(value, (byte)1, (byte)16);
	}

	/// <summary>Gets or sets the compression method.</summary>
	public PngCompression Compression
	{
		get;
		set;
	}

	/// <summary>Gets or sets the filter method.</summary>
	public PngFilterMethod FilterMethod
	{
		get;
		set;
	}

	/// <summary>Gets or sets the interlace method.</summary>
	public PngInterlaceMethod InterlaceMethod
	{
		get;
		set;
	}

	/// <summary>Gets or sets a value indicating whether the image uses a palette.</summary>
	public bool UsesPalette
	{
		get;
		set;
	}

	/// <summary>Gets or sets a value indicating whether the image has transparency.</summary>
	public bool HasTransparency
	{
		get;
		set;
	}

	/// <summary>Gets or sets a value indicating whether the image has an alpha channel.</summary>
	public bool HasAlphaChannel
	{
		get;
		set;
	}

	/// <summary>Gets the number of samples per pixel.</summary>
	public int SamplesPerPixel => ColorType switch
	{
		PngColorType.Grayscale          => 1,
		PngColorType.Truecolor          => 3,
		PngColorType.IndexedColor       => 1,
		PngColorType.GrayscaleWithAlpha => 2,
		PngColorType.TruecolorWithAlpha => 4,
		_                               => 1
	};

	/// <summary>Gets or sets the compression level (0-9, where 9 is maximum compression).</summary>
	public int CompressionLevel
	{
		get => _compressionLevel;
		set => _compressionLevel = Math.Clamp(value, 0, 9);
	}

	/// <summary>Gets the PNG metadata.</summary>
	public PngMetadata Metadata
	{
		get;
	} = new();

	/// <summary>Gets the palette data for indexed-color images.</summary>
	public byte[]? PaletteData
	{
		get;
		set;
	}

	/// <summary>Gets or sets the transparency data.</summary>
	public byte[]? TransparencyData
	{
		get;
		set;
	}

	/// <summary>Validates the PNG raster image.</summary>
	/// <returns>True if the image is valid, false otherwise.</returns>
	public bool IsValid()
	{
		return Width > 0 && Width <= PngConstants.MaxWidth &&
		       Height > 0 && Height <= PngConstants.MaxHeight &&
		       IsValidBitDepthForColorType() &&
		       CompressionLevel >= 0 && CompressionLevel <= 9;
	}

	/// <summary>Gets the estimated file size in bytes.</summary>
	/// <returns>The estimated file size.</returns>
	public long GetEstimatedFileSize()
	{
		if (!IsValid())
			return 0;

		// Estimate based on uncompressed data with typical compression ratio
		var bytesPerPixel    = (SamplesPerPixel * BitDepth + 7) / 8;
		var uncompressedSize = (long)Width * Height * bytesPerPixel;

		// PNG typically achieves 30-70% compression ratio depending on content
		var compressionRatio = CompressionLevel switch
		{
			0    => 1.0,// No compression
			<= 3 => 0.7,// Low compression
			<= 6 => 0.5,// Medium compression
			_    => 0.3 // High compression
		};

		var compressedDataSize = (long)(uncompressedSize * compressionRatio);

		// Add overhead for PNG chunks and headers
		var overhead = PngConstants.SignatureLength +
		               100 +// IHDR, IEND and other critical chunks
		               (Metadata.TextChunks.Count + Metadata.CompressedTextChunks.Count + Metadata.InternationalTextChunks.Count) * 50;

		return compressedDataSize + overhead;
	}

	/// <summary>Gets the color depth in bits per pixel.</summary>
	/// <returns>The color depth.</returns>
	public int GetColorDepth()
	{
		return SamplesPerPixel * BitDepth;
	}

	/// <summary>Disposes of the PNG raster resources.</summary>
	public void Dispose()
	{
		PaletteData      = null;
		TransparencyData = null;
		Metadata?.CustomChunks.Clear();
		GC.SuppressFinalize(this);
	}

	/// <summary>Updates dependent properties when color type changes.</summary>
	private void UpdateDependentProperties()
	{
		UsesPalette     = ColorType == PngColorType.IndexedColor;
		HasAlphaChannel = ColorType is PngColorType.GrayscaleWithAlpha or PngColorType.TruecolorWithAlpha;

		// Set appropriate bit depth for color type if current value is invalid
		if (!IsValidBitDepthForColorType())
		{
			BitDepth = ColorType switch
			{
				PngColorType.Grayscale          => 8,  // Default: 8-bit (supports 1,2,4,8,16)
				PngColorType.Truecolor          => 8,  // Default: 8-bit per channel (supports 8,16)
				PngColorType.IndexedColor       => 4,  // Default: 4-bit for 16 colors (supports 1,2,4,8)
				PngColorType.GrayscaleWithAlpha => 8,  // Default: 8-bit per channel (supports 8,16)
				PngColorType.TruecolorWithAlpha => 8,  // Default: 8-bit per channel (supports 8,16)
				_                               => 8
			};
		}
	}

	/// <summary>Validates if the current bit depth is valid for the current color type.</summary>
	/// <returns>True if the bit depth is valid for the color type, false otherwise.</returns>
	private bool IsValidBitDepthForColorType()
	{
		var allowedBitDepths = ColorType switch
		{
			PngColorType.Grayscale          => PngConstants.BitDepths.Grayscale,
			PngColorType.Truecolor          => PngConstants.BitDepths.Truecolor,
			PngColorType.IndexedColor       => PngConstants.BitDepths.IndexedColor,
			PngColorType.GrayscaleWithAlpha => PngConstants.BitDepths.GrayscaleWithAlpha,
			PngColorType.TruecolorWithAlpha => PngConstants.BitDepths.TruecolorWithAlpha,
			_                               => []
		};

		return allowedBitDepths.Contains(BitDepth);
	}
}
