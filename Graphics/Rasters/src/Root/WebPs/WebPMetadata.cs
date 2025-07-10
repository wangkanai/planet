// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.WebPs;

/// <summary>Represents metadata information for WebP images.</summary>
public class WebPMetadata : RasterMetadata
{
	private ReadOnlyMemory<byte> _iccProfileMemory;
	
	/// <summary>Gets or sets the ICC profile data.</summary>
	public new byte[]? IccProfile
	{
		get => _iccProfileMemory.IsEmpty ? null : _iccProfileMemory.ToArray();
		set
		{
			_iccProfileMemory = value ?? ReadOnlyMemory<byte>.Empty;
			base.IccProfile = value;
		}
	}
	
	/// <summary>Gets or sets the ICC profile as ReadOnlyMemory for WebP operations.</summary>
	internal ReadOnlyMemory<byte> IccProfileMemory
	{
		get => _iccProfileMemory;
		set
		{
			_iccProfileMemory = value;
			base.IccProfile = value.IsEmpty ? null : value.ToArray();
		}
	}

	private ReadOnlyMemory<byte> _exifDataMemory;
	
	/// <summary>Gets or sets the EXIF data.</summary>
	public new byte[]? ExifData
	{
		get => _exifDataMemory.IsEmpty ? null : _exifDataMemory.ToArray();
		set
		{
			_exifDataMemory = value ?? ReadOnlyMemory<byte>.Empty;
			base.ExifData = value;
		}
	}
	
	/// <summary>Gets or sets the EXIF data as ReadOnlyMemory for WebP operations.</summary>
	internal ReadOnlyMemory<byte> ExifDataMemory
	{
		get => _exifDataMemory;
		set
		{
			_exifDataMemory = value;
			base.ExifData = value.IsEmpty ? null : value.ToArray();
		}
	}

	private ReadOnlyMemory<byte> _xmpDataMemory;
	
	/// <summary>Gets or sets the XMP data.</summary>
	public new string? XmpData
	{
		get => _xmpDataMemory.IsEmpty ? null : System.Text.Encoding.UTF8.GetString(_xmpDataMemory.Span);
		set
		{
			_xmpDataMemory = value == null ? ReadOnlyMemory<byte>.Empty : System.Text.Encoding.UTF8.GetBytes(value);
			base.XmpData = value;
		}
	}
	
	/// <summary>Gets or sets the XMP data as ReadOnlyMemory for WebP operations.</summary>
	internal ReadOnlyMemory<byte> XmpDataMemory
	{
		get => _xmpDataMemory;
		set
		{
			_xmpDataMemory = value;
			base.XmpData = value.IsEmpty ? null : System.Text.Encoding.UTF8.GetString(value.Span);
		}
	}

	/// <summary>Gets or sets the WebP-specific creation date and time.</summary>
	public DateTime? CreationDateTime
	{
		get => CreationTime;
		set => CreationTime = value;
	}

	// Note: Software, Description, Copyright are inherited from base class

	/// <summary>Gets or sets the artist or creator name.</summary>
	/// <remarks>Maps to the Author property from base class for backward compatibility.</remarks>
	public string? Artist
	{
		get => Author;
		set => Author = value;
	}

	/// <summary>Gets or sets the image title.</summary>
	public string? Title { get; set; }

	/// <summary>Gets or sets custom metadata chunks.</summary>
	public Dictionary<string, ReadOnlyMemory<byte>> CustomChunks { get; set; } = new();

	/// <summary>Gets or sets a value indicating whether the image has animation.</summary>
	public bool HasAnimation { get; set; }

	/// <summary>Gets or sets the number of animation loops (0 = infinite).</summary>
	public ushort AnimationLoops { get; set; }

	/// <summary>Gets or sets the background color for animations.</summary>
	public uint BackgroundColor { get; set; } = WebPConstants.DefaultBackgroundColor;

	/// <summary>Gets or sets the animation frames.</summary>
	public List<WebPAnimationFrame> AnimationFrames { get; set; } = new();

	/// <summary>Gets or sets a value indicating whether the image uses the extended format.</summary>
	public bool IsExtended { get; set; }

	/// <summary>Gets or sets a value indicating whether the image has an alpha channel.</summary>
	public bool HasAlpha { get; set; }

	/// <summary>Gets or sets a value indicating whether the image has an ICC profile.</summary>
	public bool HasIccProfile { get; set; }

	/// <summary>Gets or sets a value indicating whether the image has EXIF data.</summary>
	public bool HasExif { get; set; }

	/// <summary>Gets or sets a value indicating whether the image has XMP data.</summary>
	public bool HasXmp { get; set; }

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			var size = base.EstimatedMetadataSize;

			// Add size of WebP-specific metadata components
			if (!_iccProfileMemory.IsEmpty)
				size += _iccProfileMemory.Length;
			if (!_exifDataMemory.IsEmpty)
				size += _exifDataMemory.Length;
			if (!_xmpDataMemory.IsEmpty)
				size += _xmpDataMemory.Length;

			// Add size of custom chunks
			foreach (var chunk in CustomChunks.Values)
				size += chunk.Length;

			// Add estimated size of animation frames
			if (HasAnimation)
			{
				foreach (var frame in AnimationFrames)
					size += frame.Data.Length;
			}
			
			// Add text metadata
			size += EstimateStringSize(Title);

