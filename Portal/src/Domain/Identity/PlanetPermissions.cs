// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Planet.Portal.Data;

[Flags]
public enum PlanetPermissions
{
	None    = 0,                             // 000_000
	View    = 1 << 0,                        // 000_001
	Edit    = 1 << 1,                        // 000_010
	Create  = 1 << 3,                        // 000_100
	Delete  = 1 << 4,                        // 001_000
	Achieve = 1 << 5,                        // 001_000
	Manage  = View | Edit | Create | Achieve,// 001_111
	Full    = Manage | Delete                // 011_111
}
