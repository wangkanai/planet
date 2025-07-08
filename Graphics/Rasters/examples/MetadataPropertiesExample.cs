// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Avifs;
using Wangkanai.Graphics.Rasters.Jpeg2000s;
using Wangkanai.Graphics.Rasters.Pngs;
using Wangkanai.Graphics.Rasters.WebPs;

namespace Wangkanai.Graphics.Rasters.Examples;

/// <summary>
/// Example demonstrating the usage of specific metadata properties added to raster classes.
/// </summary>
public static class MetadataPropertiesExample
{
	/// <summary>
	/// Demonstrates accessing AVIF-specific metadata through the AvifMetadata property.
	/// </summary>
	public static void AvifMetadataExample()
	{
		using var avif = new AvifRaster(1920, 1080, hasAlpha: true);
		
		// Access AVIF-specific metadata directly
		var avifMetadata = avif.AvifMetadata;
		Console.WriteLine($"AVIF Width: {avifMetadata.Width}");
		Console.WriteLine($"AVIF Height: {avifMetadata.Height}");
		Console.WriteLine($"AVIF Has Alpha: {avifMetadata.HasAlpha}");
		Console.WriteLine($"AVIF Color Space: {avifMetadata.ColorSpace}");
		Console.WriteLine($"AVIF Bit Depth: {avifMetadata.BitDepth}");
	}
	
	/// <summary>
	/// Demonstrates accessing JPEG2000-specific metadata through the Jpeg2000Metadata property.
	/// </summary>
	public static void Jpeg2000MetadataExample()
	{
		using var jpeg2000 = new Jpeg2000Raster(2048, 2048, components: 3);
		
		// Access JPEG2000-specific metadata directly
		var jp2Metadata = jpeg2000.Jpeg2000Metadata;
		Console.WriteLine($"JPEG2000 Width: {jp2Metadata.Width}");
		Console.WriteLine($"JPEG2000 Height: {jp2Metadata.Height}");
		Console.WriteLine($"JPEG2000 Components: {jp2Metadata.Components}");
		Console.WriteLine($"JPEG2000 Bit Depth: {jp2Metadata.BitDepth}");
		Console.WriteLine($"JPEG2000 Has ICC Profile: {jp2Metadata.HasIccProfile}");
	}
	
	/// <summary>
	/// Demonstrates accessing WebP-specific metadata through the WebPMetadata property.
	/// </summary>
	public static void WebPMetadataExample()
	{
		using var webp = new WebPRaster(800, 600, quality: 85);
		
		// Access WebP-specific metadata directly
		var webpMetadata = webp.WebPMetadata;
		Console.WriteLine($"WebP Has Alpha: {webpMetadata.HasAlpha}");
		Console.WriteLine($"WebP Has Animation: {webpMetadata.HasAnimation}");
		Console.WriteLine($"WebP Is Extended: {webpMetadata.IsExtended}");
		Console.WriteLine($"WebP Creation DateTime: {webpMetadata.CreationDateTime?.ToString() ?? "Not set"}");
	}
	
	/// <summary>
	/// Demonstrates accessing PNG-specific metadata through the PngMetadata property.
	/// </summary>
	public static void PngMetadataExample()
	{
		using var png = new PngRaster(1024, 768);
		
		// Access PNG-specific metadata directly
		var pngMetadata = png.PngMetadata;
		pngMetadata.Title = "Example PNG Image";
		pngMetadata.Author = "Graphics Library";
		pngMetadata.Description = "Demonstrates PNG metadata access";
		
		Console.WriteLine($"PNG Title: {pngMetadata.Title}");
		Console.WriteLine($"PNG Author: {pngMetadata.Author}");
		Console.WriteLine($"PNG Description: {pngMetadata.Description}");
		Console.WriteLine($"PNG Has Large Metadata: {pngMetadata.HasLargeMetadata}");
	}
}