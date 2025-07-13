# Chapter 13: Color Space Management

Color exists at the intersection of physics, biology, and mathematics—a trinity that makes color space management one of
the most intellectually challenging aspects of graphics processing. While humans perceive color effortlessly, accurately
representing and transforming colors across different devices, standards, and viewing conditions requires sophisticated
mathematical frameworks and careful engineering. In an era where content flows seamlessly from professional cameras to
smartphones, from print to HDR displays, the ability to preserve artistic intent while adapting to device capabilities
has become paramount. This chapter explores how modern .NET applications can implement robust color management systems
that handle everything from basic sRGB workflows to cinema-grade wide gamut pipelines, ensuring that a sunset
photographed in Adobe RGB maintains its golden warmth whether viewed on a professional monitor or a mobile device.

## 13.1 ICC Profile Integration

The International Color Consortium (ICC) profile system represents the industry standard for describing color
characteristics of devices and color spaces. These profiles act as mathematical rosetta stones, enabling accurate color
translation between different devices and working spaces. Understanding how to parse, interpret, and apply ICC profiles
forms the foundation of any serious color management implementation.

### Understanding ICC profile architecture

ICC profiles encode complex color transformations in a standardized binary format, containing lookup tables, matrices,
and curves that map device colors to a profile connection space (PCS). The architecture supports multiple rendering
intents—perceptual, relative colorimetric, saturation, and absolute colorimetric—each optimized for different use cases.
The profile structure includes a header describing basic characteristics, a tag table indexing data elements, and tagged
element data containing the actual transformation information.

```csharp
// Comprehensive ICC profile parser with v4 support
public class ICCProfileParser
{
    private readonly Dictionary<uint, ITagParser> _tagParsers;
    private readonly byte[] _profileData;

    public ICCProfile Parse(byte[] profileData)
    {
        _profileData = profileData;
        var profile = new ICCProfile();

        // Parse header (128 bytes)
        using var reader = new BinaryReader(new MemoryStream(profileData));

        profile.Size = ReadUInt32BE(reader);
        profile.CMMType = ReadSignature(reader);
        profile.Version = ParseVersion(reader);
        profile.ProfileClass = (ProfileClass)ReadSignature(reader);
        profile.ColorSpace = (ColorSpaceType)ReadSignature(reader);
        profile.PCS = (ColorSpaceType)ReadSignature(reader);

        // Skip to creation date
        reader.BaseStream.Seek(24, SeekOrigin.Begin);
        profile.CreationDate = ReadDateTime(reader);

        // Profile signature 'acsp'
        var signature = ReadSignature(reader);
        if (signature != 0x61637370)
            throw new InvalidDataException("Invalid ICC profile signature");

        // Platform, flags, manufacturer, model
        profile.Platform = (PlatformSignature)ReadSignature(reader);
        profile.Flags = ReadUInt32BE(reader);
        profile.DeviceManufacturer = ReadSignature(reader);
        profile.DeviceModel = ReadSignature(reader);

        // Device attributes and rendering intent
        profile.DeviceAttributes = ReadUInt64BE(reader);
        profile.RenderingIntent = (RenderingIntent)ReadUInt32BE(reader);

        // Illuminant XYZ values
        profile.Illuminant = new XYZNumber
        {
            X = ReadS15Fixed16(reader),
            Y = ReadS15Fixed16(reader),
            Z = ReadS15Fixed16(reader)
        };

        // Profile creator signature
        profile.Creator = ReadSignature(reader);

        // Parse tag table
        reader.BaseStream.Seek(128, SeekOrigin.Begin);
        var tagCount = ReadUInt32BE(reader);

        for (int i = 0; i < tagCount; i++)
        {
            var tagSignature = ReadSignature(reader);
            var offset = ReadUInt32BE(reader);
            var size = ReadUInt32BE(reader);

            // Parse tag based on signature
            if (_tagParsers.TryGetValue(tagSignature, out var parser))
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var tagData = reader.ReadBytes((int)size);
                profile.Tags[tagSignature] = parser.Parse(tagData);
            }
        }

        // Validate required tags based on profile class
        ValidateRequiredTags(profile);

        return profile;
    }

    // Matrix/TRC based profile transformation
    public class MatrixTRCTransform : IColorTransform
    {
        private readonly ToneCurve _redTRC;
        private readonly ToneCurve _greenTRC;
        private readonly ToneCurve _blueTRC;
        private readonly Matrix3x3 _matrix;
        private readonly Matrix3x3 _inverseMatrix;

        public Vector3 ToXYZ(Vector3 deviceRGB)
        {
            // Apply TRC curves
            var linear = new Vector3(
                _redTRC.Apply(deviceRGB.X),
                _greenTRC.Apply(deviceRGB.Y),
                _blueTRC.Apply(deviceRGB.Z)
            );

            // Matrix multiplication to XYZ
            return _matrix.Transform(linear);
        }

        public Vector3 FromXYZ(Vector3 xyz)
        {
            // Inverse matrix to linear RGB
            var linear = _inverseMatrix.Transform(xyz);

            // Apply inverse TRC curves
            return new Vector3(
                _redTRC.ApplyInverse(linear.X),
                _greenTRC.ApplyInverse(linear.Y),
                _blueTRC.ApplyInverse(linear.Z)
            );
        }
    }
}

// Optimized tone reproduction curve with caching
public class ToneCurve
{
    private readonly float[] _lut;
    private readonly int _lutSize;
    private readonly CurveType _type;
    private readonly float _gamma;

    public ToneCurve(byte[] curveData)
    {
        using var reader = new BinaryReader(new MemoryStream(curveData));
        var typeSignature = ReadSignature(reader);

        if (typeSignature == 0x63757276) // 'curv'
        {
            var count = ReadUInt32BE(reader);
            if (count == 0)
            {
                // Identity curve
                _type = CurveType.Identity;
            }
            else if (count == 1)
            {
                // Gamma curve
                _type = CurveType.Gamma;
                _gamma = ReadUInt16BE(reader) / 256f;
            }
            else
            {
                // LUT curve
                _type = CurveType.LUT;
                _lutSize = (int)count;
                _lut = new float[_lutSize];

                for (int i = 0; i < _lutSize; i++)
                {
                    _lut[i] = ReadUInt16BE(reader) / 65535f;
                }
            }
        }
        else if (typeSignature == 0x70617261) // 'para'
        {
            // Parametric curve
            _type = CurveType.Parametric;
            ParseParametricCurve(reader);
        }
    }

    public float Apply(float input)
    {
        return _type switch
        {
            CurveType.Identity => input,
            CurveType.Gamma => MathF.Pow(input, _gamma),
            CurveType.LUT => InterpolateLUT(input),
            CurveType.Parametric => ApplyParametric(input),
            _ => input
        };
    }

    private float InterpolateLUT(float input)
    {
        var scaledInput = input * (_lutSize - 1);
        var index = (int)scaledInput;
        var fraction = scaledInput - index;

        if (index >= _lutSize - 1)
            return _lut[_lutSize - 1];

        // Linear interpolation for smooth results
        return _lut[index] * (1 - fraction) + _lut[index + 1] * fraction;
    }
}
```

### Multi-dimensional lookup table implementation

For complex color transformations that cannot be represented by simple matrices and curves, ICC profiles employ
multi-dimensional lookup tables (LUTs). These tables provide arbitrary mappings between color spaces but require
sophisticated interpolation algorithms to avoid artifacts. The implementation must balance memory usage, interpolation
quality, and computational efficiency.

