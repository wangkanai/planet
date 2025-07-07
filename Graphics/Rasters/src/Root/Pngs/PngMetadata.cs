// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Pngs;

/// <summary>Represents PNG metadata information including ancillary chunks.</summary>
public class PngMetadata : IMetadata
{
	/// <summary>Gets or sets the image title.</summary>
	public string? Title
	{
		get;
		set;
	}

	/// <summary>Gets or sets the image description.</summary>
	public string? Description
	{
		get;
		set;
	}

	/// <summary>Gets or sets the software used to create the image.</summary>
	public string? Software
	{
		get;
		set;
	}

	/// <summary>Gets or sets the creation timestamp.</summary>
	public DateTime? Created
	{
		get;
		set;
	}

	/// <summary>Gets or sets the last modification timestamp.</summary>
	public DateTime? Modified
	{
		get;
		set;
	}

	/// <summary>Gets or sets the image author.</summary>
	public string? Author
	{
		get;
		set;
	}

	/// <summary>Gets or sets the image copyright information.</summary>
	public string? Copyright
	{
		get;
		set;
	}

	/// <summary>Gets or sets the image comment.</summary>
	public string? Comment
	{
		get;
		set;
	}

	/// <summary>Gets or sets the gamma value for color correction.</summary>
	public double? Gamma
	{
		get;
		set;
	}

	/// <summary>Gets or sets the horizontal resolution in pixels per unit.</summary>
	public uint? XResolution
	{
		get;
		set;
	}

	/// <summary>Gets or sets the vertical resolution in pixels per unit.</summary>
	public uint? YResolution
	{
		get;
		set;
	}

	/// <summary>Gets or sets the resolution unit (0 = unknown, 1 = meter).</summary>
	public byte? ResolutionUnit
	{
		get;
		set;
	}

	/// <summary>Gets or sets the background color.</summary>
	public uint? BackgroundColor
	{
		get;
		set;
	}

	/// <summary>Gets or sets the white point chromaticity coordinates.</summary>
	public (uint x, uint y)? WhitePoint
	{
		get;
		set;
	}

	/// <summary>Gets or sets the red primary chromaticity coordinates.</summary>
	public (uint x, uint y)? RedPrimary
	{
		get;
		set;
	}

	/// <summary>Gets or sets the green primary chromaticity coordinates.</summary>
	public (uint x, uint y)? GreenPrimary
	{
		get;
		set;
	}

	/// <summary>Gets or sets the blue primary chromaticity coordinates.</summary>
	public (uint x, uint y)? BluePrimary
	{
		get;
		set;
	}

	/// <summary>Gets or sets the standard RGB color space rendering intent.</summary>
	/// <remarks>0 = Perceptual, 1 = Relative colorimetric, 2 = Saturation, 3 = Absolute colorimetric</remarks>
	public byte? SrgbRenderingIntent
	{
		get;
		set;
	}

	/// <summary>Gets the collection of custom text chunks.</summary>
	/// <remarks>Key is the chunk keyword, value is the text content.</remarks>
	public Dictionary<string, string> TextChunks
	{
		get;
	} = new();

	/// <summary>Gets the collection of compressed text chunks.</summary>
	/// <remarks>Key is the chunk keyword, value is the text content.</remarks>
	public Dictionary<string, string> CompressedTextChunks
	{
		get;
	} = new();

	/// <summary>Gets the collection of international text chunks.</summary>
	/// <remarks>Key is the chunk keyword, value contains language tag and text content.</remarks>
	public Dictionary<string, (string? languageTag, string? translatedKeyword, string text)> InternationalTextChunks
	{
		get;
	} = new();

	/// <summary>Gets the collection of custom chunks not defined in the PNG specification.</summary>
	/// <remarks>Key is the chunk type, value is the raw chunk data.</remarks>
	public Dictionary<string, byte[]> CustomChunks
	{
		get;
	} = new();

	/// <summary>Gets or sets the transparency information for the image.</summary>
	public ReadOnlyMemory<byte> TransparencyData
	{
		get;
		set;
	}

