// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.WebPs;

/// <summary>Defines the WebP encoding presets for optimization.</summary>
public enum WebPPreset : byte
{
	/// <summary>Default preset with balanced quality and size.</summary>
	Default = 0,

	/// <summary>Optimized for pictures with lots of details.</summary>
	Picture = 1,

	/// <summary>Optimized for photographic images.</summary>
	Photo = 2,

	/// <summary>Optimized for drawings and graphics.</summary>
	Drawing = 3,

	/// <summary>Optimized for icon images.</summary>
	Icon = 4,

	/// <summary>Optimized for text content.</summary>
	Text = 5
}
