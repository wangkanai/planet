using Wangkanai.Graphics.Rasters.Webps;

var webp = new WebPRaster();
Console.WriteLine($"Initial: Format={webp.Format}, Compression={webp.Compression}");

webp.Compression = WebPCompression.VP8L;
Console.WriteLine($"After setting VP8L: Format={webp.Format}, Compression={webp.Compression}");