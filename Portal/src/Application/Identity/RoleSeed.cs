// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Planet.Portal.Data;

namespace Wangkanai.Planet.Portal.Identity;

internal static class RoleSeed
{
	internal static List<PlanetRole> Roles =>
	[
		Create(PlanetRoles.Admin),
		Create(PlanetRoles.Moderator),
		Create(PlanetRoles.Editor),
		Create(PlanetRoles.Contributor),
		Create(PlanetRoles.User),
		Create(PlanetRoles.Guest),
	];

	private static PlanetRole Create(PlanetRoles role)
		=> new()
		   {
			   Name           = role.ToString(),
			   NormalizedName = role.ToString().ToUpperInvariant(),
		   };
}
