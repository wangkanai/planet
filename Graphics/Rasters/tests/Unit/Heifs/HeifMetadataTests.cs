// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters;
using Wangkanai.Graphics.Rasters.Heifs;
using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Tests.Unit.Heifs;

public class HeifMetadataTests
{
	[Fact]
	public void DefaultConstructor_SetsCorrectDefaults()
	{
		// Act
		var metadata = new HeifMetadata();

		// Assert
		Assert.Null(metadata.ExifData);
		Assert.Null(metadata.XmpData);
		Assert.Null(metadata.IccProfile);
		Assert.Null(metadata.HdrMetadata);
		Assert.Null(metadata.CreationTime);
		Assert.Null(metadata.ModificationTime);
		Assert.Null(metadata.Software);
		Assert.Null(metadata.Description);
		Assert.Null(metadata.Copyright);
		Assert.Null(metadata.Author);
		Assert.Null(metadata.CameraMake);
		Assert.Null(metadata.CameraModel);
		Assert.Null(metadata.LensMake);
		Assert.Null(metadata.LensModel);
		Assert.Null(metadata.FocalLength);
		Assert.Null(metadata.Aperture);
		Assert.Null(metadata.ExposureTime);
		Assert.Null(metadata.IsoSensitivity);
		Assert.Null(metadata.GpsCoordinates);
		Assert.Equal(ImageOrientation.Normal, metadata.Orientation);
		Assert.Null(metadata.PixelDensity);
		Assert.Null(metadata.ColorSpaceInfo);
		Assert.Null(metadata.WhiteBalance);
		Assert.Null(metadata.CodecParameters);
		Assert.Null(metadata.CustomMetadata);
		Assert.Null(metadata.ThumbnailData);
		Assert.Null(metadata.PreviewData);
		Assert.Null(metadata.DepthMapData);
		Assert.Null(metadata.AuxiliaryImages);
	}

	[Fact]
	public void EstimatedSize_WithoutData_ReturnsBaseSize()
	{
		// Arrange
		var metadata = new HeifMetadata();

		// Act
		var size = metadata.EstimatedSize;

		// Assert
		Assert.Equal(2048, size); // Base estimate for text properties and HDR metadata
	}

	[Fact]
	public void EstimatedSize_WithData_ReturnsCorrectSize()
	{
		// Arrange
		var metadata = new HeifMetadata
		{
			ExifData = new byte[1024],
			XmpData = new byte[512],
			IccProfile = new byte[2048],
			ThumbnailData = new byte[4096],
			PreviewData = new byte[8192],
			DepthMapData = new byte[1024]
		};

		metadata.AuxiliaryImages = new Dictionary<string, byte[]>
		{
			["depth"] = new byte[2048],
			["alpha"] = new byte[1024]
		};

		// Act
		var size = metadata.EstimatedSize;

		// Assert
		var expectedSize = 1024 + 512 + 2048 + 4096 + 8192 + 1024 + 2048 + 1024 + 2048; // Data + base
		Assert.Equal(expectedSize, size);
	}

	[Fact]
	public void HasLargeData_WithSmallData_ReturnsFalse()
	{
		// Arrange
		var metadata = new HeifMetadata
		{
			ExifData = new byte[1024]
		};

		// Act & Assert
		Assert.False(metadata.HasLargeData);
	}

	[Fact]
	public void HasLargeData_WithLargeData_ReturnsTrue()
	{
		// Arrange
		var largeSize = HeifConstants.Memory.DefaultMetadataBufferSizeMB * 1024 * 1024 + 1;
		var metadata = new HeifMetadata
		{
			PreviewData = new byte[largeSize]
		};

		// Act & Assert
		Assert.True(metadata.HasLargeData);
	}

	[Fact]
	public void Clone_WithEmptyMetadata_ReturnsIndependentCopy()
	{
		// Arrange
		var original = new HeifMetadata();

		// Act
		var clone = original.Clone();

		// Assert
		Assert.NotSame(original, clone);
		Assert.Equal(original.Orientation, clone.Orientation);
		Assert.Null(clone.ExifData);
		Assert.Null(clone.HdrMetadata);
	}

