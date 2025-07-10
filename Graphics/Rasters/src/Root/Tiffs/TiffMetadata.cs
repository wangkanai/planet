// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Represents metadata information for TIFF images.</summary>
public class TiffMetadata : RasterMetadata
{
	/// <summary>Gets or sets the image description.</summary>
	/// <remarks>Maps to the Description property from base class for backward compatibility.</remarks>
	public string? ImageDescription
	{
		get => Description;
		set => Description = value;
	}

	/// <summary>Gets or sets the camera make.</summary>
	public string? Make { get; set; }

	/// <summary>Gets or sets the camera model.</summary>
	public string? Model { get; set; }

	/// <summary>Gets or sets the artist/photographer.</summary>
	/// <remarks>Maps to the Author property from base class for backward compatibility.</remarks>
	public string? Artist
	{
		get => Author;
		set => Author = value;
	}

	/// <summary>Gets or sets the TIFF-specific creation date and time.</summary>
	public DateTime? DateTime
	{
		get => CreationTime;
		set => CreationTime = value;
	}

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

	/// <summary>Gets or sets TIFF-specific EXIF IFD data.</summary>
	public byte[]? ExifIfd
	{
		get => ExifData;
		set => ExifData = value;
	}

	/// <summary>Gets or sets GPS metadata as a byte array.</summary>
	public byte[]? GpsIfd { get; set; }

	// Note: IccProfile and XmpData are inherited from base class

	/// <summary>Gets or sets IPTC metadata as a byte array.</summary>
	public byte[]? IptcData { get; set; }

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			var size = base.EstimatedMetadataSize;

			// Add TIFF-specific string metadata sizes
			size += EstimateStringSize(Make);
			size += EstimateStringSize(Model);

			// Add TIFF-specific array data sizes
			size += EstimateArraySize(StripOffsets, sizeof(int));
			size += EstimateArraySize(StripByteCounts, sizeof(int));
			size += EstimateArraySize(TileOffsets, sizeof(int));
			size += EstimateArraySize(TileByteCounts, sizeof(int));

			// Add color data sizes
			size += EstimateArraySize(ColorMap, sizeof(ushort));
			size += EstimateArraySize(TransferFunction, sizeof(ushort));

			// Add chromaticity and color space data
			size += EstimateArraySize(WhitePoint, sizeof(double));
			size += EstimateArraySize(PrimaryChromaticities, sizeof(double));
			size += EstimateArraySize(YCbCrCoefficients, sizeof(double));
			size += EstimateArraySize(ReferenceBlackWhite, sizeof(double));

			// Add embedded metadata sizes
			size += EstimateByteArraySize(GpsIfd);
			size += EstimateByteArraySize(IptcData);

			// Add custom tags size - TIFF uses a more specific calculation
			foreach (var tag in CustomTags.Values)
				size += tag switch
				{
					string str => EstimateStringSize(str),
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
	protected override void DisposeManagedResources()
	{
		base.DisposeManagedResources();
		
		// Clear TIFF-specific large arrays
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
		GpsIfd = null;
		IptcData = null;
		CustomTags.Clear();
	}

	/// <inheritdoc />
	public override IMetadata Clone() => CloneRaster();
	
	/// <inheritdoc />
	public override IRasterMetadata CloneRaster()
	{
		var clone = new TiffMetadata();
		CopyRasterTo(clone);
		
		// Copy TIFF-specific properties
		clone.Make = Make;
		clone.Model = Model;
		clone.XResolution = XResolution;
		clone.YResolution = YResolution;
		clone.ResolutionUnit = ResolutionUnit;
		clone.StripOffsets = StripOffsets?.ToArray();
		clone.StripByteCounts = StripByteCounts?.ToArray();
		clone.TileOffsets = TileOffsets?.ToArray();
		clone.TileByteCounts = TileByteCounts?.ToArray();
		clone.ColorMap = ColorMap?.ToArray();
		clone.TransferFunction = TransferFunction?.ToArray();
		clone.WhitePoint = WhitePoint?.ToArray();
		clone.PrimaryChromaticities = PrimaryChromaticities?.ToArray();
		clone.YCbCrCoefficients = YCbCrCoefficients?.ToArray();
		clone.ReferenceBlackWhite = ReferenceBlackWhite?.ToArray();
		clone.GpsIfd = GpsIfd?.ToArray();
		clone.IptcData = IptcData?.ToArray();
		clone.CustomTags = new Dictionary<int, object>(CustomTags);
		
		return clone;
	}
	
	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();
		
		// Clear TIFF-specific properties
		Make = null;
		Model = null;
		XResolution = null;
		YResolution = null;
		ResolutionUnit = null;
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
		GpsIfd = null;
		IptcData = null;
		CustomTags.Clear();
	}
}
