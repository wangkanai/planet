# Chapter 14: Metadata Handling Systems

Modern image files contain far more than pixel data—they carry rich metadata describing everything from camera settings
to copyright information. This chapter explores the architecture and implementation of comprehensive metadata handling
systems in .NET 9.0, covering the major standards (EXIF, IPTC, XMP), custom schema design, preservation strategies, and
the critical performance considerations that separate professional implementations from basic metadata readers.

## 14.1 EXIF, IPTC, and XMP Standards

### Understanding the metadata ecosystem

The evolution of image metadata reflects the growing complexity of digital imaging workflows. **EXIF** (Exchangeable
Image File Format) emerged from the digital camera industry, encoding technical capture parameters directly into image
files. **IPTC** (International Press Telecommunications Council) arose from journalism's need for standardized caption
and copyright information. **XMP** (Extensible Metadata Platform) represents Adobe's attempt to unify metadata handling
through RDF/XML, providing extensibility that earlier binary formats lacked.

Each standard serves distinct purposes while overlapping in functionality. EXIF excels at technical data—exposure
settings, GPS coordinates, lens information. IPTC focuses on editorial metadata—captions, keywords, usage rights. XMP
provides a framework for both while enabling custom schemas for specialized applications. Understanding these
distinctions guides architectural decisions in metadata system design.

### EXIF implementation architecture

EXIF metadata follows the TIFF IFD (Image File Directory) structure, requiring careful binary parsing with attention to
endianness, pointer chains, and nested sub-IFDs. The implementation must handle both standard tags defined by the EXIF
specification and proprietary maker notes that vary by manufacturer.

```csharp
// Comprehensive EXIF reader with maker note support
public class ExifReader
{
    private readonly Dictionary<ushort, ExifTagDefinition> _standardTags;
    private readonly Dictionary<string, IMakerNoteParser> _makerNoteParsers;
    private readonly MemoryPool<byte> _memoryPool;

    public ExifReader()
    {
        _standardTags = ExifTagDefinitions.LoadStandardTags();
        _makerNoteParsers = new Dictionary<string, IMakerNoteParser>
        {
            ["Canon"] = new CanonMakerNoteParser(),
            ["Nikon"] = new NikonMakerNoteParser(),
            ["Sony"] = new SonyMakerNoteParser(),
            ["Fujifilm"] = new FujifilmMakerNoteParser()
        };
        _memoryPool = MemoryPool<byte>.Shared;
    }

    public async Task<ExifData> ReadExifAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        // Validate stream contains EXIF data
        var header = await ReadHeaderAsync(stream, cancellationToken);
        if (!IsValidExifHeader(header))
        {
            return null;
        }

        // Determine byte order
        var byteOrder = DetermineByteOrder(header);
        using var reader = new EndianBinaryReader(stream, byteOrder, leaveOpen: true);

        // Read TIFF header
        var tiffMagic = reader.ReadUInt16();
        if (tiffMagic != 0x002A) // TIFF magic number
        {
            throw new InvalidDataException($"Invalid TIFF magic number: 0x{tiffMagic:X4}");
        }

        // Read IFD offset
        var ifdOffset = reader.ReadUInt32();

        // Parse IFD chain with cycle detection
        var exifData = new ExifData();
        var visitedOffsets = new HashSet<uint>();
        await ParseIfdChainAsync(reader, ifdOffset, exifData, visitedOffsets, cancellationToken);

        return exifData;
    }

    private async Task ParseIfdChainAsync(
        EndianBinaryReader reader,
        uint offset,
        ExifData exifData,
        HashSet<uint> visitedOffsets,
        CancellationToken cancellationToken)
    {
        while (offset != 0)
        {
            // Prevent infinite loops from corrupted data
            if (!visitedOffsets.Add(offset))
            {
                throw new InvalidDataException($"Circular IFD reference detected at offset {offset}");
            }

            reader.BaseStream.Seek(offset, SeekOrigin.Begin);

            var entryCount = reader.ReadUInt16();
            if (entryCount > 1000) // Sanity check
            {
                throw new InvalidDataException($"Suspicious IFD entry count: {entryCount}");
            }

            // Process directory entries
            for (int i = 0; i < entryCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entry = ReadDirectoryEntry(reader);
                await ProcessDirectoryEntryAsync(reader, entry, exifData, visitedOffsets, cancellationToken);
            }

            // Read next IFD offset
            offset = reader.ReadUInt32();
        }
    }

    private async Task ProcessDirectoryEntryAsync(
        EndianBinaryReader reader,
        DirectoryEntry entry,
        ExifData exifData,
        HashSet<uint> visitedOffsets,
        CancellationToken cancellationToken)
    {
        // Handle special IFD pointers
        switch (entry.Tag)
        {
            case 0x8769: // EXIF SubIFD
                await ParseIfdChainAsync(reader, entry.ValueOffset, exifData, visitedOffsets, cancellationToken);
                return;

            case 0x8825: // GPS IFD
                var gpsData = new GpsData();
                await ParseGpsIfdAsync(reader, entry.ValueOffset, gpsData, cancellationToken);
                exifData.GpsData = gpsData;
                return;

            case 0xA005: // Interoperability IFD
                await ParseInteropIfdAsync(reader, entry.ValueOffset, exifData, cancellationToken);
                return;
        }

        // Read tag value
        var value = await ReadTagValueAsync(reader, entry, cancellationToken);

        // Special handling for maker notes
        if (entry.Tag == 0x927C) // MakerNote
        {
            await ProcessMakerNoteAsync(reader, value as byte[], exifData, cancellationToken);
            return;
        }

        // Store in appropriate collection
        if (_standardTags.TryGetValue(entry.Tag, out var tagDef))
        {
            exifData.AddTag(tagDef.Name, value, tagDef.Category);
        }
        else
        {
            // Preserve unknown tags for round-trip fidelity
            exifData.AddUnknownTag(entry.Tag, value, entry.Type);
        }
    }

    private async Task<object> ReadTagValueAsync(
        EndianBinaryReader reader,
        DirectoryEntry entry,
        CancellationToken cancellationToken)
    {
        var dataSize = GetDataSize(entry.Type) * entry.Count;

        // Inline data optimization
        if (dataSize <= 4)
        {
            return ParseInlineValue(entry.ValueOffset, entry.Type, entry.Count);
        }

        // Read from offset
        var currentPosition = reader.BaseStream.Position;
        reader.BaseStream.Seek(entry.ValueOffset, SeekOrigin.Begin);

        using var buffer = _memoryPool.Rent((int)dataSize);
        var memory = buffer.Memory.Slice(0, (int)dataSize);
        await reader.BaseStream.ReadAsync(memory, cancellationToken);

        var value = ParseValue(memory.Span, entry.Type, entry.Count);

        reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
        return value;
    }

    private async Task ProcessMakerNoteAsync(
        EndianBinaryReader reader,
        byte[] makerNoteData,
        ExifData exifData,
        CancellationToken cancellationToken)
    {
        // Identify manufacturer
        var make = exifData.GetTag<string>("Make")?.Trim();
        if (string.IsNullOrEmpty(make))
        {
            return;
        }

        // Find appropriate parser
        if (_makerNoteParsers.TryGetValue(make, out var parser))
        {
            try
            {
                var makerData = await parser.ParseAsync(makerNoteData, cancellationToken);
                exifData.MakerNoteData = makerData;
            }
            catch (Exception ex)
            {
                // Log but don't fail - maker notes are often poorly documented
                exifData.AddWarning($"Failed to parse {make} maker notes: {ex.Message}");
            }
        }
    }
}

// Type-safe EXIF value representation
public class ExifData
{
    private readonly Dictionary<string, ExifValue> _tags = new();
    private readonly Dictionary<ushort, RawExifValue> _unknownTags = new();
    private readonly List<string> _warnings = new();

    public IReadOnlyDictionary<string, ExifValue> Tags => _tags;
    public GpsData GpsData { get; set; }
    public IMakerNoteData MakerNoteData { get; set; }

    public void AddTag(string name, object value, ExifCategory category)
    {
        _tags[name] = new ExifValue(name, value, category);
    }

    public void AddUnknownTag(ushort tag, object value, ExifType type)
    {
        _unknownTags[tag] = new RawExifValue(tag, value, type);
    }

    public T GetTag<T>(string name)
    {
        if (_tags.TryGetValue(name, out var value))
        {
            return value.GetValue<T>();
        }
        return default;
    }

    // Structured GPS data access
    public (double? latitude, double? longitude) GetGpsCoordinates()
    {
        if (GpsData == null)
            return (null, null);

        return (GpsData.GetLatitude(), GpsData.GetLongitude());
    }

    // Common photography queries
    public ExposureInfo GetExposureInfo()
    {
        return new ExposureInfo
        {
            FNumber = GetTag<Rational?>("FNumber")?.ToDouble(),
            ExposureTime = GetTag<Rational?>("ExposureTime")?.ToDouble(),
            ISO = GetTag<ushort?>("ISOSpeedRatings"),
            ExposureBias = GetTag<SRational?>("ExposureBiasValue")?.ToDouble(),
            FocalLength = GetTag<Rational?>("FocalLength")?.ToDouble(),
            FocalLength35mm = GetTag<ushort?>("FocalLengthIn35mmFilm")
        };
    }
}
```

### IPTC-IIM implementation

IPTC Information Interchange Model (IIM) predates XMP and uses a binary format embedded within image files. While
largely superseded by IPTC Core in XMP, many legacy workflows still depend on IPTC-IIM, requiring continued support. The
implementation must handle the dataset/record structure and character encoding challenges inherent in the format.

