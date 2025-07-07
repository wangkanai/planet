// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors.Svgs;

public class SvgVector : ISvgVector
{
	public int  Width                 { get; set; }
	public int  Height                { get; set; }
	public bool HasLargeMetadata      { get; }
	public long EstimatedMetadataSize { get; }

	public void Dispose()
	{
		// TODO release managed resources here
	}

	public async ValueTask DisposeAsync()
	{
		// TODO release managed resources here
	}
}

public interface ISvgVector : IVector { }
