// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Spatial;

public interface ILocalTileSource : ITileSource
{
	Task<byte[]?> GetTileAsync(TileInfo tileInfo);
}
