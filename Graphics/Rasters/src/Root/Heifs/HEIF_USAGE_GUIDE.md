# HEIF Usage Guide

## Quick Start

### Installation
The HEIF support is included in the Wangkanai.Graphics.Rasters library.

```csharp
using Wangkanai.Graphics.Rasters.Heifs;
```

### Basic Usage

#### Loading HEIF Images
```csharp
// Load from file
var heif = await HeifRaster.LoadAsync("photo.heif");

// Load from byte array
byte[] imageData = await File.ReadAllBytesAsync("photo.heic");
var heif = HeifRaster.FromBytes(imageData);

// Load from stream
using var stream = File.OpenRead("photo.heif");
var heif = await HeifRaster.LoadAsync(stream);
```

#### Creating New HEIF Images
```csharp
// Create from pixel data
var heif = new HeifRaster
{
    Width = 1920,
    Height = 1080,
    BitDepth = 8,
    PixelData = pixelArray
};

// Create with specific codec
var heif = new HeifRaster
{
    Compression = HeifCompression.Hevc,
    Profile = HeifProfile.Main
};
```

#### Saving HEIF Images
```csharp
// Save with default options
await heif.SaveAsync("output.heif");

// Save with custom options
var options = new HeifEncodingOptions
{
    Quality = 90,
    Compression = HeifCompression.Hevc,
    EnableThumbnails = true
};
await heif.SaveAsync("output.heif", options);
```

## Common Scenarios

### 1. Converting JPEG to HEIF
```csharp
// Load JPEG
var jpeg = await JpegRaster.LoadAsync("photo.jpg");

// Convert to HEIF with 50% smaller file size
var heif = new HeifRaster
{
    Width = jpeg.Width,
    Height = jpeg.Height,
    PixelData = jpeg.PixelData,
    Compression = HeifCompression.Hevc,
    Metadata = new HeifMetadata
    {
        // Copy metadata
        Title = jpeg.Metadata.Title,
        Description = jpeg.Metadata.Description,
        Camera = jpeg.Metadata.Camera
    }
};

// Save with optimized settings
var options = HeifEncodingOptions.CreateWebOptimized();
options.Quality = 85; // Visually lossless
await heif.SaveAsync("photo.heif", options);
```

### 2. Creating iPhone-Compatible HEIF
```csharp
var heif = HeifExamples.CreateAppleCompatible(4032, 3024);

// Set iPhone metadata
heif.Metadata.Camera = new CameraMetadata
{
    CameraMake = "Apple",
    CameraModel = "iPhone 15 Pro",
    LensMake = "Apple",
    LensModel = "iPhone 15 Pro back triple camera 6.86mm f/1.78",
    FocalLength = 6.86,
    Aperture = 1.78,
    IsoSensitivity = 64
};

// Save with Apple-compatible settings
await heif.SaveAsync("iphone_photo.heic");
```

### 3. Creating HDR Images
```csharp
// Create HDR10 image
var hdr = HeifExamples.CreateHdr(3840, 2160);

// Configure HDR metadata
hdr.Metadata.HdrMetadata = new HdrMetadata
{
    Format = HdrFormat.Hdr10,
    MaxLuminance = 1000.0,          // 1000 nits
    MinLuminance = 0.0001,          // 0.0001 nits
    MaxContentLightLevel = 1000.0,
    MaxFrameAverageLightLevel = 400.0,
    ColorPrimaries = HdrColorPrimaries.Bt2020,
    TransferCharacteristics = HdrTransferCharacteristics.SmpteSt2084,
    MatrixCoefficients = HdrMatrixCoefficients.Bt2020NonConstant
};

// Set color volume
hdr.Metadata.HdrMetadata.ColorVolume = new ColorVolumeMetadata
{
    RedPrimaryX = 0.708,
    RedPrimaryY = 0.292,
    GreenPrimaryX = 0.170,
    GreenPrimaryY = 0.797,
    BluePrimaryX = 0.131,
    BluePrimaryY = 0.046,
    WhitePointX = 0.3127,
    WhitePointY = 0.3290
};
```

### 4. Creating Image Sequences (Burst Photos)
```csharp
// Create burst photo sequence
var burst = HeifExamples.CreatePhotoSequence(1920, 1080, 10);

burst.Metadata.IsImageSequence = true;
burst.Metadata.ImageCount = 10;
burst.Metadata.PrimaryImageIndex = 5; // Best shot

// Add timing information
burst.Metadata.SequenceTimings = new List<double>
{
    0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9
};
```

