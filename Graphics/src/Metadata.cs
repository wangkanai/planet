// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Text;

namespace Wangkanai.Graphics;

/// <summary>
/// Abstract base class for all metadata implementations providing common functionality.
/// </summary>
public abstract class Metadata : IMetadata
{
	private bool _disposed;

	private const int DefaultObjectEstimate = 16;

	/// <inheritdoc />
	public int Width { get; set; }

	/// <inheritdoc />
	public int Height { get; set; }

	/// <summary>Gets or sets the image/document title.</summary>
	public string? Title { get; set; }

	/// <summary>Gets or sets the orientation of the content.</summary>
	public int? Orientation { get; set; }

	/// <summary>Gets or sets the author or artist name.</summary>
	public string? Author { get; set; }

	/// <summary>Gets or sets the copyright information.</summary>
	public string? Copyright { get; set; }

	/// <summary>Gets or sets the description.</summary>
	public string? Description { get; set; }

	/// <summary>Gets or sets the software used to create or modify the content.</summary>
	public string? Software { get; set; }

	/// <summary>Gets or sets the creation date and time.</summary>
	public DateTime? CreationTime { get; set; }

	/// <summary>Gets or sets the modification date and time.</summary>
	public DateTime? ModificationTime { get; set; }

	/// <inheritdoc />
	public abstract long EstimatedMetadataSize { get; }

	/// <inheritdoc />
	public virtual bool HasLargeMetadata
		=> EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold;

	/// <summary>
	/// Gets the base memory size for the metadata object.
	/// </summary>
	/// <returns>Base size in bytes.</returns>
	protected long GetBaseMemorySize()
	{
		long size = 256; // Base object size estimate

		// Add sizes for common string properties
		size += EstimateStringSize(Author);
		size += EstimateStringSize(Copyright);
		size += EstimateStringSize(Description);
		size += EstimateStringSize(Software);
		size += EstimateStringSize(Title);

		// Add sizes for basic properties
		size += sizeof(int) * 2;            // Width and Height
		size += 16 * 2;                     // CreationTime and ModificationTime (estimated)
		size += sizeof(int) + sizeof(bool); // Orientation (estimated, accounting for nullable overhead)

		return size;
	}

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
	protected static long EstimateDictionarySize<TKey>(Dictionary<TKey, string>? dictionary)
		where TKey : notnull
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

	/// <summary>
	/// Creates a deep copy of the metadata.
	/// </summary>
	/// <returns>A new instance with the same values.</returns>
	public abstract IMetadata Clone();

	/// <summary>
	/// Validates the metadata for compliance with format specifications.
	/// </summary>
	/// <returns>True if metadata is valid, false otherwise.</returns>
	public virtual bool ValidateMetadata()
	{
		ThrowIfDisposed();

		// Basic validation - dimensions should be positive
		if (Width < 0 || Height < 0)
			return false;

		// Orientation should be within valid range (1-8 for EXIF)
		if (Orientation.HasValue && (Orientation < 1 || Orientation > 8))
			return false;

		return true;
	}

	/// <summary>
	/// Clears all metadata values to their defaults.
	/// </summary>
	public virtual void Clear()
	{
		ThrowIfDisposed();

		Width            = 0;
		Height           = 0;
		Author           = null;
		Copyright        = null;
		Description      = null;
		Software         = null;
		Title            = null;
		Orientation      = null;
		CreationTime     = null;
		ModificationTime = null;
	}

	/// <summary>
	/// Copies base metadata properties from this instance to another.
	/// </summary>
	/// <param name="target">The target metadata instance.</param>
	protected void CopyBaseTo(Metadata target)
	{
		target.Width            = Width;
		target.Height           = Height;
		target.Author           = Author;
		target.Copyright        = Copyright;
		target.Description      = Description;
		target.Software         = Software;
		target.Title            = Title;
		target.Orientation      = Orientation;
		target.CreationTime     = CreationTime;
		target.ModificationTime = ModificationTime;
	}
}
