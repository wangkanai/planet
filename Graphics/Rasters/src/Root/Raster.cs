// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters;

/// <summary>Represents a raster image</summary>
public abstract class Raster : IRaster
{
	private bool _disposed;

	public virtual int Width  { get; set; }
	public virtual int Height { get; set; }

	/// <inheritdoc />
	public abstract IMetadata Metadata { get; }

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the managed and unmanaged resources used by the raster image.</summary>
	/// <param name="disposing">
	/// true to release both managed and unmanaged resources;
	/// false to release only unmanaged resources.
	/// </param>
	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;
	}

	/// <inheritdoc />
	public virtual async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}

	/// <summary>Core disposal logic for asynchronous disposal.</summary>
	/// <returns>A ValueTask representing the asynchronous disposal operation.</returns>
	protected virtual ValueTask DisposeAsyncCore()
	{
		// For base implementation, call synchronous disposal
		// Derived classes should override this method for custom async disposal logic
		Dispose(true);
		return ValueTask.CompletedTask;
	}
}
