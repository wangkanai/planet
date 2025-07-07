// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Provides a base implementation for raster image encoding options.
/// </summary>
public abstract class RasterEncodingOptionsBase : IRasterEncodingOptions
{
	/// <inheritdoc />
	public virtual int Quality { get; set; } = RasterConstants.QualityPresets.Standard;

	/// <inheritdoc />
	public virtual int Speed { get; set; } = RasterConstants.SpeedPresets.Default;

	/// <inheritdoc />
	public virtual bool IsLossless { get; set; }

	/// <inheritdoc />
	public virtual int ThreadCount { get; set; } = 0; // Auto-detect

	/// <inheritdoc />
	public virtual ChromaSubsampling ChromaSubsampling { get; set; } = ChromaSubsampling.Yuv420;

	/// <inheritdoc />
	public virtual bool PreserveMetadata { get; set; } = true;

	/// <inheritdoc />
	public virtual bool PreserveColorProfile { get; set; } = true;

	/// <inheritdoc />
	public virtual int MaxPixelBufferSizeMB { get; set; } = RasterConstants.Memory.DefaultPixelBufferSizeMB;

	/// <inheritdoc />
	public virtual int MaxMetadataBufferSizeMB { get; set; } = RasterConstants.Memory.DefaultMetadataBufferSizeMB;

	/// <inheritdoc />
	public virtual bool Validate(out string? error)
	{
		error = null;

		// Validate quality bounds
		if (!ValidateQuality(out error))
			return false;

		// Validate speed bounds
		if (!ValidateSpeed(out error))
			return false;

		// Validate thread count
		if (!ValidateThreadCount(out error))
			return false;

		// Validate memory constraints
		if (!ValidateMemoryConstraints(out error))
			return false;

		// Validate lossless mode constraints
		if (!ValidateLosslessMode(out error))
			return false;

		// Perform format-specific validation
		return ValidateFormatSpecific(out error);
	}

	/// <inheritdoc />
	public abstract IRasterEncodingOptions Clone();

	/// <summary>
	/// Creates default encoding options.
	/// </summary>
	/// <typeparam name="T">The concrete type of encoding options.</typeparam>
	/// <returns>A new instance with default settings.</returns>
	public static T CreateDefault<T>() where T : RasterEncodingOptionsBase, new()
	{
		return new T();
	}

	/// <summary>
	/// Creates encoding options for lossless compression.
	/// </summary>
	/// <typeparam name="T">The concrete type of encoding options.</typeparam>
	/// <returns>A new instance configured for lossless encoding.</returns>
	public static T CreateLossless<T>() where T : RasterEncodingOptionsBase, new()
	{
		return new T
		{
			IsLossless = true,
			Quality = RasterConstants.QualityPresets.Lossless,
			Speed = RasterConstants.SpeedPresets.Slow,
			ChromaSubsampling = ChromaSubsampling.Yuv444
		};
	}

	/// <summary>
	/// Creates encoding options optimized for web use.
	/// </summary>
	/// <typeparam name="T">The concrete type of encoding options.</typeparam>
	/// <returns>A new instance configured for web optimization.</returns>
	public static T CreateWebOptimized<T>() where T : RasterEncodingOptionsBase, new()
	{
		return new T
		{
			Quality = RasterConstants.QualityPresets.Web,
			Speed = RasterConstants.SpeedPresets.Fast,
			ChromaSubsampling = ChromaSubsampling.Yuv420,
			PreserveMetadata = false,
			PreserveColorProfile = false
		};
	}

	/// <summary>
	/// Creates encoding options for high quality output.
	/// </summary>
	/// <typeparam name="T">The concrete type of encoding options.</typeparam>
	/// <returns>A new instance configured for high quality.</returns>
	public static T CreateHighQuality<T>() where T : RasterEncodingOptionsBase, new()
	{
		return new T
		{
			Quality = RasterConstants.QualityPresets.Professional,
			Speed = RasterConstants.SpeedPresets.Slow,
			ChromaSubsampling = ChromaSubsampling.Yuv444,
			PreserveMetadata = true,
			PreserveColorProfile = true
		};
	}