	[Fact]
	public void Clone_WithCompleteMetadata_ReturnsIndependentCopy()
	{
		// Arrange
		var original = new HeifMetadata
		{
			ExifData = new byte[] { 1, 2, 3, 4 },
			XmpData = new byte[] { 5, 6, 7, 8 },
			IccProfile = new byte[] { 9, 10, 11, 12 },
			HdrMetadata = new HdrMetadata { MaxLuminance = 1000.0 },
			CreationTime = DateTimeOffset.UtcNow,
			ModificationTime = DateTimeOffset.UtcNow.AddMinutes(-10),
			Software = "Test Software",
			Description = "Test Description",
			Copyright = "Test Copyright",
			Author = "Test Author",
			CameraMake = "Test Make",
			CameraModel = "Test Model",
			LensMake = "Test Lens Make",
			LensModel = "Test Lens Model",
			FocalLength = 50.0,
			Aperture = 2.8,
			ExposureTime = 1.0 / 60.0,
			IsoSensitivity = 400,
			GpsCoordinates = new GpsCoordinates { Latitude = 37.7749, Longitude = -122.4194 },
			Orientation = ImageOrientation.Rotate90Clockwise,
			PixelDensity = 300.0,
			ColorSpaceInfo = "sRGB",
			WhiteBalance = "Auto",
			CodecParameters = new Dictionary<string, object> { ["profile"] = "Main" },
			CustomMetadata = new Dictionary<string, object> { ["custom"] = "value" },
			ThumbnailData = new byte[] { 13, 14, 15, 16 },
			PreviewData = new byte[] { 17, 18, 19, 20 },
			DepthMapData = new byte[] { 21, 22, 23, 24 },
			AuxiliaryImages = new Dictionary<string, byte[]> { ["aux"] = new byte[] { 25, 26, 27, 28 } }
		};

		// Act
		var clone = original.Clone();

		// Assert
		Assert.NotSame(original, clone);
		Assert.NotSame(original.ExifData, clone.ExifData);
		Assert.NotSame(original.XmpData, clone.XmpData);
		Assert.NotSame(original.IccProfile, clone.IccProfile);
		Assert.NotSame(original.HdrMetadata, clone.HdrMetadata);
		Assert.NotSame(original.GpsCoordinates, clone.GpsCoordinates);
		Assert.NotSame(original.CodecParameters, clone.CodecParameters);
		Assert.NotSame(original.CustomMetadata, clone.CustomMetadata);
		Assert.NotSame(original.ThumbnailData, clone.ThumbnailData);
		Assert.NotSame(original.PreviewData, clone.PreviewData);
		Assert.NotSame(original.DepthMapData, clone.DepthMapData);
		Assert.NotSame(original.AuxiliaryImages, clone.AuxiliaryImages);

		// Verify all values are equal
		Assert.Equal(original.ExifData, clone.ExifData);
		Assert.Equal(original.XmpData, clone.XmpData);
		Assert.Equal(original.IccProfile, clone.IccProfile);
		Assert.Equal(original.HdrMetadata.MaxLuminance, clone.HdrMetadata!.MaxLuminance);
		Assert.Equal(original.CreationTime, clone.CreationTime);
		Assert.Equal(original.ModificationTime, clone.ModificationTime);
		Assert.Equal(original.Software, clone.Software);
		Assert.Equal(original.Description, clone.Description);
		Assert.Equal(original.Copyright, clone.Copyright);
		Assert.Equal(original.Author, clone.Author);
		Assert.Equal(original.CameraMake, clone.CameraMake);
		Assert.Equal(original.CameraModel, clone.CameraModel);
		Assert.Equal(original.LensMake, clone.LensMake);
		Assert.Equal(original.LensModel, clone.LensModel);
		Assert.Equal(original.FocalLength, clone.FocalLength);
		Assert.Equal(original.Aperture, clone.Aperture);
		Assert.Equal(original.ExposureTime, clone.ExposureTime);
		Assert.Equal(original.IsoSensitivity, clone.IsoSensitivity);
		Assert.Equal(original.GpsCoordinates.Latitude, clone.GpsCoordinates!.Latitude);
		Assert.Equal(original.GpsCoordinates.Longitude, clone.GpsCoordinates.Longitude);
		Assert.Equal(original.Orientation, clone.Orientation);
		Assert.Equal(original.PixelDensity, clone.PixelDensity);
		Assert.Equal(original.ColorSpaceInfo, clone.ColorSpaceInfo);
		Assert.Equal(original.WhiteBalance, clone.WhiteBalance);
		Assert.Equal(original.CodecParameters["profile"], clone.CodecParameters!["profile"]);
		Assert.Equal(original.CustomMetadata["custom"], clone.CustomMetadata!["custom"]);
		Assert.Equal(original.ThumbnailData, clone.ThumbnailData);
		Assert.Equal(original.PreviewData, clone.PreviewData);
		Assert.Equal(original.DepthMapData, clone.DepthMapData);
		Assert.Equal(original.AuxiliaryImages["aux"], clone.AuxiliaryImages!["aux"]);
	}