```csharp
// High-performance 3D LUT with tetrahedral interpolation
public class ColorLUT3D
{
    private readonly float[,,][] _nodes; // [r,g,b][x,y,z]
    private readonly int _gridSize;
    private readonly bool _useTetrahedralInterpolation;

    public Vector3 Transform(Vector3 input)
    {
        // Scale input to grid coordinates
        var scaledR = input.X * (_gridSize - 1);
        var scaledG = input.Y * (_gridSize - 1);
        var scaledB = input.Z * (_gridSize - 1);

        // Find grid cell containing the input
        var r0 = (int)MathF.Floor(scaledR);
        var g0 = (int)MathF.Floor(scaledG);
        var b0 = (int)MathF.Floor(scaledB);

        // Fractional position within cell
        var rf = scaledR - r0;
        var gf = scaledG - g0;
        var bf = scaledB - b0;

        // Clamp to valid range
        r0 = Math.Clamp(r0, 0, _gridSize - 2);
        g0 = Math.Clamp(g0, 0, _gridSize - 2);
        b0 = Math.Clamp(b0, 0, _gridSize - 2);

        if (_useTetrahedralInterpolation)
        {
            return TetrahedralInterpolate(r0, g0, b0, rf, gf, bf);
        }
        else
        {
            return TrilinearInterpolate(r0, g0, b0, rf, gf, bf);
        }
    }

    private Vector3 TetrahedralInterpolate(int r0, int g0, int b0,
        float rf, float gf, float bf)
    {
        // Get cube vertices
        var v000 = GetNode(r0, g0, b0);
        var v001 = GetNode(r0, g0, b0 + 1);
        var v010 = GetNode(r0, g0 + 1, b0);
        var v011 = GetNode(r0, g0 + 1, b0 + 1);
        var v100 = GetNode(r0 + 1, g0, b0);
        var v101 = GetNode(r0 + 1, g0, b0 + 1);
        var v110 = GetNode(r0 + 1, g0 + 1, b0);
        var v111 = GetNode(r0 + 1, g0 + 1, b0 + 1);

        // Determine which tetrahedron contains the point
        Vector3 result;

        if (rf > gf)
        {
            if (gf > bf)
            {
                // Tetrahedron 1: P0, P4, P5, P7
                result = v000 * (1 - rf) +
                        v100 * (rf - gf) +
                        v110 * (gf - bf) +
                        v111 * bf;
            }
            else if (rf > bf)
            {
                // Tetrahedron 2: P0, P4, P6, P7
                result = v000 * (1 - rf) +
                        v100 * (rf - bf) +
                        v101 * (bf - gf) +
                        v111 * gf;
            }
            else
            {
                // Tetrahedron 3: P0, P2, P6, P7
                result = v000 * (1 - bf) +
                        v001 * (bf - rf) +
                        v011 * (rf - gf) +
                        v111 * gf;
            }
        }
        else
        {
            if (bf > gf)
            {
                // Tetrahedron 4: P0, P2, P3, P7
                result = v000 * (1 - bf) +
                        v001 * (bf - gf) +
                        v011 * (gf - rf) +
                        v111 * rf;
            }
            else if (bf > rf)
            {
                // Tetrahedron 5: P0, P1, P3, P7
                result = v000 * (1 - gf) +
                        v010 * (gf - bf) +
                        v011 * (bf - rf) +
                        v111 * rf;
            }
            else
            {
                // Tetrahedron 6: P0, P1, P5, P7
                result = v000 * (1 - gf) +
                        v010 * (gf - rf) +
                        v110 * (rf - bf) +
                        v111 * bf;
            }
        }

        return result;
    }

    private Vector3 GetNode(int r, int g, int b)
    {
        var node = _nodes[r, g, b];
        return new Vector3(node[0], node[1], node[2]);
    }
}
```

### Rendering intent implementation strategies

ICC profiles support four rendering intents, each serving different purposes in color reproduction. **Perceptual**
intent compresses the entire color gamut to fit the destination space while preserving color relationships, ideal for
photographic images. **Relative colorimetric** maps colors directly when possible, clipping out-of-gamut colors to the
nearest reproducible color, suitable for logo colors and spot colors. **Saturation** intent maximizes color vividness at
the expense of accuracy, useful for business graphics. **Absolute colorimetric** preserves the exact appearance
including paper white simulation, essential for proofing applications.

```csharp
// Sophisticated gamut mapping with perceptual intent
public class PerceptualGamutMapper
{
    private readonly IGamutBoundary _sourceGamut;
    private readonly IGamutBoundary _destGamut;
    private readonly float _compressionRatio;

    public Vector3 MapColor(Vector3 sourceColor, ColorSpace sourceSpace)
    {
        // Convert to perceptually uniform space (Lab)
        var lab = sourceSpace.ToLab(sourceColor);

        // Check if color is within destination gamut
        if (_destGamut.Contains(lab))
            return lab;

        // Find the focal point for compression
        var focalPoint = ComputeFocalPoint(lab);

        // Vector from focal point to color
        var vector = lab - focalPoint;
        var distance = vector.Length();

        // Find gamut boundary intersection
        var boundaryPoint = _destGamut.FindIntersection(focalPoint, vector);
        var boundaryDistance = (boundaryPoint - focalPoint).Length();

        // Apply soft compression curve
        var compressedDistance = ApplyCompressionCurve(
            distance,
            boundaryDistance,
            _compressionRatio
        );

        // Compute final position
        var compressedColor = focalPoint + vector.Normalized() * compressedDistance;

        // Preserve hue while compressing chroma and lightness
        return PreserveHueRelationships(lab, compressedColor);
    }

    private float ApplyCompressionCurve(float distance, float boundary, float ratio)
    {
        // Smooth compression using sigmoid function
        var normalized = distance / boundary;

        if (normalized <= 1.0f)
            return distance; // Within gamut, no compression

        // Soft knee compression
        var compressed = 1.0f + (normalized - 1.0f) * ratio;
        var softness = 0.1f; // Knee radius

        // Smooth transition at gamut boundary
        if (normalized < 1.0f + softness)
        {
            var t = (normalized - 1.0f) / softness;
            var smooth = SmoothStep(0, 1, t);
            compressed = normalized + (compressed - normalized) * smooth;
        }

        return compressed * boundary;
    }

    private float SmoothStep(float edge0, float edge1, float x)
    {
        var t = Math.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
        return t * t * (3.0f - 2.0f * t);
    }
}
```

## 13.2 Wide Gamut and HDR Support

The evolution from standard dynamic range (SDR) sRGB content to wide gamut and high dynamic range (HDR) workflows
represents a paradigm shift in color management. Modern displays can reproduce colors far beyond traditional sRGB, while
HDR enables luminance ranges from deep shadows to brilliant highlights. Supporting these capabilities requires
fundamental changes to color pipelines, from expanded numerical precision to perceptually-based tone mapping algorithms.

### Mathematical foundations of wide gamut spaces

Wide gamut color spaces like Adobe RGB, Display P3, and Rec. 2020 encompass significantly more colors than sRGB,
requiring careful handling to prevent clipping and maintain color relationships. These spaces use the same RGB model but
with different primaries and white points, necessitating precise transformation matrices. The mathematics involve
converting between different RGB spaces through the CIE XYZ connection space, applying chromatic adaptation when white
points differ.

```csharp
// Comprehensive wide gamut color space definitions and transformations
public class WideGamutColorSpace
{
    // Precisely defined color space primaries and white points
    public static class ColorSpaceDefinitions
    {
        public static readonly ColorSpacePrimaries sRGB = new()
        {
            Red = new CIExy(0.6400, 0.3300),
            Green = new CIExy(0.3000, 0.6000),
            Blue = new CIExy(0.1500, 0.0600),
            White = IlluminantD65
        };

        public static readonly ColorSpacePrimaries AdobeRGB = new()
        {
            Red = new CIExy(0.6400, 0.3300),
            Green = new CIExy(0.2100, 0.7100),
            Blue = new CIExy(0.1500, 0.0600),
            White = IlluminantD65
        };

        public static readonly ColorSpacePrimaries DisplayP3 = new()
        {
            Red = new CIExy(0.6800, 0.3200),
            Green = new CIExy(0.2650, 0.6900),
            Blue = new CIExy(0.1500, 0.0600),
            White = IlluminantD65
        };

        public static readonly ColorSpacePrimaries Rec2020 = new()
        {
            Red = new CIExy(0.7080, 0.2920),
            Green = new CIExy(0.1700, 0.7970),
            Blue = new CIExy(0.1310, 0.0460),
            White = IlluminantD65
        };
    }

    // High-precision transformation matrix computation
    public class ColorSpaceTransform
    {
        private readonly Matrix3x3 _rgbToXYZ;
        private readonly Matrix3x3 _xyzToRGB;
        private readonly ChromaticAdaptationTransform _adaptation;

        public ColorSpaceTransform(ColorSpacePrimaries source, ColorSpacePrimaries dest)
        {
            // Compute RGB to XYZ matrices
            _rgbToXYZ = ComputeRGBToXYZMatrix(source);
            var destRGBToXYZ = ComputeRGBToXYZMatrix(dest);
            _xyzToRGB = destRGBToXYZ.Inverse();

            // Setup chromatic adaptation if white points differ
            if (!source.White.Equals(dest.White))
            {
                _adaptation = new ChromaticAdaptationTransform(
                    source.White,
                    dest.White,
                    ChromaticAdaptationMethod.Bradford
                );
            }
        }

        private Matrix3x3 ComputeRGBToXYZMatrix(ColorSpacePrimaries primaries)
        {
            // Convert primaries from xy to XYZ
            var Xr = primaries.Red.ToXYZ();
            var Xg = primaries.Green.ToXYZ();
            var Xb = primaries.Blue.ToXYZ();

            // Build matrix from primaries
            var M = new Matrix3x3(
                Xr.X, Xg.X, Xb.X,
                Xr.Y, Xg.Y, Xb.Y,
                Xr.Z, Xg.Z, Xb.Z
            );

            // Compute scaling factors
            var Xw = primaries.White.ToXYZ();
            var S = M.Inverse() * Xw;

            // Apply scaling to get final matrix
            return new Matrix3x3(
                S.X * Xr.X, S.Y * Xg.X, S.Z * Xb.X,
                S.X * Xr.Y, S.Y * Xg.Y, S.Z * Xb.Y,
                S.X * Xr.Z, S.Y * Xg.Z, S.Z * Xb.Z
            );
        }

        public Vector3 Transform(Vector3 rgb)
        {
            // Convert to XYZ
            var xyz = _rgbToXYZ * rgb;

            // Apply chromatic adaptation if needed
            if (_adaptation != null)
                xyz = _adaptation.Transform(xyz);

            // Convert to destination RGB
            return _xyzToRGB * xyz;
        }
    }
}

// HDR tone mapping with multiple algorithms
public class HDRToneMapper
{
    public enum ToneMappingOperator
    {
        Reinhard,
        ReinhardExtended,
        ACES,
        Hable,
        Lottes,
        AgX
    }

    // ACES (Academy Color Encoding System) filmic tone mapping
    public Vector3 ApplyACESFilmic(Vector3 color, float exposure = 1.0f)
    {
        // Pre-exposure adjustment
        color *= exposure;

        // ACES RRT and ODT approximation
        const float a = 2.51f;
        const float b = 0.03f;
        const float c = 2.43f;
        const float d = 0.59f;
        const float e = 0.14f;

        color = (color * (a * color + b)) / (color * (c * color + d) + e);

        return color;
    }

    // AgX tone mapping for better color preservation
    public Vector3 ApplyAgX(Vector3 color, float exposure = 1.0f)
    {
        // Convert to AgX working space
        var agxTransform = new Matrix3x3(
            0.842479f, 0.0423282f, 0.0423756f,
            0.0784335f, 0.878468f, 0.0784336f,
            0.0792237f, 0.0791661f, 0.879142f
        );

        color = agxTransform * (color * exposure);

        // Apply AgX curve
        color = ApplyAgXCurve(color);

        // Convert back to linear sRGB
        var agxInverse = new Matrix3x3(
            1.19687f, -0.0528968f, -0.0529716f,
            -0.0980208f, 1.15190f, -0.0980434f,
            -0.0990297f, -0.0989611f, 1.15107f
        );

        return agxInverse * color;
    }

    private Vector3 ApplyAgXCurve(Vector3 color)
    {
        // AgX Log2 encoding
        const float minEv = -12.47393f;
        const float maxEv = 4.026069f;

        color = Clamp(Log2(color), minEv, maxEv);
        color = (color - minEv) / (maxEv - minEv);

        // 6th order polynomial approximation
        return ApplyPolynomial6(color, new[]
        {
            new Vector3(0.18f, 0.18f, 0.18f),
            new Vector3(4.356f, 4.346f, 4.366f),
            new Vector3(-7.643f, -7.598f, -7.689f),
            new Vector3(5.858f, 5.784f, 5.958f),
            new Vector3(-1.642f, -1.605f, -1.679f),
            new Vector3(0.178f, 0.174f, 0.182f),
            new Vector3(-0.007f, -0.007f, -0.007f)
        });
    }
}
```

