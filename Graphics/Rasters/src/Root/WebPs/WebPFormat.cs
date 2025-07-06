// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.WebPs;

/// <summary>Defines the WebP format types.</summary>
public enum WebPFormat : byte
{
	/// <summary>Simple lossy format using VP8 compression.</summary>
	Simple = 0,

	/// <summary>Lossless format using VP8L compression.</summary>
	Lossless = 1,

	/// <summary>Extended format with additional features (VP8X).</summary>
	Extended = 2
}
