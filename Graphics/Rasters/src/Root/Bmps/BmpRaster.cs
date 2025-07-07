// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

/// <summary>Represents a BMP raster image with format-specific properties.</summary>
public class BmpRaster : Raster, IBmpRaster
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
	public BmpMetadata Metadata { get; set; } = new();

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
		Metadata.Width  = width;
		Metadata.Height = height;
	}

	/// <summary>Initializes a new instance of the <see cref="BmpRaster"/> class with specified dimensions and color depth.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="colorDepth">The color depth of the image.</param>
	public BmpRaster(int width, int height, BmpColorDepth colorDepth) : this(width, height)
	{
		ColorDepth            = colorDepth;
		Metadata.BitsPerPixel = (ushort)colorDepth;

		// Initialize default bit masks for specific formats
		InitializeDefaultBitMasks();
	}

	/// <inheritdoc />
	public bool HasPalette => ColorDepth <= BmpColorDepth.EightBit;

	/// <inheritdoc />
	public bool HasTransparency => ColorDepth == BmpColorDepth.ThirtyTwoBit ||
	                               (Compression == BmpCompression.BitFields && Metadata.AlphaMask != 0);

	/// <inheritdoc />
	public bool IsTopDown => Metadata.Height < 0;

	/// <inheritdoc />
	public int BytesPerPixel => (int)ColorDepth switch
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
	public uint PixelDataSize => (uint)(RowStride * Math.Abs(Height));

	/// <inheritdoc />
	public override bool HasLargeMetadata
	{
		get
		{
			// Consider BMP as having large metadata if:
			// 1. It has a large color palette (256+ colors)
			// 2. The total file size is large
			// 3. It has ICC profile data (V5 headers)
			var estimatedSize = EstimatedMetadataSize;
			return estimatedSize > ImageConstants.LargeMetadataThreshold;
		}
	}

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			var size = 0L;

			// File header
			size += BmpConstants.FileHeaderSize;

			// DIB header
			size += Metadata.HeaderSize;

			// Color palette
			if (HasPalette && ColorPalette != null)
				size += ColorPalette.Length;

			// ICC profile data (for V5 headers)
			if (Metadata.HeaderSize >= BmpConstants.BitmapV5HeaderSize)
				size += Metadata.ProfileSize;

			// Custom metadata fields
			foreach (var field in Metadata.CustomFields.Values)
			{
				size += field switch
				{
					string str   => System.Text.Encoding.UTF8.GetByteCount(str),
					byte[] bytes => bytes.Length,
					_            => 32 // Default estimate for other types
				};
			}

			return size;
		}
	}

	/// <inheritdoc />
	public (uint Red, uint Green, uint Blue, uint Alpha) GetBitMasks()
	{
		// Return current bit masks or defaults based on color depth and compression
		if (Compression == BmpCompression.BitFields)
		{
			return (Metadata.RedMask, Metadata.GreenMask, Metadata.BlueMask, Metadata.AlphaMask);
		}

		// Return default masks based on color depth
		return ColorDepth switch
		{
			BmpColorDepth.SixteenBit => (BmpConstants.RGB555Masks.Red, BmpConstants.RGB555Masks.Green,
				                            BmpConstants.RGB555Masks.Blue, BmpConstants.RGB555Masks.Alpha),
			BmpColorDepth.ThirtyTwoBit => (BmpConstants.ARGB8888Masks.Red, BmpConstants.ARGB8888Masks.Green,
				                              BmpConstants.ARGB8888Masks.Blue, BmpConstants.ARGB8888Masks.Alpha),
			_ => (0, 0, 0, 0)
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
		Metadata.BitsPerPixel = (ushort)ColorDepth;
		Metadata.Compression  = Compression;

		// Clear bit masks and palette
		Metadata.RedMask   = 0;
		Metadata.GreenMask = 0;
		Metadata.BlueMask  = 0;
		Metadata.AlphaMask = 0;
		ColorPalette       = null;
	}

	/// <inheritdoc />
	public void ApplyPalette(byte[] palette)
	{
		if (!HasPalette)
			throw new InvalidOperationException($"Cannot apply palette to {ColorDepth}-bit image. Palettes are only supported for 1, 4, and 8-bit images.");

		if (palette.Length % BmpConstants.PaletteEntrySize != 0)
			throw new ArgumentException($"Palette size must be a multiple of {BmpConstants.PaletteEntrySize} bytes (BGRA format).", nameof(palette));

		var maxColors      = Metadata.PaletteColors;
		var providedColors = palette.Length / BmpConstants.PaletteEntrySize;

		if (providedColors > maxColors)
			throw new ArgumentException($"Palette contains too many colors. Maximum for {ColorDepth}-bit is {maxColors}, provided {providedColors}.", nameof(palette));

		ColorPalette = new byte[palette.Length];
		Array.Copy(palette, ColorPalette, palette.Length);
		Metadata.ColorPalette = ColorPalette;
		Metadata.ColorsUsed   = (uint)providedColors;
	}

	/// <inheritdoc />
	public void SetBitMasks(uint redMask, uint greenMask, uint blueMask, uint alphaMask = 0)
	{
		if (ColorDepth != BmpColorDepth.SixteenBit && ColorDepth != BmpColorDepth.ThirtyTwoBit)
			throw new InvalidOperationException($"Bit masks are only supported for 16-bit and 32-bit images, not {ColorDepth}.");

		Compression          = BmpCompression.BitFields;
		Metadata.Compression = Compression;
		Metadata.RedMask     = redMask;
		Metadata.GreenMask   = greenMask;
		Metadata.BlueMask    = blueMask;
		Metadata.AlphaMask   = alphaMask;
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
		if (HasPalette && ColorPalette == null && Metadata.ColorsUsed > 0)
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
		fileSize += Metadata.HeaderSize;

		// Color palette
		if (HasPalette)
		{
			var paletteColors = Metadata.ColorsUsed > 0 ? Metadata.ColorsUsed : Metadata.PaletteColors;
			fileSize += paletteColors * BmpConstants.PaletteEntrySize;
		}

		// Pixel data
		fileSize += PixelDataSize;

		// ICC profile data (for V5 headers)
		if (Metadata.HeaderSize >= BmpConstants.BitmapV5HeaderSize)
			fileSize += Metadata.ProfileSize;

		return fileSize;
	}

	/// <summary>Initializes default bit masks based on color depth.</summary>
	private void InitializeDefaultBitMasks()
	{
		switch (ColorDepth)
		{
			case BmpColorDepth.SixteenBit:
				// Default to RGB555 format
				Metadata.RedMask   = BmpConstants.RGB555Masks.Red;
				Metadata.GreenMask = BmpConstants.RGB555Masks.Green;
				Metadata.BlueMask  = BmpConstants.RGB555Masks.Blue;
				Metadata.AlphaMask = BmpConstants.RGB555Masks.Alpha;
				break;

			case BmpColorDepth.ThirtyTwoBit:
				// Default to ARGB8888 format
				Metadata.RedMask   = BmpConstants.ARGB8888Masks.Red;
				Metadata.GreenMask = BmpConstants.ARGB8888Masks.Green;
				Metadata.BlueMask  = BmpConstants.ARGB8888Masks.Blue;
				Metadata.AlphaMask = BmpConstants.ARGB8888Masks.Alpha;
				break;
		}
	}

	/// <inheritdoc />
	protected override async ValueTask DisposeAsyncCore()
	{
		if (HasLargeMetadata)
		{
			// For large BMP metadata, clear in stages with yielding
			await Task.Yield();
			ColorPalette = null;

			await Task.Yield();
			Metadata.ColorPalette = null;

			await Task.Yield();
			Metadata.CustomFields.Clear();

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
			Metadata.ColorPalette = null;
			Metadata.CustomFields.Clear();
		}

		// Call base class disposal
		base.Dispose(disposing);
	}
}