### Display calibration and profiling integration

Accurate color reproduction requires integration with display calibration systems, reading display profiles, and
applying appropriate corrections. Modern displays may provide multiple picture modes with different color spaces and
gamma curves. The implementation must query display capabilities, respect user preferences, and handle multi-monitor
setups with different color characteristics.

```csharp
// Display profiling and calibration system
public class DisplayColorManager
{
    private readonly Dictionary<string, DisplayProfile> _displayProfiles;
    private readonly IDisplayQueryService _displayService;

    public class DisplayProfile
    {
        public string DeviceId { get; set; }
        public ColorSpacePrimaries Primaries { get; set; }
        public ToneCurve TransferFunction { get; set; }
        public float MaxLuminance { get; set; }
        public float MinLuminance { get; set; }
        public HDRMetadata HDRCapabilities { get; set; }
        public Matrix3x3 CalibrationMatrix { get; set; }
        public DateTime CalibrationDate { get; set; }
    }

    // Automatic display profiling with hardware support
    public async Task<DisplayProfile> ProfileDisplayAsync(string displayId)
    {
        var profile = new DisplayProfile { DeviceId = displayId };

        // Query EDID for basic capabilities
        var edid = await _displayService.GetEDIDAsync(displayId);
        profile.Primaries = ParsePrimariesFromEDID(edid);

        // Check for HDR support
        if (_displayService.SupportsHDR(displayId))
        {
            profile.HDRCapabilities = await QueryHDRCapabilities(displayId);
            profile.MaxLuminance = profile.HDRCapabilities.MaxLuminance;
            profile.MinLuminance = profile.HDRCapabilities.MinLuminance;
        }
        else
        {
            // SDR display defaults
            profile.MaxLuminance = 100.0f; // 100 nits typical SDR
            profile.MinLuminance = 0.1f;
        }

        // Measure actual display response if calibration hardware available
        if (await _displayService.HasCalibrationHardware())
        {
            profile = await PerformHardwareCalibration(profile);
        }
        else
        {
            // Use standard curves based on display type
            profile.TransferFunction = EstimateTransferFunction(edid);
        }

        return profile;
    }

    // Real-time display compensation pipeline
    public class DisplayCompensationPipeline
    {
        private readonly DisplayProfile _profile;
        private readonly ComputeShader _compensationShader;

        public Texture2D ApplyDisplayCompensation(
            Texture2D sourceTexture,
            RenderingIntent intent,
            bool preserveHDR)
        {
            var parameters = new CompensationParameters
            {
                CalibrationMatrix = _profile.CalibrationMatrix,
                MaxLuminance = _profile.MaxLuminance,
                MinLuminance = _profile.MinLuminance,
                TransferCurve = _profile.TransferFunction.GetLUT(1024),
                Intent = intent,
                PreserveHDR = preserveHDR
            };

            // GPU-accelerated compensation
            _compensationShader.SetTexture("_SourceTex", sourceTexture);
            _compensationShader.SetBuffer("_Parameters", parameters);

            var result = RenderTexture.GetTemporary(
                sourceTexture.width,
                sourceTexture.height,
                0,
                preserveHDR ? RenderTextureFormat.ARGBFloat :
                             RenderTextureFormat.ARGB32
            );

            _compensationShader.Dispatch(
                sourceTexture.width / 8,
                sourceTexture.height / 8,
                1
            );

            return result;
        }
    }

    // Multi-monitor color consistency
    public class MultiMonitorColorSync
    {
        private readonly List<DisplayProfile> _connectedDisplays;
        private readonly ColorSpace _referenceSpace;

        public void SynchronizeDisplays()
        {
            // Find common gamut intersection
            var commonGamut = ComputeGamutIntersection(_connectedDisplays);

            // Generate compensation LUTs for each display
            foreach (var display in _connectedDisplays)
            {
                var compensationLUT = GenerateCompensationLUT(
                    display.Primaries,
                    commonGamut,
                    _referenceSpace
                );

                ApplyDisplayLUT(display.DeviceId, compensationLUT);
            }
        }

        private IGamutBoundary ComputeGamutIntersection(List<DisplayProfile> displays)
        {
            // Start with first display's gamut
            var intersection = new GamutBoundary(displays[0].Primaries);

            // Intersect with each subsequent display
            for (int i = 1; i < displays.Count; i++)
            {
                var displayGamut = new GamutBoundary(displays[i].Primaries);
                intersection = intersection.Intersect(displayGamut);
            }

            return intersection;
        }
    }
}
```

### Metadata preservation for HDR workflows

HDR content carries essential metadata describing the mastering display characteristics, content light levels, and
dynamic metadata for scene-by-scene optimization. Preserving this metadata throughout the processing pipeline ensures
proper display mapping and maintains the creative intent. The implementation must handle static metadata (HDR10),
dynamic metadata (HDR10+, Dolby Vision), and hybrid approaches.

