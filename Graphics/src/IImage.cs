// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics;

/// <summary>Represents an image object with width and height properties.</summary>
public interface IImage : IDisposable, IAsyncDisposable
{
	/// <summary>Gets and sets the width of the image.</summary>
	int Width { get; set; }

	/// <summary>Gets and sets the height of the image.</summary>
	int Height { get; set; }

	/// <summary>Gets a value indicating whether the image has large metadata that benefits from async disposal.</summary>
	bool HasLargeMetadata { get; }

	/// <summary>Gets the estimated size of metadata in bytes.</summary>
	long EstimatedMetadataSize { get; }
}
