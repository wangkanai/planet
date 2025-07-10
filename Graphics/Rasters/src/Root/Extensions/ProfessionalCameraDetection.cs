// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Extensions;

/// <summary>
/// Sophisticated professional camera detection system with comprehensive camera brand and model database.
/// </summary>
public static class ProfessionalCameraDetection
{
	/// <summary>Professional camera brands with their associated professional indicators.</summary>
	private static readonly Dictionary<string, CameraBrandInfo> ProfessionalBrands = new(StringComparer.OrdinalIgnoreCase)
	{
		["Canon"] = new CameraBrandInfo
		{
			ProfessionalModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"EOS", "1D", "5D", "6D", "R5", "R6", "C70", "C200", "C300", "C500"
			},
			ConsumerModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"Rebel", "Kiss", "PowerShot", "IXUS", "SX"
			},
			ProfessionalWeight = 0.9
		},
		["Nikon"] = new CameraBrandInfo
		{
			ProfessionalModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"D850", "D780", "D750", "D500", "Z9", "Z7", "Z6", "Z5", "D6", "D5"
			},
			ConsumerModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"D3500", "D5600", "COOLPIX", "P"
			},
			ProfessionalWeight = 0.9
		},
		["Sony"] = new CameraBrandInfo
		{
			ProfessionalModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"A7R", "A7S", "A9", "A1", "FX", "α7R", "α7S", "α9", "α1"
			},
			ConsumerModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"A6000", "A6100", "A6400", "DSC", "ZV"
			},
			ProfessionalWeight = 0.8
		},
		["Fujifilm"] = new CameraBrandInfo
		{
			ProfessionalModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"GFX", "X-T4", "X-T5", "X-H", "X-Pro"
			},
			ConsumerModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"X-A", "X-E", "instax"
			},
			ProfessionalWeight = 0.7
		},
		["Leica"] = new CameraBrandInfo
		{
			ProfessionalModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"M10", "M11", "SL2", "Q2", "S3"
			},
			ConsumerModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"D-LUX", "C-LUX", "V-LUX"
			},
			ProfessionalWeight = 0.95
		},
		["Panasonic"] = new CameraBrandInfo
		{
			ProfessionalModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"S1", "S5", "GH5", "GH6", "G9"
			},
			ConsumerModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"G85", "G95", "LX", "TZ"
			},
			ProfessionalWeight = 0.6
		},
		["Olympus"] = new CameraBrandInfo
		{
			ProfessionalModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"E-M1", "E-M5", "OM-1"
			},
			ConsumerModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"E-M10", "E-PL", "TG"
			},
			ProfessionalWeight = 0.6
		}
	};

	/// <summary>
	/// Analyzes camera metadata to determine professional photography likelihood.
	/// </summary>
	/// <param name="make">Camera manufacturer/make.</param>
	/// <param name="model">Camera model.</param>
	/// <returns>Professional score from 0.0 to 1.0.</returns>
	public static double CalculateProfessionalCameraScore(string? make, string? model)
	{
		if (string.IsNullOrWhiteSpace(make))
			return 0.0;

		if (!ProfessionalBrands.TryGetValue(make.Trim(), out var brandInfo))
			return 0.1; // Unknown brand gets minimal score

		if (string.IsNullOrWhiteSpace(model))
			return brandInfo.ProfessionalWeight * 0.3; // Brand only, reduced score

		var modelTrimmed = model.Trim();

		// Check if it's a known professional model
		foreach (var professionalModel in brandInfo.ProfessionalModels)
		{
			if (modelTrimmed.Contains(professionalModel, StringComparison.OrdinalIgnoreCase))
				return brandInfo.ProfessionalWeight;
		}

		// Check if it's a known consumer model
		foreach (var consumerModel in brandInfo.ConsumerModels)
		{
			if (modelTrimmed.Contains(consumerModel, StringComparison.OrdinalIgnoreCase))
				return brandInfo.ProfessionalWeight * 0.2; // Consumer models get low score
		}

		// Unknown model from professional brand gets medium score
		return brandInfo.ProfessionalWeight * 0.5;
	}

	/// <summary>
	/// Analyzes exposure settings to determine professional photography likelihood.
	/// </summary>
	/// <param name="exposureTime">Exposure time in seconds.</param>
	/// <param name="fNumber">Aperture f-number.</param>
	/// <param name="isoSpeed">ISO speed rating.</param>
	/// <param name="whiteBalance">White balance setting.</param>
	/// <returns>Professional score from 0.0 to 1.0.</returns>
	public static double CalculateProfessionalExposureScore(double? exposureTime, double? fNumber, 
		int? isoSpeed, int? whiteBalance)
	{
		var score = 0.0;

		// Manual white balance indicates professional control
		if (whiteBalance == MetadataConstants.ProfessionalDetection.ManualWhiteBalance)
			score += 0.4;

		// Professional aperture ranges (wide for portraits, narrow for landscapes)
		if (fNumber.HasValue)
		{
			if (fNumber >= 1.2 && fNumber <= 2.8) // Wide aperture for portraits
				score += 0.3;
			else if (fNumber >= 8.0 && fNumber <= 16.0) // Narrow aperture for landscapes
				score += 0.2;
		}

		// Professional ISO usage patterns
		if (isoSpeed.HasValue)
		{
			if (isoSpeed <= MetadataConstants.Performance.CleanIsoThreshold) // Clean, professional ISO range
				score += 0.2;
			else if (isoSpeed <= MetadataConstants.Performance.AcceptableIsoThreshold) // Acceptable professional range
				score += 0.1;
		}

		// Professional exposure time patterns
		if (exposureTime.HasValue)
		{
			if (exposureTime >= MetadataConstants.Performance.LongExposureThreshold) // Long exposure for artistic effect
				score += 0.1;
			else if (exposureTime >= MetadataConstants.Performance.ProfessionalShutterSpeedMin && 
			         exposureTime <= MetadataConstants.Performance.ProfessionalShutterSpeedMax) // Standard professional range
				score += 0.1;
		}

		return Math.Min(score, 1.0);
	}

	/// <summary>
	/// Determines if the camera/lens combination suggests professional use.
	/// </summary>
	/// <param name="focalLength">Lens focal length in millimeters.</param>
	/// <param name="fNumber">Maximum aperture of the lens.</param>
	/// <returns>True if the lens characteristics suggest professional use.</returns>
	public static bool IsProfessionalLens(double? focalLength, double? fNumber)
	{
		if (!focalLength.HasValue || !fNumber.HasValue)
			return false;

		// Professional lens indicators
		// 1. Very wide apertures (expensive professional lenses)
		if (fNumber <= 1.4)
			return true;

		// 2. Professional focal length ranges
		if (focalLength >= 70 && focalLength <= 200 && fNumber <= 2.8) // Professional zoom range
			return true;

		if (focalLength >= 300) // Telephoto (sports/wildlife professional use)
			return true;

		if (focalLength <= 24 && fNumber <= 2.8) // Wide angle professional
			return true;

		return false;
	}

	/// <summary>Information about a camera brand's professional characteristics.</summary>
	private record CameraBrandInfo
	{
		public HashSet<string> ProfessionalModels { get; init; } = new();
		public HashSet<string> ConsumerModels { get; init; } = new();
		public double ProfessionalWeight { get; init; }
	}
}