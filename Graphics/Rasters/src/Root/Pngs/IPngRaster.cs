// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Pngs;

/// <summary>Represents a PNG raster image with format-specific properties.</summary>
public interface IPngRaster : IRaster
{
	/// <summary>Gets or sets the PNG color type.</summary>
	PngColorType ColorType { get; set; }

	/// <summary>Gets or sets the bit depth per color component.</summary>
	byte BitDepth { get; set; }

	/// <summary>Gets or sets the compression method.</summary>
	PngCompression Compression { get; set; }

	/// <summary>Gets or sets the filter method.</summary>
	PngFilterMethod FilterMethod { get; set; }

	/// <summary>Gets or sets the interlace method.</summary>
	PngInterlaceMethod InterlaceMethod { get; set; }

	/// <summary>Gets or sets a value indicating whether the image uses a palette.</summary>
	bool UsesPalette { get; set; }

	/// <summary>Gets or sets a value indicating whether the image has transparency.</summary>
	bool HasTransparency { get; set; }

	/// <summary>Gets or sets a value indicating whether the image has an alpha channel.</summary>
	bool HasAlphaChannel { get; set; }

	/// <summary>Gets the number of samples per pixel.</summary>
	int SamplesPerPixel { get; }

	/// <summary>Gets or sets the compression level (0-9, where 9 is the maximum compression).</summary>
	int CompressionLevel { get; set; }

	/// <summary>Gets the PNG metadata.</summary>
	new PngMetadata Metadata { get; }

	/// <summary>Gets the palette data for indexed-color images.</summary>
	ReadOnlyMemory<byte> PaletteData { get; set; }

	/// <summary>Gets or sets the transparency data.</summary>
	ReadOnlyMemory<byte> TransparencyData { get; set; }
}
