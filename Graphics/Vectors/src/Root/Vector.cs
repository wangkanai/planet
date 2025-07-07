// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors;

/// <summary>Represents a vector image</summary>
public abstract class Vector : IVector
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

	/// <summary>Releases the managed and unmanaged resources used by the vector image.</summary>
	/// <param name="disposing">
	/// true to release both managed and unmanaged resources;
	/// false to release only unmanaged resources.
	/// </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				// Dispose managed resources here
				// Derived classes should override this method to dispose their specific resources
			}

			// Dispose unmanaged resources here if any
			// This should be done regardless of the disposing parameter

			_disposed = true;
		}
	}

	/// <summary>Throws an ObjectDisposedException if the object has been disposed.</summary>
	protected void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(GetType().Name);
	}
}
