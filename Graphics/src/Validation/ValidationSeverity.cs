// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Validation;

/// <summary>
/// Validation severity levels.
/// </summary>
public enum ValidationSeverity
{
	/// <summary>Informational message.</summary>
	Info,

	/// <summary>Warning that doesn't prevent usage.</summary>
	Warning,

	/// <summary>Error that may prevent proper usage.</summary>
	Error
}
