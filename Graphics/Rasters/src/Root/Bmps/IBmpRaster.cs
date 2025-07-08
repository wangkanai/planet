// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

/// <summary>Represents BMP format specifications and properties.</summary>
public interface IBmpRaster : IRaster
{
	/// <summary>Gets the color depth of the BMP image.</summary>
	BmpColorDepth ColorDepth { get; }

	/// <summary>Gets the compression method used in the BMP image.</summary>
	BmpCompression Compression { get; }

	/// <summary>Gets the metadata associated with the BMP image.</summary>
	new BmpMetadata Metadata { get; }

	/// <summary>Gets the horizontal resolution in pixels per meter.</summary>
	int HorizontalResolution { get; }

	/// <summary>Gets the vertical resolution in pixels per meter.</summary>
	int VerticalResolution { get; }

	/// <summary>Gets the color palette data for indexed color images.</summary>
	byte[]? ColorPalette { get; }

	/// <summary>Gets a value indicating whether the image uses a color palette.</summary>
	bool HasPalette { get; }

	/// <summary>Gets a value indicating whether the image has transparency (alpha channel).</summary>
	bool HasTransparency { get; }

	/// <summary>Gets a value indicating whether the image is stored in top-down format.</summary>
	bool IsTopDown { get; }

	/// <summary>Gets the number of bytes per pixel for the current color depth.</summary>
	int BytesPerPixel { get; }

	/// <summary>Gets the row stride in bytes (including padding).</summary>
	int RowStride { get; }

	/// <summary>Gets the total size of pixel data in bytes.</summary>
	uint PixelDataSize { get; }

	/// <summary>Gets the bit field masks for custom color formats.</summary>
	(uint Red, uint Green, uint Blue, uint Alpha) GetBitMasks();

	/// <summary>Converts the image to 24-bit RGB format.</summary>
	void ConvertToRgb();

	/// <summary>Applies a color palette to the image (for indexed color formats).</summary>
	/// <param name="palette">The color palette to apply.</param>
	void ApplyPalette(byte[] palette);

	/// <summary>Sets custom bit field masks for BI_BITFIELDS compression.</summary>
	/// <param name="redMask">The red component bit mask.</param>
	/// <param name="greenMask">The green component bit mask.</param>
	/// <param name="blueMask">The blue component bit mask.</param>
	/// <param name="alphaMask">The alpha component bit mask.</param>
	void SetBitMasks(uint redMask, uint greenMask, uint blueMask, uint alphaMask = 0);

	/// <summary>Validates the BMP format and structure.</summary>
	/// <returns>True if the BMP format is valid, false otherwise.</returns>
	bool IsValid();

	/// <summary>Gets the estimated file size for the BMP image.</summary>
	/// <returns>The estimated file size in bytes.</returns>
	long GetEstimatedFileSize();
}