	[Fact]
	public void Clone_ModifyingOriginal_DoesNotAffectClone()
	{
		// Arrange
		var original = new HeifMetadata
		{
			Software = "Original Software",
			CodecParameters = new Dictionary<string, object> { ["profile"] = "Main" }
		};
		var clone = original.Clone();

		// Act
		original.Software = "Modified Software";
		original.CodecParameters["profile"] = "High";
		original.CodecParameters["new"] = "value";

		// Assert
		Assert.Equal("Original Software", clone.Software);
		Assert.Equal("Main", clone.CodecParameters!["profile"]);
		Assert.False(clone.CodecParameters.ContainsKey("new"));
	}

	[Fact]
	public void Clear_RemovesAllMetadata()
	{
		// Arrange
		var metadata = new HeifMetadata
		{
			ExifData = new byte[] { 1, 2, 3 },
			XmpData = new byte[] { 4, 5, 6 },
			IccProfile = new byte[] { 7, 8, 9 },
			HdrMetadata = new HdrMetadata(),
			Software = "Test Software",
			Description = "Test Description",
			CodecParameters = new Dictionary<string, object> { ["test"] = "value" },
			CustomMetadata = new Dictionary<string, object> { ["custom"] = "data" },
			ThumbnailData = new byte[] { 10, 11, 12 },
			AuxiliaryImages = new Dictionary<string, byte[]> { ["aux"] = new byte[] { 13, 14, 15 } }
		};

		// Act
		metadata.Clear();

		// Assert
		Assert.Null(metadata.ExifData);
		Assert.Null(metadata.XmpData);
		Assert.Null(metadata.IccProfile);
		Assert.Null(metadata.HdrMetadata);
		Assert.Null(metadata.Software);
		Assert.Null(metadata.Description);
		Assert.Equal(ImageOrientation.Normal, metadata.Orientation);
		Assert.NotNull(metadata.CodecParameters); // Collection exists but is empty
		Assert.NotNull(metadata.CustomMetadata); // Collection exists but is empty
		Assert.Empty(metadata.CodecParameters);
		Assert.Empty(metadata.CustomMetadata);
		Assert.Null(metadata.ThumbnailData);
		Assert.NotNull(metadata.AuxiliaryImages); // Collection exists but is empty
		Assert.Empty(metadata.AuxiliaryImages);
	}

	[Fact]
	public void Dispose_MultipleCalls_DoesNotThrow()
	{
		// Arrange
		var metadata = new HeifMetadata();

		// Act & Assert
		metadata.Dispose();
		metadata.Dispose(); // Should not throw
	}

	[Fact]
	public async Task DisposeAsync_WithSmallData_CompletesSync()
	{
		// Arrange
		var metadata = new HeifMetadata
		{
			ExifData = new byte[1024]
		};

		// Act & Assert
		await metadata.DisposeAsync(); // Should complete synchronously
		await metadata.DisposeAsync(); // Should not throw
	}

	[Fact]
	public async Task DisposeAsync_WithLargeData_CompletesAsync()
	{
		// Arrange
		var largeSize = HeifConstants.Memory.DefaultMetadataBufferSizeMB * 1024 * 1024 + 1;
		var metadata = new HeifMetadata
		{
			PreviewData = new byte[largeSize]
		};

		// Act & Assert
		await metadata.DisposeAsync(); // Should complete asynchronously
		await metadata.DisposeAsync(); // Should not throw
	}

