// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

/// <summary>Comprehensive metadata for JPEG2000 images including JP2 boxes and codestream parameters.</summary>
public class Jpeg2000Metadata : IAsyncDisposable, IDisposable
{
	private bool _disposed;

	/// <summary>Basic image properties.</summary>
	public int Width { get; set; }

	/// <summary>Image height in pixels.</summary>
	public int Height { get; set; }

	/// <summary>Number of image components (channels).</summary>
	public int Components { get; set; } = 3;

	/// <summary>Bit depth per component.</summary>
	public int BitDepth { get; set; } = 8;

	/// <summary>Indicates if component values are signed.</summary>
	public bool IsSigned { get; set; }

	/// <summary>Image compression parameters.</summary>
	public bool IsLossless { get; set; } = true;

	/// <summary>Target compression ratio for lossy encoding.</summary>
	public float CompressionRatio { get; set; } = Jpeg2000Constants.DefaultCompressionRatio;

	/// <summary>Number of DWT decomposition levels.</summary>
	public int DecompositionLevels { get; set; } = Jpeg2000Constants.DefaultDecompositionLevels;

	/// <summary>Progression order for codestream organization.</summary>
	public Jpeg2000Progression ProgressionOrder { get; set; } = Jpeg2000Progression.LayerResolutionComponentPosition;

	/// <summary>Number of quality layers.</summary>
	public int QualityLayers { get; set; } = Jpeg2000Constants.QualityLayers.DefaultLayers;

	/// <summary>Color space information.</summary>
	public ushort ColorSpace { get; set; } = Jpeg2000Constants.ColorSpaces.sRGB;

	/// <summary>Indicates if ICC profile is present.</summary>
	public bool HasIccProfile { get; set; }

	/// <summary>ICC color profile data.</summary>
	public byte[]? IccProfile { get; set; }

	/// <summary>Tiling configuration.</summary>
	public int TileWidth { get; set; } = Jpeg2000Constants.DefaultTileSize;

	/// <summary>Tile height in pixels.</summary>
	public int TileHeight { get; set; } = Jpeg2000Constants.DefaultTileSize;

	/// <summary>Number of tiles horizontally.</summary>
	public int TilesAcross => (Width + TileWidth - 1) / TileWidth;

	/// <summary>Number of tiles vertically.</summary>
	public int TilesDown => (Height + TileHeight - 1) / TileHeight;

	/// <summary>Total number of tiles.</summary>
	public int TotalTiles => TilesAcross * TilesDown;

	/// <summary>Resolution metadata.</summary>
	public float CaptureResolutionX { get; set; } = 72.0f;

	/// <summary>Capture resolution Y in DPI.</summary>
	public float CaptureResolutionY { get; set; } = 72.0f;

	/// <summary>Display resolution X in DPI.</summary>
	public float DisplayResolutionX { get; set; } = 72.0f;

	/// <summary>Display resolution Y in DPI.</summary>
	public float DisplayResolutionY { get; set; } = 72.0f;

	/// <summary>Geospatial metadata (GeoJP2).</summary>
	public byte[]? GeoTiffMetadata { get; set; }

	/// <summary>Geographic Markup Language metadata.</summary>
	public string? GmlData { get; set; }

	/// <summary>World file transformation parameters.</summary>
	public double[]? GeoTransform { get; set; }

	/// <summary>Coordinate Reference System as WKT.</summary>
	public string? CoordinateReferenceSystem { get; set; }

	/// <summary>Custom UUID boxes for application-specific metadata.</summary>
	public Dictionary<string, byte[]> UuidBoxes { get; set; } = new();

	/// <summary>XML metadata boxes.</summary>
	public List<string> XmlMetadata { get; set; } = new();

	/// <summary>Comments from COM markers.</summary>
	public List<string> Comments { get; set; } = new();

	/// <summary>Wavelet transform type.</summary>
	public byte WaveletTransform { get; set; } = Jpeg2000Constants.WaveletTransforms.Reversible53;

	/// <summary>Error resilience settings.</summary>
	public byte ErrorResilience { get; set; } = Jpeg2000Constants.ErrorResilience.None;

	/// <summary>Region of Interest (ROI) settings.</summary>
	public Rectangle? RegionOfInterest { get; set; }

	/// <summary>ROI quality enhancement factor.</summary>
	public float RoiQualityFactor { get; set; } = 1.0f;

	/// <summary>Channel definition information.</summary>
	public List<ChannelDefinition> ChannelDefinitions { get; set; } = new();

	/// <summary>Component mapping for palette images.</summary>
	public List<ComponentMapping> ComponentMappings { get; set; } = new();

	/// <summary>Palette data for indexed color images.</summary>
	public byte[]? PaletteData { get; set; }

	/// <summary>Number of palette entries.</summary>
	public int PaletteEntries { get; set; }

