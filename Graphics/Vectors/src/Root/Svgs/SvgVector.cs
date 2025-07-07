// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO.Compression;

namespace Wangkanai.Graphics.Vectors.Svgs;

/// <summary>
/// Represents an SVG vector graphics implementation with comprehensive
/// SVG 1.1/2.0 support, geospatial integration, and performance optimization.
/// </summary>
/// <remarks>
/// Provides full SVG document handling including parsing, serialization,
/// coordinate system transformations, and optimization for map tile rendering.
/// Supports both standard SVG and compressed SVGZ formats.
/// </remarks>
public class SvgVector : Vector, ISvgVector
{
	private bool _disposed;
	private XDocument? _document;
	private readonly SvgMetadata _metadata;
	private byte[]? _rawData;
	private string? _sourceFilePath;

	/// <summary>Initializes a new SVG vector with default settings.</summary>
	public SvgVector() : this(100, 100) { }

	/// <summary>Initializes a new SVG vector with specified dimensions.</summary>
	/// <param name="width">The viewport width.</param>
	/// <param name="height">The viewport height.</param>
	public SvgVector(int width, int height)
	{
		Width = width;
		Height = height;
		_metadata = new SvgMetadata
		{
			ViewportWidth = width,
			ViewportHeight = height,
			ViewBox = new SvgViewBox(0, 0, width, height)
		};
		_document = CreateEmptyDocument();
	}

	/// <summary>Initializes a new SVG vector from existing SVG content.</summary>
	/// <param name="svgContent">The SVG XML content.</param>
	public SvgVector(string svgContent)
	{
		_metadata = new SvgMetadata();
		LoadFromString(svgContent);
	}

	/// <summary>Initializes a new SVG vector from file path.</summary>
	/// <param name="filePath">Path to the SVG file.</param>
	public SvgVector(string filePath, bool isFilePath)
	{
		if (!isFilePath) throw new ArgumentException("Use other constructor for SVG content", nameof(isFilePath));

		_metadata = new SvgMetadata();
		_sourceFilePath = filePath;
		LoadFromFile(filePath);
	}

	/// <inheritdoc />
	public override int Width
	{
		get => (int)_metadata.ViewportWidth;
		set
		{
			_metadata.ViewportWidth = value;
			UpdateDocumentDimensions();
		}
	}

	/// <inheritdoc />
	public override int Height
	{
		get => (int)_metadata.ViewportHeight;
		set
		{
			_metadata.ViewportHeight = value;
			UpdateDocumentDimensions();
		}
	}

	/// <inheritdoc />
	public override bool HasLargeMetadata => _metadata.IsVeryLargeSvg;

	/// <inheritdoc />
	public override long EstimatedMetadataSize => _metadata.CalculateEstimatedMemoryUsage();

	/// <summary>Gets the SVG metadata.</summary>
	public ISvgMetadata Metadata => _metadata;

	/// <summary>Gets the SVG document as XDocument.</summary>
	public XDocument? Document => _document;

	/// <summary>Gets whether the SVG is loaded from a compressed format.</summary>
	public bool IsCompressed => _metadata.IsCompressed;

	/// <summary>Gets the source file path if loaded from file.</summary>
	public string? SourceFilePath => _sourceFilePath;

	/// <summary>Gets the SVG content as XML string.</summary>
	public string ToXmlString()
	{
		ThrowIfDisposed();
		return _document?.ToString() ?? string.Empty;
	}

	/// <summary>Gets the SVG content as formatted XML string.</summary>
	public string ToFormattedXmlString()
	{
		ThrowIfDisposed();
		if (_document == null) return string.Empty;

		var settings = new XmlWriterSettings
		{
			Indent = true,
			IndentChars = "  ",
			Encoding = Encoding.UTF8,
			OmitXmlDeclaration = false
		};

		using var stringWriter = new StringWriter();
		using var xmlWriter = XmlWriter.Create(stringWriter, settings);
		_document.WriteTo(xmlWriter);
		return stringWriter.ToString();
	}

