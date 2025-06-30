// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Planet.Portal.Data;

namespace Wangkanai.Planet.Portal.Identity;

public static class RoleSeed
{
	internal static List<PlanetRole> Roles =>
	[
		Create("Admin"),
		Create("Moderator"),
		Create("Editor"),
		Create("Contributor"),
		Create("User"),
	];

	private static PlanetRole Create(string name)
		=> new()
		   {
			   Name           = name,
			   NormalizedName = name.ToUpperInvariant(),
		   };
}