```csharp
// Comprehensive HDR metadata handling
public class HDRMetadataProcessor
{
    // Static HDR metadata (SMPTE ST 2086)
    public class StaticHDRMetadata
    {
        // Display primaries in CIE 1931 xy coordinates
        public CIExy RedPrimary { get; set; }
        public CIExy GreenPrimary { get; set; }
        public CIExy BluePrimary { get; set; }
        public CIExy WhitePoint { get; set; }

        // Display luminance characteristics
        public float MaxDisplayMasteringLuminance { get; set; } // in nits
        public float MinDisplayMasteringLuminance { get; set; } // in nits

        // Content light levels (CTA-861.3)
        public ushort MaxContentLightLevel { get; set; } // MaxCLL
        public ushort MaxFrameAverageLightLevel { get; set; } // MaxFALL

        public byte[] Serialize()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            // Write primaries as 0.00002 fixed point
            writer.Write((ushort)(RedPrimary.x * 50000));
            writer.Write((ushort)(RedPrimary.y * 50000));
            writer.Write((ushort)(GreenPrimary.x * 50000));
            writer.Write((ushort)(GreenPrimary.y * 50000));
            writer.Write((ushort)(BluePrimary.x * 50000));
            writer.Write((ushort)(BluePrimary.y * 50000));
            writer.Write((ushort)(WhitePoint.x * 50000));
            writer.Write((ushort)(WhitePoint.y * 50000));

            // Luminance in 0.0001 nits
            writer.Write((uint)(MaxDisplayMasteringLuminance * 10000));
            writer.Write((uint)(MinDisplayMasteringLuminance * 10000));

            // Content light levels
            writer.Write(MaxContentLightLevel);
            writer.Write(MaxFrameAverageLightLevel);

            return stream.ToArray();
        }
    }

    // Dynamic HDR metadata (SMPTE ST 2094)
    public class DynamicHDRMetadata
    {
        public class SceneInfo
        {
            public int SceneId { get; set; }
            public float SceneMaxLuminance { get; set; }
            public float SceneAverageLuminance { get; set; }
            public BezierCurve ToneMappingCurve { get; set; }
            public ColorVolumeTransform ColorTransform { get; set; }
        }

        public List<SceneInfo> Scenes { get; set; }
        public InterpolationMethod SceneTransition { get; set; }

        // Apply dynamic metadata to frame
        public Vector3 ApplyToPixel(Vector3 pixel, int frameNumber)
        {
            var sceneInfo = GetSceneForFrame(frameNumber);

            // Apply tone mapping curve
            var luminance = RGBToLuminance(pixel);
            var mappedLuminance = sceneInfo.ToneMappingCurve.Evaluate(luminance);
            var scale = mappedLuminance / luminance;

            // Apply color volume transform
            pixel *= scale;
            pixel = sceneInfo.ColorTransform.Apply(pixel);

            return pixel;
        }
    }

    // Metadata-aware processing pipeline
    public class HDRProcessingPipeline
    {
        private StaticHDRMetadata _staticMetadata;
        private DynamicHDRMetadata _dynamicMetadata;

        public void ProcessHDRImage(HDRImage image, IHDROperation operation)
        {
            // Preserve metadata through operation
            var processedPixels = operation.Process(image.Pixels);

            // Update metadata based on operation
            if (operation.AffectsLuminance)
            {
                UpdateLuminanceMetadata(processedPixels);
            }

            if (operation.AffectsColorVolume)
            {
                UpdateColorVolumeMetadata(processedPixels);
            }

            // Validate metadata consistency
            ValidateMetadata();
        }

        private void UpdateLuminanceMetadata(float[] pixels)
        {
            // Parallel computation of new light levels
            var stats = new ParallelLuminanceStats();

            Parallel.ForEach(Partitioner.Create(pixels, true), partition =>
            {
                float localMax = 0;
                float localSum = 0;
                int localCount = 0;

                foreach (var pixel in partition)
                {
                    var luminance = ComputeLuminance(pixel);
                    localMax = Math.Max(localMax, luminance);
                    localSum += luminance;
                    localCount++;
                }

                stats.AddLocal(localMax, localSum, localCount);
            });

            // Update metadata
            _staticMetadata.MaxContentLightLevel = (ushort)stats.MaxLuminance;
            _staticMetadata.MaxFrameAverageLightLevel = (ushort)stats.AverageLuminance;
        }
    }
}
```

## 13.3 Color Space Conversions

Color space conversion represents one of the most frequently performed operations in graphics pipelines, yet achieving
both accuracy and performance requires careful implementation. The mathematical transformations between spaces like RGB,
Lab, XYZ, and HSL involve non-linear operations, matrix multiplications, and careful handling of edge cases. Modern
implementations must balance computational efficiency with numerical precision while supporting the expanding variety of
color spaces in professional workflows.

### Optimized transformation matrices

The foundation of efficient color space conversion lies in pre-computed transformation matrices and lookup tables. While
the mathematics are well-defined, implementation details significantly impact performance. Matrix operations benefit
from SIMD instructions, cache-friendly memory layouts, and elimination of redundant calculations through algebraic
simplification.

```csharp
// High-performance color transformation engine
public class ColorTransformEngine
{
    // Pre-computed transformation matrices with full precision
    private static class TransformMatrices
    {
        // sRGB to XYZ (D65 illuminant)
        public static readonly Matrix3x3 sRGBToXYZ = new(
            0.4124564f, 0.3575761f, 0.1804375f,
            0.2126729f, 0.7151522f, 0.0721750f,
            0.0193339f, 0.1191920f, 0.9503041f
        );

        // XYZ to sRGB
        public static readonly Matrix3x3 XYZTosRGB = new(
             3.2404542f, -1.5371385f, -0.4985314f,
            -0.9692660f,  1.8760108f,  0.0415560f,
             0.0556434f, -0.2040259f,  1.0572252f
        );

        // Adobe RGB to XYZ (D65)
        public static readonly Matrix3x3 AdobeRGBToXYZ = new(
            0.5767309f, 0.1855540f, 0.1881852f,
            0.2973769f, 0.6273491f, 0.0752741f,
            0.0270343f, 0.0706872f, 0.9911085f
        );

        // Bradford chromatic adaptation matrix
        public static readonly Matrix3x3 BradfordMatrix = new(
             0.8951f,  0.2664f, -0.1614f,
            -0.7502f,  1.7135f,  0.0367f,
             0.0389f, -0.0685f,  1.0296f
        );
    }

    // SIMD-optimized batch color transformation
    public unsafe void TransformBatch(
        ReadOnlySpan<Vector3> source,
        Span<Vector3> destination,
        Matrix3x3 transform)
    {
        int vectorSize = Vector256<float>.Count / 3; // Process multiple colors per iteration
        int i = 0;

        // Process vectorized portion
        fixed (Vector3* srcPtr = source)
        fixed (Vector3* dstPtr = destination)
        {
            float* src = (float*)srcPtr;
            float* dst = (float*)dstPtr;

            for (; i <= source.Length - vectorSize; i += vectorSize)
            {
                // Load color components
                var r = Vector256.Load(src + i * 3);
                var g = Vector256.Load(src + i * 3 + 8);
                var b = Vector256.Load(src + i * 3 + 16);

                // Apply transformation matrix
                var x = r * transform.M11 + g * transform.M12 + b * transform.M13;
                var y = r * transform.M21 + g * transform.M22 + b * transform.M23;
                var z = r * transform.M31 + g * transform.M32 + b * transform.M33;

                // Store results
                x.Store(dst + i * 3);
                y.Store(dst + i * 3 + 8);
                z.Store(dst + i * 3 + 16);
            }
        }

        // Process remaining elements
        for (; i < source.Length; i++)
        {
            destination[i] = transform * source[i];
        }
    }

    // Optimized sRGB gamma encoding/decoding with LUT
    public class GammaProcessor
    {
        private readonly float[] _linearToGammaLUT;
        private readonly float[] _gammaToLinearLUT;
        private const int LUTSize = 4096;

        public GammaProcessor()
        {
            _linearToGammaLUT = new float[LUTSize];
            _gammaToLinearLUT = new float[LUTSize];

            // Pre-compute lookup tables
            for (int i = 0; i < LUTSize; i++)
            {
                float normalized = i / (float)(LUTSize - 1);

                // sRGB gamma encoding
                _linearToGammaLUT[i] = normalized <= 0.0031308f
                    ? 12.92f * normalized
                    : 1.055f * MathF.Pow(normalized, 1.0f / 2.4f) - 0.055f;

                // sRGB gamma decoding
                _gammaToLinearLUT[i] = normalized <= 0.04045f
                    ? normalized / 12.92f
                    : MathF.Pow((normalized + 0.055f) / 1.055f, 2.4f);
            }
        }

        public float LinearToGamma(float linear)
        {
            var index = (int)(linear * (LUTSize - 1));
            if (index < 0) return 0;
            if (index >= LUTSize - 1) return _linearToGammaLUT[LUTSize - 1];

            // Linear interpolation for values between LUT entries
            var fraction = linear * (LUTSize - 1) - index;
            return _linearToGammaLUT[index] * (1 - fraction) +
                   _linearToGammaLUT[index + 1] * fraction;
        }
    }
}

// Lab color space conversions with perceptual uniformity
public class LabColorSpace
{
    private const float Epsilon = 216f / 24389f;
    private const float Kappa = 24389f / 27f;

    // Reference white (D65)
    private static readonly Vector3 D65 = new(0.95047f, 1.00000f, 1.08883f);

    public static Vector3 XYZToLab(Vector3 xyz)
    {
        // Normalize by reference white
        var x = xyz.X / D65.X;
        var y = xyz.Y / D65.Y;
        var z = xyz.Z / D65.Z;

        // Apply cube root compression
        x = x > Epsilon ? MathF.Pow(x, 1f / 3f) : (Kappa * x + 16f) / 116f;
        y = y > Epsilon ? MathF.Pow(y, 1f / 3f) : (Kappa * y + 16f) / 116f;
        z = z > Epsilon ? MathF.Pow(z, 1f / 3f) : (Kappa * z + 16f) / 116f;

        return new Vector3(
            116f * y - 16f,  // L*
            500f * (x - y),  // a*
            200f * (y - z)   // b*
        );
    }

    public static Vector3 LabToXYZ(Vector3 lab)
    {
        var l = lab.X;
        var a = lab.Y;
        var b = lab.Z;

        var y = (l + 16f) / 116f;
        var x = a / 500f + y;
        var z = y - b / 200f;

        // Inverse cube root compression
        var x3 = x * x * x;
        var y3 = y * y * y;
        var z3 = z * z * z;

        x = x3 > Epsilon ? x3 : (116f * x - 16f) / Kappa;
        y = y3 > Epsilon ? y3 : (116f * y - 16f) / Kappa;
        z = z3 > Epsilon ? z3 : (116f * z - 16f) / Kappa;

        // Denormalize by reference white
        return new Vector3(x * D65.X, y * D65.Y, z * D65.Z);
    }

    // Delta E color difference computation
    public static float DeltaE2000(Vector3 lab1, Vector3 lab2)
    {
        var l1 = lab1.X;
        var a1 = lab1.Y;
        var b1 = lab1.Z;

        var l2 = lab2.X;
        var a2 = lab2.Y;
        var b2 = lab2.Z;

        // Calculate C and h
        var c1 = MathF.Sqrt(a1 * a1 + b1 * b1);
        var c2 = MathF.Sqrt(a2 * a2 + b2 * b2);
        var cAvg = (c1 + c2) / 2f;

        var g = 0.5f * (1f - MathF.Sqrt(MathF.Pow(cAvg, 7f) /
                                       (MathF.Pow(cAvg, 7f) + MathF.Pow(25f, 7f))));

        var ap1 = (1f + g) * a1;
        var ap2 = (1f + g) * a2;

        var cp1 = MathF.Sqrt(ap1 * ap1 + b1 * b1);
        var cp2 = MathF.Sqrt(ap2 * ap2 + b2 * b2);

        var hp1 = MathF.Atan2(b1, ap1);
        var hp2 = MathF.Atan2(b2, ap2);

        // Calculate deltas
        var dL = l2 - l1;
        var dC = cp2 - cp1;
        var dhp = hp2 - hp1;

        if (dhp > MathF.PI) dhp -= 2f * MathF.PI;
        if (dhp < -MathF.PI) dhp += 2f * MathF.PI;

        var dH = 2f * MathF.Sqrt(cp1 * cp2) * MathF.Sin(dhp / 2f);

        // Calculate averages
        var lAvg = (l1 + l2) / 2f;
        var cpAvg = (cp1 + cp2) / 2f;
        var hpAvg = (hp1 + hp2) / 2f;

        if (MathF.Abs(hp1 - hp2) > MathF.PI)
            hpAvg += MathF.PI;

        // Weighting functions
        var t = 1f - 0.17f * MathF.Cos(hpAvg - MathF.PI / 6f) +
                0.24f * MathF.Cos(2f * hpAvg) +
                0.32f * MathF.Cos(3f * hpAvg + MathF.PI / 30f) -
                0.20f * MathF.Cos(4f * hpAvg - 63f * MathF.PI / 180f);

        var sl = 1f + (0.015f * MathF.Pow(lAvg - 50f, 2f)) /
                 MathF.Sqrt(20f + MathF.Pow(lAvg - 50f, 2f));
        var sc = 1f + 0.045f * cpAvg;
        var sh = 1f + 0.015f * cpAvg * t;

        var rt = -2f * MathF.Sqrt(MathF.Pow(cpAvg, 7f) /
                                 (MathF.Pow(cpAvg, 7f) + MathF.Pow(25f, 7f))) *
                 MathF.Sin(60f * MathF.PI / 180f *
                          MathF.Exp(-MathF.Pow((hpAvg - 275f * MathF.PI / 180f) /
                                              (25f * MathF.PI / 180f), 2f)));

        // CIEDE2000 formula
        var kl = 1f; // Parametric factors
        var kc = 1f;
        var kh = 1f;

        return MathF.Sqrt(MathF.Pow(dL / (kl * sl), 2f) +
                         MathF.Pow(dC / (kc * sc), 2f) +
                         MathF.Pow(dH / (kh * sh), 2f) +
                         rt * (dC / (kc * sc)) * (dH / (kh * sh)));
    }
}
```