			return size;
		}
	}

	/// <inheritdoc />
	protected override void DisposeManagedResources()
	{
		base.DisposeManagedResources();
		
		// Clear WebP-specific resources
		_iccProfileMemory = ReadOnlyMemory<byte>.Empty;
		_exifDataMemory = ReadOnlyMemory<byte>.Empty;
		_xmpDataMemory = ReadOnlyMemory<byte>.Empty;
		CustomChunks.Clear();
		AnimationFrames.Clear();
	}

	/// <inheritdoc />
	public override async ValueTask DisposeAsync()
	{
		if (HasLargeMetadata)
		{
			// For large WebP metadata, clear in stages with yielding
			if (!_iccProfileMemory.IsEmpty)
			{
				await Task.Yield();
				_iccProfileMemory = ReadOnlyMemory<byte>.Empty;
			}

			if (!_exifDataMemory.IsEmpty)
			{
				await Task.Yield();
				_exifDataMemory = ReadOnlyMemory<byte>.Empty;
			}

			if (!_xmpDataMemory.IsEmpty)
			{
				await Task.Yield();
				_xmpDataMemory = ReadOnlyMemory<byte>.Empty;
			}

			// Clear animation frames in batches for large collections
			if (AnimationFrames.Count > ImageConstants.DisposalBatchSize)
			{
				var batchSize = 50;
				for (var i = 0; i < AnimationFrames.Count; i += batchSize)
				{
					var endIndex = Math.Min(i + batchSize, AnimationFrames.Count);

					for (var j = i; j < endIndex; j++) AnimationFrames[j].Data = ReadOnlyMemory<byte>.Empty;
					// Yield control after each batch
					await Task.Yield();
				}
			}

			await Task.Yield();
			AnimationFrames.Clear();

			await Task.Yield();
			CustomChunks.Clear();
		}
		else
		{
			// For small metadata, use synchronous disposal
			Dispose(true);
		}
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public override IMetadata Clone() => CloneRaster();
	
	/// <inheritdoc />
	public override IRasterMetadata CloneRaster()
	{
		var clone = new WebPMetadata();
		CopyRasterTo(clone);
		
		// Copy WebP-specific properties
		clone.IccProfileMemory = IccProfileMemory;
		clone.ExifDataMemory = ExifDataMemory;
		clone.XmpDataMemory = XmpDataMemory;
		clone.Title = Title;
		clone.HasAnimation = HasAnimation;
		clone.AnimationLoops = AnimationLoops;
		clone.BackgroundColor = BackgroundColor;
		clone.IsExtended = IsExtended;
		clone.HasAlpha = HasAlpha;
		clone.HasIccProfile = HasIccProfile;
		clone.HasExif = HasExif;
		clone.HasXmp = HasXmp;
		
		// Deep copy collections
		foreach (var kvp in CustomChunks)
			clone.CustomChunks[kvp.Key] = kvp.Value;
		foreach (var frame in AnimationFrames)
			clone.AnimationFrames.Add(frame.Clone());
		
		return clone;
	}
	
	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();
		
		// Clear WebP-specific properties
		_iccProfileMemory = ReadOnlyMemory<byte>.Empty;
		_exifDataMemory = ReadOnlyMemory<byte>.Empty;
		_xmpDataMemory = ReadOnlyMemory<byte>.Empty;
		Title = null;
		HasAnimation = false;
		AnimationLoops = 0;
		BackgroundColor = WebPConstants.DefaultBackgroundColor;
		IsExtended = false;
		HasAlpha = false;
		HasIccProfile = false;
		HasExif = false;
		HasXmp = false;
		CustomChunks.Clear();
		AnimationFrames.Clear();
	}
}

/// <summary>Represents an animation frame in a WebP image.</summary>
public class WebPAnimationFrame
{
	/// <summary>Gets or sets the X offset of the frame.</summary>
	public ushort OffsetX { get; set; }

	/// <summary>Gets or sets the Y offset of the frame.</summary>
	public ushort OffsetY { get; set; }

	/// <summary>Gets or sets the width of the frame.</summary>
	public ushort Width { get; set; }

	/// <summary>Gets or sets the height of the frame.</summary>
	public ushort Height { get; set; }

	/// <summary>Gets or sets the duration of the frame in milliseconds.</summary>
	public uint Duration { get; set; }

	/// <summary>Gets or sets the disposal method for the frame.</summary>
	public WebPDisposalMethod DisposalMethod { get; set; }

	/// <summary>Gets or sets the blending method for the frame.</summary>
	public WebPBlendingMethod BlendingMethod { get; set; }

	/// <summary>Gets or sets the frame data.</summary>
	public ReadOnlyMemory<byte> Data { get; set; }

	/// <summary>Creates a deep copy of this animation frame.</summary>
	/// <returns>A new instance with the same values.</returns>
	public WebPAnimationFrame Clone()
	{
		return new WebPAnimationFrame
		{
			OffsetX = OffsetX,
			OffsetY = OffsetY,
			Width = Width,
			Height = Height,
			Duration = Duration,
			DisposalMethod = DisposalMethod,
			BlendingMethod = BlendingMethod,
			Data = Data
		};
	}
}

/// <summary>Defines the disposal methods for animation frames.</summary>
public enum WebPDisposalMethod : byte
{
	/// <summary>Do not dispose. The frame is left in place.</summary>
	None = 0,

	/// <summary>Dispose to background. The frame's region is cleared.</summary>
	Background = 1
}

/// <summary>Defines the blending methods for animation frames.</summary>
public enum WebPBlendingMethod : byte
{
	/// <summary>Use alpha blending.</summary>
	AlphaBlend = 0,

	/// <summary>Do not blend. Overwrite the canvas.</summary>
	NoBlend = 1
}