	/// <summary>Saves the SVG to a file.</summary>
	/// <param name="filePath">The file path to save to.</param>
	/// <param name="compressed">Whether to save as compressed SVGZ format.</param>
	public async Task SaveToFileAsync(string filePath, bool compressed = false)
	{
		ThrowIfDisposed();
		if (_document == null) throw new InvalidOperationException("No SVG document loaded");

		_metadata.ModificationDate = DateTime.UtcNow;
		_metadata.IsCompressed = compressed;

		var xmlContent = ToFormattedXmlString();

		if (compressed)
		{
			await SaveCompressedAsync(filePath, xmlContent);
		}
		else
		{
			await File.WriteAllTextAsync(filePath, xmlContent, Encoding.UTF8);
		}

		_sourceFilePath = filePath;
	}

	/// <summary>Loads SVG content from a file.</summary>
	/// <param name="filePath">The file path to load from.</param>
	public async Task LoadFromFileAsync(string filePath)
	{
		ThrowIfDisposed();
		_sourceFilePath = filePath;

		var extension = Path.GetExtension(filePath).ToLowerInvariant();
		var isCompressed = extension == SvgConstants.CompressedFileExtension;

		string content;
		if (isCompressed)
		{
			content = await LoadCompressedAsync(filePath);
			_metadata.IsCompressed = true;
		}
		else
		{
			content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
			_metadata.IsCompressed = false;
		}

		LoadFromString(content);
	}

	/// <summary>Optimizes the SVG for performance by removing unnecessary elements and attributes.</summary>
	public void Optimize()
	{
		ThrowIfDisposed();
		if (_document?.Root == null) return;

		// Remove comments
		_document.DescendantNodes().OfType<XComment>().Remove();

		// Remove empty groups
		var emptyGroups = _document.Root.Descendants()
			.Where(e => e.Name.LocalName == "g" && !e.HasElements && string.IsNullOrWhiteSpace(e.Value))
			.ToList();
		emptyGroups.ForEach(g => g.Remove());

		// Round coordinate values to reduce precision
		RoundCoordinates(_document.Root, SvgConstants.CoordinatePrecision);

		// Update metadata
		UpdateMetadataFromDocument();
		_metadata.ModificationDate = DateTime.UtcNow;
	}

	/// <summary>Validates the SVG document for compliance with specified version.</summary>
	public bool ValidateDocument()
	{
		ThrowIfDisposed();
		return _metadata.ValidateCompliance() && _document?.Root != null;
	}

	/// <summary>Creates an empty SVG document with default structure.</summary>
	private XDocument CreateEmptyDocument()
	{
		var svg = new XElement(XName.Get("svg", SvgConstants.SvgNamespace),
			new XAttribute("version", _metadata.Version),
			new XAttribute("viewBox", _metadata.ViewBox.ToString()),
			new XAttribute("width", _metadata.ViewportWidth),
			new XAttribute("height", _metadata.ViewportHeight),
			new XAttribute(XNamespace.Xmlns + "svg", SvgConstants.SvgNamespace),
			new XAttribute(XNamespace.Xmlns + "xlink", SvgConstants.XLinkNamespace)
		);

		return new XDocument(
			new XDeclaration("1.0", "UTF-8", "no"),
			svg
		);
	}

	/// <summary>Loads SVG content from string and parses metadata.</summary>
	private void LoadFromString(string svgContent)
	{
		try
		{
			_document = XDocument.Parse(svgContent);
			UpdateMetadataFromDocument();
		}
		catch (XmlException ex)
		{
			throw new InvalidOperationException($"Invalid SVG content: {ex.Message}", ex);
		}
	}

	/// <summary>Loads SVG content from file synchronously.</summary>
	private void LoadFromFile(string filePath)
	{
		var extension = Path.GetExtension(filePath).ToLowerInvariant();
		var isCompressed = extension == SvgConstants.CompressedFileExtension;

		string content;
		if (isCompressed)
		{
			content = LoadCompressed(filePath);
			_metadata.IsCompressed = true;
		}
		else
		{
			content = File.ReadAllText(filePath, Encoding.UTF8);
			_metadata.IsCompressed = false;
		}

		LoadFromString(content);
	}

	/// <summary>Updates document dimensions based on metadata.</summary>
	private void UpdateDocumentDimensions()
	{
		if (_document?.Root == null) return;

		_document.Root.SetAttributeValue("width", _metadata.ViewportWidth);
		_document.Root.SetAttributeValue("height", _metadata.ViewportHeight);
		_document.Root.SetAttributeValue("viewBox", _metadata.ViewBox.ToString());
	}

