// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Heifs;

/// <summary>
/// Represents the result of a HEIF validation operation.
/// </summary>
public sealed class HeifValidationResult
{
	private HeifValidationResult(bool isValid, IReadOnlyList<string> errors)
	{
		IsValid = isValid;
		Errors = errors;
	}

	/// <summary>
	/// Gets whether the validation was successful.
	/// </summary>
	public bool IsValid { get; }

	/// <summary>
	/// Gets the list of validation errors, if any.
	/// </summary>
	public IReadOnlyList<string> Errors { get; }

	/// <summary>
	/// Gets the first error message, or null if validation was successful.
	/// </summary>
	public string? FirstError => Errors.Count > 0 ? Errors[0] : null;

	/// <summary>
	/// Gets all error messages joined with newlines.
	/// </summary>
	public string AllErrors => string.Join(Environment.NewLine, Errors);

	/// <summary>
	/// Creates a successful validation result.
	/// </summary>
	/// <returns>A successful validation result.</returns>
	public static HeifValidationResult CreateSuccess()
	{
		return new HeifValidationResult(true, Array.Empty<string>());
	}

	/// <summary>
	/// Creates a failed validation result with a single error.
	/// </summary>
	/// <param name="error">The validation error message.</param>
	/// <returns>A failed validation result.</returns>
	public static HeifValidationResult CreateFailure(string error)
	{
		if (string.IsNullOrWhiteSpace(error))
			throw new ArgumentException("Error message cannot be null or whitespace.", nameof(error));

		return new HeifValidationResult(false, new[] { error });
	}

	/// <summary>
	/// Creates a failed validation result with multiple errors.
	/// </summary>
	/// <param name="errors">The validation error messages.</param>
	/// <returns>A failed validation result.</returns>
	public static HeifValidationResult CreateFailure(IEnumerable<string> errors)
	{
		if (errors == null)
			throw new ArgumentNullException(nameof(errors));

		var errorList = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
		
		if (errorList.Count == 0)
			throw new ArgumentException("At least one non-empty error message must be provided.", nameof(errors));

		return new HeifValidationResult(false, errorList);
	}

	/// <summary>
	/// Throws an exception if the validation failed.
	/// </summary>
	/// <param name="exceptionMessage">Optional custom exception message.</param>
	/// <exception cref="InvalidOperationException">Thrown if validation failed.</exception>
	public void ThrowIfInvalid(string? exceptionMessage = null)
	{
		if (!IsValid)
		{
			var message = exceptionMessage ?? $"HEIF validation failed: {AllErrors}";
			throw new InvalidOperationException(message);
		}
	}

	/// <summary>
	/// Combines this validation result with another.
	/// </summary>
	/// <param name="other">The other validation result to combine with.</param>
	/// <returns>A combined validation result.</returns>
	public HeifValidationResult Combine(HeifValidationResult other)
	{
		if (other == null)
			throw new ArgumentNullException(nameof(other));

		if (IsValid && other.IsValid)
			return CreateSuccess();

		var allErrors = new List<string>();
		allErrors.AddRange(Errors);
		allErrors.AddRange(other.Errors);

		return CreateFailure(allErrors);
	}

	/// <summary>
	/// Returns a string representation of the validation result.
	/// </summary>
	/// <returns>A string describing the validation result.</returns>
	public override string ToString()
	{
		return IsValid 
			? "Validation successful" 
			: $"Validation failed with {Errors.Count} error(s): {AllErrors}";
	}
}