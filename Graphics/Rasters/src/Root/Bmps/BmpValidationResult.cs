// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

/// <summary>Represents the result of BMP validation with errors and warnings.</summary>
public class BmpValidationResult
{
	/// <summary>Gets the list of validation errors.</summary>
	public List<string> Errors { get; } = new();

	/// <summary>Gets the list of validation warnings.</summary>
	public List<string> Warnings { get; } = new();

	/// <summary>Gets a value indicating whether the validation passed without errors.</summary>
	public bool IsValid => Errors.Count == 0;

	/// <summary>Gets a value indicating whether there are any warnings.</summary>
	public bool HasWarnings => Warnings.Count > 0;

	/// <summary>Gets the total number of issues (errors + warnings).</summary>
	public int TotalIssues => Errors.Count + Warnings.Count;

	/// <summary>Adds an error to the validation result.</summary>
	/// <param name="message">The error message.</param>
	public void AddError(string message)
	{
		Errors.Add(message);
	}

	/// <summary>Adds a warning to the validation result.</summary>
	/// <param name="message">The warning message.</param>
	public void AddWarning(string message)
	{
		Warnings.Add(message);
	}

	/// <summary>Adds multiple errors to the validation result.</summary>
	/// <param name="messages">The error messages to add.</param>
	public void AddErrors(IEnumerable<string> messages)
	{
		Errors.AddRange(messages);
	}

	/// <summary>Adds multiple warnings to the validation result.</summary>
	/// <param name="messages">The warning messages to add.</param>
	public void AddWarnings(IEnumerable<string> messages)
	{
		Warnings.AddRange(messages);
	}

	/// <summary>Merges another validation result into this one.</summary>
	/// <param name="other">The validation result to merge.</param>
	public void Merge(BmpValidationResult other)
	{
		Errors.AddRange(other.Errors);
		Warnings.AddRange(other.Warnings);
	}

	/// <summary>Clears all errors and warnings.</summary>
	public void Clear()
	{
		Errors.Clear();
		Warnings.Clear();
	}

	/// <summary>Returns a string representation of the validation result.</summary>
	/// <returns>A formatted string containing all errors and warnings.</returns>
	public override string ToString()
	{
		if (IsValid && !HasWarnings)
			return "BMP validation passed with no issues.";

		var result = new System.Text.StringBuilder();

		if (Errors.Count > 0)
		{
			result.AppendLine($"Errors ({Errors.Count}):");
			foreach (var error in Errors)
				result.AppendLine($"  - {error}");
		}

		if (Warnings.Count > 0)
		{
			if (result.Length > 0)
				result.AppendLine();

			result.AppendLine($"Warnings ({Warnings.Count}):");
			foreach (var warning in Warnings)
				result.AppendLine($"  - {warning}");
		}

		return result.ToString().TrimEnd();
	}

	/// <summary>Gets a summary of the validation result.</summary>
	/// <returns>A brief summary string.</returns>
	public string GetSummary()
	{
		if (IsValid && !HasWarnings)
			return "Valid";

		if (!IsValid && !HasWarnings)
			return $"Invalid ({Errors.Count} errors)";

		if (IsValid && HasWarnings)
			return $"Valid with warnings ({Warnings.Count} warnings)";

		return $"Invalid ({Errors.Count} errors, {Warnings.Count} warnings)";
	}
}