```csharp
// IPTC-IIM reader with proper encoding support
public class IptcReader
{
    private const byte IptcMarker = 0x1C;
    private readonly Dictionary<(byte, byte), IptcDataSet> _dataSets;

    public IptcReader()
    {
        _dataSets = IptcDataSets.LoadStandardDataSets();
    }

    public IptcData ReadIptc(Stream stream)
    {
        var iptcData = new IptcData();
        var reader = new BinaryReader(stream);

        while (stream.Position < stream.Length - 5)
        {
            // Look for IPTC marker
            if (reader.ReadByte() != IptcMarker)
                continue;

            var record = reader.ReadByte();
            var dataSet = reader.ReadByte();

            // Read data size
            var sizeBytes = reader.ReadBytes(2);
            int dataSize;

            if (sizeBytes[0] < 0x80)
            {
                // Standard size format
                dataSize = (sizeBytes[0] << 8) | sizeBytes[1];
            }
            else
            {
                // Extended size format
                var extendedSizeLength = sizeBytes[0] & 0x7F;
                var extendedSize = 0;

                for (int i = 0; i < extendedSizeLength; i++)
                {
                    extendedSize = (extendedSize << 8) | reader.ReadByte();
                }

                dataSize = extendedSize;
            }

            // Read data
            var data = reader.ReadBytes(dataSize);

            // Process based on dataset type
            if (_dataSets.TryGetValue((record, dataSet), out var dataSetInfo))
            {
                var value = DecodeValue(data, dataSetInfo);
                iptcData.AddDataSet(dataSetInfo.Name, value, dataSetInfo.IsRepeatable);
            }
            else
            {
                // Preserve unknown datasets
                iptcData.AddUnknownDataSet(record, dataSet, data);
            }
        }

        return iptcData;
    }

    private object DecodeValue(byte[] data, IptcDataSet dataSetInfo)
    {
        // Handle character encoding
        var encoding = dataSetInfo.Record == 1 && dataSetInfo.DataSet == 90
            ? GetEncodingFromCodedCharacterSet(data)
            : Encoding.UTF8; // Default to UTF-8

        switch (dataSetInfo.Type)
        {
            case IptcType.String:
                return encoding.GetString(data).TrimEnd('\0');

            case IptcType.Date:
                return ParseIptcDate(data);

            case IptcType.Time:
                return ParseIptcTime(data);

            case IptcType.Binary:
                return data;

            default:
                return data;
        }
    }

    private DateTime? ParseIptcDate(byte[] data)
    {
        if (data.Length != 8)
            return null;

        var dateStr = Encoding.ASCII.GetString(data);
        if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, DateTimeStyles.None, out var date))
        {
            return date;
        }

        return null;
    }
}

// IPTC data container with semantic access
public class IptcData
{
    private readonly Dictionary<string, List<object>> _dataSets = new();
    private readonly List<UnknownDataSet> _unknownDataSets = new();

    public void AddDataSet(string name, object value, bool isRepeatable)
    {
        if (!_dataSets.TryGetValue(name, out var values))
        {
            values = new List<object>();
            _dataSets[name] = values;
        }

        if (isRepeatable || values.Count == 0)
        {
            values.Add(value);
        }
        else
        {
            values[0] = value; // Replace existing
        }
    }

    // Semantic accessors for common fields
    public string Caption => GetString("Caption/Abstract");
    public string Headline => GetString("Headline");
    public string Credit => GetString("Credit");
    public string Source => GetString("Source");
    public string Copyright => GetString("CopyrightNotice");
    public List<string> Keywords => GetStringList("Keywords");
    public DateTime? DateCreated => GetDate("DateCreated");
    public string City => GetString("City");
    public string Country => GetString("Country/PrimaryLocationName");

    private string GetString(string name)
    {
        if (_dataSets.TryGetValue(name, out var values) && values.Count > 0)
        {
            return values[0] as string;
        }
        return null;
    }

    private List<string> GetStringList(string name)
    {
        if (_dataSets.TryGetValue(name, out var values))
        {
            return values.OfType<string>().ToList();
        }
        return new List<string>();
    }
}
```

### XMP architecture and extensibility

XMP's RDF/XML foundation provides unparalleled extensibility but requires sophisticated parsing and serialization. The
implementation must handle multiple serialization formats (compact, canonical, pretty-printed), namespace management,
and proper RDF constructs (bags, sequences, alternatives, structures).

```csharp
// Comprehensive XMP processor with full RDF support
public class XmpProcessor
{
    private readonly Dictionary<string, XmpSchema> _schemas;
    private readonly XmpSerializationOptions _defaultOptions;

    public XmpProcessor()
    {
        _schemas = new Dictionary<string, XmpSchema>();
        RegisterStandardSchemas();

        _defaultOptions = new XmpSerializationOptions
        {
            OmitPacketWrapper = false,
            UseCanonicalFormat = false,
            Indent = true,
            NewlineStyle = Environment.NewLine
        };
    }

    private void RegisterStandardSchemas()
    {
        // Dublin Core
        RegisterSchema(new XmpSchema(
            "http://purl.org/dc/elements/1.1/",
            "dc",
            new[]
            {
                new XmpProperty("title", XmpValueType.LangAlt),
                new XmpProperty("creator", XmpValueType.Seq),
                new XmpProperty("description", XmpValueType.LangAlt),
                new XmpProperty("subject", XmpValueType.Bag),
                new XmpProperty("rights", XmpValueType.LangAlt)
            }));

        // XMP Basic
        RegisterSchema(new XmpSchema(
            "http://ns.adobe.com/xap/1.0/",
            "xmp",
            new[]
            {
                new XmpProperty("CreateDate", XmpValueType.Date),
                new XmpProperty("ModifyDate", XmpValueType.Date),
                new XmpProperty("MetadataDate", XmpValueType.Date),
                new XmpProperty("CreatorTool", XmpValueType.Text),
                new XmpProperty("Rating", XmpValueType.Integer)
            }));

        // IPTC Core
        RegisterSchema(new XmpSchema(
            "http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/",
            "Iptc4xmpCore",
            new[]
            {
                new XmpProperty("Location", XmpValueType.Text),
                new XmpProperty("CountryCode", XmpValueType.Text),
                new XmpProperty("Scene", XmpValueType.Bag),
                new XmpProperty("SubjectCode", XmpValueType.Bag)
            }));
    }

    public async Task<XmpDocument> ParseXmpAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        // Find XMP packet in stream
        var packet = await FindXmpPacketAsync(stream, cancellationToken);
        if (packet == null)
        {
            return null;
        }

        // Parse XML with namespace awareness
        var settings = new XmlReaderSettings
        {
            Async = true,
            IgnoreWhitespace = true,
            IgnoreComments = true,
            DtdProcessing = DtdProcessing.Prohibit // Security
        };

        using var stringReader = new StringReader(packet);
        using var xmlReader = XmlReader.Create(stringReader, settings);

        var document = new XmpDocument();
        await ParseRdfRootAsync(xmlReader, document, cancellationToken);

        return document;
    }

    private async Task ParseRdfRootAsync(
        XmlReader reader,
        XmpDocument document,
        CancellationToken cancellationToken)
    {
        // Navigate to RDF root
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "RDF" &&
                reader.NamespaceURI == "http://www.w3.org/1999/02/22-rdf-syntax-ns#")
            {
                break;
            }
        }

        // Process Description elements
        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Description")
            {
                await ParseDescriptionAsync(reader, document, cancellationToken);
            }
        }
    }

    private async Task ParseDescriptionAsync(
        XmlReader reader,
        XmpDocument document,
        CancellationToken cancellationToken)
    {
        // Read about attribute
        var about = reader.GetAttribute("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        document.About = about;

        // Collect namespace declarations
        var namespaces = new Dictionary<string, string>();
        if (reader.MoveToFirstAttribute())
        {
            do
            {
                if (reader.Prefix == "xmlns")
                {
                    namespaces[reader.LocalName] = reader.Value;
                    document.RegisterNamespace(reader.LocalName, reader.Value);
                }
            } while (reader.MoveToNextAttribute());

            reader.MoveToElement();
        }

        // Process properties
        using var subtreeReader = reader.ReadSubtree();
        while (await subtreeReader.ReadAsync())
        {
            if (subtreeReader.NodeType == XmlNodeType.Element &&
                subtreeReader.Depth > 0)
            {
                await ParsePropertyAsync(subtreeReader, document, cancellationToken);
            }
        }
    }

    private async Task ParsePropertyAsync(
        XmlReader reader,
        XmpDocument document,
        CancellationToken cancellationToken)
    {
        var namespaceUri = reader.NamespaceURI;
        var propertyName = reader.LocalName;

        // Check for RDF type attributes
        var parseType = reader.GetAttribute("parseType", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");

        if (parseType == "Resource")
        {
            // Struct value
            var structValue = await ParseStructAsync(reader, cancellationToken);
            document.SetProperty(namespaceUri, propertyName, structValue);
        }
        else if (await HasRdfCollectionAsync(reader))
        {
            // Array value (Seq, Bag, or Alt)
            var arrayValue = await ParseArrayAsync(reader, cancellationToken);
            document.SetProperty(namespaceUri, propertyName, arrayValue);
        }
        else
        {
            // Simple value
            var textValue = await reader.GetValueAsync();
            var xmlLang = reader.GetAttribute("lang", "http://www.w3.org/XML/1998/namespace");

            if (!string.IsNullOrEmpty(xmlLang))
            {
                // Language alternative
                var langAlt = new XmpLangAlt();
                langAlt.AddValue(xmlLang, textValue);
                document.SetProperty(namespaceUri, propertyName, langAlt);
            }
            else
            {
                document.SetProperty(namespaceUri, propertyName, textValue);
            }
        }
    }

    public async Task<byte[]> SerializeXmpAsync(
        XmpDocument document,
        XmpSerializationOptions options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= _defaultOptions;

        using var output = new MemoryStream();
        using var writer = new StreamWriter(output, Encoding.UTF8);

        // Write packet header if requested
        if (!options.OmitPacketWrapper)
        {
            await writer.WriteLineAsync("<?xpacket begin=\"\ufeff\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>");
        }

        // Write XMP
        await WriteXmpDocumentAsync(writer, document, options, cancellationToken);

        // Write packet trailer
        if (!options.OmitPacketWrapper)
        {
            var padding = GeneratePadding(options.Padding);
            await writer.WriteAsync(padding);
            await writer.WriteLineAsync("<?xpacket end=\"w\"?>");
        }

        await writer.FlushAsync();
        return output.ToArray();
    }

    private string GeneratePadding(int targetSize)
    {
        // XMP padding for in-place updates
        const string paddingUnit = "                                                                                ";
        var sb = new StringBuilder(targetSize);

        while (sb.Length < targetSize - paddingUnit.Length)
        {
            sb.AppendLine(paddingUnit);
        }

        // Fill remaining
        if (sb.Length < targetSize)
        {
            sb.Append(' ', targetSize - sb.Length);
        }

        return sb.ToString();
    }
}

// Type-safe XMP value representations
public abstract class XmpValue
{
    public abstract XmpValueType ValueType { get; }
    public abstract void WriteTo(XmlWriter writer);
    public abstract object GetValue();
}

public class XmpArray : XmpValue
{
    private readonly List<XmpValue> _items = new();
    private readonly XmpArrayType _arrayType;

    public XmpArray(XmpArrayType arrayType)
    {
        _arrayType = arrayType;
    }

    public override XmpValueType ValueType => _arrayType switch
    {
        XmpArrayType.Seq => XmpValueType.Seq,
        XmpArrayType.Bag => XmpValueType.Bag,
        XmpArrayType.Alt => XmpValueType.Alt,
        _ => throw new InvalidOperationException()
    };

    public void AddItem(XmpValue value)
    {
        _items.Add(value);
    }

    public override void WriteTo(XmlWriter writer)
    {
        var elementName = _arrayType.ToString();
        writer.WriteStartElement(elementName, "http://www.w3.org/1999/02/22-rdf-syntax-ns#");

        foreach (var item in _items)
        {
            writer.WriteStartElement("li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            item.WriteTo(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    public override object GetValue() => _items.AsReadOnly();
}

public class XmpStruct : XmpValue
{
    private readonly Dictionary<string, XmpValue> _fields = new();

    public override XmpValueType ValueType => XmpValueType.Struct;

    public void SetField(string namespaceUri, string fieldName, XmpValue value)
    {
        var key = $"{{{namespaceUri}}}{fieldName}";
        _fields[key] = value;
    }

    public override void WriteTo(XmlWriter writer)
    {
        writer.WriteAttributeString("parseType", "http://www.w3.org/1999/02/22-rdf-syntax-ns#", "Resource");

        foreach (var (key, value) in _fields)
        {
            // Extract namespace and local name
            var match = Regex.Match(key, @"^\{(.+)\}(.+)$");
            if (match.Success)
            {
                writer.WriteStartElement(match.Groups[2].Value, match.Groups[1].Value);
                value.WriteTo(writer);
                writer.WriteEndElement();
            }
        }
    }

    public override object GetValue() => _fields.AsReadOnly();
}
```

