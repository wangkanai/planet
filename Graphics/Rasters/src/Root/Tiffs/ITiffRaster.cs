// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Represents TIFF format specifications and properties.</summary>
public interface ITiffRaster : IRaster
{
	/// <summary>Gets the color depth of the TIFF image.</summary>
	TiffColorDepth ColorDepth { get; }

	/// <summary>Gets the compression algorithm used in the TIFF image.</summary>
	TiffCompression Compression { get; }

	/// <summary>Gets the metadata associated with the TIFF image.</summary>
	new TiffMetadata Metadata { get; }

	/// <summary>Gets the number of samples per pixel.</summary>
	int SamplesPerPixel { get; }

	/// <summary>
	/// Gets the bits per sample for each channel as a read-only span.
	/// Uses inline storage for 1-4 samples for optimal performance.
	/// </summary>
	ReadOnlySpan<int> BitsPerSample { get; }

	/// <summary>Gets the photometric interpretation of the image data.</summary>
	PhotometricInterpretation PhotometricInterpretation { get; }

	/// <summary>Gets a value indicating whether the image has transparency/alpha channel.</summary>
	bool HasAlpha { get; }

	/// <summary>Gets the planar configuration (1 = chunky, 2 = planar).</summary>
	int PlanarConfiguration { get; }
}
