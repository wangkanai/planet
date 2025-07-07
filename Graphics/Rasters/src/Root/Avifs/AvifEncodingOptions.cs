// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Avifs;

/// <summary>
/// Represents encoding options for AVIF image compression.
/// </summary>
public class AvifEncodingOptions
{
	/// <summary>Gets or sets the quality level (0-100, where 100 is best).</summary>
	public int Quality { get; set; } = AvifConstants.DefaultQuality;

	/// <summary>Gets or sets the alpha quality level (0-100).</summary>
	public int AlphaQuality { get; set; } = AvifConstants.DefaultQuality;

	/// <summary>Gets or sets the encoding speed (0-10, where 0 is slowest/best).</summary>
	public int Speed { get; set; } = AvifConstants.DefaultSpeed;

	/// <summary>Gets or sets whether to use lossless compression.</summary>
	public bool IsLossless { get; set; }

	/// <summary>Gets or sets whether to use lossless compression for alpha channel.</summary>
	public bool IsAlphaLossless { get; set; }

	/// <summary>Gets or sets the chroma subsampling mode.</summary>
	public AvifChromaSubsampling ChromaSubsampling { get; set; } = AvifChromaSubsampling.Yuv420;

	/// <summary>Gets or sets the number of threads to use (0 = auto).</summary>
	public int ThreadCount { get; set; } = AvifConstants.DefaultThreadCount;

	/// <summary>Gets or sets whether to enable film grain synthesis.</summary>
	public bool EnableFilmGrain { get; set; }

	/// <summary>Gets or sets the film grain strength (0-50).</summary>
	public int FilmGrainStrength { get; set; } = 0;

	/// <summary>Gets or sets whether to enable auto tiling for large images.</summary>
	public bool EnableAutoTiling { get; set; } = true;

	/// <summary>Gets or sets the tile size for encoding (0 = auto).</summary>
	public int TileSize { get; set; } = 0;

	/// <summary>Gets or sets whether to include EXIF metadata.</summary>
	public bool IncludeExif { get; set; } = true;

	/// <summary>Gets or sets whether to include XMP metadata.</summary>
	public bool IncludeXmp { get; set; } = true;

	/// <summary>Gets or sets whether to include ICC profile.</summary>
	public bool IncludeIccProfile { get; set; } = true;

	/// <summary>Gets or sets whether to optimize for file size.</summary>
	public bool OptimizeForSize { get; set; }

	/// <summary>Gets or sets whether to use adaptive quantization.</summary>
	public bool UseAdaptiveQuantization { get; set; } = true;

	/// <summary>Gets or sets the minimum quantizer (0-63).</summary>
	public int MinQuantizer { get; set; } = 0;

	/// <summary>Gets or sets the maximum quantizer (0-63).</summary>
	public int MaxQuantizer { get; set; } = 63;

	/// <summary>Gets or sets the minimum quantizer for alpha (0-63).</summary>
	public int MinQuantizerAlpha { get; set; } = 0;

	/// <summary>Gets or sets the maximum quantizer for alpha (0-63).</summary>
	public int MaxQuantizerAlpha { get; set; } = 63;

	/// <summary>Gets or sets whether to enable denoising.</summary>
	public bool EnableDenoising { get; set; }

	/// <summary>Gets or sets the denoising strength (0-50).</summary>
	public int DenoisingStrength { get; set; } = 0;

	/// <summary>Gets or sets whether to enable sharpening.</summary>
	public bool EnableSharpening { get; set; }

	/// <summary>Gets or sets the sharpening strength (0-7).</summary>
	public int SharpeningStrength { get; set; } = 0;

	/// <summary>Gets or sets whether to preserve animation timing.</summary>
	public bool PreserveAnimationTiming { get; set; } = true;

	/// <summary>Gets or sets the keyframe interval for animations.</summary>
	public int KeyframeInterval { get; set; } = 0;

	/// <summary>Gets or sets whether to add a preview image.</summary>
	public bool AddPreviewImage { get; set; }

	/// <summary>Gets or sets the maximum preview dimension.</summary>
	public int PreviewMaxDimension { get; set; } = 256;

