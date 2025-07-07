// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Pngs;

/// <summary>Provides validation functionality for PNG raster images.</summary>
public static class PngValidator
{
	/// <summary>Validates a PNG raster image.</summary>
	/// <param name="png">The PNG raster to validate.</param>
	/// <returns>A validation result indicating if the image is valid and any errors.</returns>
	public static PngValidationResult Validate(this IPngRaster png)
	{
		ArgumentNullException.ThrowIfNull(png);

		var result = new PngValidationResult();

		png.ValidateDimensions(result);
		png.ValidateColorTypeAndBitDepth(result);
		png.ValidateCompressionSettings(result);
		png.ValidatePaletteRequirements(result);
		png.ValidateTransparencyData(result);
		png.ValidateMetadata(result);

		return result;
	}

	/// <summary>Validates the dimensions of a PNG raster image.</summary>
	/// <param name="png">The PNG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	public static void ValidateDimensions(this IPngRaster png, PngValidationResult result)
	{
		if (png.Width <= 0)
			result.AddError($"Invalid width: {png.Width}. Width must be greater than 0.");

		if (png.Height <= 0)
			result.AddError($"Invalid height: {png.Height}. Height must be greater than 0.");

		if (png.Width > PngConstants.MaxWidth)
			result.AddError($"Width exceeds maximum: {png.Width} > {PngConstants.MaxWidth}.");

		if (png.Height > PngConstants.MaxHeight)
			result.AddError($"Height exceeds maximum: {png.Height} > {PngConstants.MaxHeight}.");
	}

	/// <summary>Validates the color type and bit depth combination of a PNG raster image.</summary>
	/// <param name="png">The PNG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	public static void ValidateColorTypeAndBitDepth(this IPngRaster png, PngValidationResult result)
	{
		var allowedBitDepths = png.ColorType switch
		{
			PngColorType.Grayscale          => PngConstants.BitDepths.Grayscale,
			PngColorType.Truecolor          => PngConstants.BitDepths.Truecolor,
			PngColorType.IndexedColor       => PngConstants.BitDepths.IndexedColor,
			PngColorType.GrayscaleWithAlpha => PngConstants.BitDepths.GrayscaleWithAlpha,
			PngColorType.TruecolorWithAlpha => PngConstants.BitDepths.TruecolorWithAlpha,
			_                               => []
		};

		if (allowedBitDepths.Length == 0)
			result.AddError($"Invalid color type: {png.ColorType}.");
		else if (!allowedBitDepths.Contains(png.BitDepth))
			result.AddError($"Invalid bit depth {png.BitDepth} for color type {png.ColorType}. Allowed values: {string.Join(", ", allowedBitDepths)}.");

		// Validate color type specific properties
		switch (png.ColorType)
		{
			case PngColorType.IndexedColor when !png.UsesPalette:
				result.AddError("Indexed-color images must use a palette.");
				break;
			case PngColorType.GrayscaleWithAlpha when !png.HasAlphaChannel:
				result.AddWarning("Grayscale with alpha should have alpha channel enabled.");
				break;
			case PngColorType.TruecolorWithAlpha when !png.HasAlphaChannel:
				result.AddWarning("Truecolor with alpha should have alpha channel enabled.");
				break;
		}
	}

	/// <summary>Validates the compression settings of a PNG raster image.</summary>
	/// <param name="png">The PNG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	public static void ValidateCompressionSettings(this IPngRaster png, PngValidationResult result)
	{
		if (png.Compression != PngCompression.Deflate)
			result.AddError($"Invalid compression method: {png.Compression}. PNG only supports DEFLATE compression.");

		if (png.CompressionLevel < 0 || png.CompressionLevel > 9)
			result.AddError($"Invalid compression level: {png.CompressionLevel}. Must be between 0 and 9.");

		if (png.FilterMethod != PngFilterMethod.Standard)
			result.AddError($"Invalid filter method: {png.FilterMethod}. PNG only supports standard filter method.");
	}

	/// <summary>Validates the palette requirements of a PNG raster image.</summary>
	/// <param name="png">The PNG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	public static void ValidatePaletteRequirements(this IPngRaster png, PngValidationResult result)
	{
		if (png.ColorType == PngColorType.IndexedColor)
		{
			if (png.PaletteData.IsEmpty)
				result.AddError("Indexed-color images require palette data.");
			else
			{
				var maxPaletteEntries = 1 << png.BitDepth;
				var paletteEntries    = png.PaletteData.Length / 3; // RGB triplets

				if (png.PaletteData.Length % 3 != 0)
					result.AddError("Palette data length must be a multiple of 3 (RGB triplets).");

				if (paletteEntries > maxPaletteEntries)
					result.AddError($"Palette has too many entries: {paletteEntries}. Maximum for {png.BitDepth}-bit depth: {maxPaletteEntries}.");

				if (paletteEntries > 256)
					result.AddError($"Palette has too many entries: {paletteEntries}. Maximum allowed: 256.");
			}
		}
		else if (!png.PaletteData.IsEmpty) result.AddWarning($"Palette data is present but not required for color type {png.ColorType}.");
	}

	/// <summary>Validates the transparency data of a PNG raster image.</summary>
	/// <param name="png">The PNG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	public static void ValidateTransparencyData(this IPngRaster png, PngValidationResult result)
	{
		if (!png.TransparencyData.IsEmpty)
		{
			switch (png.ColorType)
			{
				case PngColorType.Grayscale:
					if (png.TransparencyData.Length != 2)
						result.AddError("Transparency data for grayscale images must be 2 bytes (gray value).");
					break;

				case PngColorType.Truecolor:
					if (png.TransparencyData.Length != 6)
						result.AddError("Transparency data for truecolor images must be 6 bytes (RGB values).");
					break;

				case PngColorType.IndexedColor:
					if (!png.PaletteData.IsEmpty)
					{
						var paletteEntries = png.PaletteData.Length / 3;
						if (png.TransparencyData.Length > paletteEntries)
							result.AddError($"Transparency data has more entries ({png.TransparencyData.Length}) than palette ({paletteEntries}).");
					}

					break;

				case PngColorType.GrayscaleWithAlpha:
				case PngColorType.TruecolorWithAlpha:
					result.AddWarning($"Transparency chunk is not recommended for {png.ColorType} (alpha channel already provides transparency).");
					break;
			}
		}

		// Validate HasTransparency flag consistency
		var shouldHaveTransparency = !png.TransparencyData.IsEmpty;
		if (png.HasTransparency != shouldHaveTransparency)
			result.AddWarning($"HasTransparency flag ({png.HasTransparency}) does not match actual transparency data presence ({shouldHaveTransparency}).");
	}

	/// <summary>Validates the metadata of a PNG raster image.</summary>
	/// <param name="png">The PNG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	public static void ValidateMetadata(this IPngRaster png, PngValidationResult result)
	{
		var metadataResult = png.Metadata.ValidateMetadata();

		foreach (var error in metadataResult.Errors)
			result.AddError(error);

		foreach (var warning in metadataResult.Warnings)
			result.AddWarning(warning);
	}

	/// <summary>Validates PNG file signature.</summary>
	/// <param name="data">The file data to validate.</param>
	/// <returns>True if the data has a valid PNG signature, false otherwise.</returns>
	public static bool IsValidPngSignature(ReadOnlySpan<byte> data)
	{
		if (data.Length < PngConstants.SignatureLength)
			return false;

		var signature = data[..PngConstants.SignatureLength];
		return signature.SequenceEqual(PngConstants.Signature);
	}
}
