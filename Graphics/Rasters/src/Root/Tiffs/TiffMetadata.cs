// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Represents metadata information for TIFF images.</summary>
public class TiffMetadata : IMetadata
{
	/// <summary>Gets or sets the image description.</summary>
	public string? ImageDescription { get; set; }

	/// <summary>Gets or sets the camera make.</summary>
	public string? Make { get; set; }

	/// <summary>Gets or sets the camera model.</summary>
	public string? Model { get; set; }

	/// <summary>Gets or sets the software used to create the image.</summary>
	public string? Software { get; set; }

	/// <summary>Gets or sets the copyright information.</summary>
	public string? Copyright { get; set; }

	/// <summary>Gets or sets the artist/photographer.</summary>
	public string? Artist { get; set; }

	/// <summary>Gets or sets the creation date and time.</summary>
	public DateTime? DateTime { get; set; }

	/// <summary>Gets or sets the horizontal resolution in pixels per inch.</summary>
	public double? XResolution { get; set; }

	/// <summary>Gets or sets the vertical resolution in pixels per inch.</summary>
	public double? YResolution { get; set; }

	/// <summary>Gets or sets the resolution unit (1 = no unit, 2 = inches, 3 = centimeters).</summary>
	public int? ResolutionUnit { get; set; }

	/// <summary>Gets or sets additional custom metadata tags.</summary>
	public Dictionary<int, object> CustomTags { get; set; } = new();

	/// <summary>Gets or sets the strip offsets (locations of data strips in the file).</summary>
	public int[]? StripOffsets { get; set; }

	/// <summary>Gets or sets the strip byte counts (sizes of data strips).</summary>
	public int[]? StripByteCounts { get; set; }

	/// <summary>Gets or sets the tile offsets for tiled images.</summary>
	public int[]? TileOffsets { get; set; }

	/// <summary>Gets or sets the tile byte counts for tiled images.</summary>
	public int[]? TileByteCounts { get; set; }

	/// <summary>Gets or sets the color map for palette-indexed images.</summary>
	public ushort[]? ColorMap { get; set; }

	/// <summary>Gets or sets the transfer function for color correction.</summary>
	public ushort[]? TransferFunction { get; set; }

	/// <summary>Gets or sets the white point chromaticity coordinates.</summary>
	public double[]? WhitePoint { get; set; }

	/// <summary>Gets or sets the primary chromaticity coordinates.</summary>
	public double[]? PrimaryChromaticities { get; set; }

	/// <summary>Gets or sets the YCbCr coefficients for color space conversion.</summary>
	public double[]? YCbCrCoefficients { get; set; }

	/// <summary>Gets or sets the reference black and white values.</summary>
	public double[]? ReferenceBlackWhite { get; set; }

	/// <summary>Gets or sets EXIF metadata as a byte array.</summary>
	public byte[]? ExifIfd { get; set; }

	/// <summary>Gets or sets GPS metadata as a byte array.</summary>
	public byte[]? GpsIfd { get; set; }

	/// <summary>Gets or sets ICC color profile data.</summary>
	public byte[]? IccProfile { get; set; }

	/// <summary>Gets or sets XMP metadata as a byte array.</summary>
	public byte[]? XmpData { get; set; }

	/// <summary>Gets or sets IPTC metadata as a byte array.</summary>
	public byte[]? IptcData { get; set; }

	/// <inheritdoc />
	public bool HasLargeMetadata => EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold;

	/// <inheritdoc />
	public long EstimatedMetadataSize
	{
		get
		{
			var size = 0L;

			// Add string metadata sizes
			if (!string.IsNullOrEmpty(ImageDescription))
				size += System.Text.Encoding.UTF8.GetByteCount(ImageDescription);
			if (!string.IsNullOrEmpty(Make))
				size += System.Text.Encoding.UTF8.GetByteCount(Make);
			if (!string.IsNullOrEmpty(Model))
				size += System.Text.Encoding.UTF8.GetByteCount(Model);
			if (!string.IsNullOrEmpty(Software))
				size += System.Text.Encoding.UTF8.GetByteCount(Software);
			if (!string.IsNullOrEmpty(Copyright))
				size += System.Text.Encoding.UTF8.GetByteCount(Copyright);
			if (!string.IsNullOrEmpty(Artist))
				size += System.Text.Encoding.UTF8.GetByteCount(Artist);

			// Add TIFF-specific array data sizes
			if (StripOffsets != null)
				size += StripOffsets.Length * sizeof(int);
			if (StripByteCounts != null)
				size += StripByteCounts.Length * sizeof(int);
			if (TileOffsets != null)
				size += TileOffsets.Length * sizeof(int);
			if (TileByteCounts != null)
				size += TileByteCounts.Length * sizeof(int);

			// Add color data sizes
			if (ColorMap != null)
				size += ColorMap.Length * sizeof(ushort);
			if (TransferFunction != null)
				size += TransferFunction.Length * sizeof(ushort);

			// Add chromaticity and color space data
			if (WhitePoint != null)
				size += WhitePoint.Length * sizeof(double);
			if (PrimaryChromaticities != null)
				size += PrimaryChromaticities.Length * sizeof(double);
			if (YCbCrCoefficients != null)
				size += YCbCrCoefficients.Length * sizeof(double);
			if (ReferenceBlackWhite != null)
				size += ReferenceBlackWhite.Length * sizeof(double);

			// Add embedded metadata sizes
			if (ExifIfd != null)
				size += ExifIfd.Length;
			if (GpsIfd != null)
				size += GpsIfd.Length;
			if (IccProfile != null)
				size += IccProfile.Length;
			if (XmpData != null)
				size += XmpData.Length;
			if (IptcData != null)
				size += IptcData.Length;

			// Add custom tags size
			foreach (var tag in CustomTags.Values)
				size += tag switch
				{
					string str => System.Text.Encoding.UTF8.GetByteCount(str),
					byte[] bytes => bytes.Length,
					int[] ints => ints.Length * sizeof(int),
					ushort[] ushorts => ushorts.Length * sizeof(ushort),
					double[] doubles => doubles.Length * sizeof(double),
					float[] floats => floats.Length * sizeof(float),
					_ => 16 // Default estimate for other types
				};

			// Add TIFF directory entry overhead
			var estimatedTagCount = 20; // Standard TIFF tags
			if (CustomTags.Count > 0)
				estimatedTagCount += CustomTags.Count;

			size += estimatedTagCount * 12; // 12 bytes per directory entry
			size += 6; // IFD header

			return size;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Clear large arrays
		StripOffsets = null;
		StripByteCounts = null;
		TileOffsets = null;
		TileByteCounts = null;
		ColorMap = null;
		TransferFunction = null;
		WhitePoint = null;
		PrimaryChromaticities = null;
		YCbCrCoefficients = null;
		ReferenceBlackWhite = null;
		ExifIfd = null;
		GpsIfd = null;
		IccProfile = null;
		XmpData = null;
		IptcData = null;
		CustomTags.Clear();
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await Task.Run(() => Dispose()).ConfigureAwait(false);
	}
}
