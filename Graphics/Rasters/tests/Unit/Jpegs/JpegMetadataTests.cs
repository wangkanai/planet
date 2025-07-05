// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Jpegs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Jpegs;

public class JpegMetadataTests
{
	[Fact]
	public void Constructor_ShouldInitializeEmptyCollections()
	{
		// Act
		var metadata = new JpegMetadata();

		// Assert
		Assert.NotNull(metadata.CustomExifTags);
		Assert.Empty(metadata.CustomExifTags);
		Assert.NotNull(metadata.IptcTags);
		Assert.Empty(metadata.IptcTags);
		Assert.NotNull(metadata.XmpTags);
		Assert.Empty(metadata.XmpTags);
		Assert.Null(metadata.IccProfile);
	}

	[Fact]
	public void BasicProperties_CanBeSetAndRetrieved()
	{
		// Arrange
		var metadata = new JpegMetadata();
		var now = DateTime.Now;

		// Act
		metadata.ImageDescription = "Test JPEG Image";
		metadata.Make = "Canon";
		metadata.Model = "EOS R5";
		metadata.Software = "Adobe Photoshop";
		metadata.Copyright = "Copyright 2025";
		metadata.Artist = "John Doe";
		metadata.DateTime = now;

		// Assert
		Assert.Equal("Test JPEG Image", metadata.ImageDescription);
		Assert.Equal("Canon", metadata.Make);
		Assert.Equal("EOS R5", metadata.Model);
		Assert.Equal("Adobe Photoshop", metadata.Software);
		Assert.Equal("Copyright 2025", metadata.Copyright);
		Assert.Equal("John Doe", metadata.Artist);
		Assert.Equal(now, metadata.DateTime);
	}

	[Fact]
	public void ResolutionProperties_CanBeSetAndRetrieved()
	{
		// Arrange
		var metadata = new JpegMetadata();

		// Act
		metadata.XResolution = 300.0;
		metadata.YResolution = 300.0;
		metadata.ResolutionUnit = 2; // Inches

		// Assert
		Assert.Equal(300.0, metadata.XResolution);
		Assert.Equal(300.0, metadata.YResolution);
		Assert.Equal(2, metadata.ResolutionUnit);
	}

	[Fact]
	public void CameraProperties_CanBeSetAndRetrieved()
	{
		// Arrange
		var metadata = new JpegMetadata();

		// Act
		metadata.Orientation = 1;
		metadata.ExposureTime = 1.0 / 125.0;
		metadata.FNumber = 5.6;
		metadata.IsoSpeedRating = 400;
		metadata.FocalLength = 85.0;
		metadata.ColorSpace = 1;
		metadata.WhiteBalance = 0;

		// Assert
		Assert.Equal(1, metadata.Orientation);
		Assert.Equal(1.0 / 125.0, metadata.ExposureTime);
		Assert.Equal(5.6, metadata.FNumber);
		Assert.Equal(400, metadata.IsoSpeedRating);
		Assert.Equal(85.0, metadata.FocalLength);
		Assert.Equal(1, metadata.ColorSpace);
		Assert.Equal(0, metadata.WhiteBalance);
	}

	[Fact]
	public void GpsProperties_CanBeSetAndRetrieved()
	{
		// Arrange
		var metadata = new JpegMetadata();

		// Act
		metadata.GpsLatitude = 35.6762; // Tokyo latitude
		metadata.GpsLongitude = 139.6503; // Tokyo longitude

		// Assert
		Assert.Equal(35.6762, metadata.GpsLatitude);
		Assert.Equal(139.6503, metadata.GpsLongitude);
	}

	[Fact]
	public void CustomExifTags_CanBeAddedAndRetrieved()
	{
		// Arrange
		var metadata = new JpegMetadata();

		// Act
		metadata.CustomExifTags.Add(271, "Camera Make");
		metadata.CustomExifTags.Add(272, "Camera Model");
		metadata.CustomExifTags.Add(305, "Software Version");

		// Assert
		Assert.Equal(3, metadata.CustomExifTags.Count);
		Assert.Equal("Camera Make", metadata.CustomExifTags[271]);
		Assert.Equal("Camera Model", metadata.CustomExifTags[272]);
		Assert.Equal("Software Version", metadata.CustomExifTags[305]);
	}

	[Fact]
	public void IptcTags_CanBeAddedAndRetrieved()
	{
		// Arrange
		var metadata = new JpegMetadata();

		// Act
		metadata.IptcTags.Add("Keywords", "landscape, sunset, photography");
		metadata.IptcTags.Add("Caption", "Beautiful sunset over mountains");
		metadata.IptcTags.Add("Category", "Nature");

		// Assert
		Assert.Equal(3, metadata.IptcTags.Count);
		Assert.Equal("landscape, sunset, photography", metadata.IptcTags["Keywords"]);
		Assert.Equal("Beautiful sunset over mountains", metadata.IptcTags["Caption"]);
		Assert.Equal("Nature", metadata.IptcTags["Category"]);
	}

	[Fact]
	public void XmpTags_CanBeAddedAndRetrieved()
	{
		// Arrange
		var metadata = new JpegMetadata();

		// Act
		metadata.XmpTags.Add("dc:creator", "John Doe");
		metadata.XmpTags.Add("dc:title", "Test Image");
		metadata.XmpTags.Add("xmp:CreatorTool", "Adobe Photoshop");

		// Assert
		Assert.Equal(3, metadata.XmpTags.Count);
		Assert.Equal("John Doe", metadata.XmpTags["dc:creator"]);
		Assert.Equal("Test Image", metadata.XmpTags["dc:title"]);
		Assert.Equal("Adobe Photoshop", metadata.XmpTags["xmp:CreatorTool"]);
	}