	/// <summary>Validates the PNG metadata.</summary>
	/// <returns>A validation result indicating if the metadata is valid.</returns>
	public PngValidationResult ValidateMetadata()
	{
		var result = new PngValidationResult();

		// Validate gamma value
		if (Gamma.HasValue && (Gamma <= 0 || Gamma > 10))
			result.AddError($"Invalid gamma value: {Gamma}. Gamma must be between 0 and 10.");

		// Validate resolution values
		if (XResolution.HasValue && XResolution == 0)
			result.AddError("Invalid X resolution: 0. Resolution must be greater than 0.");

		if (YResolution.HasValue && YResolution == 0)
			result.AddError("Invalid Y resolution: 0. Resolution must be greater than 0.");

		// Validate resolution unit
		if (ResolutionUnit.HasValue && ResolutionUnit > 1)
			result.AddError($"Invalid resolution unit: {ResolutionUnit}. Must be 0 (unknown) or 1 (meter).");

		// Validate sRGB rendering intent
		if (SrgbRenderingIntent.HasValue && SrgbRenderingIntent > 3)
			result.AddError($"Invalid sRGB rendering intent: {SrgbRenderingIntent}. Must be 0-3.");

		// Validate text chunk keywords
		foreach (var keyword in TextChunks.Keys.Concat(CompressedTextChunks.Keys).Concat(InternationalTextChunks.Keys))
			if (string.IsNullOrEmpty(keyword))
				result.AddError("Text chunk keyword cannot be null or empty.");
			else if (keyword.Length > 79)
				result.AddError($"Text chunk keyword '{keyword}' exceeds maximum length of 79 characters.");
			else if (!IsValidKeyword(keyword))
				result.AddError($"Text chunk keyword '{keyword}' contains invalid characters. Only Latin-1 printable characters and spaces are allowed.");

		return result;
	}

	/// <summary>Validates if a keyword contains only valid characters.</summary>
	/// <param name="keyword">The keyword to validate.</param>
	/// <returns>True if the keyword is valid, false otherwise.</returns>
	private static bool IsValidKeyword(string keyword)
	{
		if (string.IsNullOrEmpty(keyword)) return false;
		if (keyword.StartsWith(' ') || keyword.EndsWith(' ')) return false;
		if (keyword.Contains("  ")) return false; // No consecutive spaces

		foreach (var c in keyword)
			// Latin-1 printable characters (32-126 and 161-255)
			if (c is < (char)32 or > (char)126 and < (char)161)
				return false;

		return true;
	}

	/// <inheritdoc />
	public bool HasLargeMetadata => EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold;

	/// <inheritdoc />
	public long EstimatedMetadataSize
	{
		get
		{
			var size = 0L;

			// Add transparency data size
			if (!TransparencyData.IsEmpty)
				size += TransparencyData.Length;

			// Add text chunk sizes
			foreach (var textChunk in TextChunks.Values)
				size += System.Text.Encoding.UTF8.GetByteCount(textChunk);

			foreach (var compressedTextChunk in CompressedTextChunks.Values)
				size += compressedTextChunk.Length;

			foreach (var internationalTextChunk in InternationalTextChunks.Values)
				size += System.Text.Encoding.UTF8.GetByteCount(internationalTextChunk.text);

			// Add custom chunk sizes
			foreach (var customChunk in CustomChunks.Values)
				size += customChunk.Length;

			return size;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (HasLargeMetadata)
		{
			// For large metadata, clear in stages with yielding
			await Task.Yield();
			TransparencyData = ReadOnlyMemory<byte>.Empty;

			await Task.Yield();
			TextChunks.Clear();

			await Task.Yield();
			CompressedTextChunks.Clear();

			await Task.Yield();
			InternationalTextChunks.Clear();

			await Task.Yield();
			CustomChunks.Clear();
		}
		else
		{
			// For small metadata, use synchronous disposal
			Dispose(true);
		}
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases unmanaged and - optionally - managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Clear managed resources
			TransparencyData = ReadOnlyMemory<byte>.Empty;
			TextChunks.Clear();
			CompressedTextChunks.Clear();
			InternationalTextChunks.Clear();
			CustomChunks.Clear();
		}
	}
}
