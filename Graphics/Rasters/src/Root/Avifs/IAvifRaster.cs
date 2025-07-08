// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;

namespace Wangkanai.Graphics.Rasters.Avifs;

/// <summary>
/// Represents an AVIF (AV1 Image File Format) raster image with advanced compression and HDR capabilities.
/// </summary>
public interface IAvifRaster : IRaster
{
	/// <summary>Gets or sets the color space used by the AVIF image.</summary>
	AvifColorSpace ColorSpace { get; set; }

	/// <summary>Gets or sets the quality level for encoding (0-100).</summary>
	int Quality { get; set; }

	/// <summary>Gets or sets the comprehensive metadata for the AVIF image.</summary>
	new AvifMetadata Metadata { get; set; }

	/// <summary>Gets or sets the bit depth per channel (8, 10, or 12).</summary>
	int BitDepth { get; set; }

	/// <summary>Gets or sets whether the image has an alpha channel.</summary>
	bool HasAlpha { get; set; }

	/// <summary>Gets or sets the chroma subsampling mode.</summary>
	AvifChromaSubsampling ChromaSubsampling { get; set; }

	/// <summary>Gets or sets the encoder speed setting (0-10, where 0 is slowest/best quality).</summary>
	int Speed { get; set; }

	/// <summary>Gets or sets whether lossless compression is used.</summary>
	bool IsLossless { get; set; }

	/// <summary>Gets whether the image contains HDR metadata.</summary>
	bool HasHdrMetadata { get; }

	/// <summary>Gets the number of threads to use for encoding/decoding (0 = auto).</summary>
	int ThreadCount { get; set; }

	/// <summary>Gets or sets whether to enable film grain synthesis.</summary>
	bool EnableFilmGrain { get; set; }

	/// <summary>
	/// Encodes the raster image to AVIF format asynchronously.
	/// </summary>
	/// <param name="options">Optional encoding options.</param>
	/// <returns>The encoded AVIF data.</returns>
	Task<byte[]> EncodeAsync(AvifEncodingOptions? options = null);

	/// <summary>
	/// Decodes AVIF data into the raster image asynchronously.
	/// </summary>
	/// <param name="data">The AVIF file data to decode.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task DecodeAsync(byte[] data);

	/// <summary>
	/// Sets HDR metadata for the image.
	/// </summary>
	/// <param name="hdrMetadata">The HDR metadata to apply.</param>
	void SetHdrMetadata(HdrMetadata hdrMetadata);

	/// <summary>
	/// Gets the estimated file size for the current encoding settings.
	/// </summary>
	/// <returns>Estimated file size in bytes.</returns>
	long GetEstimatedFileSize();

	/// <summary>
	/// Validates if the current configuration is valid for AVIF encoding.
	/// </summary>
	/// <returns>True if valid, false otherwise.</returns>
	bool IsValid();

	/// <summary>
	/// Creates a thumbnail from the AVIF image.
	/// </summary>
	/// <param name="maxWidth">Maximum width of the thumbnail.</param>
	/// <param name="maxHeight">Maximum height of the thumbnail.</param>
	/// <returns>Thumbnail data encoded as AVIF.</returns>
	Task<byte[]> CreateThumbnailAsync(int maxWidth, int maxHeight);

	/// <summary>
	/// Applies color profile to the image.
	/// </summary>
	/// <param name="iccProfile">ICC color profile data.</param>
	void ApplyColorProfile(byte[] iccProfile);

	/// <summary>
	/// Gets supported features for the current platform.
	/// </summary>
	/// <returns>Supported AVIF features.</returns>
	AvifFeatures GetSupportedFeatures();
}