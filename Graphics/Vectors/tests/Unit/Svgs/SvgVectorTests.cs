// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Vectors.Svgs;

namespace Wangkanai.Graphics.Vectors.Tests.Svgs;

public class SvgVectorTests
{
	[Fact]
	public void Constructor_Default_ShouldInitializeWithDefaultDimensions()
	{
		// Arrange & Act
		using var svg = new SvgVector();

		// Assert
		Assert.Equal(100, svg.Width);
		Assert.Equal(100, svg.Height);
		Assert.NotNull(svg.Document);
		Assert.NotNull(svg.Metadata);
		Assert.False(svg.IsCompressed);
	}

	[Fact]
	public void Constructor_WithDimensions_ShouldInitializeCorrectly()
	{
		// Arrange & Act
		using var svg = new SvgVector(200, 300);

		// Assert
		Assert.Equal(200, svg.Width);
		Assert.Equal(300, svg.Height);
		var metadata = (SvgMetadata)svg.Metadata;
		Assert.Equal(200, metadata.ViewportWidth);
		Assert.Equal(300, metadata.ViewportHeight);
		Assert.Equal(new SvgViewBox(0, 0, 200, 300), metadata.ViewBox);
	}

	[Fact]
	public void Constructor_WithSvgContent_ShouldParseCorrectly()
	{
		// Arrange
		var svgContent = """
			<svg xmlns="http://www.w3.org/2000/svg" width="150" height="250" viewBox="0 0 150 250">
				<rect x="10" y="10" width="30" height="30" fill="red"/>
			</svg>
			""";

		// Act
		using var svg = new SvgVector(svgContent);

		// Assert
		Assert.Equal(150, svg.Width);
		Assert.Equal(250, svg.Height);
		var metadata = (SvgMetadata)svg.Metadata;
		Assert.True(metadata.ElementCount > 0);
		Assert.NotNull(svg.Document);
	}

	[Theory]
	[InlineData(50, 75)]
	[InlineData(1920, 1080)]
	[InlineData(4096, 2160)]
	public void Width_SetValue_ShouldUpdateMetadataAndDocument(int width, int height)
	{
		// Arrange
		using var svg = new SvgVector();

		// Act
		svg.Width = width;
		svg.Height = height;

		// Assert
		Assert.Equal(width, svg.Width);
		Assert.Equal(height, svg.Height);
		var metadata = (SvgMetadata)svg.Metadata;
		Assert.Equal(width, metadata.ViewportWidth);
		Assert.Equal(height, metadata.ViewportHeight);
		Assert.Equal(width.ToString(), svg.Document?.Root?.Attribute("width")?.Value);
		Assert.Equal(height.ToString(), svg.Document?.Root?.Attribute("height")?.Value);
	}

