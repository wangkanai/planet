// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Webps;

/// <summary>Represents metadata information for WebP images.</summary>
public class WebPMetadata
{
	/// <summary>Gets or sets the ICC color profile data.</summary>
	public ReadOnlyMemory<byte> IccProfile { get; set; }

	/// <summary>Gets or sets the EXIF metadata.</summary>
	public ReadOnlyMemory<byte> ExifData { get; set; }

	/// <summary>Gets or sets the XMP metadata.</summary>
	public ReadOnlyMemory<byte> XmpData { get; set; }

	/// <summary>Gets or sets the creation date and time.</summary>
	public DateTime? CreationDateTime { get; set; }

	/// <summary>Gets or sets the software that created the image.</summary>
	public string? Software { get; set; }

	/// <summary>Gets or sets the image description.</summary>
	public string? Description { get; set; }

	/// <summary>Gets or sets the copyright information.</summary>
	public string? Copyright { get; set; }

	/// <summary>Gets or sets the artist or creator name.</summary>
	public string? Artist { get; set; }

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