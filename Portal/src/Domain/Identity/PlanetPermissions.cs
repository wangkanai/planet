// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Planet.Portal.Data;

[Flags]
public enum PlanetPermissions
{
	None    = 0,
	View    = 1 << 0,
	Edit    = 1 << 1,
	Create  = 1 << 3,
	Delete  = 1 << 4,
	Achieve = 1 << 5,
	Manage  = View | Edit | Create | Achieve,
	Full    = Manage | Delete
}
