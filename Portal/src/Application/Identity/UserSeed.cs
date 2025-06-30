// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Microsoft.AspNetCore.Identity;

using Wangkanai.Planet.Portal.Data;

namespace Wangkanai.Planet.Portal.Identity;

public static class UserSeed
{
	internal static List<PlanetUser> Users =>
	[
		Create("sarin@wangkanai.com", "P@ssw0rd", "Sarin", "Na Wangkanai"),
		Create("user@demo.com", "P@ssw0rd", "Demo", "User"),
	];

	private static PasswordHasher<PlanetUser> Hasher => new();

	private static PlanetUser Create(string email, string password, string firstname, string lastname)
		=> new()
		   {
			   Email              = email,
			   EmailConfirmed     = true,
			   UserName           = email,
			   NormalizedUserName = email.ToUpperInvariant(),
			   NormalizedEmail    = email.ToUpperInvariant(),
			   PasswordHash       = Hasher.HashPassword(null!, password),
			   Firstname          = firstname,
			   Lastname           = lastname,
		   };
}
