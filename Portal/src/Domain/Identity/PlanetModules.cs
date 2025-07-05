// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Portal.Identity;

/// <summary>Defines the available planet modules.</summary>
[Flags]
public enum PlanetModules
{
	None      = 0,                                  // 0000_0000
	Dashboard = 1 << 0,                             // 0000_0001
	Maps      = 1 << 1,                             // 0000_0010
	Tiles     = 1 << 2,                             // 0000_0100
	Settings  = 1 << 3,                             // 0000_1000
	Identity  = 1 << 4,                             // 0001_0000
	Manage    = Dashboard | Maps | Tiles | Settings,// 0000_1111
	Full      = Manage | Identity                   // 0001_1111
}
