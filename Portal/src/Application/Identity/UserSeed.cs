// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Microsoft.AspNetCore.Identity;

using Wangkanai.Planet.Portal.Data;

namespace Wangkanai.Planet.Portal.Identity;

internal static class UserSeed
{
	internal static List<PlanetUser> Users =>
	[
		Create(1, "sarin@wangkanai.com", "P@ssw0rd", "Sarin", "Na Wangkanai", PlanetTheme.Dark),
		Create(2, "admin@demo.com", "P@ssw0rd", "Admin", "Demo"),
		Create(3, "moderator@demo.com", "P@ssw0rd", "Moderator", "Demo"),
		Create(4, "editor@demo.com", "P@ssw0rd", "Editor", "Demo"),
		Create(5, "contributor@demo.com", "P@ssw0rd", "Contributor", "Demo"),
		Create(6, "user@demo.com", "P@ssw0rd", "User", "Demo"),
	];

	private static PasswordHasher<PlanetUser> Hasher => new();

	private static PlanetUser Create(int id, string email, string password, string firstname, string lastname, PlanetTheme theme = PlanetTheme.System)
		=> new()
		   {
			   Id                 = id,
			   Email              = email,
			   EmailConfirmed     = true,
			   UserName           = email,
			   NormalizedUserName = email.ToUpperInvariant(),
			   NormalizedEmail    = email.ToUpperInvariant(),
			   PasswordHash       = Hasher.HashPassword(null!, password),
			   Firstname          = firstname,
			   Lastname           = lastname,
			   Theme              = theme
		   };
}
