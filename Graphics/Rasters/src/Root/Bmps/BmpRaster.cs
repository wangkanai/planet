// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

/// <summary>Represents a BMP raster image with format-specific properties.</summary>
public sealed class BmpRaster : Raster, IBmpRaster
{
	/// <inheritdoc />
	public override int Width { get; set; }

	/// <inheritdoc />
	public override int Height { get; set; }

	/// <inheritdoc />
	public BmpColorDepth ColorDepth { get; set; }

	/// <inheritdoc />
	public BmpCompression Compression { get; set; }

	/// <inheritdoc />
	public override IMetadata Metadata => BmpMetadata;

	/// <summary>Gets the BMP-specific metadata.</summary>
	BmpMetadata IBmpRaster.Metadata => BmpMetadata;

	/// <summary>Gets or sets the BMP-specific metadata.</summary>
	public BmpMetadata BmpMetadata { get; set; } = new();

	/// <inheritdoc />
	public int HorizontalResolution { get; set; } = BmpConstants.DefaultHorizontalResolution;

	/// <inheritdoc />
	public int VerticalResolution { get; set; } = BmpConstants.DefaultVerticalResolution;

	/// <inheritdoc />
	public byte[]? ColorPalette { get; set; }

	/// <summary>Initializes a new instance of the <see cref="BmpRaster"/> class.</summary>
	public BmpRaster()
	{
		ColorDepth  = BmpColorDepth.TwentyFourBit;
		Compression = BmpCompression.Rgb;
	}

	/// <summary>Initializes a new instance of the <see cref="BmpRaster"/> class with specified dimensions.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	public BmpRaster(int width, int height) : this()
	{
		Width           = width;
		Height          = height;
		BmpMetadata.Width  = width;
		BmpMetadata.Height = height;
	}

	/// <summary>Initializes a new instance of the <see cref="BmpRaster"/> class with specified dimensions and color depth.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="colorDepth">The color depth of the image.</param>
	public BmpRaster(int width, int height, BmpColorDepth colorDepth) : this(width, height)
	{
		ColorDepth            = colorDepth;
		BmpMetadata.BitsPerPixel = (ushort)colorDepth;

		// Initialize default bit masks for specific formats
		InitializeDefaultBitMasks();
	}

	/// <inheritdoc />
	public bool HasPalette
		=> ColorDepth <= BmpColorDepth.EightBit;

	/// <inheritdoc />
	public bool HasTransparency
		=> ColorDepth == BmpColorDepth.ThirtyTwoBit ||
		   Compression == BmpCompression.BitFields &&
		   BmpMetadata.AlphaMask != 0;

	/// <inheritdoc />
	public bool IsTopDown
		=> BmpMetadata.RawHeight < 0;

	/// <inheritdoc />
	public uint PixelDataSize
		=> (uint)(RowStride * Math.Abs(Height));

	/// <inheritdoc />
	public int BytesPerPixel
		=> (int)ColorDepth switch
		{
			1  => 0, // Packed bits
			4  => 0, // Packed bits
			8  => 1,
			16 => 2,
			24 => 3,
			32 => 4,
			_  => throw new NotSupportedException($"Unsupported color depth: {ColorDepth}")
		};

	/// <inheritdoc />
	public int RowStride
	{
		get
		{
			var bitsPerRow  = Width * (int)ColorDepth;
			var bytesPerRow = (bitsPerRow + 7) / 8;                                                  // Round up to nearest byte
			return (bytesPerRow + BmpConstants.RowAlignment - 1) & ~(BmpConstants.RowAlignment - 1); // Align to 4-byte boundary
		}
	}


	/// <inheritdoc />
	public (uint Red, uint Green, uint Blue, uint Alpha) GetBitMasks()
	{
		// Return current bit masks or defaults based on color depth and compression
		if (Compression == BmpCompression.BitFields)
			return (BmpMetadata.RedMask, BmpMetadata.GreenMask, BmpMetadata.BlueMask, BmpMetadata.AlphaMask);

		// Return default masks based on color depth
		return ColorDepth switch
		{
			BmpColorDepth.SixteenBit   => (BmpConstants.RGB555Masks.Red, BmpConstants.RGB555Masks.Green, BmpConstants.RGB555Masks.Blue, BmpConstants.RGB555Masks.Alpha),
			BmpColorDepth.ThirtyTwoBit => (BmpConstants.ARGB8888Masks.Red, BmpConstants.ARGB8888Masks.Green, BmpConstants.ARGB8888Masks.Blue, BmpConstants.ARGB8888Masks.Alpha),
			_                          => (0, 0, 0, 0)
		};
	}

	/// <inheritdoc />
	public void ConvertToRgb()
	{
		if (ColorDepth == BmpColorDepth.TwentyFourBit && Compression == BmpCompression.Rgb)
			return; // Already in RGB format

		// Convert to 24-bit RGB
		ColorDepth            = BmpColorDepth.TwentyFourBit;
		Compression           = BmpCompression.Rgb;
		BmpMetadata.BitsPerPixel = (ushort)ColorDepth;
		BmpMetadata.Compression  = Compression;

		// Clear bit masks and palette
		BmpMetadata.RedMask   = 0;
		BmpMetadata.GreenMask = 0;
		BmpMetadata.BlueMask  = 0;
		BmpMetadata.AlphaMask = 0;
		ColorPalette       = null;
	}

