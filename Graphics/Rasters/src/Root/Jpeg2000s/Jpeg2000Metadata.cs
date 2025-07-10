// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;
using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

/// <summary>Comprehensive metadata for JPEG2000 images including JP2 boxes and codestream parameters.</summary>
public class Jpeg2000Metadata : RasterMetadata
{

	// Note: Width and Height are inherited from base class

	/// <summary>Number of image components (channels).</summary>
	public int Components { get; set; } = 3;

	// Note: BitDepth is inherited from base class

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

	// Note: IccProfile is inherited from base class

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

	// Note: CreationTime and ModificationTime are inherited from base class

	/// <summary>Intellectual property information.</summary>
	public string? IntellectualProperty { get; set; }

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			var size = base.EstimatedMetadataSize;
			size += EstimateByteArraySize(GeoTiffMetadata);
			size += UuidBoxes.Values.Sum(data => data.Length);
			foreach (var xml in XmlMetadata)
				size += EstimateStringSize(xml);
			foreach (var comment in Comments)
				size += EstimateStringSize(comment);
			size += PaletteData?.Length ?? 0;
			size += ChannelDefinitions.Count * 16; // Approximate size per definition
			size += ComponentMappings.Count * 8;   // Approximate size per mapping
			size += Boxes.Count * 24;              // Approximate size per box info
			size += Markers.Count * 16;            // Approximate size per marker info
			
			// Add size for tile management overhead
			// Each tile needs metadata tracking (position, size, etc.)
			// Estimate ~100 bytes per tile for management structures
			if (TileWidth > 0 && TileHeight > 0)
			{
				var tilesAcross = (Width + TileWidth - 1) / TileWidth;
				var tilesDown = (Height + TileHeight - 1) / TileHeight;
				var totalTiles = tilesAcross * tilesDown;
				size += totalTiles * 100; // ~100 bytes per tile metadata
			}
			
			return size;
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

	/// <inheritdoc />
	public override IMetadata Clone() => CloneRaster();
	
	/// <inheritdoc />
	public override IRasterMetadata CloneRaster()
	{
		var clone = new Jpeg2000Metadata();
		CopyRasterTo(clone);
		
		// Copy JPEG2000-specific properties
		clone.Components = Components;
		clone.IsSigned = IsSigned;
		clone.IsLossless = IsLossless;
		clone.CompressionRatio = CompressionRatio;
		clone.DecompositionLevels = DecompositionLevels;
		clone.ProgressionOrder = ProgressionOrder;
		clone.QualityLayers = QualityLayers;
		clone.ColorSpace = ColorSpace;
		clone.HasIccProfile = HasIccProfile;
		clone.TileWidth = TileWidth;
		clone.TileHeight = TileHeight;
		clone.CaptureResolutionX = CaptureResolutionX;
		clone.CaptureResolutionY = CaptureResolutionY;
		clone.DisplayResolutionX = DisplayResolutionX;
		clone.DisplayResolutionY = DisplayResolutionY;
		clone.GeoTiffMetadata = GeoTiffMetadata?.ToArray();
		clone.GmlData = GmlData;
		clone.GeoTransform = GeoTransform?.ToArray();
		clone.CoordinateReferenceSystem = CoordinateReferenceSystem;
		clone.WaveletTransform = WaveletTransform;
		clone.ErrorResilience = ErrorResilience;
		clone.RegionOfInterest = RegionOfInterest;
		clone.RoiQualityFactor = RoiQualityFactor;
		clone.PaletteData = PaletteData?.ToArray();
		clone.PaletteEntries = PaletteEntries;
		clone.IntellectualProperty = IntellectualProperty;
		
		// Deep copy UUID boxes
		foreach (var kvp in UuidBoxes)
			clone.UuidBoxes[kvp.Key] = kvp.Value.ToArray();
		
		// Deep copy collections
		clone.ComponentMappings = new List<ComponentMapping>(ComponentMappings.Select(m => m.Clone()));
		clone.ChannelDefinitions = new List<ChannelDefinition>(ChannelDefinitions.Select(d => d.Clone()));
		clone.XmlMetadata = new List<string>(XmlMetadata);
		clone.Comments = new List<string>(Comments);
		clone.Boxes = new List<BoxInfo>(Boxes.Select(b => b.Clone()));
		clone.Markers = new List<MarkerInfo>(Markers.Select(m => m.Clone()));

		return clone;
	}

	/// <inheritdoc />
	protected override void DisposeManagedResources()
	{
		base.DisposeManagedResources();
		
		// Clear JPEG2000-specific resources
		GeoTiffMetadata = null;
		PaletteData = null;
		GeoTransform = null;
		
		// Clear collections
		UuidBoxes.Clear();
		XmlMetadata.Clear();
		Comments.Clear();
		ChannelDefinitions.Clear();
		ComponentMappings.Clear();
		Boxes.Clear();
		Markers.Clear();
	}
	
	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();
		
		// Reset JPEG2000-specific properties to defaults
		Components = 3;
		IsSigned = false;
		IsLossless = true;
		CompressionRatio = Jpeg2000Constants.DefaultCompressionRatio;
		DecompositionLevels = Jpeg2000Constants.DefaultDecompositionLevels;
		ProgressionOrder = Jpeg2000Progression.LayerResolutionComponentPosition;
		QualityLayers = Jpeg2000Constants.QualityLayers.DefaultLayers;
		ColorSpace = Jpeg2000Constants.ColorSpaces.sRGB;
		HasIccProfile = false;
		TileWidth = Jpeg2000Constants.DefaultTileSize;
		TileHeight = Jpeg2000Constants.DefaultTileSize;
		CaptureResolutionX = 0;
		CaptureResolutionY = 0;
		DisplayResolutionX = 0;
		DisplayResolutionY = 0;
		GeoTiffMetadata = null;
		GmlData = null;
		GeoTransform = null;
		CoordinateReferenceSystem = null;
		WaveletTransform = Jpeg2000Constants.WaveletTransforms.Reversible53;
		ErrorResilience = Jpeg2000Constants.ErrorResilience.None;
		RegionOfInterest = null;
		RoiQualityFactor = 1.0f;
		PaletteData = null;
		PaletteEntries = 0;
		IntellectualProperty = null;
		UuidBoxes.Clear();
		XmlMetadata.Clear();
		Comments.Clear();
		ChannelDefinitions.Clear();
		ComponentMappings.Clear();
		Boxes.Clear();
		Markers.Clear();
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
