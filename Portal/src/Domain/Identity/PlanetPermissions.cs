// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Portal.Data;

/// <summary>Defines the permissions for planet-related operations.</summary>
[Flags]
public enum PlanetPermissions
{
	None    = 0,                             // 0000_0000
	View    = 1 << 0,                        // 0000_0001
	Edit    = 1 << 1,                        // 0000_0010
	Create  = 1 << 3,                        // 0000_0100
	Delete  = 1 << 4,                        // 0000_1000
	Achieve = 1 << 5,                        // 0000_1000
	Manage  = View | Edit | Create | Achieve,// 0000_1111
	Full    = Manage | Delete                // 0001_1111
}