	/// <inheritdoc />
	public void ApplyPalette(byte[] palette)
	{
		if (!HasPalette)
			throw new InvalidOperationException($"Cannot apply palette to {ColorDepth}-bit image. Palettes are only supported for 1, 4, and 8-bit images.");

		if (palette.Length % BmpConstants.PaletteEntrySize != 0)
			throw new ArgumentException($"Palette size must be a multiple of {BmpConstants.PaletteEntrySize} bytes (BGRA format).", nameof(palette));

		var maxColors      = BmpMetadata.PaletteColors;
		var providedColors = palette.Length / BmpConstants.PaletteEntrySize;

		if (providedColors > maxColors)
			throw new ArgumentException($"Palette contains too many colors. Maximum for {ColorDepth}-bit is {maxColors}, provided {providedColors}.", nameof(palette));

		ColorPalette = new byte[palette.Length];
		Array.Copy(palette, ColorPalette, palette.Length);
		BmpMetadata.ColorPalette = ColorPalette;
		BmpMetadata.ColorsUsed   = (uint)providedColors;
	}

	/// <inheritdoc />
	public void SetBitMasks(uint redMask, uint greenMask, uint blueMask, uint alphaMask = 0)
	{
		if (ColorDepth != BmpColorDepth.SixteenBit && ColorDepth != BmpColorDepth.ThirtyTwoBit)
			throw new InvalidOperationException($"Bit masks are only supported for 16-bit and 32-bit images, not {ColorDepth}.");

		Compression          = BmpCompression.BitFields;
		BmpMetadata.Compression = Compression;
		BmpMetadata.RedMask     = redMask;
		BmpMetadata.GreenMask   = greenMask;
		BmpMetadata.BlueMask    = blueMask;
		BmpMetadata.AlphaMask   = alphaMask;
	}

	/// <inheritdoc />
	public bool IsValid()
	{
		// Basic validation checks
		if (Width <= 0 || Height == 0)
			return false;

		if (Math.Abs(Height) > BmpConstants.MaxHeight || Width > BmpConstants.MaxWidth)
			return false;

		if (!Enum.IsDefined(typeof(BmpColorDepth), ColorDepth))
			return false;

		if (!Enum.IsDefined(typeof(BmpCompression), Compression))
			return false;

		// Validate color depth and compression compatibility
		if (Compression == BmpCompression.Rle4 && ColorDepth != BmpColorDepth.FourBit)
			return false;

		if (Compression == BmpCompression.Rle8 && ColorDepth != BmpColorDepth.EightBit)
			return false;

		// Validate palette requirements
		if (HasPalette && ColorPalette == null && BmpMetadata.ColorsUsed > 0)
			return false;

		return true;
	}

	/// <inheritdoc />
	public long GetEstimatedFileSize()
	{
		var fileSize = 0L;

		// File header
		fileSize += BmpConstants.FileHeaderSize;

		// DIB header
		fileSize += BmpMetadata.HeaderSize;

		// Color palette
		if (HasPalette)
		{
			var paletteColors = BmpMetadata.ColorsUsed > 0 ? BmpMetadata.ColorsUsed : BmpMetadata.PaletteColors;
			fileSize += paletteColors * BmpConstants.PaletteEntrySize;
		}

		// Pixel data
		fileSize += PixelDataSize;

		// ICC profile data (for V5 headers)
		if (BmpMetadata.HeaderSize >= BmpConstants.BitmapV5HeaderSize)
			fileSize += BmpMetadata.ProfileSize;

		return fileSize;
	}

	/// <summary>Initializes default bit masks based on color depth.</summary>
	private void InitializeDefaultBitMasks()
	{
		switch (ColorDepth)
		{
			case BmpColorDepth.SixteenBit:
				// Default to RGB555 format
				BmpMetadata.RedMask   = BmpConstants.RGB555Masks.Red;
				BmpMetadata.GreenMask = BmpConstants.RGB555Masks.Green;
				BmpMetadata.BlueMask  = BmpConstants.RGB555Masks.Blue;
				BmpMetadata.AlphaMask = BmpConstants.RGB555Masks.Alpha;
				break;

			case BmpColorDepth.ThirtyTwoBit:
				// Default to ARGB8888 format
				BmpMetadata.RedMask   = BmpConstants.ARGB8888Masks.Red;
				BmpMetadata.GreenMask = BmpConstants.ARGB8888Masks.Green;
				BmpMetadata.BlueMask  = BmpConstants.ARGB8888Masks.Blue;
				BmpMetadata.AlphaMask = BmpConstants.ARGB8888Masks.Alpha;
				break;
		}
	}

	/// <inheritdoc />
	protected override async ValueTask DisposeAsyncCore()
	{
		if (BmpMetadata.HasLargeMetadata)
		{
			// For large BMP metadata, clear in stages with yielding
			await Task.Yield();
			ColorPalette = null;

			await Task.Yield();
			BmpMetadata.ColorPalette = null;

			await Task.Yield();
			BmpMetadata.CustomFields.Clear();

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
			// Clear BMP-specific managed resources
			ColorPalette          = null;
			BmpMetadata.ColorPalette = null;
			BmpMetadata.CustomFields.Clear();
		}

		// Call base class disposal
		base.Dispose(disposing);
	}
}
