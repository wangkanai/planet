// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

/// <summary>Provides validation functionality for BMP raster images.</summary>
public static class BmpValidator
{
	/// <summary>Validates a BMP raster image and returns detailed validation results.</summary>
	/// <param name="bmp">The BMP raster to validate.</param>
	/// <returns>A validation result containing any errors or warnings.</returns>
	public static BmpValidationResult Validate(IBmpRaster bmp)
	{
		var result = new BmpValidationResult();

		ValidateDimensions(bmp, result);
		ValidateColorDepth(bmp, result);
		ValidateCompression(bmp, result);
		ValidatePalette(bmp, result);
		ValidateBitMasks(bmp, result);
		ValidateMetadata(bmp, result);
		ValidateFileStructure(bmp, result);

		return result;
	}

	/// <summary>Validates the dimensions of the BMP image.</summary>
	/// <param name="bmp">The BMP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateDimensions(IBmpRaster bmp, BmpValidationResult result)
	{
		if (bmp.Width <= 0)
			result.AddError($"Invalid width: {bmp.Width}. Width must be greater than 0.");

		if (bmp.Height == 0)
			result.AddError($"Invalid height: {bmp.Height}. Height cannot be 0.");

		if (bmp.Width > BmpConstants.MaxWidth)
			result.AddError($"Width exceeds maximum: {bmp.Width} > {BmpConstants.MaxWidth}.");

		if (Math.Abs(bmp.Height) > BmpConstants.MaxHeight)
			result.AddError($"Height exceeds maximum: {Math.Abs(bmp.Height)} > {BmpConstants.MaxHeight}.");

		// Check for extremely large images that could cause memory issues
		var totalPixels = (long)bmp.Width * Math.Abs(bmp.Height);
		if (totalPixels > int.MaxValue)
			result.AddWarning($"Very large image: {totalPixels:N0} pixels. This may cause memory issues.");
	}

	/// <summary>Validates the color depth and related properties.</summary>
	/// <param name="bmp">The BMP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateColorDepth(IBmpRaster bmp, BmpValidationResult result)
	{
		if (!Enum.IsDefined(typeof(BmpColorDepth), bmp.ColorDepth))
		{
			result.AddError($"Unsupported color depth: {(int)bmp.ColorDepth} bits per pixel.");
			return;
		}

		// Validate bits per pixel consistency
		if (bmp.Metadata.BitsPerPixel != (ushort)bmp.ColorDepth)
			result.AddError($"Color depth mismatch: Property={bmp.ColorDepth}, Metadata={bmp.Metadata.BitsPerPixel}.");

		// Validate bytes per pixel calculation
		try
		{
			_ = bmp.BytesPerPixel;
		}
		catch (NotSupportedException ex)
		{
			result.AddError($"Invalid color depth configuration: {ex.Message}");
		}
	}

	/// <summary>Validates the compression method and compatibility.</summary>
	/// <param name="bmp">The BMP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateCompression(IBmpRaster bmp, BmpValidationResult result)
	{
		if (!Enum.IsDefined(typeof(BmpCompression), bmp.Compression))
		{
			result.AddError($"Unsupported compression method: {(uint)bmp.Compression}.");
			return;
		}

		// Validate compression and color depth compatibility
		switch (bmp.Compression)
		{
			case BmpCompression.Rle4:
				if (bmp.ColorDepth != BmpColorDepth.FourBit)
					result.AddError("RLE4 compression is only valid for 4-bit images.");
				break;

			case BmpCompression.Rle8:
				if (bmp.ColorDepth != BmpColorDepth.EightBit)
					result.AddError("RLE8 compression is only valid for 8-bit images.");
				break;

			case BmpCompression.BitFields:
				if (bmp.ColorDepth != BmpColorDepth.SixteenBit && bmp.ColorDepth != BmpColorDepth.ThirtyTwoBit)
					result.AddError("BI_BITFIELDS compression is only valid for 16-bit and 32-bit images.");
				break;

			case BmpCompression.Jpeg:
			case BmpCompression.Png:
				result.AddWarning($"{bmp.Compression} compression in BMP files is rarely supported and may cause compatibility issues.");
				break;
		}

		// Validate compression consistency with metadata
		if (bmp.Metadata.Compression != bmp.Compression)
			result.AddError($"Compression mismatch: Property={bmp.Compression}, Metadata={bmp.Metadata.Compression}.");
	}

