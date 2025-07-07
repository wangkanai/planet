// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

/// <summary>Represents metadata information for BMP images.</summary>
public class BmpMetadata
{
	/// <summary>Gets or sets the file signature (should be "BM").</summary>
	public string? FileSignature { get; set; } = "BM";

	/// <summary>Gets or sets the total file size in bytes.</summary>
	public uint FileSize { get; set; }

	/// <summary>Gets or sets the offset to the pixel data from the beginning of the file.</summary>
	public uint PixelDataOffset { get; set; }

	/// <summary>Gets or sets the size of the DIB header in bytes.</summary>
	public uint HeaderSize { get; set; } = BmpConstants.BitmapInfoHeaderSize;

	/// <summary>Gets or sets the image width in pixels.</summary>
	public int Width { get; set; }

	/// <summary>Gets or sets the image height in pixels (positive for bottom-up, negative for top-down).</summary>
	public int Height { get; set; }

	/// <summary>Gets or sets the number of color planes (always 1 for BMP).</summary>
	public ushort Planes { get; set; } = BmpConstants.Planes;

	/// <summary>Gets or sets the number of bits per pixel.</summary>
	public ushort BitsPerPixel { get; set; }

	/// <summary>Gets or sets the compression method used.</summary>
	public BmpCompression Compression { get; set; } = BmpCompression.Rgb;

	/// <summary>Gets or sets the size of the compressed image data in bytes (0 for uncompressed).</summary>
	public uint ImageSize { get; set; }

	/// <summary>Gets or sets the horizontal resolution in pixels per meter.</summary>
	public int XPixelsPerMeter { get; set; } = BmpConstants.DefaultHorizontalResolution;

	/// <summary>Gets or sets the vertical resolution in pixels per meter.</summary>
	public int YPixelsPerMeter { get; set; } = BmpConstants.DefaultVerticalResolution;

	/// <summary>Gets or sets the number of colors used in the palette (0 means use maximum for bit depth).</summary>
	public uint ColorsUsed { get; set; }

	/// <summary>Gets or sets the number of important colors (0 means all colors are important).</summary>
	public uint ColorsImportant { get; set; }

	// Extended properties for BITMAPV4HEADER and BITMAPV5HEADER

	/// <summary>Gets or sets the red component bit mask (for BI_BITFIELDS compression).</summary>
	public uint RedMask { get; set; }

	/// <summary>Gets or sets the green component bit mask (for BI_BITFIELDS compression).</summary>
	public uint GreenMask { get; set; }

	/// <summary>Gets or sets the blue component bit mask (for BI_BITFIELDS compression).</summary>
	public uint BlueMask { get; set; }

	/// <summary>Gets or sets the alpha component bit mask (for 32-bit images).</summary>
	public uint AlphaMask { get; set; }

	/// <summary>Gets or sets the color space type (for V4/V5 headers).</summary>
	public uint ColorSpaceType { get; set; }

	/// <summary>Gets or sets the red gamma value (for V4/V5 headers).</summary>
	public uint GammaRed { get; set; }

	/// <summary>Gets or sets the green gamma value (for V4/V5 headers).</summary>
	public uint GammaGreen { get; set; }

	/// <summary>Gets or sets the blue gamma value (for V4/V5 headers).</summary>
	public uint GammaBlue { get; set; }

	/// <summary>Gets or sets the rendering intent (for V5 headers).</summary>
	public uint Intent { get; set; }

	/// <summary>Gets or sets the ICC profile data offset (for V5 headers).</summary>
	public uint ProfileData { get; set; }

	/// <summary>Gets or sets the ICC profile data size (for V5 headers).</summary>
	public uint ProfileSize { get; set; }

	/// <summary>Gets or sets the color palette data.</summary>
	public byte[]? ColorPalette { get; set; }

	/// <summary>Gets or sets additional custom metadata fields.</summary>
	public Dictionary<string, object> CustomFields { get; set; } = new();

	/// <summary>Gets a value indicating whether the image uses a color palette.</summary>
	public bool HasPalette => BitsPerPixel <= 8;

	/// <summary>Gets a value indicating whether the image has transparency (alpha channel).</summary>
	public bool HasAlpha => BitsPerPixel == 32 || (Compression == BmpCompression.BitFields && AlphaMask != 0);

	/// <summary>Gets a value indicating whether the image is top-down (negative height).</summary>
	public bool IsTopDown => Height < 0;

	/// <summary>Gets the absolute height value.</summary>
	public int AbsoluteHeight => Math.Abs(Height);

	/// <summary>Gets the number of colors in the palette based on bits per pixel.</summary>
	public uint PaletteColors => BitsPerPixel switch
	{
		1 => 2,
		4 => 16,
		8 => 256,
		_ => 0
	};

	/// <summary>Gets the expected palette size in bytes.</summary>
	public uint PaletteSizeInBytes => PaletteColors * BmpConstants.PaletteEntrySize;

	/// <summary>Gets the bytes per pixel for the current color depth.</summary>
	public int BytesPerPixel => BitsPerPixel switch
	{
		1 => 0, // Packed bits
		4 => 0, // Packed bits
		8 => 1,
		16 => 2,
		24 => 3,
		32 => 4,
		_ => throw new NotSupportedException($"Unsupported bit depth: {BitsPerPixel}")
	};

	/// <summary>Gets the row stride in bytes (including padding to 4-byte boundary).</summary>
	public int RowStride
	{
		get
		{
			var bitsPerRow = Width * BitsPerPixel;
			var bytesPerRow = (bitsPerRow + 7) / 8; // Round up to nearest byte
			return (bytesPerRow + BmpConstants.RowAlignment - 1) & ~(BmpConstants.RowAlignment - 1); // Align to 4-byte boundary
		}
	}

	/// <summary>Gets the total pixel data size in bytes.</summary>
	public uint PixelDataSize => (uint)(RowStride * AbsoluteHeight);

	/// <summary>Gets the BMP header type based on header size.</summary>
	public string HeaderType => HeaderSize switch
	{
		BmpConstants.BitmapInfoHeaderSize => "BITMAPINFOHEADER",
		BmpConstants.BitmapV4HeaderSize => "BITMAPV4HEADER",
		BmpConstants.BitmapV5HeaderSize => "BITMAPV5HEADER",
		_ => $"Unknown ({HeaderSize} bytes)"
	};
}