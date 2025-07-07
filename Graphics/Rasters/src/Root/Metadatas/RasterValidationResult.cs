// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Represents the result of raster image validation.
/// </summary>
public class RasterValidationResult
{
	/// <summary>
	/// Gets a value indicating whether the validation passed.
	/// </summary>
	public bool IsValid => Errors.Count == 0;

	/// <summary>
	/// Gets the list of validation errors.
	/// </summary>
	public List<string> Errors { get; } = new();

	/// <summary>
	/// Gets the list of validation warnings.
	/// </summary>
	public List<string> Warnings { get; } = new();

	/// <summary>
	/// Gets the list of validation information messages.
	/// </summary>
	public List<string> Information { get; } = new();

	/// <summary>
	/// Adds an error to the validation result.
	/// </summary>
	/// <param name="error">The error message to add.</param>
	public void AddError(string error)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(error);
		Errors.Add(error);
	}

	/// <summary>
	/// Adds a warning to the validation result.
	/// </summary>
	/// <param name="warning">The warning message to add.</param>
	public void AddWarning(string warning)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(warning);
		Warnings.Add(warning);
	}

	/// <summary>
	/// Adds an information message to the validation result.
	/// </summary>
	/// <param name="info">The information message to add.</param>
	public void AddInformation(string info)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(info);
		Information.Add(info);
	}

	/// <summary>
	/// Merges another validation result into this one.
	/// </summary>
	/// <param name="other">The other validation result to merge.</param>
	public void Merge(RasterValidationResult other)
	{
		ArgumentNullException.ThrowIfNull(other);

		Errors.AddRange(other.Errors);
		Warnings.AddRange(other.Warnings);
		Information.AddRange(other.Information);
	}

	/// <summary>
	/// Gets a summary of all validation issues.
	/// </summary>
	/// <returns>A formatted string containing all errors, warnings, and information.</returns>
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
			if (summary.Count > 0) summary.Add(string.Empty);
			summary.Add($"Warnings ({Warnings.Count}):");
			summary.AddRange(Warnings.Select(w => $"  - {w}"));
		}

		if (Information.Count > 0)
		{
			if (summary.Count > 0) summary.Add(string.Empty);
			summary.Add($"Information ({Information.Count}):");
			summary.AddRange(Information.Select(i => $"  - {i}"));
		}

		return summary.Count > 0
			       ? string.Join(Environment.NewLine, summary)
			       : "Validation passed with no issues.";
	}

	/// <summary>
	/// Clears all validation messages.
	/// </summary>
	public void Clear()
	{
		Errors.Clear();
		Warnings.Clear();
		Information.Clear();
	}

	/// <summary>
	/// Creates a successful validation result with no issues.
	/// </summary>
	/// <returns>A new validation result indicating success.</returns>
	public static RasterValidationResult Success()
	{
		return new RasterValidationResult();
	}

	/// <summary>
	/// Creates a failed validation result with an error message.
	/// </summary>
	/// <param name="error">The error message.</param>
	/// <returns>A new validation result with the specified error.</returns>
	public static RasterValidationResult Failure(string error)
	{
		var result = new RasterValidationResult();
		result.AddError(error);
		return result;
	}

	/// <summary>
	/// Creates a validation result with a warning message.
	/// </summary>
	/// <param name="warning">The warning message.</param>
	/// <returns>A new validation result with the specified warning.</returns>
	public static RasterValidationResult WithWarning(string warning)
	{
		var result = new RasterValidationResult();
		result.AddWarning(warning);
		return result;
	}
}