## 14.2 Custom Metadata Schemas

### Designing extensible metadata schemas

Custom metadata schemas enable domain-specific information storage while maintaining compatibility with standard XMP
processors. Well-designed schemas follow RDF principles, use appropriate value types, and provide clear semantics for
automated processing. The architecture must support schema registration, validation, and versioning.

```csharp
// Custom schema framework with validation
public class CustomXmpSchema : XmpSchema
{
    private readonly Dictionary<string, PropertyValidator> _validators;
    private readonly Version _schemaVersion;

    public CustomXmpSchema(
        string namespaceUri,
        string preferredPrefix,
        Version schemaVersion,
        IEnumerable<XmpPropertyDefinition> properties)
        : base(namespaceUri, preferredPrefix, properties)
    {
        _schemaVersion = schemaVersion;
        _validators = new Dictionary<string, PropertyValidator>();

        foreach (var prop in properties)
        {
            if (prop.Validator != null)
            {
                _validators[prop.Name] = prop.Validator;
            }
        }
    }

    public ValidationResult ValidateDocument(XmpDocument document)
    {
        var result = new ValidationResult();

        foreach (var property in Properties)
        {
            var value = document.GetProperty(NamespaceUri, property.Name);

            if (property.IsRequired && value == null)
            {
                result.AddError($"Required property '{property.Name}' is missing");
                continue;
            }

            if (value != null && _validators.TryGetValue(property.Name, out var validator))
            {
                var validationErrors = validator.Validate(value);
                result.AddErrors(validationErrors);
            }
        }

        return result;
    }

    // Schema evolution support
    public XmpValue MigrateValue(string propertyName, XmpValue oldValue, Version fromVersion)
    {
        // Handle schema version migrations
        if (fromVersion < new Version(2, 0) && _schemaVersion >= new Version(2, 0))
        {
            // Example: v1 to v2 migration
            switch (propertyName)
            {
                case "LegacyField":
                    // Convert old format to new structure
                    var newStruct = new XmpStruct();
                    newStruct.SetField(NamespaceUri, "ModernField", oldValue);
                    newStruct.SetField(NamespaceUri, "MigrationDate",
                        new XmpText(DateTime.UtcNow.ToString("O")));
                    return newStruct;

                default:
                    return oldValue;
            }
        }

        return oldValue;
    }
}

// Example: Photography workflow schema
public class PhotographyWorkflowSchema : CustomXmpSchema
{
    public const string NamespaceUri = "http://example.com/xmp/workflow/1.0/";
    public const string PreferredPrefix = "workflow";

    public PhotographyWorkflowSchema() : base(
        NamespaceUri,
        PreferredPrefix,
        new Version(1, 0),
        DefineProperties())
    {
    }

    private static IEnumerable<XmpPropertyDefinition> DefineProperties()
    {
        yield return new XmpPropertyDefinition(
            "ProcessingStage",
            XmpValueType.Text,
            "Current stage in workflow",
            isRequired: true,
            validator: new ChoiceValidator("Raw", "Developed", "Edited", "Final", "Archived"));

        yield return new XmpPropertyDefinition(
            "EditingHistory",
            XmpValueType.Seq,
            "Sequence of editing operations",
            elementType: XmpValueType.Struct);

        yield return new XmpPropertyDefinition(
            "ColorGrading",
            XmpValueType.Struct,
            "Color grading parameters");

        yield return new XmpPropertyDefinition(
            "ClientApproval",
            XmpValueType.Struct,
            "Client approval information");

        yield return new XmpPropertyDefinition(
            "PublicationRights",
            XmpValueType.Bag,
            "Publication usage rights");

        yield return new XmpPropertyDefinition(
            "ArchiveLocation",
            XmpValueType.Text,
            "Long-term archive location",
            validator: new UriValidator());
    }

    // Structured property definitions
    public static XmpStruct CreateEditingOperation(
        string operation,
        DateTime timestamp,
        string software,
        Dictionary<string, object> parameters = null)
    {
        var editOp = new XmpStruct();
        editOp.SetField(NamespaceUri, "Operation", new XmpText(operation));
        editOp.SetField(NamespaceUri, "Timestamp", new XmpDate(timestamp));
        editOp.SetField(NamespaceUri, "Software", new XmpText(software));

        if (parameters != null)
        {
            var paramStruct = new XmpStruct();
            foreach (var (key, value) in parameters)
            {
                paramStruct.SetField(NamespaceUri, key,
                    XmpValue.CreateFrom(value));
            }
            editOp.SetField(NamespaceUri, "Parameters", paramStruct);
        }

        return editOp;
    }

    public static XmpStruct CreateColorGrading(
        double temperature,
        double tint,
        double vibrance,
        double saturation,
        string lutName = null)
    {
        var grading = new XmpStruct();
        grading.SetField(NamespaceUri, "Temperature", new XmpReal(temperature));
        grading.SetField(NamespaceUri, "Tint", new XmpReal(tint));
        grading.SetField(NamespaceUri, "Vibrance", new XmpReal(vibrance));
        grading.SetField(NamespaceUri, "Saturation", new XmpReal(saturation));

        if (!string.IsNullOrEmpty(lutName))
        {
            grading.SetField(NamespaceUri, "LUTName", new XmpText(lutName));
        }

        return grading;
    }
}

// Schema registry for custom schemas
public class XmpSchemaRegistry
{
    private readonly Dictionary<string, XmpSchema> _schemas = new();
    private readonly Dictionary<string, string> _prefixMap = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public void RegisterSchema(XmpSchema schema)
    {
        _lock.EnterWriteLock();
        try
        {
            _schemas[schema.NamespaceUri] = schema;
            _prefixMap[schema.PreferredPrefix] = schema.NamespaceUri;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public XmpSchema GetSchema(string namespaceUri)
    {
        _lock.EnterReadLock();
        try
        {
            return _schemas.TryGetValue(namespaceUri, out var schema) ? schema : null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public string GetNamespaceUri(string prefix)
    {
        _lock.EnterReadLock();
        try
        {
            return _prefixMap.TryGetValue(prefix, out var uri) ? uri : null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    // Generate XSD for documentation
    public XmlSchema GenerateXsd(string namespaceUri)
    {
        var schema = GetSchema(namespaceUri);
        if (schema == null)
            return null;

        var xsd = new XmlSchema
        {
            TargetNamespace = namespaceUri,
            ElementFormDefault = XmlSchemaForm.Qualified
        };

        // Add schema documentation
        var annotation = new XmlSchemaAnnotation();
        var documentation = new XmlSchemaDocumentation();
        documentation.Markup = new XmlNode[]
        {
            CreateTextNode($"XMP Schema: {schema.PreferredPrefix}")
        };
        annotation.Items.Add(documentation);
        xsd.Items.Add(annotation);

        // Generate types for properties
        foreach (var property in schema.Properties)
        {
            var element = new XmlSchemaElement
            {
                Name = property.Name,
                SchemaTypeName = MapToXsdType(property.ValueType)
            };

            if (!string.IsNullOrEmpty(property.Description))
            {
                var propAnnotation = new XmlSchemaAnnotation();
                var propDoc = new XmlSchemaDocumentation();
                propDoc.Markup = new XmlNode[] { CreateTextNode(property.Description) };
                propAnnotation.Items.Add(propDoc);
                element.Annotation = propAnnotation;
            }

            xsd.Items.Add(element);
        }

        return xsd;
    }
}
```

