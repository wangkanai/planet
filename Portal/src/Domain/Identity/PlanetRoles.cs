// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Portal.Data;

/// <summary>Defines the different roles that a user can have in the system.</summary>
[Flags]
public enum PlanetRoles
{
	None        = 0,     // 0000_0000
	Admin       = 1 << 0,// 0000_0001
	Moderator   = 1 << 1,// 0000_0010
	Editor      = 1 << 2,// 0000_0100
	Contributor = 1 << 3,// 0000_1000
	User        = 1 << 4,// 0001_0000
}
