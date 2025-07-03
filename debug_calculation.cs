using System;

// Let's calculate what the current implementation produces
class DebugCalculation
{
    static void Main()
    {
        // Test case: px: -256, py: -256, zoom: 1
        double px = -256;
        double py = -256;
        int zoom = 1;

        // Mercator values (from the constructor)
        int tileSize = 512;
        double mapExtentMax = 6378137.0; // MapExtent.Max
        double initialResolution = 2 * Math.PI * mapExtentMax / tileSize;
        double originShift = 2 * Math.PI * mapExtentMax / 2.0;

        Console.WriteLine($"TileSize: {tileSize}");
        Console.WriteLine($"MapExtent.Max: {mapExtentMax}");
        Console.WriteLine($"InitialResolution: {initialResolution:F6}");
        Console.WriteLine($"OriginShift: {originShift:F6}");

        // Resolution at zoom level 1
        double resolution = initialResolution / (1 << zoom); // 1 << 1 = 2
        Console.WriteLine($"Resolution at zoom {zoom}: {resolution:F6}");

        // Current PixelToMeters calculation
        double resultX = px * resolution - originShift;
        double resultY = originShift - py * resolution;

        Console.WriteLine($"\nPixelToMeters result:");
        Console.WriteLine($"X: {resultX:F6}");
        Console.WriteLine($"Y: {resultY:F6}");
        Console.WriteLine($"OriginShift bounds: [{-originShift:F6}, {originShift:F6}]");

        // Check if within bounds
        bool xValid = resultX >= -originShift && resultX <= originShift;
        bool yValid = resultY >= -originShift && resultY <= originShift;

        Console.WriteLine($"\nBounds check:");
        Console.WriteLine($"X valid: {xValid} ({resultX:F6} in [{-originShift:F6}, {originShift:F6}])");
        Console.WriteLine($"Y valid: {yValid} ({resultY:F6} in [{-originShift:F6}, {originShift:F6}])");

        Console.WriteLine($"\nX exceeds lower bound by: {Math.Abs(resultX + originShift):F6}");
    }
}
