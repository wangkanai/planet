// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.WebPs;

/// <summary>Represents WebP format specifications and properties.</summary>
public interface IWebPRaster : IRaster
{
	/// <summary>Gets the WebP format type.</summary>
	WebPFormat Format { get; }

	/// <summary>Gets the compression type used.</summary>
	WebPCompression Compression { get; }

	/// <summary>Gets the color mode of the WebP image.</summary>
	WebPColorMode ColorMode { get; }

	/// <summary>Gets the quality level for lossy compression (0-100).</summary>
	int Quality { get; }

	/// <summary>Gets the compression level for lossless compression (0-9).</summary>
	int CompressionLevel { get; }

	/// <summary>Gets the encoding preset used for optimization.</summary>
	WebPPreset Preset { get; }

	/// <summary>Gets the metadata associated with the WebP image.</summary>
	new WebPMetadata Metadata { get; }

	/// <summary>Gets the number of color channels.</summary>
	int Channels { get; }

	/// <summary>Gets a value indicating whether the image has an alpha channel.</summary>
	bool HasAlpha { get; }

	/// <summary>Gets a value indicating whether the image uses lossless compression.</summary>
	bool IsLossless { get; }

	/// <summary>Gets a value indicating whether the image is animated.</summary>
	bool IsAnimated { get; }

	/// <summary>Gets the estimated compression ratio achieved.</summary>
	double CompressionRatio { get; }

	/// <summary>Gets the estimated file size in bytes.</summary>
	long GetEstimatedFileSize();

	/// <summary>Validates the WebP raster image.</summary>
	/// <returns>True if the image is valid, false otherwise.</returns>
	bool IsValid();
}