### Industry-specific metadata schemas

Different industries require specialized metadata schemas tailored to their workflows. Medical imaging uses DICOM tags,
geospatial applications need GeoTIFF metadata, and digital asset management systems require extensive descriptive
metadata. The implementation must handle these diverse requirements while maintaining interoperability.

```csharp
// Medical imaging metadata schema
public class MedicalImagingSchema : CustomXmpSchema
{
    public const string NamespaceUri = "http://example.com/xmp/medical/1.0/";
    public const string PreferredPrefix = "medical";

    public MedicalImagingSchema() : base(
        NamespaceUri,
        PreferredPrefix,
        new Version(1, 0),
        DefineProperties())
    {
    }

    private static IEnumerable<XmpPropertyDefinition> DefineProperties()
    {
        // Patient information (anonymized)
        yield return new XmpPropertyDefinition(
            "PatientID",
            XmpValueType.Text,
            "Anonymized patient identifier",
            isRequired: true,
            validator: new RegexValidator(@"^[A-Z0-9]{8,16}$"));

        yield return new XmpPropertyDefinition(
            "StudyDate",
            XmpValueType.Date,
            "Date of medical study",
            isRequired: true);

        yield return new XmpPropertyDefinition(
            "Modality",
            XmpValueType.Text,
            "Imaging modality",
            validator: new ChoiceValidator("CT", "MR", "US", "XR", "NM", "PET"));

        // Technical parameters
        yield return new XmpPropertyDefinition(
            "AcquisitionParameters",
            XmpValueType.Struct,
            "Image acquisition parameters");

        yield return new XmpPropertyDefinition(
            "ReconstructionMethod",
            XmpValueType.Text,
            "Reconstruction algorithm used");

        // Clinical information
        yield return new XmpPropertyDefinition(
            "Findings",
            XmpValueType.Seq,
            "Clinical findings",
            elementType: XmpValueType.Struct);

        yield return new XmpPropertyDefinition(
            "Measurements",
            XmpValueType.Bag,
            "Quantitative measurements",
            elementType: XmpValueType.Struct);

        // Compliance and audit
        yield return new XmpPropertyDefinition(
            "ComplianceInfo",
            XmpValueType.Struct,
            "Regulatory compliance information");
    }

    public static XmpStruct CreateAcquisitionParameters(
        double? kvp = null,
        double? mas = null,
        double? sliceThickness = null,
        string sequenceName = null)
    {
        var parameters = new XmpStruct();

        if (kvp.HasValue)
            parameters.SetField(NamespaceUri, "KVP", new XmpReal(kvp.Value));

        if (mas.HasValue)
            parameters.SetField(NamespaceUri, "mAs", new XmpReal(mas.Value));

        if (sliceThickness.HasValue)
            parameters.SetField(NamespaceUri, "SliceThickness",
                new XmpReal(sliceThickness.Value));

        if (!string.IsNullOrEmpty(sequenceName))
            parameters.SetField(NamespaceUri, "SequenceName",
                new XmpText(sequenceName));

        return parameters;
    }

    // DICOM tag mapping
    public static void MapFromDicom(XmpDocument xmp, DicomDataset dicom)
    {
        // Map common DICOM tags to XMP
        if (dicom.TryGetString(DicomTag.PatientID, out var patientId))
        {
            xmp.SetProperty(NamespaceUri, "PatientID",
                AnonymizePatientId(patientId));
        }

        if (dicom.TryGetDateTime(DicomTag.StudyDate, DicomTag.StudyTime, out var studyDateTime))
        {
            xmp.SetProperty(NamespaceUri, "StudyDate",
                new XmpDate(studyDateTime));
        }

        if (dicom.TryGetString(DicomTag.Modality, out var modality))
        {
            xmp.SetProperty(NamespaceUri, "Modality", modality);
        }

        // Map technical parameters
        var acquisitionParams = new XmpStruct();

        if (dicom.TryGetDouble(DicomTag.KVP, out var kvp))
        {
            acquisitionParams.SetField(NamespaceUri, "KVP", new XmpReal(kvp));
        }

        if (dicom.TryGetDouble(DicomTag.Exposure, out var exposure))
        {
            acquisitionParams.SetField(NamespaceUri, "mAs", new XmpReal(exposure));
        }

        xmp.SetProperty(NamespaceUri, "AcquisitionParameters", acquisitionParams);
    }
}

// Geospatial metadata schema
public class GeospatialSchema : CustomXmpSchema
{
    public const string NamespaceUri = "http://example.com/xmp/geo/1.0/";
    public const string PreferredPrefix = "geo";

    public GeospatialSchema() : base(
        NamespaceUri,
        PreferredPrefix,
        new Version(1, 0),
        DefineProperties())
    {
    }

    private static IEnumerable<XmpPropertyDefinition> DefineProperties()
    {
        yield return new XmpPropertyDefinition(
            "CoordinateSystem",
            XmpValueType.Text,
            "Spatial reference system",
            isRequired: true);

        yield return new XmpPropertyDefinition(
            "BoundingBox",
            XmpValueType.Struct,
            "Geographic bounding box",
            isRequired: true);

        yield return new XmpPropertyDefinition(
            "Resolution",
            XmpValueType.Struct,
            "Spatial resolution");

        yield return new XmpPropertyDefinition(
            "AcquisitionDate",
            XmpValueType.Date,
            "Date of data acquisition");

        yield return new XmpPropertyDefinition(
            "ProcessingLevel",
            XmpValueType.Text,
            "Data processing level");

        yield return new XmpPropertyDefinition(
            "QualityMetrics",
            XmpValueType.Struct,
            "Data quality indicators");
    }

    public static XmpStruct CreateBoundingBox(
        double west, double south, double east, double north)
    {
        var bbox = new XmpStruct();
        bbox.SetField(NamespaceUri, "West", new XmpReal(west));
        bbox.SetField(NamespaceUri, "South", new XmpReal(south));
        bbox.SetField(NamespaceUri, "East", new XmpReal(east));
        bbox.SetField(NamespaceUri, "North", new XmpReal(north));
        return bbox;
    }

    // GeoTIFF tag mapping
    public static void MapFromGeoTiff(XmpDocument xmp, GeoTiffDirectory geoTiff)
    {
        // Map coordinate system
        if (geoTiff.TryGetProjectedCSTypeGeoKey(out var pcsCode))
        {
            xmp.SetProperty(NamespaceUri, "CoordinateSystem",
                $"EPSG:{pcsCode}");
        }
        else if (geoTiff.TryGetGeographicTypeGeoKey(out var gcsCode))
        {
            xmp.SetProperty(NamespaceUri, "CoordinateSystem",
                $"EPSG:{gcsCode}");
        }

        // Map bounding box from tie points and pixel scale
        if (geoTiff.HasModelTiePoints && geoTiff.HasModelPixelScale)
        {
            var tiePoints = geoTiff.GetModelTiePoints();
            var pixelScale = geoTiff.GetModelPixelScale();

            // Calculate bounds
            var west = tiePoints[3];
            var north = tiePoints[4];
            var east = west + (geoTiff.ImageWidth * pixelScale[0]);
            var south = north - (geoTiff.ImageHeight * pixelScale[1]);

            var bbox = CreateBoundingBox(west, south, east, north);
            xmp.SetProperty(NamespaceUri, "BoundingBox", bbox);
        }

        // Map resolution
        if (geoTiff.HasModelPixelScale)
        {
            var pixelScale = geoTiff.GetModelPixelScale();
            var resolution = new XmpStruct();
            resolution.SetField(NamespaceUri, "X", new XmpReal(pixelScale[0]));
            resolution.SetField(NamespaceUri, "Y", new XmpReal(pixelScale[1]));
            resolution.SetField(NamespaceUri, "Unit", new XmpText("meters"));
            xmp.SetProperty(NamespaceUri, "Resolution", resolution);
        }
    }
}
```

## 14.3 Metadata Preservation Strategies

### Non-destructive metadata handling

Preserving metadata during image processing requires sophisticated strategies that maintain original information while
tracking modifications. The architecture must support copy-on-write semantics, handle unknown metadata formats
gracefully, and maintain byte-for-byte fidelity for unmodified sections.

