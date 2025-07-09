// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Text;

namespace Wangkanai.Graphics;

/// <summary>
/// Abstract base class for all metadata implementations providing common functionality.
/// </summary>
public abstract class MetadataBase : IMetadata
{
	private bool _disposed;

	private const int DefaultObjectEstimate = 16;

	/// <inheritdoc />
	public abstract long EstimatedMetadataSize { get; }

	/// <inheritdoc />
	public virtual bool HasLargeMetadata
		=> EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold;

	/// <summary>
	/// Gets the base memory size for the metadata object.
	/// </summary>
	/// <returns>Base size in bytes.</returns>
	protected virtual long GetBaseMemorySize()
		=> 256; // Base object size estimate

	/// <summary>
	/// Estimates the memory size of a string using UTF-8 encoding.
	/// </summary>
	/// <param name="str">The string to estimate.</param>
	/// <returns>Estimated size in bytes.</returns>
	protected static long EstimateStringSize(string? str)
	{
		if (string.IsNullOrEmpty(str))
			return 0;

		// UTF-8 encoding estimate
		return Encoding.UTF8.GetByteCount(str);
	}

	/// <summary>
	/// Estimates the memory size of a byte array.
	/// </summary>
	/// <param name="array">The byte array to estimate.</param>
	/// <returns>Size in bytes.</returns>
	protected static long EstimateByteArraySize(byte[]? array)
		=> array?.Length ?? 0;

	/// <summary>
	/// Estimates the memory size of an array.
	/// </summary>
	/// <typeparam name="T">The array element type.</typeparam>
	/// <param name="array">The array to estimate.</param>
	/// <param name="elementSize">Size of each element in bytes.</param>
	/// <returns>Size in bytes.</returns>
	protected static long EstimateArraySize<T>(T[]? array, int elementSize)
		=> array != null ? array.Length * elementSize : 0;

	/// <summary>
	/// Estimates the memory size of a dictionary containing string values.
	/// </summary>
	/// <param name="dictionary">The dictionary to estimate.</param>
	/// <returns>Estimated size in bytes.</returns>
	protected static long EstimateDictionarySize<TKey>(Dictionary<TKey, string>? dictionary) where TKey : notnull
	{
		if (dictionary == null || dictionary.Count == 0)
			return 0;

		long size = dictionary.Count * 64; // Overhead per entry
		foreach (var value in dictionary.Values)
			size += EstimateStringSize(value);

		return size;
	}

	/// <summary>
	/// Estimates the memory size of a dictionary containing byte array values.
	/// </summary>
	/// <param name="dictionary">The dictionary to estimate.</param>
	/// <returns>Estimated size in bytes.</returns>
	protected static long EstimateDictionaryByteArraySize<TKey>(Dictionary<TKey, byte[]>? dictionary) where TKey : notnull
	{
		if (dictionary == null || dictionary.Count == 0)
			return 0;

		long size = dictionary.Count * 64; // Overhead per entry
		foreach (var value in dictionary.Values)
			size += value.Length;

		return size;
	}

	/// <summary>
	/// Estimates the memory size of a dictionary containing object values.
	/// </summary>
	/// <param name="dictionary">The dictionary to estimate.</param>
	/// <returns>Estimated size in bytes.</returns>
	protected static long EstimateDictionaryObjectSize<TKey>(Dictionary<TKey, object>? dictionary) where TKey : notnull
	{
		if (dictionary == null || dictionary.Count == 0)
			return 0;

		long size = dictionary.Count * 64; // Overhead per entry
		foreach (var value in dictionary.Values)
		{
			size += value switch
			{
				string str       => EstimateStringSize(str),
				byte[] bytes     => bytes.Length,
				int[] ints       => ints.Length * sizeof(int),
				ushort[] ushorts => ushorts.Length * sizeof(ushort),
				double[] doubles => doubles.Length * sizeof(double),
				float[] floats   => floats.Length * sizeof(float),
				_                => DefaultObjectEstimate // Default estimate for other types
			};
		}

		return size;
	}

	/// <summary>
	/// Throws an ObjectDisposedException if this instance has been disposed.
	/// </summary>
	protected void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(GetType().Name);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public virtual async ValueTask DisposeAsync()
	{
		if (HasLargeMetadata)
			await Task.Run(() => Dispose(true)).ConfigureAwait(false);
		else
			Dispose(true);

		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Releases unmanaged and - optionally - managed resources.
	/// </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
			DisposeManagedResources();

		_disposed = true;
	}

	/// <summary>
	/// When overridden in a derived class, releases managed resources.
	/// </summary>
	protected abstract void DisposeManagedResources();
}