	/// <summary>Updates metadata from the loaded document.</summary>
	private void UpdateMetadataFromDocument()
	{
		if (_document?.Root == null) return;

		var root = _document.Root;

		// Extract basic attributes
		_metadata.Version = root.Attribute("version")?.Value ?? SvgConstants.DefaultVersion;

		// Extract viewBox
		var viewBoxAttr = root.Attribute("viewBox")?.Value;
		if (!string.IsNullOrEmpty(viewBoxAttr))
		{
			try { _metadata.ViewBox = SvgViewBox.Parse(viewBoxAttr); }
			catch { _metadata.ViewBox = SvgViewBox.Default; }
		}

		// Extract dimensions
		if (double.TryParse(root.Attribute("width")?.Value?.Replace("px", ""), out var width))
			_metadata.ViewportWidth = width;
		if (double.TryParse(root.Attribute("height")?.Value?.Replace("px", ""), out var height))
			_metadata.ViewportHeight = height;

		// Extract title and description
		_metadata.Title = root.Element(XName.Get("title", SvgConstants.SvgNamespace))?.Value;
		_metadata.Description = root.Element(XName.Get("desc", SvgConstants.SvgNamespace))?.Value;

		// Count elements
		_metadata.ElementCount = root.Descendants().Count();

		// Calculate total path length
		_metadata.TotalPathLength = CalculateTotalPathLength(root);

		// Extract namespaces
		_metadata.Namespaces.Clear();
		foreach (var attr in root.Attributes().Where(a => a.IsNamespaceDeclaration))
		{
			_metadata.Namespaces[attr.Name.LocalName] = attr.Value;
		}

		_metadata.ModificationDate = DateTime.UtcNow;
	}

	/// <summary>Calculates the total path length for performance estimation.</summary>
	private static double CalculateTotalPathLength(XElement root)
	{
		var totalLength = 0.0;
		var pathElements = root.Descendants().Where(e => e.Name.LocalName == "path");

		foreach (var path in pathElements)
		{
			var d = path.Attribute("d")?.Value;
			if (!string.IsNullOrEmpty(d))
			{
				// Rough estimation based on path data length
				totalLength += d.Length;
			}
		}

		return totalLength;
	}

	/// <summary>Rounds coordinates to specified precision.</summary>
	private static void RoundCoordinates(XElement element, int precision)
	{
		var coordinateAttributes = new[] { "x", "y", "x1", "y1", "x2", "y2", "cx", "cy", "r", "rx", "ry" };

		foreach (var attr in element.Attributes())
		{
			if (coordinateAttributes.Contains(attr.Name.LocalName) && double.TryParse(attr.Value, out var value))
			{
				attr.Value = Math.Round(value, precision).ToString();
			}
		}

		foreach (var child in element.Elements())
		{
			RoundCoordinates(child, precision);
		}
	}

	/// <summary>Saves compressed SVG content asynchronously.</summary>
	private async Task SaveCompressedAsync(string filePath, string content)
	{
		await using var fileStream = File.Create(filePath);
		await using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
		await using var writer = new StreamWriter(gzipStream, Encoding.UTF8);
		await writer.WriteAsync(content);
	}

	/// <summary>Loads compressed SVG content asynchronously.</summary>
	private async Task<string> LoadCompressedAsync(string filePath)
	{
		await using var fileStream = File.OpenRead(filePath);
		await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
		using var reader = new StreamReader(gzipStream, Encoding.UTF8);
		return await reader.ReadToEndAsync();
	}

	/// <summary>Loads compressed SVG content synchronously.</summary>
	private string LoadCompressed(string filePath)
	{
		using var fileStream = File.OpenRead(filePath);
		using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
		using var reader = new StreamReader(gzipStream, Encoding.UTF8);
		return reader.ReadToEnd();
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_metadata?.Dispose();
				_document = null;
				_rawData = null;
			}
			_disposed = true;
		}
		base.Dispose(disposing);
	}

	/// <inheritdoc />
	protected override async ValueTask DisposeAsyncCore()
	{
		if (_metadata != null)
			await _metadata.DisposeAsync().ConfigureAwait(false);

		_document = null;
		_rawData = null;
		_disposed = true;

		await base.DisposeAsyncCore().ConfigureAwait(false);
	}

	/// <summary>Throws ObjectDisposedException if the vector has been disposed.</summary>
	new protected void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(SvgVector));
	}
}
