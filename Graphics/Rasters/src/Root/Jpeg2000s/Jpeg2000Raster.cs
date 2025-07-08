// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

/// <summary>Implementation of JPEG2000 raster format with wavelet-based compression and advanced features.</summary>
/// <remarks>
/// Provides comprehensive JPEG2000 support including lossless/lossy compression, multi-resolution pyramids,
/// progressive transmission, region-of-interest encoding, tiling, and geospatial metadata (GeoJP2).
/// </remarks>
public sealed class Jpeg2000Raster : Raster, IJpeg2000Raster
{
	private byte[]? _encodedData;
	private bool    _disposed;

	/// <summary>Initializes a new JPEG2000 raster with default settings.</summary>
	public Jpeg2000Raster()
	{
		InitializeDefaults();
	}

	/// <summary>Initializes a new JPEG2000 raster with specified dimensions.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="components">Number of color components (1=grayscale, 3=RGB, 4=RGBA).</param>
	public Jpeg2000Raster(int width, int height, int components = 3)
	{
		if (width <= 0)
			throw new ArgumentException("Width must be positive.", nameof(width));
		if (height <= 0)
			throw new ArgumentException("Height must be positive.", nameof(height));
		if (components <= 0 || components > Jpeg2000Constants.MaxComponents)
			throw new ArgumentException($"Components must be between 1 and {Jpeg2000Constants.MaxComponents}.", nameof(components));

		Width  = width;
		Height = height;

		_metadata.Width      = width;
		_metadata.Height     = height;
		_metadata.Components = components;

		InitializeDefaults();
	}

	/// <summary>Gets or sets the width of the image.</summary>
	public override int Width
	{
		get
		{
			ThrowIfDisposed();
			return base.Width;
		}
		set
		{
			ThrowIfDisposed();
			base.Width = value;
		}
	}

	/// <summary>Gets or sets the height of the image.</summary>
	public override int Height
	{
		get
		{
			ThrowIfDisposed();
			return base.Height;
		}
		set
		{
			ThrowIfDisposed();
			base.Height = value;
		}
	}

	private Jpeg2000Metadata _metadata = new();

	/// <inheritdoc />
	public override IMetadata Metadata => _metadata;

	/// <summary>Comprehensive JPEG2000 metadata including JP2 boxes and codestream parameters.</summary>
	Jpeg2000Metadata IJpeg2000Raster.Metadata
	{
		get => _metadata;
		set => _metadata = value;
	}

	/// <summary>Gets the JPEG2000-specific metadata.</summary>
	public Jpeg2000Metadata Jpeg2000Metadata => _metadata;

