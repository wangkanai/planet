// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

using Microsoft.AspNetCore.Identity;

namespace Wangkanai.Planet.Portal.Identity;

public static class UserRoleSeed
{
	internal static List<IdentityUserRole<string>> UserRoles =>
	[
		new()
		{
			UserId = "1",// Assuming the user ID for the seeded user is "1"
			RoleId = "1",// Assuming the role ID for "Admin" is "1"
		}
	];
}
