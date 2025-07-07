// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Defines the contract for raster image encoding options across different formats.
/// </summary>
public interface IRasterEncodingOptions
{
	/// <summary>
	/// Gets or sets the quality level for encoding (0-100, where 100 is best).
	/// </summary>
	int Quality { get; set; }

	/// <summary>
	/// Gets or sets the encoding speed (0-10, where 0 is slowest/best quality).
	/// </summary>
	int Speed { get; set; }

	/// <summary>
	/// Gets or sets whether to use lossless compression.
	/// </summary>
	bool IsLossless { get; set; }

	/// <summary>
	/// Gets or sets the number of threads to use for encoding (0 = auto-detect).
	/// </summary>
	int ThreadCount { get; set; }

	/// <summary>
	/// Gets or sets the chroma subsampling mode.
	/// </summary>
	ChromaSubsampling ChromaSubsampling { get; set; }

	/// <summary>
	/// Gets or sets whether to preserve metadata during encoding.
	/// </summary>
	bool PreserveMetadata { get; set; }

	/// <summary>
	/// Gets or sets whether to preserve color profile information.
	/// </summary>
	bool PreserveColorProfile { get; set; }

	/// <summary>
	/// Gets or sets the maximum pixel buffer size in megabytes.
	/// </summary>
	int MaxPixelBufferSizeMB { get; set; }

	/// <summary>
	/// Gets or sets the maximum metadata buffer size in megabytes.
	/// </summary>
	int MaxMetadataBufferSizeMB { get; set; }

	/// <summary>
	/// Validates the encoding options.
	/// </summary>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the options are valid, false otherwise.</returns>
	bool Validate(out string? error);

	/// <summary>
	/// Creates a deep copy of the encoding options.
	/// </summary>
	/// <returns>A new instance with the same values.</returns>
	IRasterEncodingOptions Clone();
}