```csharp
// Comprehensive metadata preservation system
public class MetadataPreservationManager
{
    private readonly IMetadataReaderRegistry _readers;
    private readonly IMetadataWriterRegistry _writers;
    private readonly PreservationOptions _defaultOptions;

    public MetadataPreservationManager()
    {
        _readers = new MetadataReaderRegistry();
        _writers = new MetadataWriterRegistry();

        _defaultOptions = new PreservationOptions
        {
            PreserveUnknownTags = true,
            PreserveByteOrder = true,
            PreserveMakerNotes = true,
            TrackModifications = true,
            CreateBackup = true
        };
    }

    public async Task<PreservedMetadata> ExtractMetadataAsync(
        Stream source,
        PreservationOptions options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= _defaultOptions;
        var preserved = new PreservedMetadata();

        // Identify all metadata segments
        var segments = await IdentifyMetadataSegmentsAsync(source, cancellationToken);

        foreach (var segment in segments)
        {
            try
            {
                // Try to parse with known readers
                if (_readers.TryGetReader(segment.Type, out var reader))
                {
                    var parsed = await reader.ReadAsync(
                        source,
                        segment.Offset,
                        segment.Length,
                        cancellationToken);

                    preserved.AddParsedSegment(segment, parsed);
                }
                else if (options.PreserveUnknownTags)
                {
                    // Preserve as raw bytes
                    var rawData = await ReadSegmentBytesAsync(
                        source,
                        segment.Offset,
                        segment.Length,
                        cancellationToken);

                    preserved.AddRawSegment(segment, rawData);
                }
            }
            catch (Exception ex)
            {
                // Log parsing error but continue
                preserved.AddError(segment, ex);

                if (options.PreserveUnknownTags)
                {
                    // Fall back to raw preservation
                    var rawData = await ReadSegmentBytesAsync(
                        source,
                        segment.Offset,
                        segment.Length,
                        cancellationToken);

                    preserved.AddRawSegment(segment, rawData);
                }
            }
        }

        return preserved;
    }

    public async Task<byte[]> ReconstructWithModificationsAsync(
        Stream originalSource,
        PreservedMetadata preserved,
        MetadataModifications modifications,
        CancellationToken cancellationToken = default)
    {
        using var output = new MemoryStream();

        // Copy image data segments
        await CopyImageDataAsync(originalSource, output, preserved, cancellationToken);

        // Reconstruct metadata with modifications
        foreach (var segment in preserved.Segments)
        {
            if (modifications.HasModifications(segment.Type))
            {
                // Apply modifications
                var modified = await ApplyModificationsAsync(
                    segment,
                    modifications,
                    cancellationToken);

                await WriteMetadataSegmentAsync(output, modified, cancellationToken);
            }
            else if (segment.HasParsedData)
            {
                // Rewrite parsed data (may update offsets)
                var writer = _writers.GetWriter(segment.Type);
                await writer.WriteAsync(
                    output,
                    segment.ParsedData,
                    cancellationToken);
            }
            else
            {
                // Write raw bytes unchanged
                await output.WriteAsync(segment.RawData, cancellationToken);
            }
        }

        return output.ToArray();
    }

    // Modification tracking
    public class MetadataModifications
    {
        private readonly Dictionary<string, List<Modification>> _modifications = new();
        private readonly List<ModificationRecord> _history = new();

        public void AddModification(
            MetadataType type,
            string path,
            object oldValue,
            object newValue,
            string reason = null)
        {
            var modification = new Modification
            {
                Path = path,
                OldValue = oldValue,
                NewValue = newValue,
                Timestamp = DateTime.UtcNow,
                Reason = reason
            };

            if (!_modifications.TryGetValue(type.ToString(), out var list))
            {
                list = new List<Modification>();
                _modifications[type.ToString()] = list;
            }

            list.Add(modification);

            // Track in history
            _history.Add(new ModificationRecord
            {
                Type = type,
                Modification = modification
            });
        }

        public bool HasModifications(MetadataType type)
        {
            return _modifications.ContainsKey(type.ToString());
        }

        public IEnumerable<Modification> GetModifications(MetadataType type)
        {
            return _modifications.TryGetValue(type.ToString(), out var list)
                ? list
                : Enumerable.Empty<Modification>();
        }

        // Generate modification report
        public XmpStruct GenerateXmpHistory()
        {
            var history = new XmpStruct();
            var modifications = new XmpArray(XmpArrayType.Seq);

            foreach (var record in _history)
            {
                var mod = new XmpStruct();
                mod.SetField(
                    "http://example.com/xmp/history/1.0/",
                    "Type",
                    new XmpText(record.Type.ToString()));

                mod.SetField(
                    "http://example.com/xmp/history/1.0/",
                    "Path",
                    new XmpText(record.Modification.Path));

                mod.SetField(
                    "http://example.com/xmp/history/1.0/",
                    "Timestamp",
                    new XmpDate(record.Modification.Timestamp));

                if (!string.IsNullOrEmpty(record.Modification.Reason))
                {
                    mod.SetField(
                        "http://example.com/xmp/history/1.0/",
                        "Reason",
                        new XmpText(record.Modification.Reason));
                }

                modifications.AddItem(mod);
            }

            history.SetField(
                "http://example.com/xmp/history/1.0/",
                "Modifications",
                modifications);

            return history;
        }
    }
}

// Metadata segment identification
public class MetadataSegmentIdentifier
{
    private readonly Dictionary<byte[], MetadataType> _signatures;

    public MetadataSegmentIdentifier()
    {
        _signatures = new Dictionary<byte[], MetadataType>(new ByteArrayComparer())
        {
            // JPEG APP1 EXIF
            [new byte[] { 0xFF, 0xE1 }] = MetadataType.Exif,
            // JPEG APP13 IPTC
            [new byte[] { 0xFF, 0xED }] = MetadataType.Iptc,
            // PNG tEXt
            [Encoding.ASCII.GetBytes("tEXt")] = MetadataType.Text,
            // PNG iTXt
            [Encoding.ASCII.GetBytes("iTXt")] = MetadataType.InternationalText,
            // PNG eXIf
            [Encoding.ASCII.GetBytes("eXIf")] = MetadataType.Exif
        };
    }

    public async Task<List<MetadataSegment>> IdentifySegmentsAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var segments = new List<MetadataSegment>();
        var format = await IdentifyFormatAsync(stream, cancellationToken);

        switch (format)
        {
            case ImageFormat.Jpeg:
                segments.AddRange(await ScanJpegSegmentsAsync(stream, cancellationToken));
                break;

            case ImageFormat.Png:
                segments.AddRange(await ScanPngChunksAsync(stream, cancellationToken));
                break;

            case ImageFormat.Tiff:
                segments.AddRange(await ScanTiffIfdAsync(stream, cancellationToken));
                break;

            case ImageFormat.WebP:
                segments.AddRange(await ScanWebPChunksAsync(stream, cancellationToken));
                break;
        }

        return segments;
    }

    private async Task<List<MetadataSegment>> ScanJpegSegmentsAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var segments = new List<MetadataSegment>();
        stream.Seek(0, SeekOrigin.Begin);

        // Skip SOI marker
        var marker = new byte[2];
        await stream.ReadAsync(marker, cancellationToken);

        if (marker[0] != 0xFF || marker[1] != 0xD8)
        {
            throw new InvalidDataException("Not a valid JPEG file");
        }

        while (stream.Position < stream.Length - 2)
        {
            await stream.ReadAsync(marker, cancellationToken);

            if (marker[0] != 0xFF)
            {
                continue;
            }

            var markerType = marker[1];

            // Skip padding
            while (markerType == 0xFF && stream.Position < stream.Length)
            {
                markerType = (byte)stream.ReadByte();
            }

            // Check for metadata markers
            if (IsMetadataMarker(markerType))
            {
                var lengthBytes = new byte[2];
                await stream.ReadAsync(lengthBytes, cancellationToken);
                var length = (lengthBytes[0] << 8) | lengthBytes[1];

                var segment = new MetadataSegment
                {
                    Type = GetMetadataType(markerType),
                    Offset = stream.Position - 4,
                    Length = length + 2,
                    Marker = markerType
                };

                // Check for specific metadata signatures
                if (markerType == 0xE1) // APP1
                {
                    var signature = new byte[6];
                    await stream.ReadAsync(signature, cancellationToken);

                    if (Encoding.ASCII.GetString(signature) == "Exif\0\0")
                    {
                        segment.Type = MetadataType.Exif;
                    }
                    else if (Encoding.ASCII.GetString(signature, 0, 5) == "http:")
                    {
                        segment.Type = MetadataType.Xmp;
                    }

                    stream.Seek(-6, SeekOrigin.Current);
                }

                segments.Add(segment);

                // Skip segment data
                stream.Seek(length - 2, SeekOrigin.Current);
            }
            else if (markerType == 0xDA) // Start of Scan
            {
                // Image data follows
                break;
            }
            else if (markerType != 0xD0 && markerType != 0xD1 &&
                     markerType != 0xD2 && markerType != 0xD3 &&
                     markerType != 0xD4 && markerType != 0xD5 &&
                     markerType != 0xD6 && markerType != 0xD7 &&
                     markerType != 0xD8 && markerType != 0xD9)
            {
                // Markers with length
                var lengthBytes = new byte[2];
                await stream.ReadAsync(lengthBytes, cancellationToken);
                var length = (lengthBytes[0] << 8) | lengthBytes[1];
                stream.Seek(length - 2, SeekOrigin.Current);
            }
        }

        return segments;
    }
}

// Round-trip preservation validator
public class MetadataPreservationValidator
{
    public async Task<ValidationReport> ValidatePreservationAsync(
        Stream original,
        Stream processed,
        CancellationToken cancellationToken = default)
    {
        var report = new ValidationReport();

        // Extract metadata from both streams
        var originalMetadata = await ExtractAllMetadataAsync(original, cancellationToken);
        var processedMetadata = await ExtractAllMetadataAsync(processed, cancellationToken);

        // Compare each metadata type
        foreach (var type in Enum.GetValues<MetadataType>())
        {
            var originalData = originalMetadata.GetMetadata(type);
            var processedData = processedMetadata.GetMetadata(type);

            if (originalData == null && processedData == null)
                continue;

            if (originalData == null || processedData == null)
            {
                report.AddIssue(type, "Metadata missing in processed file");
                continue;
            }

            // Deep comparison
            var differences = CompareMetadata(originalData, processedData);
            foreach (var diff in differences)
            {
                report.AddDifference(type, diff);
            }
        }

        // Check for byte-level preservation of unknown data
        var unknownOriginal = originalMetadata.GetUnknownSegments();
        var unknownProcessed = processedMetadata.GetUnknownSegments();

        if (unknownOriginal.Count != unknownProcessed.Count)
        {
            report.AddIssue(MetadataType.Unknown,
                $"Unknown segment count mismatch: {unknownOriginal.Count} vs {unknownProcessed.Count}");
        }
        else
        {
            for (int i = 0; i < unknownOriginal.Count; i++)
            {
                if (!unknownOriginal[i].SequenceEqual(unknownProcessed[i]))
                {
                    report.AddIssue(MetadataType.Unknown,
                        $"Unknown segment {i} content mismatch");
                }
            }
        }

        return report;
    }
}
```