	[Fact]
	public void DisposedMetadata_ThrowsObjectDisposedException()
	{
		// Arrange
		var metadata = new HeifMetadata();
		metadata.Dispose();

		// Act & Assert
		Assert.Throws<ObjectDisposedException>(() => metadata.Clear());
		Assert.Throws<ObjectDisposedException>(() => metadata.Clone());
	}

	[Fact]
	public void Properties_SetAndGet_WorkCorrectly()
	{
		// Arrange
		var metadata = new HeifMetadata();
		var testTime = DateTimeOffset.UtcNow;
		var gpsCoords = new GpsCoordinates { Latitude = 40.7128, Longitude = -74.0060 };
		var hdrMetadata = new HdrMetadata { MaxLuminance = 4000.0 };

		// Act
		metadata.CreationTime = testTime;
		metadata.ModificationTime = testTime.AddMinutes(-5);
		metadata.Software = "Test Software";
		metadata.Description = "Test Description";
		metadata.Copyright = "© 2025 Test";
		metadata.Author = "Test Author";
		metadata.CameraMake = "Canon";
		metadata.CameraModel = "EOS R5";
		metadata.LensMake = "Canon";
		metadata.LensModel = "RF 24-70mm f/2.8L";
		metadata.FocalLength = 50.0;
		metadata.Aperture = 2.8;
		metadata.ExposureTime = 1.0 / 125.0;
		metadata.IsoSensitivity = 800;
		metadata.GpsCoordinates = gpsCoords;
		metadata.Orientation = ImageOrientation.Rotate180;
		metadata.PixelDensity = 300.0;
		metadata.ColorSpaceInfo = "Adobe RGB";
		metadata.WhiteBalance = "Daylight";
		metadata.HdrMetadata = hdrMetadata;

		// Assert
		Assert.Equal(testTime, metadata.CreationTime);
		Assert.Equal(testTime.AddMinutes(-5), metadata.ModificationTime);
		Assert.Equal("Test Software", metadata.Software);
		Assert.Equal("Test Description", metadata.Description);
		Assert.Equal("© 2025 Test", metadata.Copyright);
		Assert.Equal("Test Author", metadata.Author);
		Assert.Equal("Canon", metadata.CameraMake);
		Assert.Equal("EOS R5", metadata.CameraModel);
		Assert.Equal("Canon", metadata.LensMake);
		Assert.Equal("RF 24-70mm f/2.8L", metadata.LensModel);
		Assert.Equal(50.0, metadata.FocalLength);
		Assert.Equal(2.8, metadata.Aperture);
		Assert.Equal(1.0 / 125.0, metadata.ExposureTime);
		Assert.Equal(800, metadata.IsoSensitivity);
		Assert.Equal(gpsCoords, metadata.GpsCoordinates);
		Assert.Equal(ImageOrientation.Rotate180, metadata.Orientation);
		Assert.Equal(300.0, metadata.PixelDensity);
		Assert.Equal("Adobe RGB", metadata.ColorSpaceInfo);
		Assert.Equal("Daylight", metadata.WhiteBalance);
		Assert.Equal(hdrMetadata, metadata.HdrMetadata);
	}

	[Fact]
	public void CodecParameters_LazyInitialization_WorksCorrectly()
	{
		// Arrange
		var metadata = new HeifMetadata();

		// Act
		metadata.CodecParameters = new Dictionary<string, object> { ["profile"] = "Main" };

		// Assert
		Assert.NotNull(metadata.CodecParameters);
		Assert.Equal("Main", metadata.CodecParameters["profile"]);
	}

	[Fact]
	public void AuxiliaryImages_LazyInitialization_WorksCorrectly()
	{
		// Arrange
		var metadata = new HeifMetadata();

		// Act
		metadata.AuxiliaryImages = new Dictionary<string, byte[]> { ["depth"] = new byte[] { 1, 2, 3 } };

		// Assert
		Assert.NotNull(metadata.AuxiliaryImages);
		Assert.Equal(new byte[] { 1, 2, 3 }, metadata.AuxiliaryImages["depth"]);
	}
}