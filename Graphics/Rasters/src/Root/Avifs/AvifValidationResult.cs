// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Avifs;

/// <summary>
/// Represents the result of AVIF format validation.
/// </summary>
public class AvifValidationResult
{
	/// <summary>List of validation errors that prevent proper AVIF encoding/decoding.</summary>
	public List<string> Errors { get; } = new();

	/// <summary>List of validation warnings that may affect quality or performance.</summary>
	public List<string> Warnings { get; } = new();

	/// <summary>Indicates if the AVIF raster is valid (no errors).</summary>
	public bool IsValid => Errors.Count == 0;

	/// <summary>Indicates if there are any warnings.</summary>
	public bool HasWarnings => Warnings.Count > 0;

	/// <summary>Gets the total number of issues (errors + warnings).</summary>
	public int TotalIssues => Errors.Count + Warnings.Count;

	/// <summary>
	/// Adds a validation error.
	/// </summary>
	/// <param name="error">The error message.</param>
	public void AddError(string error)
	{
		if (!string.IsNullOrEmpty(error))
			Errors.Add(error);
	}

	/// <summary>
	/// Adds a validation warning.
	/// </summary>
	/// <param name="warning">The warning message.</param>
	public void AddWarning(string warning)
	{
		if (!string.IsNullOrEmpty(warning))
			Warnings.Add(warning);
	}

	/// <summary>
	/// Gets a summary of the validation results.
	/// </summary>
	/// <returns>A human-readable summary string.</returns>
	public string GetSummary()
	{
		if (IsValid && !HasWarnings)
			return "Valid AVIF configuration with no issues.";

		if (IsValid && HasWarnings)
			return $"Valid AVIF configuration with {Warnings.Count} warning(s).";

		return $"Invalid AVIF configuration: {Errors.Count} error(s), {Warnings.Count} warning(s).";
	}

	/// <summary>
	/// Gets all issues as a formatted string.
	/// </summary>
	/// <returns>A formatted string containing all errors and warnings.</returns>
	public string GetFormattedResults()
	{
		var result = new List<string> { GetSummary() };

		if (Errors.Count > 0)
		{
			result.Add("\nErrors:");
			result.AddRange(Errors.Select(e => $"  - {e}"));
		}

		if (Warnings.Count > 0)
		{
			result.Add("\nWarnings:");
			result.AddRange(Warnings.Select(w => $"  - {w}"));
		}

		return string.Join("\n", result);
	}

	/// <summary>
	/// Merges another validation result into this one.
	/// </summary>
	/// <param name="other">The other validation result to merge.</param>
	public void Merge(AvifValidationResult other)
	{
		if (other == null)
			return;

		Errors.AddRange(other.Errors);
		Warnings.AddRange(other.Warnings);
	}

	/// <summary>
	/// Gets validation results categorized by severity.
	/// </summary>
	/// <returns>A dictionary with severity levels as keys and issues as values.</returns>
	public Dictionary<string, List<string>> GetCategorizedResults()
	{
		return new Dictionary<string, List<string>>
		{
			["Errors"] = new List<string>(Errors),
			["Warnings"] = new List<string>(Warnings)
		};
	}

	/// <summary>
	/// Clears all validation results.
	/// </summary>
	public void Clear()
	{
		Errors.Clear();
		Warnings.Clear();
	}

	/// <summary>
	/// Creates a copy of the validation result.
	/// </summary>
	/// <returns>A new instance with the same errors and warnings.</returns>
	public AvifValidationResult Clone()
	{
		var clone = new AvifValidationResult();
		clone.Errors.AddRange(Errors);
		clone.Warnings.AddRange(Warnings);
		return clone;
	}
}