### 5. Adding Auxiliary Images
```csharp
var heif = new HeifRaster();

// Add depth map
heif.Metadata.AuxiliaryImages = new List<AuxiliaryImageInfo>
{
    new()
    {
        Type = AuxiliaryImageType.DepthMap,
        Index = 1,
        Width = 1920,
        Height = 1080,
        Description = "Depth map from dual camera"
    },
    new()
    {
        Type = AuxiliaryImageType.AlphaChannel,
        Index = 2,
        Description = "Transparency mask"
    },
    new()
    {
        Type = AuxiliaryImageType.Thumbnail,
        Index = 3,
        Width = 256,
        Height = 256,
        Description = "Preview thumbnail"
    }
};
```

### 6. Batch Processing
```csharp
public async Task ConvertFolderToHeif(string inputFolder, string outputFolder)
{
    var jpegFiles = Directory.GetFiles(inputFolder, "*.jpg");
    
    var options = HeifEncodingOptions.CreateHighQuality();
    options.EnableThumbnails = true;
    options.EnableMetadata = true;
    
    var tasks = jpegFiles.Select(async jpegPath =>
    {
        try
        {
            // Load JPEG
            var jpeg = await JpegRaster.LoadAsync(jpegPath);
            
            // Convert to HEIF
            var heif = new HeifRaster
            {
                Width = jpeg.Width,
                Height = jpeg.Height,
                PixelData = jpeg.PixelData,
                Compression = HeifCompression.Hevc
            };
            
            // Copy metadata
            heif.Metadata.Title = jpeg.Metadata.Title;
            heif.Metadata.Camera = jpeg.Metadata.Camera;
            heif.Metadata.GpsLocation = jpeg.Metadata.GpsLocation;
            
            // Save HEIF
            var outputPath = Path.Combine(outputFolder, 
                Path.GetFileNameWithoutExtension(jpegPath) + ".heif");
            await heif.SaveAsync(outputPath, options);
            
            Console.WriteLine($"Converted: {Path.GetFileName(jpegPath)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting {jpegPath}: {ex.Message}");
        }
    });
    
    await Task.WhenAll(tasks);
}
```

### 7. Memory-Efficient Processing
```csharp
// Process large HEIF files with minimal memory
public async Task ProcessLargeHeif(string inputPath, string outputPath)
{
    // Configure memory limits
    var options = new HeifEncodingOptions
    {
        MaxPixelBufferSizeMB = 512,      // Limit pixel buffer
        MaxMetadataBufferSizeMB = 64,    // Limit metadata buffer
        UseTiling = true,                // Enable tile-based processing
        TileSize = 512
    };
    
    await using var input = File.OpenRead(inputPath);
    await using var output = File.Create(outputPath);
    
    // Stream-based processing
    await HeifRaster.ProcessStreamAsync(input, output, image =>
    {
        // Process in tiles to save memory
        image.ProcessInTiles(tile =>
        {
            // Apply processing to each tile
            ApplyFilter(tile);
        });
    }, options);
}
```

## Advanced Features

### Clean Aperture (Cropping without Re-encoding)
```csharp
// Define clean aperture
heif.Metadata.CleanAperture = new CleanApertureBox
{
    Width = 1920,
    Height = 1080,
    HorizontalOffset = 100,
    VerticalOffset = 50
};
```

### Rotation and Mirroring
```csharp
// Lossless rotation
heif.Metadata.Orientation = ImageOrientation.Rotate90CW;

// Mirror horizontally
heif.Metadata.Orientation = ImageOrientation.MirrorHorizontal;
```

### Multi-Resolution Storage
```csharp
// Store multiple resolutions
heif.Metadata.IsMultiResolution = true;
heif.Metadata.ResolutionLevels = new List<ResolutionInfo>
{
    new() { Width = 3840, Height = 2160, Index = 0 }, // 4K
    new() { Width = 1920, Height = 1080, Index = 1 }, // Full HD
    new() { Width = 1280, Height = 720, Index = 2 },  // HD
    new() { Width = 640, Height = 360, Index = 3 }    // Preview
};
```

### Grid Images (Large Image Tiling)
```csharp
// Configure grid for very large images
heif.Metadata.IsGridImage = true;
heif.Metadata.GridConfiguration = new GridConfiguration
{
    TileWidth = 512,
    TileHeight = 512,
    GridWidth = 16,    // 16x16 grid
    GridHeight = 16,
    TotalWidth = 8192,
    TotalHeight = 8192
};
```

## Performance Optimization

