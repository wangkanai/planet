// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Provides validation for TIFF format specifications.</summary>
public static class TiffValidator
{
	/// <summary>Validates that the color depth is supported by TIFF format.</summary>
	/// <param name="colorDepth">The color depth to validate.</param>
	/// <returns>True if the color depth is valid, otherwise false.</returns>
	public static bool IsValidColorDepth(TiffColorDepth colorDepth)
	{
		return Enum.IsDefined<TiffColorDepth>(colorDepth);
	}

	/// <summary>Validates that the compression algorithm is supported by TIFF format.</summary>
	/// <param name="compression">The compression algorithm to validate.</param>
	/// <returns>True if the compression is valid, otherwise false.</returns>
	public static bool IsValidCompression(TiffCompression compression)
	{
		return Enum.IsDefined<TiffCompression>(compression);
	}

	/// <summary>Validates that the photometric interpretation is valid for the given color depth.</summary>
	/// <param name="photometric">The photometric interpretation.</param>
	/// <param name="colorDepth">The color depth.</param>
	/// <returns>True if the combination is valid, otherwise false.</returns>
	public static bool IsValidPhotometricInterpretation(PhotometricInterpretation photometric, TiffColorDepth colorDepth)
	{
		return photometric switch
		{
			PhotometricInterpretation.WhiteIsZero or PhotometricInterpretation.BlackIsZero => colorDepth is TiffColorDepth.Bilevel or TiffColorDepth.FourBit or TiffColorDepth.EightBit or TiffColorDepth.SixteenBit,
			PhotometricInterpretation.Rgb                                                  => colorDepth is TiffColorDepth.TwentyFourBit or TiffColorDepth.ThirtyTwoBit or TiffColorDepth.FortyEightBit or TiffColorDepth.SixtyFourBit,
			PhotometricInterpretation.Palette                                              => colorDepth is TiffColorDepth.Bilevel or TiffColorDepth.FourBit or TiffColorDepth.EightBit,
			PhotometricInterpretation.Cmyk                                                 => colorDepth is TiffColorDepth.ThirtyTwoBit,
			_                                                                              => true // Allow other combinations for flexibility
		};
	}

	/// <summary>Validates the samples per pixel against the color depth and photometric interpretation.</summary>
	/// <param name="samplesPerPixel">The number of samples per pixel.</param>
	/// <param name="photometric">The photometric interpretation.</param>
	/// <param name="hasAlpha">Whether the image has an alpha channel.</param>
	/// <returns>True if the samples per pixel is valid, otherwise false.</returns>
	public static bool IsValidSamplesPerPixel(int samplesPerPixel, PhotometricInterpretation photometric, bool hasAlpha)
	{
		var expectedSamples = photometric switch
		{
			PhotometricInterpretation.WhiteIsZero or PhotometricInterpretation.BlackIsZero => 1,
			PhotometricInterpretation.Rgb                                                  => hasAlpha ? 4 : 3,
			PhotometricInterpretation.Palette                                              => 1,
			PhotometricInterpretation.Cmyk                                                 => hasAlpha ? 5 : 4,
			PhotometricInterpretation.YCbCr                                                => hasAlpha ? 4 : 3,
			_                                                                              => samplesPerPixel // Allow flexibility for other formats
		};

		return samplesPerPixel == expectedSamples;
	}

	/// <summary>Validates the bits per sample span.</summary>
	/// <param name="bitsPerSample">The bits per sample span.</param>
	/// <param name="samplesPerPixel">The number of samples per pixel.</param>
	/// <returns>True if the bits per sample is valid, otherwise false.</returns>
	public static bool IsValidBitsPerSample(ReadOnlySpan<int> bitsPerSample, int samplesPerPixel)
	{
		if (bitsPerSample.Length != samplesPerPixel)
			return false;

		// All values should be valid bit depths
		foreach (var bits in bitsPerSample)
			if (bits is not (1 or 4 or 8 or 16 or 32))
				return false;

		return true;
	}

	/// <summary>Validates a complete TIFF raster configuration.</summary>
	/// <param name="tiffRaster">The TIFF raster to validate.</param>
	/// <returns>True if the configuration is valid, otherwise false.</returns>
	public static bool IsValid(ITiffRaster tiffRaster)
	{
		if (tiffRaster.Width <= 0 || tiffRaster.Height <= 0)
			return false;

		if (!IsValidColorDepth(tiffRaster.ColorDepth))
			return false;

		if (!IsValidCompression(tiffRaster.Compression))
			return false;

		if (!IsValidPhotometricInterpretation(tiffRaster.PhotometricInterpretation, tiffRaster.ColorDepth))
			return false;

		if (!IsValidSamplesPerPixel(tiffRaster.SamplesPerPixel, tiffRaster.PhotometricInterpretation, tiffRaster.HasAlpha))
			return false;

		if (!IsValidBitsPerSample(tiffRaster.BitsPerSample, tiffRaster.SamplesPerPixel))
			return false;

		return true;
	}
}
