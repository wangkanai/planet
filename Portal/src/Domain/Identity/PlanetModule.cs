// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Planet.Portal.Data;

[Flags]
public enum PlanetModule
{
	None      = 0,                                            // 000_000
	Dashboard = 1 << 0,                                       // 000_001
	Maps      = 1 << 1,                                       // 000_010
	Tiles     = 1 << 2,                                       // 000_100
	Settings  = 1 << 3,                                       // 001_000
	Identity  = 1 << 4,                                       // 010_000
	Full      = Dashboard | Maps | Tiles | Settings | Identity// 011_111
}
