// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.WebPs;

/// <summary>Represents metadata information for WebP images.</summary>
public class WebPMetadata : RasterMetadataBase
{
	/// <summary>Gets or sets the WebP-specific ICC profile data.</summary>
	public new ReadOnlyMemory<byte> IccProfile { get; set; }

	/// <summary>Gets or sets the ICC profile as byte array for base class compatibility.</summary>
	byte[]? IRasterMetadata.IccProfile
	{
		get => IccProfile.IsEmpty ? null : IccProfile.ToArray();
		set => IccProfile = value ?? ReadOnlyMemory<byte>.Empty;
	}

	/// <summary>Gets or sets the WebP-specific EXIF data.</summary>
	public new ReadOnlyMemory<byte> ExifData { get; set; }

	/// <summary>Gets or sets the EXIF data as byte array for base class compatibility.</summary>
	byte[]? IRasterMetadata.ExifData
	{
		get => ExifData.IsEmpty ? null : ExifData.ToArray();
		set => ExifData = value ?? ReadOnlyMemory<byte>.Empty;
	}

	/// <summary>Gets or sets the WebP-specific XMP data.</summary>
	public ReadOnlyMemory<byte> XmpDataBytes { get; set; }

	/// <summary>Gets or sets the XMP data as string for base class compatibility.</summary>
	public override string? XmpData
	{
		get => XmpDataBytes.IsEmpty ? null : System.Text.Encoding.UTF8.GetString(XmpDataBytes.Span);
		set => XmpDataBytes = value == null ? ReadOnlyMemory<byte>.Empty : System.Text.Encoding.UTF8.GetBytes(value);
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
			if (!IccProfile.IsEmpty)
				size += IccProfile.Length;
			if (!ExifData.IsEmpty)
				size += ExifData.Length;
			if (!XmpDataBytes.IsEmpty)
				size += XmpDataBytes.Length;

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
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (HasLargeMetadata)
		{
			// For large WebP metadata, clear in stages with yielding
			if (!IccProfile.IsEmpty)
			{
				await Task.Yield();
				IccProfile = ReadOnlyMemory<byte>.Empty;
			}

			if (!ExifData.IsEmpty)
			{
				await Task.Yield();
				ExifData = ReadOnlyMemory<byte>.Empty;
			}

			if (!XmpDataBytes.IsEmpty)
			{
				await Task.Yield();
				XmpDataBytes = ReadOnlyMemory<byte>.Empty;
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

	/// <summary>Releases unmanaged and - optionally - managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Clear WebP-specific managed resources
			IccProfile = ReadOnlyMemory<byte>.Empty;
			ExifData   = ReadOnlyMemory<byte>.Empty;
			XmpDataBytes = ReadOnlyMemory<byte>.Empty;
			CustomChunks.Clear();
			AnimationFrames.Clear();
		}
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
