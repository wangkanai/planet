// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;
using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Heifs;

/// <summary>
/// Represents a HEIF (High Efficiency Image File Format) raster image with advanced compression and HDR capabilities.
/// </summary>
public interface IHeifRaster : IRaster
{
	/// <summary>Gets or sets the color space used by the HEIF image.</summary>
	HeifColorSpace ColorSpace { get; set; }

	/// <summary>Gets or sets the quality level for encoding (0-100).</summary>
	int Quality { get; set; }

	/// <summary>Gets or sets the comprehensive metadata for the HEIF image.</summary>
	new HeifMetadata Metadata { get; set; }

	/// <summary>Gets or sets the bit depth per channel (8, 10, 12, or 16).</summary>
	int BitDepth { get; set; }

	/// <summary>Gets or sets whether the image has an alpha channel.</summary>
	bool HasAlpha { get; set; }

	/// <summary>Gets or sets the chroma subsampling mode.</summary>
	HeifChromaSubsampling ChromaSubsampling { get; set; }

	/// <summary>Gets or sets the encoder speed setting (0-9, where 0 is slowest/best quality).</summary>
	int Speed { get; set; }

	/// <summary>Gets or sets the compression method used.</summary>
	HeifCompression Compression { get; set; }

	/// <summary>Gets or sets whether lossless compression is used.</summary>
	bool IsLossless { get; set; }

	/// <summary>Gets whether the image contains HDR metadata.</summary>
	bool HasHdrMetadata { get; }

	/// <summary>Gets the number of threads to use for encoding/decoding (0 = auto).</summary>
	int ThreadCount { get; set; }

	/// <summary>Gets or sets the HEIF profile to use for encoding.</summary>
	HeifProfile Profile { get; set; }

	/// <summary>Gets or sets whether to enable progressive decoding.</summary>
	bool EnableProgressiveDecoding { get; set; }

	/// <summary>Gets or sets whether to generate thumbnails automatically.</summary>
	bool GenerateThumbnails { get; set; }

	/// <summary>
	/// Encodes the raster image to HEIF format asynchronously.
	/// </summary>
	/// <param name="options">Optional encoding options.</param>
	/// <returns>The encoded HEIF data.</returns>
	Task<byte[]> EncodeAsync(HeifEncodingOptions? options = null);

	/// <summary>
	/// Decodes HEIF data into the raster image asynchronously.
	/// </summary>
	/// <param name="data">The HEIF file data to decode.</param>
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
	/// Validates if the current configuration is valid for HEIF encoding.
	/// </summary>
	/// <returns>True if valid, false otherwise.</returns>
	bool IsValid();

	/// <summary>
	/// Creates a thumbnail from the HEIF image.
	/// </summary>
	/// <param name="maxWidth">Maximum width of the thumbnail.</param>
	/// <param name="maxHeight">Maximum height of the thumbnail.</param>
	/// <returns>Thumbnail data encoded as HEIF.</returns>
	Task<byte[]> CreateThumbnailAsync(int maxWidth, int maxHeight);

	/// <summary>
	/// Applies color profile to the image.
	/// </summary>
	/// <param name="iccProfile">ICC color profile data.</param>
	void ApplyColorProfile(byte[] iccProfile);

	/// <summary>
	/// Gets supported features for the current platform.
	/// </summary>
	/// <returns>Supported HEIF features.</returns>
	HeifFeatures GetSupportedFeatures();

	/// <summary>
	/// Sets the codec-specific parameters.
	/// </summary>
	/// <param name="codecParameters">Codec-specific configuration parameters.</param>
	void SetCodecParameters(Dictionary<string, object> codecParameters);

	/// <summary>
	/// Gets the container format information.
	/// </summary>
	/// <returns>Container format details.</returns>
	HeifContainerInfo GetContainerInfo();
}