### Metadata versioning and history tracking

Professional workflows require tracking metadata changes over time, enabling audit trails and rollback capabilities. The
implementation must efficiently store metadata versions, track modifications with attribution, and provide tools for
comparing and merging metadata from different sources.

```csharp
// Metadata versioning system
public class MetadataVersioningSystem
{
    private readonly IMetadataStore _store;
    private readonly IVersioningStrategy _strategy;
    private readonly IClock _clock;

    public MetadataVersioningSystem(
        IMetadataStore store,
        IVersioningStrategy strategy,
        IClock clock)
    {
        _store = store;
        _strategy = strategy;
        _clock = clock;
    }

    public async Task<MetadataVersion> CreateVersionAsync(
        string assetId,
        MetadataDocument metadata,
        VersionInfo info,
        CancellationToken cancellationToken = default)
    {
        // Generate version identifier
        var versionId = _strategy.GenerateVersionId(assetId, metadata, info);

        // Create version record
        var version = new MetadataVersion
        {
            Id = versionId,
            AssetId = assetId,
            Timestamp = _clock.UtcNow,
            Author = info.Author,
            Comment = info.Comment,
            ParentVersionId = info.ParentVersionId,
            Type = info.Type
        };

        // Store metadata content
        var contentHash = await _store.StoreContentAsync(
            versionId,
            metadata,
            cancellationToken);

        version.ContentHash = contentHash;

        // Calculate deltas if incremental storage
        if (_strategy.SupportsDeltas && info.ParentVersionId != null)
        {
            var parentContent = await _store.GetContentAsync(
                info.ParentVersionId,
                cancellationToken);

            var delta = CalculateDelta(parentContent, metadata);

            if (delta.Size < metadata.Size * 0.5) // Only use delta if smaller
            {
                version.DeltaContent = delta;
                version.IsDelta = true;
            }
        }

        // Store version record
        await _store.StoreVersionAsync(version, cancellationToken);

        // Update version graph
        await UpdateVersionGraphAsync(assetId, version, cancellationToken);

        return version;
    }

    public async Task<MetadataDocument> GetVersionAsync(
        string versionId,
        CancellationToken cancellationToken = default)
    {
        var version = await _store.GetVersionAsync(versionId, cancellationToken);

        if (version.IsDelta)
        {
            // Reconstruct from deltas
            return await ReconstructFromDeltasAsync(version, cancellationToken);
        }
        else
        {
            // Direct retrieval
            return await _store.GetContentAsync(versionId, cancellationToken);
        }
    }

    private async Task<MetadataDocument> ReconstructFromDeltasAsync(
        MetadataVersion version,
        CancellationToken cancellationToken)
    {
        // Find base version
        var baseVersion = version;
        var deltas = new Stack<MetadataDelta>();

        while (baseVersion.IsDelta)
        {
            deltas.Push(baseVersion.DeltaContent);
            baseVersion = await _store.GetVersionAsync(
                baseVersion.ParentVersionId,
                cancellationToken);
        }

        // Load base content
        var document = await _store.GetContentAsync(
            baseVersion.Id,
            cancellationToken);

        // Apply deltas in order
        while (deltas.Count > 0)
        {
            var delta = deltas.Pop();
            document = ApplyDelta(document, delta);
        }

        return document;
    }

    // Version comparison
    public async Task<MetadataComparison> CompareVersionsAsync(
        string versionId1,
        string versionId2,
        ComparisonOptions options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= ComparisonOptions.Default;

        var metadata1 = await GetVersionAsync(versionId1, cancellationToken);
        var metadata2 = await GetVersionAsync(versionId2, cancellationToken);

        var comparison = new MetadataComparison
        {
            Version1 = versionId1,
            Version2 = versionId2,
            Timestamp = _clock.UtcNow
        };

        // Compare each metadata namespace
        var namespaces = metadata1.Namespaces
            .Union(metadata2.Namespaces)
            .Select(n => n.Uri)
            .Distinct();

        foreach (var namespaceUri in namespaces)
        {
            var ns1 = metadata1.GetNamespace(namespaceUri);
            var ns2 = metadata2.GetNamespace(namespaceUri);

            if (ns1 == null && ns2 != null)
            {
                comparison.AddedNamespaces.Add(namespaceUri);
            }
            else if (ns1 != null && ns2 == null)
            {
                comparison.RemovedNamespaces.Add(namespaceUri);
            }
            else if (ns1 != null && ns2 != null)
            {
                var namespaceDiff = CompareNamespaces(ns1, ns2, options);
                if (namespaceDiff.HasChanges)
                {
                    comparison.ModifiedNamespaces[namespaceUri] = namespaceDiff;
                }
            }
        }

        return comparison;
    }

    // Version merging with conflict resolution
    public async Task<MetadataDocument> MergeVersionsAsync(
        string baseVersionId,
        string version1Id,
        string version2Id,
        MergeStrategy strategy,
        CancellationToken cancellationToken = default)
    {
        var baseMetadata = await GetVersionAsync(baseVersionId, cancellationToken);
        var metadata1 = await GetVersionAsync(version1Id, cancellationToken);
        var metadata2 = await GetVersionAsync(version2Id, cancellationToken);

        var merged = new MetadataDocument();
        var conflicts = new List<MergeConflict>();

        // Three-way merge for each namespace
        var allNamespaces = new HashSet<string>();
        allNamespaces.UnionWith(baseMetadata.Namespaces.Select(n => n.Uri));
        allNamespaces.UnionWith(metadata1.Namespaces.Select(n => n.Uri));
        allNamespaces.UnionWith(metadata2.Namespaces.Select(n => n.Uri));

        foreach (var namespaceUri in allNamespaces)
        {
            var baseNs = baseMetadata.GetNamespace(namespaceUri);
            var ns1 = metadata1.GetNamespace(namespaceUri);
            var ns2 = metadata2.GetNamespace(namespaceUri);

            var mergedNs = MergeNamespaces(baseNs, ns1, ns2, strategy, conflicts);
            if (mergedNs != null)
            {
                merged.SetNamespace(namespaceUri, mergedNs);
            }
        }

        // Handle conflicts based on strategy
        if (conflicts.Any())
        {
            switch (strategy.ConflictResolution)
            {
                case ConflictResolution.Fail:
                    throw new MergeConflictException(conflicts);

                case ConflictResolution.PreferVersion1:
                    foreach (var conflict in conflicts)
                    {
                        ApplyValue(merged, conflict.Path, conflict.Value1);
                    }
                    break;

                case ConflictResolution.PreferVersion2:
                    foreach (var conflict in conflicts)
                    {
                        ApplyValue(merged, conflict.Path, conflict.Value2);
                    }
                    break;

                case ConflictResolution.Interactive:
                    foreach (var conflict in conflicts)
                    {
                        var resolution = await strategy.ConflictResolver(conflict);
                        ApplyValue(merged, conflict.Path, resolution);
                    }
                    break;
            }
        }

        return merged;
    }
}

// Efficient delta calculation
public class MetadataDeltaCalculator
{
    public MetadataDelta CalculateDelta(
        MetadataDocument oldDoc,
        MetadataDocument newDoc)
    {
        var delta = new MetadataDelta();

        // Compare namespaces
        var oldNamespaces = oldDoc.Namespaces.ToDictionary(n => n.Uri);
        var newNamespaces = newDoc.Namespaces.ToDictionary(n => n.Uri);

        // Find removed namespaces
        foreach (var uri in oldNamespaces.Keys.Except(newNamespaces.Keys))
        {
            delta.RemovedNamespaces.Add(uri);
        }

        // Find added namespaces
        foreach (var uri in newNamespaces.Keys.Except(oldNamespaces.Keys))
        {
            delta.AddedNamespaces.Add(uri, newNamespaces[uri]);
        }

        // Find modified namespaces
        foreach (var uri in oldNamespaces.Keys.Intersect(newNamespaces.Keys))
        {
            var oldNs = oldNamespaces[uri];
            var newNs = newNamespaces[uri];

            var nsDelta = CalculateNamespaceDelta(oldNs, newNs);
            if (nsDelta.HasChanges)
            {
                delta.ModifiedNamespaces.Add(uri, nsDelta);
            }
        }

        return delta;
    }

    private NamespaceDelta CalculateNamespaceDelta(
        MetadataNamespace oldNs,
        MetadataNamespace newNs)
    {
        var delta = new NamespaceDelta();

        var oldProps = oldNs.Properties.ToDictionary(p => p.Name);
        var newProps = newNs.Properties.ToDictionary(p => p.Name);

        // Removed properties
        foreach (var name in oldProps.Keys.Except(newProps.Keys))
        {
            delta.RemovedProperties.Add(name);
        }

        // Added properties
        foreach (var name in newProps.Keys.Except(oldProps.Keys))
        {
            delta.AddedProperties.Add(name, newProps[name]);
        }

        // Modified properties
        foreach (var name in oldProps.Keys.Intersect(newProps.Keys))
        {
            if (!AreValuesEqual(oldProps[name], newProps[name]))
            {
                delta.ModifiedProperties.Add(name, new PropertyChange
                {
                    OldValue = oldProps[name],
                    NewValue = newProps[name]
                });
            }
        }

        return delta;
    }
}
```

## 14.4 Performance Considerations

### Memory-efficient metadata processing

Metadata can constitute a significant portion of file size, particularly for XMP with embedded previews or extensive
IPTC keywords. The implementation must use streaming APIs where possible, implement lazy loading for large metadata
blocks, and pool buffers for repeated operations.