	/// <summary>
	/// Creates encoding options for fast encoding.
	/// </summary>
	/// <typeparam name="T">The concrete type of encoding options.</typeparam>
	/// <returns>A new instance configured for fast encoding.</returns>
	public static T CreateFast<T>() where T : RasterEncodingOptionsBase, new()
	{
		return new T
		{
			Quality = RasterConstants.QualityPresets.Standard,
			Speed = RasterConstants.SpeedPresets.Fastest,
			ChromaSubsampling = ChromaSubsampling.Yuv420
		};
	}

	/// <summary>
	/// Validates the quality setting.
	/// </summary>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the quality is valid, false otherwise.</returns>
	protected virtual bool ValidateQuality(out string? error)
	{
		error = null;
		if (Quality < RasterConstants.MinQuality || Quality > RasterConstants.MaxQuality)
		{
			error = $"Quality must be between {RasterConstants.MinQuality} and {RasterConstants.MaxQuality}.";
			return false;
		}
		return true;
	}

	/// <summary>
	/// Validates the speed setting.
	/// </summary>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the speed is valid, false otherwise.</returns>
	protected virtual bool ValidateSpeed(out string? error)
	{
		error = null;
		if (Speed < RasterConstants.MinSpeed || Speed > RasterConstants.MaxSpeed)
		{
			error = $"Speed must be between {RasterConstants.MinSpeed} and {RasterConstants.MaxSpeed}.";
			return false;
		}
		return true;
	}

	/// <summary>
	/// Validates the thread count setting.
	/// </summary>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the thread count is valid, false otherwise.</returns>
	protected virtual bool ValidateThreadCount(out string? error)
	{
		error = null;
		if (ThreadCount < 0 || ThreadCount > RasterConstants.Memory.MaxThreads)
		{
			error = $"Thread count must be between 0 and {RasterConstants.Memory.MaxThreads}.";
			return false;
		}
		return true;
	}

	/// <summary>
	/// Validates memory constraint settings.
	/// </summary>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the memory constraints are valid, false otherwise.</returns>
	protected virtual bool ValidateMemoryConstraints(out string? error)
	{
		error = null;
		
		if (MaxPixelBufferSizeMB < RasterConstants.Memory.MinPixelBufferSizeMB ||
		    MaxPixelBufferSizeMB > RasterConstants.Memory.MaxPixelBufferSizeMB)
		{
			error = $"Max pixel buffer size must be between {RasterConstants.Memory.MinPixelBufferSizeMB} and {RasterConstants.Memory.MaxPixelBufferSizeMB} MB.";
			return false;
		}

		if (MaxMetadataBufferSizeMB < RasterConstants.Memory.MinMetadataBufferSizeMB ||
		    MaxMetadataBufferSizeMB > RasterConstants.Memory.MaxMetadataBufferSizeMB)
		{
			error = $"Max metadata buffer size must be between {RasterConstants.Memory.MinMetadataBufferSizeMB} and {RasterConstants.Memory.MaxMetadataBufferSizeMB} MB.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates lossless mode constraints.
	/// </summary>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the lossless mode settings are valid, false otherwise.</returns>
	protected virtual bool ValidateLosslessMode(out string? error)
	{
		error = null;
		if (IsLossless && Quality != RasterConstants.QualityPresets.Lossless)
		{
			error = "Lossless mode requires quality to be 100.";
			return false;
		}
		return true;
	}

	/// <summary>
	/// Performs format-specific validation.
	/// </summary>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the format-specific settings are valid, false otherwise.</returns>
	protected abstract bool ValidateFormatSpecific(out string? error);

	/// <summary>
	/// Copies base properties to another instance.
	/// </summary>
	/// <param name="target">The target instance to copy to.</param>
	protected virtual void CopyTo(RasterEncodingOptionsBase target)
	{
		ArgumentNullException.ThrowIfNull(target);

		target.Quality = Quality;
		target.Speed = Speed;
		target.IsLossless = IsLossless;
		target.ThreadCount = ThreadCount;
		target.ChromaSubsampling = ChromaSubsampling;
		target.PreserveMetadata = PreserveMetadata;
		target.PreserveColorProfile = PreserveColorProfile;
		target.MaxPixelBufferSizeMB = MaxPixelBufferSizeMB;
		target.MaxMetadataBufferSizeMB = MaxMetadataBufferSizeMB;
	}
}