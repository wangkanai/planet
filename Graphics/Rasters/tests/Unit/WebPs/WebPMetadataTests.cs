// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.WebPs;

namespace Wangkanai.Graphics.Rasters.WebPs;

public class WebPMetadataTests
{
	[Fact]
	public void Constructor_ShouldInitializeWithDefaults()
	{
		// Act
		var metadata = new WebPMetadata();

		// Assert
		Assert.True(metadata.IccProfile.IsEmpty);
		Assert.True(metadata.ExifData.IsEmpty);
		Assert.True(metadata.XmpData.IsEmpty);
		Assert.Null(metadata.CreationDateTime);
		Assert.Null(metadata.Software);
		Assert.Null(metadata.Description);
		Assert.Null(metadata.Copyright);
		Assert.Null(metadata.Artist);
		Assert.Null(metadata.Title);
		Assert.Empty(metadata.CustomChunks);
		Assert.False(metadata.HasAnimation);
		Assert.Equal(0, metadata.AnimationLoops);
		Assert.Equal(WebPConstants.DefaultBackgroundColor, metadata.BackgroundColor);
		Assert.Empty(metadata.AnimationFrames);
		Assert.False(metadata.IsExtended);
		Assert.False(metadata.HasAlpha);
		Assert.False(metadata.HasIccProfile);
		Assert.False(metadata.HasExif);
		Assert.False(metadata.HasXmp);
	}

	[Fact]
	public void IccProfile_ShouldAcceptData()
	{
		// Arrange
		var metadata = new WebPMetadata();
		var profileData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		metadata.IccProfile = profileData;

		// Assert
		Assert.False(metadata.IccProfile.IsEmpty);
		Assert.Equal(4, metadata.IccProfile.Length);
		Assert.True(metadata.IccProfile.Span.SequenceEqual(profileData));
	}

	[Fact]
	public void ExifData_ShouldAcceptData()
	{
		// Arrange
		var metadata = new WebPMetadata();
		var exifData = new byte[] { 0xFF, 0xE1, 0x00, 0x16 };

		// Act
		metadata.ExifData = exifData;

		// Assert
		Assert.False(metadata.ExifData.IsEmpty);
		Assert.Equal(4, metadata.ExifData.Length);
		Assert.True(metadata.ExifData.Span.SequenceEqual(exifData));
	}

	[Fact]
	public void XmpData_ShouldAcceptData()
	{
		// Arrange
		var metadata = new WebPMetadata();
		var xmpData = "<?xml version='1.0'?><rdf:RDF></rdf:RDF>"u8.ToArray();

		// Act
		metadata.XmpData = xmpData;

		// Assert
		Assert.False(metadata.XmpData.IsEmpty);
		Assert.True(metadata.XmpData.Length > 0);
	}

	[Fact]
	public void StringProperties_ShouldAcceptValues()
	{
		// Arrange
		var metadata = new WebPMetadata();

		// Act
		metadata.Software = "Test Software";
		metadata.Description = "Test Description";
		metadata.Copyright = "Test Copyright";
		metadata.Artist = "Test Artist";
		metadata.Title = "Test Title";

		// Assert
		Assert.Equal("Test Software", metadata.Software);
		Assert.Equal("Test Description", metadata.Description);
		Assert.Equal("Test Copyright", metadata.Copyright);
		Assert.Equal("Test Artist", metadata.Artist);
		Assert.Equal("Test Title", metadata.Title);
	}

	[Fact]
	public void CreationDateTime_ShouldAcceptDateTime()
	{
		// Arrange
		var metadata = new WebPMetadata();
		var dateTime = new DateTime(2025, 7, 6, 12, 0, 0, DateTimeKind.Utc);

		// Act
		metadata.CreationDateTime = dateTime;

		// Assert
		Assert.NotNull(metadata.CreationDateTime);
		Assert.Equal(dateTime, metadata.CreationDateTime.Value);
	}

	[Fact]
	public void CustomChunks_ShouldSupportAddingChunks()
	{
		// Arrange
		var metadata = new WebPMetadata();
		var chunkData = new byte[] { 0x01, 0x02, 0x03 };

		// Act
		metadata.CustomChunks.Add("TEST", chunkData);
		metadata.CustomChunks.Add("USER", new byte[] { 0x04, 0x05 });

		// Assert
		Assert.Equal(2, metadata.CustomChunks.Count);
		Assert.True(metadata.CustomChunks.ContainsKey("TEST"));
		Assert.True(metadata.CustomChunks.ContainsKey("USER"));
		Assert.True(metadata.CustomChunks["TEST"].Span.SequenceEqual(chunkData));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(5)]
	[InlineData(ushort.MaxValue)]
	public void AnimationLoops_ShouldAcceptValidValues(ushort loops)
	{
		// Arrange
		var metadata = new WebPMetadata();

		// Act
		metadata.AnimationLoops = loops;

		// Assert
		Assert.Equal(loops, metadata.AnimationLoops);
	}