	/// <summary>JP2 box structure information.</summary>
	public List<BoxInfo> Boxes { get; set; } = new();

	/// <summary>Codestream marker information.</summary>
	public List<MarkerInfo> Markers { get; set; } = new();

	/// <summary>File creation timestamp.</summary>
	public DateTime CreationTime { get; set; } = DateTime.UtcNow;

	/// <summary>Last modification timestamp.</summary>
	public DateTime ModificationTime { get; set; } = DateTime.UtcNow;

	/// <summary>Intellectual property information.</summary>
	public string? IntellectualProperty { get; set; }

	/// <summary>Determines if this metadata represents a large dataset requiring async disposal.</summary>
	public bool HasLargeMetadata =>
		(IccProfile?.Length ?? 0) > 1024 * 1024 ||
		(GeoTiffMetadata?.Length ?? 0) > 1024 * 1024 ||
		UuidBoxes.Values.Any(data => data.Length > 1024 * 1024) ||
		TotalTiles > 10000;

	/// <summary>Gets the estimated metadata size in bytes.</summary>
	public long EstimatedMetadataSize
	{
		get
		{
			var size = 0L;
			size += IccProfile?.Length ?? 0;
			size += GeoTiffMetadata?.Length ?? 0;
			size += UuidBoxes.Values.Sum(data => data.Length);
			size += XmlMetadata.Sum(xml => xml.Length * 2); // Unicode characters
			size += Comments.Sum(comment => comment.Length * 2);
			size += PaletteData?.Length ?? 0;
			size += ChannelDefinitions.Count * 16; // Approximate size per definition
			size += ComponentMappings.Count * 8;   // Approximate size per mapping
			size += Boxes.Count * 24;              // Approximate size per box info
			size += Markers.Count * 16;            // Approximate size per marker info
			return size + 1024;                    // Base metadata overhead
		}
	}

	/// <summary>Gets the header type based on available metadata.</summary>
	public string HeaderType
	{
		get
		{
			if (!string.IsNullOrEmpty(GmlData) || GeoTransform != null)
				return "GeoJP2";
			if (HasIccProfile)
				return "JP2 with ICC";
			if (XmlMetadata.Count > 0 || UuidBoxes.Count > 0)
				return "JP2 Extended";
			return "JP2 Basic";
		}
	}

	/// <summary>Validates the metadata for consistency and completeness.</summary>
	/// <returns>True if metadata is valid, false otherwise.</returns>
	public bool IsValid()
	{
		if (Width <= 0 || Height <= 0)
			return false;

		if (Components <= 0 || Components > Jpeg2000Constants.MaxComponents)
			return false;

		if (BitDepth <= 0 || BitDepth > Jpeg2000Constants.MaxBitDepth)
			return false;

		if (DecompositionLevels < 0 || DecompositionLevels > Jpeg2000Constants.MaxDecompositionLevels)
			return false;

		if (QualityLayers < Jpeg2000Constants.QualityLayers.MinLayers ||
		    QualityLayers > Jpeg2000Constants.QualityLayers.MaxLayers)
			return false;

		if (TileWidth <= 0 || TileHeight <= 0)
			return false;

		if (!IsLossless && CompressionRatio <= 1.0f)
			return false;

		return true;
	}

	/// <summary>Clones this metadata instance.</summary>
	/// <returns>A deep copy of the metadata.</returns>
	public Jpeg2000Metadata Clone()
	{
		var clone = new Jpeg2000Metadata
		            {
			            Width                     = Width,
			            Height                    = Height,
			            Components                = Components,
			            BitDepth                  = BitDepth,
			            IsSigned                  = IsSigned,
			            IsLossless                = IsLossless,
			            CompressionRatio          = CompressionRatio,
			            DecompositionLevels       = DecompositionLevels,
			            ProgressionOrder          = ProgressionOrder,
			            QualityLayers             = QualityLayers,
			            ColorSpace                = ColorSpace,
			            HasIccProfile             = HasIccProfile,
			            IccProfile                = IccProfile?.ToArray(),
			            TileWidth                 = TileWidth,
			            TileHeight                = TileHeight,
			            CaptureResolutionX        = CaptureResolutionX,
			            CaptureResolutionY        = CaptureResolutionY,
			            DisplayResolutionX        = DisplayResolutionX,
			            DisplayResolutionY        = DisplayResolutionY,
			            GeoTiffMetadata           = GeoTiffMetadata?.ToArray(),
			            GmlData                   = GmlData,
			            GeoTransform              = GeoTransform?.ToArray(),
			            CoordinateReferenceSystem = CoordinateReferenceSystem,
			            UuidBoxes = new Dictionary<string, byte[]>(UuidBoxes.ToDictionary(
				            kvp => kvp.Key,
				            kvp => kvp.Value.ToArray())),
			            XmlMetadata          = new List<string>(XmlMetadata),
			            Comments             = new List<string>(Comments),
			            WaveletTransform     = WaveletTransform,
			            ErrorResilience      = ErrorResilience,
			            RegionOfInterest     = RegionOfInterest,
			            RoiQualityFactor     = RoiQualityFactor,
			            ChannelDefinitions   = new List<ChannelDefinition>(ChannelDefinitions.Select(cd => cd.Clone())),
			            ComponentMappings    = new List<ComponentMapping>(ComponentMappings.Select(cm => cm.Clone())),
			            PaletteData          = PaletteData?.ToArray(),
			            PaletteEntries       = PaletteEntries,
			            Boxes                = new List<BoxInfo>(Boxes.Select(b => b.Clone())),
			            Markers              = new List<MarkerInfo>(Markers.Select(m => m.Clone())),
			            CreationTime         = CreationTime,
			            ModificationTime     = ModificationTime,
			            IntellectualProperty = IntellectualProperty
		            };

		return clone;
	}

