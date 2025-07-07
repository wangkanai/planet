// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Represents camera and photographic metadata extracted from image files.
/// </summary>
public class CameraMetadata
{
	/// <summary>Gets or sets the camera manufacturer.</summary>
	public string? CameraMake { get; set; }

	/// <summary>Gets or sets the camera model.</summary>
	public string? CameraModel { get; set; }

	/// <summary>Gets or sets the lens manufacturer.</summary>
	public string? LensMake { get; set; }

	/// <summary>Gets or sets the lens model.</summary>
	public string? LensModel { get; set; }

	/// <summary>Gets or sets the focal length in millimeters.</summary>
	public double? FocalLength { get; set; }

	/// <summary>Gets or sets the aperture value (F-number).</summary>
	public double? Aperture { get; set; }

	/// <summary>Gets or sets the exposure time in seconds.</summary>
	public double? ExposureTime { get; set; }

	/// <summary>Gets or sets the ISO sensitivity.</summary>
	public int? IsoSensitivity { get; set; }

	/// <summary>Gets or sets the white balance mode.</summary>
	/// <remarks>0 = Auto, 1 = Manual</remarks>
	public int? WhiteBalance { get; set; }

	/// <summary>Gets or sets the flash mode.</summary>
	public int? Flash { get; set; }

	/// <summary>Gets or sets the exposure bias value in EV.</summary>
	public double? ExposureBias { get; set; }

	/// <summary>Gets or sets the metering mode.</summary>
	public int? MeteringMode { get; set; }

	/// <summary>Gets or sets the exposure program.</summary>
	public int? ExposureProgram { get; set; }

	/// <summary>Gets or sets the light source.</summary>
	public int? LightSource { get; set; }

	/// <summary>Gets or sets the 35mm equivalent focal length.</summary>
	public double? FocalLengthIn35mm { get; set; }

	/// <summary>Gets or sets the digital zoom ratio.</summary>
	public double? DigitalZoomRatio { get; set; }

	/// <summary>Gets or sets the scene capture type.</summary>
	public int? SceneCaptureType { get; set; }

	/// <summary>Gets or sets the contrast setting.</summary>
	public int? Contrast { get; set; }

	/// <summary>Gets or sets the saturation setting.</summary>
	public int? Saturation { get; set; }

	/// <summary>Gets or sets the sharpness setting.</summary>
	public int? Sharpness { get; set; }

	/// <summary>Gets or sets the subject distance range.</summary>
	public int? SubjectDistanceRange { get; set; }

	/// <summary>Gets or sets the sensing method.</summary>
	public int? SensingMethod { get; set; }

	/// <summary>Gets or sets the gain control.</summary>
	public int? GainControl { get; set; }

	/// <summary>Gets or sets the body serial number.</summary>
	public string? BodySerialNumber { get; set; }

	/// <summary>Gets or sets the lens serial number.</summary>
	public string? LensSerialNumber { get; set; }

	/// <summary>Gets or sets the lens specification.</summary>
	/// <remarks>Typically contains min/max focal length and aperture.</remarks>
	public double[]? LensSpecification { get; set; }

	/// <summary>Gets or sets the pixel density in X direction (horizontal resolution).</summary>
	public double? XResolution { get; set; }

	/// <summary>Gets or sets the pixel density in Y direction (vertical resolution).</summary>
	public double? YResolution { get; set; }

	/// <summary>Gets or sets the resolution unit.</summary>
	/// <remarks>1 = No unit, 2 = Inches, 3 = Centimeters</remarks>
	public int? ResolutionUnit { get; set; }

	/// <summary>Gets or sets the GPS latitude.</summary>
	public double? GpsLatitude { get; set; }

	/// <summary>Gets or sets the GPS longitude.</summary>
	public double? GpsLongitude { get; set; }

	/// <summary>Gets or sets the GPS altitude in meters.</summary>
	public double? GpsAltitude { get; set; }

	/// <summary>Gets or sets the GPS timestamp.</summary>
	public DateTime? GpsTimestamp { get; set; }

	/// <summary>Creates a copy of the camera metadata.</summary>
	public CameraMetadata Clone()
		=> new()
		   {
			   CameraMake           = CameraMake,
			   CameraModel          = CameraModel,
			   LensMake             = LensMake,
			   LensModel            = LensModel,
			   FocalLength          = FocalLength,
			   Aperture             = Aperture,
			   ExposureTime         = ExposureTime,
			   IsoSensitivity       = IsoSensitivity,
			   WhiteBalance         = WhiteBalance,
			   Flash                = Flash,
			   ExposureBias         = ExposureBias,
			   MeteringMode         = MeteringMode,
			   ExposureProgram      = ExposureProgram,
			   LightSource          = LightSource,
			   FocalLengthIn35mm    = FocalLengthIn35mm,
			   DigitalZoomRatio     = DigitalZoomRatio,
			   SceneCaptureType     = SceneCaptureType,
			   Contrast             = Contrast,
			   Saturation           = Saturation,
			   Sharpness            = Sharpness,
			   SubjectDistanceRange = SubjectDistanceRange,
			   SensingMethod        = SensingMethod,
			   GainControl          = GainControl,
			   BodySerialNumber     = BodySerialNumber,
			   LensSerialNumber     = LensSerialNumber,
			   LensSpecification    = LensSpecification?.ToArray(),
			   XResolution          = XResolution,
			   YResolution          = YResolution,
			   ResolutionUnit       = ResolutionUnit,
			   GpsLatitude          = GpsLatitude,
			   GpsLongitude         = GpsLongitude,
			   GpsAltitude          = GpsAltitude,
			   GpsTimestamp         = GpsTimestamp
		   };

	/// <summary>Clears all camera metadata values.</summary>
	public void Clear()
	{
		CameraMake           = null;
		CameraModel          = null;
		LensMake             = null;
		LensModel            = null;
		FocalLength          = null;
		Aperture             = null;
		ExposureTime         = null;
		IsoSensitivity       = null;
		WhiteBalance         = null;
		Flash                = null;
		ExposureBias         = null;
		MeteringMode         = null;
		ExposureProgram      = null;
		LightSource          = null;
		FocalLengthIn35mm    = null;
		DigitalZoomRatio     = null;
		SceneCaptureType     = null;
		Contrast             = null;
		Saturation           = null;
		Sharpness            = null;
		SubjectDistanceRange = null;
		SensingMethod        = null;
		GainControl          = null;
		BodySerialNumber     = null;
		LensSerialNumber     = null;
		LensSpecification    = null;
		XResolution          = null;
		YResolution          = null;
		ResolutionUnit       = null;
		GpsLatitude          = null;
		GpsLongitude         = null;
		GpsAltitude          = null;
		GpsTimestamp         = null;
	}
}