	[Theory]
	[InlineData(0x00000000u)] // Transparent
	[InlineData(0xFF000000u)] // Black
	[InlineData(0xFFFFFFFFu)] // White
	[InlineData(0x80FF0000u)] // Semi-transparent red
	public void BackgroundColor_ShouldAcceptValidValues(uint color)
	{
		// Arrange
		var metadata = new WebPMetadata();

		// Act
		metadata.BackgroundColor = color;

		// Assert
		Assert.Equal(color, metadata.BackgroundColor);
	}

	[Fact]
	public void AnimationFrames_ShouldSupportAddingFrames()
	{
		// Arrange
		var metadata = new WebPMetadata();
		var frame1 = new WebPAnimationFrame
		{
			Width = 100,
			Height = 100,
			Duration = 1000,
			OffsetX = 0,
			OffsetY = 0
		};
		var frame2 = new WebPAnimationFrame
		{
			Width = 200,
			Height = 150,
			Duration = 500,
			OffsetX = 50,
			OffsetY = 25
		};

		// Act
		metadata.AnimationFrames.Add(frame1);
		metadata.AnimationFrames.Add(frame2);

		// Assert
		Assert.Equal(2, metadata.AnimationFrames.Count);
		Assert.Equal(frame1, metadata.AnimationFrames[0]);
		Assert.Equal(frame2, metadata.AnimationFrames[1]);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void BooleanFlags_ShouldAcceptValues(bool value)
	{
		// Arrange
		var metadata = new WebPMetadata();

		// Act
		metadata.HasAnimation = value;
		metadata.IsExtended = value;
		metadata.HasAlpha = value;
		metadata.HasIccProfile = value;
		metadata.HasExif = value;
		metadata.HasXmp = value;

		// Assert
		Assert.Equal(value, metadata.HasAnimation);
		Assert.Equal(value, metadata.IsExtended);
		Assert.Equal(value, metadata.HasAlpha);
		Assert.Equal(value, metadata.HasIccProfile);
		Assert.Equal(value, metadata.HasExif);
		Assert.Equal(value, metadata.HasXmp);
	}
}

public class WebPAnimationFrameTests
{
	[Fact]
	public void Constructor_ShouldInitializeWithDefaults()
	{
		// Act
		var frame = new WebPAnimationFrame();

		// Assert
		Assert.Equal(0, frame.OffsetX);
		Assert.Equal(0, frame.OffsetY);
		Assert.Equal(0, frame.Width);
		Assert.Equal(0, frame.Height);
		Assert.Equal(0u, frame.Duration);
		Assert.Equal(WebPDisposalMethod.None, frame.DisposalMethod);
		Assert.Equal(WebPBlendingMethod.AlphaBlend, frame.BlendingMethod);
		Assert.True(frame.Data.IsEmpty);
	}

	[Theory]
	[InlineData(100, 200, 300, 400)]
	[InlineData(0, 0, 1920, 1080)]
	[InlineData(50, 75, 800, 600)]
	public void DimensionProperties_ShouldAcceptValues(ushort offsetX, ushort offsetY, ushort width, ushort height)
	{
		// Arrange
		var frame = new WebPAnimationFrame();

		// Act
		frame.OffsetX = offsetX;
		frame.OffsetY = offsetY;
		frame.Width = width;
		frame.Height = height;

		// Assert
		Assert.Equal(offsetX, frame.OffsetX);
		Assert.Equal(offsetY, frame.OffsetY);
		Assert.Equal(width, frame.Width);
		Assert.Equal(height, frame.Height);
	}

	[Theory]
	[InlineData(0u)]      // Instant
	[InlineData(100u)]    // 100ms
	[InlineData(1000u)]   // 1 second
	[InlineData(5000u)]   // 5 seconds
	public void Duration_ShouldAcceptValues(uint duration)
	{
		// Arrange
		var frame = new WebPAnimationFrame();

		// Act
		frame.Duration = duration;

		// Assert
		Assert.Equal(duration, frame.Duration);
	}

	[Theory]
	[InlineData(WebPDisposalMethod.None)]
	[InlineData(WebPDisposalMethod.Background)]
	public void DisposalMethod_ShouldAcceptValidValues(WebPDisposalMethod method)
	{
		// Arrange
		var frame = new WebPAnimationFrame();

		// Act
		frame.DisposalMethod = method;

		// Assert
		Assert.Equal(method, frame.DisposalMethod);
	}

	[Theory]
	[InlineData(WebPBlendingMethod.AlphaBlend)]
	[InlineData(WebPBlendingMethod.NoBlend)]
	public void BlendingMethod_ShouldAcceptValidValues(WebPBlendingMethod method)
	{
		// Arrange
		var frame = new WebPAnimationFrame();

		// Act
		frame.BlendingMethod = method;

		// Assert
		Assert.Equal(method, frame.BlendingMethod);
	}

	[Fact]
	public void Data_ShouldAcceptFrameData()
	{
		// Arrange
		var frame = new WebPAnimationFrame();
		var frameData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

		// Act
		frame.Data = frameData;

		// Assert
		Assert.False(frame.Data.IsEmpty);
		Assert.Equal(5, frame.Data.Length);
		Assert.True(frame.Data.Span.SequenceEqual(frameData));
	}
}
