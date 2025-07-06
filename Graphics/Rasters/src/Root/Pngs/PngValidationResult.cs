// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Pngs;

/// <summary>Represents the result of PNG format validation.</summary>
public class PngValidationResult
{
	/// <summary>Gets a value indicating whether the PNG is valid.</summary>
	public bool IsValid => Errors.Count == 0;

	/// <summary>Gets the list of validation errors.</summary>
	public List<string> Errors
	{
		get;
	} = [];

	/// <summary>Gets the list of validation warnings.</summary>
	public List<string> Warnings
	{
		get;
	} = [];

	/// <summary>Adds an error to the validation result.</summary>
	/// <param name="error">The error message to add.</param>
	public void AddError(string error)
	{
		ArgumentNullException.ThrowIfNull(error);
		Errors.Add(error);
	}

	/// <summary>Adds a warning to the validation result.</summary>
	/// <param name="warning">The warning message to add.</param>
	public void AddWarning(string warning)
	{
		ArgumentNullException.ThrowIfNull(warning);
		Warnings.Add(warning);
	}

	/// <summary>Gets a summary of all errors and warnings.</summary>
	/// <returns>A formatted summary string.</returns>
	public string GetSummary()
	{
		var summary = new List<string>();

		if (Errors.Count > 0)
			summary.AddRange(Errors.Select(e => $"Error: {e}"));

		if (Warnings.Count > 0)
			summary.AddRange(Warnings.Select(w => $"Warning: {w}"));

		return string.Join(Environment.NewLine, summary);
	}
}
