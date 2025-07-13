// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Validation;

/// <summary>Validation result class for metadata validation operations.</summary>
public class ValidationResult
{
	private readonly List<ValidationIssue> _issues = new();

	/// <summary>Gets all validation issues.</summary>
	public IReadOnlyList<ValidationIssue> Issues
		=> _issues.AsReadOnly();

	/// <summary>Gets all errors.</summary>
	public IEnumerable<ValidationIssue> Errors
		=> _issues.Where(i => i.Severity == ValidationSeverity.Error);

	/// <summary>Gets all warnings.</summary>
	public IEnumerable<ValidationIssue> Warnings
		=> _issues.Where(i => i.Severity == ValidationSeverity.Warning);

	/// <summary>Gets all informational messages.</summary>
	public IEnumerable<ValidationIssue> Info
		=> _issues.Where(i => i.Severity == ValidationSeverity.Info);

	/// <summary>Gets whether validation passed (no errors).</summary>
	public bool IsValid
		=> !Errors.Any();

	/// <summary>Gets whether there are any warnings.</summary>
	public bool HasWarnings
		=> Warnings.Any();

	/// <summary>Gets the total number of issues.</summary>
	public int IssueCount
		=> _issues.Count;

	/// <summary>Adds an error to the validation result.</summary>
	public void AddError(string message)
		=> _issues.Add(new ValidationIssue(ValidationSeverity.Error, message));

	/// <summary>Adds a warning to the validation result.</summary>
	public void AddWarning(string message)
		=> _issues.Add(new ValidationIssue(ValidationSeverity.Warning, message));

	/// <summary>Adds an informational message to the validation result.</summary>
	public void AddInfo(string message)
		=> _issues.Add(new ValidationIssue(ValidationSeverity.Info, message));

	/// <summary>Merges another validation result into this one.</summary>
	public void Merge(ValidationResult other)
		=> _issues.AddRange(other._issues);

	/// <summary>Gets a summary string of all issues.</summary>
	public string GetSummary()
	{
		if (!_issues.Any())
			return "Validation passed with no issues.";

		var errorCount   = Errors.Count();
		var warningCount = Warnings.Count();
		var infoCount    = Info.Count();

		return $"Validation completed: {errorCount} errors, {warningCount} warnings, {infoCount} info messages.";
	}
}
