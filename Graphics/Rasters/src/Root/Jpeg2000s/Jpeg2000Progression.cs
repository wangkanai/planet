// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

/// <summary>Defines the progression order for JPEG2000 codestreams.</summary>
public enum Jpeg2000Progression : byte
{
	/// <summary>Layer-Resolution-Component-Position progression order.</summary>
	/// <remarks>
	/// Provides quality progression where each layer adds more quality to the entire image.
	/// Best for applications requiring progressive quality enhancement.
	/// </remarks>
	LayerResolutionComponentPosition = Jpeg2000Constants.ProgressionOrders.LRCP,

	/// <summary>Resolution-Layer-Component-Position progression order.</summary>
	/// <remarks>
	/// Provides resolution progression where each resolution level is transmitted completely.
	/// Best for applications requiring progressive resolution enhancement.
	/// </remarks>
	ResolutionLayerComponentPosition = Jpeg2000Constants.ProgressionOrders.RLCP,

	/// <summary>Resolution-Position-Component-Layer progression order.</summary>
	/// <remarks>
	/// Combines resolution and spatial progression.
	/// Best for large images where spatial regions are accessed independently.
	/// </remarks>
	ResolutionPositionComponentLayer = Jpeg2000Constants.ProgressionOrders.RPCL,

	/// <summary>Position-Component-Resolution-Layer progression order.</summary>
	/// <remarks>
	/// Provides spatial progression where image regions are transmitted sequentially.
	/// Best for streaming applications with region-of-interest access.
	/// </remarks>
	PositionComponentResolutionLayer = Jpeg2000Constants.ProgressionOrders.PCRL,

	/// <summary>Component-Position-Resolution-Layer progression order.</summary>
	/// <remarks>
	/// Provides component progression where each color component is transmitted separately.
	/// Best for multi-spectral or hyperspectral imagery applications.
	/// </remarks>
	ComponentPositionResolutionLayer = Jpeg2000Constants.ProgressionOrders.CPRL
}

/// <summary>Extension methods for Jpeg2000Progression enumeration.</summary>
public static class Jpeg2000ProgressionExtensions
{
	/// <summary>Gets a human-readable description of the progression order.</summary>
	/// <param name="progression">The progression order.</param>
	/// <returns>A descriptive string explaining the progression characteristics.</returns>
	public static string GetDescription(this Jpeg2000Progression progression)
	{
		return progression switch
		{
			Jpeg2000Progression.LayerResolutionComponentPosition => "Quality progression - each layer adds quality to entire image",
			Jpeg2000Progression.ResolutionLayerComponentPosition => "Resolution progression - each level adds resolution detail",
			Jpeg2000Progression.ResolutionPositionComponentLayer => "Mixed resolution and spatial progression",
			Jpeg2000Progression.PositionComponentResolutionLayer => "Spatial progression - regions transmitted sequentially",
			Jpeg2000Progression.ComponentPositionResolutionLayer => "Component progression - color channels transmitted separately",
			_                                                    => "Unknown progression order"
		};
	}

	/// <summary>Gets the recommended use case for the progression order.</summary>
	/// <param name="progression">The progression order.</param>
	/// <returns>A string describing the recommended use case.</returns>
	public static string GetRecommendedUseCase(this Jpeg2000Progression progression)
	{
		return progression switch
		{
			Jpeg2000Progression.LayerResolutionComponentPosition => "Progressive quality enhancement, web streaming",
			Jpeg2000Progression.ResolutionLayerComponentPosition => "Progressive resolution, thumbnail generation",
			Jpeg2000Progression.ResolutionPositionComponentLayer => "Large images with spatial access patterns",
			Jpeg2000Progression.PositionComponentResolutionLayer => "Region-of-interest streaming, tiled access",
			Jpeg2000Progression.ComponentPositionResolutionLayer => "Multi-spectral imagery, scientific data",
			_                                                    => "General purpose"
		};
	}

	/// <summary>Determines if the progression order supports efficient spatial access.</summary>
	/// <param name="progression">The progression order.</param>
	/// <returns>True if spatial access is efficient, false otherwise.</returns>
	public static bool SupportsEfficientSpatialAccess(this Jpeg2000Progression progression)
	{
		return progression is Jpeg2000Progression.ResolutionPositionComponentLayer or
			       Jpeg2000Progression.PositionComponentResolutionLayer;
	}

	/// <summary>Determines if the progression order supports efficient quality scaling.</summary>
	/// <param name="progression">The progression order.</param>
	/// <returns>True if quality scaling is efficient, false otherwise.</returns>
	public static bool SupportsEfficientQualityScaling(this Jpeg2000Progression progression)
	{
		return progression is Jpeg2000Progression.LayerResolutionComponentPosition or
			       Jpeg2000Progression.ResolutionLayerComponentPosition;
	}

	/// <summary>Determines if the progression order supports efficient resolution scaling.</summary>
	/// <param name="progression">The progression order.</param>
	/// <returns>True if resolution scaling is efficient, false otherwise.</returns>
	public static bool SupportsEfficientResolutionScaling(this Jpeg2000Progression progression)
	{
		return progression is Jpeg2000Progression.ResolutionLayerComponentPosition or
			       Jpeg2000Progression.ResolutionPositionComponentLayer;
	}

	/// <summary>Gets the byte value for the progression order.</summary>
	/// <param name="progression">The progression order.</param>
	/// <returns>The byte value used in JPEG2000 codestreams.</returns>
	public static byte ToByte(this Jpeg2000Progression progression)
	{
		return (byte)progression;
	}
}
