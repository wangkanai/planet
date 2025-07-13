// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.

namespace Wangkanai.Graphics;

/// <summary>
/// Size difference information between two metadata instances.
/// </summary>
public record SizeDifference
{
	public long   AbsoluteDifference   { get; init; }
	public double PercentageDifference { get; init; }
	public bool   IsLarger             { get; init; }
	public bool   IsSignificant        { get; init; }
}