	/// <summary>Disposes of resources synchronously.</summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Disposes of resources asynchronously.</summary>
	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		Dispose(false);
		GC.SuppressFinalize(this);
	}

	/// <summary>Protected virtual dispose pattern implementation.</summary>
	/// <param name="disposing">True if disposing managed resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			// Clear large byte arrays
			IccProfile      = null;
			GeoTiffMetadata = null;
			PaletteData     = null;

			// Clear collections
			UuidBoxes.Clear();
			XmlMetadata.Clear();
			Comments.Clear();
			ChannelDefinitions.Clear();
			ComponentMappings.Clear();
			Boxes.Clear();
			Markers.Clear();

			_disposed = true;
		}
	}

	/// <summary>Async disposal implementation for large metadata.</summary>
	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (HasLargeMetadata)
		{
			// For very large metadata, yield control to avoid blocking
			await Task.Yield();
		}

		// Dispose synchronously after yielding
		Dispose(true);
	}
}

/// <summary>Represents a channel definition for multi-component images.</summary>
public class ChannelDefinition
{
	/// <summary>Channel index.</summary>
	public ushort ChannelIndex { get; set; }

	/// <summary>Channel type (color, opacity, etc.).</summary>
	public ushort ChannelType { get; set; }

	/// <summary>Channel association (component mapping).</summary>
	public ushort ChannelAssociation { get; set; }

	/// <summary>Clones this channel definition.</summary>
	public ChannelDefinition Clone()
	{
		return new ChannelDefinition
		       {
			       ChannelIndex       = ChannelIndex,
			       ChannelType        = ChannelType,
			       ChannelAssociation = ChannelAssociation
		       };
	}
}

/// <summary>Represents component mapping for palette images.</summary>
public class ComponentMapping
{
	/// <summary>Component index.</summary>
	public ushort ComponentIndex { get; set; }

	/// <summary>Mapping type.</summary>
	public byte MappingType { get; set; }

	/// <summary>Palette column.</summary>
	public byte PaletteColumn { get; set; }

	/// <summary>Clones this component mapping.</summary>
	public ComponentMapping Clone()
	{
		return new ComponentMapping
		       {
			       ComponentIndex = ComponentIndex,
			       MappingType    = MappingType,
			       PaletteColumn  = PaletteColumn
		       };
	}
}

/// <summary>Represents information about a JP2 box.</summary>
public class BoxInfo
{
	/// <summary>Box type identifier.</summary>
	public string BoxType { get; set; } = string.Empty;

	/// <summary>Box size in bytes.</summary>
	public long BoxSize { get; set; }

	/// <summary>Box offset in file.</summary>
	public long BoxOffset { get; set; }

	/// <summary>Indicates if box size is extended (64-bit).</summary>
	public bool IsExtendedSize { get; set; }

	/// <summary>Clones this box info.</summary>
	public BoxInfo Clone()
	{
		return new BoxInfo
		       {
			       BoxType        = BoxType,
			       BoxSize        = BoxSize,
			       BoxOffset      = BoxOffset,
			       IsExtendedSize = IsExtendedSize
		       };
	}
}

/// <summary>Represents information about a JPEG2000 codestream marker.</summary>
public class MarkerInfo
{
	/// <summary>Marker type.</summary>
	public ushort MarkerType { get; set; }

	/// <summary>Marker offset in codestream.</summary>
	public long MarkerOffset { get; set; }

	/// <summary>Marker segment length.</summary>
	public int SegmentLength { get; set; }

	/// <summary>Clones this marker info.</summary>
	public MarkerInfo Clone()
	{
		return new MarkerInfo
		       {
			       MarkerType    = MarkerType,
			       MarkerOffset  = MarkerOffset,
			       SegmentLength = SegmentLength
		       };
	}
}
