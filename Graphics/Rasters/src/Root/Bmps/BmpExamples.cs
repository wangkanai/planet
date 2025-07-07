// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

/// <summary>Provides example usage patterns and common scenarios for BMP raster images.</summary>
public static class BmpExamples
{
	/// <summary>Creates a simple 24-bit RGB BMP image.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured BMP raster for 24-bit RGB.</returns>
	public static BmpRaster CreateRgb24(int width, int height)
	{
		var bmp = new BmpRaster(width, height, BmpColorDepth.TwentyFourBit)
		{
			Compression = BmpCompression.Rgb
		};

		// Set up metadata
		bmp.Metadata.BitsPerPixel = 24;
		bmp.Metadata.Compression = BmpCompression.Rgb;
		bmp.Metadata.Planes = BmpConstants.Planes;
		
		return bmp;
	}

	/// <summary>Creates a 32-bit ARGB BMP image with alpha channel.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured BMP raster for 32-bit ARGB.</returns>
	public static BmpRaster CreateArgb32(int width, int height)
	{
		var bmp = new BmpRaster(width, height, BmpColorDepth.ThirtyTwoBit)
		{
			Compression = BmpCompression.BitFields
		};

		// Set ARGB bit masks
		bmp.SetBitMasks(
			BmpConstants.ARGB8888Masks.Red,
			BmpConstants.ARGB8888Masks.Green,
			BmpConstants.ARGB8888Masks.Blue,
			BmpConstants.ARGB8888Masks.Alpha
		);

		return bmp;
	}

	/// <summary>Creates an 8-bit indexed color BMP with a grayscale palette.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured BMP raster with grayscale palette.</returns>
	public static BmpRaster CreateGrayscale8(int width, int height)
	{
		var bmp = new BmpRaster(width, height, BmpColorDepth.EightBit);

		// Create grayscale palette (256 shades of gray)
		var palette = new byte[256 * BmpConstants.PaletteEntrySize];
		for (var i = 0; i < 256; i++)
		{
			var offset = i * BmpConstants.PaletteEntrySize;
			palette[offset] = (byte)i;     // Blue
			palette[offset + 1] = (byte)i; // Green
			palette[offset + 2] = (byte)i; // Red
			palette[offset + 3] = 0;       // Reserved (Alpha)
		}

		bmp.ApplyPalette(palette);
		return bmp;
	}

	/// <summary>Creates a 1-bit monochrome BMP image.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured BMP raster for monochrome.</returns>
	public static BmpRaster CreateMonochrome(int width, int height)
	{
		var bmp = new BmpRaster(width, height, BmpColorDepth.Monochrome);

		// Create black and white palette
		var palette = new byte[2 * BmpConstants.PaletteEntrySize];
		
		// Black (index 0)
		palette[0] = 0;   // Blue
		palette[1] = 0;   // Green
		palette[2] = 0;   // Red
		palette[3] = 0;   // Reserved

		// White (index 1)
		palette[4] = 255; // Blue
		palette[5] = 255; // Green
		palette[6] = 255; // Red
		palette[7] = 0;   // Reserved

		bmp.ApplyPalette(palette);
		return bmp;
	}

	/// <summary>Creates a 16-bit RGB565 BMP image.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured BMP raster for RGB565.</returns>
	public static BmpRaster CreateRgb565(int width, int height)
	{
		var bmp = new BmpRaster(width, height, BmpColorDepth.SixteenBit);

		// Set RGB565 bit masks
		bmp.SetBitMasks(
			BmpConstants.RGB565Masks.Red,
			BmpConstants.RGB565Masks.Green,
			BmpConstants.RGB565Masks.Blue
		);

		return bmp;
	}

	/// <summary>Creates a top-down BMP image (negative height).</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image (will be made negative).</param>
	/// <returns>A configured top-down BMP raster.</returns>
	public static BmpRaster CreateTopDown(int width, int height)
	{
		var bmp = CreateRgb24(width, height);
		
		// Make height negative for top-down format
		bmp.Height = -Math.Abs(height);
		bmp.Metadata.Height = bmp.Height;

		return bmp;
	}

	/// <summary>Creates a BMP with custom resolution settings.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="dpi">The resolution in dots per inch.</param>
	/// <returns>A configured BMP raster with custom resolution.</returns>
	public static BmpRaster CreateWithResolution(int width, int height, int dpi)
	{
		var bmp = CreateRgb24(width, height);

		// Convert DPI to pixels per meter (1 inch = 0.0254 meters)
		var pixelsPerMeter = (int)(dpi / 0.0254);
		
		bmp.HorizontalResolution = pixelsPerMeter;
		bmp.VerticalResolution = pixelsPerMeter;
		bmp.Metadata.XPixelsPerMeter = pixelsPerMeter;
		bmp.Metadata.YPixelsPerMeter = pixelsPerMeter;

		return bmp;
	}