	/// <summary>Validates color palette configuration.</summary>
	/// <param name="bmp">The BMP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidatePalette(IBmpRaster bmp, BmpValidationResult result)
	{
		var requiresPalette = bmp.HasPalette;
		var hasPalette      = bmp.ColorPalette != null && bmp.ColorPalette.Length > 0;

		if (requiresPalette && !hasPalette && bmp.Metadata.ColorsUsed > 0)
			result.AddError($"Palette required for {bmp.ColorDepth}-bit image but not provided.");

		if (!requiresPalette && hasPalette)
			result.AddWarning($"Palette provided for {bmp.ColorDepth}-bit image but not required.");

		if (hasPalette)
		{
			var palette = bmp.ColorPalette!;

			// Validate palette size alignment
			if (palette.Length % BmpConstants.PaletteEntrySize != 0)
				result.AddError($"Invalid palette size: {palette.Length} bytes. Must be multiple of {BmpConstants.PaletteEntrySize}.");

			// Validate number of colors
			var providedColors = palette.Length / BmpConstants.PaletteEntrySize;
			var maxColors      = bmp.Metadata.PaletteColors;

			if (providedColors > maxColors)
				result.AddError($"Too many palette colors: {providedColors} provided, maximum {maxColors} for {bmp.ColorDepth}-bit.");

			// Validate colors used consistency
			if (bmp.Metadata.ColorsUsed > 0 && bmp.Metadata.ColorsUsed != providedColors)
				result.AddWarning($"ColorsUsed mismatch: Metadata={bmp.Metadata.ColorsUsed}, Palette={providedColors}.");
		}
	}

	/// <summary>Validates bit field masks for custom color formats.</summary>
	/// <param name="bmp">The BMP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateBitMasks(IBmpRaster bmp, BmpValidationResult result)
	{
		if (bmp.Compression == BmpCompression.BitFields)
		{
			var (red, green, blue, alpha) = bmp.GetBitMasks();

			// Check that masks are defined
			if (red == 0 && green == 0 && blue == 0)
				result.AddError("BI_BITFIELDS compression requires non-zero color masks.");

			// Check for overlapping masks
			if ((red & green) != 0 || (red & blue) != 0 || (green & blue) != 0)
				result.AddError("Color bit masks cannot overlap.");

			// Check alpha mask consistency
			if (bmp.ColorDepth == BmpColorDepth.ThirtyTwoBit && alpha == 0)
				result.AddWarning("32-bit image with BI_BITFIELDS typically includes an alpha mask.");

			if (bmp.ColorDepth == BmpColorDepth.SixteenBit && alpha != 0)
				result.AddWarning("16-bit image with alpha mask is uncommon and may cause compatibility issues.");
		}
		else
		{
			// For non-bitfield compression, masks should be zero or default
			var (red, green, blue, alpha) = bmp.GetBitMasks();
			if ((red != 0 || green != 0 || blue != 0 || alpha != 0) && bmp.Compression != BmpCompression.Rgb) result.AddWarning($"Bit masks defined for {bmp.Compression} compression may be ignored.");
		}
	}

