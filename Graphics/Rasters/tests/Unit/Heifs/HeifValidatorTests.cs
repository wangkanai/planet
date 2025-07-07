// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Heifs;

public class HeifValidatorTests
{
	[Fact]
	public void ValidateRaster_WithNullRaster_ReturnsFailure()
	{
		// Act
		var result = HeifValidator.ValidateRaster(null!);

		// Assert
		Assert.False(result.IsValid);
		Assert.Single(result.Errors);
		Assert.Contains("Raster cannot be null", result.Errors[0]);
	}

	[Fact]
	public void ValidateRaster_WithValidRaster_ReturnsSuccess()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080);

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Theory]
	[InlineData(0, 1080, "Width must be greater than 0")]
	[InlineData(-1, 1080, "Width must be greater than 0")]
	[InlineData(1920, 0, "Height must be greater than 0")]
	[InlineData(1920, -1, "Height must be greater than 0")]
	public void ValidateRaster_WithInvalidDimensions_ReturnsFailure(int width, int height, string expectedError)
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080) { Width = width, Height = height };

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError + ".", result.Errors);
	}

	[Fact]
	public void ValidateRaster_WithDimensionsTooLarge_ReturnsFailure()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080)
		{
			Width = HeifConstants.MaxDimension + 1,
			Height = HeifConstants.MaxDimension + 1
		};

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains($"Width cannot exceed {HeifConstants.MaxDimension} pixels", result.Errors);
		Assert.Contains($"Height cannot exceed {HeifConstants.MaxDimension} pixels", result.Errors);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(101)]
	public void ValidateRaster_WithInvalidQuality_ReturnsFailure(int quality)
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080) { Quality = quality };

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains($"Quality must be between {HeifConstants.MinQuality} and {HeifConstants.MaxQuality}.", result.Errors);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(10)]
	public void ValidateRaster_WithInvalidSpeed_ReturnsFailure(int speed)
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080) { Speed = speed };

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains($"Speed must be between {HeifConstants.MinSpeed} and {HeifConstants.MaxSpeed}.", result.Errors);
	}

	[Theory]
	[InlineData(7)]
	[InlineData(17)]
	public void ValidateRaster_WithInvalidBitDepth_ReturnsFailure(int bitDepth)
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080) { BitDepth = bitDepth };

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains($"Bit depth must be between {HeifConstants.MinBitDepth} and {HeifConstants.MaxBitDepth}", result.Errors);
	}

	[Theory]
	[InlineData(9)]
	[InlineData(11)]
	[InlineData(13)]
	[InlineData(14)]
	[InlineData(15)]
	public void ValidateRaster_WithUnsupportedBitDepth_ReturnsFailure(int bitDepth)
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080) { BitDepth = bitDepth };

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("Bit depth must be 8, 10, 12, or 16.", result.Errors);
	}

	[Fact]
	public void ValidateRaster_WithNegativeThreadCount_ReturnsFailure()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080) { ThreadCount = -1 };

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("Thread count cannot be negative", result.Errors);
	}

	[Fact]
	public void ValidateRaster_WithExcessiveThreadCount_ReturnsFailure()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080) { ThreadCount = HeifConstants.Memory.MaxThreads + 1 };

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains($"Thread count cannot exceed {HeifConstants.Memory.MaxThreads}", result.Errors);
	}

	[Fact]
	public void ValidateRaster_WithLosslessAndWrongQuality_ReturnsFailure()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080)
		{
			IsLossless = true,
			Quality = 90
		};

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("Lossless mode requires quality to be 100", result.Errors);
	}

	[Fact]
	public void ValidateRaster_WithJpegAndLossless_ReturnsFailure()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080)
		{
			Compression = HeifCompression.Jpeg,
			IsLossless = true,
			Quality = 100
		};

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("JPEG compression cannot be used with lossless mode", result.Errors);
	}

	[Fact]
	public void ValidateRaster_WithMain10AndLowBitDepth_ReturnsFailure()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080)
		{
			Profile = HeifProfile.Main10,
			BitDepth = 8
		};

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("Main 10 profile requires bit depth of at least 10", result.Errors);
	}

	[Fact]
	public void ValidateRaster_WithMonochromeAndAlpha_ReturnsFailure()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080, true)
		{
			ChromaSubsampling = HeifChromaSubsampling.Yuv400
		};

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("Monochrome (YUV 4:0:0) subsampling cannot be used with alpha channel", result.Errors);
	}

	[Fact]
	public void ValidateRaster_WithHdrAndLowBitDepth_ReturnsFailure()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080) { BitDepth = 8 };
		raster.SetHdrMetadata(new HdrMetadata());

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("HDR metadata requires bit depth of at least 10", result.Errors);
	}

	[Fact]
	public void ValidateRaster_WithLargeImageAndSingleThread_ReturnsFailure()
	{
		// Arrange
		var raster = new HeifRaster(10000, 10000) { ThreadCount = 1 };

		// Act
		var result = HeifValidator.ValidateRaster(raster);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("Large images should use multiple threads for better performance", result.Errors);
	}

	[Fact]
	public void ValidateEncodingOptions_WithNullOptions_ReturnsFailure()
	{
		// Act
		var result = HeifValidator.ValidateEncodingOptions(null!);

		// Assert
		Assert.False(result.IsValid);
		Assert.Single(result.Errors);
		Assert.Contains("Encoding options cannot be null", result.Errors[0]);
	}

	[Fact]
	public void ValidateEncodingOptions_WithValidOptions_ReturnsSuccess()
	{
		// Arrange
		var options = HeifEncodingOptions.CreateDefault();

		// Act
		var result = HeifValidator.ValidateEncodingOptions(options);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void ValidateEncodingOptions_WithInvalidOptions_ReturnsFailure()
	{
		// Arrange
		var options = new HeifEncodingOptions { Quality = -1 };

		// Act
		var result = HeifValidator.ValidateEncodingOptions(options);

		// Assert
		Assert.False(result.IsValid);
		Assert.NotEmpty(result.Errors);
	}

	[Fact]
	public void ValidateFileData_WithNullData_ReturnsFailure()
	{
		// Act
		var result = HeifValidator.ValidateFileData(null!);

		// Assert
		Assert.False(result.IsValid);
		Assert.Single(result.Errors);
		Assert.Contains("File data cannot be null", result.Errors[0]);
	}

	[Fact]
	public void ValidateFileData_WithTooSmallData_ReturnsFailure()
	{
		// Arrange
		var data = new byte[4];

		// Act
		var result = HeifValidator.ValidateFileData(data);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("File data is too small to be a valid HEIF file", result.Errors);
	}

	[Fact]
	public void ValidateFileData_WithInvalidHeader_ReturnsFailure()
	{
		// Arrange
		var data = new byte[12];
		data[4] = 0x69; data[5] = 0x6E; data[6] = 0x76; data[7] = 0x61; // "inva" instead of "ftyp"

		// Act
		var result = HeifValidator.ValidateFileData(data);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("File does not start with a valid HEIF file type box", result.Errors);
	}

	[Fact]
	public void ValidateFileData_WithInvalidBrand_ReturnsFailure()
	{
		// Arrange
		var data = new byte[12];
		data[4] = 0x66; data[5] = 0x74; data[6] = 0x79; data[7] = 0x70; // "ftyp"
		data[8] = 0x69; data[9] = 0x6E; data[10] = 0x76; data[11] = 0x61; // "inva" instead of HEIF brand

		// Act
		var result = HeifValidator.ValidateFileData(data);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("File does not contain a recognized HEIF brand", result.Errors);
	}

	[Theory]
	[InlineData("heic")]
	[InlineData("heis")]
	[InlineData("hevc")]
	[InlineData("avif")]
	public void ValidateFileData_WithValidBrand_ReturnsSuccess(string brand)
	{
		// Arrange
		var data = new byte[12];
		data[4] = 0x66; data[5] = 0x74; data[6] = 0x79; data[7] = 0x70; // "ftyp"
		var brandBytes = System.Text.Encoding.ASCII.GetBytes(brand);
		Array.Copy(brandBytes, 0, data, 8, 4);

		// Act
		var result = HeifValidator.ValidateFileData(data);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void ValidateMetadata_WithNullMetadata_ReturnsFailure()
	{
		// Act
		var result = HeifValidator.ValidateMetadata(null!);

		// Assert
		Assert.False(result.IsValid);
		Assert.Single(result.Errors);
		Assert.Contains("Metadata cannot be null", result.Errors[0]);
	}

	[Fact]
	public void ValidateMetadata_WithValidMetadata_ReturnsSuccess()
	{
		// Arrange
		var metadata = new HeifMetadata();

		// Act
		var result = HeifValidator.ValidateMetadata(metadata);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void ValidateMetadata_WithInvalidExifData_ReturnsFailure()
	{
		// Arrange
		var metadata = new HeifMetadata { ExifData = new byte[3] }; // Too small

		// Act
		var result = HeifValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("EXIF data is too small to be valid", result.Errors);
	}

	[Fact]
	public void ValidateMetadata_WithInvalidXmpData_ReturnsFailure()
	{
		// Arrange
		var metadata = new HeifMetadata { XmpData = "<x>" }; // Too small

		// Act
		var result = HeifValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("XMP data is too small to be valid", result.Errors);
	}

	[Fact]
	public void ValidateMetadata_WithInvalidIccProfile_ReturnsFailure()
	{
		// Arrange
		var metadata = new HeifMetadata { IccProfile = new byte[100] }; // Too small

		// Act
		var result = HeifValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("ICC profile data is too small to be valid", result.Errors);
	}

	[Theory]
	[InlineData(-91.0, 0.0, "GPS latitude must be between -90 and 90 degrees")]
	[InlineData(91.0, 0.0, "GPS latitude must be between -90 and 90 degrees")]
	[InlineData(0.0, -181.0, "GPS longitude must be between -180 and 180 degrees")]
	[InlineData(0.0, 181.0, "GPS longitude must be between -180 and 180 degrees")]
	public void ValidateMetadata_WithInvalidGpsCoordinates_ReturnsFailure(double latitude, double longitude, string expectedError)
	{
		// Arrange
		var metadata = new HeifMetadata
		{
			GpsCoordinates = new GpsCoordinates { Latitude = latitude, Longitude = longitude }
		};

		// Act
		var result = HeifValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError + ".", result.Errors);
	}

	[Theory]
	[InlineData(-1.0, "Aperture value must be positive")]
	[InlineData(-0.001, "Exposure time must be positive")]
	[InlineData(-10.0, "Focal length must be positive")]
	[InlineData(-100, "ISO sensitivity must be positive")]
	[InlineData(-72.0, "Pixel density must be positive")]
	public void ValidateMetadata_WithNegativeCameraSettings_ReturnsFailure(object value, string expectedError)
	{
		// Arrange
		var metadata = new HeifMetadata
		{
			CameraMetadata = new CameraMetadata()
		};

		switch (expectedError)
		{
			case string s when s.Contains("Aperture"):
				metadata.CameraMetadata.Aperture = (double)value;
				break;
			case string s when s.Contains("Exposure"):
				metadata.CameraMetadata.ExposureTime = (double)value;
				break;
			case string s when s.Contains("Focal"):
				metadata.CameraMetadata.FocalLength = (double)value;
				break;
			case string s when s.Contains("ISO"):
				metadata.CameraMetadata.IsoSensitivity = (int)value;
				break;
			case string s when s.Contains("Pixel"):
				metadata.CameraMetadata.XResolution = (double)value;
				metadata.CameraMetadata.YResolution = (double)value;
				break;
		}

		// Act
		var result = HeifValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError + ".", result.Errors);
	}

	[Fact]
	public void ValidateComplete_WithValidConfiguration_ReturnsSuccess()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080);
		var options = HeifEncodingOptions.CreateDefault();

		// Act
		var result = HeifValidator.ValidateComplete(raster, options);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void ValidateComplete_WithInvalidRasterAndOptions_ReturnsAllErrors()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080) { Quality = -1 }; // Invalid quality
		var options = new HeifEncodingOptions { Speed = -1 }; // Invalid speed

		// Act
		var result = HeifValidator.ValidateComplete(raster, options);

		// Assert
		Assert.False(result.IsValid);
		Assert.True(result.Errors.Count >= 2); // Should have errors from both raster and options
	}

	[Fact]
	public void ValidateComplete_WithNullOptions_ValidatesOnlyRaster()
	{
		// Arrange
		var raster = new HeifRaster(1920, 1080);

		// Act
		var result = HeifValidator.ValidateComplete(raster, null);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}
}