### 1. Codec Selection for Performance
```csharp
// Fastest encoding (lower quality)
var fast = new HeifEncodingOptions
{
    Compression = HeifCompression.Avc,  // H.264 is faster
    Speed = RasterConstants.SpeedPresets.Fastest,
    Quality = 75
};

// Balanced performance
var balanced = new HeifEncodingOptions
{
    Compression = HeifCompression.Hevc,
    Speed = RasterConstants.SpeedPresets.Default,
    Quality = 85
};

// Best quality (slowest)
var best = new HeifEncodingOptions
{
    Compression = HeifCompression.Av1,  // Best compression
    Speed = RasterConstants.SpeedPresets.Slowest,
    Quality = 95
};
```

### 2. Multi-threading Configuration
```csharp
// Auto-detect optimal threads
options.ThreadCount = 0;

// Conservative threading
options.ThreadCount = Math.Max(1, Environment.ProcessorCount / 2);

// Maximum performance
options.ThreadCount = Environment.ProcessorCount;
```

### 3. Memory Usage Patterns
```csharp
// Low memory mode
var lowMem = new HeifEncodingOptions
{
    MaxPixelBufferSizeMB = 128,
    UseTiling = true,
    TileSize = 256
};

// High performance mode
var highPerf = new HeifEncodingOptions
{
    MaxPixelBufferSizeMB = 2048,
    UseTiling = false,
    ThreadCount = Environment.ProcessorCount
};
```

## Error Handling and Validation

### Comprehensive Error Handling
```csharp
try
{
    var heif = await HeifRaster.LoadAsync("image.heif");
    
    // Validate before processing
    var validator = new HeifValidator();
    var result = validator.Validate(heif);
    
    if (!result.IsValid)
    {
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"Error: {error}");
        }
        return;
    }
    
    // Process image
    var processed = await ProcessHeifImage(heif);
    await processed.SaveAsync("output.heif");
}
catch (HeifException ex)
{
    Console.WriteLine($"HEIF-specific error: {ex.Message}");
}
catch (UnsupportedCodecException ex)
{
    Console.WriteLine($"Codec '{ex.CodecName}' is not supported");
}
catch (InvalidImageDataException ex)
{
    Console.WriteLine($"Invalid image data: {ex.Message}");
}
catch (OutOfMemoryException)
{
    Console.WriteLine("Insufficient memory for operation");
}
```

### Pre-flight Validation
```csharp
// Validate before encoding
public bool ValidateHeifConfiguration(HeifRaster heif, HeifEncodingOptions options)
{
    var validator = new HeifValidator();
    
    // Basic validation
    var result = validator.Validate(heif);
    if (!result.IsValid) return false;
    
    // Options validation
    result = HeifValidator.ValidateWithOptions(heif, options);
    if (!result.IsValid) return false;
    
    // Custom validation
    if (heif.Width * heif.Height > 100_000_000) // 100MP
    {
        if (!options.UseTiling)
        {
            Console.WriteLine("Large image requires tiling");
            return false;
        }
    }
    
    return true;
}
```

## Integration Examples

### Web API Integration
```csharp
[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    [HttpPost("convert-to-heif")]
    public async Task<IActionResult> ConvertToHeif(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");
        
        try
        {
            using var stream = file.OpenReadStream();
            var jpeg = await JpegRaster.LoadAsync(stream);
            
            var heif = new HeifRaster
            {
                Width = jpeg.Width,
                Height = jpeg.Height,
                PixelData = jpeg.PixelData,
                Compression = HeifCompression.Hevc
            };
            
            var options = HeifEncodingOptions.CreateWebOptimized();
            
            using var outputStream = new MemoryStream();
            await heif.SaveAsync(outputStream, options);
            
            return File(outputStream.ToArray(), "image/heif", 
                Path.GetFileNameWithoutExtension(file.FileName) + ".heif");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Conversion failed: {ex.Message}");
        }
    }
}
```

### Desktop Application Integration
```csharp
public partial class MainWindow : Window
{
    private async void ConvertButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*"
        };
        
        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                StatusLabel.Content = "Converting...";
                
                var jpeg = await JpegRaster.LoadAsync(openFileDialog.FileName);
                var heif = ConvertJpegToHeif(jpeg);
                
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "HEIF files (*.heif)|*.heif",
                    FileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName) + ".heif"
                };
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    var options = GetEncodingOptions();
                    await heif.SaveAsync(saveFileDialog.FileName, options);
                    StatusLabel.Content = "Conversion complete!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Conversion Failed", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
```