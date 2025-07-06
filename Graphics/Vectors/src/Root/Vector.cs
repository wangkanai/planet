// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors;

/// <summary>Represents a vector image</summary>
public class Vector : IVector
{
	public int Width  { get; set; }
	public int Height { get; set; }

	/// <inheritdoc />
	public virtual bool HasLargeMetadata => EstimatedMetadataSize > 1_000_000; // 1MB threshold

	/// <inheritdoc />
	public virtual long EstimatedMetadataSize => 0; // Base vector class has no metadata

	public void Dispose()
	{
		// Implementation for resource cleanup
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public virtual async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}

	/// <summary>Core disposal logic for asynchronous disposal.</summary>
	protected virtual ValueTask DisposeAsyncCore()
	{
		// Base implementation - just call synchronous disposal
		Dispose(false);
		return ValueTask.CompletedTask;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Free managed resources here if any
		}

		// Free unmanaged resources here if any
	}
}
