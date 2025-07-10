// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors;

/// <summary>
/// Base implementation of vector metadata with common properties and functionality.
/// </summary>
public abstract class VectorMetadata : Metadata, IVectorMetadata
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


	/// <inheritdoc />
	public virtual string? CoordinateReferenceSystem { get; set; }

	/// <inheritdoc />
	public virtual string? ColorSpace { get; set; }

	/// <inheritdoc />
	public virtual int ElementCount { get; set; }

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			// Get base size which includes common properties
			long size = GetBaseMemorySize();

			// Add vector-specific properties
			size += EstimateStringSize(CoordinateReferenceSystem);
			size += EstimateStringSize(ColorSpace);
			size += sizeof(double) * 4; // ViewBox coordinates
			size += sizeof(int); // ElementCount

			return size;
		}
	}

	/// <inheritdoc />
	public abstract override IMetadata Clone();
	
	/// <summary>
	/// Creates a deep copy of the vector metadata.
	/// </summary>
	/// <returns>A new instance with the same values.</returns>
	public abstract IVectorMetadata CloneVector();

	/// <inheritdoc />
	IVectorMetadata IVectorMetadata.Clone() => CloneVector();

	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();
		
		ViewBoxWidth              = 0;
		ViewBoxHeight             = 0;
		ViewBoxX                  = 0;
		ViewBoxY                  = 0;
		CoordinateReferenceSystem = null;
		ColorSpace                = null;
		ElementCount              = 0;
	}

	/// <inheritdoc />
	protected override void DisposeManagedResources()
	{
		// Clear vector-specific strings
		CoordinateReferenceSystem = null;
		ColorSpace                = null;
	}

	/// <summary>
	/// Copies vector metadata properties from this instance to another.
	/// </summary>
	/// <param name="target">The target vector metadata instance.</param>
	protected virtual void CopyVectorTo(VectorMetadata target)
	{
		// Copy base properties
		base.CopyBaseTo(target);
		
		// Copy vector-specific properties
		target.ViewBoxWidth              = ViewBoxWidth;
		target.ViewBoxHeight             = ViewBoxHeight;
		target.ViewBoxX                  = ViewBoxX;
		target.ViewBoxY                  = ViewBoxY;
		target.CoordinateReferenceSystem = CoordinateReferenceSystem;
		target.ColorSpace                = ColorSpace;
		target.ElementCount              = ElementCount;
	}
}
