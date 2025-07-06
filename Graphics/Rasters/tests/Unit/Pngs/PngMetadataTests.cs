// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Pngs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Pngs;

public class PngMetadataTests
{
	[Fact]
	public void Constructor_ShouldInitializeCollections()
	{
		// Act
		var metadata = new PngMetadata();

		// Assert
		Assert.NotNull(metadata.TextChunks);
		Assert.NotNull(metadata.CompressedTextChunks);
		Assert.NotNull(metadata.InternationalTextChunks);
		Assert.NotNull(metadata.CustomChunks);
		Assert.Empty(metadata.TextChunks);
		Assert.Empty(metadata.CompressedTextChunks);
		Assert.Empty(metadata.InternationalTextChunks);
		Assert.Empty(metadata.CustomChunks);
	}

	[Fact]
	public void Properties_ShouldAcceptNullValues()
	{
		// Arrange
		var metadata = new PngMetadata();

		// Act & Assert
		metadata.Title       = null;
		metadata.Description = null;
		metadata.Software    = null;
		metadata.Author      = null;
		metadata.Copyright   = null;
		metadata.Comment     = null;

		Assert.Null(metadata.Title);
		Assert.Null(metadata.Description);
		Assert.Null(metadata.Software);
		Assert.Null(metadata.Author);
		Assert.Null(metadata.Copyright);
		Assert.Null(metadata.Comment);
	}

	[Fact]
	public void Properties_ShouldAcceptValidValues()
	{
		// Arrange
		var metadata = new PngMetadata();
		var now      = DateTime.UtcNow;

		// Act
		metadata.Title               = "Test Image";
		metadata.Description         = "A test PNG image";
		metadata.Software            = "Test Software";
		metadata.Author              = "Test Author";
		metadata.Copyright           = "Test Copyright";
		metadata.Comment             = "Test Comment";
		metadata.Created             = now;
		metadata.Modified            = now;
		metadata.Gamma               = 2.2;
		metadata.XResolution         = 300;
		metadata.YResolution         = 300;
		metadata.ResolutionUnit      = 1;
		metadata.BackgroundColor     = 0xFFFFFF;
		metadata.SrgbRenderingIntent = 0;

		// Assert
		Assert.Equal("Test Image", metadata.Title);
		Assert.Equal("A test PNG image", metadata.Description);
		Assert.Equal("Test Software", metadata.Software);
		Assert.Equal("Test Author", metadata.Author);
		Assert.Equal("Test Copyright", metadata.Copyright);
		Assert.Equal("Test Comment", metadata.Comment);
		Assert.Equal(now, metadata.Created);
		Assert.Equal(now, metadata.Modified);
		Assert.Equal(2.2, metadata.Gamma);
		Assert.Equal(300u, metadata.XResolution);
		Assert.Equal(300u, metadata.YResolution);
		Assert.Equal((byte)1, metadata.ResolutionUnit);
		Assert.Equal(0xFFFFFFu, metadata.BackgroundColor);
		Assert.Equal((byte)0, metadata.SrgbRenderingIntent);
	}

	[Fact]
	public void ValidateMetadata_WithValidData_ShouldReturnValid()
	{
		// Arrange
		var metadata = new PngMetadata
		               {
			               Gamma               = 2.2,
			               XResolution         = 300,
			               YResolution         = 300,
			               ResolutionUnit      = 1,
			               SrgbRenderingIntent = 0
		               };

		metadata.TextChunks["Title"]                 = "Test Image";
		metadata.CompressedTextChunks["Description"] = "Test Description";

		// Act
		var result = metadata.ValidateMetadata();

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(0)]
	[InlineData(11)]
	public void ValidateMetadata_WithInvalidGamma_ShouldAddError(double gamma)
	{
		// Arrange
		var metadata = new PngMetadata { Gamma = gamma };

		// Act
		var result = metadata.ValidateMetadata();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid gamma value"));
	}

