// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Heifs;

/// <summary>
/// Represents container format information for HEIF files.
/// </summary>
public sealed class HeifContainerInfo
{
	/// <summary>
	/// Gets or sets the major brand of the HEIF file.
	/// </summary>
	public string MajorBrand { get; set; } = "heic";

	/// <summary>
	/// Gets or sets the minor version of the container format.
	/// </summary>
	public uint MinorVersion { get; set; }

	/// <summary>
	/// Gets or sets the compatible brands supported by this file.
	/// </summary>
	public string[] CompatibleBrands { get; set; } = Array.Empty<string>();

	/// <summary>
	/// Gets or sets whether the container has thumbnail images.
	/// </summary>
	public bool HasThumbnails { get; set; }

	/// <summary>
	/// Gets or sets the number of items in the container.
	/// </summary>
	public int ItemCount { get; set; }

	/// <summary>
	/// Gets or sets the number of boxes in the container.
	/// </summary>
	public int BoxCount { get; set; }

	/// <summary>
	/// Gets or sets the container file size in bytes.
	/// </summary>
	public long FileSize { get; set; }

	/// <summary>
	/// Gets or sets whether the container supports progressive decoding.
	/// </summary>
	public bool SupportsProgressiveDecoding { get; set; }

	/// <summary>
	/// Gets or sets whether the container has image sequences.
	/// </summary>
	public bool HasImageSequences { get; set; }

	/// <summary>
	/// Gets or sets whether the container has image collections.
	/// </summary>
	public bool HasImageCollections { get; set; }

	/// <summary>
	/// Gets or sets whether the container has auxiliary images (like depth maps).
	/// </summary>
	public bool HasAuxiliaryImages { get; set; }

	/// <summary>
	/// Gets or sets the primary item ID.
	/// </summary>
	public uint PrimaryItemId { get; set; }

	/// <summary>
	/// Gets or sets additional container properties.
	/// </summary>
	public Dictionary<string, object> Properties { get; set; } = new();
}