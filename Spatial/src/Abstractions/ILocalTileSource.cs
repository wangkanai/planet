// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Spatial;

public interface ILocalTileSource { }

public interface ITileSource
{
	string      Name        { get; }
	ITileSchema Schema      { get; }
	Attribution Attribution { get; }
}
