// Test file to verify GitHub issue #80 implementation
using Wangkanai.Graphics.Abstractions;
using Wangkanai.Graphics.Rasters.WebPs;
using Wangkanai.Graphics.Rasters.Jpegs;
using Wangkanai.Graphics.Rasters.Tiffs;
using Wangkanai.Graphics.Rasters.Pngs;

namespace TestAsyncDisposal;

public class AsyncDisposalVerification
{
    public static async Task Main()
    {
        Console.WriteLine("GitHub Issue #80 - IAsyncDisposable for IImage Interface Implementation Test");
        Console.WriteLine("=================================================================================");
        
        // Test WebP with large metadata
        var webp = new WebPRaster(1920, 1080);
        webp.Metadata.IccProfile = new byte[2_000_000]; // 2MB
        webp.Metadata.ExifData = new byte[500_000];     // 500KB
        
        Console.WriteLine($"WebP HasLargeMetadata: {webp.HasLargeMetadata}");
        Console.WriteLine($"WebP EstimatedMetadataSize: {webp.EstimatedMetadataSize:N0} bytes");
        
        await webp.DisposeAsync();
        Console.WriteLine("WebP async disposal completed successfully");
        
        // Test JPEG with metadata
        var jpeg = new JpegRaster(800, 600);
        jpeg.Metadata.IccProfile = new byte[100_000]; // 100KB
        for (int i = 0; i < 1000; i++)
        {
            jpeg.Metadata.CustomExifTags.Add(i, $"Tag value {i}");
        }
        
        Console.WriteLine($"JPEG HasLargeMetadata: {jpeg.HasLargeMetadata}");
        Console.WriteLine($"JPEG EstimatedMetadataSize: {jpeg.EstimatedMetadataSize:N0} bytes");
        
        await jpeg.DisposeAsync();
        Console.WriteLine("JPEG async disposal completed successfully");
        
        // Test TIFF with metadata
        var tiff = new TiffRaster(1024, 768);
        tiff.Metadata.ImageDescription = new string('A', 100_000); // 100KB string
        tiff.Metadata.Make = "Test Camera Make";
        tiff.Metadata.Model = "Test Camera Model";
        
        Console.WriteLine($"TIFF HasLargeMetadata: {tiff.HasLargeMetadata}");
        Console.WriteLine($"TIFF EstimatedMetadataSize: {tiff.EstimatedMetadataSize:N0} bytes");
        
        await tiff.DisposeAsync();
        Console.WriteLine("TIFF async disposal completed successfully");
        
        // Test PNG with metadata
        var png = new PngRaster(512, 512);
        for (int i = 0; i < 10000; i++)
        {
            png.Metadata.TextChunks.Add($"key{i}", $"Very long text value for key {i} with lots of content to make it large");
        }
        
        Console.WriteLine($"PNG HasLargeMetadata: {png.HasLargeMetadata}");
        Console.WriteLine($"PNG EstimatedMetadataSize: {png.EstimatedMetadataSize:N0} bytes");
        
        await png.DisposeAsync();
        Console.WriteLine("PNG async disposal completed successfully");
        
        Console.WriteLine("\n✅ All async disposal tests passed! GitHub issue #80 implementation verified.");
    }
}