	/// <summary>Creates encoding options for lossless compression.</summary>
	public static AvifEncodingOptions CreateLossless()
	{
		return new AvifEncodingOptions
		{
			IsLossless = true,
			IsAlphaLossless = true,
			Quality = AvifConstants.QualityPresets.Lossless,
			AlphaQuality = AvifConstants.QualityPresets.Lossless,
			Speed = AvifConstants.SpeedPresets.Slow,
			ChromaSubsampling = AvifChromaSubsampling.Yuv444
		};
	}

	/// <summary>Creates encoding options for web optimization.</summary>
	public static AvifEncodingOptions CreateWebOptimized()
	{
		return new AvifEncodingOptions
		{
			Quality = AvifConstants.QualityPresets.Web,
			Speed = AvifConstants.SpeedPresets.Fast,
			OptimizeForSize = true,
			ChromaSubsampling = AvifChromaSubsampling.Yuv420,
			AddPreviewImage = true,
			PreviewMaxDimension = 128
		};
	}

	/// <summary>Creates encoding options for high quality.</summary>
	public static AvifEncodingOptions CreateHighQuality()
	{
		return new AvifEncodingOptions
		{
			Quality = AvifConstants.QualityPresets.Professional,
			AlphaQuality = AvifConstants.QualityPresets.Professional,
			Speed = AvifConstants.SpeedPresets.Slow,
			ChromaSubsampling = AvifChromaSubsampling.Yuv444,
			UseAdaptiveQuantization = true
		};
	}

	/// <summary>Creates encoding options for fast encoding.</summary>
	public static AvifEncodingOptions CreateFast()
	{
		return new AvifEncodingOptions
		{
			Quality = AvifConstants.QualityPresets.Standard,
			Speed = AvifConstants.SpeedPresets.Fastest,
			EnableAutoTiling = false,
			UseAdaptiveQuantization = false
		};
	}

	/// <summary>Creates encoding options for HDR content.</summary>
	public static AvifEncodingOptions CreateHdr()
	{
		return new AvifEncodingOptions
		{
			Quality = AvifConstants.QualityPresets.Professional,
			Speed = AvifConstants.SpeedPresets.Default,
			ChromaSubsampling = AvifChromaSubsampling.Yuv444,
			IncludeIccProfile = true,
			UseAdaptiveQuantization = true
		};
	}

	/// <summary>Validates the encoding options.</summary>
	public bool Validate(out string? error)
	{
		error = null;

		if (Quality < AvifConstants.MinQuality || Quality > AvifConstants.MaxQuality)
		{
			error = $"Quality must be between {AvifConstants.MinQuality} and {AvifConstants.MaxQuality}.";
			return false;
		}

		if (AlphaQuality < AvifConstants.MinQuality || AlphaQuality > AvifConstants.MaxQuality)
		{
			error = $"Alpha quality must be between {AvifConstants.MinQuality} and {AvifConstants.MaxQuality}.";
			return false;
		}

		if (Speed < AvifConstants.MinSpeed || Speed > AvifConstants.MaxSpeed)
		{
			error = $"Speed must be between {AvifConstants.MinSpeed} and {AvifConstants.MaxSpeed}.";
			return false;
		}

		if (ThreadCount < 0 || ThreadCount > AvifConstants.Memory.MaxThreads)
		{
			error = $"Thread count must be between 0 and {AvifConstants.Memory.MaxThreads}.";
			return false;
		}

		if (FilmGrainStrength < 0 || FilmGrainStrength > 50)
		{
			error = "Film grain strength must be between 0 and 50.";
			return false;
		}

		if (MinQuantizer < 0 || MinQuantizer > 63)
		{
			error = "Minimum quantizer must be between 0 and 63.";
			return false;
		}

		if (MaxQuantizer < 0 || MaxQuantizer > 63)
		{
			error = "Maximum quantizer must be between 0 and 63.";
			return false;
		}

		if (MinQuantizer > MaxQuantizer)
		{
			error = "Minimum quantizer cannot be greater than maximum quantizer.";
			return false;
		}

		if (IsLossless && Quality != AvifConstants.QualityPresets.Lossless)
		{
			error = "Lossless mode requires quality to be 100.";
			return false;
		}

		return true;
	}
}