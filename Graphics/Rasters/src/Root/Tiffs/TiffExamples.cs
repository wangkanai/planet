// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Provides examples of using TIFF format specifications.</summary>
public static class TiffExamples
{
	private static readonly int[] BitsPerSampleSingle = [16];
	private static readonly int[] BitsPerSampleArray  = [8, 8, 8, 8];

	/// <summary>Creates a basic RGB TIFF raster with metadata.</summary>
	public static ITiffRaster CreateBasicRgbTiff()
	{
		var tiff = new TiffRaster(1024, 768)
		           {
			           ColorDepth                = TiffColorDepth.TwentyFourBit,
			           Compression               = TiffCompression.Lzw,
			           PhotometricInterpretation = PhotometricInterpretation.Rgb,
			           SamplesPerPixel           = 3,
			           HasAlpha                  = false
		           };
		
		tiff.TiffMetadata.ImageDescription = "Sample RGB Image";
		tiff.TiffMetadata.Software         = "Wangkanai Planet";
		tiff.TiffMetadata.DateTime         = DateTime.UtcNow;
		tiff.TiffMetadata.XResolution      = 300.0;
		tiff.TiffMetadata.YResolution      = 300.0;
		tiff.TiffMetadata.ResolutionUnit   = 2; // inches
		
		tiff.SetBitsPerSample(BitsPerSampleArray);

		return tiff;
	}

	/// <summary>Creates a high-bit depth grayscale TIFF.</summary>
	public static ITiffRaster CreateGrayscaleTiff()
	{
		var tiff = new TiffRaster(2048, 1536)
		           {
			           ColorDepth                = TiffColorDepth.SixteenBit,
			           Compression               = TiffCompression.None,
			           PhotometricInterpretation = PhotometricInterpretation.BlackIsZero,
			           SamplesPerPixel           = 1,
			           HasAlpha                  = false
		           };
		
		tiff.TiffMetadata.ImageDescription = "High-precision grayscale image";
		tiff.TiffMetadata.Make             = "Professional Camera";
		tiff.TiffMetadata.Model            = "Model X";
		
		tiff.SetBitsPerSample(BitsPerSampleSingle);

		return tiff;
	}

	/// <summary>Creates a CMYK print-ready TIFF.</summary>
	public static ITiffRaster CreateCmykPrintTiff()
	{
		var tiff = new TiffRaster(3000, 2000)
		           {
			           ColorDepth                = TiffColorDepth.ThirtyTwoBit,
			           Compression               = TiffCompression.PackBits,
			           PhotometricInterpretation = PhotometricInterpretation.Cmyk,
			           SamplesPerPixel           = 4,
			           HasAlpha                  = false
		           };
		
		tiff.TiffMetadata.ImageDescription = "Print-ready CMYK image";
		tiff.TiffMetadata.Software         = "Professional Publishing Suite";
		tiff.TiffMetadata.Copyright        = "© 2025 Company Name";
		
		tiff.SetBitsPerSample(BitsPerSampleArray);

		return tiff;
	}
}