	/// <summary>Creates a BMP with V5 header for ICC profile support.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured BMP raster with V5 header.</returns>
	public static BmpRaster CreateWithV5Header(int width, int height)
	{
		var bmp = CreateRgb24(width, height);

		// Configure for V5 header
		bmp.Metadata.HeaderSize = BmpConstants.BitmapV5HeaderSize;
		bmp.Metadata.ColorSpaceType = BmpConstants.ColorSpace.LCS_sRGB;
		bmp.Metadata.Intent = BmpConstants.Intent.LCS_GM_IMAGES;

		return bmp;
	}

	/// <summary>Creates a BMP with RLE8 compression.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured BMP raster with RLE8 compression.</returns>
	public static BmpRaster CreateRle8(int width, int height)
	{
		var bmp = new BmpRaster(width, height, BmpColorDepth.EightBit)
		{
			Compression = BmpCompression.Rle8
		};

		bmp.Metadata.Compression = BmpCompression.Rle8;

		// Create a simple 256-color palette
		var palette = new byte[256 * BmpConstants.PaletteEntrySize];
		for (var i = 0; i < 256; i++)
		{
			var offset = i * BmpConstants.PaletteEntrySize;
			palette[offset] = (byte)(i & 0xFF);         // Blue
			palette[offset + 1] = (byte)((i >> 8) & 0xFF); // Green
			palette[offset + 2] = (byte)((i >> 16) & 0xFF); // Red
			palette[offset + 3] = 0;                    // Reserved
		}

		bmp.ApplyPalette(palette);
		return bmp;
	}

	/// <summary>Demonstrates validation of a BMP image.</summary>
	/// <param name="bmp">The BMP raster to validate.</param>
	/// <returns>The validation result with any errors or warnings.</returns>
	public static BmpValidationResult ValidateExample(BmpRaster bmp)
	{
		// Perform comprehensive validation
		var result = BmpValidator.Validate(bmp);

		// Display results
		Console.WriteLine($"BMP Validation: {result.GetSummary()}");
		
		if (!result.IsValid)
		{
			Console.WriteLine("Errors:");
			foreach (var error in result.Errors)
				Console.WriteLine($"  - {error}");
		}

		if (result.HasWarnings)
		{
			Console.WriteLine("Warnings:");
			foreach (var warning in result.Warnings)
				Console.WriteLine($"  - {warning}");
		}

		return result;
	}

	/// <summary>Demonstrates common BMP operations.</summary>
	public static void DemonstrateOperations()
	{
		Console.WriteLine("BMP Raster Examples");
		Console.WriteLine("==================");

		// Create various BMP formats
		var rgb24 = CreateRgb24(800, 600);
		Console.WriteLine($"24-bit RGB: {rgb24.Width}x{rgb24.Height}, {rgb24.GetEstimatedFileSize():N0} bytes");

		var argb32 = CreateArgb32(800, 600);
		Console.WriteLine($"32-bit ARGB: {argb32.Width}x{argb32.Height}, {argb32.GetEstimatedFileSize():N0} bytes");

		var grayscale = CreateGrayscale8(800, 600);
		Console.WriteLine($"8-bit Grayscale: {grayscale.Width}x{grayscale.Height}, {grayscale.GetEstimatedFileSize():N0} bytes");

		var monochrome = CreateMonochrome(800, 600);
		Console.WriteLine($"1-bit Monochrome: {monochrome.Width}x{monochrome.Height}, {monochrome.GetEstimatedFileSize():N0} bytes");

		// Demonstrate format conversion
		Console.WriteLine("\nFormat Conversion:");
		Console.WriteLine($"Before: {argb32.ColorDepth}, {argb32.Compression}");
		argb32.ConvertToRgb();
		Console.WriteLine($"After:  {argb32.ColorDepth}, {argb32.Compression}");

		// Demonstrate validation
		Console.WriteLine("\nValidation Example:");
		ValidateExample(rgb24);

		// Demonstrate properties
		Console.WriteLine("\nProperties:");
		Console.WriteLine($"Row Stride: {rgb24.RowStride} bytes");
		Console.WriteLine($"Pixel Data Size: {rgb24.PixelDataSize:N0} bytes");
		Console.WriteLine($"Has Palette: {rgb24.HasPalette}");
		Console.WriteLine($"Has Transparency: {rgb24.HasTransparency}");
	}
}