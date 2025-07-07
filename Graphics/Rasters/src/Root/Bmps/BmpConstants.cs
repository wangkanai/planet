// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Collections.Immutable;

namespace Wangkanai.Graphics.Rasters.Bmps;

/// <summary>Defines constants for BMP format specifications.</summary>
public static class BmpConstants
{
	/// <summary>The BMP signature bytes ("BM").</summary>
	public static readonly ImmutableArray<byte> Signature = "BM"u8.ToImmutableArray();

	/// <summary>The size of the BMP file header in bytes.</summary>
	public const int FileHeaderSize = 14;

	/// <summary>The size of the BITMAPINFOHEADER in bytes.</summary>
	public const int BitmapInfoHeaderSize = 40;

	/// <summary>The size of the BITMAPV4HEADER in bytes.</summary>
	public const int BitmapV4HeaderSize = 108;

	/// <summary>The size of the BITMAPV5HEADER in bytes.</summary>
	public const int BitmapV5HeaderSize = 124;

	/// <summary>The minimum width for BMP images.</summary>
	public const int MinWidth = 1;

	/// <summary>The maximum width for BMP images (32-bit signed integer limit).</summary>
	public const int MaxWidth = int.MaxValue;

	/// <summary>The minimum height for BMP images.</summary>
	public const int MinHeight = 1;

	/// <summary>The maximum height for BMP images (32-bit signed integer limit).</summary>
	public const int MaxHeight = int.MaxValue;

	/// <summary>The number of planes for BMP images (always 1).</summary>
	public const ushort Planes = 1;

	/// <summary>Row padding alignment in bytes (BMP rows must be aligned to 4-byte boundaries).</summary>
	public const int RowAlignment = 4;

	/// <summary>Default horizontal resolution in pixels per meter (96 DPI).</summary>
	public const int DefaultHorizontalResolution = 3780;

	/// <summary>Default vertical resolution in pixels per meter (96 DPI).</summary>
	public const int DefaultVerticalResolution = 3780;

	/// <summary>Maximum number of colors in a palette (for 8-bit images).</summary>
	public const uint MaxPaletteColors = 256;

	/// <summary>Size of each palette entry in bytes (BGRA format).</summary>
	public const int PaletteEntrySize = 4;

	/// <summary>Compression type values.</summary>
	public static class Compression
	{
		/// <summary>Uncompressed RGB format.</summary>
		public const uint BI_RGB = 0;

		/// <summary>8-bit run-length encoding.</summary>
		public const uint BI_RLE8 = 1;

		/// <summary>4-bit run-length encoding.</summary>
		public const uint BI_RLE4 = 2;

		/// <summary>Uncompressed format with bit field masks.</summary>
		public const uint BI_BITFIELDS = 3;

		/// <summary>JPEG compression (for BMP embedded JPEG).</summary>
		public const uint BI_JPEG = 4;

		/// <summary>PNG compression (for BMP embedded PNG).</summary>
		public const uint BI_PNG = 5;
	}

	/// <summary>Color space values for V4/V5 headers.</summary>
	public static class ColorSpace
	{
		/// <summary>Calibrated RGB color space.</summary>
		public const uint LCS_CALIBRATED_RGB = 0;

		/// <summary>sRGB color space.</summary>
		public const uint LCS_sRGB = 0x73524742; // 'sRGB'

		/// <summary>Windows default color space.</summary>
		public const uint LCS_WINDOWS_COLOR_SPACE = 0x57696E20; // 'Win '
	}

	/// <summary>Rendering intent values for V5 headers.</summary>
	public static class Intent
	{
		/// <summary>Graphic intent.</summary>
		public const uint LCS_GM_BUSINESS = 1;

		/// <summary>Proof intent.</summary>
		public const uint LCS_GM_GRAPHICS = 2;

		/// <summary>Picture intent.</summary>
		public const uint LCS_GM_IMAGES = 4;

		/// <summary>Absolute colorimetric intent.</summary>
		public const uint LCS_GM_ABS_COLORIMETRIC = 8;
	}

	/// <summary>Common bit depths supported by BMP format.</summary>
	public static class BitDepth
	{
		/// <summary>1-bit monochrome.</summary>
		public const ushort Monochrome = 1;

		/// <summary>4-bit with 16-color palette.</summary>
		public const ushort FourBit = 4;

		/// <summary>8-bit with 256-color palette.</summary>
		public const ushort EightBit = 8;

		/// <summary>16-bit high color.</summary>
		public const ushort SixteenBit = 16;

		/// <summary>24-bit true color.</summary>
		public const ushort TwentyFourBit = 24;

		/// <summary>32-bit true color with alpha.</summary>
		public const ushort ThirtyTwoBit = 32;
	}

	/// <summary>Default bit field masks for 16-bit RGB555 format.</summary>
	public static class RGB555Masks
	{
		/// <summary>Red component mask (bits 10-14).</summary>
		public const uint Red = 0x7C00;

		/// <summary>Green component mask (bits 5-9).</summary>
		public const uint Green = 0x03E0;

		/// <summary>Blue component mask (bits 0-4).</summary>
		public const uint Blue = 0x001F;

		/// <summary>Alpha component mask (not used in RGB555).</summary>
		public const uint Alpha = 0x0000;
	}

	/// <summary>Default bit field masks for 16-bit RGB565 format.</summary>
	public static class RGB565Masks
	{
		/// <summary>Red component mask (bits 11-15).</summary>
		public const uint Red = 0xF800;

		/// <summary>Green component mask (bits 5-10).</summary>
		public const uint Green = 0x07E0;

		/// <summary>Blue component mask (bits 0-4).</summary>
		public const uint Blue = 0x001F;

		/// <summary>Alpha component mask (not used in RGB565).</summary>
		public const uint Alpha = 0x0000;
	}

	/// <summary>Default bit field masks for 32-bit ARGB8888 format.</summary>
	public static class ARGB8888Masks
	{
		/// <summary>Red component mask (bits 16-23).</summary>
		public const uint Red = 0x00FF0000;

		/// <summary>Green component mask (bits 8-15).</summary>
		public const uint Green = 0x0000FF00;

		/// <summary>Blue component mask (bits 0-7).</summary>
		public const uint Blue = 0x000000FF;

		/// <summary>Alpha component mask (bits 24-31).</summary>
		public const uint Alpha = 0xFF000000;
	}

	/// <summary>RLE escape codes for compressed formats.</summary>
	public static class RLE
	{
		/// <summary>End of line marker.</summary>
		public const byte EndOfLine = 0x00;

		/// <summary>End of bitmap marker.</summary>
		public const byte EndOfBitmap = 0x01;

		/// <summary>Delta marker (move cursor).</summary>
		public const byte Delta = 0x02;
	}
}
