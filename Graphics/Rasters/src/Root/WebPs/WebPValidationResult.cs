// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.WebPs;

/// <summary>Represents the result of WebP validation.</summary>
public class WebPValidationResult
{
	/// <summary>Gets a value indicating whether the validation passed.</summary>
	public bool IsValid => Errors.Count == 0;

	/// <summary>Gets the list of validation errors.</summary>
	public List<string> Errors { get; } = new();

	/// <summary>Gets the list of validation warnings.</summary>
	public List<string> Warnings { get; } = new();

	/// <summary>Adds an error to the validation result.</summary>
	/// <param name="error">The error message to add.</param>
	public void AddError(string error)
	{
		Errors.Add(error);
	}

	/// <summary>Adds a warning to the validation result.</summary>
	/// <param name="warning">The warning message to add.</param>
	public void AddWarning(string warning)
	{
		Warnings.Add(warning);
	}

	/// <summary>Gets a summary of all validation issues.</summary>
	/// <returns>A formatted string containing all errors and warnings.</returns>
	public string GetSummary()
	{
		var summary = new List<string>();

		if (Errors.Count > 0)
		{
			summary.Add($"Errors ({Errors.Count}):");
			summary.AddRange(Errors.Select(e => $"  - {e}"));
		}

		if (Warnings.Count > 0)
		{
			summary.Add($"Warnings ({Warnings.Count}):");
			summary.AddRange(Warnings.Select(w => $"  - {w}"));
		}

		return summary.Count > 0
			       ? string.Join(Environment.NewLine, summary)
			       : "No validation issues found.";
	}
}
