// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Planet.Portal.Data;

[Flags]
public enum PlanetModule
{
	None      = 0,     // 000_000
	Dashboard = 1 << 0,// 000_001
	Maps      = 1 << 1,// 000_010
	Tiles     = 1 << 2,// 0b0100
	Settings  = 1 << 3,// 0b1000
	Identity  = 1 << 4,// 0b10000
	Full      = Dashboard | Maps | Tiles | Settings | Identity
}
