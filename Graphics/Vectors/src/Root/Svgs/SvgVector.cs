// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors.Svgs;

public class SvgVector : ISvgVector
{
	public int  Width                 { get; set; }
	public int  Height                { get; set; }
	public bool HasLargeMetadata      { get; }
	public long EstimatedMetadataSize { get; }

	private bool _disposed = false;

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				// Release managed resources here
			}

			// Release unmanaged resources here
			_disposed = true;
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (!_disposed)
		{
			// Release managed resources asynchronously here
			_disposed = true;
		}
		GC.SuppressFinalize(this);
	}

	~SvgVector()
	{
		Dispose(false);
	}
}