```csharp
// High-performance metadata processor
public class HighPerformanceMetadataProcessor
{
    private readonly ArrayPool<byte> _bytePool;
    private readonly ObjectPool<MemoryStream> _streamPool;
    private readonly ObjectPool<StringBuilder> _stringBuilderPool;
    private readonly ParallelOptions _parallelOptions;

    public HighPerformanceMetadataProcessor()
    {
        _bytePool = ArrayPool<byte>.Create(maxArrayLength: 1024 * 1024); // 1MB max

        _streamPool = new DefaultObjectPool<MemoryStream>(
            new MemoryStreamPooledObjectPolicy());

        _stringBuilderPool = new DefaultObjectPool<StringBuilder>(
            new StringBuilderPooledObjectPolicy());

        _parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
    }

    public async Task ProcessBatchAsync(
        IEnumerable<string> filePaths,
        MetadataProcessingOptions options,
        CancellationToken cancellationToken = default)
    {
        using var semaphore = new SemaphoreSlim(options.MaxConcurrency);

        var tasks = filePaths.Select(async filePath =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await ProcessFileAsync(filePath, options, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task ProcessFileAsync(
        string filePath,
        MetadataProcessingOptions options,
        CancellationToken cancellationToken)
    {
        // Use memory-mapped files for large files
        var fileInfo = new FileInfo(filePath);

        if (fileInfo.Length > options.MemoryMappedThreshold)
        {
            await ProcessLargeFileAsync(filePath, options, cancellationToken);
        }
        else
        {
            await ProcessSmallFileAsync(filePath, options, cancellationToken);
        }
    }

    private async Task ProcessLargeFileAsync(
        string filePath,
        MetadataProcessingOptions options,
        CancellationToken cancellationToken)
    {
        using var mmf = MemoryMappedFile.CreateFromFile(
            filePath,
            FileMode.Open,
            null,
            0,
            MemoryMappedFileAccess.Read);

        using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

        // Process metadata segments without loading entire file
        var segments = await IdentifyMetadataSegmentsAsync(accessor, cancellationToken);

        await Parallel.ForEachAsync(
            segments,
            _parallelOptions,
            async (segment, ct) =>
            {
                await ProcessSegmentAsync(accessor, segment, options, ct);
            });
    }

    // Optimized EXIF reading with minimal allocations
    public async Task<ExifData> ReadExifOptimizedAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        const int BufferSize = 4096;
        var buffer = _bytePool.Rent(BufferSize);

        try
        {
            // Read header
            await stream.ReadAsync(buffer.AsMemory(0, 12), cancellationToken);

            if (!ValidateExifHeader(buffer))
                return null;

            var endianness = buffer[6] == 'I' ? Endianness.Little : Endianness.Big;
            var reader = new BinaryPrimitives(endianness);

            // Read IFD offset
            var ifdOffset = reader.ReadUInt32(buffer, 8);

            // Process IFDs with streaming
            var exifData = new ExifData();
            await ProcessIfdStreamAsync(
                stream,
                ifdOffset,
                exifData,
                buffer,
                reader,
                cancellationToken);

            return exifData;
        }
        finally
        {
            _bytePool.Return(buffer);
        }
    }

    // Zero-allocation XMP parsing
    public async Task<XmpDocument> ParseXmpZeroAllocAsync(
        ReadOnlyMemory<byte> xmpPacket,
        CancellationToken cancellationToken = default)
    {
        var document = new XmpDocument();

        // Use Utf8JsonReader for zero-allocation parsing
        var reader = new Utf8JsonReader(xmpPacket.Span, new JsonReaderOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        // Custom XMP parser using spans
        await ParseXmpWithSpansAsync(reader, document, cancellationToken);

        return document;
    }

    // Batch metadata extraction with pipelining
    public async IAsyncEnumerable<MetadataResult> ExtractMetadataPipelineAsync(
        IAsyncEnumerable<string> filePaths,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<MetadataExtractionTask>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

        // Producer task
        var producerTask = Task.Run(async () =>
        {
            await foreach (var filePath in filePaths.WithCancellation(cancellationToken))
            {
                await channel.Writer.WriteAsync(
                    new MetadataExtractionTask { FilePath = filePath },
                    cancellationToken);
            }
            channel.Writer.Complete();
        });

        // Consumer tasks
        var consumerTasks = Enumerable.Range(0, Environment.ProcessorCount)
            .Select(_ => ProcessChannelAsync(channel.Reader, cancellationToken))
            .ToArray();

        // Yield results as they complete
        await foreach (var result in MergeAsyncEnumerables(consumerTasks, cancellationToken))
        {
            yield return result;
        }

        await producerTask;
    }

    // SIMD-accelerated metadata search
    public unsafe int FindMetadataMarker(
        ReadOnlySpan<byte> buffer,
        byte marker1,
        byte marker2)
    {
        if (!Vector.IsHardwareAccelerated || buffer.Length < Vector<byte>.Count * 2)
        {
            // Fallback to scalar search
            return FindMetadataMarkerScalar(buffer, marker1, marker2);
        }

        fixed (byte* ptr = buffer)
        {
            var marker1Vector = new Vector<byte>(marker1);
            var marker2Vector = new Vector<byte>(marker2);

            int i = 0;
            int lastIndex = buffer.Length - Vector<byte>.Count;

            while (i < lastIndex)
            {
                var v1 = Unsafe.Read<Vector<byte>>(ptr + i);
                var v2 = Unsafe.Read<Vector<byte>>(ptr + i + 1);

                var matches = Vector.BitwiseAnd(
                    Vector.Equals(v1, marker1Vector),
                    Vector.Equals(v2, marker2Vector));

                if (!Vector.EqualsAll(matches, Vector<byte>.Zero))
                {
                    // Found potential match, verify
                    for (int j = 0; j < Vector<byte>.Count; j++)
                    {
                        if (matches[j] != 0 && i + j + 1 < buffer.Length)
                        {
                            return i + j;
                        }
                    }
                }

                i += Vector<byte>.Count;
            }

            // Check remaining bytes
            return FindMetadataMarkerScalar(buffer.Slice(i), marker1, marker2) + i;
        }
    }

    // Memory pool for temporary allocations
    private class MetadataMemoryPool : MemoryPool<byte>
    {
        private readonly ArrayPool<byte> _arrayPool;
        private readonly int _maxBufferSize;

        public MetadataMemoryPool(int maxBufferSize = 1024 * 1024)
        {
            _arrayPool = ArrayPool<byte>.Create(maxBufferSize);
            _maxBufferSize = maxBufferSize;
        }

        public override int MaxBufferSize => _maxBufferSize;

        public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            if (minBufferSize == -1)
                minBufferSize = 4096;

            var array = _arrayPool.Rent(minBufferSize);
            return new ArrayMemoryOwner(array, minBufferSize, _arrayPool);
        }

        protected override void Dispose(bool disposing)
        {
            // ArrayPool is shared, no disposal needed
        }

        private class ArrayMemoryOwner : IMemoryOwner<byte>
        {
            private byte[] _array;
            private readonly int _length;
            private readonly ArrayPool<byte> _pool;

            public ArrayMemoryOwner(byte[] array, int length, ArrayPool<byte> pool)
            {
                _array = array;
                _length = length;
                _pool = pool;
            }

            public Memory<byte> Memory => _array.AsMemory(0, _length);

            public void Dispose()
            {
                var array = Interlocked.Exchange(ref _array, null);
                if (array != null)
                {
                    _pool.Return(array);
                }
            }
        }
    }
}

// Benchmark-driven optimization strategies
[MemoryDiagnoser]
[DisassemblyDiagnoser]
public class MetadataPerformanceBenchmarks
{
    private byte[] _jpegWithMetadata;
    private byte[] _tiffWithMetadata;
    private byte[] _xmpPacket;

    [GlobalSetup]
    public void Setup()
    {
        // Load test files
        _jpegWithMetadata = File.ReadAllBytes("test_with_metadata.jpg");
        _tiffWithMetadata = File.ReadAllBytes("test_with_metadata.tif");
        _xmpPacket = Encoding.UTF8.GetBytes(File.ReadAllText("test.xmp"));
    }

    [Benchmark]
    public async Task<ExifData> ReadExifTraditional()
    {
        using var stream = new MemoryStream(_jpegWithMetadata);
        var reader = new TraditionalExifReader();
        return await reader.ReadAsync(stream);
    }

    [Benchmark]
    public async Task<ExifData> ReadExifOptimized()
    {
        using var stream = new MemoryStream(_jpegWithMetadata);
        var processor = new HighPerformanceMetadataProcessor();
        return await processor.ReadExifOptimizedAsync(stream);
    }

    [Benchmark]
    public XmpDocument ParseXmpDom()
    {
        var doc = XDocument.Parse(Encoding.UTF8.GetString(_xmpPacket));
        return XmpDocument.FromXDocument(doc);
    }

    [Benchmark]
    public async Task<XmpDocument> ParseXmpStreaming()
    {
        var processor = new HighPerformanceMetadataProcessor();
        return await processor.ParseXmpZeroAllocAsync(_xmpPacket);
    }

    [Benchmark]
    public int FindMarkerScalar()
    {
        return FindJpegMarkerScalar(_jpegWithMetadata, 0xFF, 0xE1);
    }

    [Benchmark]
    public int FindMarkerSimd()
    {
        var processor = new HighPerformanceMetadataProcessor();
        return processor.FindMetadataMarker(_jpegWithMetadata, 0xFF, 0xE1);
    }
}
```

### Caching strategies for metadata operations

Metadata operations often exhibit temporal locality—the same metadata is accessed repeatedly during processing
workflows. Implementing intelligent caching reduces parsing overhead and improves response times for metadata-intensive
operations.

