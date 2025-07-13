// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors.Svgs;

/// <summary>
/// Implementation of SVG vector graphics metadata.
/// Provides comprehensive metadata management for SVG documents including
/// viewBox, coordinate systems, styling, and performance optimization data.
/// </summary>
public class SvgMetadata : VectorMetadata, ISvgMetadata
{
	private bool _disposed;

	private readonly Dictionary<string, string> _namespaces;
	private readonly Dictionary<string, object> _customProperties;

	/// <summary>Initializes a new instance of the SvgMetadata class with default values.</summary>
	public SvgMetadata()
	{
		Version                   = SvgConstants.DefaultVersion;
		ViewBox                   = SvgViewBox.Default;
		ViewportWidth             = 100;
		ViewportHeight            = 100;
		CoordinateReferenceSystem = SvgConstants.DefaultCrs;
		_namespaces               = new Dictionary<string, string>(SvgConstants.StandardNamespaces);
		CreationDate              = DateTime.UtcNow;
		ModificationDate          = DateTime.UtcNow;
		IsCompressed              = false;
		CompressionLevel          = SvgConstants.DefaultCompressionLevel;
		ElementCount              = 0;
		TotalPathLength           = 0;
		ColorSpace                = SvgColorSpace.sRGB;
		_customProperties         = new Dictionary<string, object>();
	}

	/// <inheritdoc />
	public string Version { get; set; }

	/// <inheritdoc />
	public SvgViewBox ViewBox { get; set; }

	/// <inheritdoc />
	public double ViewportWidth { get; set; }

	/// <inheritdoc />
	public double ViewportHeight { get; set; }

	/// <inheritdoc />
	public string? CoordinateReferenceSystem { get; set; }

	/// <inheritdoc />
	public Dictionary<string, string> Namespaces => _namespaces;

	/// <inheritdoc />
	public new string? Title
	{
		get => base.Title;
		set => base.Title = value;
	}

	/// <inheritdoc />
	public new string? Description
	{
		get => base.Description;
		set => base.Description = value;
	}

	/// <inheritdoc />
	public DateTime CreationDate { get; set; }

	/// <inheritdoc />
	public DateTime ModificationDate { get; set; }

	/// <inheritdoc />
	public string? Creator { get; set; }

	/// <inheritdoc />
	public bool IsCompressed { get; set; }

	/// <inheritdoc />
	public int CompressionLevel { get; set; }

	/// <inheritdoc />
	public int ElementCount { get; set; }

	/// <inheritdoc />
	public double TotalPathLength { get; set; }

	/// <inheritdoc />
	public SvgColorSpace ColorSpace { get; set; }

	/// <inheritdoc />
	public Dictionary<string, object> CustomProperties
		=> _customProperties;

	/// <inheritdoc />
	public long CalculateEstimatedMetadataSize()
	{
		ThrowIfDisposed();
		var baseSize             = 1024; // Base SVG structure
		var elementSize          = ElementCount * SvgConstants.MemoryPerElement;
		var pathSize             = (long)(TotalPathLength * SvgConstants.MemoryPerPathSegment);
		var namespaceSize        = _namespaces.Count * 64;
		var customPropertiesSize = _customProperties.Count * 128;

		var totalSize = baseSize + elementSize + pathSize + namespaceSize + customPropertiesSize;

		// Add overhead for string properties
		totalSize += (Title?.Length ?? 0) * 2;
		totalSize += (Description?.Length ?? 0) * 2;
		totalSize += (Creator?.Length ?? 0) * 2;
		totalSize += (CoordinateReferenceSystem?.Length ?? 0) * 2;

		return totalSize;
	}

	/// <inheritdoc />
	public bool ValidateCompliance()
	{
		ThrowIfDisposed();
		// Validate SVG version
		if (!SvgConstants.SupportedVersions.Contains(Version))
			return false;

		// Validate viewBox dimensions
		if (ViewBox.Width <= 0 || ViewBox.Height <= 0)
			return false;

		// Validate viewport dimensions
		if (ViewportWidth <= 0 || ViewportHeight <= 0)
			return false;

		// Validate compression level
		if (IsCompressed && (CompressionLevel < 1 || CompressionLevel > SvgConstants.MaxCompressionLevel))
			return false;

		// Validate element count
		if (ElementCount < 0)
			return false;

		// Validate path length
		if (TotalPathLength < 0)
			return false;

		return true;
	}

