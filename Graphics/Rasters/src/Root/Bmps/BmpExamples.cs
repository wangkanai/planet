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
		bmp.BmpMetadata.BitsPerPixel = 24;
		bmp.BmpMetadata.Compression  = BmpCompression.Rgb;
		bmp.BmpMetadata.Planes       = BmpConstants.Planes;

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
			palette[offset]     = (byte)i; // Blue
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
		palette[0] = 0; // Blue
		palette[1] = 0; // Green
		palette[2] = 0; // Red
		palette[3] = 0; // Reserved

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
		bmp.Height          = -Math.Abs(height);
		bmp.BmpMetadata.Height = bmp.Height;

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

		bmp.HorizontalResolution     = pixelsPerMeter;
		bmp.VerticalResolution       = pixelsPerMeter;
		bmp.BmpMetadata.XPixelsPerMeter = pixelsPerMeter;
		bmp.BmpMetadata.YPixelsPerMeter = pixelsPerMeter;

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
		bmp.BmpMetadata.HeaderSize     = BmpConstants.BitmapV5HeaderSize;
		bmp.BmpMetadata.ColorSpaceType = BmpConstants.ColorSpace.LCS_sRGB;
		bmp.BmpMetadata.Intent         = BmpConstants.Intent.LCS_GM_IMAGES;

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

		bmp.BmpMetadata.Compression = BmpCompression.Rle8;

		// Create a simple 256-color palette
		var palette = new byte[256 * BmpConstants.PaletteEntrySize];
		for (var i = 0; i < 256; i++)
		{
			var offset = i * BmpConstants.PaletteEntrySize;
			palette[offset]     = (byte)(i & 0xFF); // Blue
			palette[offset + 1] = (byte)(i & 0xFF); // Green
			palette[offset + 2] = (byte)(i & 0xFF); // Red
			palette[offset + 3] = 0;                // Reserved
		}

		bmp.ApplyPalette(palette);
		return bmp;
	}

	/// <summary>Creates a 4-bit indexed color BMP with 16-color palette.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured BMP raster with 16-color palette.</returns>
	public static BmpRaster Create16Color(int width, int height)
	{
		var bmp = new BmpRaster(width, height, BmpColorDepth.FourBit);

		// Create standard 16-color palette (VGA colors)
		var palette = new byte[16 * BmpConstants.PaletteEntrySize];
		var colors = new[]
		             {
			             (0, 0, 0),       // Black
			             (128, 0, 0),     // Dark Red
			             (0, 128, 0),     // Dark Green
			             (128, 128, 0),   // Dark Yellow
			             (0, 0, 128),     // Dark Blue
			             (128, 0, 128),   // Dark Magenta
			             (0, 128, 128),   // Dark Cyan
			             (192, 192, 192), // Light Gray
			             (128, 128, 128), // Dark Gray
			             (255, 0, 0),     // Red
			             (0, 255, 0),     // Green
			             (255, 255, 0),   // Yellow
			             (0, 0, 255),     // Blue
			             (255, 0, 255),   // Magenta
			             (0, 255, 255),   // Cyan
			             (255, 255, 255)  // White
		             };

		for (var i = 0; i < 16; i++)
		{
			var offset = i * BmpConstants.PaletteEntrySize;
			palette[offset]     = (byte)colors[i].Item3; // Blue
			palette[offset + 1] = (byte)colors[i].Item2; // Green
			palette[offset + 2] = (byte)colors[i].Item1; // Red
			palette[offset + 3] = 0;                     // Reserved
		}

		bmp.ApplyPalette(palette);
		return bmp;
	}

	/// <summary>Creates a BMP with custom bit masks for 16-bit format.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="redMask">Red component bit mask.</param>
	/// <param name="greenMask">Green component bit mask.</param>
	/// <param name="blueMask">Blue component bit mask.</param>
	/// <param name="alphaMask">Alpha component bit mask (optional).</param>
	/// <returns>A configured BMP raster with custom bit masks.</returns>
	public static BmpRaster CreateWithCustomMasks(int width, int height, uint redMask, uint greenMask, uint blueMask, uint alphaMask = 0)
	{
		var bmp = new BmpRaster(width, height, BmpColorDepth.SixteenBit);
		bmp.SetBitMasks(redMask, greenMask, blueMask, alphaMask);
		return bmp;
	}

	/// <summary>Creates a minimal BMP with the smallest possible configuration.</summary>
	/// <returns>A 1x1 pixel monochrome BMP.</returns>
	public static BmpRaster CreateMinimal()
	{
		return CreateMonochrome(1, 1);
	}

	/// <summary>Creates a BMP configured for web usage (sRGB color space).</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured BMP raster optimized for web usage.</returns>
	public static BmpRaster CreateForWeb(int width, int height)
	{
		var bmp = CreateRgb24(width, height);

		// Configure for sRGB and standard web resolution (96 DPI)
		bmp.BmpMetadata.HeaderSize     = BmpConstants.BitmapV4HeaderSize;
		bmp.BmpMetadata.ColorSpaceType = BmpConstants.ColorSpace.LCS_sRGB;

		// Set standard web resolution (96 DPI)
		bmp.HorizontalResolution     = BmpConstants.DefaultHorizontalResolution;
		bmp.VerticalResolution       = BmpConstants.DefaultVerticalResolution;
		bmp.BmpMetadata.XPixelsPerMeter = BmpConstants.DefaultHorizontalResolution;
		bmp.BmpMetadata.YPixelsPerMeter = BmpConstants.DefaultVerticalResolution;

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

		var color16 = Create16Color(800, 600);
		Console.WriteLine($"4-bit 16-color: {color16.Width}x{color16.Height}, {color16.GetEstimatedFileSize():N0} bytes");

		var rgb565 = CreateRgb565(800, 600);
		Console.WriteLine($"16-bit RGB565: {rgb565.Width}x{rgb565.Height}, {rgb565.GetEstimatedFileSize():N0} bytes");

		// Demonstrate specialized formats
		Console.WriteLine("\nSpecialized Formats:");
		var webBmp = CreateForWeb(800, 600);
		Console.WriteLine($"Web-optimized: {webBmp.BmpMetadata.HeaderType}, sRGB color space");

		var topDown = CreateTopDown(800, 600);
		Console.WriteLine($"Top-down format: Height={topDown.Height} (negative), IsTopDown={topDown.IsTopDown}");

		var highDpi = CreateWithResolution(800, 600, 300);
		Console.WriteLine($"300 DPI: {highDpi.HorizontalResolution} pixels/meter");

		var minimal = CreateMinimal();
		Console.WriteLine($"Minimal: {minimal.Width}x{minimal.Height}, {minimal.GetEstimatedFileSize():N0} bytes");

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

		// Demonstrate bit masks
		Console.WriteLine("\nBit Masks (RGB565):");
		var (red, green, blue, alpha) = rgb565.GetBitMasks();
		Console.WriteLine($"Red: 0x{red:X4}, Green: 0x{green:X4}, Blue: 0x{blue:X4}, Alpha: 0x{alpha:X4}");
	}
}
