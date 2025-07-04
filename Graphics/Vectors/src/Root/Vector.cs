// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors;

/// <summary>Represents a vector image</summary>
public class Vector : IVector
{
	public int Width  { get; set; }
	public int Height { get; set; }

	public void Dispose()
	{
		// Implementation for resource cleanup
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Free managed resources here if any
		}

		// Free unmanaged resources here if any
	}
}
