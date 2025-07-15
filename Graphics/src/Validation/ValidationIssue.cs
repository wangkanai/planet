// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Validation;

/// <summary>Represents a single validation issue.</summary>
public record ValidationIssue(ValidationSeverity Severity, string Message);