	/// <summary>Indicates if the image uses lossless compression.</summary>
	public bool IsLossless
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.IsLossless;
		}
		set
		{
			ThrowIfDisposed();
			_metadata.IsLossless = value;
			_metadata.WaveletTransform = value
				                            ? Jpeg2000Constants.WaveletTransforms.Reversible53
				                            : Jpeg2000Constants.WaveletTransforms.Irreversible97;
		}
	}

	/// <summary>Target compression ratio for lossy encoding.</summary>
	public float CompressionRatio
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.CompressionRatio;
		}
		set
		{
			ThrowIfDisposed();
			if (value <= 1.0f && !IsLossless)
				throw new ArgumentException("Compression ratio must be greater than 1.0 for lossy compression.");
			_metadata.CompressionRatio = value;
		}
	}

	/// <summary>Number of DWT decomposition levels.</summary>
	public int DecompositionLevels
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.DecompositionLevels;
		}
		set
		{
			ThrowIfDisposed();
			if (value < 0 || value > Jpeg2000Constants.MaxDecompositionLevels)
				throw new ArgumentException($"Decomposition levels must be between 0 and {Jpeg2000Constants.MaxDecompositionLevels}.");
			_metadata.DecompositionLevels = value;
		}
	}

	/// <summary>Progression order for codestream organization.</summary>
	public Jpeg2000Progression ProgressionOrder
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.ProgressionOrder;
		}
		set
		{
			ThrowIfDisposed();
			_metadata.ProgressionOrder = value;
		}
	}

	/// <summary>Tile width for tiled images.</summary>
	public int TileWidth
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.TileWidth;
		}
		set
		{
			ThrowIfDisposed();
			if (value <= 0)
				throw new ArgumentException("Tile width must be positive.");
			_metadata.TileWidth = value;
		}
	}

	/// <summary>Tile height for tiled images.</summary>
	public int TileHeight
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.TileHeight;
		}
		set
		{
			ThrowIfDisposed();
			if (value <= 0)
				throw new ArgumentException("Tile height must be positive.");
			_metadata.TileHeight = value;
		}
	}

	/// <summary>Number of quality layers for progressive transmission.</summary>
	public int QualityLayers
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.QualityLayers;
		}
		set
		{
			ThrowIfDisposed();
			if (value < Jpeg2000Constants.QualityLayers.MinLayers || value > Jpeg2000Constants.QualityLayers.MaxLayers)
				throw new ArgumentException($"Quality layers must be between {Jpeg2000Constants.QualityLayers.MinLayers} and {Jpeg2000Constants.QualityLayers.MaxLayers}.");
			_metadata.QualityLayers = value;
		}
	}

	/// <summary>Gets the number of available resolution levels.</summary>
	public int AvailableResolutionLevels
	{
		get
		{
			ThrowIfDisposed();
			return DecompositionLevels + 1;
		}
	}

	/// <summary>Current region of interest for enhanced quality encoding.</summary>
	public Rectangle? RegionOfInterest
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.RegionOfInterest;
		}
		set
		{
			ThrowIfDisposed();
			_metadata.RegionOfInterest = value;
		}
	}

	/// <summary>Quality enhancement factor for ROI.</summary>
	public float RoiQualityFactor
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.RoiQualityFactor;
		}
		set
		{
			ThrowIfDisposed();
			if (value <= 0.0f)
				throw new ArgumentException("ROI quality factor must be positive.");
			_metadata.RoiQualityFactor = value;
		}
	}

	/// <summary>Indicates if the image uses tiling.</summary>
	public bool SupportsTiling
	{
		get
		{
			ThrowIfDisposed();
			return TileWidth < Width || TileHeight < Height;
		}
	}

	/// <summary>Indicates if geospatial metadata is present.</summary>
	public bool HasGeospatialMetadata
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.GeoTiffMetadata != null ||
			       !string.IsNullOrEmpty(_metadata.GmlData) ||
			       _metadata.GeoTransform != null;
		}
	}

	/// <summary>Indicates if ICC color profile is embedded.</summary>
	public bool HasIccProfile
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.HasIccProfile && _metadata.IccProfile != null;
		}
	}

	/// <summary>Gets the total number of tiles in the image.</summary>
	public int TotalTiles
	{
		get
		{
			ThrowIfDisposed();
			return _metadata.TotalTiles;
		}
	}


	/// <summary>Initializes default settings for the raster.</summary>
	private void InitializeDefaults()
	{
		// Set default compression based on format
		if (_metadata.Components == 1)
		{
			// Grayscale images often benefit from lossless compression
			IsLossless = true;
		}
		else
		{
			// Color images may use lossy compression for better ratios
			IsLossless       = false;
			CompressionRatio = Jpeg2000Constants.DefaultCompressionRatio;
		}

		// Calculate optimal decomposition levels based on image size
		var minDimension = Math.Min(Width, Height);
		if (minDimension > 0)
		{
			var maxLevels = (int)Math.Floor(Math.Log2(minDimension / 32.0));
			DecompositionLevels = Math.Min(
				Jpeg2000Constants.DefaultDecompositionLevels,
				Math.Max(0, maxLevels) // Ensure non-negative
			);
		}
		else
			DecompositionLevels = Jpeg2000Constants.DefaultDecompositionLevels;

		// Set tile size based on image dimensions
		if (Width > 0 && Height > 0)
		{
			if (Width > 2048 || Height > 2048)
			{
				// Large images benefit from tiling
				TileWidth  = Math.Min(Jpeg2000Constants.DefaultTileSize, Width);
				TileHeight = Math.Min(Jpeg2000Constants.DefaultTileSize, Height);
			}
			else
			{
				// Small images can use single tile (tile size = image size)
				TileWidth  = Width;
				TileHeight = Height;
			}
		}
		else
		{
			// Default tile size for uninitialized dimensions
			_metadata.TileWidth  = Jpeg2000Constants.DefaultTileSize;
			_metadata.TileHeight = Jpeg2000Constants.DefaultTileSize;
		}
	}

	/// <summary>Encodes the raster image to JPEG2000 format.</summary>
	public async Task<byte[]> EncodeAsync(Jpeg2000EncodingOptions? options = null)
	{
		ThrowIfDisposed();

		options ??= new Jpeg2000EncodingOptions
		            {
			            IsLossless          = IsLossless,
			            CompressionRatio    = CompressionRatio,
			            DecompositionLevels = DecompositionLevels,
			            QualityLayers       = QualityLayers,
			            ProgressionOrder    = ProgressionOrder
		            };

		// Apply encoding options
		ApplyEncodingOptions(options);

		// Validate encoding parameters
		if (!IsValid())
			throw new InvalidOperationException("Invalid raster configuration for JPEG2000 encoding.");

		// For now, return a placeholder implementation
		// In a real implementation, this would integrate with OpenJPEG or similar library
		await Task.Yield(); // Simulate async operation

		_encodedData = CreatePlaceholderEncodedData();
		return _encodedData;
	}

	/// <summary>Decodes JPEG2000 data into the raster image.</summary>
	public async Task DecodeAsync(byte[] data, int resolutionLevel = 0, int qualityLayer = -1)
	{
		ThrowIfDisposed();

		if (data == null || data.Length == 0)
			throw new ArgumentException("JPEG2000 data cannot be null or empty.", nameof(data));

		if (resolutionLevel < 0 || resolutionLevel >= AvailableResolutionLevels)
			throw new ArgumentException($"Resolution level must be between 0 and {AvailableResolutionLevels - 1}.", nameof(resolutionLevel));

		if (qualityLayer != -1 && (qualityLayer < 0 || qualityLayer >= QualityLayers))
			throw new ArgumentException($"Quality layer must be -1 (all layers) or between 0 and {QualityLayers - 1}.", nameof(qualityLayer));

		// For now, simulate decoding
		await Task.Yield();

		_encodedData = data;
		ParsePlaceholderData(data);
	}

	/// <summary>Decodes a specific region of the JPEG2000 image.</summary>
	public async Task<byte[]> DecodeRegionAsync(Rectangle region, int resolutionLevel = 0)
	{
		ThrowIfDisposed();

		if (region.Width <= 0 || region.Height <= 0)
			throw new ArgumentException("Region dimensions must be positive.");

		if (region.X < 0 || region.Y < 0 || region.Right > Width || region.Bottom > Height)
			throw new ArgumentException("Region is outside image bounds.");

		// For now, simulate region decoding
		await Task.Yield();

		var bytesPerPixel  = GetBytesPerPixel();
		var regionDataSize = region.Width * region.Height * bytesPerPixel;

		// Scale for resolution level
		var scaleFactor = 1 << resolutionLevel;
		regionDataSize /= scaleFactor * scaleFactor;

		return new byte[regionDataSize];
	}

	/// <summary>Decodes the image at a specific resolution level.</summary>
	public async Task<byte[]> DecodeResolutionAsync(int resolutionLevel)
	{
		ThrowIfDisposed();

		if (resolutionLevel < 0 || resolutionLevel >= AvailableResolutionLevels)
			throw new ArgumentException($"Resolution level must be between 0 and {AvailableResolutionLevels - 1}.");

		await Task.Yield();

		var (width, height) = GetResolutionDimensions(resolutionLevel);
		var bytesPerPixel = GetBytesPerPixel();
		return new byte[width * height * bytesPerPixel];
	}

	/// <summary>Sets a region of interest with enhanced quality settings.</summary>
	public void SetRegionOfInterest(Rectangle roi, float qualityFactor = 2.0f)
	{
		ThrowIfDisposed();

		if (roi.X < 0 || roi.Y < 0 || roi.Right > Width || roi.Bottom > Height)
			throw new ArgumentException("ROI is outside image bounds.");

		if (qualityFactor <= 0.0f)
			throw new ArgumentException("Quality factor must be positive.");

		RegionOfInterest = roi;
		RoiQualityFactor = qualityFactor;
	}

	/// <summary>Clears the current region of interest setting.</summary>
	public void ClearRegionOfInterest()
	{
		ThrowIfDisposed();
		RegionOfInterest = null;
		RoiQualityFactor = 1.0f;
	}

	/// <summary>Gets available resolution levels for progressive decoding.</summary>
	public int[] GetAvailableResolutions()
	{
		ThrowIfDisposed();
		return Enumerable.Range(0, AvailableResolutionLevels).ToArray();
	}

	/// <summary>Gets the image dimensions at a specific resolution level.</summary>
	public (int Width, int Height) GetResolutionDimensions(int resolutionLevel)
	{
		ThrowIfDisposed();

		if (resolutionLevel < 0 || resolutionLevel >= AvailableResolutionLevels)
			throw new ArgumentException($"Resolution level must be between 0 and {AvailableResolutionLevels - 1}.");

		var scaleFactor = 1 << resolutionLevel;
		return (Width / scaleFactor, Height / scaleFactor);
	}

	/// <summary>Estimates the file size for current encoding settings.</summary>
	public long GetEstimatedFileSize()
	{
		ThrowIfDisposed();

		var baseSize = (long)Width * Height * GetBytesPerPixel();

		if (IsLossless)
		{
			// Lossless compression typically achieves 2:1 to 4:1 ratio
			return baseSize / 3;
		}
		else
		{
			// Use specified compression ratio
			return (long)(baseSize / CompressionRatio);
		}
	}

	/// <summary>Validates JPEG2000 format compliance and settings.</summary>
	public bool IsValid()
	{
		var validation = Jpeg2000Validator.Validate(this);
		return validation.IsValid;
	}

	/// <summary>Applies geospatial metadata for GeoJP2 support.</summary>
	public void ApplyGeospatialMetadata(double[] geoTransform, string coordinateSystem, byte[]? geoTiffTags = null)
	{
		ThrowIfDisposed();

		if (geoTransform == null || geoTransform.Length != 6)
			throw new ArgumentException("GeoTransform must be a 6-element array.");

		if (string.IsNullOrEmpty(coordinateSystem))
			throw new ArgumentException("Coordinate system cannot be null or empty.");

		_metadata.GeoTransform              = geoTransform;
		_metadata.CoordinateReferenceSystem = coordinateSystem;
		_metadata.GeoTiffMetadata           = geoTiffTags;
	}

	/// <summary>Applies an ICC color profile for accurate color reproduction.</summary>
	public void ApplyIccProfile(byte[] profileData)
	{
		ThrowIfDisposed();

		if (profileData == null || profileData.Length == 0)
			throw new ArgumentException("ICC profile data cannot be null or empty.");

		_metadata.IccProfile    = profileData;
		_metadata.HasIccProfile = true;
	}

	/// <summary>Adds custom metadata as UUID box.</summary>
	public void AddUuidMetadata(string uuid, byte[] data)
	{
		ThrowIfDisposed();

		if (string.IsNullOrEmpty(uuid))
			throw new ArgumentException("UUID cannot be null or empty.");

		if (data == null || data.Length == 0)
			throw new ArgumentException("Metadata data cannot be null or empty.");

		_metadata.UuidBoxes[uuid] = data;
	}

	/// <summary>Adds XML metadata box.</summary>
	public void AddXmlMetadata(string xmlContent)
	{
		ThrowIfDisposed();

		if (string.IsNullOrEmpty(xmlContent))
			throw new ArgumentException("XML content cannot be null or empty.");

		_metadata.XmlMetadata.Add(xmlContent);
	}

	/// <summary>Gets tile bounds for the specified tile index.</summary>
	public Rectangle GetTileBounds(int tileIndex)
	{
		ThrowIfDisposed();

		if (tileIndex < 0 || tileIndex >= TotalTiles)
			throw new ArgumentException($"Tile index must be between 0 and {TotalTiles - 1}.");

		var tilesAcross = _metadata.TilesAcross;
		var tileX       = tileIndex % tilesAcross;
		var tileY       = tileIndex / tilesAcross;

		var x      = tileX * TileWidth;
		var y      = tileY * TileHeight;
		var width  = Math.Min(TileWidth, Width - x);
		var height = Math.Min(TileHeight, Height - y);

		return new Rectangle(x, y, width, height);
	}

	/// <summary>Gets the tile index for the specified image coordinates.</summary>
	public int GetTileIndex(int x, int y)
	{
		ThrowIfDisposed();

		if (x < 0 || x >= Width || y < 0 || y >= Height)
			throw new ArgumentException("Coordinates are outside image bounds.");

		var tileX = x / TileWidth;
		var tileY = y / TileHeight;
		return tileY * _metadata.TilesAcross + tileX;
	}

	/// <summary>Sets the tile size for tiled image organization.</summary>
	public void SetTileSize(int tileWidth = Jpeg2000Constants.DefaultTileSize, int tileHeight = Jpeg2000Constants.DefaultTileSize)
	{
		ThrowIfDisposed();

		if (tileWidth <= 0 || tileHeight <= 0)
			throw new ArgumentException("Tile dimensions must be positive.");

		TileWidth  = Math.Min(tileWidth, Width);
		TileHeight = Math.Min(tileHeight, Height);
	}

	/// <summary>Gets the number of bytes per pixel based on components and bit depth.</summary>
	private int GetBytesPerPixel()
	{
		var bitsPerPixel = _metadata.Components * _metadata.BitDepth;
		return (bitsPerPixel + 7) / 8; // Round up to nearest byte
	}

	/// <summary>Applies encoding options to the raster configuration.</summary>
	private void ApplyEncodingOptions(Jpeg2000EncodingOptions options)
	{
		IsLossless               = options.IsLossless;
		CompressionRatio         = options.CompressionRatio;
		DecompositionLevels      = options.DecompositionLevels;
		QualityLayers            = options.QualityLayers;
		ProgressionOrder         = options.ProgressionOrder;
		_metadata.ErrorResilience = options.ErrorResilience;

		if (options.EnableTiling) SetTileSize(options.TileWidth, options.TileHeight);

		if (options.RegionOfInterest.HasValue) SetRegionOfInterest(options.RegionOfInterest.Value, options.RoiQualityFactor);
	}

	/// <summary>Creates placeholder encoded data for demonstration purposes.</summary>
	private byte[] CreatePlaceholderEncodedData()
	{
		// Create a minimal JP2 file structure
		using var stream = new MemoryStream();
		using var writer = new BinaryWriter(stream);

		// JP2 Signature box
		writer.Write((uint)12); // Box size
		writer.Write(Jpeg2000Constants.SignatureBoxType.AsSpan());
		writer.Write(Jpeg2000Constants.SignatureData.AsSpan());

		// File Type box
		writer.Write((uint)20); // Box size
		writer.Write(Jpeg2000Constants.FileTypeBoxType.AsSpan());
		writer.Write(Jpeg2000Constants.Jp2Brand.AsSpan());
		writer.Write((uint)0);                             // Minor version
		writer.Write(Jpeg2000Constants.Jp2Brand.AsSpan()); // Compatibility list

		// Placeholder codestream
		var estimatedSize = GetEstimatedFileSize();
		var remainingSize = Math.Max(100, (int)(estimatedSize - stream.Position));
		writer.Write(new byte[remainingSize]);

		return stream.ToArray();
	}

	/// <summary>Parses placeholder encoded data for demonstration purposes.</summary>
	private void ParsePlaceholderData(byte[] data)
	{
		// In a real implementation, this would parse the JP2 boxes and codestream
		// For now, just extract basic information from the data size

		if (data.Length < 32)
			throw new ArgumentException("Invalid JPEG2000 data - too small.");

		// Update metadata based on parsed information
		_metadata.ModificationTime = DateTime.UtcNow;

		// Calculate estimated dimensions based on data size
		var estimatedPixels    = data.Length * (IsLossless ? 3 : (int)CompressionRatio);
		var estimatedDimension = (int)Math.Sqrt(estimatedPixels / GetBytesPerPixel());

		if (Width == 0)
			Width   = estimatedDimension;
		if (Height == 0)
			Height = estimatedDimension;

		_metadata.Width  = Width;
		_metadata.Height = Height;
	}

	/// <summary>Throws ObjectDisposedException if the raster has been disposed.</summary>
	private void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(Jpeg2000Raster));
	}

	/// <summary>Protected dispose implementation.</summary>
	protected override void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_encodedData = null;
			_disposed    = true;
		}

		base.Dispose(disposing);
	}

	/// <summary>Async dispose implementation for large metadata.</summary>
	protected override async ValueTask DisposeAsyncCore()
	{
		if (_metadata.HasLargeMetadata)
			await _metadata.DisposeAsync().ConfigureAwait(false);

		await base.DisposeAsyncCore().ConfigureAwait(false);
	}
}
