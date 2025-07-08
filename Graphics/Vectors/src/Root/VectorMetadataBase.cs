// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors;

/// <summary>
/// Base implementation of vector metadata with common properties and functionality.
/// </summary>
public abstract class VectorMetadataBase : MetadataBase, IVectorMetadata
{
	/// <summary>
	/// Gets or sets the viewbox width.
	/// </summary>
	public virtual double ViewBoxWidth { get; set; }

	/// <summary>
	/// Gets or sets the viewbox height.
	/// </summary>
	public virtual double ViewBoxHeight { get; set; }

	/// <summary>
	/// Gets or sets the viewbox X coordinate.
	/// </summary>
	public virtual double ViewBoxX { get; set; }

	/// <summary>
	/// Gets or sets the viewbox Y coordinate.
	/// </summary>
	public virtual double ViewBoxY { get; set; }

	/// <summary>
	/// Gets or sets the title of the vector graphic.
	/// </summary>
	public virtual string? Title { get; set; }

	/// <summary>
	/// Gets or sets the description of the vector graphic.
	/// </summary>
	public virtual string? Description { get; set; }

	/// <summary>
	/// Gets or sets the author of the vector graphic.
	/// </summary>
	public virtual string? Author { get; set; }

	/// <summary>
	/// Gets or sets the copyright information.
	/// </summary>
	public virtual string? Copyright { get; set; }

	/// <summary>
	/// Gets or sets the creation date.
	/// </summary>
	public virtual DateTime? CreationDate { get; set; }

	/// <summary>
	/// Gets or sets the modification date.
	/// </summary>
	public virtual DateTime? ModificationDate { get; set; }

	/// <summary>
	/// Gets or sets the software used to create the vector graphic.
	/// </summary>
	public virtual string? Software { get; set; }

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			long size = GetBaseMemorySize();

			// Add string sizes
			size += EstimateStringSize(Title);
			size += EstimateStringSize(Description);
			size += EstimateStringSize(Author);
			size += EstimateStringSize(Copyright);
			size += EstimateStringSize(Software);

			// Add basic property sizes
			size += sizeof(double) * 4; // ViewBox coordinates
			size += 16 * 2; // Creation and modification dates (estimated)

			return size;
		}
	}

	/// <summary>
	/// Creates a deep copy of the metadata.
	/// </summary>
	/// <returns>A new instance with the same values.</returns>
	public abstract IVectorMetadata Clone();

	/// <summary>
	/// Clears all metadata values to their defaults.
	/// </summary>
	public virtual void Clear()
	{
		ThrowIfDisposed();

		ViewBoxWidth = 0;
		ViewBoxHeight = 0;
		ViewBoxX = 0;
		ViewBoxY = 0;
		Title = null;
		Description = null;
		Author = null;
		Copyright = null;
		CreationDate = null;
		ModificationDate = null;
		Software = null;
	}

	/// <inheritdoc />
	protected override void DisposeManagedResources()
	{
		// Clear strings
		Title = null;
		Description = null;
		Author = null;
		Copyright = null;
		Software = null;
	}

	/// <summary>
	/// Copies base metadata properties from this instance to another.
	/// </summary>
	/// <param name="target">The target metadata instance.</param>
	protected virtual void CopyBaseTo(VectorMetadataBase target)
	{
		target.ViewBoxWidth = ViewBoxWidth;
		target.ViewBoxHeight = ViewBoxHeight;
		target.ViewBoxX = ViewBoxX;
		target.ViewBoxY = ViewBoxY;
		target.Title = Title;
		target.Description = Description;
		target.Author = Author;
		target.Copyright = Copyright;
		target.CreationDate = CreationDate;
		target.ModificationDate = ModificationDate;
		target.Software = Software;
	}
}