### Perceptually uniform color spaces

Perceptually uniform color spaces like Lab and Luv provide consistent visual differences across the color spectrum,
essential for color matching, quality assessment, and image processing operations. These spaces require non-linear
transformations that can be computationally expensive but are crucial for professional applications. Modern
implementations use approximations and lookup tables to balance accuracy with performance.

```csharp
// Advanced perceptual color space implementations
public class PerceptualColorSpaces
{
    // Oklab - improved perceptual uniformity over CIELAB
    public static class Oklab
    {
        // Optimized transformation matrices
        private static readonly Matrix3x3 LinearRGBToLMS = new(
            0.4122214708f, 0.5363325363f, 0.0514459929f,
            0.2119034982f, 0.6806995451f, 0.1073969566f,
            0.0883024619f, 0.2817188376f, 0.6299787005f
        );

        private static readonly Matrix3x3 LMSToOklab = new(
            0.2104542553f, 0.7936177850f, -0.0040720468f,
            1.9779984951f, -2.4285922050f, 0.4505937099f,
            0.0259040371f, 0.7827717662f, -0.8086757660f
        );

        public static Vector3 RGBToOklab(Vector3 rgb)
        {
            // Convert to linear RGB
            var linear = new Vector3(
                GammaToLinear(rgb.X),
                GammaToLinear(rgb.Y),
                GammaToLinear(rgb.Z)
            );

            // Transform to LMS
            var lms = LinearRGBToLMS * linear;

            // Apply cube root (perceptual compression)
            lms = new Vector3(
                MathF.Cbrt(lms.X),
                MathF.Cbrt(lms.Y),
                MathF.Cbrt(lms.Z)
            );

            // Transform to Oklab
            return LMSToOklab * lms;
        }

        // Fast approximation using polynomial
        private static float MathF_Cbrt(float x)
        {
            // Halley's method with good initial guess
            float y = MathF.Pow(x, 0.33333334f);
            float y3 = y * y * y;
            return y * (y3 + 2f * x) / (2f * y3 + x);
        }
    }

    // JzAzBz for HDR applications
    public static class JzAzBz
    {
        private const float B = 1.15f;
        private const float G = 0.66f;
        private const float C1 = 3424f / 4096f;
        private const float C2 = 2413f / 128f;
        private const float C3 = 2392f / 128f;
        private const float N = 2610f / 16384f;
        private const float P = 1.7f * 2523f / 32f;
        private const float D = -0.56f;
        private const float D0 = 1.6295499532821566e-11f;

        public static Vector3 XYZToJzAzBz(Vector3 xyz, float luminance = 10000f)
        {
            // Apply luminance scaling
            xyz *= luminance / 10000f;

            // XYZ to LMS
            var lms = XYZToLMS * xyz;

            // PQ encoding
            lms = ApplyPQEncoding(lms);

            // LMS to Izazbz
            var izazbz = LMSToIzazbz * lms;

            // Compute Jz with perceptual quantizer
            var jz = ((1f + D) * izazbz.X) / (1f + D * izazbz.X) - D0;

            return new Vector3(jz, izazbz.Y, izazbz.Z);
        }

        private static Vector3 ApplyPQEncoding(Vector3 lms)
        {
            return new Vector3(
                PQEncode(lms.X),
                PQEncode(lms.Y),
                PQEncode(lms.Z)
            );
        }

        private static float PQEncode(float x)
        {
            var xn = MathF.Pow(x / 10000f, N);
            return MathF.Pow((C1 + C2 * xn) / (1f + C3 * xn), P);
        }
    }
}

// GPU-accelerated color space conversion
public class GPUColorConverter
{
    private readonly ComputeShader _conversionShader;
    private readonly Dictionary<(ColorSpace, ColorSpace), int> _kernelIndices;

    public RenderTexture ConvertColorSpace(
        RenderTexture source,
        ColorSpace sourceSpace,
        ColorSpace targetSpace)
    {
        var kernelIndex = _kernelIndices[(sourceSpace, targetSpace)];

        // Set transformation parameters
        _conversionShader.SetTexture(kernelIndex, "_Source", source);
        _conversionShader.SetMatrix("_ColorMatrix",
            GetTransformMatrix(sourceSpace, targetSpace));

        // Handle non-linear operations
        if (RequiresGammaConversion(sourceSpace, targetSpace))
        {
            _conversionShader.SetBuffer(kernelIndex, "_GammaLUT",
                GetGammaLUT(sourceSpace));
        }

        var result = RenderTexture.GetTemporary(
            source.width, source.height, 0,
            GetOptimalFormat(targetSpace)
        );

        _conversionShader.SetTexture(kernelIndex, "_Result", result);

        // Dispatch with optimal thread group size
        int threadGroupsX = (source.width + 7) / 8;
        int threadGroupsY = (source.height + 7) / 8;
        _conversionShader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);

        return result;
    }
}
```

### Color gamut mapping algorithms

When converting between color spaces with different gamuts, out-of-gamut colors must be mapped to reproducible values.
Simple clipping produces unnatural results, so sophisticated gamut mapping algorithms preserve perceptual attributes
like hue and lightness relationships. The choice of algorithm depends on content type, with different strategies for
photographic images, vector graphics, and spot colors.

