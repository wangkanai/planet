// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Extensions;

/// <summary>
/// Types of validation that can be performed.
/// </summary>
[Flags]
public enum ValidationTypes
{
	/// <summary>Basic metadata validation.</summary>
	Basic = 1,

	/// <summary>Web compatibility validation.</summary>
	Web = 2,

	/// <summary>Print compatibility validation.</summary>
	Print = 4,

	/// <summary>Security validation.</summary>
	Security = 8,

	/// <summary>Archival compatibility validation.</summary>
	Archival = 16,

	/// <summary>Performance validation.</summary>
	Performance = 32,

	/// <summary>Professional use validation.</summary>
	Professional = 64,

	/// <summary>All validation types.</summary>
	All = Basic | Web | Print | Security | Archival | Performance | Professional
}