	/// <summary>Validates BMP metadata consistency and structure.</summary>
	/// <param name="bmp">The BMP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateMetadata(IBmpRaster bmp, BmpValidationResult result)
	{
		var metadata = bmp.Metadata;

		// Validate file signature
		if (metadata.FileSignature != "BM")
			result.AddError($"Invalid BMP signature: '{metadata.FileSignature}'. Expected 'BM'.");

		// Validate header size
		if (metadata.HeaderSize != BmpConstants.BitmapInfoHeaderSize &&
		    metadata.HeaderSize != BmpConstants.BitmapV4HeaderSize &&
		    metadata.HeaderSize != BmpConstants.BitmapV5HeaderSize)
			result.AddError($"Unsupported header size: {metadata.HeaderSize} bytes.");

		// Validate dimensions consistency
		if (metadata.Width != bmp.Width)
			result.AddError($"Width mismatch: Property={bmp.Width}, Metadata={metadata.Width}.");

		if (metadata.Height != bmp.Height)
			result.AddError($"Height mismatch: Property={bmp.Height}, Metadata={metadata.Height}.");

		// Validate planes (should always be 1)
		if (metadata.Planes != BmpConstants.Planes)
			result.AddError($"Invalid planes value: {metadata.Planes}. BMP files must have exactly 1 plane.");

		// Validate resolution values
		if (metadata.XPixelsPerMeter < 0 || metadata.YPixelsPerMeter < 0)
			result.AddError("Resolution values cannot be negative.");

		// Validate pixel data size
		var expectedPixelSize = bmp.PixelDataSize;
		if (metadata.ImageSize != 0 && metadata.ImageSize != expectedPixelSize)
		{
			if (bmp.Compression == BmpCompression.Rgb)
				result.AddWarning($"ImageSize mismatch: Expected={expectedPixelSize}, Metadata={metadata.ImageSize}. For uncompressed images, ImageSize can be 0.");
			else
				result.AddError($"ImageSize mismatch for compressed image: Expected≤{expectedPixelSize}, Metadata={metadata.ImageSize}.");
		}
	}

	/// <summary>Validates overall file structure and constraints.</summary>
	/// <param name="bmp">The BMP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateFileStructure(IBmpRaster bmp, BmpValidationResult result)
	{
		try
		{
			// Validate calculated properties don't throw exceptions
			_ = bmp.RowStride;
			_ = bmp.PixelDataSize;
			var estimatedSize = bmp.GetEstimatedFileSize();

			// Check for reasonable file size
			if (estimatedSize > int.MaxValue)
				result.AddWarning($"Very large estimated file size: {estimatedSize:N0} bytes. This may cause issues with some applications.");

			// Validate pixel data offset would be reasonable
			var minOffset = (uint)(BmpConstants.FileHeaderSize + bmp.Metadata.HeaderSize);
			if (bmp.HasPalette)
				minOffset += bmp.Metadata.PaletteSizeInBytes;

			if (bmp.Metadata.PixelDataOffset != 0 && bmp.Metadata.PixelDataOffset < minOffset)
				result.AddError($"Invalid pixel data offset: {bmp.Metadata.PixelDataOffset}. Minimum required: {minOffset}.");
		}
		catch (Exception ex) when (ex is NotSupportedException || ex is OverflowException)
		{
			result.AddError($"File structure validation failed: {ex.Message}");
		}
	}

	/// <summary>Validates a BMP file signature from raw data.</summary>
	/// <param name="data">The file data to validate.</param>
	/// <returns>True if the data starts with a valid BMP signature, false otherwise.</returns>
	public static bool IsValidBmpSignature(ReadOnlySpan<byte> data)
	{
		if (data.Length < BmpConstants.Signature.Length)
			return false;

		return data[..BmpConstants.Signature.Length].SequenceEqual(BmpConstants.Signature.AsSpan());
	}

	/// <summary>Detects the BMP header type from file data.</summary>
	/// <param name="data">The file data to analyze.</param>
	/// <returns>The detected header size, or 0 if invalid.</returns>
	public static uint DetectHeaderType(ReadOnlySpan<byte> data)
	{
		if (!IsValidBmpSignature(data))
			return 0;

		if (data.Length < BmpConstants.FileHeaderSize + 4)
			return 0;

		// Read header size from DIB header (4 bytes after file header)
		var headerSizeBytes = data.Slice(BmpConstants.FileHeaderSize, 4);
		var headerSize      = BitConverter.ToUInt32(headerSizeBytes);

		return headerSize switch
		{
			BmpConstants.BitmapInfoHeaderSize => BmpConstants.BitmapInfoHeaderSize,
			BmpConstants.BitmapV4HeaderSize   => BmpConstants.BitmapV4HeaderSize,
			BmpConstants.BitmapV5HeaderSize   => BmpConstants.BitmapV5HeaderSize,
			_                                 => 0 // Unknown or unsupported header type
		};
	}
}