```csharp
// Advanced gamut mapping with multiple algorithms
public class GamutMappingEngine
{
    public enum MappingAlgorithm
    {
        Clipping,
        MinimumDeltaE,
        CUSP,
        SigmoidalCompression,
        HuePreserving,
        ChromaReduction
    }

    // Gamut boundary descriptor using convex hull
    public class GamutBoundaryDescriptor
    {
        private readonly ConvexHull3D _labHull;
        private readonly Dictionary<float, GamutSlice> _hueSlices;

        public GamutBoundaryDescriptor(ColorSpace colorSpace)
        {
            // Sample color space to build boundary
            var boundaryPoints = SampleGamutBoundary(colorSpace);
            _labHull = new ConvexHull3D(boundaryPoints);

            // Pre-compute hue slices for fast lookup
            _hueSlices = ComputeHueSlices(boundaryPoints);
        }

        public bool IsInGamut(Vector3 labColor)
        {
            return _labHull.Contains(labColor);
        }

        public Vector3 FindNearestInGamut(Vector3 labColor, MappingAlgorithm algorithm)
        {
            if (IsInGamut(labColor))
                return labColor;

            return algorithm switch
            {
                MappingAlgorithm.MinimumDeltaE => FindMinimumDeltaE(labColor),
                MappingAlgorithm.CUSP => MapToCUSP(labColor),
                MappingAlgorithm.HuePreserving => MapPreservingHue(labColor),
                MappingAlgorithm.ChromaReduction => ReduceChroma(labColor),
                _ => ClipToGamut(labColor)
            };
        }

        private Vector3 MapToCUSP(Vector3 labColor)
        {
            // Convert to LCH
            var l = labColor.X;
            var c = MathF.Sqrt(labColor.Y * labColor.Y + labColor.Z * labColor.Z);
            var h = MathF.Atan2(labColor.Z, labColor.Y);

            // Find CUSP (maximum chroma) for this hue
            var slice = GetHueSlice(h);
            var cusp = slice.FindCUSP();

            // Map along line from neutral to color through CUSP
            if (l > cusp.Lightness)
            {
                // Compress toward white
                var t = (l - cusp.Lightness) / (100f - cusp.Lightness);
                var targetC = cusp.Chroma * (1f - t);
                c = Math.Min(c, targetC);
            }
            else
            {
                // Compress toward black
                var t = l / cusp.Lightness;
                var targetC = cusp.Chroma * t;
                c = Math.Min(c, targetC);
            }

            // Convert back to Lab
            return new Vector3(l, c * MathF.Cos(h), c * MathF.Sin(h));
        }
    }

    // Sigmoidal compression for smooth gamut mapping
    public class SigmoidalGamutCompressor
    {
        private readonly float _threshold;
        private readonly float _limit;
        private readonly float _power;

        public Vector3 Compress(Vector3 color, GamutBoundaryDescriptor gamut)
        {
            // Work in JCH space for better results
            var jch = LabToJCH(color);

            // Find maximum chroma for this J,H
            var maxChroma = gamut.GetMaxChroma(jch.X, jch.Z);

            if (jch.Y <= maxChroma * _threshold)
                return color; // Within threshold, no compression

            // Apply sigmoidal compression
            var normalized = jch.Y / maxChroma;
            var compressed = SigmoidalCompress(normalized);
            jch.Y = compressed * maxChroma;

            return JCHToLab(jch);
        }

        private float SigmoidalCompress(float x)
        {
            if (x <= _threshold)
                return x;

            // Smooth compression curve
            var a = _threshold;
            var b = _limit;
            var p = _power;

            var t = (x - a) / (1 - a);
            var s = 1 - MathF.Pow(1 - t, p);

            return a + (b - a) * s;
        }
    }

    // Spectral gamut mapping for maximum accuracy
    public class SpectralGamutMapper
    {
        private readonly SpectralPowerDistribution _illuminant;
        private readonly ColorMatchingFunctions _cmf;

        public Vector3 MapSpectral(
            SpectralPowerDistribution spd,
            ColorSpace targetSpace)
        {
            // Convert spectral to XYZ
            var xyz = IntegrateSpectrum(spd);

            // Check if in gamut
            var rgb = targetSpace.FromXYZ(xyz);
            if (IsValidRGB(rgb))
                return rgb;

            // Find metameric spectral distribution within gamut
            var targetSPD = FindMetamericSPD(spd, targetSpace);

            // Convert optimized SPD to RGB
            xyz = IntegrateSpectrum(targetSPD);
            return targetSpace.FromXYZ(xyz);
        }

        private SpectralPowerDistribution FindMetamericSPD(
            SpectralPowerDistribution original,
            ColorSpace targetSpace)
        {
            // Optimization to find spectral distribution with same XYZ
            // but within target gamut
            var optimizer = new SpectralOptimizer();

            return optimizer.Optimize(original, spd =>
            {
                var xyz = IntegrateSpectrum(spd);
                var rgb = targetSpace.FromXYZ(xyz);

                // Penalty for out-of-gamut values
                var penalty = 0f;
                if (rgb.X < 0) penalty += -rgb.X;
                if (rgb.Y < 0) penalty += -rgb.Y;
                if (rgb.Z < 0) penalty += -rgb.Z;
                if (rgb.X > 1) penalty += rgb.X - 1;
                if (rgb.Y > 1) penalty += rgb.Y - 1;
                if (rgb.Z > 1) penalty += rgb.Z - 1;

                // Minimize difference from original
                var diff = SpectralDifference(original, spd);

                return diff + penalty * 1000f;
            });
        }
    }
}
```

## 13.4 Display Calibration Integration

The final step in the color management pipeline involves adapting content to the specific characteristics of the display
device. Modern displays vary wildly in their capabilities—from basic sRGB monitors to professional displays covering
Adobe RGB, from SDR panels to HDR displays capable of 1000+ nits. Effective display calibration integration ensures that
colors are reproduced as accurately as possible within the constraints of each device.

### Hardware calibration workflows

Professional display calibration requires integration with colorimeters and spectrophotometers, devices that measure
actual light output from displays. The calibration process involves displaying known color patches, measuring the
response, and computing correction curves and matrices. Modern implementations must handle various calibration hardware
protocols, manage measurement workflows, and generate accurate correction profiles.

