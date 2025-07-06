// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Pngs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Pngs;

public class PngConstantsTests
{
	[Fact]
	public void Signature_ShouldHaveCorrectValues()
	{
		// Arrange & Act
		var signature = PngConstants.Signature;

		// Assert
		Assert.Equal(8, signature.Length);
		Assert.Equal(0x89, signature[0]);
		Assert.Equal(0x50, signature[1]); // 'P'
		Assert.Equal(0x4E, signature[2]); // 'N'
		Assert.Equal(0x47, signature[3]); // 'G'
		Assert.Equal(0x0D, signature[4]); // \r
		Assert.Equal(0x0A, signature[5]); // \n
		Assert.Equal(0x1A, signature[6]); // \x1A
		Assert.Equal(0x0A, signature[7]); // \n
	}

	[Fact]
	public void MimeType_ShouldBeCorrect()
	{
		// Assert
		Assert.Equal("image/png", PngConstants.MimeType);
	}

	[Fact]
	public void FileExtension_ShouldBeCorrect()
	{
		// Assert
		Assert.Equal(".png", PngConstants.FileExtension);
	}

	[Fact]
	public void MaxDimensions_ShouldBeValid()
	{
		// Assert
		Assert.Equal((uint)int.MaxValue, PngConstants.MaxWidth);
		Assert.Equal((uint)int.MaxValue, PngConstants.MaxHeight);
		Assert.Equal(1u, PngConstants.MinWidth);
		Assert.Equal(1u, PngConstants.MinHeight);
	}

	[Fact]
	public void SignatureLength_ShouldMatchActualSignature()
	{
		// Assert
		Assert.Equal(PngConstants.Signature.Length, PngConstants.SignatureLength);
	}

	[Fact]
	public void ChunkSizes_ShouldBeCorrect()
	{
		// Assert
		Assert.Equal(4, PngConstants.ChunkLengthSize);
		Assert.Equal(4, PngConstants.ChunkTypeSize);
		Assert.Equal(4, PngConstants.ChunkCrcSize);
		Assert.Equal(12, PngConstants.MinChunkSize);
	}

	[Theory]
	[InlineData("IHDR")]
	[InlineData("PLTE")]
	[InlineData("IDAT")]
	[InlineData("IEND")]
	public void CriticalChunkTypes_ShouldHaveCorrectValues(string expectedChunkType)
	{
		// Act & Assert
		var field = typeof(PngConstants.ChunkTypes).GetField(expectedChunkType);
		Assert.NotNull(field);
		Assert.Equal(expectedChunkType, field!.GetValue(null));
	}

	[Theory]
	[InlineData("tRNS")]
	[InlineData("gAMA")]
	[InlineData("cHRM")]
	[InlineData("sRGB")]
	[InlineData("pHYs")]
	[InlineData("tEXt")]
	[InlineData("zTXt")]
	[InlineData("iTXt")]
	[InlineData("bKGD")]
	[InlineData("tIME")]
	public void AncillaryChunkTypes_ShouldHaveCorrectValues(string expectedChunkType)
	{
		// Act & Assert
		var field = typeof(PngConstants.ChunkTypes).GetField(expectedChunkType);
		Assert.NotNull(field);
		Assert.Equal(expectedChunkType, field!.GetValue(null));
	}

	[Fact]
	public void BitDepths_Grayscale_ShouldHaveCorrectValues()
	{
		// Arrange
		var expectedBitDepths = new byte[] { 1, 2, 4, 8, 16 };

		// Act & Assert
		Assert.Equal(expectedBitDepths, PngConstants.BitDepths.Grayscale);
	}

	[Fact]
	public void BitDepths_Truecolor_ShouldHaveCorrectValues()
	{
		// Arrange
		var expectedBitDepths = new byte[] { 8, 16 };

		// Act & Assert
		Assert.Equal(expectedBitDepths, PngConstants.BitDepths.Truecolor);
	}

	[Fact]
	public void BitDepths_IndexedColor_ShouldHaveCorrectValues()
	{
		// Arrange
		var expectedBitDepths = new byte[] { 1, 2, 4, 8 };

		// Act & Assert
		Assert.Equal(expectedBitDepths, PngConstants.BitDepths.IndexedColor);
	}

	[Fact]
	public void BitDepths_GrayscaleWithAlpha_ShouldHaveCorrectValues()
	{
		// Arrange
		var expectedBitDepths = new byte[] { 8, 16 };

		// Act & Assert
		Assert.Equal(expectedBitDepths, PngConstants.BitDepths.GrayscaleWithAlpha);
	}

	[Fact]
	public void BitDepths_TruecolorWithAlpha_ShouldHaveCorrectValues()
	{
		// Arrange
		var expectedBitDepths = new byte[] { 8, 16 };

		// Act & Assert
		Assert.Equal(expectedBitDepths, PngConstants.BitDepths.TruecolorWithAlpha);
	}
}