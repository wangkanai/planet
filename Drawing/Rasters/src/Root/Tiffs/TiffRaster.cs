// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Drawing.Rasters.Tiffs;

/// <summary>Represents a TIFF raster image with format-specific properties.</summary>
public class TiffRaster : ITiffRaster
{
	/// <inheritdoc />
	public int Width { get; set; }
	
	/// <inheritdoc />
	public int Height { get; set; }
	
	/// <inheritdoc />
	public TiffColorDepth ColorDepth { get; set; }
	
	/// <inheritdoc />
	public TiffCompression Compression { get; set; }
	
	/// <inheritdoc />
	public TiffMetadata Metadata { get; set; } = new();
	
	/// <inheritdoc />
	public int SamplesPerPixel { get; set; }
	
	/// <inheritdoc />
	public int[] BitsPerSample { get; set; } = Array.Empty<int>();
	
	/// <inheritdoc />
	public PhotometricInterpretation PhotometricInterpretation { get; set; }
	
	/// <inheritdoc />
	public bool HasAlpha { get; set; }
	
	/// <inheritdoc />
	public int PlanarConfiguration { get; set; } = 1;
	
	/// <summary>Initializes a new instance of the <see cref="TiffRaster"/> class.</summary>
	public TiffRaster()
	{
		ColorDepth = TiffColorDepth.TwentyFourBit;
		Compression = TiffCompression.None;
		PhotometricInterpretation = PhotometricInterpretation.Rgb;
		SamplesPerPixel = 3;
		BitsPerSample = new[] { 8, 8, 8 };
	}
	
	/// <summary>Initializes a new instance of the <see cref="TiffRaster"/> class with specified dimensions.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	public TiffRaster(int width, int height) : this()
	{
		Width = width;
		Height = height;
	}
	
	/// <inheritdoc />
	public void Dispose()
	{
		// Implementation for resource cleanup
		GC.SuppressFinalize(this);
	}
}