	[Fact]
	public void IccProfile_CanBeSetAndRetrieved()
	{
		// Arrange
		var metadata = new JpegMetadata();
		var iccProfile = new byte[] { 0x41, 0x44, 0x42, 0x45, 0x02, 0x10, 0x00, 0x00 };

		// Act
		metadata.IccProfile = iccProfile;

		// Assert
		Assert.NotNull(metadata.IccProfile);
		Assert.Equal(iccProfile, metadata.IccProfile);
	}

	[Fact]
	public void AllNullableProperties_CanBeSetToNull()
	{
		// Arrange
		var metadata = new JpegMetadata
		{
			ImageDescription = "Test",
			Make = "Test",
			Model = "Test",
			Software = "Test",
			Copyright = "Test",
			Artist = "Test",
			DateTime = DateTime.Now,
			XResolution = 300.0,
			YResolution = 300.0,
			ResolutionUnit = 2,
			Orientation = 1,
			ExposureTime = 1.0,
			FNumber = 5.6,
			IsoSpeedRating = 400,
			FocalLength = 85.0,
			GpsLatitude = 35.0,
			GpsLongitude = 139.0,
			ColorSpace = 1,
			WhiteBalance = 0,
			IccProfile = new byte[] { 0x01, 0x02, 0x03 }
		};

		// Act
		metadata.ImageDescription = null;
		metadata.Make = null;
		metadata.Model = null;
		metadata.Software = null;
		metadata.Copyright = null;
		metadata.Artist = null;
		metadata.DateTime = null;
		metadata.XResolution = null;
		metadata.YResolution = null;
		metadata.ResolutionUnit = null;
		metadata.Orientation = null;
		metadata.ExposureTime = null;
		metadata.FNumber = null;
		metadata.IsoSpeedRating = null;
		metadata.FocalLength = null;
		metadata.GpsLatitude = null;
		metadata.GpsLongitude = null;
		metadata.ColorSpace = null;
		metadata.WhiteBalance = null;
		metadata.IccProfile = null;

		// Assert
		Assert.Null(metadata.ImageDescription);
		Assert.Null(metadata.Make);
		Assert.Null(metadata.Model);
		Assert.Null(metadata.Software);
		Assert.Null(metadata.Copyright);
		Assert.Null(metadata.Artist);
		Assert.Null(metadata.DateTime);
		Assert.Null(metadata.XResolution);
		Assert.Null(metadata.YResolution);
		Assert.Null(metadata.ResolutionUnit);
		Assert.Null(metadata.Orientation);
		Assert.Null(metadata.ExposureTime);
		Assert.Null(metadata.FNumber);
		Assert.Null(metadata.IsoSpeedRating);
		Assert.Null(metadata.FocalLength);
		Assert.Null(metadata.GpsLatitude);
		Assert.Null(metadata.GpsLongitude);
		Assert.Null(metadata.ColorSpace);
		Assert.Null(metadata.WhiteBalance);
		Assert.Null(metadata.IccProfile);
	}

	[Fact]
	public void ComplexMetadata_CanBeBuilt()
	{
		// Arrange & Act
		var metadata = new JpegMetadata
		{
			ImageDescription = "Professional landscape photograph",
			Make = "Canon",
			Model = "EOS R5",
			Software = "Adobe Lightroom Classic",
			Copyright = "© 2025 John Doe Photography",
			Artist = "John Doe",
			DateTime = new DateTime(2025, 1, 15, 14, 30, 0),
			XResolution = 300.0,
			YResolution = 300.0,
			ResolutionUnit = 2,
			Orientation = 1,
			ExposureTime = 1.0 / 250.0,
			FNumber = 8.0,
			IsoSpeedRating = 100,
			FocalLength = 24.0,
			GpsLatitude = 46.5197,
			GpsLongitude = 6.6323,
			ColorSpace = 1,
			WhiteBalance = 0
		};

		// Add EXIF tags
		metadata.CustomExifTags.Add(36867, "2025:01:15 14:30:00"); // DateTimeOriginal
		metadata.CustomExifTags.Add(37377, 8.64); // ShutterSpeedValue
		metadata.CustomExifTags.Add(37378, 6.0); // ApertureValue

		// Add IPTC tags
		metadata.IptcTags.Add("Keywords", "landscape, mountains, nature, switzerland");
		metadata.IptcTags.Add("Caption", "Beautiful mountain landscape in the Swiss Alps");
		metadata.IptcTags.Add("Category", "Nature");
		metadata.IptcTags.Add("Urgency", "5");

		// Add XMP tags
		metadata.XmpTags.Add("dc:creator", "John Doe");
		metadata.XmpTags.Add("dc:title", "Swiss Alps Landscape");
		metadata.XmpTags.Add("dc:description", "Professional landscape photograph of the Swiss Alps");
		metadata.XmpTags.Add("xmp:CreatorTool", "Adobe Lightroom Classic");

		// Assert
		Assert.Equal("Professional landscape photograph", metadata.ImageDescription);
		Assert.Equal("Canon", metadata.Make);
		Assert.Equal("EOS R5", metadata.Model);
		Assert.Equal(3, metadata.CustomExifTags.Count);
		Assert.Equal(4, metadata.IptcTags.Count);
		Assert.Equal(4, metadata.XmpTags.Count);
		Assert.Equal(46.5197, metadata.GpsLatitude);
		Assert.Equal(6.6323, metadata.GpsLongitude);
	}
}
