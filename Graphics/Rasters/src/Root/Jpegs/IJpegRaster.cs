// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Represents JPEG format specifications and properties.</summary>
public interface IJpegRaster : IRaster
{
	/// <summary>Gets the color mode of the JPEG image.</summary>
	JpegColorMode ColorMode { get; }

	/// <summary>Gets the quality level of the JPEG image (0-100).</summary>
	int Quality { get; }

	/// <summary>Gets the encoding format of the JPEG image.</summary>
	JpegEncoding Encoding { get; }

	/// <summary>Gets the metadata associated with the JPEG image.</summary>
	new JpegMetadata Metadata { get; }

	/// <summary>Gets the number of samples per pixel.</summary>
	int SamplesPerPixel { get; }

	/// <summary>Gets the bits per sample for each channel.</summary>
	int BitsPerSample { get; }

	/// <summary>Gets the chroma subsampling format.</summary>
	JpegChromaSubsampling ChromaSubsampling { get; }

	/// <summary>Gets a value indicating whether the image uses progressive encoding.</summary>
	bool IsProgressive { get; }

	/// <summary>Gets a value indicating whether the image uses optimized encoding.</summary>
	bool IsOptimized { get; }

	/// <summary>Gets the compression ratio achieved.</summary>
	double CompressionRatio { get; }
}
