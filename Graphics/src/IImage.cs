// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics;

/// <summary>Represents an image object with width and height properties.</summary>
public interface IImage : IDisposable, IAsyncDisposable
{
	/// <summary>Gets and sets the width of the image.</summary>
	int Width { get; set; }

	/// <summary>Gets and sets the height of the image.</summary>
	int Height { get; set; }

	/// <summary>Gets the metadata associated with this image.</summary>
	IMetadata Metadata { get; }
}