	[Fact]
	public void ToXmlString_ValidDocument_ShouldReturnXmlContent()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);

		// Act
		var xmlString = svg.ToXmlString();

		// Assert
		Assert.NotEmpty(xmlString);
		Assert.Contains("<svg", xmlString);
		Assert.Contains("xmlns=\"http://www.w3.org/2000/svg\"", xmlString);
		Assert.Contains("width=\"100\"", xmlString);
		Assert.Contains("height=\"100\"", xmlString);
	}

	[Fact]
	public void ToFormattedXmlString_ValidDocument_ShouldReturnFormattedXml()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);

		// Act
		var formattedXml = svg.ToFormattedXmlString();

		// Assert
		Assert.NotEmpty(formattedXml);
		Assert.Contains("<?xml version=\"1.0\" encoding=\"utf-8\"?>", formattedXml);
		Assert.Contains("\n", formattedXml); // Should be formatted with newlines
	}

	[Fact]
	public void ValidateDocument_ValidSvg_ShouldReturnTrue()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);

		// Act
		var isValid = svg.ValidateDocument();

		// Assert
		Assert.True(isValid);
	}

	[Fact]
	public void Optimize_WithComments_ShouldRemoveComments()
	{
		// Arrange
		var svgContent = """
			<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100">
				<!-- This is a comment -->
				<rect x="10" y="10" width="30" height="30" fill="red"/>
				<!-- Another comment -->
			</svg>
			""";
		using var svg = new SvgVector(svgContent);

		// Act
		svg.Optimize();
		var optimizedXml = svg.ToXmlString();

		// Assert
		Assert.DoesNotContain("<!--", optimizedXml);
		Assert.DoesNotContain("-->", optimizedXml);
	}

	[Fact]
	public void Optimize_WithEmptyGroups_ShouldRemoveEmptyGroups()
	{
		// Arrange
		var svgContent = """
			<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100">
				<g></g>
				<rect x="10" y="10" width="30" height="30" fill="red"/>
				<g>   </g>
			</svg>
			""";
		using var svg = new SvgVector(svgContent);

		// Act
		svg.Optimize();
		var optimizedXml = svg.ToXmlString();

		// Assert
		Assert.DoesNotContain("<g></g>", optimizedXml);
		Assert.DoesNotContain("<g>   </g>", optimizedXml);
		Assert.Contains("rect", optimizedXml);
	}

	[Fact]
	public async Task SaveToFileAsync_UncompressedFormat_ShouldCreateFile()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);
		var tempFile = Path.GetTempFileName();
		
		try
		{
			// Act
			await svg.SaveToFileAsync(tempFile, compressed: false);

			// Assert
			Assert.True(File.Exists(tempFile));
			var content = await File.ReadAllTextAsync(tempFile);
			Assert.Contains("<svg", content);
			Assert.False(svg.IsCompressed);
		}
		finally
		{
			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task SaveToFileAsync_CompressedFormat_ShouldCreateCompressedFile()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);
		var tempFile = Path.GetTempFileName();
		
		try
		{
			// Act
			await svg.SaveToFileAsync(tempFile, compressed: true);

			// Assert
			Assert.True(File.Exists(tempFile));
			Assert.True(svg.IsCompressed);
			
			// File should be smaller due to compression
			var fileInfo = new FileInfo(tempFile);
			Assert.True(fileInfo.Length > 0);
		}
		finally
		{
			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task LoadFromFileAsync_ValidSvgFile_ShouldLoadCorrectly()
	{
		// Arrange
		var svgContent = """
			<?xml version="1.0" encoding="UTF-8"?>
			<svg xmlns="http://www.w3.org/2000/svg" width="200" height="150" viewBox="0 0 200 150">
				<title>Test SVG</title>
				<desc>A test SVG file</desc>
				<rect x="10" y="10" width="50" height="30" fill="blue"/>
			</svg>
			""";
		var tempFile = Path.GetTempFileName();
		await File.WriteAllTextAsync(tempFile, svgContent);

		try
		{
			using var svg = new SvgVector();

			// Act
			await svg.LoadFromFileAsync(tempFile);

			// Assert
			Assert.Equal(200, svg.Width);
			Assert.Equal(150, svg.Height);
			var metadata = (SvgMetadata)svg.Metadata;
			Assert.Equal("Test SVG", metadata.Title);
			Assert.Equal("A test SVG file", metadata.Description);
			Assert.True(metadata.ElementCount > 0);
		}
		finally
		{
			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}
	}

	[Fact]
	public void HasLargeMetadata_SmallSvg_ShouldReturnFalse()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);

		// Act & Assert
		Assert.False(svg.HasLargeMetadata);
	}

	[Fact]
	public void EstimatedMetadataSize_ValidSvg_ShouldReturnPositiveValue()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);

		// Act
		var size = svg.EstimatedMetadataSize;

		// Assert
		Assert.True(size > 0);
	}

	[Fact]
	public async Task DisposeAsync_ShouldDisposeResourcesAsync()
	{
		// Arrange
		var svg = new SvgVector(100, 100);

		// Act
		await svg.DisposeAsync();

		// Assert
		Assert.Throws<ObjectDisposedException>(() => svg.ToXmlString());
	}

	[Fact]
	public void Dispose_ShouldDisposeResources()
	{
		// Arrange
		var svg = new SvgVector(100, 100);

		// Act
		svg.Dispose();

		// Assert
		Assert.Throws<ObjectDisposedException>(() => svg.ToXmlString());
	}

	[Fact]
	public void Constructor_InvalidXml_ShouldThrowException()
	{
		// Arrange
		var invalidXml = "<svg><rect></svg>"; // Missing closing tag

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => new SvgVector(invalidXml));
	}

	[Fact]
	public async Task SaveToFileAsync_DisposedObject_ShouldThrowException()
	{
		// Arrange
		var svg = new SvgVector(100, 100);
		svg.Dispose();
		var tempFile = Path.GetTempFileName();

		// Act & Assert
		await Assert.ThrowsAsync<ObjectDisposedException>(() => 
			svg.SaveToFileAsync(tempFile));

		if (File.Exists(tempFile))
			File.Delete(tempFile);
	}

	[Fact]
	public void ToXmlString_DisposedObject_ShouldThrowException()
	{
		// Arrange
		var svg = new SvgVector(100, 100);
		svg.Dispose();

		// Act & Assert
		Assert.Throws<ObjectDisposedException>(() => svg.ToXmlString());
	}

	[Fact]
	public void Metadata_ShouldImplementISvgMetadata()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);

		// Act
		var metadata = svg.Metadata;

		// Assert
		Assert.NotNull(metadata);
		Assert.IsAssignableFrom<ISvgMetadata>(metadata);
	}
}