```csharp
// Professional display calibration system
public class DisplayCalibrationSystem
{
    private readonly ICalibrationDevice _device;
    private readonly CalibrationSettings _settings;

    public async Task<CalibrationProfile> CalibrateDisplayAsync(
        IDisplay display,
        CalibrationTarget target)
    {
        var profile = new CalibrationProfile
        {
            DisplayId = display.Id,
            CalibrationDate = DateTime.UtcNow,
            Target = target
        };

        // Step 1: Pre-calibration measurement
        var preCalibration = await MeasureDisplayResponse(display);
        profile.PreCalibrationMeasurements = preCalibration;

        // Step 2: Adjust display hardware controls
        if (display.SupportsHardwareCalibration)
        {
            await OptimizeHardwareSettings(display, target);
        }

        // Step 3: Generate calibration patches
        var patches = GenerateCalibrationPatches(target);

        // Step 4: Measure patches and build model
        var measurements = new List<ColorMeasurement>();

        foreach (var patch in patches)
        {
            // Display patch
            await display.ShowColorPatch(patch);
            await Task.Delay(500); // Stabilization time

            // Measure with device
            var measurement = await _device.MeasureAsync();
            measurements.Add(new ColorMeasurement
            {
                Requested = patch,
                Measured = measurement,
                DisplaySettings = display.GetCurrentSettings()
            });

            // Update progress
            OnProgress?.Invoke(measurements.Count / (float)patches.Count);
        }

        // Step 5: Compute correction curves
        profile.Corrections = ComputeCorrections(measurements, target);

        // Step 6: Verify calibration
        profile.Verification = await VerifyCalibration(display, profile);

        return profile;
    }

    // Advanced correction computation with multiple algorithms
    private CalibrationCorrections ComputeCorrections(
        List<ColorMeasurement> measurements,
        CalibrationTarget target)
    {
        var corrections = new CalibrationCorrections();

        // Compute grayscale corrections first
        var grayscaleMeasurements = measurements
            .Where(m => IsGrayscale(m.Requested))
            .OrderBy(m => m.Requested.Y)
            .ToList();

        corrections.GrayscaleCorrection = ComputeGrayscaleCurves(
            grayscaleMeasurements,
            target
        );

        // Build 3D LUT for color corrections
        corrections.ColorLUT = Build3DLUT(measurements, target);

        // Compute white point adaptation matrix
        var measuredWhite = measurements
            .First(m => IsWhite(m.Requested))
            .Measured;

        corrections.WhitePointMatrix = ComputeWhitePointMatrix(
            measuredWhite,
            target.WhitePoint
        );

        return corrections;
    }

    // Iterative grayscale optimization
    private GrayscaleCorrection ComputeGrayscaleCurves(
        List<ColorMeasurement> grayscale,
        CalibrationTarget target)
    {
        var correction = new GrayscaleCorrection();

        // Extract individual channel responses
        var redResponse = grayscale.Select(m => new Vector2(
            m.Requested.X,
            m.Measured.X
        )).ToArray();

        var greenResponse = grayscale.Select(m => new Vector2(
            m.Requested.Y,
            m.Measured.Y
        )).ToArray();

        var blueResponse = grayscale.Select(m => new Vector2(
            m.Requested.Z,
            m.Measured.Z
        )).ToArray();

        // Fit curves to achieve target gamma
        correction.RedCurve = FitGammaCurve(redResponse, target.Gamma);
        correction.GreenCurve = FitGammaCurve(greenResponse, target.Gamma);
        correction.BlueCurve = FitGammaCurve(blueResponse, target.Gamma);

        // Optimize for neutral gray balance
        OptimizeGrayBalance(correction, grayscale, target);

        return correction;
    }

    // Matrix profiling for wide gamut displays
    public class MatrixProfileGenerator
    {
        public Matrix3x3 GenerateMatrix(
            List<PrimaryMeasurement> measurements,
            ColorSpace targetSpace)
        {
            // Measure actual primaries
            var measuredRed = measurements.First(m => m.Channel == Channel.Red);
            var measuredGreen = measurements.First(m => m.Channel == Channel.Green);
            var measuredBlue = measurements.First(m => m.Channel == Channel.Blue);
            var measuredWhite = measurements.First(m => m.Channel == Channel.White);

            // Build measured primary matrix
            var measuredMatrix = BuildMatrixFromPrimaries(
                measuredRed.XYZ,
                measuredGreen.XYZ,
                measuredBlue.XYZ,
                measuredWhite.XYZ
            );

            // Get target primary matrix
            var targetMatrix = targetSpace.GetRGBToXYZMatrix();

            // Compute correction matrix
            return targetMatrix * measuredMatrix.Inverse();
        }

        private Matrix3x3 BuildMatrixFromPrimaries(
            Vector3 red, Vector3 green, Vector3 blue, Vector3 white)
        {
            // Build matrix from primaries
            var M = new Matrix3x3(
                red.X, green.X, blue.X,
                red.Y, green.Y, blue.Y,
                red.Z, green.Z, blue.Z
            );

            // Compute scaling factors for white point
            var S = M.Inverse() * white;

            // Apply scaling
            return new Matrix3x3(
                S.X * red.X, S.Y * green.X, S.Z * blue.X,
                S.X * red.Y, S.Y * green.Y, S.Z * blue.Y,
                S.X * red.Z, S.Y * green.Z, S.Z * blue.Z
            );
        }
    }
}

// Real-time display compensation
public class RealtimeDisplayCompensation
{
    private readonly DisplayProfile _profile;
    private readonly GPU3DLUT _gpuLUT;
    private readonly ComputeBuffer _correctionBuffer;

    public void ApplyCompensation(RenderTexture source, RenderTexture destination)
    {
        // Upload correction data to GPU
        UpdateCorrectionBuffer();

        // Apply multi-stage compensation
        Graphics.SetRenderTarget(destination);

        // Stage 1: Apply 1D curves
        _compensationMaterial.SetTexture("_MainTex", source);
        _compensationMaterial.SetBuffer("_Curves", _correctionBuffer);
        _compensationMaterial.SetPass(0); // 1D LUT pass
        Graphics.Blit(source, _tempRT1, _compensationMaterial);

        // Stage 2: Apply 3D LUT
        _compensationMaterial.SetTexture("_MainTex", _tempRT1);
        _compensationMaterial.SetTexture("_LUT3D", _gpuLUT.Texture);
        _compensationMaterial.SetPass(1); // 3D LUT pass
        Graphics.Blit(_tempRT1, _tempRT2, _compensationMaterial);

        // Stage 3: Apply matrix correction
        _compensationMaterial.SetTexture("_MainTex", _tempRT2);
        _compensationMaterial.SetMatrix("_ColorMatrix", _profile.CorrectionMatrix);
        _compensationMaterial.SetPass(2); // Matrix pass
        Graphics.Blit(_tempRT2, destination, _compensationMaterial);
    }

    // Shader code for GPU compensation
    private const string CompensationShaderCode = @"
        Shader ""Hidden/DisplayCompensation""
        {
            Properties
            {
                _MainTex (""Texture"", 2D) = ""white"" {}
            }

            SubShader
            {
                // Pass 0: 1D Curves
                Pass
                {
                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag

                    sampler2D _MainTex;
                    StructuredBuffer<float> _Curves;

                    float4 frag(v2f i) : SV_Target
                    {
                        float4 color = tex2D(_MainTex, i.uv);

                        // Apply individual channel curves
                        color.r = SampleCurve(_Curves, color.r, 0);
                        color.g = SampleCurve(_Curves, color.g, 1);
                        color.b = SampleCurve(_Curves, color.b, 2);

                        return color;
                    }

                    float SampleCurve(StructuredBuffer<float> curves,
                                     float input, int channel)
                    {
                        int lutSize = 1024;
                        int offset = channel * lutSize;

                        float index = input * (lutSize - 1);
                        int i0 = floor(index);
                        int i1 = min(i0 + 1, lutSize - 1);
                        float frac = index - i0;

                        return lerp(curves[offset + i0],
                                   curves[offset + i1], frac);
                    }
                    ENDCG
                }

                // Pass 1: 3D LUT
                Pass
                {
                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag

                    sampler2D _MainTex;
                    sampler3D _LUT3D;

                    float4 frag(v2f i) : SV_Target
                    {
                        float4 color = tex2D(_MainTex, i.uv);

                        // Scale and offset for LUT sampling
                        float3 lutCoord = color.rgb * 0.9375 + 0.03125;

                        return float4(tex3D(_LUT3D, lutCoord).rgb, color.a);
                    }
                    ENDCG
                }
            }
        }
    ";
}

// Ambient light compensation
public class AmbientLightCompensation
{
    private readonly IAmbientLightSensor _sensor;
    private readonly CompensationCurveDatabase _curveDatabase;

    public async Task<ColorTransform> ComputeAmbientCompensation()
    {
        // Read ambient light characteristics
        var ambient = await _sensor.MeasureAmbientLight();

        // Compute adaptation state
        var adaptation = ComputeChromaticAdaptation(
            ambient.ColorTemperature,
            ambient.Illuminance
        );

        // Adjust for surround effects
        var surround = ComputeSurroundCompensation(
            ambient.Illuminance,
            _displayProfile.MaxLuminance
        );

        // Build compensation transform
        return new ColorTransform
        {
            ChromaticAdaptation = adaptation,
            SurroundCompensation = surround,
            GammaAdjustment = ComputeGammaAdjustment(ambient.Illuminance)
        };
    }

    private Matrix3x3 ComputeChromaticAdaptation(
        float ambientTemp,
        float illuminance)
    {
        // CIECAM02 based adaptation
        var ambientWhite = ColorTemperatureToXYZ(ambientTemp);
        var displayWhite = _displayProfile.WhitePoint;

        // Degree of adaptation based on illuminance
        var D = ComputeAdaptationDegree(illuminance);

        // Build CAT02 matrix
        return ChromaticAdaptation.ComputeCAT02(
            ambientWhite,
            displayWhite,
            D
        );
    }
}
```

### Multi-display color consistency

In multi-monitor setups, maintaining color consistency across displays with different characteristics presents unique
challenges. Each display may have different color gamuts, white points, and response curves. The system must find a
common color space that all displays can reproduce while minimizing visual discontinuities when content spans multiple
screens.