	[Fact]
	public void ValidateMetadata_WithZeroResolution_ShouldAddError()
	{
		// Arrange
		var metadata = new PngMetadata
		               {
			               XResolution = 0,
			               YResolution = 0
		               };

		// Act
		var result = metadata.ValidateMetadata();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid X resolution"));
		Assert.Contains(result.Errors, e => e.Contains("Invalid Y resolution"));
	}

	[Theory]
	[InlineData(2)]
	[InlineData(255)]
	public void ValidateMetadata_WithInvalidResolutionUnit_ShouldAddError(byte unit)
	{
		// Arrange
		var metadata = new PngMetadata { ResolutionUnit = unit };

		// Act
		var result = metadata.ValidateMetadata();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid resolution unit"));
	}

	[Theory]
	[InlineData(4)]
	[InlineData(255)]
	public void ValidateMetadata_WithInvalidSrgbRenderingIntent_ShouldAddError(byte intent)
	{
		// Arrange
		var metadata = new PngMetadata { SrgbRenderingIntent = intent };

		// Act
		var result = metadata.ValidateMetadata();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid sRGB rendering intent"));
	}

	[Fact]
	public void ValidateMetadata_WithEmptyKeyword_ShouldAddError()
	{
		// Arrange
		var metadata = new PngMetadata();
		metadata.TextChunks[""] = "Some text";

		// Act
		var result = metadata.ValidateMetadata();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("keyword cannot be null or empty"));
	}

	[Fact]
	public void ValidateMetadata_WithTooLongKeyword_ShouldAddError()
	{
		// Arrange
		var metadata    = new PngMetadata();
		var longKeyword = new string('A', 80);// Exceeds 79 character limit
		metadata.TextChunks[longKeyword] = "Some text";

		// Act
		var result = metadata.ValidateMetadata();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("exceeds maximum length"));
	}

	[Theory]
	[InlineData(" Invalid")]        // Leading space
	[InlineData("Invalid ")]        // Trailing space
	[InlineData("Invalid  Keyword")]// Consecutive spaces
	[InlineData("Invalid\tKeyword")]// Control character
	public void ValidateMetadata_WithInvalidKeywordCharacters_ShouldAddError(string keyword)
	{
		// Arrange
		var metadata = new PngMetadata();
		metadata.TextChunks[keyword] = "Some text";

		// Act
		var result = metadata.ValidateMetadata();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("invalid characters"));
	}

	[Theory]
	[InlineData("Valid")]
	[InlineData("Valid Keyword")]
	[InlineData("Valid-Keyword")]
	[InlineData("Valid_Keyword")]
	[InlineData("Valid123")]
	public void ValidateMetadata_WithValidKeywords_ShouldNotAddError(string keyword)
	{
		// Arrange
		var metadata = new PngMetadata();
		metadata.TextChunks[keyword] = "Some text";

		// Act
		var result = metadata.ValidateMetadata();

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void TextChunks_ShouldAllowMultipleEntries()
	{
		// Arrange
		var metadata = new PngMetadata();

		// Act
		metadata.TextChunks["Title"]                 = "Test Title";
		metadata.TextChunks["Author"]                = "Test Author";
		metadata.CompressedTextChunks["Description"] = "Test Description";
		metadata.InternationalTextChunks["Comment"]  = ("en", "Comment", "Test Comment");

		// Assert
		Assert.Equal(2, metadata.TextChunks.Count);
		Assert.Single(metadata.CompressedTextChunks);
		Assert.Single(metadata.InternationalTextChunks);
		Assert.Equal("Test Title", metadata.TextChunks["Title"]);
		Assert.Equal("Test Author", metadata.TextChunks["Author"]);
		Assert.Equal("Test Description", metadata.CompressedTextChunks["Description"]);
		Assert.Equal(("en", "Comment", "Test Comment"), metadata.InternationalTextChunks["Comment"]);
	}

	[Fact]
	public void CustomChunks_ShouldAllowArbitraryData()
	{
		// Arrange
		var metadata   = new PngMetadata();
		var customData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		metadata.CustomChunks["cUst"] = customData;

		// Assert
		Assert.Single(metadata.CustomChunks);
		Assert.Equal(customData, metadata.CustomChunks["cUst"]);
	}
}
