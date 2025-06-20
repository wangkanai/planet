// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Planet.Portal.Data;

namespace Wangkanai.Planet.Portal.Identity;

public static class RoleSeed
{
	internal static List<PlanetRole> Roles =>
	[
		Create("Admin", "Administrator"),
		Create("Moderator", "Moderator"),
		Create("Editor", "Editor"),
		Create("Contributor", "Contributor"),
		Create("User", "Registered User"),
	];

	private static PlanetRole Create(string name, string description = "")
		=> new()
		   {
			   Name           = name,
			   NormalizedName = name.ToUpperInvariant(),
			   Description    = description
		   };
}