```csharp
// Multi-display synchronization system
public class MultiDisplayColorSync
{
    private readonly List<DisplayDevice> _displays;
    private readonly ColorSyncPolicy _policy;

    public class ColorSyncPolicy
    {
        public enum SyncMode
        {
            CommonGamut,        // Restrict to smallest common gamut
            IndividualOptimal,  // Optimize each display separately
            PrimaryReference,   // Match all to primary display
            SmartBoundary      // Intelligent boundary handling
        }

        public SyncMode Mode { get; set; }
        public float TransitionWidth { get; set; } = 100f; // pixels
        public bool PreserveLuminance { get; set; } = true;
    }

    public async Task SynchronizeDisplays()
    {
        // Profile all connected displays
        var profiles = await ProfileAllDisplays();

        // Compute synchronization strategy
        var strategy = ComputeSyncStrategy(profiles, _policy);

        // Generate correction profiles
        foreach (var display in _displays)
        {
            var correction = GenerateCorrectionProfile(
                display,
                profiles[display.Id],
                strategy
            );

            await ApplyCorrectionProfile(display, correction);
        }

        // Setup boundary compensation for spanning windows
        if (_policy.Mode == ColorSyncPolicy.SyncMode.SmartBoundary)
        {
            SetupBoundaryCompensation(profiles);
        }
    }

    // Smart boundary compensation for spanning content
    private class BoundaryCompensator
    {
        private readonly Dictionary<DisplayPair, BoundaryTransform> _transforms;

        public void CompensateSpanningWindow(
            Window window,
            RenderTexture content)
        {
            var displayRegions = GetDisplayRegions(window);

            foreach (var region in displayRegions)
            {
                if (IsBoundaryRegion(region))
                {
                    // Apply smooth transition
                    var transform = GetBoundaryTransform(
                        region.LeftDisplay,
                        region.RightDisplay
                    );

                    ApplyGradientTransform(
                        content,
                        region,
                        transform
                    );
                }
                else
                {
                    // Apply display-specific correction
                    ApplyDisplayCorrection(
                        content,
                        region,
                        region.Display.CorrectionProfile
                    );
                }
            }
        }

        private void ApplyGradientTransform(
            RenderTexture content,
            BoundaryRegion region,
            BoundaryTransform transform)
        {
            // Compute gradient across boundary
            _boundaryShader.SetTexture("_MainTex", content);
            _boundaryShader.SetMatrix("_LeftTransform", transform.LeftMatrix);
            _boundaryShader.SetMatrix("_RightTransform", transform.RightMatrix);
            _boundaryShader.SetFloat("_TransitionWidth", _policy.TransitionWidth);
            _boundaryShader.SetVector("_BoundaryLine", region.BoundaryLine);

            Graphics.Blit(content, region.Target, _boundaryShader);
        }
    }
}

// HDR display tone mapping
public class HDRDisplayMapper
{
    private readonly HDRDisplayProfile _displayProfile;
    private readonly ToneMappingSettings _settings;

    public RenderTexture MapToDisplay(
        RenderTexture hdrContent,
        ContentMetadata metadata)
    {
        // Analyze content characteristics
        var analysis = AnalyzeHDRContent(hdrContent, metadata);

        // Select appropriate tone mapping
        var toneMapper = SelectToneMapper(analysis, _displayProfile);

        // Apply display-specific mapping
        return ApplyDisplayMapping(
            hdrContent,
            toneMapper,
            _displayProfile,
            metadata
        );
    }

    private IToneMapper SelectToneMapper(
        ContentAnalysis analysis,
        HDRDisplayProfile display)
    {
        // Match content to display capabilities
        var contentRange = analysis.MaxLuminance - analysis.MinLuminance;
        var displayRange = display.MaxLuminance - display.MinLuminance;

        if (contentRange <= displayRange &&
            analysis.MaxLuminance <= display.MaxLuminance)
        {
            // Content fits within display capability
            return new DirectMapper();
        }
        else if (analysis.ContentType == ContentType.Game)
        {
            // Preserve contrast for games
            return new ReinhardExtendedMapper
            {
                WhitePoint = display.MaxLuminance * 0.8f,
                Shoulder = 0.95f
            };
        }
        else if (analysis.ContentType == ContentType.Cinema)
        {
            // Filmic look for video
            return new ACESMapper
            {
                ReferenceWhite = metadata.MasteringDisplay.MaxLuminance,
                TargetPeak = display.MaxLuminance
            };
        }
        else
        {
            // Adaptive mapper for general content
            return new AdaptiveToneMapper
            {
                PreserveShadows = true,
                ProtectHighlights = true,
                TargetDisplay = display
            };
        }
    }
}
```

### Performance optimization strategies

Real-time color management requires careful optimization to maintain high frame rates while applying complex
transformations. GPU acceleration, lookup table optimization, and intelligent caching strategies enable color-accurate
rendering without sacrificing performance. Modern implementations leverage compute shaders, texture arrays, and
specialized hardware features.

```csharp
// High-performance color pipeline
public class OptimizedColorPipeline
{
    private readonly GPUResourcePool _resourcePool;
    private readonly ShaderCache _shaderCache;
    private readonly LUTCache _lutCache;

    // Optimized 3D LUT implementation
    public class GPU3DLUTOptimized
    {
        private readonly Texture3D _lutTexture;
        private readonly ComputeShader _applyShader;
        private readonly int _lutSize;

        public void Apply(RenderTexture source, RenderTexture dest)
        {
            // Use compute shader for better performance
            int kernel = _applyShader.FindKernel("ApplyLUT3D");

            _applyShader.SetTexture(kernel, "_Input", source);
            _applyShader.SetTexture(kernel, "_Output", dest);
            _applyShader.SetTexture(kernel, "_LUT", _lutTexture);

            // Optimal dispatch size
            int threadsX = (source.width + 15) / 16;
            int threadsY = (source.height + 15) / 16;

            _applyShader.Dispatch(kernel, threadsX, threadsY, 1);
        }

        // Tetrahedral interpolation in shader
        private const string LUT3DShaderCode = @"
            #pragma kernel ApplyLUT3D

            Texture2D<float4> _Input;
            RWTexture2D<float4> _Output;
            Texture3D<float4> _LUT;

            [numthreads(16, 16, 1)]
            void ApplyLUT3D(uint3 id : SV_DispatchThreadID)
            {
                float4 color = _Input[id.xy];

                // Scale to LUT coordinates
                float3 lutCoord = color.rgb * (_LUTSize - 1);
                int3 p0 = floor(lutCoord);
                float3 f = lutCoord - p0;

                // Tetrahedral interpolation
                float4 result;
                if (f.x > f.y)
                {
                    if (f.y > f.z)
                    {
                        // Tetrahedron 1
                        result = (1-f.x) * _LUT[p0] +
                                (f.x-f.y) * _LUT[p0 + int3(1,0,0)] +
                                (f.y-f.z) * _LUT[p0 + int3(1,1,0)] +
                                f.z * _LUT[p0 + int3(1,1,1)];
                    }
                    else if (f.x > f.z)
                    {
                        // Tetrahedron 2
                        result = (1-f.x) * _LUT[p0] +
                                (f.x-f.z) * _LUT[p0 + int3(1,0,0)] +
                                (f.z-f.y) * _LUT[p0 + int3(1,0,1)] +
                                f.y * _LUT[p0 + int3(1,1,1)];
                    }
                    else
                    {
                        // Tetrahedron 3
                        result = (1-f.z) * _LUT[p0] +
                                (f.z-f.x) * _LUT[p0 + int3(0,0,1)] +
                                (f.x-f.y) * _LUT[p0 + int3(1,0,1)] +
                                f.y * _LUT[p0 + int3(1,1,1)];
                    }
                }
                else
                {
                    // ... remaining tetrahedra
                }

                _Output[id.xy] = float4(result.rgb, color.a);
            }
        ";
    }

    // Cached transform chains
    public class TransformChainOptimizer
    {
        private readonly Dictionary<TransformKey, TransformChain> _chains;

        public TransformChain GetOptimizedChain(
            ColorSpace source,
            ColorSpace destination,
            RenderingIntent intent)
        {
            var key = new TransformKey(source, destination, intent);

            if (_chains.TryGetValue(key, out var cached))
                return cached;

            // Build optimized chain
            var chain = new TransformChain();

            // Combine multiple matrix operations
            var combinedMatrix = CombineMatrices(source, destination);

            // Merge adjacent 1D LUTs
            var merged1DLUT = Merge1DLUTs(source, destination);

            // Optimize 3D LUT size based on precision needs
            var optimal3DSize = DetermineLUTSize(source, destination, intent);

            chain.AddStage(new MatrixStage(combinedMatrix));
            chain.AddStage(new LUT1DStage(merged1DLUT));
            chain.AddStage(new LUT3DStage(Generate3DLUT(optimal3DSize)));

            _chains[key] = chain;
            return chain;
        }
    }

    // Parallel color processing
    public class ParallelColorProcessor
    {
        private readonly int _workerCount;
        private readonly Channel<ColorTask> _taskChannel;

        public async Task ProcessAsync(
            ColorImage source,
            ColorImage destination,
            IColorTransform transform)
        {
            var tileSize = DetermineOptimalTileSize(source.Width, source.Height);
            var tiles = GenerateTiles(source, tileSize);

            // Process tiles in parallel
            await Parallel.ForEachAsync(tiles, async (tile, ct) =>
            {
                var buffer = _bufferPool.Rent(tile.Size);

                try
                {
                    // Copy tile to buffer
                    CopyTileToBuffer(source, tile, buffer);

                    // Apply transformation
                    transform.ProcessBuffer(buffer, tile.Width, tile.Height);

                    // Copy back to destination
                    CopyBufferToTile(buffer, destination, tile);
                }
                finally
                {
                    _bufferPool.Return(buffer);
                }
            });
        }
    }
}
```

### Summary

Color space management represents a critical component of modern graphics processing systems, bridging the gap between
the physical properties of light, the biological mechanisms of human vision, and the technical constraints of display
devices. Through careful implementation of ICC profiles, wide gamut support, efficient conversion algorithms, and
display calibration integration, applications can preserve artistic intent across the entire imaging pipeline.

The techniques presented in this chapter—from the mathematical foundations of color transformation to the practical
realities of multi-display synchronization—provide the tools necessary to build color-accurate graphics applications. As
display technology continues to evolve with wider gamuts, higher dynamic ranges, and new color spaces, these fundamental
principles and architectures will adapt to meet new challenges while maintaining the core goal: reproducing colors as
intended, regardless of the viewing environment or display device.

Whether building professional photo editing software, game engines, or medical imaging systems, the color management
strategies explored here ensure that pixels on screen accurately represent the creator's vision, maintaining the
emotional impact and informational content that color conveys in our increasingly visual digital world.