	/// <inheritdoc />
	public override IMetadata Clone() => CloneVector();

	/// <inheritdoc />
	public override IVectorMetadata CloneVector()
	{
		var clone = new SvgMetadata();
		CopyVectorTo(clone);

		// Copy SVG-specific properties
		clone.Version = Version;
		clone.ViewBox = ViewBox;
		clone.ViewportWidth = ViewportWidth;
		clone.ViewportHeight = ViewportHeight;
		clone.CoordinateReferenceSystem = CoordinateReferenceSystem;
		clone.Creator = Creator;
		clone.IsCompressed = IsCompressed;
		clone.CompressionLevel = CompressionLevel;
		clone.ElementCount = ElementCount;
		clone.TotalPathLength = TotalPathLength;
		clone.ColorSpace = ColorSpace;

		// Deep copy collections
		foreach (var (key, value) in _namespaces)
			clone._namespaces[key] = value;
		foreach (var (key, value) in _customProperties)
			clone._customProperties[key] = value;

		return clone;
	}

	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();

		Version                   = SvgConstants.DefaultVersion;
		ViewBox                   = SvgViewBox.Default;
		ViewportWidth             = 100;
		ViewportHeight            = 100;
		CoordinateReferenceSystem = SvgConstants.DefaultCrs;
		_namespaces.Clear();
		foreach (var (key, value) in SvgConstants.StandardNamespaces)
			_namespaces[key] = value;

		Title            = null;
		Description      = null;
		CreationDate     = DateTime.UtcNow;
		ModificationDate = DateTime.UtcNow;
		Creator          = null;
		IsCompressed     = false;
		CompressionLevel = SvgConstants.DefaultCompressionLevel;
		ElementCount     = 0;
		TotalPathLength  = 0;
		ColorSpace       = SvgColorSpace.sRGB;
		_customProperties.Clear();
	}

	/// <summary>Gets whether this metadata represents a large SVG that benefits from optimization.</summary>
	public bool IsLargeSvg
		=> CalculateEstimatedMetadataSize() > SvgConstants.LargeSvgThreshold;

	/// <summary>Gets whether this metadata represents a very large SVG requiring streaming.</summary>
	public bool IsVeryLargeSvg
		=> CalculateEstimatedMetadataSize() > SvgConstants.VeryLargeSvgThreshold;

	/// <summary>Gets whether this SVG requires performance optimization based on element count.</summary>
	public bool RequiresOptimization
		=> ElementCount > SvgConstants.PerformanceOptimizationThreshold;

	/// <inheritdoc />
	public override bool HasLargeMetadata
		=> IsVeryLargeSvg;

	/// <inheritdoc />
	public override long EstimatedMetadataSize
		=> CalculateEstimatedMetadataSize();

	/// <inheritdoc />
	public long CalculateEstimatedMemoryUsage() => CalculateEstimatedMetadataSize();


	/// <inheritdoc />
	public override async ValueTask DisposeAsync()
	{
		// For large metadata, use async disposal with yielding
		if (IsVeryLargeSvg)
			await DisposeAsyncLarge();
		else
			await base.DisposeAsync();

		GC.SuppressFinalize(this);
	}

	/// <summary>Async disposal for very large SVG metadata.</summary>
	private async ValueTask DisposeAsyncLarge()
	{
		if (_disposed)
			return;

		// Clear collections in batches to avoid blocking
		if (_namespaces.Count > 100)
		{
			var keys = _namespaces.Keys.ToList();
			for (var i = 0; i < keys.Count; i += 50)
			{
				var batch = keys.Skip(i).Take(50);
				foreach (var key in batch)
					_namespaces.Remove(key);

				await Task.Yield();
			}
		}
		else
		{
			_namespaces.Clear();
		}

		if (_customProperties.Count > 100)
		{
			var keys = _customProperties.Keys.ToList();
			for (var i = 0; i < keys.Count; i += 50)
			{
				var batch = keys.Skip(i).Take(50);
				foreach (var key in batch)
					_customProperties.Remove(key);

				await Task.Yield();
			}
		}
		else
		{
			_customProperties.Clear();
		}

		// Suggest garbage collection for very large metadata
		if (CalculateEstimatedMetadataSize() > 10_000_000) // > 10MB
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		_disposed = true;
	}

	/// <inheritdoc />
	protected override void DisposeManagedResources()
	{
		_namespaces.Clear();
		_customProperties.Clear();
		_disposed = true;
	}
}