```csharp
// Multi-level metadata cache system
public class MetadataCacheSystem
{
    private readonly IMemoryCache _l1Cache; // Hot data
    private readonly IDistributedCache _l2Cache; // Warm data
    private readonly TimeSpan _l1Expiration;
    private readonly TimeSpan _l2Expiration;
    private readonly int _maxL1Size;

    public MetadataCacheSystem(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        CacheConfiguration config)
    {
        _l1Cache = memoryCache;
        _l2Cache = distributedCache;
        _l1Expiration = config.L1Expiration;
        _l2Expiration = config.L2Expiration;
        _maxL1Size = config.MaxL1Size;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        CacheEntryOptions options = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Check L1 cache
        if (_l1Cache.TryGetValue<T>(key, out var cachedValue))
        {
            RecordCacheHit(CacheLevel.L1, key);
            return cachedValue;
        }

        // Check L2 cache
        var l2Data = await _l2Cache.GetAsync(key, cancellationToken);
        if (l2Data != null)
        {
            RecordCacheHit(CacheLevel.L2, key);

            var deserializedValue = DeserializeValue<T>(l2Data);

            // Promote to L1
            await PromoteToL1Async(key, deserializedValue, options);

            return deserializedValue;
        }

        // Cache miss - create value
        RecordCacheMiss(key);

        var value = await factory();

        // Store in both caches
        await StoreInCachesAsync(key, value, options, cancellationToken);

        return value;
    }

    private async Task StoreInCachesAsync<T>(
        string key,
        T value,
        CacheEntryOptions options,
        CancellationToken cancellationToken) where T : class
    {
        // L1 cache with size-based eviction
        var l1Options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = options?.SlidingExpiration ?? _l1Expiration,
            Size = EstimateSize(value),
            PostEvictionCallbacks =
            {
                new PostEvictionCallbackRegistration
                {
                    EvictionCallback = OnL1Eviction,
                    State = key
                }
            }
        };

        _l1Cache.Set(key, value, l1Options);

        // L2 cache with longer expiration
        var serializedValue = SerializeValue(value);
        var l2Options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = options?.L2Expiration ?? _l2Expiration
        };

        await _l2Cache.SetAsync(key, serializedValue, l2Options, cancellationToken);
    }

    // Intelligent cache warming
    public async Task WarmCacheAsync(
        IEnumerable<string> assetIds,
        CancellationToken cancellationToken = default)
    {
        var warmingTasks = new List<Task>();
        using var semaphore = new SemaphoreSlim(10); // Limit concurrency

        foreach (var assetId in assetIds)
        {
            warmingTasks.Add(WarmSingleAssetAsync(assetId, semaphore, cancellationToken));
        }

        await Task.WhenAll(warmingTasks);
    }

    private async Task WarmSingleAssetAsync(
        string assetId,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var key = $"metadata:{assetId}";

            // Check if already cached
            if (_l1Cache.TryGetValue(key, out _))
                return;

            // Load metadata
            var metadata = await LoadMetadataAsync(assetId, cancellationToken);

            // Store with extended expiration for warmed entries
            var options = new CacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(2),
                Priority = CacheItemPriority.High
            };

            await StoreInCachesAsync(key, metadata, options, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    // Cache statistics and monitoring
    private readonly CacheStatistics _statistics = new();

    private void RecordCacheHit(CacheLevel level, string key)
    {
        _statistics.RecordHit(level, key);
    }

    private void RecordCacheMiss(string key)
    {
        _statistics.RecordMiss(key);
    }

    public CacheReport GenerateReport()
    {
        return new CacheReport
        {
            L1HitRate = _statistics.GetHitRate(CacheLevel.L1),
            L2HitRate = _statistics.GetHitRate(CacheLevel.L2),
            TotalHitRate = _statistics.GetOverallHitRate(),
            HottestKeys = _statistics.GetHottestKeys(10),
            EvictionRate = _statistics.GetEvictionRate(),
            AverageLatency = _statistics.GetAverageLatency()
        };
    }
}

// Specialized metadata index for fast queries
public class MetadataIndex
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _keywordIndex;
    private readonly ConcurrentDictionary<DateTime, HashSet<string>> _dateIndex;
    private readonly ConcurrentDictionary<string, HashSet<string>> _cameraIndex;
    private readonly SortedSet<GeoLocation> _geoIndex;
    private readonly ReaderWriterLockSlim _geoLock;

    public MetadataIndex()
    {
        _keywordIndex = new ConcurrentDictionary<string, HashSet<string>>(
            StringComparer.OrdinalIgnoreCase);
        _dateIndex = new ConcurrentDictionary<DateTime, HashSet<string>>();
        _cameraIndex = new ConcurrentDictionary<string, HashSet<string>>(
            StringComparer.OrdinalIgnoreCase);
        _geoIndex = new SortedSet<GeoLocation>(new GeoLocationComparer());
        _geoLock = new ReaderWriterLockSlim();
    }

    public async Task IndexMetadataAsync(
        string assetId,
        MetadataDocument metadata,
        CancellationToken cancellationToken = default)
    {
        var indexingTasks = new List<Task>();

        // Index keywords
        var keywords = ExtractKeywords(metadata);
        if (keywords.Any())
        {
            indexingTasks.Add(Task.Run(() => IndexKeywords(assetId, keywords), cancellationToken));
        }

        // Index dates
        var dates = ExtractDates(metadata);
        if (dates.Any())
        {
            indexingTasks.Add(Task.Run(() => IndexDates(assetId, dates), cancellationToken));
        }

        // Index camera information
        var cameraInfo = ExtractCameraInfo(metadata);
        if (cameraInfo != null)
        {
            indexingTasks.Add(Task.Run(() => IndexCamera(assetId, cameraInfo), cancellationToken));
        }

        // Index geolocation
        var location = ExtractLocation(metadata);
        if (location != null)
        {
            indexingTasks.Add(Task.Run(() => IndexLocation(assetId, location), cancellationToken));
        }

        await Task.WhenAll(indexingTasks);
    }

    // Fast keyword search with ranking
    public async Task<SearchResults> SearchByKeywordsAsync(
        IEnumerable<string> keywords,
        SearchOptions options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= SearchOptions.Default;

        var keywordList = keywords.ToList();
        var assetScores = new ConcurrentDictionary<string, double>();

        await Parallel.ForEachAsync(
            keywordList,
            cancellationToken,
            async (keyword, ct) =>
            {
                if (_keywordIndex.TryGetValue(keyword, out var assets))
                {
                    foreach (var assetId in assets)
                    {
                        assetScores.AddOrUpdate(
                            assetId,
                            1.0,
                            (_, score) => score + 1.0);
                    }
                }

                // Handle stemming and fuzzy matching
                if (options.EnableFuzzyMatch)
                {
                    var fuzzyMatches = await FindFuzzyMatchesAsync(keyword, ct);
                    foreach (var (matchedKeyword, similarity) in fuzzyMatches)
                    {
                        if (_keywordIndex.TryGetValue(matchedKeyword, out var fuzzyAssets))
                        {
                            foreach (var assetId in fuzzyAssets)
                            {
                                assetScores.AddOrUpdate(
                                    assetId,
                                    similarity,
                                    (_, score) => score + similarity);
                            }
                        }
                    }
                }
            });

        // Rank results
        var rankedResults = assetScores
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Take(options.MaxResults)
            .Select(kv => new SearchResult
            {
                AssetId = kv.Key,
                Score = kv.Value,
                MatchedKeywords = GetMatchedKeywords(kv.Key, keywordList)
            })
            .ToList();

        return new SearchResults
        {
            Results = rankedResults,
            TotalCount = assetScores.Count,
            SearchTime = TimeSpan.Zero // TODO: Implement timing
        };
    }

    // Spatial queries with R-tree optimization
    public async Task<List<string>> SearchByLocationAsync(
        double latitude,
        double longitude,
        double radiusKm,
        CancellationToken cancellationToken = default)
    {
        var results = new List<string>();
        var searchLocation = new GeoLocation(latitude, longitude, null);

        _geoLock.EnterReadLock();
        try
        {
            // Use spatial index for efficient range query
            var candidates = _geoIndex.GetViewBetween(
                new GeoLocation(latitude - radiusKm / 111.0, longitude - radiusKm / 111.0, null),
                new GeoLocation(latitude + radiusKm / 111.0, longitude + radiusKm / 111.0, null));

            foreach (var location in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var distance = CalculateDistance(searchLocation, location);
                if (distance <= radiusKm)
                {
                    results.Add(location.AssetId);
                }
            }
        }
        finally
        {
            _geoLock.ExitReadLock();
        }

        return results;
    }

    private double CalculateDistance(GeoLocation loc1, GeoLocation loc2)
    {
        // Haversine formula
        const double R = 6371; // Earth's radius in km

        var dLat = ToRadians(loc2.Latitude - loc1.Latitude);
        var dLon = ToRadians(loc2.Longitude - loc1.Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(loc1.Latitude)) * Math.Cos(ToRadians(loc2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
```

## Conclusion

Metadata handling systems represent a critical component of modern image processing architectures, bridging the gap
between raw pixel data and the rich contextual information that drives contemporary digital asset workflows. The
implementation strategies presented in this chapter—from low-level EXIF parsing to high-level schema design—demonstrate
that professional metadata handling requires careful attention to standards compliance, performance optimization, and
extensibility.

The evolution from simple EXIF tags to complex XMP schemas mirrors the broader transformation of digital imaging from
technical capture to creative workflow. Modern metadata systems must handle this full spectrum, supporting everything
from camera-generated technical data to AI-derived semantic tags, while maintaining the performance characteristics
necessary for real-time processing.

Looking forward, metadata systems will continue to evolve with emerging standards for computational photography, machine
learning annotations, and blockchain-based provenance tracking. The architectural patterns established here—lazy
loading, streaming processing, extensible schemas, and comprehensive preservation strategies—provide the foundation for
adapting to these future requirements while maintaining compatibility with decades of existing metadata standards.
