// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Pngs;

/// <summary>Represents PNG metadata information including ancillary chunks.</summary>
public class PngMetadata : RasterMetadata
{
	/// <summary>Gets or sets the image title.</summary>
	public string? Title { get; set; }

	// Note: Description and Software are inherited from base class

	/// <summary>Gets or sets the PNG-specific creation timestamp.</summary>
	public DateTime? Created
	{
		get => CreationTime;
		set => CreationTime = value;
	}

	/// <summary>Gets or sets the PNG-specific modification timestamp.</summary>
	public DateTime? Modified
	{
		get => ModificationTime;
		set => ModificationTime = value;
	}

	// Note: Author and Copyright are inherited from base class

	/// <summary>Gets or sets the image comment.</summary>
	public string? Comment { get; set; }

	/// <summary>Gets or sets the gamma value for color correction.</summary>
	public double? Gamma { get; set; }

	/// <summary>Gets or sets the horizontal resolution in pixels per unit.</summary>
	public uint? XResolution { get; set; }

	/// <summary>Gets or sets the vertical resolution in pixels per unit.</summary>
	public uint? YResolution { get; set; }

	/// <summary>Gets or sets the resolution unit (0 = unknown, 1 = meter).</summary>
	public byte? ResolutionUnit { get; set; }

	/// <summary>Gets or sets the background color.</summary>
	public uint? BackgroundColor { get; set; }

	/// <summary>Gets or sets the white point chromaticity coordinates.</summary>
	public (uint x, uint y)? WhitePoint { get; set; }

	/// <summary>Gets or sets the red primary chromaticity coordinates.</summary>
	public (uint x, uint y)? RedPrimary { get; set; }

	/// <summary>Gets or sets the green primary chromaticity coordinates.</summary>
	public (uint x, uint y)? GreenPrimary { get; set; }

	/// <summary>Gets or sets the blue primary chromaticity coordinates.</summary>
	public (uint x, uint y)? BluePrimary { get; set; }

	/// <summary>Gets or sets the standard RGB color space rendering intent.</summary>
	/// <remarks>0 = Perceptual, 1 = Relative colorimetric, 2 = Saturation, 3 = Absolute colorimetric</remarks>
	public byte? SrgbRenderingIntent { get; set; }

	/// <summary>Gets the collection of custom text chunks.</summary>
	/// <remarks>Key is the chunk keyword, value is the text content.</remarks>
	public Dictionary<string, string> TextChunks { get; } = new();

	/// <summary>Gets the collection of compressed text chunks.</summary>
	/// <remarks>Key is the chunk keyword, value is the text content.</remarks>
	public Dictionary<string, string> CompressedTextChunks { get; } = new();

	/// <summary>Gets the collection of international text chunks.</summary>
	/// <remarks>Key is the chunk keyword, value contains language tag and text content.</remarks>
	public Dictionary<string, (string? languageTag, string? translatedKeyword, string text)> InternationalTextChunks { get; } = new();

	/// <summary>Gets the collection of custom chunks not defined in the PNG specification.</summary>
	/// <remarks>Key is the chunk type, value is the raw chunk data.</remarks>
	public Dictionary<string, byte[]> CustomChunks { get; } = new();

	/// <summary>Gets or sets the transparency information for the image.</summary>
	public ReadOnlyMemory<byte> TransparencyData { get; set; } = new();

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
	public override long EstimatedMetadataSize
	{
		get
		{
			var size = base.EstimatedMetadataSize;

			// Add transparency data size
			if (!TransparencyData.IsEmpty)
				size += TransparencyData.Length;

			// Add text chunk sizes
			size += EstimateDictionarySize(TextChunks);

			foreach (var compressedTextChunk in CompressedTextChunks.Values)
				size += compressedTextChunk.Length;

			foreach (var internationalTextChunk in InternationalTextChunks.Values)
				size += System.Text.Encoding.UTF8.GetByteCount(internationalTextChunk.text);

			// Add custom chunk sizes
			size += EstimateDictionaryByteArraySize(CustomChunks);

			return size;
		}
	}

	/// <inheritdoc />
	protected override void DisposeManagedResources()
	{
		base.DisposeManagedResources();

		// Clear PNG-specific resources
		TransparencyData = ReadOnlyMemory<byte>.Empty;
		TextChunks.Clear();
		CompressedTextChunks.Clear();
		InternationalTextChunks.Clear();
		CustomChunks.Clear();
	}

	/// <inheritdoc />
	public override async ValueTask DisposeAsync()
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

	/// <inheritdoc />
	public override IMetadata Clone() => CloneRaster();
	
	/// <inheritdoc />
	public override IRasterMetadata CloneRaster()
	{
		var clone = new PngMetadata();
		CopyRasterTo(clone);

		// Copy PNG-specific properties
		clone.Title = Title;
		clone.Comment = Comment;
		clone.Gamma = Gamma;
		clone.XResolution = XResolution;
		clone.YResolution = YResolution;
		clone.ResolutionUnit = ResolutionUnit;
		clone.BackgroundColor = BackgroundColor;
		clone.WhitePoint = WhitePoint;
		clone.RedPrimary = RedPrimary;
		clone.GreenPrimary = GreenPrimary;
		clone.BluePrimary = BluePrimary;
		clone.SrgbRenderingIntent = SrgbRenderingIntent;
		clone.TransparencyData = TransparencyData;

		// Deep copy collections
		foreach (var kvp in TextChunks)
			clone.TextChunks[kvp.Key] = kvp.Value;
		foreach (var kvp in CompressedTextChunks)
			clone.CompressedTextChunks[kvp.Key] = kvp.Value;
		foreach (var kvp in InternationalTextChunks)
			clone.InternationalTextChunks[kvp.Key] = kvp.Value;
		foreach (var kvp in CustomChunks)
			clone.CustomChunks[kvp.Key] = kvp.Value.ToArray();

		return clone;
	}

	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();

		// Clear PNG-specific properties
		Title = null;
		Comment = null;
		Gamma = null;
		XResolution = null;
		YResolution = null;
		ResolutionUnit = null;
		BackgroundColor = null;
		WhitePoint = null;
		RedPrimary = null;
		GreenPrimary = null;
		BluePrimary = null;
		SrgbRenderingIntent = null;
		TransparencyData = ReadOnlyMemory<byte>.Empty;
		TextChunks.Clear();
		CompressedTextChunks.Clear();
		InternationalTextChunks.Clear();
		CustomChunks.Clear();
	}
}
